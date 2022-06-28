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
    [RoutePrefix("api/Company")]
    public class CompanyController : ApiController
    {
        private string PageUrl = "/Admin/Company";
        private string ApiName = "Company";

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

        public CompanyController()
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

                    this.SAPAPI = Convert.ToString(db.Settings.Where(x => x.vSettingID == "C32D124B-8128-4A37-A867-E15C089JDICN").FirstOrDefault().vSettingOption); // SAP API Endpoint
                }
                catch (Exception)
                {
                }
            }
        }

        [Route("SAPCompany")]
        public async Task<IHttpActionResult> SAPCompany()
        {
            using (WebAPPv0MainEntities db = new WebAPPv0MainEntities())
            {
                // attempt to download JSON data as a string
                try
                {
                    using (var w = new WebClient())
                    {
                        // default value is to retrieve all companies
                        var url = string.Concat(this.SAPIP, ":", this.SAPPort, this.SAPAPI);
                        var json_data = string.Empty;

                        w.Credentials = new NetworkCredential(this.SAPUser, this.SAPPass);
                        w.Headers.Add("Accept", "application/json");
                        w.Headers.Add("sap-client", this.SAPClient);

                        // donwload api json string
                        json_data = w.DownloadString(url);

                        // if string with JSON data is not empty, deserialize it to class and return its instance 
                        var sapdata = JsonConvert.DeserializeObject<List<SAPCompany>>(json_data);

                        // We have to get only records which is not currently exist on our database
                        // intersect 2 objects and get only what we needed
                        var keys = db.Companies.Select(x => new { x.Code });
                        sapdata.RemoveAll(x => keys.Any(k => k.Code == x.BUKRS));

                        using (var dbContextTransaction = db.Database.BeginTransaction())
                        {
                            try 
                            {
                                var cId = User.Identity.GetUserId();

                                IEnumerable<Company> source = null;
                                source = sapdata.Select(a => 
                                        new Company() 
                                        { 
                                            Code = a.BUKRS, 
                                            Address = a.ADDRESS, 
                                            Description = a.BUTXT,
                                            Published = false
                                        }).ToList();

                                await db.BulkInsertAsync<Company>(source);
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

        [Route("GetCompany")]
        public async Task<IHttpActionResult> GetCompany([FromUri] FilterModel param)
        {
            using (WebAPPv0MainEntities db = new WebAPPv0MainEntities())
            {
                try
                {                                  
                    var permissionCtrl = this.GetPermissionControl(param.PageUrl);

                    IEnumerable<CustomCompany> source = null;
                    source = await (from co in db.Companies
                                          select new CustomCompany
                                          {
                                              Id = co.Id,
                                              Code = co.Code,
                                              Description = co.Description,
                                              Address = co.Address,
                                              Published = co.Published.ToString(),
                                              isChecked = false
                                          }).ToListAsync();
                    
                    // searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.Code.ToLower().Contains(param.search) || x.Description.ToLower().Contains(param.search) || x.Address.ToLower().Contains(param.search));
                    }

                    // sorting
                    var sortby = typeof(CustomCompany).GetProperty(param.sortby);
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

                    var data = new { COUNT = source.Count(), COMPANYLIST = sourcePaged, CONTROLS = permissionCtrl };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(CustomCompany data)
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
                        var cd = db.Companies.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.Code, x.Description, x.Address, Published = x.Published.ToString() }).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "Update Company SET Published = {1} WHERE Id = {0}";
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

        [Route("SaveCompany")]
        public async Task<IHttpActionResult> SaveCompany(Company data)
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

                        bool isCompanyExists = db.Companies.Where(x => x.Code == data.Code && x.Id != data.Id).Any();
                        if (isCompanyExists)
                            return BadRequest("Company Exists");

                        if (data.Id == 0)
                        {
                            nwe = true;
                            db.Companies.Add(data);
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
                        var cd = db.Companies.Where(x => x.Id == ID).Select(x => new { x.Id, x.Code, x.Description, x.Address, Published = x.Published.ToString() }).SingleOrDefault();

                        db.Companies.RemoveRange(db.Companies.Where(x => x.Id == ID));
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
        public async Task<IHttpActionResult> RemoveRecords(CustomCompany data)
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
                        var cd = db.Companies.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.Code, x.Description, x.Address, Published = x.Published.ToString() }).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "DELETE FROM Company WHERE Id = {0}";
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