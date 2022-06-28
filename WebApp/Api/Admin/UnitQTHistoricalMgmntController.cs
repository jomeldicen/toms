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
    [RoutePrefix("api/UnitQTHistoricalMgmnt")]
    public class UnitQTHistoricalMgmntController : ApiController
    {
        private string PageUrl = "/Admin/UnitQTHistoricalMgmnt";
        private string ApiName = "Unit Historical Data Management";

        string timezone = "";

        private UnitQTHistoricalMgmntController()
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

                    // Get List of Active Projects
                    var projects = await db.VW_HistoricalUnit.Where(x=> x.TranClass == "Business Rule 2").Select(x => new { Id = x.ProjectId, x.CompanyCode, x.ProjectCode, x.BusinessEntity, x.ProjectLocation, ProjectCodeName = x.ProjectCode + " : " + x.BusinessEntity }).Distinct().OrderBy(x => x.BusinessEntity).ToListAsync();
                  
                    // Get List of Units Inventory
                    var units = await db.VW_HistoricalUnit.Where(x => x.TranClass == "Business Rule 2" && x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitCategoryCode == item.UnitCategory).Select(x => new { x.UnitNos, x.RefNos, x.CustomerNos }).OrderBy(x => x.UnitNos).ToListAsync();
                    // if customer nos is not empty then include on the condition
                    if (!String.IsNullOrEmpty(item.CustomerNos) && units != null)
                        units = units.Where(x => x.CustomerNos == item.CustomerNos).ToList();

                    // Get List of Customer with SO
                    var customers = await db.VW_HistoricalUnit.Where(x => x.TranClass == "Business Rule 2" && x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitCategoryCode == item.UnitCategory).Select(x => new { x.CustomerNos, x.CustomerName1, x.CustomerHash, x.UnitNos }).OrderBy(x => x.CustomerNos).ToListAsync();
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

        [Route("GetQTUnitHistoricalMgmnt")]
        public async Task<IHttpActionResult> GetQTUnitHistoricalMgmnt([FromUri] SearchData item)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var permissionCtrl = this.GetPermissionControl(item.PageUrl);

                    // Get Current User
                    var cId = User.Identity.GetUserId();
                    var user = db.AspNetUsersProfiles.Where(x => x.Id == cId).Select(x=> new { vFullname = x.vFirstName + " " + x.vLastName } ).SingleOrDefault().vFullname;

                    // Check if system parameter is properly set
                    var systemParameter = await db.SystemParameters.Where(x => x.Published == true).FirstOrDefaultAsync();
                    if (systemParameter == null)
                        return BadRequest("Please configure system parameter");

                    // Get List of Holidays
                    var exceptionDays = await db.HolidayDimensions.Select(x => x.TheDate2).ToArrayAsync();

                    // Get List of Turnover Options
                    var turnoveroptions = await db.TurnoverOptions.Where(x => x.Published == true).Select(x => new { x.Id, x.Name }).OrderBy(x => x.Id).ToListAsync();
                   
                    // Get List of Turnover Options
                    var punchlistcategory = await db.PunchlistCategories.Where(x => x.Published == true).Select(x => new { x.Id, x.Name, x.TurnaroundTime, x.CalendarType }).OrderBy(x => x.Id).ToListAsync();
                    
                    // -------------- HISTORICAL DATA --------------- //
                    // Get List of Qualified Clients for Scheduling                    
                    var source = await db.VW_HistoricalUnit.Where(x => x.TranClass == "Business Rule 2" && x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitCategoryCode == item.UnitCategory).ToListAsync();
                 
                    // if unit nos is set, include in the criteria
                    if (!String.IsNullOrEmpty(item.UnitNos) && source != null)
                        source = source.Where(x => x.UnitNos == item.UnitNos).ToList();

                    // if customer nos is set, include in the criteria
                    if (!String.IsNullOrEmpty(item.CustomerNos) && source != null)
                        source = source.Where(x => x.CustomerNos == item.CustomerNos).ToList();

                    // get only first record. BU must specify specific searching
                    var historicalInfo = source.FirstOrDefault();

                    // if doesn't exist display error message
                    if (historicalInfo == null)
                        return BadRequest("Record not found");
                    // -------------- QUALIFIED FOR TURNOVER --------------- //

                    // Unit Historical Data
                    CustomUnitHD_HistoricalData unithistorical = new CustomUnitHD_HistoricalData();
                    var uhist = await db.UnitHD_HistoricalData.Where(x => x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitCategory == item.UnitCategory && x.UnitNos == historicalInfo.UnitNos && x.CustomerNos == historicalInfo.CustomerNos).FirstOrDefaultAsync();
                    if (uhist != null)
                    {
                        unithistorical.Id = uhist.Id;
                        unithistorical.CompanyCode = uhist.CompanyCode;
                        unithistorical.ProjectCode = uhist.ProjectCode;
                        unithistorical.UnitNos = uhist.UnitNos;
                        unithistorical.UnitCategory = uhist.UnitCategory;
                        unithistorical.CustomerNos = uhist.CustomerNos;
                        if (uhist.FPMCAcceptanceDate != null)
                            unithistorical.FPMCAcceptanceDate = uhist.FPMCAcceptanceDate;
                        if (uhist.QCDAcceptanceDate != null)
                            unithistorical.QCDAcceptanceDate = uhist.QCDAcceptanceDate;
                        if (uhist.EmailDateNoticeSent != null)
                            unithistorical.EmailDateNoticeSent = uhist.EmailDateNoticeSent;
                        if (uhist.EmailTurnoverDate != null)
                            unithistorical.EmailTurnoverDate = uhist.EmailTurnoverDate;
                        unithistorical.EmailNoticeRemarks = uhist.EmailNoticeRemarks;
                        unithistorical.EmailNoticeAttachment = uhist.EmailNoticeAttachment;
                        if (uhist.CourierDateNoticeSent != null)
                            unithistorical.CourierDateNoticeSent = uhist.CourierDateNoticeSent;
                        if (uhist.CourierDateNoticeReceived != null)
                            unithistorical.CourierDateNoticeReceived = uhist.CourierDateNoticeReceived;
                        unithistorical.CourierReceivedBy = uhist.CourierReceivedBy;
                        unithistorical.CourierNoticeRemarks = uhist.CourierNoticeRemarks;
                        unithistorical.CourierNoticeAttachment = uhist.CourierNoticeAttachment;
                        unithistorical.HandoverAssociate = uhist.HandoverAssociate;
                        unithistorical.TurnoverOption1 = uhist.TurnoverOption1;
                        if (uhist.TurnoverDate1 != null)
                            unithistorical.TurnoverDate1 = uhist.TurnoverDate1;
                        if (uhist.TurnoverTime1 != null)
                            unithistorical.TurnoverTime1 = DateTime.Today.Add(uhist.TurnoverTime1.Value);
                        unithistorical.TurnoverRemarks1 = uhist.TurnoverRemarks1;
                        unithistorical.TurnoverAttachment1 = uhist.TurnoverAttachment1;
                        unithistorical.TurnoverOption2 = uhist.TurnoverOption2;
                        if (uhist.TurnoverDate2 != null)
                            unithistorical.TurnoverDate2 = uhist.TurnoverDate2;
                        if (uhist.TurnoverTime2 != null)
                            unithistorical.TurnoverTime2 = DateTime.Today.Add(uhist.TurnoverTime2.Value);
                        unithistorical.TurnoverRemarks2 = uhist.TurnoverRemarks2;
                        unithistorical.TurnoverAttachment2 = uhist.TurnoverAttachment2;
                        unithistorical.TurnoverStatus = uhist.TurnoverStatus;
                        unithistorical.PunchlistCategory = uhist.PunchlistCategory;
                        unithistorical.PunchlistItem = uhist.PunchlistItem;
                        unithistorical.OtherIssues = uhist.OtherIssues;
                        unithistorical.TSRemarks = uhist.TSRemarks;
                        unithistorical.TSAttachment = uhist.TSAttachment;
                        if (uhist.TurnoverStatusDate != null)
                            unithistorical.TurnoverStatusDate = uhist.TurnoverStatusDate;
                        if (uhist.UnitAcceptanceDate != null)
                            unithistorical.UnitAcceptanceDate = uhist.UnitAcceptanceDate;
                        if (uhist.KeyTransmittalDate != null)
                            unithistorical.KeyTransmittalDate = uhist.KeyTransmittalDate;
                        if (uhist.ReinspectionDate != null)
                            unithistorical.ReinspectionDate = uhist.ReinspectionDate;
                        if (uhist.AdjReinspectionDate != null)
                            unithistorical.AdjReinspectionDate = uhist.AdjReinspectionDate;
                        unithistorical.RushTicketNos = uhist.RushTicketNos;
                        unithistorical.SRRemarks = uhist.SRRemarks;
                        if (uhist.DeemedAcceptanceDate != null)
                            unithistorical.DeemedAcceptanceDate = uhist.DeemedAcceptanceDate;
                        unithistorical.DeemedAcceptanceRemarks = uhist.DeemedAcceptanceRemarks;
                        if (uhist.DAEmailDateNoticeSent != null)
                            unithistorical.DAEmailDateNoticeSent = uhist.DAEmailDateNoticeSent;
                        unithistorical.DAEmailNoticeAttachment = uhist.DAEmailNoticeAttachment;
                        unithistorical.DAEmailNoticeRemarks = uhist.DAEmailNoticeRemarks;
                        if (uhist.DACourierDateNoticeSent != null)
                            unithistorical.DACourierDateNoticeSent = uhist.DACourierDateNoticeSent;
                        if (uhist.DACourierDateNoticeReceived != null)
                            unithistorical.DACourierDateNoticeReceived = uhist.DACourierDateNoticeReceived;
                        unithistorical.DACourierReceivedBy = uhist.DACourierReceivedBy;
                        unithistorical.DACourierNoticeRemarks = uhist.DACourierNoticeRemarks;
                        unithistorical.DACourierNoticeAttachment = uhist.DACourierNoticeAttachment;
                        unithistorical.DAHandoverAssociate = uhist.DAHandoverAssociate;
                        unithistorical.CreatedDate = uhist.CreatedDate;
                        unithistorical.CreatedByPK = uhist.CreatedByPK;
                        unithistorical.ModifiedDate = uhist.ModifiedDate;
                        unithistorical.ModifiedByPK = uhist.ModifiedByPK;
                     } else
                        unithistorical = null;

                // Client Information
                IEnumerable<CustomCustomerProfile> clientinfo = await (from csp in db.CustomerProfiles
                                        where csp.CustomerNos == historicalInfo.CustomerNos
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

                    var data = new { CURUSER = user, EXCEPTIONDAYS = exceptionDays, TURNOVEROPTIONLIST = turnoveroptions, HISTORICALINFO = historicalInfo, UNITHISTORICAL = unithistorical, CLIENTINFO = clientinfo, SYSPARAM = systemParameter, CONTROLS = permissionCtrl };

                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("SaveHistoricalData")]
        public async Task<IHttpActionResult> SaveHistoricalData(CustomUnitHD_HistoricalData data)
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
                        if(systemParameter == null)
                            return BadRequest("Please configure system parameter");


                        //var unithd = db.UnitHD_HistoricalData.Where(x => x.Id == data.Id).SingleOrDefault();

                        UnitHD_HistoricalData hist = new UnitHD_HistoricalData();

                        hist.Id = data.Id;
                        hist.CompanyCode = data.CompanyCode;
                        hist.CustomerNos = data.CustomerNos;
                        hist.ProjectCode = data.ProjectCode;
                        hist.UnitNos = data.UnitNos;
                        hist.UnitCategory = data.UnitCategory;


                        if (data.QCDAcceptanceDate != null)
                            hist.QCDAcceptanceDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.QCDAcceptanceDate.GetValueOrDefault().ToLocalTime(), timezone);

                        if (data.FPMCAcceptanceDate != null)
                            hist.FPMCAcceptanceDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.FPMCAcceptanceDate.GetValueOrDefault().ToLocalTime(), timezone);

                        if (data.EmailDateNoticeSent != null)
                            hist.EmailDateNoticeSent = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.EmailDateNoticeSent.GetValueOrDefault().ToLocalTime(), timezone);

                        if (data.EmailTurnoverDate != null && data.EmailTurnoverTime != null)
                                hist.EmailTurnoverDate = new DateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.EmailTurnoverDate.GetValueOrDefault().ToLocalTime(), timezone).Year,
                                                                TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.EmailTurnoverDate.GetValueOrDefault().ToLocalTime(), timezone).Month,
                                                                TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.EmailTurnoverDate.GetValueOrDefault().ToLocalTime(), timezone).Day,
                                                                TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.EmailTurnoverTime.GetValueOrDefault().ToLocalTime(), timezone).Hour,
                                                                TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.EmailTurnoverTime.GetValueOrDefault().ToLocalTime(), timezone).Minute,
                                                                TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.EmailTurnoverTime.GetValueOrDefault().ToLocalTime(), timezone).Second);

                            hist.EmailNoticeRemarks = data.EmailNoticeRemarks;
                            hist.EmailNoticeAttachment = data.EmailNoticeAttachment;

                            if (data.CourierDateNoticeSent != null)
                            {
                                hist.CourierDateNoticeSent = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.CourierDateNoticeSent.GetValueOrDefault().ToLocalTime(), timezone);

                                // Courier Date Notice Sent will NOT accept future date
                                // Reference FRD: Transaction > Unit Qualification & Turnover Management > Notice of Unit Turnover (5.2)
                                if (DateTime.Today < hist.CourierDateNoticeSent)
                                    return BadRequest("Courier Date Notice Sent will not accept future date");
                            }

                            if (data.CourierDateNoticeReceived != null)
                            {
                                hist.CourierDateNoticeReceived = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.CourierDateNoticeReceived.GetValueOrDefault().ToLocalTime(), timezone);

                                // Courier Date Notice Received will NOT accept future date
                                // Reference FRD: Transaction > Unit Qualification & Turnover Management > Notice of Unit Turnover (5.2)
                                if (DateTime.Today < hist.CourierDateNoticeSent)
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

                            hist.CourierReceivedBy = data.CourierReceivedBy;
                            hist.CourierNoticeRemarks = data.CourierNoticeRemarks;
                            hist.CourierNoticeAttachment = data.CourierNoticeAttachment;
                            hist.HandoverAssociate = data.HandoverAssociate;

                        if (data.TurnoverDate1 != null)
                            hist.TurnoverDate1 = new DateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TurnoverDate1.GetValueOrDefault().ToLocalTime(), timezone).Year,
                                                        TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TurnoverDate1.GetValueOrDefault().ToLocalTime(), timezone).Month,
                                                        TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TurnoverDate1.GetValueOrDefault().ToLocalTime(), timezone).Day);

                        if (data.TurnoverTime1 != null)
                            hist.TurnoverTime1 = new TimeSpan(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TurnoverTime1.GetValueOrDefault().ToLocalTime(), timezone).Hour,
                                                            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TurnoverTime1.GetValueOrDefault().ToLocalTime(), timezone).Minute,
                                                            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TurnoverTime1.GetValueOrDefault().ToLocalTime(), timezone).Second);

                            hist.TurnoverOption1 = data.TurnoverOption1;
                            hist.TurnoverRemarks1 = data.TurnoverRemarks1;
                            hist.TurnoverAttachment1 = data.TurnoverAttachment1;

                            if (data.TurnoverDate2 != null)
                            {
                                hist.TurnoverDate2 = new DateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TurnoverDate2.GetValueOrDefault().ToLocalTime(), timezone).Year,
                                                                TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TurnoverDate2.GetValueOrDefault().ToLocalTime(), timezone).Month,
                                                                TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TurnoverDate2.GetValueOrDefault().ToLocalTime(), timezone).Day);

                            }

                            if (data.TurnoverTime2 != null)
                                hist.TurnoverTime2 = new TimeSpan(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TurnoverTime2.GetValueOrDefault().ToLocalTime(), timezone).Hour,
                                                                TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TurnoverTime2.GetValueOrDefault().ToLocalTime(), timezone).Minute,
                                                                TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TurnoverTime2.GetValueOrDefault().ToLocalTime(), timezone).Second);

                            hist.TurnoverOption2 = data.TurnoverOption2;
                            hist.TurnoverRemarks2 = data.TurnoverRemarks2;
                            hist.TurnoverAttachment2 = data.TurnoverAttachment2;     
                       
                            hist.TurnoverStatus = data.TurnoverStatus;
                            hist.PunchlistCategory = data.PunchlistCategory;
                            hist.PunchlistItem = data.PunchlistItem;
                            hist.OtherIssues = data.OtherIssues;
                            hist.TSRemarks = data.TSRemarks;
                            hist.TSAttachment = data.TSAttachment;
                            hist.RushTicketNos = data.RushTicketNos;
                            hist.SRRemarks = data.SRRemarks;

                            if (data.UnitAcceptanceDate != null)
                                hist.UnitAcceptanceDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.UnitAcceptanceDate.GetValueOrDefault().ToLocalTime(), timezone);

                            if (data.KeyTransmittalDate != null)
                                hist.KeyTransmittalDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.KeyTransmittalDate.GetValueOrDefault().ToLocalTime(), timezone);

                            if (data.ReinspectionDate != null)
                                hist.ReinspectionDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.ReinspectionDate.GetValueOrDefault().ToLocalTime(), timezone);

                            if (data.AdjReinspectionDate != null)
                                hist.AdjReinspectionDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.AdjReinspectionDate.GetValueOrDefault().ToLocalTime(), timezone);

                        if (data.DeemedAcceptanceDate != null)
                            hist.DeemedAcceptanceDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.DeemedAcceptanceDate.GetValueOrDefault().ToLocalTime(), timezone);

                        if (data.DAEmailDateNoticeSent != null)
                            hist.DAEmailDateNoticeSent = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.DAEmailDateNoticeSent.GetValueOrDefault().ToLocalTime(), timezone);

                        hist.DAEmailNoticeRemarks = data.DAEmailNoticeRemarks;
                            hist.DAEmailNoticeAttachment = data.DAEmailNoticeAttachment;

                            if (data.DACourierDateNoticeSent != null)
                                hist.DACourierDateNoticeSent = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.DACourierDateNoticeSent.GetValueOrDefault().ToLocalTime(), timezone);

                            if (data.DACourierDateNoticeReceived != null)
                                hist.DACourierDateNoticeReceived = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.DACourierDateNoticeReceived.GetValueOrDefault().ToLocalTime(), timezone);

                            hist.DACourierReceivedBy = data.DACourierReceivedBy;
                            hist.DACourierNoticeRemarks = data.DACourierNoticeRemarks;
                            hist.DACourierNoticeAttachment = data.DACourierNoticeAttachment;
                     
                        hist.ModifiedByPK = cId;
                        hist.ModifiedDate = DateTime.Now;
                        hist.CreatedByPK = data.CreatedByPK;
                        hist.CreatedDate = data.CreatedDate;

                        if (hist.Id == 0)
                        {
                         
                            // Will NOT accept future date
                            // Reference FRD: Transaction > Unit Qualification & Turnover Management > Notice of Unit Turnover (5.2)
                            if (DateTime.Today < hist.EmailDateNoticeSent)
                            return BadRequest("Email Date Notice Sent will not accept future date");

                            // Check if Email Turnover Time within Business Hours
                            if (data.EmailTurnoverDate != null)
                            {
                                var rs2 = db.spDateTimeChecker(hist.EmailTurnoverDate.GetValueOrDefault(), 2).SingleOrDefault().Value;
                                if (Convert.ToBoolean(rs2))
                                {
                                    DateTime BusHrFrom = new DateTime(2020, 01, 01, systemParameter.BusinessHourFrom.Hours, systemParameter.BusinessHourFrom.Minutes, systemParameter.BusinessHourFrom.Seconds);
                                    DateTime BusHrTo = new DateTime(2020, 01, 01, systemParameter.BusinessHourTo.Hours, systemParameter.BusinessHourTo.Minutes, systemParameter.BusinessHourTo.Seconds);

                                    return BadRequest("Time should be during business hours (" + BusHrFrom.ToString("hh:mm tt") + " to " + BusHrTo.ToString("hh:mm tt") + ") only");
                                }
                            }

                            // Check if TurnoverTime1 within Business Hours
                            if (data.TurnoverTime1 != null)
                            {
                                var rs2 = db.spDateTimeChecker(DateTime.Today.Add(hist.TurnoverTime1.GetValueOrDefault()), 2).SingleOrDefault().Value;
                                if (Convert.ToBoolean(rs2))
                                {
                                    DateTime BusHrFrom = new DateTime(2020, 01, 01, systemParameter.BusinessHourFrom.Hours, systemParameter.BusinessHourFrom.Minutes, systemParameter.BusinessHourFrom.Seconds);
                                    DateTime BusHrTo = new DateTime(2020, 01, 01, systemParameter.BusinessHourTo.Hours, systemParameter.BusinessHourTo.Minutes, systemParameter.BusinessHourTo.Seconds);

                                    return BadRequest("Time should be during business hours (" + BusHrFrom.ToString("hh:mm tt") + " to " + BusHrTo.ToString("hh:mm tt") + ") only");
                                }
                            }

                            // Check if TurnoverTime2 within Business Hours
                            if (data.TurnoverTime2 != null)
                            {
                                var rs2 = db.spDateTimeChecker(DateTime.Today.Add(hist.TurnoverTime2.GetValueOrDefault()), 2).SingleOrDefault().Value;
                                if (Convert.ToBoolean(rs2))
                                {
                                    DateTime BusHrFrom = new DateTime(2020, 01, 01, systemParameter.BusinessHourFrom.Hours, systemParameter.BusinessHourFrom.Minutes, systemParameter.BusinessHourFrom.Seconds);
                                    DateTime BusHrTo = new DateTime(2020, 01, 01, systemParameter.BusinessHourTo.Hours, systemParameter.BusinessHourTo.Minutes, systemParameter.BusinessHourTo.Seconds);

                                    return BadRequest("Time should be during business hours (" + BusHrFrom.ToString("hh:mm tt") + " to " + BusHrTo.ToString("hh:mm tt") + ") only");
                                }
                            }

                            hist.CreatedByPK = cId;
                            hist.CreatedDate = DateTime.Now;

                            db.UnitHD_HistoricalData.Add(hist);
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            db.Entry(hist).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }

                        // Unit Acceptance by CMG 
                        var unitAcceptance = db.UnitAcceptances.Where(x=> x.CompanyCode == hist.CompanyCode && x.ProjectCode == hist.ProjectCode && x.UnitNos == hist.UnitNos && x.UnitCategory == hist.UnitCategory && x.IsCancelled == false).Select(x => new { x.Id, x.CompanyCode, x.ProjectCode, x.UnitCategory, x.UnitNos, x.CustomerNos, x.FPMCAcceptanceDate, x.QCDAcceptanceDate, x.Remarks, x.IsCancelled, x.CreatedByPK, x.CreatedDate, x.ModifiedByPK, x.ModifiedDate }).OrderByDescending(x => x.Id).FirstOrDefault();
                    
                        UnitAcceptance ua = new UnitAcceptance();
                        ua.Id = (unitAcceptance == null) ? 0 : unitAcceptance.Id;
                        ua.CompanyCode = (unitAcceptance == null) ? hist.CompanyCode : unitAcceptance.CompanyCode;
                        ua.ProjectCode = (unitAcceptance == null) ? hist.ProjectCode : unitAcceptance.ProjectCode;
                        ua.UnitCategory = (unitAcceptance == null) ? hist.UnitCategory : unitAcceptance.UnitCategory;
                        ua.UnitNos = (unitAcceptance == null) ? hist.UnitNos : unitAcceptance.UnitNos;
                        ua.CustomerNos = (unitAcceptance == null) ? hist.CustomerNos : unitAcceptance.CustomerNos;
                        ua.FPMCAcceptanceDate = (unitAcceptance == null) ? hist.FPMCAcceptanceDate : (hist.FPMCAcceptanceDate != null && unitAcceptance.FPMCAcceptanceDate != null && hist.FPMCAcceptanceDate.Value.Date == unitAcceptance.FPMCAcceptanceDate.Value.Date) ? unitAcceptance.FPMCAcceptanceDate : hist.FPMCAcceptanceDate;
                        ua.QCDAcceptanceDate = (unitAcceptance == null) ? hist.QCDAcceptanceDate : (hist.QCDAcceptanceDate != null && unitAcceptance.QCDAcceptanceDate != null && hist.QCDAcceptanceDate.Value.Date == unitAcceptance.QCDAcceptanceDate.Value.Date) ? unitAcceptance.QCDAcceptanceDate : hist.QCDAcceptanceDate;
                        ua.Remarks = (unitAcceptance == null) ? "" : unitAcceptance.Remarks;
                        ua.IsCancelled = (unitAcceptance == null) ? false : unitAcceptance.IsCancelled;
                        ua.ModifiedDate = DateTime.Now;
                        ua.ModifiedByPK = cId;

                        if (ua.Id == 0)
                        {
                            ua.CreatedByPK = cId;
                            ua.CreatedDate = DateTime.Now;

                            db.UnitAcceptances.Add(ua);
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            ua.CreatedDate = (unitAcceptance == null) ? DateTime.Now : unitAcceptance.CreatedDate;
                            ua.CreatedByPK = (unitAcceptance == null) ? cId : unitAcceptance.CreatedByPK;

                            db.Entry(ua).State = EntityState.Modified;
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
                        log.ContentDetail = JsonConvert.SerializeObject(hist);
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
    }
}