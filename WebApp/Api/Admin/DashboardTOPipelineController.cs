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
using System.Linq.Expressions;

namespace WebApp.Api.Admin
{
    [Authorize]
    [RoutePrefix("api/DashboardTOPipeline")]
    public class DashboardTOPipelineController : ApiController
    {
        private string PageUrl = "/Admin/DashboardTOPipeline";
        //private string ApiName = "Dashboard Turnover Pipeline";

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
                    
                    // Get List of Active Projects
                    var projects = await db.VW_QualifiedUnit.Where(x => x.TOAS != null).Select(x => new { id = x.ProjectId, label = x.CompanyCode + " - " + x.ProjectCode + " : " + x.BusinessEntity }).Distinct().OrderBy(x => x.label).ToListAsync();
                    // Business Rule with SAP Cut-off Date based on System Parameter
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
                    // Dashboard Turnover Pipeline Summary
                    var DashboardSummary = await db.Database.SqlQuery<CustomDashboard_PipelineSummary>("EXEC spDashboardSummary {0}, {1}", "Pipeline", "").FirstOrDefaultAsync();

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

                    // Dashboard Turnover Pipeline Summary
                    var DashboardSummary = await db.Database.SqlQuery<CustomDashboard_PipelineSummary>("EXEC spDashboardSummary {0}, {1}", "Pipeline", string.Join(",", projCodes)).FirstOrDefaultAsync();

                    var data = new { DASHBOARDSUMMARY2 = DashboardSummary };

                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("GetDashboardTOPipeline")]
        public async Task<IHttpActionResult> GetDashboardTOPipeline([FromUri] FilterModel param)
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
                    IQueryable<VW_QualifiedTurnover> source = db.VW_QualifiedTurnover.Where(x => x.TOAS != null).OrderByDescending(x => x.EmailNoticeSentAging).OrderBy(x => x.Phase).OrderBy(x => x.RefNos).OrderBy(x => x.EmailDateNoticeSent).OrderBy(x => x.FinalTurnoverDate);

                    // Business Rule with SAP Cut-off Date based on System Parameter
                    if (systemParameter.EnableTOCutOffDate == true)
                        source = source.Where(x => x.TranClass == "Business Rule 1");

                    // For the Condition Query
                    switch (param.search2)
                    {
                        case "Pwt": //Processing within TAT
                            source = source.Where(x => x.EmailNoticeNotifDate2 > dt && x.EmailDateNoticeSent == null);
                            break;
                        case "Pbt": //Processing beyond TAT
                            source = source.Where(x => x.EmailNoticeNotifDate2 <= dt && x.EmailDateNoticeSent == null);
                            break;
                        case "Ptl": //Qualified within TAT (Pwt + Pbt)
                            source = source.Where(x => (x.EmailNoticeNotifDate2 > dt && x.EmailDateNoticeSent == null) || (x.EmailNoticeNotifDate2 <= dt && x.EmailDateNoticeSent == null));
                            break;

                        case "Cwt": //Confirmed within TAT
                            source = source.Where(x => x.ScheduleEmailNotifDate1 > dt && x.EmailDateNoticeSent != null && x.TurnoverDate1 == null); // && x.TurnoverDate1 != null && x.TurnoverDate1 >= dt
                            break;
                        case "Cbt": //Confirmed beyond TAT
                            source = source.Where(x => x.ScheduleEmailNotifDate1 <= dt && x.EmailDateNoticeSent != null && x.TurnoverDate1 == null);
                            break;
                        case "Ctl": //Qualified beyond TAT (Cwt + Cbt)
                            source = source.Where(x => (x.ScheduleEmailNotifDate1 > dt && x.EmailDateNoticeSent != null && x.TurnoverDate1 == null) || (x.ScheduleEmailNotifDate1 <= dt && x.EmailDateNoticeSent != null && x.TurnoverDate1 == null));
                            break;

                        case "Qwt": //Qualified within TAT (Pwt + Cwt)
                            source = source.Where(x => (x.EmailNoticeNotifDate2 > dt && x.EmailDateNoticeSent == null) || (x.ScheduleEmailNotifDate1 > dt && x.TurnoverDate1 != null && x.TurnoverDate1 >= dt));
                            break;
                        case "Qbt": //Qualified beyond TAT (Pbt + Cbt)
                            source = source.Where(x => (x.EmailNoticeNotifDate2 <= dt && x.EmailDateNoticeSent == null) || (x.ScheduleEmailNotifDate1 <= dt && x.TurnoverDate1 != null && x.TurnoverDate1 >= dt));
                            break;
                        case "Qtl": //Qualified within TAT (Qwt + Qbt)
                            source = source.Where(x => ((x.EmailNoticeNotifDate2 > dt && x.EmailDateNoticeSent == null) || (x.ScheduleEmailNotifDate1 > dt && x.TurnoverDate1 != null && x.TurnoverDate1 >= dt)) || ((x.EmailNoticeNotifDate2 < dt && x.EmailDateNoticeSent == null) || (x.ScheduleEmailNotifDate1 < dt && x.TurnoverDate1 != null && x.TurnoverDate1 >= dt)));
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
                            case "7": // QCD Date
                                source = source.Where(x => (x.QCDAcceptanceDate != null && x.QCDAcceptanceDate.ToString() == param.search) || (x.FPMCAcceptanceDate != null && x.FPMCAcceptanceDate.ToString() == param.search));
                                break;
                            case "8": // TOAS Date
                                source = source.Where(x => x.TOAS != null && x.TOAS.GetValueOrDefault().ToString("MM/dd/yyyy") == param.search);
                                break;
                            case "9": // Handover Associate
                                source = source.Where(x => x.HandoverAssociate != null && x.HandoverAssociate.ToLower().Contains(param.search));
                                break;
                            case "10": // Email Notice Date Sent
                                source = source.Where(x => x.EmailDateNoticeSent != null && x.EmailDateNoticeSent.ToString() == param.search);
                                break;
                            case "11": // Email Notice Sent Aging
                                source = source.Where(x => x.EmailNoticeSentAging.ToString() == param.search);
                                break;
                            case "12": // Final turnover date
                                source = source.Where(x => x.FinalTurnoverDate != null && x.FinalTurnoverDate.GetValueOrDefault().ToString("MM/dd/yyyy") == param.search);
                                break;
                            case "13": // Final turnover option
                                source = source.Where(x => x.FinalTurnoverOption != null && x.FinalTurnoverOption.ToLower().Contains(param.search));
                                break;
                            default:
                                source = source.Where(x => x.ProjectCode.ToLower().Contains(param.search) || x.BusinessEntity.ToLower().Contains(param.search) || x.UnitCategoryDesc.ToLower().Contains(param.search) || x.UnitTypeDesc.ToLower().Contains(param.search) ||
                                                    x.RefNos.ToLower().Contains(param.search) || x.CustomerNos.ToLower().Contains(param.search) || x.CustomerName1.ToLower().Contains(param.search) || x.AccountTypeDesc.ToLower().Contains(param.search) ||
                                                    (x.HandoverAssociate != null && x.HandoverAssociate.ToLower().Contains(param.search)) || (x.FinalTurnoverOption != null && x.FinalTurnoverOption.ToLower().Contains(param.search)) ||
                                                    (x.QCDAcceptanceDate != null && x.QCDAcceptanceDate.ToString() == param.search) || (x.FPMCAcceptanceDate != null && x.FPMCAcceptanceDate.ToString() == param.search) ||
                                                    (x.TOAS != null && x.TOAS.GetValueOrDefault().ToString("MM/dd/yyyy") == param.search) || (x.EmailDateNoticeSent != null && x.EmailDateNoticeSent.ToString() == param.search) ||
                                                    (x.FinalTurnoverDate != null && x.FinalTurnoverDate.GetValueOrDefault().ToString("MM/dd/yyyy") == param.search));
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
                        x.Phase,
                        x.BusinessEntity,
                        x.UnitCategoryCode,
                        x.UnitCategoryDesc,
                        x.UnitTypeDesc,
                        x.CustomerNos,
                        x.CustomerName1,
                        x.AccountTypeDesc,
                        x.EmailAdd,
                        x.ContactPerson,
                        x.TelNos,
                        x.QCDAcceptanceDate,
                        x.FPMCAcceptanceDate,
                        x.HasOccupancyPermit,
                        x.TOAS,
                        x.HandoverAssociate,
                        x.EmailNoticeSentAging,
                        x.EmailNoticeSentAgingDays,
                        x.EmailDateNoticeSent,
                        x.CourierDateNoticeSent,
                        x.CourierDateNoticeReceived,
                        x.CourierReceivedBy,
                        x.EmailTurnoverDate,
                        x.FinalTurnoverDate,
                        x.FinalTurnoverTime,
                        x.FinalTurnoverOption,
                        x.QualificationDate,
                        x.ScheduleEmailNotifDate1,
                        x.EmailNoticeNotifDate2,
                        x.TurnoverDate1,
                        x.TranClass
                    }).ToList();

                    IEnumerable<CustomerDashboard_Pipeline> pipeline = null;
                    pipeline = results.Select(x => new CustomerDashboard_Pipeline
                            {
                                UniqueHashKey = StringCipher.Crypt(String.Concat(x.CompanyCode, "|", x.ProjectCode, "|", x.UnitCategoryCode, "|", x.UnitNos, "|", x.CustomerNos, "|", x.ProjectCode, " : ", x.BusinessEntity.Replace('|', ' '), "|", x.RefNos)),
                                CompanyCode = x.CompanyCode,
                                ProjectCode = x.ProjectCode,
                                RefNos = x.RefNos,
                                Phase = x.Phase,
                                UnitNos = x.UnitNos,
                                BusinessEntity = x.BusinessEntity,
                                UnitCategoryDesc = x.UnitCategoryDesc,
                                UnitTypeDesc = x.UnitTypeDesc,
                                CustomerNos = x.CustomerNos,
                                CustomerName1 = x.CustomerName1,
                                AccountTypeDesc = x.AccountTypeDesc,
                                EmailAdd = x.EmailAdd,
                                ContactPerson = x.ContactPerson,
                                TelNos = x.TelNos,
                                QCDAcceptanceDate = x.QCDAcceptanceDate,
                                FPMCAcceptanceDate = x.FPMCAcceptanceDate,
                                HasOccupancyPermit = x.HasOccupancyPermit,
                                TOAS = x.TOAS,
                                HandoverAssociate = x.HandoverAssociate,
                                EmailNoticeSentAging = Convert.ToInt16(x.EmailNoticeSentAging),
                                EmailNoticeSentAgingDays = Convert.ToInt16(x.EmailNoticeSentAgingDays),
                                EmailDateNoticeSent = string.IsNullOrWhiteSpace(x.EmailDateNoticeSent) ? "Null" : x.EmailDateNoticeSent,
                                CourierDateNoticeSent = string.IsNullOrWhiteSpace(x.CourierDateNoticeSent) ? "Null" : x.CourierDateNoticeSent,
                                CourierDateNoticeReceived = string.IsNullOrWhiteSpace(x.CourierDateNoticeReceived) ? "Null" : x.CourierDateNoticeReceived,
                                CourierReceivedBy = string.IsNullOrWhiteSpace(x.CourierReceivedBy) ? "Null" : x.CourierReceivedBy,
                                EmailTurnoverDate = (x.EmailTurnoverDate == null) ? "Null" : x.EmailTurnoverDate.GetValueOrDefault().ToString("MM/dd/yyyy"),
                                EmailTurnoverTime = (x.EmailTurnoverDate == null) ? "Null" : x.EmailTurnoverDate.GetValueOrDefault().ToString("hh:mm tt"),
                                FinalTurnoverDate = (x.FinalTurnoverDate == null) ? "Null" : x.FinalTurnoverDate.GetValueOrDefault().ToString("MM/dd/yyyy"),
                                FinalTurnoverTime = (x.FinalTurnoverDate == null) ? "Null" : x.FinalTurnoverDate.Value.Add(x.FinalTurnoverTime.Value).ToString("hh:mm tt"),
                                FinalTurnoverOption = string.IsNullOrWhiteSpace(x.FinalTurnoverOption) ? "Null" : x.FinalTurnoverOption,
                                QualificationDate = x.QualificationDate
                            }).AsEnumerable();

                    // For Sorting
                    if (param.sortby != "default")
                    {
                        var sortby = typeof(CustomerDashboard_Pipeline).GetProperty(param.sortby);
                        switch (param.reverse)
                        {
                            case true:
                                pipeline = pipeline.OrderByDescending(s => sortby.GetValue(s, null));
                                break;
                            case false:
                                pipeline = pipeline.OrderBy(s => sortby.GetValue(s, null));
                                break;
                        }
                    }
                    else
                    {
                        pipeline = pipeline.OrderByDescending(s => s.EmailNoticeSentAging).ThenBy(s => s.Phase).ThenBy(s => s.RefNos).ThenBy(s => s.EmailDateNoticeSent).ThenBy(s => s.FinalTurnoverDate);
                    }

                    var data = new { COUNT = source.Count(), QUALIFIEDTOPIPELINE = pipeline, CURUSER = user, PROJSELECTED = projects, CONTROLS = permissionCtrl, LASTDATESYNC = lastDateSync };

                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

    }

    public partial class DashData
    {
        public static Expression<Func<DashData, bool>> Filtering()
        {
            return p => true;
        }
    }
}