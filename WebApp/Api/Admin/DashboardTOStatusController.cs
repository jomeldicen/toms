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
    [RoutePrefix("api/DashboardTOStatus")]
    public class DashboardTOStatusController : ApiController
    {
        private string PageUrl = "/Admin/DashboardTOStatus";
        //private string ApiName = "Dashboard Turnover Status";

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
                    var projects = await db.VW_QualifiedUnit.Where(x => x.TOAS != null).Select(x => new { id = x.ProjectId, label = x.CompanyCode + " - " + x.CompanyCode + " - " + x.ProjectCode + " : " + x.BusinessEntity }).Distinct().OrderBy(x => x.label).ToListAsync();
                    if (systemParameter.EnableTOCutOffDate == true)
                        projects = await db.VW_QualifiedUnit.Where(x => x.TOAS != null && x.TranClass == "Business Rule 1").Select(x => new { id = x.ProjectId, label = x.ProjectCode + " : " + x.BusinessEntity }).Distinct().OrderBy(x => x.label).ToListAsync();


                    var data = new { PROJECTLIST = projects };

                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("GetMainSummary")]
        public async Task<IHttpActionResult> GetMainSummary()
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    // Dashboard Turnover Status Summary
                    var DashboardSummary = await db.Database.SqlQuery<CustomDashboard_StatusSummary>("EXEC spDashboardSummary {0}, {1}", "Status", "").FirstOrDefaultAsync();

                    // GET the Last Date Sync in SAP
                    var lastDateSync = db.HttpClientLogs.Where(x => x.ObjectType == "GetFetchAllSAPAccountWithTOAS").OrderByDescending(x => x.CreatedDate).FirstOrDefault().CreatedDate;

                    var data = new { DASHBOARDSUMMARY = DashboardSummary, LASTDATESYNC = lastDateSync };

                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("GetDetailSummary")]
        public async Task<IHttpActionResult> GetDetailSummary([FromUri] FilterModel param)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    // Get Selected Projects
                    if (param.searchbyids == null)
                        param.searchbyids = new List<int> { 0 };

                    var projCodes = db.VW_Projects.Where(x => x.TOM == true && param.searchbyids.Contains(x.Id)).Select(x => x.CompanyCode + "-" + x.ProjectCode).ToArray();
                    if ((projCodes == null || projCodes.Count() == 0) && string.IsNullOrWhiteSpace(param.search2) && string.IsNullOrWhiteSpace(param.search3))
                        return BadRequest("Please first make a selection from Project Field");

                    // Dashboard Turnover Status Summary
                    var DashboardSummary = await db.Database.SqlQuery<CustomDashboard_StatusSummary>("EXEC spDashboardSummary {0}, {1}", "Status", string.Join(",", projCodes)).FirstOrDefaultAsync();

                    var data = new { DASHBOARDSUMMARY2 = DashboardSummary };

                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("GetDashboardTOStatus")]
        public async Task<IHttpActionResult> GetDashboardTOStatus([FromUri] FilterModel param)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    // Check if system parameter is properly set
                    var systemParameter = await db.SystemParameters.Where(x => x.Published == true).FirstOrDefaultAsync();
                    if (systemParameter == null)
                        return BadRequest("Please configure system parameter");

                    DateTime dt = DateTime.Today;
                    var permissionCtrl = this.GetPermissionControl(param.PageUrl);

                    // Get Current User
                    var cId = User.Identity.GetUserId();
                    var user = db.AspNetUsersProfiles.Where(x => x.Id == cId).Select(x => new { vFullname = x.vFirstName + " " + x.vLastName }).SingleOrDefault().vFullname;

                    // Get Selected Projects
                    if (param.searchbyids == null)
                        param.searchbyids = new List<int> { 0 };

                    var projCodes = db.VW_Projects.Where(x => x.TOM == true && param.searchbyids.Contains(x.Id)).Select(x => x.CompanyCode + "-" + x.ProjectCode).ToArray();
                    if ((projCodes == null || projCodes.Count() == 0) && string.IsNullOrWhiteSpace(param.search2) && string.IsNullOrWhiteSpace(param.search3))
                        return BadRequest("Please first make a selection from Project Field");

                    var projects = db.VW_Projects.Where(x => x.TOM == true && param.searchbyids.Contains(x.Id)).Select(x => new { x.BusinessEntity }).Distinct().OrderBy(x => x.BusinessEntity).ToList(); 

                    // GET the Last Date Sync in SAP
                    var lastDateSync = db.HttpClientLogs.Where(x => x.ObjectType == "GetFetchAllSAPAccountWithTOAS").OrderByDescending(x => x.CreatedDate).FirstOrDefault().CreatedDate;

                    // Get List of Qualified Clients for Scheduling
                    IQueryable<VW_QualifiedTurnover> source = db.VW_QualifiedTurnover.Where(x => x.TOAS != null && x.FinalTurnoverDate != null && x.FinalTurnoverOption != null).
                        OrderBy(x => x.TurnoverStatus).OrderByDescending(x => x.TOStatusAging).OrderByDescending(x => x.DAEmailAging).OrderByDescending(x => x.PunchlistAging);

                    // Business Rule with SAP Cut-off Date based on System Parameter
                    if (systemParameter.EnableTOCutOffDate == true)
                        source = source.Where(x => x.TranClass == "Business Rule 1");

                    // For the Condition Query
                    switch (param.search2)
                    {
                        case "Ttl": //For TO Status TAT (Twt + Tbt)
                            source = source.Where(x => (x.TurnoverStatusTAT > dt && (x.TurnoverStatus == null || x.TurnoverStatus == "")) || (x.TurnoverStatusTAT <= dt && (x.TurnoverStatus == null || x.TurnoverStatus == "")));
                            break;
                        case "Twt": //For TO Status within TAT
                            source = source.Where(x => x.TurnoverStatusTAT > dt && (x.TurnoverStatus == null || x.TurnoverStatus == ""));
                            break;
                        case "Tbt": //For TO Status beyond TAT
                            source = source.Where(x => x.TurnoverStatusTAT <= dt && (x.TurnoverStatus == null || x.TurnoverStatus == ""));
                            break;
                        case "Rtl": //For Re-Inspections TAT (Rwt + Rbt)
                            source = source.Where(x => (((x.TurnoverStatus == "NAPL" && x.LastReinspectionDateTAT > dt) || (x.UnitAcceptanceDate == null && x.UnitAcceptanceDateTAT > dt)) && (x.TurnoverStatus != null || x.TurnoverStatus != "") && x.LastReinspectionDate == null && x.UnitAcceptanceDate == null && x.DeemedAcceptanceDate == null) || (((x.TurnoverStatus == "NAPL" && x.LastReinspectionDateTAT <= dt) || (x.UnitAcceptanceDate == null && x.UnitAcceptanceDateTAT <= dt)) && (x.TurnoverStatus != null || x.TurnoverStatus != "") && x.LastReinspectionDate == null && x.UnitAcceptanceDate == null && x.DeemedAcceptanceDate == null));
                            break;
                        case "Rwt": //For Re-Inspection within TAT
                            source = source.Where(x => ((x.TurnoverStatus == "NAPL" && x.LastReinspectionDateTAT > dt) || (x.UnitAcceptanceDate == null && x.UnitAcceptanceDateTAT > dt)) && (x.TurnoverStatus != null || x.TurnoverStatus != "") && x.LastReinspectionDate == null && x.UnitAcceptanceDate == null && x.DeemedAcceptanceDate == null);
                            break;
                        case "Rbt": //For Re-Inspection beyond TAT
                            source = source.Where(x => ((x.TurnoverStatus == "NAPL" && x.LastReinspectionDateTAT <= dt) || (x.UnitAcceptanceDate == null && x.UnitAcceptanceDateTAT <= dt)) && (x.TurnoverStatus != null || x.TurnoverStatus != "") && x.LastReinspectionDate == null && x.UnitAcceptanceDate == null && x.DeemedAcceptanceDate == null);
                            break;
                        case "Dtl": //For DA Processing TAT (Dwt + Dbt)
                            source = source.Where(x => (x.DAEmailDateNoticeSentMaxDate > dt && x.DeemedAcceptanceDate != null && x.DAEmailDateNoticeSent == null) || (x.DAEmailDateNoticeSentMaxDate <= dt && x.DeemedAcceptanceDate != null && x.DAEmailDateNoticeSent == null));
                            break;
                        case "Dwt": //For DA Processing within TAT
                            source = source.Where(x => x.DAEmailDateNoticeSentMaxDate > dt && x.DeemedAcceptanceDate != null && x.DAEmailDateNoticeSent == null);
                            break;
                        case "Dbt": //For DA Processing beyond TAT
                            source = source.Where(x => x.DAEmailDateNoticeSentMaxDate <= dt && x.DeemedAcceptanceDate != null && x.DAEmailDateNoticeSent == null);
                            break;
                        default:
                            source = source.Where(x => projCodes.Contains(x.CompanyCode + "-" + x.ProjectCode));
                            break;
                    }

                    // for Searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        // Search by on selected column
                        switch (param.searchcol)
                        {
                            case "1": // Project Code and Name
                                source = source.Where(x => x.ProjectCode.ToLower().Contains(param.search) || x.BusinessEntity.ToLower().Contains(param.search));
                                break;
                            case "2": // Unit Category
                                source = source.Where(x => x.UnitCategoryDesc.ToLower().Contains(param.search));
                                break;
                            case "3": // Unit Type
                                source = source.Where(x => x.UnitTypeDesc.ToLower().Contains(param.search));
                                break;
                            case "4": // Unit Type
                                source = source.Where(x => x.RefNos.ToLower().Contains(param.search));
                                break;
                            case "5": // Customer Name
                                source = source.Where(x => x.CustomerNos.ToLower().Contains(param.search) || x.CustomerName1.ToLower().Contains(param.search));
                                break;
                            case "6": // Account Type
                                source = source.Where(x => x.AccountTypeDesc.ToLower().Contains(param.search));
                                break;
                            case "7": // Handover Associate
                                source = source.Where(x => x.HandoverAssociate != null && x.HandoverAssociate.ToLower().Contains(param.search));
                                break;
                            case "8": // Final turnover date
                                source = source.Where(x => x.FinalTurnoverDate != null && x.FinalTurnoverDate.GetValueOrDefault().ToString("MM/dd/yyyy") == param.search);
                                break;
                            case "9": // Final turnover option
                                source = source.Where(x => x.FinalTurnoverOption != null && x.FinalTurnoverOption.ToLower().Contains(param.search));
                                break;
                            case "10": // Turnover Status
                                source = source.Where(x => x.TurnoverStatus != null && x.TurnoverStatus.ToLower().Contains(param.search));
                                break;
                            case "11": // Punchlist Category
                                source = source.Where(x => x.PunchlistCategory != null && x.PunchlistCategory.ToLower().Contains(param.search));
                                break;
                        }
                    }

                    // paging
                    var sourcePaged = source.Skip((param.page - 1) * param.itemsPerPage).Take(param.itemsPerPage);

                    // Get the final list base on the define linq queryable parameter
                    var results = sourcePaged.Select(x => new
                    {
                        x.CompanyCode,
                        x.UnitNos,
                        x.ProjectCode,
                        x.RefNos,
                        x.BusinessEntity,
                        x.UnitCategoryCode,
                        x.UnitCategoryDesc,
                        x.UnitTypeDesc,
                        x.CustomerNos,
                        x.CustomerName1,
                        x.AccountTypeCode,
                        x.AccountTypeDesc,
                        x.EmailAdd,
                        x.ContactPerson,
                        x.TelNos,
                        x.QCDAcceptanceDate,
                        x.FPMCAcceptanceDate,
                        x.HasOccupancyPermit,
                        x.TOAS,
                        x.QualificationDate,
                        x.HandoverAssociate,
                        x.FinalTurnoverDate,
                        x.FinalTurnoverTime,
                        x.FinalTurnoverOption,
                        x.TurnoverStatus,
                        x.PunchlistCategory,
                        x.PunchlistItem,
                        x.OtherIssues,
                        x.RushTicketNos,
                        x.ReinspectionDate,
                        x.UnitAcceptanceDate,
                        x.KeyTransmittalDate,
                        x.DeemedAcceptanceDate,
                        x.DAEmailDateNoticeSent,
                        x.DACourierDateNoticeSent,
                        x.DACourierDateNoticeReceived,
                        x.DACourierReceivedBy,
                        x.TurnoverStatusTAT,
                        x.DAEmailDateNoticeSentMaxDate,
                        x.LastReinspectionDate,
                        x.LastReinspectionDateTAT,
                        x.TurnoverStatusTATNos,
                        x.PunchlistDateTATNos,
                        x.DeemedEmailDateSentTATNos,
                        x.TOStatusAging,
                        x.PunchlistAging,
                        x.DAEmailAging,
                        x.TranClass,
                        x.TurnoverStatusDate,
                        x.UnitAcceptanceDateTAT
                    }).ToList();

                    IEnumerable<CustomerDashboard_Status> tostatus = null;
                    tostatus = results.Select(x => new CustomerDashboard_Status
                    {
                        UniqueHashKey = StringCipher.Crypt(String.Concat(x.CompanyCode, "|", x.ProjectCode, "|", x.UnitCategoryCode, "|", x.UnitNos, "|", x.CustomerNos, "|", x.ProjectCode, " : ", x.BusinessEntity.Replace('|', ' '), "|", x.RefNos)),
                        CompanyCode = x.CompanyCode,
                        ProjectCode = x.ProjectCode,
                        RefNos = x.RefNos,
                        UnitNos = x.UnitNos,
                        BusinessEntity = x.BusinessEntity,
                        UnitCategoryDesc = x.UnitCategoryDesc,
                        UnitTypeDesc = x.UnitTypeDesc,
                        CustomerNos = x.CustomerNos,
                        CustomerName1 = x.CustomerName1,
                        AccountTypeCode = x.AccountTypeCode,
                        AccountTypeDesc = x.AccountTypeDesc,
                        EmailAdd = x.EmailAdd,
                        ContactPerson = x.ContactPerson,
                        TelNos = x.TelNos,
                        QCDAcceptanceDate = x.QCDAcceptanceDate,
                        FPMCAcceptanceDate = x.FPMCAcceptanceDate,
                        HasOccupancyPermit = x.HasOccupancyPermit,
                        TOAS = x.TOAS,
                        HandoverAssociate = x.HandoverAssociate,
                        FinalTurnoverDate = (x.FinalTurnoverDate == null) ? x.FinalTurnoverDate : x.FinalTurnoverDate.Value.Add(x.FinalTurnoverTime.Value),
                        FinalTurnoverOption = x.FinalTurnoverOption,
                        QualificationDate = x.QualificationDate,
                        TurnoverStatus = string.IsNullOrWhiteSpace(x.TurnoverStatus) ? "Null" : x.TurnoverStatus,
                        PunchlistCategory = string.IsNullOrWhiteSpace(x.PunchlistCategory) ? "Null" : x.PunchlistCategory,
                        PunchlistItem = string.IsNullOrWhiteSpace(x.PunchlistItem) ? "Null" : x.PunchlistItem,
                        OtherIssues = string.IsNullOrWhiteSpace(x.OtherIssues) ? "Null" : x.OtherIssues,
                        RushTicketNos = string.IsNullOrWhiteSpace(x.RushTicketNos) ? "Null" : x.RushTicketNos,
                        ReinspectionDate = (x.ReinspectionDate == null) ? "Null" : x.ReinspectionDate.GetValueOrDefault().ToString("MM/dd/yyyy"),
                        UnitAcceptanceDate = (x.UnitAcceptanceDate == null) ? "Null" : x.UnitAcceptanceDate.GetValueOrDefault().ToString("MM/dd/yyyy"),
                        KeyTransmittalDate = (x.KeyTransmittalDate == null) ? "Null" : x.KeyTransmittalDate.GetValueOrDefault().ToString("MM/dd/yyyy"),
                        DeemedAcceptanceDate = (x.DeemedAcceptanceDate == null) ? "Null" : x.DeemedAcceptanceDate.GetValueOrDefault().ToString("MM/dd/yyyy"),
                        DAEmailDateNoticeSent = (x.DAEmailDateNoticeSent == null) ? "Null" : x.DAEmailDateNoticeSent.GetValueOrDefault().ToString("MM/dd/yyyy"),
                        DACourierDateNoticeSent = (x.DACourierDateNoticeSent == null) ? "Null" : x.DACourierDateNoticeSent.GetValueOrDefault().ToString("MM/dd/yyyy"),
                        DACourierDateNoticeReceived = (x.DACourierDateNoticeReceived == null) ? "Null" : x.DACourierDateNoticeReceived.GetValueOrDefault().ToString("MM/dd/yyyy"),
                        DACourierReceivedBy = string.IsNullOrWhiteSpace(x.DACourierReceivedBy) ? "Null" : x.DACourierReceivedBy,
                        TurnoverStatusTATNos = Convert.ToInt16(x.TurnoverStatusTATNos),
                        PunchlistDateTATNos = Convert.ToInt16(x.PunchlistDateTATNos),
                        DeemedEmailDateSentTATNos = Convert.ToInt16(x.DeemedEmailDateSentTATNos),
                        TOStatusAging = Convert.ToInt16(x.TOStatusAging),
                        PunchlistAging = Convert.ToInt16(x.PunchlistAging),
                        DAEmailAging = Convert.ToInt16(x.DAEmailAging)
                    }).AsEnumerable();

                    // sorting
                    if (param.sortby != "default")
                    {
                        // sorting
                        var sortby = typeof(CustomerDashboard_Status).GetProperty(param.sortby);
                        switch (param.reverse)
                        {
                            case true:
                                tostatus = tostatus.OrderByDescending(s => sortby.GetValue(s, null));
                                break;
                            case false:
                                tostatus = tostatus.OrderBy(s => sortby.GetValue(s, null));
                                break;
                        }
                    }
                    else
                    {
                        tostatus = tostatus.OrderByDescending(s => s.TurnoverStatus).ThenByDescending(s => s.TOStatusAging).ThenByDescending(s => s.DAEmailAging).ThenByDescending(s => s.PunchlistAging);
                    }

                    var data = new { COUNT = source.Count(), QUALIFIEDTOSTATUS = tostatus,  CURUSER = user, PROJSELECTED = projects, CONTROLS = permissionCtrl, LASTDATESYNC = lastDateSync };

                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

    }
}