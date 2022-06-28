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
    [RoutePrefix("api/UnitType")]
    public class UnitTypeController : ApiController
    {
        private string PageUrl = "/Admin/UnitType";
        private string ApiName = "Unit Type";

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

        public UnitTypeController()
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

                    this.SAPAPI = Convert.ToString(db.Settings.Where(x => x.vSettingID == "C58D134B-8C18-4B37-E867-A17C089JDIEN").FirstOrDefault().vSettingOption); // SAP API Endpoint
                }
                catch (Exception)
                {
                }
            }
        }

        [Route("GetSAPUnitType")]
        public async Task<IHttpActionResult> GetSAPUnitType()
        {
            using (WebAPPv0MainEntities db = new WebAPPv0MainEntities())
            {
                // attempt to download JSON data as a string
                try
                {

                    using (var w = new WebClient())
                    {
                        // default value is to retrieve all Unit Type
                        var url = string.Concat(this.SAPIP, ":", this.SAPPort, this.SAPAPI);
                        var json_data = string.Empty;

                        w.Credentials = new NetworkCredential(this.SAPUser, this.SAPPass);
                        w.Headers.Add("Accept", "application/json");
                        w.Headers.Add("sap-client", this.SAPClient);
                    
                        json_data = w.DownloadString(url);

                        // if string with JSON data is not empty, deserialize it to class and return its instance 
                        var sapdata = JsonConvert.DeserializeObject<List<SAPUnitType>>(json_data);

                        using (var dbContextTransaction = db.Database.BeginTransaction())
                        {
                            try
                            {
                                var cId = User.Identity.GetUserId();

                                IEnumerable<UnitType> source = null;
                                source = sapdata.Select(a =>
                                        new UnitType()
                                        {
                                            Code = a.HOMTYP,
                                            UnitTypeDesc = a.DESCR,
                                            UnitTypeDesc2 = a.ZZABBR,
                                            //UnitCategoryCode = ,
                                            //UnitCategoryDesc =
                                            Published = false,
                                        }).ToList();

                                await db.BulkInsertAsync<UnitType>(source);
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

                                var data = new { COMPANYLIST = source };
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

        [Route("GetUnitType")]
        public async Task<IHttpActionResult> GetUnitType([FromUri] FilterModel param)
        {
            using (WebAPPv0MainEntities db = new WebAPPv0MainEntities())
            {
                try
                {                                  
                    var permissionCtrl = this.GetPermissionControl(param.PageUrl);

                    IEnumerable<CustomUnitType> source = null;
                    source = await (from ut in db.UnitTypes
                                          select new CustomUnitType
                                          {
                                              Id = ut.Id,
                                              Code = ut.Code,
                                              UnitTypeDesc = ut.UnitTypeDesc,
                                              UnitTypeDesc2 = ut.UnitTypeDesc2,
                                              Published = ut.Published.ToString(),
                                              isChecked = false,
                                          }).ToListAsync();
                    
                    // searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.Code.ToLower().Contains(param.search) || x.UnitTypeDesc.ToLower().Contains(param.search) || x.UnitTypeDesc2.ToLower().Contains(param.search));
                    }

                    // sorting
                    var sortby = typeof(CustomUnitType).GetProperty(param.sortby);
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

                    var data = new { COUNT = source.Count(), UNITYPELIST = sourcePaged, CONTROLS = permissionCtrl };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(CustomUnitType data)
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
                        var cd = db.UnitTypes.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.UnitTypeDesc, x.UnitTypeDesc2, x.UnitCategoryCode, x.UnitCategoryDesc, Published = x.Published.ToString() }).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "Update UnitType SET Published = {1} WHERE Id = {0}";
                            await db.Database.ExecuteSqlCommandAsync(sql, ds.Id, data.Published);
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "UPDATE";
                        log.Description = (data.Published == "True") ? "Activate list of " + this.ApiName : "Deactivate list of " + this.ApiName;
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

        [Route("SaveUnitType")]
        public async Task<IHttpActionResult> SaveUnitType(UnitType data)
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

                        bool isUnitTypesExists = db.UnitTypes.Where(x => x.Code == data.Code && x.Id != data.Id).Any();
                        if (isUnitTypesExists)
                            return BadRequest("Unit Code Exists");

                        if (data.Id == 0)
                        {
                            nwe = true;
                            db.UnitTypes.Add(data);
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
                        var cd = db.UnitTypes.Where(x => x.Id == ID).Select(x => new { x.Id, x.UnitTypeDesc, x.UnitTypeDesc2, x.UnitCategoryCode, x.UnitCategoryDesc, Published = x.Published.ToString() }).SingleOrDefault();

                        db.UnitTypes.RemoveRange(db.UnitTypes.Where(x => x.Id == ID));
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
        public async Task<IHttpActionResult> RemoveRecords(CustomUnitType data)
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
                        var cd = db.UnitTypes.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.UnitTypeDesc, x.UnitTypeDesc2, x.UnitCategoryCode, x.UnitCategoryDesc, Published = x.Published.ToString() }).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "DELETE FROM UnitType WHERE Id = {0}";
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