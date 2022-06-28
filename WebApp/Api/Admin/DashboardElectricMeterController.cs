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
    [RoutePrefix("api/DashboardElectricMeter")]
    public class DashboardElectricMeterController : ApiController
    {
        private string PageUrl = "/Admin/DashboardElectricMeter";
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
                    var projects = await db.VW_SalesInventory.Where(x => x.TSM == true && x.SalesDocStatus == "Active" && x.AccountTypeCode != "L" && x.UnitCategoryCode == "UN").Select(x => new { id = x.ProjectId, label = x.CompanyCode + " - " + x.ProjectCode + " : " + x.BusinessEntity }).Distinct().OrderBy(x => x.label).ToListAsync();

                    // Business Rule with SAP Cut-off Date based on System Parameter
                    //if (systemParameter.EnableTOCutOffDate == true)
                    //    projects = await db.VW_QualifiedUnit.Where(x => x.TOAS != null && x.TranClass == "Business Rule 1").Select(x => new { id = x.ProjectId, label = x.ProjectCode + " : " + x.BusinessEntity }).Distinct().OrderBy(x => x.label).ToListAsync();

                    var data = new { PROJECTLIST = projects};

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
                    // Dashboard Titling Status Summary
                    var DashboardSummary = await db.Database.SqlQuery<CustomDashboard_ElectricMeterStatusSummary>("EXEC spDashboardSummary {0}, {1}", "Electric", "").FirstOrDefaultAsync();

                    // GET the Last Date Sync in SAP
                    var lastDateSync = db.ProcessJobs.Where(x => x.StatusCode == "OK" && x.IsSuccessStatusCode == true && x.ObjectType == "ElectricMeterDashboard").OrderByDescending(x => x.CreatedDate).FirstOrDefault();
                    if(lastDateSync == null)
                        return BadRequest("Please click the 'Sync Data' button to retrieve the latest data.");

                    var data = new { DASHBOARDSUMMARY = DashboardSummary, LASTDATESYNC = lastDateSync.CreatedDate };

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

                    var projCodes = db.VW_Projects.Where(x => x.EMM == true && param.searchbyids.Contains(x.Id)).Select(x => x.CompanyCode + "-" + x.ProjectCode).ToArray();
                    if ((projCodes == null || projCodes.Count() == 0) && string.IsNullOrWhiteSpace(param.search2) && string.IsNullOrWhiteSpace(param.search3))
                        return BadRequest("Please first make a selection from Project Field");

                    // Dashboard Titling Status Summary
                    var DashboardSummary = await db.Database.SqlQuery<CustomDashboard_ElectricMeterStatusSummary>("EXEC spDashboardSummary {0}, {1}", "Electric", string.Join(",", projCodes)).FirstOrDefaultAsync();

                    var data = new { DASHBOARDSUMMARY2 = DashboardSummary };

                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("GetDashboardElectricMeter")]
        public async Task<IHttpActionResult> GetDashboardElectricMeter([FromUri] FilterModel param)
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

                    var projCodes = db.VW_Projects.Where(x => x.EMM == true && param.searchbyids.Contains(x.Id)).Select(x => x.CompanyCode + "-" + x.ProjectCode).ToArray();
                    if ((projCodes == null || projCodes.Count() == 0) && string.IsNullOrWhiteSpace(param.search2) && string.IsNullOrWhiteSpace(param.search3))
                        return BadRequest("Please first make a selection from Project Field");

                    var projects = db.VW_Projects.Where(x => x.EMM == true && param.searchbyids.Contains(x.Id)).Select(x => new { x.BusinessEntity }).Distinct().OrderBy(x => x.BusinessEntity).ToList();
                   
                    // Get List of Electric Meter Status
                    IQueryable<VW_DashboardProcessingJobElectric> source = db.VW_DashboardProcessingJobElectric.OrderByDescending(x => x.ApplicationProcessStatus).OrderBy(x => x.DocumentaryCompletedDate).OrderBy(x => x.ElectricMeterStatus);

                    // For the Condition Query
                    switch (param.search2)
                    {
                        case "T1": //For Documentation Completion Within TAT
                            source = source.Where(x => x.DocCompletionGroup == "Within" || x.DocCompletionGroup == "Beyond");
                            break;
                        case "T2": //For RFP Creation Within TAT
                            source = source.Where(x => x.RFPCreationGroup == "Within" || x.RFPCreationGroup == "Beyond");
                            break;
                        case "T3": //For Check Payment Release Within TAT
                            source = source.Where(x => x.CheckPaymentReleaseGroup == "Within" || x.CheckPaymentReleaseGroup == "Beyond");
                            break;
                        case "T4": //For Submission Of Requirements To Meralco Within TAT
                            source = source.Where(x => x.MeralcoSubmissionGroup == "Within" || x.MeralcoSubmissionGroup == "Beyond");
                            break;
                        case "T5": //For Transfer of Electric Meter & Service Deposit Within TAT
                            source = source.Where(x => x.TransferElectricServGroup == "Within" || x.TransferElectricServGroup == "Beyond");
                            break;

                        case "W1": //For Documentation Completion Within TAT
                            source = source.Where(x => x.DocCompletionGroup == "Within");
                            break;
                        case "W2": //For RFP Creation Within TAT
                            source = source.Where(x => x.RFPCreationGroup == "Within");
                            break;
                        case "W3": //For Check Payment Release Within TAT
                            source = source.Where(x => x.CheckPaymentReleaseGroup == "Within");
                            break;
                        case "W4": //For Submission Of Requirements To Meralco Within TAT
                            source = source.Where(x => x.MeralcoSubmissionGroup == "Within");
                            break;
                        case "W5": //For Transfer of Electric Meter & Service Deposit Within TAT
                            source = source.Where(x => x.TransferElectricServGroup == "Within");
                            break;

                        case "B1": //For Documentation Completion Beyond TAT
                            source = source.Where(x => x.DocCompletionGroup == "Beyond");
                            break;
                        case "B2": //For RFP Creation Beyond TAT
                            source = source.Where(x => x.RFPCreationGroup == "Beyond");
                            break;
                        case "B3": //For Check Payment Release Beyond TAT
                            source = source.Where(x => x.CheckPaymentReleaseGroup == "Beyond");
                            break;
                        case "B4": //For Submission Of Requirements To Meralco Beyond TAT
                            source = source.Where(x => x.MeralcoSubmissionGroup == "Beyond");
                            break;
                        case "B5": //For Transfer of Electric Meter & Service Deposit Beyond TAT
                            source = source.Where(x => x.TransferElectricServGroup == "Beyond");
                            break;
                        default:
                            source = source.Where(x => projCodes.Contains(x.CompanyCode + "-" + x.ProjectCode));
                            break;
                    }

                    // for Searching
                    if (!string.IsNullOrWhiteSpace(param.search) || (param.DateFrom.Year != 1 && param.DateTo.Year != 1))
                    {
                        string[] set1 = { "1", "2", "3", "4", "5", "6", "7" };
                        string[] set2 = { "8", "9", "10", "11", "12", "13", "14", "15", "16" };

                        if (!string.IsNullOrWhiteSpace(param.search) && set1.Contains(param.searchcol))
                        {
                            param.search = param.search.ToLower();
                            // Search by on selected column
                            switch (param.searchcol)
                            {
                                case "1": // Company
                                    source = source.Where(x => x.CompanyCode.ToLower().Contains(param.search) || x.CompanyName.ToLower().Contains(param.search));
                                    break;
                                case "2": // Project Code and Name
                                    source = source.Where(x => x.ProjectCode.ToLower().Contains(param.search) || x.BusinessEntity.ToLower().Contains(param.search));
                                    break;
                                case "3": // Unit Type
                                    source = source.Where(x => x.UnitTypeDesc.ToLower().Contains(param.search));
                                    break;
                                case "4": // Unit Nos
                                    source = source.Where(x => x.RefNos.ToLower().Contains(param.search));
                                    break;
                                case "5": // Customer Name
                                    source = source.Where(x => x.CustomerNos.ToLower().Contains(param.search) || x.CustomerName1.ToLower().Contains(param.search));
                                    break;
                                case "6": // Application Process Status
                                    source = source.Where(x => x.ApplicationProcessStatus.ToLower().Contains(param.search));
                                    break;
                                case "7": // Document status
                                    source = source.Where(x => x.ElectricMeterStatus.ToLower().Contains(param.search));
                                    break;
                                default:
                                    break;
                            }
                        }
                        else if ((param.DateFrom.Year != 1 && param.DateTo.Year != 1) && set2.Contains(param.searchcol))
                        {
                            // Search by on selected column
                            switch (param.searchcol)
                            {
                                case "8": // Documentary Completed Date
                                    source = source.Where(x => x.DocumentaryCompletedDate >= param.DateFrom.Date && x.DocumentaryCompletedDate <= param.DateTo.Date);
                                    break;
                                case "9": // RFP Rush Ticket Date
                                    source = source.Where(x => x.RFPRushTicketDate >= param.DateFrom.Date && x.RFPRushTicketDate <= param.DateTo.Date);
                                    break;
                                case "10": // Received Check Date
                                    source = source.Where(x => x.ReceivedCheckDate >= param.DateFrom.Date && x.ReceivedCheckDate <= param.DateTo.Date);
                                    break;
                                case "11": // Unpaid Bill Posted Date
                                    source = source.Where(x => x.UnpaidBillPostedDate >= param.DateFrom.Date && x.UnpaidBillPostedDate <= param.DateTo.Date);
                                    break;
                                case "12": // Paid Settled Posted Date
                                    source = source.Where(x => x.PaidSettledPostedDate >= param.DateFrom.Date && x.PaidSettledPostedDate <= param.DateTo.Date);
                                    break;
                                case "13": // Meralco Submitted Date
                                    source = source.Where(x => x.MeralcoSubmittedDate >= param.DateFrom.Date && x.MeralcoSubmittedDate <= param.DateTo.Date);
                                    break;
                                case "14": // Meralco Receipt Date
                                    source = source.Where(x => x.MeralcoReceiptDate >= param.DateFrom.Date && x.MeralcoReceiptDate <= param.DateTo.Date);
                                    break;
                                case "15": // Unit Owner Receipt Date
                                    source = source.Where(x => x.UnitOwnerReceiptDate >= param.DateFrom.Date && x.UnitOwnerReceiptDate <= param.DateTo.Date);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                    // paging
                    var sourcePaged = source.Skip((param.page - 1) * param.itemsPerPage).Take(param.itemsPerPage);
                    
                    // Get the final list base on the define linq queryable parameter
                    var results = sourcePaged.Select(x => new {
                        x.CompanyCode,
                        x.CompanyName,
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
                        x.TitleInProcessDate,
                        x.TitleTransferredDate,
                        x.TitleClaimedDate,
                        x.TaxDeclarationTransferredDate,
                        x.TaxDeclarationClaimedDate,
                        x.TOAS,
                        x.SAPTurnoverDate,
                        x.TurnoverDate,
                        x.QualificationDate,
                        x.LiquidationEndorsedDate,
                        x.TitleReleaseEndorsedDate,
                        x.BankReleasedDate,
                        x.BuyerReleasedDate,
                        x.MeterDepositAmount,
                        x.ApplicationProcessStatus,
                        x.DocumentaryCompletedDate,
                        x.DocumentaryLastModifedDate,
                        x.DocumentaryRemarks,
                        x.CCTReceivedDate,
                        x.DocCompletionTAT,
                        x.DocCompletionSysTAT,
                        x.DocCompletionGroup,
                        x.RFPRushTicketDate,
                        x.RFPCreationTAT,
                        x.RFPCreationSysTAT,
                        x.RFPCreationGroup,
                        x.RFPRushTicketNos,
                        x.IsReceivedCheck,
                        x.ReceivedCheckDate,
                        x.ReceivedCheckRemarks,
                        x.CheckPaymentReleaseTAT,
                        x.CheckPaymentReleaseSysTAT,
                        x.CheckPaymentReleaseGroup,
                        x.RFPRushTicketRemarks,
                        x.WithUnpaidBills,
                        x.UnpaidBillPostedDate,
                        x.IsPaidSettled,
                        x.PaidSettledPostedDate,
                        x.DepositApplicationRemarks,
                        x.MeralcoSubmittedDate,
                        x.MeralcoSubmissionTAT,
                        x.MeralcoSubmissionSysTAT,
                        x.MeralcoSubmissionGroup,
                        x.MeralcoSubmittedRemarks,
                        x.MeralcoReceiptDate,
                        x.TransferElectricServTAT,
                        x.TransferElectricServSysTAT,
                        x.TransferElectricServGroup,
                        x.MeralcoReceiptRemarks,
                        x.UnitOwnerReceiptDate,
                        x.UnitOwnerReceiptRemarks,
                        x.AgingRFPRushToCheckRelease,
                        x.AgingMeralcoSubmittedToReceipt,
                        x.ElectricMeterStatus,
                        x.EMCutOffDate,
                        x.TranClass
                    }).ToList();

                    IEnumerable<CustomerDashboard_ElecticMeterStatus> electric = null;
                    electric = results.Select(x => new CustomerDashboard_ElecticMeterStatus
                    {
                        UniqueHashKey = StringCipher.Crypt(String.Concat(x.CompanyCode, "|", x.ProjectCode, "|", x.UnitCategoryCode, "|", x.UnitNos, "|", x.CustomerNos, "|", x.ProjectCode, " : ", x.BusinessEntity.Replace('|', ' '), "|", x.RefNos)),
                        CompanyCode = x.CompanyCode,
                        CompanyName = x.CompanyName,
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
                        TitleInProcessDate = x.TitleInProcessDate,
                        TitleTransferredDate = x.TitleTransferredDate,
                        TitleClaimedDate = x.TitleClaimedDate,
                        TaxDeclarationTransferredDate = x.TaxDeclarationTransferredDate,
                        TaxDeclarationClaimedDate = x.TaxDeclarationClaimedDate,
                        TOAS = x.TOAS,
                        TurnoverDate = x.TurnoverDate,
                        SAPTurnoverDate = x.SAPTurnoverDate,
                        QualificationDate = x.QualificationDate,
                        LiquidationEndorsedDate = x.LiquidationEndorsedDate,
                        TitleReleaseEndorsedDate = x.TitleReleaseEndorsedDate,
                        BankReleasedDate = x.BankReleasedDate,
                        BuyerReleasedDate = x.BuyerReleasedDate,
                        MeterDepositAmount = x.MeterDepositAmount,
                        ApplicationProcessStatus = x.ApplicationProcessStatus,
                        DocumentaryCompletedDate = x.DocumentaryCompletedDate,
                        DocumentaryLastModifedDate = x.DocumentaryLastModifedDate,
                        DocumentaryRemarks = x.DocumentaryRemarks,
                        CCTReceivedDate = x.CCTReceivedDate,
                        DocCompletionTAT = x.DocCompletionTAT,
                        DocCompletionSysTAT = x.DocCompletionSysTAT,
                        DocCompletionGroup = x.DocCompletionGroup,
                        RFPRushTicketDate = x.RFPRushTicketDate,
                        RFPCreationTAT = x.RFPCreationTAT,
                        RFPCreationSysTAT = x.RFPCreationSysTAT,
                        RFPCreationGroup = x.RFPCreationGroup,
                        RFPRushTicketNos = x.RFPRushTicketNos,
                        IsReceivedCheck = x.IsReceivedCheck,
                        ReceivedCheckDate = x.ReceivedCheckDate,
                        ReceivedCheckRemarks = x.ReceivedCheckRemarks,
                        CheckPaymentReleaseTAT = x.CheckPaymentReleaseTAT,
                        CheckPaymentReleaseSysTAT = x.CheckPaymentReleaseSysTAT,
                        CheckPaymentReleaseGroup = x.CheckPaymentReleaseGroup,
                        RFPRushTicketRemarks = x.RFPRushTicketRemarks,
                        WithUnpaidBills = x.WithUnpaidBills,
                        UnpaidBillPostedDate = x.UnpaidBillPostedDate,
                        IsPaidSettled = x.IsPaidSettled,
                        PaidSettledPostedDate = x.PaidSettledPostedDate,
                        DepositApplicationRemarks = x.DepositApplicationRemarks,
                        MeralcoSubmittedDate = x.MeralcoSubmittedDate,
                        MeralcoSubmissionTAT = x.MeralcoSubmissionTAT,
                        MeralcoSubmissionSysTAT = x.MeralcoSubmissionSysTAT,
                        MeralcoSubmissionGroup = x.MeralcoSubmissionGroup,
                        MeralcoSubmittedRemarks = x.MeralcoSubmittedRemarks,
                        MeralcoReceiptDate = x.MeralcoReceiptDate,
                        TransferElectricServTAT = x.TransferElectricServTAT,
                        TransferElectricServSysTAT = x.TransferElectricServSysTAT,
                        TransferElectricServGroup = x.TransferElectricServGroup,
                        MeralcoReceiptRemarks = x.MeralcoReceiptRemarks,
                        UnitOwnerReceiptDate = x.UnitOwnerReceiptDate,
                        UnitOwnerReceiptRemarks = x.UnitOwnerReceiptRemarks,
                        ElectricMeterStatus = x.ElectricMeterStatus,
                        AgingMeralcoSubmittedToReceipt = x.AgingMeralcoSubmittedToReceipt,
                        AgingRFPRushToCheckRelease = x.AgingRFPRushToCheckRelease
                    }).AsEnumerable();

                    // For Sorting
                    if (param.sortby != "default")
                    {
                        var sortby = typeof(CustomerDashboard_ElecticMeterStatus).GetProperty(param.sortby);
                        switch (param.reverse)
                        {
                            case true:
                                electric = electric.OrderByDescending(s => sortby.GetValue(s, null));
                                break;
                            case false:
                                electric = electric.OrderBy(s => sortby.GetValue(s, null));
                                break;
                        }
                    }
                    else
                    {
                        electric = electric.OrderByDescending(s => s.ElectricMeterStatus).ThenBy(s => s.DocumentaryCompletedDate).ThenBy(s => s.ApplicationProcessStatus);
                    }

                    var data = new { COUNT = source.Count(), CURUSER = user, ELECTICMETERSTATUS = electric,  PROJSELECTED = projects, CONTROLS = permissionCtrl/*, LASTDATESYNC = lastDateSync*/ };

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