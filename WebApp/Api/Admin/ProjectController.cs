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
    [RoutePrefix("api/Project")]
    public class ProjectController : ApiController
    {
        private string PageUrl = "/Admin/Project";
        private string ApiName = "Project";

        private string SAPUser = "";
        private string SAPPass = "";
        private string SAPClient = "";
        private string SAPIP = "";
        private string SAPPort = "";
        private string SAPAPI = "";
        private string SAPAPI2 = "";

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

        public ProjectController()
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

                    this.SAPAPI = Convert.ToString(db.Settings.Where(x => x.vSettingID == "C45D124B-1C28-4A37-E867-E16C089JDICN").FirstOrDefault().vSettingOption); // SAP API Endpoint
                    this.SAPAPI2 = Convert.ToString(db.Settings.Where(x => x.vSettingID == "C32D124B-8128-4A37-A867-E15C089JDICN").FirstOrDefault().vSettingOption); // SAP API Endpoint
                }
                catch (Exception)
                {
                }
            }
        }

        [Route("SAPProject")]
        public async Task<IHttpActionResult> SAPProject(int ID)
        {
            using (WebAPPv0MainEntities db = new WebAPPv0MainEntities())
            {
                // attempt to download JSON data as a string
                try
                {
                    using (var w = new WebClient())
                    {

                        // default value is to retrieve all projects
                        var url = string.Concat(this.SAPIP, ":", this.SAPPort, this.SAPAPI);
                        if (ID != 0)
                        {
                            var company = db.Companies.Where(x => x.Id == ID).FirstOrDefault();
                            if (company == null)
                                return BadRequest("Company doesn't exist!");

                            // for specific company based on selection
                            url = string.Concat(this.SAPIP, ":", this.SAPPort, this.SAPAPI2, "/", company.Code, "/Projects");
                        }

                        var json_data = string.Empty;

                        w.Credentials = new NetworkCredential(this.SAPUser, this.SAPPass);
                        w.Headers.Add("Accept", "application/json");
                        w.Headers.Add("sap-client", this.SAPClient);

                    
                        // donwload api json string
                        json_data = w.DownloadString(url);

                        // if string with JSON data is not empty, deserialize it to class and return its instance 
                        var sapdata = JsonConvert.DeserializeObject<List<SAPProject>>(json_data);

                        if(ID == 0)
                        {
                            // We have to get only projects with active company
                            sapdata = (from x in sapdata
                                        join y in db.Companies on x.BUKRS equals y.Code
                                        where y.Published == true
                                        select new SAPProject { BUKRS = x.BUKRS, SWENR = x.SWENR, XWETEXT = x.XWETEXT }).ToList();
                        }

                        // We have to get only records which is not currently exist on our database
                        // intersect 2 objects and get only what we needed
                        var keys = db.Projects.Select(x => new { x.CompanyCode, x.ProjectCode });
                        sapdata.RemoveAll(x => keys.Any(k => k.CompanyCode == x.BUKRS && k.ProjectCode == x.SWENR));

                        using (var dbContextTransaction = db.Database.BeginTransaction())
                        {
                            try
                            {
                                var cId = User.Identity.GetUserId();

                                IEnumerable<Project> source = null;
                                source = sapdata.Select(a =>
                                        new Project()
                                        {
                                            CompanyCode = a.BUKRS,
                                            ProjectCode = a.SWENR,
                                            BusinessEntity = a.XWETEXT,
                                            ADRNR = String.Empty,
                                            ProjectLocation = String.Empty,
                                            Published = false,
                                            TOM = false,
                                            TSM = false,
                                            EMM = false
                                        }).ToList();


                                await db.BulkInsertAsync<Project>(source);
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

        [Route("GetProject")]
        public async Task<IHttpActionResult> GetProject([FromUri] FilterModel param)
        {
            using (WebAPPv0MainEntities db = new WebAPPv0MainEntities())
            {
                try
                {
                    var permissionCtrl = this.GetPermissionControl(param.PageUrl);

                    var company = await (from co in db.Companies
                                             where co.Published == true
                                             select new
                                             {
                                                 co.Id,
                                                 Company = co.Code + "-" + co.Description
                                             }).OrderBy(x => x.Company).ToListAsync();

                    IEnumerable<CustomProject> source = null;
                    source = await (from pj in db.Projects
                                    join co in db.Companies on pj.CompanyCode equals co.Code
                                    select new CustomProject
                                    {
                                        Id = pj.Id,
                                        CompanyCode = co.Code,
                                        CompanyName = co.Description,
                                        ProjectCode = pj.ProjectCode,
                                        BusinessEntity = pj.BusinessEntity,
                                        Published = pj.Published.ToString(),
                                        TOM = pj.TOM,
                                        TSM = pj.TSM,
                                        EMM = pj.EMM,
                                        isChecked = false,
                                    }).ToListAsync();
                    
                    // searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.CompanyCode.ToLower().Contains(param.search) || x.ProjectCode.ToLower().Contains(param.search) || x.BusinessEntity.ToLower().Contains(param.search));
                    }

                    // sorting
                    var sortby = typeof(CustomProject).GetProperty(param.sortby);
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

                    var data = new { COUNT = source.Count(), PROJLIST = sourcePaged, COMPANYLIST = company, CONTROLS = permissionCtrl };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("UpdateModule")]
        public async Task<IHttpActionResult> UpdateModule(List<CustomProject> data)
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
                       // var cd = db.Projects.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.CompanyCode, x.ProjectCode, x.BusinessEntity, x.ProjectLocation, Published = x.Published.ToString(), x.TOM, x.TSM, x.EMM }).ToList();

                        foreach (var ds in data)
                        {
                            var sql = "Update Project SET TOM = {1}, TSM = {2}, EMM = {3} WHERE Id = {0}";
                            await db.Database.ExecuteSqlCommandAsync(sql, ds.Id, ds.TOM, ds.TSM, ds.EMM);
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "UPDATE";
                        log.Description = "Enable Project according to Module";
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

        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(CustomProject data)
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
                        var cd = db.Projects.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.CompanyCode, x.ProjectCode, x.BusinessEntity, x.ProjectLocation, Published = x.Published.ToString(), x.TOM, x.TSM, x.EMM }).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "Update Project SET Published = {1} WHERE Id = {0}";
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

        [Route("SaveProject")]
        public async Task<IHttpActionResult> SaveProject(Project data)
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

                        bool isProjCodeExists = db.Projects.Where(x => x.ProjectCode == data.ProjectCode && x.CompanyCode == data.CompanyCode && x.Id != data.Id).Any();
                        if (isProjCodeExists)
                            return BadRequest("Project Code Exists");

                        if (data.Id == 0)
                        {
                            nwe = true;
                            db.Projects.Add(data);
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
                        var cd = db.Projects.Where(x => x.Id == ID).Select(x => new { x.Id, x.CompanyCode, x.ProjectCode, x.BusinessEntity, x.ProjectLocation, Published = x.Published.ToString(), x.TOM, x.TSM, x.EMM }).SingleOrDefault();

                        db.Projects.RemoveRange(db.Projects.Where(x => x.Id == ID));
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
        public async Task<IHttpActionResult> RemoveRecords(CustomProject data)
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
                        var cd = db.Projects.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.CompanyCode, x.ProjectCode, x.BusinessEntity, x.ProjectLocation, Published = x.Published.ToString(), x.TOM, x.TSM, x.EMM }).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "DELETE FROM Project WHERE Id = {0}";
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