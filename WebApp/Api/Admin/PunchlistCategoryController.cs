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
    [RoutePrefix("api/PunchlistCategory")]
    public class PunchlistCategoryController : ApiController
    {
        private string PageUrl = "/Admin/PunchlistCategory";
        private string ApiName = "Punchlist Category";

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

        [Route("GetPunchlistCategory")]
        public async Task<IHttpActionResult> GetPunchlistCategory([FromUri] FilterModel param)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var permissionCtrl = this.GetPermissionControl(param.PageUrl);

                    IEnumerable<CustomPunchlistCategory> source = null;
                    source = await (from pc in db.PunchlistCategories
                                    select new CustomPunchlistCategory
                                    {
                                        Id = pc.Id,
                                        Code = pc.Code,
                                        Name = pc.Name,
                                        TurnaroundTime = pc.TurnaroundTime,
                                        Published = pc.Published.ToString(),
                                        Applicability = pc.Applicability.ToString(),
                                        CalendarType = pc.CalendarType,
                                        isChecked = false,
                                        ModifiedByPK = pc.ModifiedByPK,
                                        ModifiedDate = pc.ModifiedDate,
                                        CreatedByPK = pc.CreatedByPK,
                                        CreatedDate = pc.CreatedDate
                                    }).ToListAsync();

                    // searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.Name.ToLower().Contains(param.search) || x.Code.ToLower().Contains(param.search));
                    }

                    // sorting
                    var sortby = typeof(CustomPunchlistCategory).GetProperty(param.sortby);
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

                    var data = new { COUNT = source.Count(), PunchlistCategoryLIST = sourcePaged, CONTROLS = permissionCtrl };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(CustomPunchlistCategory data)
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
                        var cd = db.PunchlistCategories.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.Name, x.TurnaroundTime, Published = x.Published.ToString() }).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "Update PunchlistCategory SET Published = {1}, ModifiedByPK = {2}, ModifiedDate = {3} WHERE Id = {0}";
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

        [Route("SavePunchlistCategory")]
        public async Task<IHttpActionResult> SavePunchlistCategory(PunchlistCategory data)
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

                        bool isPunchlistCategorynExists = db.PunchlistCategories.Where(x => x.Name == data.Name && x.Id != data.Id).Any();
                        if (isPunchlistCategorynExists)
                            return BadRequest("Exists");

                        data.ModifiedByPK = cId;
                        data.ModifiedDate = DateTime.Now;
                        if (data.Id == 0)
                        {
                            nwe = true;
                            data.Applicability = "01";
                            data.CreatedByPK = cId;
                            data.CreatedDate = DateTime.Now;

                            db.PunchlistCategories.Add(data);
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
                        log.Description = (nwe) ? "Create LOV " + this.ApiName : "Update LOV " + this.ApiName;
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
            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var cd = db.PunchlistCategories.Where(x => x.Id == ID).Select(x => new { x.Id, x.Name, x.TurnaroundTime, Published = x.Published.ToString() }).SingleOrDefault();

                        db.PunchlistCategories.RemoveRange(db.PunchlistCategories.Where(x => x.Id == ID));
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
        public async Task<IHttpActionResult> RemoveRecords(CustomPunchlistCategory data)
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
                        var cd = db.PunchlistCategories.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.Name, x.TurnaroundTime, Published = x.Published.ToString() }).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "DELETE FROM PunchlistCategory WHERE Id = {0}";
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