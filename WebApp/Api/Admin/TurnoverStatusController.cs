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

namespace WebApp.Api.Admin
{
    [Authorize]
    [RoutePrefix("api/TurnoverStatus")]
    public class TurnoverStatusController : ApiController
    {
        private string PageUrl = "/Admin/TurnoverStatus";
        private string ApiName = "Turnover Status";

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

        [Route("GetTurnoverStatus")]
        public async Task<IHttpActionResult> GetTurnoverStatus([FromUri] FilterModel param)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var permissionCtrl = this.GetPermissionControl(param.PageUrl);

                    var options = await (from op in db.Options
                                         where op.Published == true && op.OptionGroup == "Account Type"
                                         select new
                                         {
                                             id = op.Id,
                                             label = op.Description
                                         }).OrderBy(x => x.id).ToListAsync();

                    IEnumerable<CustomTurnoverStatus> source = null;
                    source = await (from ts in db.TurnoverStatus
                                    select new CustomTurnoverStatus
                                    {
                                        Id = ts.Id,
                                        Code = ts.Code,
                                        Name = ts.Name,
                                        Description = ts.Description,
                                        Published = ts.Published.ToString(),
                                        Applicability = ts.Applicability.ToString(),
                                        OptionIDs = (from w in db.TurnoverStatusAcctTypes where w.StatusID == ts.Id select new IdList { id = w.OptionID }).Distinct().ToList(),
                                        isChecked = false,
                                        ModifiedByPK = ts.ModifiedByPK,
                                        ModifiedDate = ts.ModifiedDate,
                                        CreatedByPK = ts.CreatedByPK,
                                        CreatedDate = ts.CreatedDate
                                    }).ToListAsync();

                    // searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.Name.ToLower().Contains(param.search) || x.Code.ToLower().Contains(param.search));
                    }

                    // sorting
                    var sortby = typeof(CustomTurnoverStatus).GetProperty(param.sortby);
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

                    var data = new { COUNT = source.Count(), TurnoverStatusLIST = sourcePaged, OPTIONS = options, CONTROLS = permissionCtrl };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(CustomTurnoverStatus data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var ids = data.dsList.Select(o => o.Id).ToArray();
                        var cd = db.TurnoverStatus.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.Name, Published = x.Published.ToString() }).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "Update TurnoverStatus SET Published = {1}, ModifiedByPK = {2}, ModifiedDate = {3} WHERE Id = {0}";
                            await db.Database.ExecuteSqlCommandAsync(sql, ds.Id, data.Published, User.Identity.GetUserId(), DateTime.Now);
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "UPDATE";
                        log.Description = (data.Published == "True")? "Activate list of LOV " + this.ApiName : "Deactivate list of LOV " + this.ApiName; 
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

        [Route("SaveTurnoverStatus")]
        public async Task<IHttpActionResult> SaveTurnoverStatus(CustomTurnoverStatus data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        bool nwe = false;
                        var cId = User.Identity.GetUserId();

                        bool isTurnoverStatusnExists = db.TurnoverStatus.Where(x => x.Id != data.Id && x.Name == data.Name).Any();
                        if (isTurnoverStatusnExists)
                            return BadRequest("Exists");

                        // Remove and Reset Selected Option Account Types prior to saving
                        db.TurnoverStatusAcctTypes.RemoveRange(db.TurnoverStatusAcctTypes.Where(x => x.StatusID == data.Id));
                        await db.SaveChangesAsync();

                        TurnoverStatu ts = new TurnoverStatu();

                        ts.Id = data.Id;
                        ts.Name = data.Name;
                        ts.Description = string.IsNullOrEmpty(data.Description)? "" : data.Description;
                        ts.Published = (data.Published == "True") ? true : false;
                        ts.ModifiedByPK = cId;
                        ts.ModifiedDate = DateTime.Now;
                        if (ts.Id == 0)
                        {
                            nwe = true;
                            ts.Applicability = "01";
                            ts.CreatedByPK = cId;
                            ts.CreatedDate = DateTime.Now;

                            db.TurnoverStatus.Add(ts);
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            ts.Applicability = data.Applicability;
                            ts.CreatedByPK = data.CreatedByPK;
                            ts.CreatedDate = data.CreatedDate;
                            db.Entry(ts).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }

                        // loop selected Option(Account Types) controls
                        foreach (var op in data.OptionIDs)
                        {
                            TurnoverStatusAcctType at = new TurnoverStatusAcctType();
                            at.Id = Guid.NewGuid().ToString();
                            at.StatusID = ts.Id;
                            at.OptionID = op.id;
                            db.TurnoverStatusAcctTypes.Add(at);
                            await db.SaveChangesAsync();
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = (nwe) ? "CREATE" : "UPDATE";
                        log.Description = (nwe) ? "Create LOV " + this.ApiName : "Update LOV " + this.ApiName;
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.EventName = this.ApiName;
                        log.ContentDetail = JsonConvert.SerializeObject(ts);
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
            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var cd = db.TurnoverStatus.Where(x => x.Id == ID).Select(x => new { x.Id, x.Name, x.Description, Published = x.Published.ToString()}).SingleOrDefault();

                        db.TurnoverStatus.RemoveRange(db.TurnoverStatus.Where(x => x.Id == ID));
                        db.SaveChanges();

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "DELETE";
                        log.Description = "Delete single LOV " + this.ApiName;
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
        public async Task<IHttpActionResult> RemoveRecords(CustomTurnoverStatus data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var ids = data.dsList.Select(o => o.Id).ToArray();
                        var cd = db.TurnoverStatus.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.Name, x.Description, Published = x.Published.ToString()}).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "DELETE FROM TurnoverStatus WHERE Id = {0}";
                            await db.Database.ExecuteSqlCommandAsync(sql, ds.Id);
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "DELETE";
                        log.Description = "Delete list LOV " + this.ApiName; 
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