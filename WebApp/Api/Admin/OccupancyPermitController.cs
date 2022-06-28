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
    [RoutePrefix("api/OccupancyPermit")]
    public class OccupancyPermitController : ApiController
    {
        private string PageUrl = "/Admin/OccupancyPermit";
        private string ApiName = "Occupancy Permit";

        string timezone = "";

        private OccupancyPermitController()
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

        [Route("GetOccupancyPermit")]
        public async Task<IHttpActionResult> GetOccupancyPermit([FromUri] FilterModel param)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var permissionCtrl = this.GetPermissionControl(param.PageUrl);

                    //multiple Filters
                    var a = JsonConvert.DeserializeObject<Dictionary<string, string>>(param.multiplesearch[0]);

                    string ProjectCode = "0";
                    string CompanyCode = "0";

                    if (a.ToList().Count() == 4)
                    {
                        foreach (KeyValuePair<string, string> i in a.ToList())
                        {
                            if (i.Key == "ProjectID" && !string.IsNullOrWhiteSpace(i.Value))
                            {
                                if (i.Value != "0")
                                {
                                    int ProjectID = Convert.ToInt16(i.Value);
                                    var prj = db.VW_Projects.Where(x => x.Id == ProjectID).SingleOrDefault();
                                    if (prj != null)
                                    {
                                        ProjectCode = prj.ProjectCode;
                                        CompanyCode = prj.CompanyCode;
                                    }
                                }
                            }
                        }
                    }

                    // Get List of Active Projects
                    var projects = await db.VW_UnitInventory.Where(x => x.TOM == true).Select(x => new { Id = x.ProjectId, x.CompanyCode, x.ProjectCode, x.BusinessEntity, x.ProjectLocation, ProjectCodeName = x.ProjectCode + " : " + x.BusinessEntity }).Distinct().OrderBy(x => x.BusinessEntity).ToListAsync();

                    IEnumerable<CustomOccupancyPermit> source = null;
                    source = await (from ph in db.VW_OccupancyPermit
                                    where ph.CompanyCode == CompanyCode && ph.ProjectCode == ProjectCode 
                                    select new CustomOccupancyPermit
                                    {
                                        Phase = ph.Phase,
                                        Personnel = ph.Personnel,
                                        Remarks = ph.Remarks,
                                        CompanyCode = ph.CompanyCode,
                                        ProjectCode = ph.ProjectCode,
                                        ProjectName = ph.BusinessEntity,
                                        Available = (ph.Available == 1) ? "True" : "False",
                                        PostedDateStr = ph.PostedDate.ToString(),
                                        isChecked = (ph.Available == 1)? true : false
                                    }).OrderBy(x => x.Phase).ToListAsync();

                    // searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.ProjectCode.ToLower().Contains(param.search) || x.Phase.ToLower().Contains(param.search) || x.ProjectName.ToLower().Contains(param.search));
                    }

                    // sorting
                    var sortby = typeof(CustomOccupancyPermit).GetProperty(param.sortby);
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

                    var data = new { COUNT = source.Count(), FLOORLIST = sourcePaged, PROJECTLIST = projects,  CONTROLS = permissionCtrl };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("SaveOccupancyPermit")]
        public async Task<IHttpActionResult> SaveOccupancyPermit(List<CustomOccupancyPermit> data)
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
                        DateTime dt = DateTime.Now;
                        var cId = User.Identity.GetUserId();

                        OccupancyPermit op = new OccupancyPermit(); // Occupancy Permit Table

                        foreach (var ds in data)
                        {
                            bool isExist = false;
                            string ProjectCode = "0";
                            string CompanyCode = "0";

                            // Get Company Code and Project Code based on Primary Key
                            int ProjectID = Convert.ToInt16(ds.ProjectID);
                            var prj = db.VW_Projects.Where(x => x.Id == ProjectID).SingleOrDefault();
                            if (prj != null)
                            {
                                ProjectCode = prj.ProjectCode;
                                CompanyCode = prj.CompanyCode;
                            }

                            // check record if project ID and PhaseName is already exists
                            isExist = db.OccupancyPermits.Where(x => x.CompanyCode == CompanyCode && x.ProjectCode == ProjectCode && x.Phase == ds.Phase).Any();                        
                            if (isExist) continue;

                            op.CompanyCode = ds.CompanyCode;
                            op.ProjectCode = ds.ProjectCode;
                            op.ProjectName = ds.ProjectName;
                            op.Phase = ds.Phase;

                            if (ds.PertmitDate != null)
                                op.PermitDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(ds.PertmitDate.GetValueOrDefault().ToLocalTime(), timezone);

                            op.PostedDate = dt;
                            op.Remarks = ds.Remarks;
                            op.CreatedByPK = cId;
                            op.CreatedDate = dt;
                            op.ModifiedByPK = cId;
                            op.ModifiedDate = dt;

                            db.OccupancyPermits.Add(op);
                            await db.SaveChangesAsync();
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "UPDATE";
                        log.Description = "Update " + this.ApiName;
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.EventName = this.ApiName;
                        log.ContentDetail = JsonConvert.SerializeObject(op);
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