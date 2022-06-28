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
    [RoutePrefix("api/Phase")]
    public class PhaseController : ApiController
    {
        private string PageUrl = "/Admin/Phase";
        private string ApiName = "Phase";

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

        public PhaseController()
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

                    this.SAPAPI = Convert.ToString(db.Settings.Where(x => x.vSettingID == "C79D134B-8C18-4B37-F868-A18C089JDIEN").FirstOrDefault().vSettingOption); // SAP API Endpoint
                }
                catch (Exception)
                {
                }
            }
        }

        [Route("SAPPhase")]
        public IHttpActionResult SAPPhase(int ID)
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
                        this.SAPAPI.Replace("{Code1}", project.CompanyCode).Replace("{Code2}", project.ProjectCode);
                        var url = string.Concat(this.SAPIP, ":", this.SAPPort, this.SAPAPI);
                        var json_data = string.Empty;

                        w.Credentials = new NetworkCredential(this.SAPUser, this.SAPPass);
                        w.Headers.Add("Accept", "application/json");
                        w.Headers.Add("sap-client", this.SAPClient);
                    
                        // donwload api json string
                        json_data = w.DownloadString(url);

                        // if string with JSON data is not empty, deserialize it to class and return its instance 
                        var sapdata = JsonConvert.DeserializeObject<List<SAPPhase>>(json_data);

                        if (ID == 0)
                        {
                            // We have to get only projects with active company
                            sapdata = (from x in sapdata
                                       join y in db.Projects on new { p1 = x.BUKRS, p2 = x.SWENR } equals new { p1 = y.CompanyCode, p2 = y.ProjectCode }
                                       select new SAPPhase
                                       {
                                           BUKRS = x.BUKRS,
                                           SWENR = x.SWENR,
                                           PHASE = x.PHASE
                                       }).ToList();
                        }

                        // We have to get only records which is not currently exist on our database
                        // intersect 2 objects and get only what we needed
                        var keys = db.Phases.Select(x => new { x.CompanyCode, x.ProjectCode, x.Phase1 });
                        sapdata.RemoveAll(x => keys.Any(k => k.CompanyCode == x.BUKRS && k.ProjectCode == x.SWENR && k.Phase1 == x.PHASE));

                        using (var dbContextTransaction = db.Database.BeginTransaction())
                        {
                            try
                            {
                                var cId = User.Identity.GetUserId();

                                IEnumerable<Phase> source = null;
                                source = sapdata.Select(a =>
                                        new Phase()
                                        {
                                            CompanyCode = a.BUKRS,
                                            ProjectCode = a.SWENR,
                                            Phase1 = a.PHASE,
                                            Published = true
                                        }).ToList();

                                db.BulkInsertAsync<Phase>(source);
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

        [Route("GetPhase")]
        public async Task<IHttpActionResult> GetPhase([FromUri] FilterModel param)
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

                    IEnumerable<CustomPhase> source = null;
                    source = await (from ui in db.Phases
                                    join pj in db.Projects on ui.ProjectCode equals pj.ProjectCode 
                                    select new CustomPhase
                                    {
                                        Id = ui.Id,
                                        CompanyCode = ui.CompanyCode,
                                        ProjectCode = pj.ProjectCode,
                                        Phase = ui.Phase1,
                                        Published = ui.Published.ToString(),
                                        isChecked = false
                                    }).ToListAsync();

                    // searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.ProjectCode.ToLower().Contains(param.search) || x.Phase.ToLower().Contains(param.search) || x.CompanyCode.ToLower().Contains(param.search));
                    }

                    // sorting
                    var sortby = typeof(CustomPhase).GetProperty(param.sortby);
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

                    var data = new { COUNT = source.Count(), PHASELIST = sourcePaged, PROJECTLIST = projects, CONTROLS = permissionCtrl };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(CustomPhase data)
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
                        var cd = db.Phases.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.CompanyCode, x.ProjectCode, x.Phase1, Published = x.Published.ToString() }).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "Update Phase SET Published = {1} WHERE Id = {0}";
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

        [Route("SavePhase")]
        public async Task<IHttpActionResult> SavePhase(Phase data)
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

                        bool isInventoryExists = db.Phases.Where(x => x.CompanyCode == data.ProjectCode && x.CompanyCode == data.ProjectCode && x.Phase1 == data.Phase1 && x.Id != data.Id).Any();
                        if (isInventoryExists)
                            return BadRequest("Phase Exists");

                        if (data.Id == 0)
                        {
                            nwe = true;
                            db.Phases.Add(data);
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
                        var cd = db.Phases.Where(x => x.Id == ID).Select(x => new { x.Id, x.CompanyCode, x.ProjectCode, x.Phase1, Published = x.Published.ToString() }).SingleOrDefault();

                        db.Phases.RemoveRange(db.Phases.Where(x => x.Id == ID));
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
        public async Task<IHttpActionResult> RemoveRecords(CustomPhase data)
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
                        var cd = db.Phases.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.CompanyCode, x.ProjectCode, x.Phase1, Published = x.Published.ToString() }).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "DELETE FROM Phase WHERE Id = {0}";
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