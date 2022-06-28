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
    [RoutePrefix("api/UnitAcceptance")]
    public class UnitAcceptanceController : ApiController
    {
        private string PageUrl = "/Admin/UnitAcceptance";
        private string ApiName = "Unit Acceptance";

        string timezone = "";

        public UnitAcceptanceController()
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

        [Route("GetSearchData")]
        public async Task<IHttpActionResult> GetSearchData([FromUri] SearchData item)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var prj = db.VW_Projects.Where(x => x.TOM == true && x.Id == item.ProjectID).SingleOrDefault();
                    if (prj != null)
                    {
                        item.ProjectCode = prj.ProjectCode;
                        item.CompanyCode = prj.CompanyCode;
                    }

                    //// Get List of Active Projects
                    //var projects = await db.VW_SalesInventory.Where(x=> x.TOM == true && x.SalesDocStatus == "Active" && x.TranClass == "Business Rule 1").Select(x => new { Id = x.ProjectId, x.CompanyCode, x.ProjectCode, x.BusinessEntity, x.ProjectLocation, ProjectCodeName = x.ProjectCode + " : " + x.BusinessEntity }).Distinct().OrderBy(x => x.BusinessEntity).ToListAsync();

                    //// Get List of Units Inventory
                    //var units = await db.VW_SalesInventory.Where(x => x.TOM == true && x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitCategoryCode == item.UnitCategory && x.SalesDocStatus == "Active" && x.TranClass == "Business Rule 1").Select(x => new { x.UnitNos, x.RefNos, x.CustomerNos }).Distinct().OrderBy(x => x.UnitNos).ToListAsync();

                    // Get List of Active Projects
                    var projects = await db.VW_UnitInventory.Select(x => new { Id = x.ProjectId, x.CompanyCode, x.ProjectCode, x.BusinessEntity, x.ProjectLocation, ProjectCodeName = x.ProjectCode + " : " + x.BusinessEntity }).Distinct().OrderBy(x => x.BusinessEntity).ToListAsync();
 
                    // Get List of Units Inventory
                    var units = await db.VW_UnitInventory.Where(x => x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitCategoryCode == item.UnitCategory).Select(x => new { x.UnitNos, x.RefNos }).Distinct().OrderBy(x => x.UnitNos).ToListAsync();

                    var data = new { PROJECTLIST = projects, UNITLIST = units };

                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }


        [Route("GetUnitAcceptance")]
        public async Task<IHttpActionResult> GetUnitAcceptance([FromUri] SearchData item)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var permissionCtrl = this.GetPermissionControl(item.PageUrl);

                    // Get Current User
                    var cId = User.Identity.GetUserId();
                    var user = db.AspNetUsersProfiles.Where(x => x.Id == cId).Select(x => new { vFullname = x.vFirstName + " " + x.vLastName }).SingleOrDefault().vFullname;

                    var unitInfo = await db.VW_SalesInventory.Where(x => x.TOM == true && x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitNos == item.UnitNos && x.UnitCategoryCode == item.UnitCategory && x.SalesDocStatus == "Active").Select(x => new { x.UnitNos, x.CompanyCode, x.ProjectCode, x.UnitCategoryCode, x.CustomerNos, x.RefNos, x.CompanyName, x.BusinessEntity, x.ProjectLocation, x.Phase, x.UnitType, x.UnitTypeDesc, x.UnitCategoryDesc, x.UnitArea}).FirstOrDefaultAsync();
                    if(unitInfo == null)
                    {
                        unitInfo = await db.VW_UnitInventory.Where(x => x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitNos == item.UnitNos && x.UnitCategoryCode == item.UnitCategory).Select(x => new { x.UnitNos, x.CompanyCode, x.ProjectCode, x.UnitCategoryCode, CustomerNos = "", x.RefNos, x.CompanyName, x.BusinessEntity, x.ProjectLocation, x.Phase, x.UnitType, x.UnitTypeDesc, x.UnitCategoryDesc, x.UnitArea }).FirstOrDefaultAsync();
                    }

                    CustomUnitAcceptance unitAcceptance = await (from ua in db.UnitAcceptances
                                                                 where ua.CompanyCode == item.CompanyCode && ua.ProjectCode == item.ProjectCode && ua.UnitNos == item.UnitNos && ua.UnitCategory == item.UnitCategory && ua.IsCancelled == false
                                                                 select new CustomUnitAcceptance
                                                                 {
                                                                     Id = ua.Id,
                                                                     CompanyCode = ua.CompanyCode,
                                                                     ProjectCode = ua.ProjectCode,
                                                                     UnitCategory = ua.UnitCategory,
                                                                     UnitNos = ua.UnitNos,
                                                                     CustomerNos = ua.CustomerNos,
                                                                     FPMCAcceptanceDate = (DateTime)ua.FPMCAcceptanceDate,
                                                                     QCDAcceptanceDate = (DateTime)ua.QCDAcceptanceDate,
                                                                     Remarks = ua.Remarks,
                                                                     IsCancelled = ua.IsCancelled,
                                                                     CreatedByUser = string.Concat(ua.AspNetUser.AspNetUsersProfile.vFirstName, " ", ua.AspNetUser.AspNetUsersProfile.vLastName),
                                                                     CreatedDate = ua.CreatedDate,
                                                                     CreatedByPK = ua.CreatedByPK
                                                                 }).OrderByDescending(x => x.Id).FirstOrDefaultAsync();

                    var data = new { UNITINFO = unitInfo, UNITACCEPTANCEINFO = unitAcceptance, CONTROLS = permissionCtrl };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("SaveUnitAcceptance")]
        public async Task<IHttpActionResult> SaveUnitAcceptance(UnitAcceptance data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if(String.IsNullOrWhiteSpace(data.CompanyCode) || String.IsNullOrWhiteSpace(data.ProjectCode) || String.IsNullOrWhiteSpace(data.UnitCategory) || String.IsNullOrWhiteSpace(data.UnitNos)) // || String.IsNullOrWhiteSpace(data.CustomerNos)
                return BadRequest("Some data was not set properly!");

            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        bool nwe = false;
                        var cId = User.Identity.GetUserId();

                        var checkNotice = db.UnitQD_NoticeTO.Where(x => x.CompanyCode == data.CompanyCode && x.ProjectCode == data.ProjectCode && x.UnitCategory == data.UnitCategory && x.UnitNos == data.UnitNos && x.CustomerNos == data.CustomerNos).SingleOrDefault();
                        if (checkNotice != null)
                            return BadRequest("Cannot update record with Notice Turnover already.");

                        data.ModifiedByPK = cId;
                        data.ModifiedDate = DateTime.Now;
                        data.QCDAcceptanceDate = data.QCDAcceptanceDate == null ? data.QCDAcceptanceDate?? null : TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.QCDAcceptanceDate.GetValueOrDefault().ToLocalTime(), timezone);
                        data.FPMCAcceptanceDate = data.FPMCAcceptanceDate == null ? data.FPMCAcceptanceDate ?? null : TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.FPMCAcceptanceDate.GetValueOrDefault().ToLocalTime(), timezone);
                        data.IsCancelled = (data.IsCancelled == null) ? false : data.IsCancelled;

                        if (data.Id == 0)
                        {
                            nwe = true;
                            data.CreatedByPK = cId;
                            data.CreatedDate = DateTime.Now;

                            db.UnitAcceptances.Add(data);
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            db.Entry(data).State = EntityState.Modified; 
                            await db.SaveChangesAsync();

                            var sql = "UPDATE UnitQD_Qualification SET CMGAcceptanceDate = {0} WHERE CompanyCode = {1} AND ProjectCode = {2} AND UnitCategory = {3} AND UnitNos = {4} AND CustomerNos = {5}";
                            await db.Database.ExecuteSqlCommandAsync(sql, (data.QCDAcceptanceDate == null)? data.FPMCAcceptanceDate : data.QCDAcceptanceDate, data.CompanyCode, data.ProjectCode, data.UnitCategory, data.UnitNos, data.CustomerNos);

                            sql = "UPDATE UnitHD_HistoricalData SET QCDAcceptanceDate = {0}, FPMCAcceptanceDate = {1} WHERE CompanyCode = {2} AND ProjectCode = {3} AND UnitCategory = {4} AND UnitNos = {5} AND CustomerNos = {6}";
                            await db.Database.ExecuteSqlCommandAsync(sql, (data.QCDAcceptanceDate == null) ? null : data.QCDAcceptanceDate, (data.FPMCAcceptanceDate == null) ? null : data.FPMCAcceptanceDate, data.CompanyCode, data.ProjectCode, data.UnitCategory, data.UnitNos, data.CustomerNos);
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
    }
}