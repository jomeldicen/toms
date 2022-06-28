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
    [RoutePrefix("api/Holiday")]
    public class HolidayController : ApiController
    {
        private string PageUrl = "/Admin/Holiday";
        private string ApiName = "Holiday"; 
        
        string timezone = "";

        private HolidayController()
        {
            this.timezone = "Taipei Standard Time";
        }

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

        [Route("GetHoliday")]
        public async Task<IHttpActionResult> GetHoliday([FromUri] FilterModel param)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var permissionCtrl = this.GetPermissionControl(param.PageUrl);

                    //multiple Filters
                    var a = JsonConvert.DeserializeObject<Dictionary<string, string>>(param.multiplesearch[0]);

                    int Year = 0;

                    if (a.ToList().Count() == 1)
                    {
                        foreach (KeyValuePair<string, string> i in a.ToList())
                        {
                            if (i.Key == "Year" && !string.IsNullOrWhiteSpace(i.Value))
                            {
                                if (i.Value != "0") Year = Convert.ToInt16(i.Value);
                            }
                        }
                    }

                    var yearLists = (from hd in db.HolidayDimensions
                                     select new { 
                                         Year = hd.TheDate.Year
                                     }).Distinct().ToList();

                    IEnumerable<CustomHoliday> source = null;
                    source = await (from hd in db.HolidayDimensions
                                    where hd.TheDate.Year == Year
                                    select new CustomHoliday
                                    {
                                        Id = hd.Id,
                                        YearCovered = hd.YearCovered,
                                        TheDate = hd.TheDate,
                                        HolidayText = hd.HolidayText,
                                        HolidayType = hd.HolidayType,
                                        isChecked = false,
                                        ModifiedByPK = hd.ModifiedByPK,
                                        ModifiedDate = hd.ModifiedDate,
                                        CreatedByPK = hd.CreatedByPK,
                                        CreatedDate = hd.CreatedDate
                                    }).ToListAsync();

                    // searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.HolidayText.ToLower().Contains(param.search) || x.HolidayType.ToLower().Contains(param.search));
                    }

                    // sorting
                    var sortby = typeof(CustomHoliday).GetProperty(param.sortby);
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

                    var data = new { COUNT = source.Count(), HolidayLIST = sourcePaged, YearLIST = yearLists, CONTROLS = permissionCtrl };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("SaveHoliday")]
        public async Task<IHttpActionResult> SaveHoliday(HolidayDimension data)
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

                        bool isHolidaynExists = db.HolidayDimensions.Where(x => x.TheDate == data.TheDate && x.Id != data.Id).Any();
                        if (isHolidaynExists)
                            return BadRequest("Exists");

                        data.ModifiedByPK = cId;
                        data.ModifiedDate = DateTime.Now;
                        data.TheDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TheDate, timezone);

                        if (data.Id == 0)
                        {
                            nwe = true;
                            data.CreatedByPK = cId;
                            data.CreatedDate = DateTime.Now;

                            db.HolidayDimensions.Add(data);
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
                        var cd = db.HolidayDimensions.Where(x => x.Id == ID).Select(x => new { x.Id, x.TheDate, x.YearCovered, x.HolidayText, x.HolidayType }).SingleOrDefault();

                        db.HolidayDimensions.RemoveRange(db.HolidayDimensions.Where(x => x.Id == ID));
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
        public async Task<IHttpActionResult> RemoveRecords(CustomHoliday data)
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
                        var cd = db.HolidayDimensions.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.TheDate, x.YearCovered, x.HolidayText, x.HolidayType }).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "DELETE FROM HolidayDimension WHERE Id = {0}";
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