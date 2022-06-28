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
    [RoutePrefix("api/TitlingStatus")]
    public class TitlingStatusController : ApiController
    {
        private string PageUrl = "/Admin/TitlingStatus";
        private string ApiName = "Titling Status";

        string timezone = "";

        private TitlingStatusController()
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
                    var prj = db.VW_Projects.Where(x => x.TSM == true && x.Id == item.ProjectID).SingleOrDefault();
                    if (prj != null)
                    {
                        item.ProjectCode = prj.ProjectCode;
                        item.CompanyCode = prj.CompanyCode;
                    }

                    // Get List of Active Projects
                    var projects = await db.VW_SalesInventory.Where(x => x.TSM == true && x.SalesDocStatus == "Active" && x.AccountTypeCode != "L").Select(x => new { Id = x.ProjectId, x.CompanyCode, x.ProjectCode, x.BusinessEntity, x.ProjectLocation, ProjectCodeName = x.ProjectCode + " : " + x.BusinessEntity }).Distinct().OrderBy(x => x.BusinessEntity).ToListAsync();

                    // Get List of Units Inventory
                    var units = await db.VW_SalesInventory.Where(x => x.TSM == true && x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.SalesDocStatus == "Active" && x.AccountTypeCode != "L").Select(x => new { x.UnitNos, x.RefNos, x.CustomerNos }).Distinct().OrderBy(x => x.UnitNos).ToListAsync();
                    // if customer nos is not empty then include on the condition
                    if (!String.IsNullOrEmpty(item.CustomerNos) && units != null)
                        units = units.Where(x => x.CustomerNos == item.CustomerNos).ToList();

                    // Get List of Customer with SO
                    var customers = await db.VW_SalesInventory.Where(x => x.TSM == true && x.SalesDocStatus == "Active" && x.AccountTypeCode != "L" && x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode).Select(x => new { x.CustomerNos, x.CustomerName1, x.CustomerHash, x.UnitNos }).OrderBy(x => x.CustomerNos).ToListAsync();
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

        [Route("GetTitlingStatus")]
        public async Task<IHttpActionResult> GetTitlingStatus([FromUri] SearchData item)
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
                    var titlinglocations = await db.TitlingLocations.Where(x => x.Published == true).Select(x => new { Id = x.Id.ToString(), x.Name }).OrderBy(x => x.Id).ToListAsync();
                    
                    // -------------- TITLING STATUS DATA --------------- //
                    // Get List of Qualified for Titling Status                 
                    var salesInfo = await db.VW_SalesInventory.Where(x => x.TSM == true && x.SalesDocStatus == "Active" && x.AccountTypeCode != "L" && x.CompanyCode == item.CompanyCode && x.ProjectCode == "JPD").FirstOrDefaultAsync();
                 
                    // if unit nos is set, include in the criteria
                    if (!String.IsNullOrEmpty(item.UnitNos) && String.IsNullOrEmpty(item.CustomerNos)) // && source != null
                        salesInfo = await db.VW_SalesInventory.Where(x => x.TSM == true && x.SalesDocStatus == "Active" && x.AccountTypeCode != "L" && x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitNos == item.UnitNos).FirstOrDefaultAsync();

                    // if customer nos is set, include in the criteria
                    if (!String.IsNullOrEmpty(item.CustomerNos) && String.IsNullOrEmpty(item.UnitNos)) // && source != null
                        salesInfo = await db.VW_SalesInventory.Where(x => x.TSM == true && x.SalesDocStatus == "Active" && x.AccountTypeCode != "L" && x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.CustomerNos == item.CustomerNos).FirstOrDefaultAsync();
                    //source = source.Where(x => x.CustomerNos == item.CustomerNos).ToList();

                    // if customer nos is set, include in the criteria
                    if (!String.IsNullOrEmpty(item.CustomerNos) && !String.IsNullOrEmpty(item.UnitNos)) // && source != null
                        salesInfo = await db.VW_SalesInventory.Where(x => x.TSM == true && x.SalesDocStatus == "Active" && x.AccountTypeCode != "L" && x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitNos == item.UnitNos && x.CustomerNos == item.CustomerNos).FirstOrDefaultAsync();
                   
                    //// get only first record. BU must specify specific searching
                    //var salesInfo = source;//.FirstOrDefault();

                    // if doesn't exist display error message
                    if (salesInfo == null)
                        return BadRequest("Record not found");
                    // -------------- QUALIFIED FOR TURNOVER --------------- //


                    // Unit Historical Data
                    CustomTitlingStatus titlestatus = new CustomTitlingStatus();
                    var tlsts = await db.TitlingStatus.Where(x => x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitNos == salesInfo.UnitNos && x.CustomerNos == salesInfo.CustomerNos).FirstOrDefaultAsync();
                    if (tlsts != null)
                    {
                        titlestatus.Id = tlsts.Id;
                        titlestatus.CompanyCode = tlsts.CompanyCode;
                        titlestatus.ProjectCode = tlsts.ProjectCode;
                        titlestatus.UnitNos = tlsts.UnitNos;
                        titlestatus.UnitCategory = tlsts.UnitCategory;
                        titlestatus.CustomerNos = tlsts.CustomerNos;
                        titlestatus.QuotDocNos = tlsts.QuotDocNos;
                        titlestatus.SalesDocNos = tlsts.SalesDocNos;
                        if (tlsts.TitleInProcessDate != null) titlestatus.TitleInProcessDate = tlsts.TitleInProcessDate;
                        titlestatus.TitleInProcessRemarks = db.TitlingStatusRemarks.Where(x => x.TitleStatusID == tlsts.Id && x.TitleStatusType == "Title In-Process").OrderByDescending(x => x.CreatedDate).Take(1).Select(x => x.Remarks).ToList().FirstOrDefault();
                        if (tlsts.TitleTransferredDate != null) titlestatus.TitleTransferredDate = tlsts.TitleTransferredDate;
                        titlestatus.TitleTransferredRemarks = db.TitlingStatusRemarks.Where(x => x.TitleStatusID == tlsts.Id && x.TitleStatusType == "Title Transferred").OrderByDescending(x => x.CreatedDate).Take(1).Select(x => x.Remarks).ToList().FirstOrDefault();
                        if (tlsts.TitleClaimedDate != null) titlestatus.TitleClaimedDate = tlsts.TitleClaimedDate;
                        titlestatus.TitleClaimedRemarks = db.TitlingStatusRemarks.Where(x => x.TitleStatusID == tlsts.Id && x.TitleStatusType == "Title Claimed").OrderByDescending(x => x.CreatedDate).Take(1).Select(x => x.Remarks).ToList().FirstOrDefault();
                        if (tlsts.TaxDeclarationTransferredDate != null) titlestatus.TaxDeclarationTransferredDate = tlsts.TaxDeclarationTransferredDate;
                        titlestatus.TaxDeclarationTransferredRemarks = db.TitlingStatusRemarks.Where(x => x.TitleStatusID == tlsts.Id && x.TitleStatusType == "Tax Declaration Transferred").OrderByDescending(x => x.CreatedDate).Take(1).Select(x => x.Remarks).ToList().FirstOrDefault();
                        if (tlsts.TaxDeclarationClaimedDate != null) titlestatus.TaxDeclarationClaimedDate = tlsts.TaxDeclarationClaimedDate;
                        titlestatus.TaxDeclarationClaimedRemarks = db.TitlingStatusRemarks.Where(x => x.TitleStatusID == tlsts.Id && x.TitleStatusType == "Tax Declaration Claimed").OrderByDescending(x => x.CreatedDate).Take(1).Select(x => x.Remarks).ToList().FirstOrDefault();
                        titlestatus.TaxDecNos = tlsts.TaxDecNos;
                        if (tlsts.LiquidationEndorsedDate != null) titlestatus.LiquidationEndorsedDate = tlsts.LiquidationEndorsedDate;
                        titlestatus.LiquidationRushTicketNos = tlsts.LiquidationRushTicketNos;
                        titlestatus.LiquidationEndorsedRemarks = tlsts.LiquidationEndorsedRemarks;
                        if (tlsts.TitleReleaseEndorsedDate != null) titlestatus.TitleReleaseEndorsedDate = tlsts.TitleReleaseEndorsedDate;
                        titlestatus.TitleReleaseRushTicketNos = tlsts.TitleReleaseRushTicketNos;
                        titlestatus.TitleReleaseEndorsedRemarks = tlsts.TitleReleaseEndorsedRemarks;
                        titlestatus.TitleLocationID = string.IsNullOrEmpty(tlsts.TitleLocationID.ToString())? null : tlsts.TitleLocationID.ToString();
                        titlestatus.TitleLocationName = string.IsNullOrEmpty(tlsts.TitleLocationID.ToString()) ? null : tlsts.TitlingLocation.Name;
                        titlestatus.TitleNos = tlsts.TitleNos;
                        titlestatus.TitleRemarks = tlsts.TitleRemarks;
                        if (tlsts.BankReleasedDate != null) titlestatus.BankReleasedDate = tlsts.BankReleasedDate;
                        titlestatus.BankReleasedRemarks = tlsts.BankReleasedRemarks;
                        if (tlsts.BuyerReleasedDate != null) titlestatus.BuyerReleasedDate = tlsts.BuyerReleasedDate;
                        titlestatus.BuyerReleasedRemarks = tlsts.BuyerReleasedRemarks;
                        titlestatus.TitleInProcessTAT1 = tlsts.TitleInProcessTAT1;
                        titlestatus.TitleInProcessTAT2 = tlsts.TitleInProcessTAT2;
                        titlestatus.TitleInProcessTAT3 = tlsts.TitleInProcessTAT3;
                        titlestatus.TitleTransferredTAT = tlsts.TitleTransferredTAT;
                        titlestatus.TitleClaimedTAT = tlsts.TitleClaimedTAT;
                        titlestatus.TaxDeclarationTransferredTAT = tlsts.TaxDeclarationTransferredTAT;
                        titlestatus.TaxDeclarationClaimedTAT = tlsts.TaxDeclarationClaimedTAT;
                        titlestatus.LiquidationEndorsedTAT = tlsts.LiquidationEndorsedTAT;
                        titlestatus.TitleReleaseEndorsedTAT = tlsts.TitleReleaseEndorsedTAT;
                        titlestatus.TitleInProcessTATCT = tlsts.TitleInProcessTATCT;
                        titlestatus.TitleTransferredTATCT = tlsts.TitleTransferredTATCT;
                        titlestatus.TitleClaimedTATCT = tlsts.TitleClaimedTATCT;
                        titlestatus.TaxDeclarationTransferredTATCT = tlsts.TaxDeclarationTransferredTATCT;
                        titlestatus.TaxDeclarationClaimedTATCT = tlsts.TaxDeclarationClaimedTATCT;
                        titlestatus.LiquidationEndorsedTATCT = tlsts.LiquidationEndorsedTATCT;
                        titlestatus.TitleReleaseEndorsedTATCT = tlsts.TitleReleaseEndorsedTATCT;
                        titlestatus.IsBuyerReleaseNA = tlsts.IsBuyerReleaseNA;
                        titlestatus.IsBankReleaseNA = tlsts.IsBankReleaseNA;
                        titlestatus.CreatedDate = tlsts.CreatedDate;
                        titlestatus.CreatedByPK = tlsts.CreatedByPK;
                        titlestatus.ModifiedDate = tlsts.ModifiedDate;
                        titlestatus.ModifiedByPK = tlsts.ModifiedByPK;
                     } else
                        titlestatus = null;

                    var data = new { CURUSER = user, EXCEPTIONDAYS = exceptionDays, TITLINGLOCATIONLIST = titlinglocations, SALESINFO = salesInfo, TITLESTATUS = titlestatus, DEFTITLESTATUS = titlestatus, SYSPARAM = systemParameter, CONTROLS = permissionCtrl };

                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("GetTitlingRemarks")]
        public async Task<IHttpActionResult> GetTitlingRemarks([FromUri] FilterModel param)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    string search1 = param.search1;
                    int search2 = Convert.ToInt16(param.search2);
                    IEnumerable<CustomTitleRemark> source = null;
                    source = await (from tl in db.TitlingStatusRemarks
                                    where tl.TitleStatusType == search1 && tl.TitleStatusID == search2
                                    select new CustomTitleRemark
                                    {
                                        Id = tl.Id,
                                        TitleStatusID = tl.TitleStatusID,
                                        TitleStatusType = tl.TitleStatusType,
                                        Remarks = tl.Remarks,
                                        ModifiedByPK = tl.ModifiedByPK,
                                        ModifiedDate = tl.ModifiedDate,
                                        CreatedByPK = tl.CreatedByPK,
                                        CreatedDate = tl.CreatedDate
                                    }).ToListAsync();

                    // searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.Remarks.ToLower().Contains(param.search) || x.Remarks.ToLower().Contains(param.search));
                    }

                    // sorting
                    var sortby = typeof(CustomTitleRemark).GetProperty(param.sortby);
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

                    var data = new { COUNT = source.Count(), TitlingRemarksLIST = sourcePaged };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }
        
        //[Route("GetAdjustmentDate")]
        //public async Task<IHttpActionResult> GetAdjustTurnoverDate([FromUri] CustomTitlingStatus data)
        //{
        //    using (WebAppEntities db = new WebAppEntities())
        //    {
        //        try
        //        {
        //            // Check if system parameter is properly set
        //            var systemParameter = await db.SystemParameters.Where(x => x.Published == true).FirstOrDefaultAsync();
        //            if (systemParameter == null)
        //                return BadRequest("Please configure system parameter");

        //            DateTime adjtoDate = DateTime.Today;

        //            if(data.TitleStatusType == "01")
        //            {
        //                if (systemParameter.LiquidationEndorsedTATCT == "CD")
        //                    adjtoDate = data.EmailDateNoticeSent.AddDays(systemParameter.TurnoverMaxDays); // Get only date based on calendar days 
        //                else
        //                    adjtoDate = Convert.ToDateTime(db.Database.SqlQuery<string>("SELECT dbo.fnAdjustmentDate({0}, {1})", data.EmailDateNoticeSent, systemParameter.TurnoverMaxDays + 1).Single()); // Get only date based on working days 
        //            }

        //            return Ok(adjtoDate);
        //        }
        //        catch (Exception ex)
        //        {
        //            return BadRequest(ex.Message);
        //        }
        //    }
        //}

        [Route("SaveTitlingStatus")]
        public async Task<IHttpActionResult> SaveTitlingStatus(CustomTitlingStatus data)
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

                        // if Liquidation Endorsed Date is greater than Title Release Endorsed Date
                        if (data.LiquidationEndorsedDate > data.TitleReleaseEndorsedDate && data.TitleReleaseEndorsedDate != null)
                            return BadRequest("Liquidation Endorsed Date should not be greater than Title Release Endorsed Date");

                        // if Liquidation Endorsed Date has value but no data on Liquidation Rush Ticket Nos
                        if (data.LiquidationEndorsedDate != null && String.IsNullOrEmpty(data.LiquidationRushTicketNos))
                            return BadRequest("Endorsed for Liquidation Rush Ticket No. is required");

                        // if Liquidation Rush Ticket Nos has value but no data on Liquidation Endorsed Date
                        if (data.LiquidationRushTicketNos != null && data.LiquidationEndorsedDate == null)
                            return BadRequest("Endorsed for Liquidation Rush Ticket No. is required");

                        // if Title Release Endorsed Date has value but no data on Endorsed for Title Release Rush Ticket Nos
                        if (data.TitleReleaseEndorsedDate != null && String.IsNullOrEmpty(data.TitleReleaseRushTicketNos))
                            return BadRequest("Endorsed for Title Release Rush Ticket No. is required");

                        // if Endorsed for Title Release Rush Ticket Nos has value but no data on Title Release Endorsed Date
                        if (data.TitleReleaseRushTicketNos != null && data.TitleReleaseEndorsedDate == null)
                            return BadRequest("Endorsed for Title Release Rush Ticket No. is required");

                        // Will NOT accept future date
                        // Reference FRD: 
                        if (DateTime.Today < data.LiquidationEndorsedDate)
                            return BadRequest("Liquidation Endorsed Date will not accept future date");

                        if (DateTime.Today < data.TitleReleaseEndorsedDate)
                            return BadRequest("Title Released Endorsed Date will not accept future date");

                        if (DateTime.Today < data.BankReleasedDate)
                            return BadRequest("Bank Released Date will not accept future date");

                        if (DateTime.Today < data.BuyerReleasedDate)
                            return BadRequest("Buyers Released Date will not accept future date");

                        //// if Bank Release applicable and Bank Release date is null
                        //if (data.IsBankReleaseNA == false && data.BankReleasedDate == null)
                        //    return BadRequest("Date Released to Bank is required");

                        //// if Buyer Release applicable and Buyer Release date is null
                        //if (data.IsBuyerReleaseNA == false && data.BuyerReleasedDate == null)
                        //    return BadRequest("Date Released to Buyer is required");

                        // Check if system parameter is properly set
                        var systemParameter = await db.SystemParameters.Where(x => x.Published == true).FirstOrDefaultAsync();
                        if(systemParameter == null)
                            return BadRequest("Please configure system parameter");

                        TitlingStatu tltsts = new TitlingStatu();

                        tltsts.Id = data.Id;
                        tltsts.CompanyCode = data.CompanyCode;
                        tltsts.CustomerNos = data.CustomerNos;
                        tltsts.ProjectCode = data.ProjectCode;
                        tltsts.UnitNos = data.UnitNos;
                        tltsts.UnitCategory = data.UnitCategory;
                        tltsts.SalesDocNos = data.SalesDocNos;
                        tltsts.QuotDocNos = data.QuotDocNos;

                        if (data.TitleInProcessDate != null) tltsts.TitleInProcessDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TitleInProcessDate.GetValueOrDefault().ToLocalTime(), timezone);
                        if (data.TitleTransferredDate != null) tltsts.TitleTransferredDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TitleTransferredDate.GetValueOrDefault().ToLocalTime(), timezone);
                        if (data.TitleClaimedDate != null) tltsts.TitleClaimedDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TitleClaimedDate.GetValueOrDefault().ToLocalTime(), timezone);
                        if (data.TaxDeclarationTransferredDate != null) tltsts.TaxDeclarationTransferredDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TaxDeclarationTransferredDate.GetValueOrDefault().ToLocalTime(), timezone);
                        if (data.TaxDeclarationClaimedDate != null) tltsts.TaxDeclarationClaimedDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TaxDeclarationClaimedDate.GetValueOrDefault().ToLocalTime(), timezone);
                        tltsts.TaxDecNos = data.TaxDecNos;
                        if (data.LiquidationEndorsedDate != null) tltsts.LiquidationEndorsedDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.LiquidationEndorsedDate.GetValueOrDefault().ToLocalTime(), timezone);
                        tltsts.LiquidationRushTicketNos = data.LiquidationRushTicketNos;
                        tltsts.LiquidationEndorsedRemarks = data.LiquidationEndorsedRemarks;
                        if (data.TitleReleaseEndorsedDate != null) tltsts.TitleReleaseEndorsedDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TitleReleaseEndorsedDate.GetValueOrDefault().ToLocalTime(), timezone);
                        tltsts.TitleReleaseRushTicketNos = data.TitleReleaseRushTicketNos;
                        tltsts.TitleReleaseEndorsedRemarks = data.TitleReleaseEndorsedRemarks;
                        if(!String.IsNullOrEmpty(data.TitleLocationID)) tltsts.TitleLocationID = Convert.ToInt16(data.TitleLocationID);
                        tltsts.TitleNos = data.TitleNos;
                        tltsts.TitleRemarks = data.TitleRemarks;
                        if (data.BankReleasedDate != null) tltsts.BankReleasedDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.BankReleasedDate.GetValueOrDefault().ToLocalTime(), timezone);
                        tltsts.BankReleasedRemarks = data.BankReleasedRemarks;
                        tltsts.IsBankReleaseNA = (data.IsBankReleaseNA == false) ? null : data.IsBankReleaseNA;
                        if (data.BuyerReleasedDate != null) tltsts.BuyerReleasedDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.BuyerReleasedDate.GetValueOrDefault().ToLocalTime(), timezone);
                        tltsts.BuyerReleasedRemarks = data.BuyerReleasedRemarks;
                        tltsts.IsBuyerReleaseNA = (data.IsBuyerReleaseNA == false)? null : data.IsBuyerReleaseNA;

                        // Check System Parameter Effectivity date on Titling Status on TAT
                        if (systemParameter.TitlingStatusEffectivityDate <= DateTime.Today)
                        {
                            if (data.TitleInProcessTAT1 == systemParameter.TitleInProcessTAT1)
                                tltsts.TitleInProcessTAT1 = data.TitleInProcessTAT1;
                            else
                                tltsts.TitleInProcessTAT1 = systemParameter.TitleInProcessTAT1;

                            if (data.TitleInProcessTAT2 == systemParameter.TitleInProcessTAT2)
                                tltsts.TitleInProcessTAT2 = data.TitleInProcessTAT2;
                            else
                                tltsts.TitleInProcessTAT2 = systemParameter.TitleInProcessTAT2;

                            if (data.TitleInProcessTAT3 == systemParameter.TitleInProcessTAT3)
                                tltsts.TitleInProcessTAT3 = data.TitleInProcessTAT3;
                            else
                                tltsts.TitleInProcessTAT3 = systemParameter.TitleInProcessTAT3;

                            if (data.TitleTransferredTAT == systemParameter.TitleTransferredTAT)
                                tltsts.TitleTransferredTAT = data.TitleTransferredTAT;
                            else
                                tltsts.TitleTransferredTAT = systemParameter.TitleTransferredTAT;

                            if (data.TitleClaimedTAT == systemParameter.TitleClaimedTAT)
                                tltsts.TitleClaimedTAT = data.TitleClaimedTAT;
                            else
                                tltsts.TitleClaimedTAT = systemParameter.TitleClaimedTAT;

                            if (data.TaxDeclarationTransferredTAT == systemParameter.TaxDeclarationTransferredTAT)
                                tltsts.TaxDeclarationTransferredTAT = data.TaxDeclarationTransferredTAT;
                            else
                                tltsts.TaxDeclarationTransferredTAT = systemParameter.TaxDeclarationTransferredTAT;

                            if (data.TaxDeclarationClaimedTAT == systemParameter.TaxDeclarationClaimedTAT)
                                tltsts.TaxDeclarationClaimedTAT = data.TaxDeclarationClaimedTAT;
                            else
                                tltsts.TaxDeclarationClaimedTAT = systemParameter.TaxDeclarationClaimedTAT;

                            if (data.LiquidationEndorsedTAT == systemParameter.LiquidationEndorsedTAT)
                                tltsts.LiquidationEndorsedTAT = data.LiquidationEndorsedTAT;
                            else
                                tltsts.LiquidationEndorsedTAT = systemParameter.LiquidationEndorsedTAT;

                            if (data.TitleReleaseEndorsedTAT == systemParameter.TitleReleaseEndorsedTAT)
                                tltsts.TitleReleaseEndorsedTAT = data.TitleReleaseEndorsedTAT;
                            else
                                tltsts.TitleReleaseEndorsedTAT = systemParameter.TitleReleaseEndorsedTAT;

                            if (data.TitleInProcessTATCT == systemParameter.TitleInProcessTATCT)
                                tltsts.TitleInProcessTATCT = data.TitleInProcessTATCT;
                            else
                                tltsts.TitleInProcessTATCT = systemParameter.TitleInProcessTATCT;

                            if (data.TitleTransferredTATCT == systemParameter.TitleTransferredTATCT)
                                tltsts.TitleTransferredTATCT = data.TitleTransferredTATCT;
                            else
                                tltsts.TitleTransferredTATCT = systemParameter.TitleTransferredTATCT;

                            if (data.TitleClaimedTATCT == systemParameter.TitleClaimedTATCT)
                                tltsts.TitleClaimedTATCT = data.TitleClaimedTATCT;
                            else
                                tltsts.TitleClaimedTATCT = systemParameter.TitleClaimedTATCT;

                            if (data.TaxDeclarationTransferredTATCT == systemParameter.TaxDeclarationTransferredTATCT)
                                tltsts.TaxDeclarationTransferredTATCT = data.TaxDeclarationTransferredTATCT;
                            else
                                tltsts.TaxDeclarationTransferredTATCT = systemParameter.TaxDeclarationTransferredTATCT;

                            if (data.TaxDeclarationClaimedTATCT == systemParameter.TaxDeclarationClaimedTATCT)
                                tltsts.TaxDeclarationClaimedTATCT = data.TaxDeclarationClaimedTATCT;
                            else
                                tltsts.TaxDeclarationClaimedTATCT = systemParameter.TaxDeclarationClaimedTATCT;

                            if (data.LiquidationEndorsedTATCT == systemParameter.LiquidationEndorsedTATCT)
                                tltsts.LiquidationEndorsedTATCT = data.LiquidationEndorsedTATCT;
                            else
                                tltsts.LiquidationEndorsedTATCT = systemParameter.LiquidationEndorsedTATCT;

                            if (data.TitleReleaseEndorsedTATCT == systemParameter.TitleReleaseEndorsedTATCT)
                                tltsts.TitleReleaseEndorsedTATCT = data.TitleReleaseEndorsedTATCT;
                            else
                                tltsts.TitleReleaseEndorsedTATCT = systemParameter.TitleReleaseEndorsedTATCT;
                        } else
                        {
                            tltsts.TitleInProcessTAT1 = data.TitleInProcessTAT1;
                            tltsts.TitleInProcessTAT2 = data.TitleInProcessTAT2;
                            tltsts.TitleInProcessTAT3 = data.TitleInProcessTAT3;
                            tltsts.TitleTransferredTAT = data.TitleTransferredTAT;
                            tltsts.TitleClaimedTAT = data.TitleClaimedTAT;
                            tltsts.TaxDeclarationTransferredTAT = data.TaxDeclarationTransferredTAT;
                            tltsts.TaxDeclarationClaimedTAT = data.TaxDeclarationClaimedTAT;
                            tltsts.LiquidationEndorsedTAT = data.LiquidationEndorsedTAT;
                            tltsts.TitleReleaseEndorsedTAT = data.TitleReleaseEndorsedTAT;
                            tltsts.TitleInProcessTATCT = data.TitleInProcessTATCT;
                            tltsts.TitleTransferredTATCT = data.TitleTransferredTATCT;
                            tltsts.TitleClaimedTATCT = data.TitleClaimedTATCT;
                            tltsts.TaxDeclarationTransferredTATCT = data.TaxDeclarationTransferredTATCT;
                            tltsts.TaxDeclarationClaimedTATCT = data.TaxDeclarationClaimedTATCT;
                            tltsts.LiquidationEndorsedTATCT = data.LiquidationEndorsedTATCT;
                            tltsts.TitleReleaseEndorsedTATCT = data.TitleReleaseEndorsedTATCT;
                        }

                        tltsts.ModifiedByPK = cId;
                        tltsts.ModifiedDate = DateTime.Now;
                        tltsts.CreatedByPK = data.CreatedByPK;
                        tltsts.CreatedDate = data.CreatedDate;

                        if (tltsts.Id == 0)
                        {
                            tltsts.CreatedByPK = cId;
                            tltsts.CreatedDate = DateTime.Now;

                            db.TitlingStatus.Add(tltsts);
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            db.Entry(tltsts).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }

                        if (!string.IsNullOrEmpty(data.TitleStatusType))
                        { 
                            // Add Remarks if any
                            TitlingStatusRemark tsr = new TitlingStatusRemark();
                            tsr.Id = 0;
                            tsr.TitleStatusID = tltsts.Id;
                            tsr.TitleStatusType = data.TitleStatusType;
                            tsr.Remarks = data.Remarks;
                            tsr.CreatedByPK = cId;
                            tsr.CreatedDate = DateTime.Now;
                            tsr.ModifiedByPK = cId;
                            tsr.ModifiedDate = DateTime.Now;

                            db.TitlingStatusRemarks.Add(tsr);
                            await db.SaveChangesAsync();
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = (string.IsNullOrEmpty(data.ReasonForChange)) ? "ADD" : "UPDATE";
                        log.EventName = this.ApiName;
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.Remarks = data.ReasonForChange;

                        if (data.Transaction == "Title Transfer")
                        {
                            var logObject = new
                            {
                                CompanyCode = tltsts.CompanyCode,
                                CustomerNos = tltsts.CustomerNos,
                                ProjectCode = tltsts.ProjectCode,
                                UnitNos = data.RefNos,
                                UnitCategory = tltsts.UnitCategory,
                                SalesDocNos = tltsts.SalesDocNos,
                                QuotDocNos = tltsts.QuotDocNos,
                                TitleInProcessDate = (tltsts.TitleInProcessDate == null) ? "" : tltsts.TitleInProcessDate.GetValueOrDefault().ToString("MM/dd/yyyy hh:mm:ss tt"),
                                TitleInProcessRemarks = data.TitleInProcessRemarks,
                                TitleTransferredDate = (tltsts.TitleTransferredDate == null) ? "" : tltsts.TitleTransferredDate.GetValueOrDefault().ToString("MM/dd/yyyy hh:mm:ss tt"),
                                TitleTransferredRemarks = data.TitleTransferredRemarks,
                                TitleClaimedDate = (tltsts.TitleClaimedDate == null) ? "" : tltsts.TitleClaimedDate.GetValueOrDefault().ToString("MM/dd/yyyy hh:mm:ss tt"),
                                TitleClaimedRemarks = data.TitleClaimedRemarks,
                                TaxDeclarationTransferredDate = (tltsts.TaxDeclarationTransferredDate == null) ? "" : tltsts.TaxDeclarationTransferredDate.GetValueOrDefault().ToString("MM/dd/yyyy hh:mm:ss tt"),
                                TaxDeclarationTransferredRemarks = data.TaxDeclarationTransferredRemarks,
                                TaxDeclarationClaimedDate = (tltsts.TaxDeclarationClaimedDate == null) ? "" : tltsts.TaxDeclarationClaimedDate.GetValueOrDefault().ToString("MM/dd/yyyy hh:mm:ss tt"),
                                TaxDeclarationClaimedRemarks = data.TaxDeclarationClaimedRemarks,
                                TaxDecNos = tltsts.TaxDecNos,
                                ModifiedDate = tltsts.ModifiedDate.ToString("MM/dd/yyyy hh:mm:ss tt"),
                                ModifiedByPK = tltsts.ModifiedByPK,
                                ReasonForChange = data.ReasonForChange,
                            };

                            log.Description = ((string.IsNullOrEmpty(data.ReasonForChange)) ? "ADD " : "UPDATE ") + this.ApiName + " - Title Transfer & Tax Dec Status";
                            log.ContentDetail = JsonConvert.SerializeObject(logObject);
                        }
                        else if (data.Transaction == "Title Endorsement")
                        {
                            var logObject = new
                            {
                                CompanyCode = tltsts.CompanyCode,
                                CustomerNos = tltsts.CustomerNos,
                                ProjectCode = tltsts.ProjectCode,
                                UnitNos = data.RefNos,
                                UnitCategory = tltsts.UnitCategory,
                                SalesDocNos = tltsts.SalesDocNos,
                                QuotDocNos = tltsts.QuotDocNos,
                                LiquidationEndorsedDate = (tltsts.LiquidationEndorsedDate == null) ? "" : tltsts.LiquidationEndorsedDate.GetValueOrDefault().ToString("MM/dd/yyyy hh:mm:ss tt"),
                                LiquidationRushTicketNos = tltsts.LiquidationRushTicketNos,
                                LiquidationEndorsedRemarks = tltsts.LiquidationEndorsedRemarks,
                                TitleReleaseEndorsedDate = (tltsts.TitleReleaseEndorsedDate == null) ? "" : tltsts.TitleReleaseEndorsedDate.GetValueOrDefault().ToString("MM/dd/yyyy hh:mm:ss tt"),
                                TitleReleaseRushTicketNos = tltsts.TitleReleaseRushTicketNos,
                                TitleReleaseEndorsedRemarks = tltsts.TitleReleaseEndorsedRemarks,
                                ModifiedDate = tltsts.ModifiedDate.ToString("MM/dd/yyyy hh:mm:ss tt"),
                                ModifiedByPK = tltsts.ModifiedByPK,
                                ReasonForChange = data.ReasonForChange,
                            };

                            log.Description = ((string.IsNullOrEmpty(data.ReasonForChange)) ? "ADD " : "UPDATE ") + this.ApiName + " - Title Endrosement Status";
                            log.ContentDetail = JsonConvert.SerializeObject(logObject);
                        }
                        else if (data.Transaction == "Title Release")
                        {
                            var logObject = new
                            {
                                CompanyCode = tltsts.CompanyCode,
                                CustomerNos = tltsts.CustomerNos,
                                ProjectCode = tltsts.ProjectCode,
                                UnitNos = data.RefNos,
                                UnitCategory = tltsts.UnitCategory,
                                SalesDocNos = tltsts.SalesDocNos,
                                QuotDocNos = tltsts.QuotDocNos,
                                TitleLocationName = data.TitleLocationName,
                                TitleNos = tltsts.TitleNos,
                                TitleRemarks = tltsts.TitleRemarks,
                                IsBankReleaseNA = tltsts.IsBankReleaseNA,
                                BankReleasedDate = (tltsts.BankReleasedDate == null) ? "" : tltsts.BankReleasedDate.GetValueOrDefault().ToString("MM/dd/yyyy hh:mm:ss tt"),
                                BankReleasedRemarks = tltsts.BankReleasedRemarks,
                                IsBuyerReleaseNA = tltsts.IsBuyerReleaseNA,
                                BuyerReleasedDate = (tltsts.BuyerReleasedDate == null) ? "" : tltsts.BuyerReleasedDate.GetValueOrDefault().ToString("MM/dd/yyyy hh:mm:ss tt"),
                                BuyerReleasedRemarks = tltsts.BuyerReleasedRemarks,
                                ModifiedDate = tltsts.ModifiedDate.ToString("MM/dd/yyyy hh:mm:ss tt"),
                                ModifiedByPK = tltsts.ModifiedByPK,
                                ReasonForChange = data.ReasonForChange,
                            };

                            log.Description = ((string.IsNullOrEmpty(data.ReasonForChange)) ? "ADD " : "UPDATE ") + this.ApiName + " - Title Details & Release Status";
                            log.ContentDetail = JsonConvert.SerializeObject(logObject);
                        }
                        // ---------------- End Transaction Activity Logs -------------------- //

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

        [Route("SaveTitleRemarks")]
        public async Task<IHttpActionResult> SaveTitleRemarks(CustomTitleRemark data)
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

                        TitlingStatusRemark ctr = new TitlingStatusRemark();

                        ctr.Id = data.Id;
                        ctr.TitleStatusID = data.TitleStatusID;
                        ctr.TitleStatusType = data.TitleStatusType;
                        ctr.Remarks = data.Remarks;
                        ctr.ModifiedByPK = cId;
                        ctr.ModifiedDate = DateTime.Now;
                        ctr.CreatedByPK = data.CreatedByPK;
                        ctr.CreatedDate = data.CreatedDate;

                        if (ctr.Id == 0)
                        {
                            nwe = true;
                            ctr.CreatedByPK = cId;
                            ctr.CreatedDate = DateTime.Now;

                            db.TitlingStatusRemarks.Add(ctr);
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            db.Entry(ctr).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = (nwe) ? "CREATE" : "UPDATE";
                        log.Description = (nwe) ? "Create " + this.ApiName + " - Title Status Remarks (" + data.TitleStatusType + ")" : "Update " + this.ApiName + " - Title Status Remarks (" + data.TitleStatusType + ")";
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.EventName = this.ApiName;
                        log.ContentDetail = JsonConvert.SerializeObject(ctr);
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