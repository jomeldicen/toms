using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using WebApp.Models;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using System.Collections;
using System.Collections.Generic;
using WebApp.Helper;
using Newtonsoft.Json;
using System.Net;
using EntityFramework.BulkInsert.Extensions;

namespace WebApp.Api.Admin
{
    [Authorize]
    [RoutePrefix("api/UnitInventory")]
    public class UnitInventoryController : ApiController
    {
        private string PageUrl = "/Admin/UnitInventory";
        private string ApiName = "Unit Inventory";

        private string SAPUser = "";
        private string SAPPass = "";
        private string SAPClient = "";
        private string SAPIP = "";
        private string SAPPort = "";
        private string SAPAPI = "";

        private CustomControl GetPermissionControl(string PageUrl)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                this.PageUrl = PageUrl;
                var cId = User.Identity.GetUserId();
                var roleId = db.AspNetUserRoles.Where(x => x.UserId == cId).FirstOrDefault().RoleId;

                return db.Database.SqlQuery<CustomControl>("EXEC spPermissionControls {0}, {1}", roleId, PageUrl).SingleOrDefault();
            }
        }

        public UnitInventoryController()
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    this.SAPUser = Convert.ToString(db.Settings.Where(x => x.vSettingID == "B16D224B-1C28-4A37-B767-B15C089JOMEL").FirstOrDefault().vSettingOption); // SAP User
                    this.SAPPass = Convert.ToString(db.Settings.Where(x => x.vSettingID == "B23D124B-0D28-4A37-A867-D15C089DICEN").FirstOrDefault().vSettingOption); // SAP Pass
                    this.SAPClient = Convert.ToString(db.Settings.Where(x => x.vSettingID == "B33D124B-0D28-4A37-A867-D15C08900001").FirstOrDefault().vSettingOption); // SAP Client

                    this.SAPIP = Convert.ToString(db.Settings.Where(x => x.vSettingID == "B34D124B-0D28-4A37-A867-D15C08900002").FirstOrDefault().vSettingOption); // SAP IP
                    this.SAPPort = Convert.ToString(db.Settings.Where(x => x.vSettingID == "B35D124B-0D28-4A37-A867-D15C08900003").FirstOrDefault().vSettingOption); // SAP Port

                    this.SAPAPI = Convert.ToString(db.Settings.Where(x => x.vSettingID == "C68D134B-8C18-4B37-E867-A17C089JDIEN").FirstOrDefault().vSettingOption); // SAP API Endpoint
                }
                catch (Exception)
                {
                }
            }
        }

        [Route("SAPUnitInventory")]
        public IHttpActionResult SAPUnitInventory(int ID)
        {
            if (ID == 0)
            {
                return BadRequest("Please select projects to sync!");
            }

            using (WebAPPv0MainEntities db = new WebAPPv0MainEntities())
            {
                // attempt to download JSON data as a string
                try
                {
                    using (var w = new WebClient())
                    {                    
                        var project = db.Projects.Where(x => x.Id == ID).FirstOrDefault();
                        if (project == null)
                            return BadRequest("Projects doesn't exist!");

                        // for specific company based on selection
                        this.SAPAPI = this.SAPAPI.Replace("{Code1}", project.CompanyCode).Replace("{Code2}", project.ProjectCode);
                        var url = string.Concat(this.SAPIP, ":", this.SAPPort, this.SAPAPI);
                        var json_data = string.Empty;

                        w.Credentials = new NetworkCredential(this.SAPUser, this.SAPPass);
                        w.Headers.Add("Accept", "application/json");
                        w.Headers.Add("sap-client", this.SAPClient);
                    
                        // donwload api json string
                        json_data = w.DownloadString(url);

                        // if string with JSON data is not empty, deserialize it to class and return its instance 
                        var sapdata = JsonConvert.DeserializeObject<List<SAPUnitInventory>>(json_data);

                        if (ID == 0)
                        {
                            // We have to get only projects with active company
                            sapdata = (from x in sapdata
                                        join y in db.Projects on new { p1 = x.BUKRS, p2 = x.SWENR } equals new { p1 = y.CompanyCode, p2 = y.ProjectCode }
                                        select new SAPUnitInventory { 
                                            BUKRS = x.BUKRS,
                                            SWENR = x.SWENR,
                                            SMENR = x.SMENR,
                                            UNIT_TYPE = x.UNIT_TYPE,
                                            TURNOVER_DATE = x.TURNOVER_DATE,
                                            PHASE = x.PHASE
                                        }).ToList();
                        }

                        // We have to get only records which is not currently exist on our database
                        // intersect 2 objects and get only what we needed
                        var keys = db.UnitInventories.Select(x => new { x.CompanyCode, x.ProjectCode, x.UnitNos });
                        sapdata.RemoveAll(x => keys.Any(k => k.CompanyCode == x.BUKRS && k.ProjectCode == x.SWENR && k.UnitNos == x.SMENR));

                        using (var dbContextTransaction = db.Database.BeginTransaction())
                        {
                            try
                            {
                                var cId = User.Identity.GetUserId();

                                IEnumerable<UnitInventory> source = null;
                                source = sapdata.Select(a =>
                                        new UnitInventory()
                                        {
                                            CompanyCode = a.BUKRS,
                                            ProjectCode = a.SWENR,
                                            UnitNos = a.SMENR,
                                            UnitType = a.UNIT_TYPE.HOMTYP,
                                            UnitTypeDesc = a.UNIT_TYPE.DESCR,
                                            //UnitTypeCode = ,
                                            UnitArea = a.TURNOVER_DATE.UNIT_AREA,
                                            //UnitMsrmnt = ,
                                            UnitCategoryCode = a.UNIT_TYPE.ZZSALES_UNIT_TYPE.UNIT_TYPE_CODE,
                                            UnitCategoryDesc = a.UNIT_TYPE.ZZSALES_UNIT_TYPE.DESCRIPTION,
                                            //RefNos = ,
                                            Phase = a.PHASE,
                                            //SNKS = ,
                                            //Intreno = ,
                                            //Usr08 = ,
                                            //Usr09 = ,
                                            ZoneNos = "For Discussion",
                                            ZoneType = "For Discussion",
                                        }).ToList();

                                db.BulkInsertAsync<UnitInventory>(source);
                                dbContextTransaction.Commit();

                                // ---------------- Start Transaction Activity Logs ------------------ //
                                AuditTrail log = new AuditTrail();
                                log.EventType = "FETCH";
                                log.Description = "Fetch Data from SAP - " + this.ApiName;
                                log.PageUrl = this.PageUrl;
                                log.ObjectType = this.GetType().Name;
                                log.EventName = this.ApiName;
                                log.ContentDetail = JsonConvert.SerializeObject(source);
                                log.SaveTransactionLogs();
                                // ---------------- End Transaction Activity Logs -------------------- //

                                var data = new { RecordCount = source.Count() };
                                return Ok(data);
                            }
                            catch (Exception ex)
                            {
                                dbContextTransaction.Rollback();
                                return BadRequest(ex.Message);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("GetUnitInventory")]
        public async Task<IHttpActionResult> GetUnitInventory([FromUri] FilterModel param)
        {
            using (WebAPPv0MainEntities db = new WebAPPv0MainEntities())
            {
                try
                {
                    var permissionCtrl = this.GetPermissionControl(param.PageUrl);

                    var projects = await (from pr in db.Projects
                                          where pr.Published == true
                                          select new
                                          {
                                              pr.Id,
                                              pr.BusinessEntity
                                          }).OrderBy(x => x.BusinessEntity).ToListAsync();

                    IEnumerable<CustomUnitInventory> source = null;
                    source = await (from ui in db.UnitInventories
                                    join pj in db.Projects on ui.ProjectCode equals pj.ProjectCode
                                    select new CustomUnitInventory
                                    {
                                        Id = ui.Id,
                                        CompanyCode = ui.CompanyCode,
                                        ProjectCode = ui.ProjectCode,
                                        UnitNos = ui.UnitNos,
                                        UnitType = ui.UnitType,
                                        UnitTypeDesc = ui.UnitTypeDesc,
                                        UnitTypeCode = ui.UnitTypeCode,
                                        UnitArea = ui.UnitArea,
                                        UnitMsrmnt = ui.UnitMsrmnt,
                                        UnitCategoryCode = ui.UnitCategoryCode,
                                        UnitCategoryDesc = ui.UnitCategoryDesc,
                                        RefNos = ui.RefNos,
                                        Phase = ui.Phase,
                                        SNKS = ui.SNKS,
                                        Intreno = ui.Intreno,
                                        isChecked = false
                                    }).ToListAsync();

                    // searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.ProjectCode.ToLower().Contains(param.search) || x.UnitNos.ToLower().Contains(param.search) || x.UnitType.ToLower().Contains(param.search));
                    }

                    // sorting
                    var sortby = typeof(CustomUnitInventory).GetProperty(param.sortby);
                    switch (param.reverse)
                    {
                        case true:
                            source = source.OrderByDescending(s => sortby.GetValue(s, null));
                            break;
                        case false:
                            source = source.OrderBy(s => sortby.GetValue(s, null));
                            break;
                    }

                    // paging
                    var sourcePaged = source.Skip((param.page - 1) * param.itemsPerPage).Take(param.itemsPerPage);

                    var data = new { COUNT = source.Count(), INVENTORYLIST = sourcePaged, PROJECTLIST = projects, CONTROLS = permissionCtrl };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(CustomUnitInventory data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (WebAPPv0MainEntities db = new WebAPPv0MainEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var ids = data.dsList.Select(o => o.Id).ToArray();
                        var cd = db.UnitInventories.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.CompanyCode, x.ProjectCode, x.UnitNos, x.UnitType, x.UnitTypeDesc, x.UnitTypeCode, x.UnitArea, x.UnitMsrmnt, x.UnitCategoryCode, x.UnitCategoryDesc, x.RefNos, x.Phase, x.SNKS, x.Intreno, x.Usr09  }).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "Update UnitInventory SET Published = {1}, ModifiedByPK = {2}, ModifiedDate = {3} WHERE Id = {0}";
                            await db.Database.ExecuteSqlCommandAsync(sql, ds.Id, data.Published, User.Identity.GetUserId(), DateTime.Now);
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "UPDATE";
                        log.Description = (data.Published == "True") ? "Activate list of LOV " + this.ApiName : "Deactivate list of LOV " + this.ApiName;
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.EventName = this.ApiName;
                        log.ContentDetail = JsonConvert.SerializeObject(cd);
                        log.SaveTransactionLogs();
                        // ---------------- End Transaction Activity Logs -------------------- //

                        return Ok();
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return BadRequest(ex.Message);
                    }
                }
            }
        }

        [Route("SaveUnitInventory")]
        public async Task<IHttpActionResult> SaveUnitInventory(UnitInventory data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (WebAPPv0MainEntities db = new WebAPPv0MainEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        bool nwe = false;
                        var cId = User.Identity.GetUserId();

                        bool isInventoryExists = db.UnitInventories.Where(x => x.CompanyCode == data.CompanyCode && x.ProjectCode == data.ProjectCode && x.UnitNos == data.UnitNos && x.Id != data.Id).Any();
                        if (isInventoryExists)
                            return BadRequest("Unit Inventory Exists");

                        if (data.Id == 0)
                        {
                            nwe = true;
                            db.UnitInventories.Add(data);
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            db.Entry(data).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = (nwe) ? "CREATE" : "UPDATE";
                        log.Description = (nwe) ? "Create " + this.ApiName : "Update " + this.ApiName;
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.EventName = this.ApiName;
                        log.ContentDetail = JsonConvert.SerializeObject(data);
                        log.SaveTransactionLogs();
                        // ---------------- End Transaction Activity Logs -------------------- //

                        return Ok();
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return BadRequest(ex.Message);
                    }
                }
            }
        }

        [Route("RemoveData")]
        public IHttpActionResult RemoveData(int ID)
        {
            using (WebAPPv0MainEntities db = new WebAPPv0MainEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var cd = db.UnitInventories.Where(x => x.Id == ID).Select(x => new { x.Id, x.CompanyCode, x.ProjectCode, x.UnitNos, x.UnitType, x.UnitTypeDesc, x.UnitTypeCode, x.UnitArea, x.UnitMsrmnt, x.UnitCategoryCode, x.UnitCategoryDesc, x.RefNos, x.Phase, x.SNKS, x.Intreno, x.Usr09 }).SingleOrDefault();

                        db.UnitInventories.RemoveRange(db.UnitInventories.Where(x => x.Id == ID));
                        db.SaveChanges();

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "DELETE";
                        log.Description = "Delete single " + this.ApiName;
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.EventName = this.ApiName;
                        log.ContentDetail = JsonConvert.SerializeObject(cd);
                        log.SaveTransactionLogs();
                        // ---------------- End Transaction Activity Logs -------------------- //

                        return Ok();
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return BadRequest(ex.Message);
                    }
                }
            }
        }

        [Route("RemoveRecords")]
        public async Task<IHttpActionResult> RemoveRecords(CustomUnitInventory data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (WebAPPv0MainEntities db = new WebAPPv0MainEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var ids = data.dsList.Select(o => o.Id).ToArray();
                        var cd = db.UnitInventories.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.CompanyCode, x.ProjectCode, x.UnitNos, x.UnitType, x.UnitTypeDesc, x.UnitTypeCode, x.UnitArea, x.UnitMsrmnt, x.UnitCategoryCode, x.UnitCategoryDesc, x.RefNos, x.Phase, x.SNKS, x.Intreno, x.Usr09 }).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "DELETE FROM UnitInventory WHERE Id = {0}";
                            await db.Database.ExecuteSqlCommandAsync(sql, ds.Id);
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "DELETE";
                        log.Description = "Delete list of " + this.ApiName;
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.EventName = this.ApiName;
                        log.ContentDetail = JsonConvert.SerializeObject(cd);
                        log.SaveTransactionLogs();
                        // ---------------- End Transaction Activity Logs -------------------- //

                        return Ok();
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return BadRequest(ex.Message);
                    }
                }
            }
        }

    }
}