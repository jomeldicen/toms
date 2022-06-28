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
    [RoutePrefix("api/DashboardTitleStatus")]
    public class DashboardTitleStatusController : ApiController
    {
        private string PageUrl = "/Admin/DashboardTitleStatus";
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
                    var projects = await db.VW_SalesInventory.Where(x => x.TSM == true && x.SalesDocStatus == "Active").Select(x => new { id = x.ProjectId, label = x.CompanyCode + " - " + x.ProjectCode + " : " + x.BusinessEntity }).Distinct().OrderBy(x => x.label).ToListAsync();

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
                    var DashboardSummary = await db.Database.SqlQuery<CustomDashboard_TitlingStatusSummary>("EXEC spDashboardSummary {0}, {1}", "Titling", "").FirstOrDefaultAsync();

                    // GET the Last Date Sync in SAP
                    var lastDateSync = db.ProcessJobs.Where(x => x.StatusCode == "OK" && x.IsSuccessStatusCode == true && x.ObjectType == "TitlingStatusDashboard").OrderByDescending(x => x.CreatedDate).FirstOrDefault();
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

                    var projCodes = db.VW_Projects.Where(x => x.TSM == true && param.searchbyids.Contains(x.Id)).Select(x => x.CompanyCode + "-" + x.ProjectCode).ToArray();
                    if ((projCodes == null || projCodes.Count() == 0) && string.IsNullOrWhiteSpace(param.search2) && string.IsNullOrWhiteSpace(param.search3))
                        return BadRequest("Please first make a selection from Project Field");

                    // Dashboard Titling Status Summary
                    var DashboardSummary = await db.Database.SqlQuery<CustomDashboard_TitlingStatusSummary>("EXEC spDashboardSummary {0}, {1}", "Titling", string.Join(",", projCodes)).FirstOrDefaultAsync();

                    var data = new { DASHBOARDSUMMARY2 = DashboardSummary };

                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("GetDashboardTitleStatus")]
        public async Task<IHttpActionResult> GetDashboardTitleStatus([FromUri] FilterModel param)
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

                    var projCodes = db.VW_Projects.Where(x => x.TSM == true && param.searchbyids.Contains(x.Id)).Select(x => x.CompanyCode + "-" + x.ProjectCode).ToArray();
                    if ((projCodes == null || projCodes.Count() == 0) && string.IsNullOrWhiteSpace(param.search2) && string.IsNullOrWhiteSpace(param.search3))
                        return BadRequest("Please first make a selection from Project Field");

                    var projects = db.VW_Projects.Where(x => x.TSM == true && param.searchbyids.Contains(x.Id)).Select(x => new { x.BusinessEntity }).Distinct().OrderBy(x => x.BusinessEntity).ToList();
                   
                    // Get List of Titling Status
                    IQueryable<VW_DashboardProcessingJob> source = db.VW_DashboardProcessingJob.OrderByDescending(x => x.TitleStatus).OrderBy(x => x.BusinessEntity).OrderBy(x => x.RefNos);

                    // For the Condition Query
                    switch (param.search2)
                    {
                        case "T1": //Title In Process Within TAT
                            source = source.Where(x => x.TOAS == null);
                            break;
                        case "T2": //Title In Process Within TAT
                            source = source.Where(x => x.TitleInProcessGroup == "Within" || x.TitleInProcessGroup == "Beyond");
                            break;
                        case "T3": //Title Transferred Within TAT
                            source = source.Where(x => x.TitleTransferredGroup == "Within" || x.TitleTransferredGroup == "Beyond");
                            break;
                        case "T4": //Title Claimed Within TAT
                            source = source.Where(x => x.TitleClaimedGroup == "Within" || x.TitleClaimedGroup == "Beyond");
                            break;
                        case "T5": //Tax Dec Transferred Within TAT
                            source = source.Where(x => x.TaxDeclarationTransferredGroup == "Within" || x.TaxDeclarationTransferredGroup == "Beyond");
                            break;
                        case "T6": //Tax Dec Claimed Within TAT
                            source = source.Where(x => x.TaxDeclarationClaimedGroup == "Within" || x.TaxDeclarationClaimedGroup == "Beyond");
                            break;
                        case "T7": //Endorsed Liquidation Within TAT
                            source = source.Where(x => x.LiquidationEndorsedGroup == "Within" || x.LiquidationEndorsedGroup == "Beyond");
                            break;
                        case "T8": //Endorsed Title Released Within TAT
                            source = source.Where(x => x.TitleReleaseEndorsedGroup == "Within" || x.TitleReleaseEndorsedGroup == "Beyond");
                            break;
                        case "T9": //Bank Released Within TAT
                            source = source.Where(x => x.BankReleasedGroup == "Within" || x.BankReleasedGroup == "Beyond");
                            break;
                        case "T10": //Buyer Released Within TAT
                            source = source.Where(x => x.BuyerReleasedGroup == "Within" || x.BuyerReleasedGroup == "Beyond");
                            break;

                        case "W2": //Title In Process Within TAT
                            source = source.Where(x => x.TitleInProcessGroup == "Within");
                            break;
                        case "W3": //Title Transferred Within TAT
                            source = source.Where(x => x.TitleTransferredGroup == "Within");
                            break;
                        case "W4": //Title Claimed Within TAT
                            source = source.Where(x => x.TitleClaimedGroup == "Within");
                            break;
                        case "W5": //Tax Dec Transferred Within TAT
                            source = source.Where(x => x.TaxDeclarationTransferredGroup == "Within");
                            break;
                        case "W6": //Tax Dec Claimed Within TAT
                            source = source.Where(x => x.TaxDeclarationClaimedGroup == "Within");
                            break;
                        case "W7": //Endorsed Liquidation Within TAT
                            source = source.Where(x => x.LiquidationEndorsedGroup == "Within");
                            break;
                        case "W8": //Endorsed Title Released Within TAT
                            source = source.Where(x => x.TitleReleaseEndorsedGroup == "Within");
                            break;
                        case "W9": //Bank Released Within TAT
                            source = source.Where(x => x.BankReleasedGroup == "Within");
                            break;
                        case "W10": //Buyer Released Within TAT
                            source = source.Where(x => x.BuyerReleasedGroup == "Within");
                            break;

                        case "B2": //Title In Process Beyond TAT
                            source = source.Where(x => x.TitleInProcessGroup == "Beyond");
                            break;
                        case "B3": //Title Transferred Beyond TAT
                            source = source.Where(x => x.TitleTransferredGroup == "Beyond");
                            break;
                        case "B4": //Title Claimed Beyond TAT
                            source = source.Where(x => x.TitleClaimedGroup == "Beyond");
                            break;
                        case "B5": //Tax Dec Transferred Beyond TAT
                            source = source.Where(x => x.TaxDeclarationTransferredGroup == "Beyond");
                            break;
                        case "B6": //Tax Dec Claimed Beyond TAT
                            source = source.Where(x => x.TaxDeclarationClaimedGroup == "Beyond");
                            break;
                        case "B7": //Endorsed Liquidation Beyond TAT
                            source = source.Where(x => x.LiquidationEndorsedGroup == "Beyond");
                            break;
                        case "B8": //Endorsed Title Released Beyond TAT
                            source = source.Where(x => x.TitleReleaseEndorsedGroup == "Beyond");
                            break;
                        case "B9": //Bank Released Beyond TAT
                            source = source.Where(x => x.BankReleasedGroup == "Beyond");
                            break;
                        case "B10": //Buyer Released Beyond TAT
                            source = source.Where(x => x.BuyerReleasedGroup == "Beyond");
                            break;
                        default:
                            source = source.Where(x => projCodes.Contains(x.CompanyCode + "-" + x.ProjectCode));
                            break;
                    }

                    // for Searching
                    if (!string.IsNullOrWhiteSpace(param.search) || (param.DateFrom.Year != 1 && param.DateTo.Year != 1))
                    {
                        string[] set1 = { "1", "2", "3", "4", "5", "6" };
                        string[] set2 = { "7", "8", "9", "10", "11", "12", "13", "14", "15", "16" };

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
                                case "6": // Title Status
                                    source = source.Where(x => x.TitleStatus.ToLower().Contains(param.search));
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
                                case "7": // TOAS Date
                                    source = source.Where(x => x.TOAS >= param.DateFrom.Date && x.TOAS <= param.DateTo.Date);
                                    break;
                                case "8": // Title In Process Date
                                    source = source.Where(x => x.TitleInProcessDate >= param.DateFrom.Date && x.TitleInProcessDate <= param.DateTo.Date);
                                    break;
                                case "9": // Title Transferred Date
                                    source = source.Where(x => x.TitleTransferredDate >= param.DateFrom.Date && x.TitleTransferredDate <= param.DateTo.Date);
                                    break;
                                case "10": // Title Claimed Date
                                    source = source.Where(x => x.TitleClaimedDate >= param.DateFrom.Date && x.TitleClaimedDate <= param.DateTo.Date);
                                    break;
                                case "11": // Tax Declaration Transferred Date
                                    source = source.Where(x => x.TaxDeclarationTransferredDate >= param.DateFrom.Date && x.TaxDeclarationTransferredDate <= param.DateTo.Date);
                                    break;
                                case "12": // Tax Declaration Claimed Date
                                    source = source.Where(x => x.TaxDeclarationClaimedDate >= param.DateFrom.Date && x.TaxDeclarationClaimedDate <= param.DateTo.Date);
                                    break;
                                case "13": // Liquidation Endorsement
                                    source = source.Where(x => x.LiquidationEndorsedDate >= param.DateFrom.Date && x.LiquidationEndorsedDate <= param.DateTo.Date);
                                    break;
                                case "14": // Title Release Endorsement
                                    source = source.Where(x => x.TitleReleaseEndorsedDate >= param.DateFrom.Date && x.TitleReleaseEndorsedDate <= param.DateTo.Date);
                                    break;
                                case "15": // Title Release to Buyer
                                    source = source.Where(x => x.BuyerReleasedDate >= param.DateFrom.Date && x.BuyerReleasedDate <= param.DateTo.Date);
                                    break;
                                case "16": // Title Release to Bank
                                    source = source.Where(x => x.BankReleasedDate >= param.DateFrom.Date && x.BankReleasedDate <= param.DateTo.Date);
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
                        x.TitleInProcessGroup,
                        x.TitleInProcessTAT,
                        x.TitleInProcessSysTAT,
                        x.TitleInProcessRemarks,
                        x.TitleTransferredDate,
                        x.TitleTransferredGroup,
                        x.TitleTransferredTAT,
                        x.TitleTransferredSysTAT,
                        x.TitleTransferredRemarks,
                        x.TitleClaimedDate,
                        x.TitleClaimedGroup,
                        x.TitleClaimedTAT,
                        x.TitleClaimedSysTAT,
                        x.TitleClaimedRemarks,
                        x.TaxDeclarationTransferredDate,
                        x.TaxDeclarationTransferredGroup,
                        x.TaxDeclarationTransferredTAT,
                        x.TaxDeclarationTransferredSysTAT,
                        x.TaxDeclarationTransferredRemarks,
                        x.TaxDeclarationClaimedDate,
                        x.TaxDeclarationClaimedGroup,
                        x.TaxDeclarationClaimedTAT,
                        x.TaxDeclarationClaimedSysTAT,
                        x.TaxDeclarationClaimedRemarks,
                        x.TOAS,
                        x.SAPTurnoverDate,
                        x.TurnoverDate,
                        x.QualificationDate,
                        x.TaxDecNos,
                        x.LiquidationEndorsedDate,
                        x.LiquidationEndorsedGroup,
                        x.LiquidationEndorsedTAT,
                        x.LiquidationEndorsedSysTAT,
                        x.LiquidationRushTicketNos,
                        x.LiquidationEndorsedRemarks,
                        x.TitleReleaseEndorsedDate,
                        x.TitleReleaseEndorsedGroup,
                        x.TitleReleaseEndorsedTAT,
                        x.TitleReleaseEndorsedSysTAT,
                        x.TitleReleaseRushTicketNos,
                        x.TitleReleaseEndorsedRemarks,
                        x.TitleLocationName,
                        x.TitleNos,
                        x.TitleRemarks,
                        x.BankReleasedDate,
                        x.BankReleasedGroup,
                        x.BankReleasedTAT,
                        x.BankReleasedSysTAT,
                        x.BankReleasedRemarks,
                        x.BuyerReleasedDate,
                        x.BuyerReleasedGroup,
                        x.BuyerReleasedTAT,
                        x.BuyerReleasedSysTAT,
                        x.BuyerReleasedRemarks,
                        x.TranClass,
                        x.TitleStatus,
                        x.MeralcoReceiptDate,
                        x.MeralcoSubmittedDate,
                        x.UnitOwnerReceiptDate,
                        x.AgingTOASTaxDecTransfer,
                        x.AgingQualifTaxDecTransfer,
                        x.AgingTTransferReleaseBank,
                        x.AgingTTransferReleaseBuyer
                    }).ToList();

                    IEnumerable<CustomerDashboard_TitlingStatus> titling = null;
                    titling = results.Select(x => new CustomerDashboard_TitlingStatus
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
                        TitleInProcessGroup = x.TitleInProcessGroup,
                        TitleInProcessTAT = x.TitleInProcessTAT,
                        TitleInProcessSysTAT = x.TitleInProcessSysTAT,
                        TitleInProcessRemarks = x.TitleInProcessRemarks,
                        TitleTransferredDate = x.TitleTransferredDate,
                        TitleTransferredGroup = x.TitleTransferredGroup,
                        TitleTransferredTAT = x.TitleTransferredTAT,
                        TitleTransferredSysTAT = x.TitleTransferredSysTAT,
                        TitleTransferredRemarks = x.TitleTransferredRemarks,
                        TitleClaimedDate = x.TitleClaimedDate,
                        TitleClaimedGroup = x.TitleClaimedGroup,
                        TitleClaimedTAT = x.TitleClaimedTAT,
                        TitleClaimedSysTAT = x.TitleClaimedSysTAT,
                        TitleClaimedRemarks = x.TitleClaimedRemarks,
                        TaxDeclarationTransferredDate = x.TaxDeclarationTransferredDate,
                        TaxDeclarationTransferredGroup = x.TaxDeclarationTransferredGroup,
                        TaxDeclarationTransferredTAT = x.TaxDeclarationTransferredTAT,
                        TaxDeclarationTransferredSysTAT = x.TaxDeclarationTransferredSysTAT,
                        TaxDeclarationTransferredRemarks = x.TaxDeclarationTransferredRemarks,
                        TaxDeclarationClaimedDate = x.TaxDeclarationClaimedDate,
                        TaxDeclarationClaimedGroup = x.TaxDeclarationClaimedGroup,
                        TaxDeclarationClaimedTAT = x.TaxDeclarationClaimedTAT,
                        TaxDeclarationClaimedSysTAT = x.TaxDeclarationClaimedSysTAT,
                        TaxDeclarationClaimedRemarks = x.TaxDeclarationClaimedRemarks,
                        TOAS = x.TOAS,
                        TurnoverDate = x.TurnoverDate,
                        SAPTurnoverDate = x.SAPTurnoverDate,
                        QualificationDate = x.QualificationDate,
                        TaxDecNos = x.TaxDecNos,
                        LiquidationEndorsedDate = x.LiquidationEndorsedDate,
                        LiquidationEndorsedGroup = x.LiquidationEndorsedGroup,
                        LiquidationEndorsedTAT = x.LiquidationEndorsedTAT,
                        LiquidationEndorsedSysTAT = x.LiquidationEndorsedSysTAT,
                        LiquidationRushTicketNos = x.LiquidationRushTicketNos,
                        LiquidationEndorsedRemarks = x.LiquidationEndorsedRemarks,
                        TitleReleaseEndorsedDate = x.TitleReleaseEndorsedDate,
                        TitleReleaseEndorsedGroup = x.TitleReleaseEndorsedGroup,
                        TitleReleaseEndorsedTAT = x.TitleReleaseEndorsedTAT,
                        TitleReleaseEndorsedSysTAT = x.TitleReleaseEndorsedSysTAT,
                        TitleReleaseRushTicketNos = x.TitleReleaseRushTicketNos,
                        TitleReleaseEndorsedRemarks = x.TitleReleaseEndorsedRemarks,
                        TitleLocationName = x.TitleLocationName,
                        TitleNos = x.TitleNos,
                        TitleRemarks = x.TitleRemarks,
                        BankReleasedDate = x.BankReleasedDate,
                        BankReleasedGroup = x.BankReleasedGroup,
                        BankReleasedTAT = x.BankReleasedTAT,
                        BankReleasedSysTAT = x.BankReleasedSysTAT,
                        BankReleasedRemarks = x.BankReleasedRemarks,
                        BuyerReleasedDate = x.BuyerReleasedDate,
                        BuyerReleasedGroup = x.BuyerReleasedGroup,
                        BuyerReleasedTAT = x.BuyerReleasedTAT,
                        BuyerReleasedSysTAT = x.BuyerReleasedSysTAT,
                        BuyerReleasedRemarks = x.BankReleasedRemarks,
                        TitleStatus = x.TitleStatus,
                        MeralcoReceiptDate = x.MeralcoReceiptDate,
                        UnitOwnerReceiptDate = x.UnitOwnerReceiptDate,
                        MeralcoSubmittedDate = x.MeralcoSubmittedDate,
                        AgingTOASTaxDecTransfer = x.AgingTOASTaxDecTransfer,
                        AgingQualifTaxDecTransfer = x.AgingQualifTaxDecTransfer,
                        AgingTTransferReleaseBank = x.AgingTTransferReleaseBank,
                        AgingTTransferReleaseBuyer = x.AgingTTransferReleaseBuyer
                    }).AsEnumerable();

                    // For Sorting
                    if (param.sortby != "default")
                    {
                        var sortby = typeof(CustomerDashboard_TitlingStatus).GetProperty(param.sortby);
                        switch (param.reverse)
                        {
                            case true:
                                titling = titling.OrderByDescending(s => sortby.GetValue(s, null));
                                break;
                            case false:
                                titling = titling.OrderBy(s => sortby.GetValue(s, null));
                                break;
                        }
                    }
                    else
                    {
                        titling = titling.OrderByDescending(s => s.TitleStatus).ThenBy(s => s.BusinessEntity).ThenBy(s => s.RefNos);
                    }

                    var data = new { COUNT = source.Count(), CURUSER = user, TITLINGSTATUS = titling,  PROJSELECTED = projects, CONTROLS = permissionCtrl/*, LASTDATESYNC = lastDateSync*/ };

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