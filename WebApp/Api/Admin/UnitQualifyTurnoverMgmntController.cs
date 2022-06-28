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
using System.Data.SqlClient;

namespace WebApp.Api.Admin
{
    [Authorize]
    [RoutePrefix("api/UnitQualifyTurnoverMgmnt")]
    public class UnitQualifyTurnoverMgmntController : ApiController
    {
        private string PageUrl = "/Admin/UnitQualifyTurnoverMgmnt";
        private string ApiName = "Unit Qualification and Turnover Management";

        string timezone = "";

        private UnitQualifyTurnoverMgmntController()
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
        [Route("GetUnitQualifyMasterlist")]
        public async Task<IHttpActionResult> GetUnitQualifyMasterlist([FromUri] FilterModel param)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var permissionCtrl = this.GetPermissionControl(param.PageUrl);

                    // Get List of Qualified Clients for Scheduling
                    IQueryable<VW_QualifyList> source = db.VW_QualifyList.OrderBy(x => x.QualificationDate);

                    // searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.CustomerNos.ToLower().Contains(param.search) || x.CustomerName1.ToLower().Contains(param.search) || x.RefNos.ToLower().Contains(param.search) ||
                                                    x.UnitCategory.ToLower().Contains(param.search) || x.ProjectCode.ToLower().Contains(param.search) ||
                                                    x.CompanyCode.ToLower().Contains(param.search));
                    }

                    // paging
                    var sourcePaged = source.Skip((param.page - 1) * param.itemsPerPage).Take(param.itemsPerPage);

                    var results = await sourcePaged.Select(x => new
                    {
                        x.Id,
                        x.NoticeTOID,
                        x.CompanyCode,
                        x.ProjectCode,
                        x.BusinessEntity,
                        x.RefNos,
                        x.UnitNos,
                        x.UnitCategory,
                        x.CustomerNos,
                        x.CustomerName1,
                        x.TOAS,
                        x.CMGAcceptanceDate,
                        x.OccupancyPermitDate,
                        x.QualificationDate,
                        x.SalesDocNos,
                        x.QuotDocNos,
                        x.SalesDocStatus
                    }).ToListAsync();

                    IEnumerable<CustomUnitQD_Qualifylist> qualify = null;
                    qualify = results.Select(x => new CustomUnitQD_Qualifylist
                    {
                        UniqueHashKey = StringCipher.Crypt(String.Concat(x.CompanyCode, "|", x.ProjectCode, "|", x.UnitCategory, "|", x.UnitNos, "|", x.CustomerNos, "|", x.ProjectCode, " : ", x.BusinessEntity.Replace('|', ' '), "|", x.RefNos)),
                        Id = x.Id,
                        NoticeTOID = x.NoticeTOID,
                        CompanyCode = x.CompanyCode,
                        ProjectCode = x.ProjectCode,
                        UnitNos = x.UnitNos,
                        RefNos = x.RefNos,
                        UnitCategory = x.UnitCategory,
                        CustomerNos = x.CustomerNos,
                        CustomerName = x.CustomerName1,
                        TOAS = x.TOAS,
                        CMGAcceptanceDate = x.CMGAcceptanceDate,
                        OccupancyPermitDate = x.OccupancyPermitDate,
                        QualificationDate = x.QualificationDate,
                        QuotDocNos = x.QuotDocNos,
                        SalesDocNos = x.SalesDocNos,
                        SalesDocStatus = x.SalesDocStatus
                    }).AsEnumerable();

                    // sorting
                    var sortby = typeof(CustomUnitQD_Qualifylist).GetProperty(param.sortby);
                    switch (param.reverse)
                    {
                        case true:
                            qualify = qualify.OrderByDescending(s => sortby.GetValue(s, null));
                            break;
                        case false:
                            qualify = qualify.OrderBy(s => sortby.GetValue(s, null));
                            break;
                    }

                    var data = new { COUNT = source.Count(), UnitQualifyMasterlistLIST = qualify, CONTROLS = permissionCtrl };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }
        [Route("GetSearchData")]
        public async Task<IHttpActionResult> GetSearchData([FromUri] SearchData item)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    // Check if system parameter is properly set
                    var systemParameter = await db.SystemParameters.Where(x => x.Published == true).FirstOrDefaultAsync();
                    if (systemParameter == null)
                        return BadRequest("Please configure system parameter");

                    var prj = db.VW_Projects.Where(x => x.TOM == true && x.Id == item.ProjectID).SingleOrDefault();
                    if (prj != null)
                    {
                        item.ProjectCode = prj.ProjectCode;
                        item.CompanyCode = prj.CompanyCode;
                    }

                    // Get List of Active Projects
                    var projects = await db.VW_QualifiedUnit.Where(x => x.TOAS != null && x.TranClass == "Business Rule 1").Select(x => new { Id = x.ProjectId, x.CompanyCode, x.ProjectCode, x.BusinessEntity, x.ProjectLocation, ProjectCodeName = x.ProjectCode + " : " + x.BusinessEntity }).Distinct().OrderBy(x => x.BusinessEntity).ToListAsync();

                    // Get List of Units Inventory
                    var units = await db.VW_QualifiedUnit.Where(x => x.TOAS != null && x.TranClass == "Business Rule 1" && x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitCategoryCode == item.UnitCategory).Select(x => new { x.UnitNos, x.RefNos, x.CustomerNos }).OrderBy(x => x.UnitNos).ToListAsync();

                    // Get List of Customer with SO
                    var customers = await db.VW_QualifiedUnit.Where(x => x.TOAS != null && x.TranClass == "Business Rule 1" && x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitCategoryCode == item.UnitCategory).Select(x => new { x.CustomerNos, x.CustomerName1, x.CustomerHash, x.UnitNos }).OrderBy(x => x.CustomerNos).ToListAsync();

                    // Business Rule without SAP Cut-off Date based on System Parameter
                    if (systemParameter.EnableTOCutOffDate == false)
                    {
                        projects = await db.VW_QualifiedUnit.Where(x => x.TOAS != null).Select(x => new { Id = x.ProjectId, x.CompanyCode, x.ProjectCode, x.BusinessEntity, x.ProjectLocation, ProjectCodeName = x.ProjectCode + " : " + x.BusinessEntity }).Distinct().OrderBy(x => x.BusinessEntity).ToListAsync();
                        units = await db.VW_QualifiedUnit.Where(x => x.TOAS != null && x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitCategoryCode == item.UnitCategory).Select(x => new { x.UnitNos, x.RefNos, x.CustomerNos }).OrderBy(x => x.UnitNos).ToListAsync();
                        customers = await db.VW_QualifiedUnit.Where(x => x.TOAS != null && x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitCategoryCode == item.UnitCategory).Select(x => new { x.CustomerNos, x.CustomerName1, x.CustomerHash, x.UnitNos }).OrderBy(x => x.CustomerNos).ToListAsync();
                    }

                    // if customer nos is not empty then include on the condition
                    if (!String.IsNullOrEmpty(item.CustomerNos) && units != null)
                        units = units.Where(x => x.CustomerNos == item.CustomerNos).ToList();

                    // if unit nos is not empty then include on the condition
                    if (!String.IsNullOrEmpty(item.UnitNos) && customers != null)
                        customers = customers.Where(x => x.UnitNos == item.UnitNos).ToList();

                    var data = new { PROJECTLIST = projects, UNITLIST = units, CUSTOMERLIST = customers };

                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }
        [Route("GetUnitQualifyTurnoverMgmnt")]
        public async Task<IHttpActionResult> GetUnitQualifyTurnoverMgmnt([FromUri] SearchData item)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var permissionCtrl = this.GetPermissionControl(item.PageUrl);

                    // Get Current User
                    var cId = User.Identity.GetUserId();
                    var user = db.AspNetUsersProfiles.Where(x => x.Id == cId).Select(x => new { vFullname = x.vFirstName + " " + x.vLastName }).SingleOrDefault().vFullname;

                    // Check if system parameter is properly set
                    var systemParameter = await db.SystemParameters.Where(x => x.Published == true).FirstOrDefaultAsync();
                    if (systemParameter == null)
                        return BadRequest("Please configure system parameter");

                    // Get List of Holidays
                    var exceptionDays = await db.HolidayDimensions.Select(x => x.TheDate2).ToArrayAsync();

                    // Get List of Turnover Options
                    var turnoveroptions = await db.TurnoverOptions.Where(x => x.Published == true).Select(x => new { x.Id, x.Name }).OrderBy(x => x.Id).ToListAsync();

                    // -------------- QUALIFIED FOR TURNOVER --------------- //
                    // Get List of Qualified Clients for Scheduling                    
                    var source = await db.VW_QualifiedTurnover.Where(x => x.TOAS != null && x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitCategoryCode == item.UnitCategory).ToListAsync();
                    if (source != null)
                    {
                        // Business Rule with SAP Cut-off Date based on System Parameter
                        if (systemParameter.EnableTOCutOffDate == true)
                            source = source.Where(x => x.TranClass == "Business Rule 1").ToList();

                        // if unit nos is set, include in the criteria
                        if (!String.IsNullOrEmpty(item.UnitNos) && source != null)
                            source = source.Where(x => x.UnitNos == item.UnitNos).ToList();

                        // if customer nos is set, include in the criteria
                        if (!String.IsNullOrEmpty(item.CustomerNos) && source != null)
                            source = source.Where(x => x.CustomerNos == item.CustomerNos).ToList();
                    }

                    // get only first record. BU must specify specific searching
                    var qualifiedInfo = source.FirstOrDefault();

                    // if doesn't exist display error message
                    if (qualifiedInfo == null)
                        return BadRequest("Record not found");
                    // -------------- QUALIFIED FOR TURNOVER --------------- //
                    // Unit Qualification Data
                    var unitqualify = await db.UnitQD_Qualification.Where(x => x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitCategory == item.UnitCategory && x.UnitNos == qualifiedInfo.UnitNos && x.CustomerNos == qualifiedInfo.CustomerNos).FirstOrDefaultAsync();

                    // Notice of Unit Turnover Data
                    var noticeunitto = await db.UnitQD_NoticeTO.Where(x => x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitCategory == item.UnitCategory && x.UnitNos == qualifiedInfo.UnitNos && x.CustomerNos == qualifiedInfo.CustomerNos).FirstOrDefaultAsync();

                    // Turnover Schedule and Option
                    CustomUnitQD_TOSchedule toschedule = new CustomUnitQD_TOSchedule();
                    var tos = await db.UnitQD_TOSchedule.Where(x => x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitCategory == item.UnitCategory && x.UnitNos == qualifiedInfo.UnitNos && x.CustomerNos == qualifiedInfo.CustomerNos).FirstOrDefaultAsync();
                    if (tos != null)
                    {
                        toschedule.Id = tos.Id;
                        toschedule.CompanyCode = tos.CompanyCode;
                        toschedule.ProjectCode = tos.ProjectCode;
                        toschedule.UnitCategory = tos.UnitCategory;
                        toschedule.UnitNos = tos.UnitNos;
                        toschedule.CustomerNos = tos.CustomerNos;
                        toschedule.TurnoverDate1 = tos.TurnoverDate1;
                        toschedule.TurnoverTime1 = DateTime.Today.Add(tos.TurnoverTime1);
                        toschedule.TurnoverOption1 = tos.TurnoverOption1;
                        toschedule.TurnoverAttachment1 = tos.TurnoverAttachment1;
                        toschedule.TurnoverRemarks1 = tos.TurnoverRemarks1;
                        if (tos.TurnoverDate2 != null)
                            toschedule.TurnoverDate2 = tos.TurnoverDate2;
                        if (tos.TurnoverTime2 != null)
                            toschedule.TurnoverTime2 = DateTime.Today.Add(tos.TurnoverTime2.GetValueOrDefault());
                        toschedule.TurnoverOption2 = tos.TurnoverOption2;
                        toschedule.TurnoverAttachment2 = tos.TurnoverAttachment2;
                        toschedule.TurnoverRemarks2 = tos.TurnoverRemarks2;
                        toschedule.CreatedDate = tos.CreatedDate;
                        toschedule.CreatedByPK = tos.CreatedByPK;
                        toschedule.ModifiedDate = tos.ModifiedDate;
                        toschedule.ModifiedByPK = tos.ModifiedByPK;
                    }
                    else
                        toschedule = null;

                    // Client Information
                    IEnumerable<CustomCustomerProfile> clientinfo = await (from csp in db.CustomerProfiles
                                                                           where csp.CustomerNos == qualifiedInfo.CustomerNos
                                                                           select new CustomCustomerProfile
                                                                           {
                                                                               Id = csp.Id,
                                                                               CustomerNos = csp.CustomerNos,
                                                                               ClientRemarks = csp.ClientRemarks,
                                                                               ClientAttachment = csp.ClientAttachment,
                                                                               ModifiedByPK = csp.ModifiedByPK,
                                                                               ModifiedDate = csp.ModifiedDate,
                                                                               CreatedByPK = csp.CreatedByPK,
                                                                               CreatedDate = csp.CreatedDate
                                                                           }).OrderByDescending(x => x.CreatedDate).ToListAsync();

                    var data = new { CURUSER = user, EXCEPTIONDAYS = exceptionDays, TURNOVEROPTIONLIST = turnoveroptions, QUALIFIEDNFO = qualifiedInfo, UNITQUALIF = unitqualify, NUTOINFO = noticeunitto, TOSCHEDULEINFO = toschedule, CLIENTINFO = clientinfo, SYSPARAM = systemParameter, CONTROLS = permissionCtrl };

                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }
        [Route("GetAdjustTurnoverDate")]
        public async Task<IHttpActionResult> GetAdjustTurnoverDate([FromUri] CustomUnitQD_NoticeTO data)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    // Check if system parameter is properly set
                    var systemParameter = await db.SystemParameters.Where(x => x.Published == true).FirstOrDefaultAsync();
                    if (systemParameter == null)
                        return BadRequest("Please configure system parameter");

                    DateTime adjtoDate = DateTime.Today;
                    if (systemParameter.TurnoverMaxDaysCT == "CD")
                        adjtoDate = data.EmailDateNoticeSent.AddDays(systemParameter.TurnoverMaxDays); // Get only date based on calendar days 
                    else
                        adjtoDate = Convert.ToDateTime(db.Database.SqlQuery<string>("SELECT dbo.fnAdjustmentDate({0}, {1})", data.EmailDateNoticeSent, systemParameter.TurnoverMaxDays + 1).Single()); // Get only date based on working days 
                    return Ok(adjtoDate);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
        [Route("SaveNoticeUnitTurnover")]
        public async Task<IHttpActionResult> SaveNoticeUnitTurnover(CustomUnitQD_NoticeTO data)
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
                        var cId = User.Identity.GetUserId();

                        // Check if system parameter is properly set
                        var systemParameter = await db.SystemParameters.Where(x => x.Published == true).FirstOrDefaultAsync();
                        if (systemParameter == null)
                            return BadRequest("Please configure system parameter");

                        // Fill-out mandatory fields
                        if (data.EmailDateNoticeSent == null || data.EmailTurnoverDate == null || data.EmailTurnoverTime == null || (data.CourierDateNoticeReceived != null && data.CourierReceivedBy == null) || (data.CourierReceivedBy != null && data.CourierDateNoticeReceived == null) || (data.CourierDateNoticeSent == null && (data.CourierReceivedBy != null && data.CourierDateNoticeReceived != null)))
                            return BadRequest("Please fill-out or tag mandatory fields to be able to proceed");

                        UnitQD_NoticeTO nuto = new UnitQD_NoticeTO();

                        nuto.Id = data.Id;
                        nuto.SalesDocNos = data.SalesDocNos;
                        nuto.QuotDocNos = data.QuotDocNos;
                        nuto.CompanyCode = data.CompanyCode;
                        nuto.CustomerNos = data.CustomerNos;
                        nuto.ProjectCode = data.ProjectCode;
                        nuto.UnitNos = data.UnitNos;
                        nuto.UnitCategory = data.UnitCategory;

                        nuto.EmailDateNoticeSent = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.EmailDateNoticeSent, timezone);

                        nuto.EmailTurnoverDate = new DateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.EmailTurnoverDate, timezone).Year,
                                                            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.EmailTurnoverDate, timezone).Month,
                                                            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.EmailTurnoverDate, timezone).Day,
                                                            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.EmailTurnoverTime, timezone).Hour,
                                                            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.EmailTurnoverTime, timezone).Minute,
                                                            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.EmailTurnoverTime, timezone).Second);
                        nuto.EmailNoticeRemarks = data.EmailNoticeRemarks;
                        nuto.EmailNoticeAttachment = data.EmailNoticeAttachment;

                        if (data.CourierDateNoticeSent != null)
                        {
                            nuto.CourierDateNoticeSent = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.CourierDateNoticeSent.GetValueOrDefault().ToLocalTime(), timezone);

                            // Courier Date Notice Sent will NOT accept future date
                            // Reference FRD: Transaction > Unit Qualification & Turnover Management > Notice of Unit Turnover (5.2)
                            if (DateTime.Today < nuto.CourierDateNoticeSent)
                                return BadRequest("Courier Date Notice Sent will not accept future date");
                        }

                        if (data.CourierDateNoticeReceived != null)
                        {
                            nuto.CourierDateNoticeReceived = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.CourierDateNoticeReceived.GetValueOrDefault().ToLocalTime(), timezone);

                            // Courier Date Notice Received will NOT accept future date
                            // Reference FRD: Transaction > Unit Qualification & Turnover Management > Notice of Unit Turnover (5.2)
                            if (DateTime.Today < nuto.CourierDateNoticeSent)
                                return BadRequest("Courier Date Notice Received will not accept future date");
                        }

                        // Courier Date Notice Received must be tagged at the same time or after the Courier Date Notice Sent 
                        // field is tagged or posted on the system, and not before it
                        // Reference FRD: Transaction > Unit Qualification & Turnover Management > Notice of Unit Turnover (5.2.3.5)
                        if (data.CourierDateNoticeSent != null && data.CourierDateNoticeReceived != null)
                        {
                            if (data.CourierDateNoticeSent > data.CourierDateNoticeReceived)
                                return BadRequest("Courier Date Notice Received must be tagged at the same time or after the Courier Date Notice Sent");
                        }

                        nuto.CourierReceivedBy = data.CourierReceivedBy;
                        nuto.CourierNoticeRemarks = data.CourierNoticeRemarks;
                        nuto.CourierNoticeAttachment = data.CourierNoticeAttachment;
                        nuto.HandoverAssociate = data.HandoverAssociate;
                        nuto.ModifiedByPK = cId;
                        nuto.ModifiedDate = DateTime.Now;
                        nuto.CreatedByPK = data.CreatedByPK;
                        nuto.CreatedDate = data.CreatedDate;

                        if (nuto.Id == 0)
                        {
                            // Will NOT accept future date
                            // Reference FRD: Transaction > Unit Qualification & Turnover Management > Notice of Unit Turnover (5.2)
                            if (DateTime.Today < nuto.EmailDateNoticeSent)
                                return BadRequest("Email Date Notice Sent will not accept future date");

                            // Email Notice of Unit Turnover Date should not be beyond x (calendar type) days from the Qualification date
                            // Reference FRD: Transaction > Unit Qualification & Turnover Management > Notice of Unit Turnover (5.2)
                            if (nuto.EmailTurnoverDate > TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.EmailTurnoverMaxDate, timezone))
                                return BadRequest("Date or time provided is beyond the " + systemParameter.TurnoverMaxDays + " day(s) allowable timeline");

                            // Email Notice of Unit Turnover Date will not allow date on Saturday, Sunday and holidays
                            // Reference FRD: Transaction > Unit Qualification & Turnover Management > Notice of Unit Turnover (5.2)
                            var rs1 = db.spDateTimeChecker(nuto.EmailTurnoverDate, 1).SingleOrDefault().Value;
                            if (Convert.ToBoolean(rs1))
                                return BadRequest("Date on Saturday, Sunday and Holidays are not allowed");

                            // Email Notice of Unit Turnover Date will not allow date on Saturday, Sunday and holidays
                            // Reference FRD: Transaction > Unit Qualification & Turnover Management > Notice of Unit Turnover (5.2)
                            var rs2 = db.spDateTimeChecker(nuto.EmailTurnoverDate, 2).SingleOrDefault().Value;
                            if (Convert.ToBoolean(rs2))
                            {
                                DateTime BusHrFrom = new DateTime(2020, 01, 01, systemParameter.BusinessHourFrom.Hours, systemParameter.BusinessHourFrom.Minutes, systemParameter.BusinessHourFrom.Seconds);
                                DateTime BusHrTo = new DateTime(2020, 01, 01, systemParameter.BusinessHourTo.Hours, systemParameter.BusinessHourTo.Minutes, systemParameter.BusinessHourTo.Seconds);

                                return BadRequest("Time should be during business hours (" + BusHrFrom.ToString("hh:mm tt") + " to " + BusHrTo.ToString("hh:mm tt") + ") only");
                            }

                            // Turnover Schedule Date should not be beyond x (calendar type) days from the Date of Email Notice Sent
                            // Reference FRD: Transaction > Unit Qualification & Turnover Management > Turnover Schedule & Option (5.2)
                            DateTime toTAT = DateTime.Today;
                            if (systemParameter.TurnoverMaxDays2CT == "CD")
                                toTAT = nuto.EmailDateNoticeSent.AddDays(systemParameter.TurnoverMaxDays2); // Get only date based on calendar days 
                            else
                                toTAT = Convert.ToDateTime(db.Database.SqlQuery<string>("SELECT dbo.fnAdjustmentDate({0}, {1})", nuto.EmailDateNoticeSent, systemParameter.TurnoverMaxDays2).Single()); // Get only date based on working days 
                            nuto.ScheduleTurnoverMaxDate = toTAT;

                            // NO confirmation of 1st Turnover Schedule & preferred Turnover Option after x (calendar type) from the Date of Email Notice Sent
                            // Reference FRD: Transaction > Unit Qualification & Turnover Management > Turnover Schedule & Option (5.4)
                            DateTime notif1Date = DateTime.Today;
                            if (systemParameter.ReschedMaxDays1CT == "CD")
                                notif1Date = nuto.EmailDateNoticeSent.AddDays(systemParameter.ReschedMaxDays1 + 1); // Get only date based on calendar days 
                            else
                                notif1Date = Convert.ToDateTime(db.Database.SqlQuery<string>("SELECT dbo.fnAdjustmentDate({0}, {1})", nuto.EmailDateNoticeSent, systemParameter.ReschedMaxDays1 + 2).Single()); // Get only date based on working days 
                            nuto.ScheduleEmailNotifDate1 = notif1Date;

                            // NO confirmation of 1st Turnover Schedule & preferred Turnover Option after x (calendar type) from the Date of Email Notice Sent
                            // Reference FRD: Transaction > Unit Qualification & Turnover Management > Turnover Schedule & Option (5.4)
                            DateTime notif2Date = DateTime.Today;
                            if (systemParameter.ReschedMaxDays2CT == "CD")
                                notif2Date = nuto.EmailDateNoticeSent.AddDays(systemParameter.ReschedMaxDays2 + 1); // Get only date based on calendar days 
                            else
                                notif2Date = Convert.ToDateTime(db.Database.SqlQuery<string>("SELECT dbo.fnAdjustmentDate({0}, {1})", nuto.EmailDateNoticeSent, systemParameter.ReschedMaxDays2 + 2).Single()); // Get only date based on working days 
                            nuto.ScheduleEmailNotifDate2 = notif2Date;

                            nuto.CreatedByPK = cId;
                            nuto.CreatedDate = DateTime.Now;

                            db.UnitQD_NoticeTO.Add(nuto);
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            nuto.ScheduleTurnoverMaxDate = data.ScheduleTurnoverMaxDate;
                            nuto.ScheduleEmailNotifDate1 = data.ScheduleEmailNotifDate1;
                            nuto.ScheduleEmailNotifDate2 = data.ScheduleEmailNotifDate2;
                            db.Entry(nuto).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "UPDATE";
                        log.Description = "Update " + this.ApiName + " - Notice of Unit Turnover";
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.EventName = this.ApiName;
                        log.ContentDetail = JsonConvert.SerializeObject(nuto);
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
        [Route("SaveTurnoverSchedule")]
        public async Task<IHttpActionResult> SaveTurnoverSchedule(CustomUnitQD_TOSchedule data)
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
                        var cId = User.Identity.GetUserId();
                        DateTime dt = DateTime.Now;

                        // Check if system parameter is properly set
                        var systemParameter = await db.SystemParameters.Where(x => x.Published == true).FirstOrDefaultAsync();
                        if (systemParameter == null)
                            return BadRequest("System Parameter is not properly set. Please contact System Administrator.");

                        // Fill-out mandatory fields
                        if (data.TurnoverDate1 == null || data.TurnoverTime1 == null || string.IsNullOrEmpty(data.TurnoverOption1))
                            return BadRequest("Please fill-out or tag mandatory fields to be able to proceed");

                        // Check TO Schedule Record
                        var toSchedule = db.VW_QualifiedTurnover.Where(x => x.TOAS != null && x.TranClass == "Business Rule 1" && x.CompanyCode == data.CompanyCode && x.ProjectCode == data.ProjectCode && x.UnitCategoryCode == data.UnitCategory && x.UnitNos == data.UnitNos && x.CustomerNos == data.CustomerNos).FirstOrDefault();

                        // Business Rule with SAP Cut-off Date based on System Parameter
                        if (systemParameter.EnableTOCutOffDate == false)
                            toSchedule = db.VW_QualifiedTurnover.Where(x => x.TOAS != null && x.CompanyCode == data.CompanyCode && x.ProjectCode == data.ProjectCode && x.UnitCategoryCode == data.UnitCategory && x.UnitNos == data.UnitNos && x.CustomerNos == data.CustomerNos).FirstOrDefault();

                        if (toSchedule != null)
                        {
                            if (toSchedule.wAcceptance == 1)
                            {
                                if (Convert.ToBoolean(toSchedule.TOIsPosted)) return BadRequest("Record is already Posted");  // Check if Posted
                                if (Convert.ToBoolean(toSchedule.TORule2) && Convert.ToBoolean(toSchedule.wAcceptance)) return BadRequest("Record was already tag with Acceptance/Deemed Acceptance Date");  // Check if with Acceptance/Deemed Acceptance Date
                            }
                        }

                        UnitQD_TOSchedule tos = new UnitQD_TOSchedule();

                        tos.Id = data.Id;
                        tos.SalesDocNos = data.SalesDocNos;
                        tos.QuotDocNos = data.QuotDocNos;
                        tos.CompanyCode = data.CompanyCode;
                        tos.CustomerNos = data.CustomerNos;
                        tos.ProjectCode = data.ProjectCode;
                        tos.UnitNos = data.UnitNos;
                        tos.UnitCategory = data.UnitCategory;

                        tos.TurnoverDate1 = new DateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TurnoverDate1, timezone).Year,
                                                        TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TurnoverDate1, timezone).Month,
                                                        TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TurnoverDate1, timezone).Day);

                        tos.TurnoverTime1 = new TimeSpan(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TurnoverTime1, timezone).Hour,
                                                        TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TurnoverTime1, timezone).Minute,
                                                        TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TurnoverTime1, timezone).Second);

                        tos.TurnoverOption1 = data.TurnoverOption1;
                        tos.TurnoverRemarks1 = data.TurnoverRemarks1;
                        tos.TurnoverAttachment1 = data.TurnoverAttachment1;

                        // Compute Turnover Status Tagging final/confirmed turnover schedule of unit owner
                        if (systemParameter.TurnoverStatusTATCT == "CD")
                            tos.TurnoverStatusTAT = tos.TurnoverDate1.AddDays(systemParameter.TurnoverStatusTAT); // Get only date based on calendar days 
                        else
                            tos.TurnoverStatusTAT = Convert.ToDateTime(db.Database.SqlQuery<string>("SELECT dbo.fnAdjustmentDate({0}, {1})", tos.TurnoverDate1, systemParameter.TurnoverStatusTAT + 1).Single()); // Get only date based on working days 

                        if (data.TurnoverDate2 != null)
                        {
                            tos.TurnoverDate2 = new DateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TurnoverDate2.GetValueOrDefault().ToLocalTime(), timezone).Year,
                                                            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TurnoverDate2.GetValueOrDefault().ToLocalTime(), timezone).Month,
                                                            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TurnoverDate2.GetValueOrDefault().ToLocalTime(), timezone).Day);

                            // Compute Turnover Status Tagging final/confirmed turnover schedule of unit owner
                            if (systemParameter.TurnoverStatusTATCT == "CD")
                                tos.TurnoverStatusTAT = tos.TurnoverDate2.Value.AddDays(systemParameter.TurnoverStatusTAT); // Get only date based on calendar days 
                            else
                                tos.TurnoverStatusTAT = Convert.ToDateTime(db.Database.SqlQuery<string>("SELECT dbo.fnAdjustmentDate({0}, {1})", tos.TurnoverDate2.Value, systemParameter.TurnoverStatusTAT + 1).Single()); // Get only date based on working days 
                        }

                        if (data.TurnoverTime2 != null)
                            tos.TurnoverTime2 = new TimeSpan(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TurnoverTime2.GetValueOrDefault().ToLocalTime(), timezone).Hour,
                                                            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TurnoverTime2.GetValueOrDefault().ToLocalTime(), timezone).Minute,
                                                            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TurnoverTime2.GetValueOrDefault().ToLocalTime(), timezone).Second);

                        tos.TurnoverOption2 = data.TurnoverOption2;
                        tos.TurnoverRemarks2 = data.TurnoverRemarks2;
                        tos.TurnoverAttachment2 = data.TurnoverAttachment2;
                        tos.ModifiedByPK = cId;
                        tos.ModifiedDate = dt;
                        tos.CreatedByPK = data.CreatedByPK;
                        tos.CreatedDate = data.CreatedDate;

                        if (tos.Id == 0)
                        {
                            // If date provided on the Turnover Date field is beyond x (calendar type) days
                            // (parameterized), from Email Date Notice Sent, or time on Turnover Time field is beyond the
                            // business hours, system will not allow and will display an error message.
                            if (tos.TurnoverDate1 > TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.ScheduleTurnoverMaxDate, timezone))
                                return BadRequest("Date or time provided is beyond the " + systemParameter.TurnoverMaxDays2 + " day(s) allowable timeline");

                            // Email Notice of Unit Turnover Date will not allow date on Saturday, Sunday and holidays
                            var rs2 = db.spDateTimeChecker(DateTime.Today.Add(tos.TurnoverTime1), 2).SingleOrDefault().Value;
                            if (Convert.ToBoolean(rs2))
                            {
                                DateTime BusHrFrom = new DateTime(2020, 01, 01, systemParameter.BusinessHourFrom.Hours, systemParameter.BusinessHourFrom.Minutes, systemParameter.BusinessHourFrom.Seconds);
                                DateTime BusHrTo = new DateTime(2020, 01, 01, systemParameter.BusinessHourTo.Hours, systemParameter.BusinessHourTo.Minutes, systemParameter.BusinessHourTo.Seconds);

                                return BadRequest("Time should be during business hours (" + BusHrFrom.ToString("hh:mm tt") + " to " + BusHrTo.ToString("hh:mm tt") + ") only");
                            }

                            tos.IsPosted = false;
                            tos.CreatedByPK = cId;
                            tos.CreatedDate = dt;

                            db.UnitQD_TOSchedule.Add(tos);
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            tos.IsPosted = data.IsPosted;

                            if (data.PostedDate != null)
                                tos.PostedDate = data.PostedDate;

                            if (data.TurnoverTime2 != null)
                            {
                                // Email Notice of Unit Turnover Date will not allow date on Saturday, Sunday and holidays
                                var rs2 = db.spDateTimeChecker(DateTime.Today.Add(tos.TurnoverTime2.GetValueOrDefault()), 2).SingleOrDefault().Value;
                                if (Convert.ToBoolean(rs2))
                                {
                                    DateTime BusHrFrom = new DateTime(2020, 01, 01, systemParameter.BusinessHourFrom.Hours, systemParameter.BusinessHourFrom.Minutes, systemParameter.BusinessHourFrom.Seconds);
                                    DateTime BusHrTo = new DateTime(2020, 01, 01, systemParameter.BusinessHourTo.Hours, systemParameter.BusinessHourTo.Minutes, systemParameter.BusinessHourTo.Seconds);

                                    return BadRequest("Time should be during business hours (" + BusHrFrom.ToString("hh:mm tt") + " to " + BusHrTo.ToString("hh:mm tt") + ") only");
                                }
                            }

                            db.Entry(tos).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }

                        // If value on the Turnover Option field for the unit is Express, system will automatically populate / tag the
                        // following fields with value / data which cannot be changed by the user(disabled)
                        // Reference FRD: Transaction > Unit Inspection & Acceptance Management > Turnover & Acceptance Status (5.3)
                        // Check TO Acceptance Record if exist
                        UnitID_TOAcceptance toa = new UnitID_TOAcceptance();
                        var toAcceptance = await db.UnitID_TOAcceptance.Where(x => x.CompanyCode == tos.CompanyCode && x.ProjectCode == tos.ProjectCode && x.UnitCategory == tos.UnitCategory && x.UnitNos == tos.UnitNos && x.CustomerNos == tos.CustomerNos).FirstOrDefaultAsync();
                        if ((tos.TurnoverOption1 == "Express" && string.IsNullOrEmpty(tos.TurnoverOption2)) || tos.TurnoverOption2 == "Express")
                        {
                            var turnoverDate = (tos.TurnoverDate2 != null) ? tos.TurnoverDate2 : tos.TurnoverDate1;

                            if (toAcceptance == null)
                            {
                                toa.Id = 0;
                                toa.SalesDocNos = tos.SalesDocNos;
                                toa.QuotDocNos = tos.QuotDocNos;
                                toa.CompanyCode = tos.CompanyCode;
                                toa.CustomerNos = tos.CustomerNos;
                                toa.ProjectCode = tos.ProjectCode;
                                toa.UnitNos = tos.UnitNos;
                                toa.UnitCategory = tos.UnitCategory;
                                toa.TurnoverStatus = "AWOP";
                                toa.TurnoverStatusDate = dt;
                                toa.IsUnitAcceptanceDateSAPSync = 0; //(data.TurnoverDate2 != null && data.TurnoverTime2 != null && !string.IsNullOrEmpty(tos.TurnoverOption2)) ? 0 : 2;
                                toa.UnitAcceptanceDate = turnoverDate;
                                toa.KeyTransmittalDate = toa.UnitAcceptanceDate;
                                toa.ModifiedByPK = cId;
                                toa.ModifiedDate = dt;
                                toa.CreatedByPK = cId;
                                toa.CreatedDate = dt;

                                db.UnitID_TOAcceptance.Add(toa);
                                await db.SaveChangesAsync();
                            }
                            else
                            {
                                var sql = "Update UnitID_TOAcceptance SET TurnoverStatusDate = {1}, ModifiedDate = {1}, UnitAcceptanceDate = {2}, KeyTransmittalDate = {2}, ModifiedByPK = {3} WHERE Id = {0}";
                                await db.Database.ExecuteSqlCommandAsync(sql, toAcceptance.Id, dt, turnoverDate, cId);
                            }
                        }
                        else
                        {
                            if (toAcceptance != null)
                            {
                                var isTOSync = 0; //(toSchedule.TORule2 == 1 && toSchedule.AccountTypeCode != "L") ? 0 : 2;
                                var sql = "Update UnitID_TOAcceptance SET TurnoverStatus = {1}, IsUnitAcceptanceDateSAPSync = {2},  UnitAcceptanceDate = {3}, KeyTransmittalDate = {3}, ModifiedDate = {4}, ModifiedByPK = {5} WHERE Id = {0}";
                                await db.Database.ExecuteSqlCommandAsync(sql, toAcceptance.Id, "", isTOSync, null, dt, cId);
                            }
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "UPDATE";
                        log.Description = "Update " + this.ApiName + " - Turnover Schedule and Option";
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.EventName = this.ApiName;
                        log.ContentDetail = JsonConvert.SerializeObject(tos);
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
        [Route("SaveCustomerProfile")]
        public async Task<IHttpActionResult> SaveCustomerProfile(CustomCustomerProfile data)
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

                        CustomerProfile cp = new CustomerProfile();

                        cp.Id = data.Id;

                        cp.CustomerNos = data.CustomerNos;
                        cp.ClientRemarks = data.ClientRemarks;
                        cp.ClientAttachment = data.ClientAttachment;
                        cp.ModifiedByPK = cId;
                        cp.ModifiedDate = DateTime.Now;
                        cp.CreatedByPK = data.CreatedByPK;
                        cp.CreatedDate = data.CreatedDate;

                        if (cp.Id == 0)
                        {
                            nwe = true;
                            cp.CreatedByPK = cId;
                            cp.CreatedDate = DateTime.Now;

                            db.CustomerProfiles.Add(cp);
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            db.Entry(cp).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = (nwe) ? "CREATE" : "UPDATE";
                        log.Description = (nwe) ? "Create " + this.ApiName + " - Client Information" : "Update " + this.ApiName + " - Client Information";
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.EventName = this.ApiName;
                        log.ContentDetail = JsonConvert.SerializeObject(cp);
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
                        var checkNotice = db.VW_QualifyList.Where(x => x.Id == ID && x.NoticeTOID != 0).SingleOrDefault();
                        if (checkNotice != null)
                            return BadRequest("Cannot deleted record with Notice Turnover already.");

                        var cd = db.UnitQD_Qualification.Where(x => x.Id == ID).Select(x => new { x.Id, x.CompanyCode, x.ProjectCode, x.UnitCategory, x.UnitNos, x.CustomerNos, x.TOAS, x.CMGAcceptanceDate, x.OccupancyPermitDate, x.QualificationDate }).SingleOrDefault();

                        db.UnitQD_Qualification.RemoveRange(db.UnitQD_Qualification.Where(x => x.Id == ID));
                        db.SaveChanges();

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "DELETED";
                        log.Description = "Deleted single " + this.ApiName;
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
        public async Task<IHttpActionResult> RemoveRecords(CustomTurnoverOption data)
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
                        var cd = db.UnitQD_Qualification.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.CompanyCode, x.ProjectCode, x.UnitCategory, x.UnitNos, x.CustomerNos, x.TOAS, x.CMGAcceptanceDate, x.OccupancyPermitDate, x.QualificationDate }).ToList();

                        int cnt = 0, cnt1 = 0;
                        foreach (var ds in data.dsList)
                        {
                            var checkNotice = db.VW_QualifyList.Where(x => x.Id == ds.Id && x.NoticeTOID != 0).SingleOrDefault();
                            if (checkNotice != null)
                                cnt++;

                            var sql = "DELETE FROM UnitQD_Qualification WHERE Id = {0}";
                            await db.Database.ExecuteSqlCommandAsync(sql, ds.Id);
                            cnt1++;
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "DELETED";
                        log.Description = "Deleted list of " + this.ApiName;
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.EventName = this.ApiName;
                        log.ContentDetail = JsonConvert.SerializeObject(cd);
                        log.SaveTransactionLogs();
                        // ---------------- End Transaction Activity Logs -------------------- //
                        string messageNotif = cnt1 + " out of " + data.dsList.Count + " record(s) was successfuly deleted.";

                        return Ok(messageNotif);
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