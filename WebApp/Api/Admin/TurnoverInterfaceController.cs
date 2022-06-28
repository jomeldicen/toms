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
    [RoutePrefix("api/TurnoverInterface")]
    public class TurnoverInterfaceController : ApiController
    {
        private string PageUrl = "/Admin/TurnoverInterface";
        private string ApiName = "Turnover Option";

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

        [Route("GetTurnoverInterface")]
        public async Task<IHttpActionResult> GetTurnoverInterface([FromUri] FilterModel param)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var permissionCtrl = this.GetPermissionControl(param.PageUrl);

                    IEnumerable<CustomTOInterface> source1 = null;
                    source1 = await (from ta in db.UnitID_TOAcceptance
                                    where ta.UnitAcceptanceDate != null
                                    select new CustomTOInterface
                                    {
                                        Id = ta.Id,
                                        TOModule = "TO Acceptance",
                                        CompanyCode = ta.CompanyCode,
                                        ProjectCode = ta.ProjectCode,
                                        UnitNos = ta.UnitNos,
                                        UnitCategory = ta.UnitCategory,
                                        CustomerNos = ta.CustomerNos,
                                        TurnoverStatus = ta.TurnoverStatus,
                                        TurnoverStatusDate = ta.TurnoverStatusDate,
                                        TOAcceptanceDate = ta.UnitAcceptanceDate,
                                        IsTODateSAPSync = ta.IsUnitAcceptanceDateSAPSync,
                                        TODateSyncDate = ta.UnitAcceptanceDateSyncDate,
                                        isChecked = false
                                    }).ToListAsync();


                    IEnumerable<CustomTOInterface> source2 = null;
                    source2 = await  (from da in db.UnitID_DeemedAcceptance
                                    join ta in db.UnitID_TOAcceptance on new { da.CompanyCode, da.ProjectCode, da.UnitCategory, da.UnitNos, da.CustomerNos } equals new { ta.CompanyCode, ta.ProjectCode, ta.UnitCategory, ta.UnitNos, ta.CustomerNos }
                                     where da.DeemedAcceptanceDate != null                                     
                                     select new CustomTOInterface
                                     {
                                         Id = da.Id,
                                         TOModule = "Deemed Acceptance",
                                         CompanyCode = da.CompanyCode,
                                         ProjectCode = da.ProjectCode,
                                         UnitNos = da.UnitNos,
                                         UnitCategory = da.UnitCategory,
                                         CustomerNos = da.CustomerNos,
                                         TurnoverStatus = ta.TurnoverStatus,
                                         TurnoverStatusDate = ta.TurnoverStatusDate,
                                         TOAcceptanceDate = da.DeemedAcceptanceDate,
                                         IsTODateSAPSync = da.IsDeemedAcceptanceDateSAPSync,
                                         TODateSyncDate = da.DeemedAcceptanceDateSyncDate,
                                         isChecked = false
                                     }).ToListAsync();

                    // Merge 2 IEnumerable Data (UnitID_TOAcceptance & UnitID_DeemedAcceptance)

                    IEnumerable<CustomTOInterface> source = source1.Concat(source2);

                    // searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.TOModule.ToLower().Contains(param.search) || x.CompanyCode.ToLower().Contains(param.search) || x.ProjectCode.ToLower().Contains(param.search) || 
                                                    x.UnitNos.ToLower().Contains(param.search) || x.UnitCategory.ToLower().Contains(param.search) || x.CustomerNos.ToLower().Contains(param.search) ||
                                                    x.TurnoverStatus.ToLower().Contains(param.search));
                    }

                    // sorting
                    var sortby = typeof(CustomTOInterface).GetProperty(param.sortby);
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

                    var data = new { COUNT = source.Count(), TURNOVERINTERFACELIST = sourcePaged, CONTROLS = permissionCtrl };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(CustomTOInterface data)
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
                        foreach (var ds in data.dsList)
                        {
                            var sql = "Update UnitID_TOAcceptance SET IsUnitAcceptanceDateSAPSync = {1} WHERE Id = {0}";
                            if(ds.TOModule == "Deemed Acceptance")
                                sql = "Update UnitID_DeemedAcceptance SET IsDeemedAcceptanceDateSAPSync = {1} WHERE Id = {0}";

                            await db.Database.ExecuteSqlCommandAsync(sql, ds.Id, 0);
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "UPDATE";
                        log.Description = "Re-Synching TO Date from TOMS to SAP";
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.EventName = this.ApiName;
                        log.ContentDetail = JsonConvert.SerializeObject(data.dsList);
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