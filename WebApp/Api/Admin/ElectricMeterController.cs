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
    [RoutePrefix("api/ElectricMeter")]
    public class ElectricMeterController : ApiController
    {
        private string PageUrl = "/Admin/ElectricMeter";
        private string ApiName = "Electric Meter";

        string timezone = "";

        private ElectricMeterController()
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
                    var prj = db.VW_Projects.Where(x => x.EMM == true && x.Id == item.ProjectID).SingleOrDefault();
                    if (prj != null)
                    {
                        item.ProjectCode = prj.ProjectCode;
                        item.CompanyCode = prj.CompanyCode;
                    }

                    // Get Documentary Requirements
                    var documentLists = await db.DocumentaryRequirements.Where(x => x.Published == true).Select(x => new CustomElectricMeterDocument { Id = x.Id, DocumentName = x.Name, isChecked = false }).OrderBy(x => x.Id).ToListAsync();

                    // Get List of Active Projects
                    var projects = await db.VW_SalesInventory.Where(x => x.EMM == true && x.SalesDocStatus == "Active" && x.AccountTypeCode != "L").Select(x => new { Id = x.ProjectId, x.CompanyCode, x.ProjectCode, x.BusinessEntity, x.ProjectLocation, ProjectCodeName = x.ProjectCode + " : " + x.BusinessEntity }).Distinct().OrderBy(x => x.BusinessEntity).ToListAsync();

                    // Get List of Units Inventory
                    var units = await db.VW_SalesInventory.Where(x => x.EMM == true && x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.SalesDocStatus == "Active" && x.AccountTypeCode != "L").Select(x => new { x.UnitNos, x.RefNos, x.CustomerNos }).Distinct().OrderBy(x => x.UnitNos).ToListAsync();
                    // if customer nos is not empty then include on the condition
                    if (!String.IsNullOrEmpty(item.CustomerNos) && units != null)
                        units = units.Where(x => x.CustomerNos == item.CustomerNos).ToList();

                    // Get List of Customer with SO
                    var customers = await db.VW_SalesInventory.Where(x => x.EMM == true && x.SalesDocStatus == "Active" && x.AccountTypeCode != "L" && x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode).Select(x => new { x.CustomerNos, x.CustomerName1, x.CustomerHash, x.UnitNos }).OrderBy(x => x.CustomerNos).ToListAsync();
                    // if unit nos is not empty then include on the condition
                    if (!String.IsNullOrEmpty(item.UnitNos) && customers != null)
                        customers = customers.Where(x => x.UnitNos == item.UnitNos).ToList();

                    var data = new { PROJECTLIST = projects, UNITLIST = units, CUSTOMERLIST = customers, DOCUMENTLIST = documentLists };

                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("GetElectricMeter")]
        public async Task<IHttpActionResult> GetElectricMeter([FromUri] SearchData item)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var permissionCtrl = this.GetPermissionControl(item.PageUrl);

                    // Get Current User
                    var cId = User.Identity.GetUserId();
                    var user = db.AspNetUsersProfiles.Where(x => x.Id == cId).Select(x=> new { vFullname = x.vFirstName + " " + x.vLastName } ).SingleOrDefault().vFullname;
                    DateTime dt = DateTime.Now;

                    // Check if system parameter is properly set
                    var systemParameter = await db.SystemParameters.Where(x => x.Published == true).FirstOrDefaultAsync();
                    if (systemParameter == null)
                        return BadRequest("Please configure system parameter");

                    // Get List of Holidays
                    var exceptionDays = await db.HolidayDimensions.Select(x => x.TheDate2).ToArrayAsync();

                    // Get Documentary Requirements
                    var documentLists = await db.DocumentaryRequirements.Where(x => x.Published == true).Select(x => new CustomElectricMeterDocument { Id = x.Id, DocumentName = x.Name, isChecked = false }).OrderBy(x => x.Id).ToListAsync();

                    // -------------- TITLING STATUS DATA --------------- //
                    // Get List of Qualified Clients for Electric Meter                    
                    var salesInfo = await db.VW_SalesInventory.Where(x => x.EMM == true && x.SalesDocStatus == "Active" && x.AccountTypeCode != "L" && x.CompanyCode == item.CompanyCode && x.ProjectCode == "JPD").FirstOrDefaultAsync();

                    // if unit nos is set, include in the criteria
                    if (!String.IsNullOrEmpty(item.UnitNos) && String.IsNullOrEmpty(item.CustomerNos)) // && source != null
                        salesInfo = await db.VW_SalesInventory.Where(x => x.EMM == true && x.SalesDocStatus == "Active" && x.AccountTypeCode != "L" && x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitNos == item.UnitNos).FirstOrDefaultAsync();

                    // if customer nos is set, include in the criteria
                    if (!String.IsNullOrEmpty(item.CustomerNos) && String.IsNullOrEmpty(item.UnitNos)) // && source != null
                        salesInfo = await db.VW_SalesInventory.Where(x => x.EMM == true && x.SalesDocStatus == "Active" && x.AccountTypeCode != "L" && x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.CustomerNos == item.CustomerNos).FirstOrDefaultAsync();
                    //source = source.Where(x => x.CustomerNos == item.CustomerNos).ToList();

                    // if customer nos is set, include in the criteria
                    if (!String.IsNullOrEmpty(item.CustomerNos) && !String.IsNullOrEmpty(item.UnitNos)) // && source != null
                        salesInfo = await db.VW_SalesInventory.Where(x => x.EMM == true && x.SalesDocStatus == "Active" && x.AccountTypeCode != "L" && x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitNos == item.UnitNos && x.CustomerNos == item.CustomerNos).FirstOrDefaultAsync();

                    //// get only first record. BU must specify specific searching
                    //var salesInfo = source;//.FirstOrDefault();

                    // if doesn't exist display error message
                    if (salesInfo == null)
                        return BadRequest("Record not found");
                    // -------------- QUALIFIED FOR TURNOVER --------------- //

                    // Unit Historical Data
                    CustomElectricMeter electricmeter = new CustomElectricMeter();

                    decimal MeterDepositAmount = 0;
                    bool IsMeterDepositAmountEditable = true;
                    var serviceDeposit = db.ElectricMeterInvServiceDeposits.Where(x => x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitNos == salesInfo.UnitNos).FirstOrDefault();
                    if (serviceDeposit != null)
                    {
                        MeterDepositAmount = serviceDeposit.MeterDepositAmount;
                        IsMeterDepositAmountEditable = (MeterDepositAmount > 0)? false : true;
                    }

                    var emd = await db.ElectricMeters.Where(x => x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitNos == salesInfo.UnitNos && x.CustomerNos == salesInfo.CustomerNos).FirstOrDefaultAsync();
                    if (emd != null)
                    {
                        electricmeter.Id = emd.Id;
                        electricmeter.CompanyCode = emd.CompanyCode;
                        electricmeter.ProjectCode = emd.ProjectCode;
                        electricmeter.UnitNos = emd.UnitNos;
                        electricmeter.UnitCategory = emd.UnitCategory;
                        electricmeter.CustomerNos = emd.CustomerNos;
                        electricmeter.QuotDocNos = emd.QuotDocNos;
                        electricmeter.QuotDocNos = emd.SalesDocNos;
                        electricmeter.MeterDepositAmount = emd.MeterDepositAmount;
                        electricmeter.IsMeterDepositAmountEditable = IsMeterDepositAmountEditable;
                        electricmeter.ApplicationProcessStatus = emd.ApplicationProcessStatus;
                        electricmeter.DocumentaryRemarks = emd.DocumentaryRemarks;
                        if (emd.DocumentaryCompletedDate != null) electricmeter.DocumentaryCompletedDate = emd.DocumentaryCompletedDate;
                        if (emd.DocumentaryLastModifedDate != null) electricmeter.DocumentaryLastModifedDate = emd.DocumentaryLastModifedDate;
                        if (emd.RFPRushTicketDate != null) electricmeter.RFPRushTicketDate = emd.RFPRushTicketDate;
                        electricmeter.RFPRushTicketNos = emd.RFPRushTicketNos;
                        electricmeter.RFPRushTicketRemarks = emd.RFPRushTicketRemarks;
                        electricmeter.IsReceivedCheck = emd.IsReceivedCheck;
                        if (emd.ReceivedCheckDate != null) electricmeter.ReceivedCheckDate = emd.ReceivedCheckDate;
                        electricmeter.ReceivedCheckRemarks = emd.ReceivedCheckRemarks;
                        electricmeter.WithUnpaidBills = emd.WithUnpaidBills;
                        electricmeter.UnpaidBillPostedDate = emd.UnpaidBillPostedDate;
                        electricmeter.IsPaidSettled = emd.IsPaidSettled;
                        electricmeter.PaidSettledPostedDate = emd.PaidSettledPostedDate;
                        electricmeter.DepositApplicationRemarks = emd.DepositApplicationRemarks;
                        if (emd.MeralcoSubmittedDate != null) electricmeter.MeralcoSubmittedDate = emd.MeralcoSubmittedDate;
                        electricmeter.MeralcoSubmittedRemarks = emd.MeralcoSubmittedRemarks;
                        if (emd.MeralcoReceiptDate != null) electricmeter.MeralcoReceiptDate = emd.MeralcoReceiptDate;
                        electricmeter.MeralcoReceiptRemarks = emd.MeralcoReceiptRemarks;
                        electricmeter.DocCompletionTAT = emd.DocCompletionTAT;
                        electricmeter.DocCompletionTATCT = emd.DocCompletionTATCT;
                        electricmeter.RFPCreationTAT = emd.RFPCreationTAT;
                        electricmeter.RFPCreationTATCT = emd.RFPCreationTATCT;
                        electricmeter.CheckPaymentReleaseTAT = emd.CheckPaymentReleaseTAT;
                        electricmeter.CheckPaymentReleaseTATCT = emd.CheckPaymentReleaseTATCT;
                        electricmeter.MeralcoSubmissionTAT = emd.MeralcoSubmissionTAT;
                        electricmeter.MeralcoSubmissionTATCT = emd.MeralcoSubmissionTATCT;
                        electricmeter.TransferElectricServTAT = emd.TransferElectricServTAT;
                        electricmeter.TransferElectricServTATCT = emd.TransferElectricServTATCT;

                        if (emd.UnitOwnerReceiptDate != null)
                        {
                            electricmeter.UnitOwnerReceiptDate = emd.UnitOwnerReceiptDate;
                            electricmeter.UnitOwnerReceiptTAT = emd.UnitOwnerReceiptTAT;
                            electricmeter.UnitOwnerReceiptTATCT = emd.UnitOwnerReceiptTATCT;

                            if (systemParameter.UnitOwnerReceiptTATCT == "CD")
                                electricmeter.UnitOwnerReceiptStatus = db.Database.SqlQuery<string>("SELECT (CASE WHEN DATEDIFF(dd, {0}, GETDATE()) > {2} THEN 'Beyond' ELSE 'Within' END)", emd.UnitOwnerReceiptDate, electricmeter.UnitOwnerReceiptTAT + 1).Single(); // Get only date based on calendar days 
                            else
                                electricmeter.UnitOwnerReceiptStatus = db.Database.SqlQuery<string>("SELECT (CASE WHEN dbo.fnCalcAgingWorkingDays({0}, GETDATE()) > {1} THEN 'Beyond' ELSE 'Within' END)", emd.UnitOwnerReceiptDate, electricmeter.UnitOwnerReceiptTAT + 1).Single(); // Get only date based on working days 
                        }

                        electricmeter.UnitOwnerReceiptRemarks = emd.UnitOwnerReceiptRemarks;

                        electricmeter.CreatedDate = emd.CreatedDate;
                        electricmeter.CreatedByPK = emd.CreatedByPK;
                        electricmeter.ModifiedDate = emd.ModifiedDate;
                        electricmeter.ModifiedByPK = emd.ModifiedByPK;
                        electricmeter.SalesElectricDocStatus = emd.SalesElectricDocStatus;
                        electricmeter.DocumentaryStatus = emd.DocumentaryStatus;

                        // Get Documentary Requirements
                        documentLists = (from pt in db.DocumentaryRequirements
                                            join ptt in db.ElectricMeterDocuments on new { a = pt.Id, b = emd.Id } equals new { a = ptt.DocumentID, b = ptt.ElectricMeterID } into unpk
                                            from ptt in unpk.DefaultIfEmpty()
                                            select new CustomElectricMeterDocument
                                            {
                                                Id = pt.Id,
                                                DocumentName = pt.Name,
                                                isChecked = (ptt.Id == null) ? false : true,
                                                isDisabled = (ptt.Id == null) ? false : true,
                                                resetChecked = (ptt.Id == null) ? false : true
                                            }).ToList();

                        electricmeter.IsDocumentCompleted = !Convert.ToBoolean(documentLists.Where(x => x.isChecked == false).Count());
                    }
                    else
                        electricmeter = null;  

                    var data = new { CURUSER = user, EXCEPTIONDAYS = exceptionDays, SALESINFO = salesInfo, ELECTRICMETER = electricmeter, METERDEPOSITAMNT = MeterDepositAmount, ISMETERDEPOSITAMNTEDITABLE = IsMeterDepositAmountEditable, DEFELECTRICMETER = electricmeter, DOCUMENTLIST = documentLists, SYSPARAM = systemParameter, CONTROLS = permissionCtrl };

                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("SaveElectricMeter")]
        public async Task<IHttpActionResult> SaveElectricMeter(CustomElectricMeter data)
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

                        // ******************** Validation ******************** //

                        decimal MeterDepositAmount = 0;
                        var serviceDeposit = db.ElectricMeterInvServiceDeposits.Where(x => x.CompanyCode == data.CompanyCode && x.ProjectCode == data.ProjectCode && x.UnitNos == data.UnitNos).FirstOrDefault();
                        if (serviceDeposit != null)
                            MeterDepositAmount = serviceDeposit.MeterDepositAmount;

                        if (data.MeterDepositAmount != MeterDepositAmount && data.IsMeterDepositAmountEditable == false)
                            return BadRequest("Meter Deposit Amount should not be editable!");

                        // Will NOT accept future date
                        // Reference FRD: 
                        if (DateTime.Today < data.RFPRushTicketDate)
                            return BadRequest("RFP Rush Ticket Date will not accept future date");

                        if (data.IsReceivedCheck == true && (string.IsNullOrEmpty(data.RFPRushTicketNos) || data.RFPRushTicketDate == null))
                            return BadRequest("Received Check is not allowed. Rush ticket Nos & Date are required");

                        if (DateTime.Today < data.MeralcoSubmittedDate)
                            return BadRequest("Meralco Submitted Application Date will not accept future date");

                        if (DateTime.Today < data.MeralcoReceiptDate)
                            return BadRequest("Receipt from Meralco Date will not accept future date");

                        if (DateTime.Today < data.UnitOwnerReceiptDate)
                            return BadRequest("Receipt from Unit Owner Date will not accept future date");

                        if (data.IsPaidSettled == true && data.WithUnpaidBills == false)
                            return BadRequest("Tagging of paid/settle is not allowed if with remaining unpaid bills");

                        // if Receipt from Meralco Date is greater than Meralco Submitted Application Date
                        if (data.MeralcoSubmittedDate > data.MeralcoReceiptDate && data.MeralcoReceiptDate != null)
                            return BadRequest("Receipt from Meralco Date should not be greater than Meralco Submitted Application Date");

                        // if Receipt from Unit Owner Date is greater than Receipt from Meralco Date
                        if (data.MeralcoReceiptDate > data.UnitOwnerReceiptDate && data.UnitOwnerReceiptDate != null)
                            return BadRequest("Receipt from Unit Owner Date should not be greater than Receipt from Meralco Date");

                        // if Meralco Submitted Application Dat is blank but details from Receipt from Meralco has value
                        if (data.MeralcoSubmittedDate == null && (data.MeralcoReceiptDate != null && !string.IsNullOrEmpty(data.MeralcoReceiptRemarks)))
                            return BadRequest("Entry of Receipt from Unit Owner detail is not allowed. Receipt from Meralco Date is required");

                        // if Receipt from Meralco Date is blank but details from Receipt from Unit Owner has value
                        if (data.MeralcoReceiptDate == null && (data.UnitOwnerReceiptDate != null && !string.IsNullOrEmpty(data.UnitOwnerReceiptRemarks)))
                            return BadRequest("Entry of Receipt from Unit Owner detail is not allowed. Receipt from Meralco Date is required");

                        //  Once “Date Application Submitted to Meralco” field from Meralco Service Transfer Status transaction TAB is already tagged or with posted date already, system will NO longer allow editing / updating of any information on this transaction tab or editing will already be locked. 
                        if (data.MeralcoSubmittedDate != null && (data.Transaction == "Document Status" || data.Transaction == "RFP Status"))
                            return BadRequest("Date Application Submitted to Meralco was already psoted. Editing/updating is no longer allowed.");

                        //  After 15 working days from the actual date posting/ tagging of “Date Receipt of Unit Owner” field for the new service contract, system will NO longer allow editing / updating of any information on the said transaction tab or editing will already be locked. 
                        if (data.UnitOwnerReceiptStatus == "Beyond")
                            return BadRequest("Editing/updating is no longer allowed.");
                        // ******************** Validation ******************** //

                        var isDocumentCompleted = data.DocumentList.Where(x => x.isChecked == false).Count();

                        // check if exisitingdata in db is aligned on data in ui
                        List<int> existingDbDocs = db.ElectricMeterDocuments.Where(x => x.ElectricMeterID == data.Id).Select(x => x.DocumentID).ToList();
                        List<int> currentDocs = data.DocumentList.Where(x => x.isChecked == true).Select(x => x.Id).ToList();
                        bool isEqual = Enumerable.SequenceEqual(existingDbDocs.OrderBy(e => e), currentDocs.OrderBy(e => e));

                        ElectricMeter etm = new ElectricMeter();

                        etm.Id = data.Id;
                        etm.CompanyCode = data.CompanyCode;
                        etm.CustomerNos = data.CustomerNos;
                        etm.ProjectCode = data.ProjectCode;
                        etm.UnitNos = data.UnitNos;
                        etm.UnitCategory = data.UnitCategory;
                        etm.SalesDocNos = data.SalesDocNos;
                        etm.QuotDocNos = data.QuotDocNos;

                        // ******************** Document Status ******************** //
                        // Auto-populated for units included on the mass uploading during initial set-up
                        etm.MeterDepositAmount = data.MeterDepositAmount;
                        etm.ApplicationProcessStatus = (String.IsNullOrEmpty(data.ApplicationProcessStatus))? "In-Process" : data.ApplicationProcessStatus;
                        // Check if documents is already complete
                        if (data.DocumentaryCompletedDate == null && isDocumentCompleted == 0)
                            etm.DocumentaryCompletedDate = DateTime.Now;
                        else
                            etm.DocumentaryCompletedDate = data.DocumentaryCompletedDate;

                        // Check last modified date on document
                        if (etm.DocumentaryCompletedDate != null && !isEqual)
                            if (existingDbDocs.Count == 0 && etm.DocumentaryLastModifedDate == null)
                                etm.DocumentaryLastModifedDate = null;
                            else
                                etm.DocumentaryLastModifedDate = DateTime.Now;
                        else
                            etm.DocumentaryLastModifedDate = data.DocumentaryLastModifedDate;

                        etm.DocumentaryRemarks = data.DocumentaryRemarks;
                        etm.DocumentaryStatus = (isDocumentCompleted == 0) ? true : false;
                        etm.SalesElectricDocStatus = data.SalesElectricDocStatus;

                        // ******************** RFP Status ******************** //
                        if (data.RFPRushTicketDate != null) etm.RFPRushTicketDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.RFPRushTicketDate.GetValueOrDefault().ToLocalTime(), timezone);
                        etm.RFPRushTicketNos = data.RFPRushTicketNos;
                        etm.RFPRushTicketRemarks = data.RFPRushTicketRemarks;
                        etm.IsReceivedCheck = (data.IsReceivedCheck == false) ? null : data.IsReceivedCheck;
                        if (data.IsReceivedCheck == true)
                            etm.ReceivedCheckDate = (data.ReceivedCheckDate == null) ? DateTime.Now : data.ReceivedCheckDate;
                        else
                            etm.ReceivedCheckDate = null;

                        etm.ReceivedCheckRemarks = data.ReceivedCheckRemarks;

                        // ******************** Meralco Status ******************** //                       
                        etm.WithUnpaidBills = data.WithUnpaidBills;
                        if (data.UnpaidBillPostedDate != null) etm.UnpaidBillPostedDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.UnpaidBillPostedDate.GetValueOrDefault().ToLocalTime(), timezone);
                        etm.IsPaidSettled = (data.IsPaidSettled == null) ? null : data.IsPaidSettled;
                        if (data.PaidSettledPostedDate != null && etm.IsPaidSettled != null) etm.PaidSettledPostedDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.PaidSettledPostedDate.GetValueOrDefault().ToLocalTime(), timezone);
                        // Record Unpaid Posting Date  when the “With Unpaid Bills?”field  is tagged / modified by either “Yes” or “No” Date and Posting Date is empty
                        etm.UnpaidBillPostedDate = (data.WithUnpaidBills != null && etm.UnpaidBillPostedDate == null) ? DateTime.Now : etm.UnpaidBillPostedDate;
                        // Record Paid Settled Posting Date when the “Paid/ Settled (for unpaid bills)?” field  is tagged/ modified as “Yes” and Posting Date is empty
                        etm.PaidSettledPostedDate = (data.IsPaidSettled == true && etm.PaidSettledPostedDate == null) ? DateTime.Now : etm.PaidSettledPostedDate;
                        etm.DepositApplicationRemarks = data.DepositApplicationRemarks;
                        if (data.MeralcoSubmittedDate != null) etm.MeralcoSubmittedDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.MeralcoSubmittedDate.GetValueOrDefault().ToLocalTime(), timezone);
                        etm.MeralcoSubmittedRemarks = data.MeralcoSubmittedRemarks;
                        if (data.MeralcoReceiptDate != null) etm.MeralcoReceiptDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.MeralcoReceiptDate.GetValueOrDefault().ToLocalTime(), timezone);
                        etm.MeralcoReceiptRemarks = data.MeralcoReceiptRemarks;
                        if (data.UnitOwnerReceiptDate != null) etm.UnitOwnerReceiptDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.UnitOwnerReceiptDate.GetValueOrDefault().ToLocalTime(), timezone);
                        etm.UnitOwnerReceiptRemarks = data.UnitOwnerReceiptRemarks;

                        if (data.Transaction == "Document Status")
                        {
                            // 4.1 status once List of Requirements field are complete or all documentary list are with check mark. 
                            etm.ApplicationProcessStatus = (isDocumentCompleted == 0) ? "DOCUMENT REQUIREMENT COMPLETED" : "In-Process";
                        }
                        else if (data.Transaction == "RFP Status")
                        {
                            // 4.1 status once Received Check? field on RFP Status is tagged as “Yes”. 
                            etm.ApplicationProcessStatus = (etm.IsReceivedCheck == true) ? "RFP COMPLETED" : etm.ApplicationProcessStatus;
                        }
                        else if (data.Transaction == "Electric Status")
                        {
                            // 4.1 status once Date Application Submitted to Meralco field is with posted date. 
                            etm.ApplicationProcessStatus = (etm.MeralcoSubmittedDate != null) ? "SUBMITTED TO MERALCO" : etm.ApplicationProcessStatus;
                            // 4.1 status once Date Receipt from Meralco field for the new service contract is with posted date. 
                            etm.ApplicationProcessStatus = (etm.MeralcoReceiptDate != null) ? "APPLICATION COMPLETED" : etm.ApplicationProcessStatus;
                            // 4.1 status if With Unpaid Bills? field on Electric Meter & Service Deposit Application Details is tagged as “Yes”.
                            etm.ApplicationProcessStatus = (etm.WithUnpaidBills == true) ? "WITH UNPAID BILLS" : etm.ApplicationProcessStatus;
                        }
                        

                        // Check System Parameter Effectivity date on Electric Meter on TAT
                        if (systemParameter.ElectricMeterEffectivityDate <= DateTime.Today)
                        {
                            if (data.DocCompletionTAT == systemParameter.DocCompletionTAT)
                                etm.DocCompletionTAT = data.DocCompletionTAT;
                            else
                                etm.DocCompletionTAT = systemParameter.DocCompletionTAT;

                            if (data.DocCompletionTATCT == systemParameter.DocCompletionTATCT)
                                etm.DocCompletionTATCT = data.DocCompletionTATCT;
                            else
                                etm.DocCompletionTATCT = systemParameter.DocCompletionTATCT;

                            if (data.RFPCreationTAT == systemParameter.RFPCreationTAT)
                                etm.RFPCreationTAT = data.RFPCreationTAT;
                            else
                                etm.RFPCreationTAT = systemParameter.RFPCreationTAT;

                            if (data.RFPCreationTATCT == systemParameter.RFPCreationTATCT)
                                etm.RFPCreationTATCT = data.RFPCreationTATCT;
                            else
                                etm.RFPCreationTATCT = systemParameter.RFPCreationTATCT;

                            if (data.CheckPaymentReleaseTAT == systemParameter.CheckPaymentReleaseTAT)
                                etm.CheckPaymentReleaseTAT = data.CheckPaymentReleaseTAT;
                            else
                                etm.CheckPaymentReleaseTAT = systemParameter.CheckPaymentReleaseTAT;

                            if (data.CheckPaymentReleaseTATCT == systemParameter.CheckPaymentReleaseTATCT)
                                etm.CheckPaymentReleaseTATCT = data.CheckPaymentReleaseTATCT;
                            else
                                etm.CheckPaymentReleaseTATCT = systemParameter.CheckPaymentReleaseTATCT;

                            if (data.MeralcoSubmissionTAT == systemParameter.MeralcoSubmissionTAT)
                                etm.MeralcoSubmissionTAT = data.MeralcoSubmissionTAT;
                            else
                                etm.MeralcoSubmissionTAT = systemParameter.MeralcoSubmissionTAT;

                            if (data.MeralcoSubmissionTATCT == systemParameter.MeralcoSubmissionTATCT)
                                etm.MeralcoSubmissionTATCT = data.MeralcoSubmissionTATCT;
                            else
                                etm.MeralcoSubmissionTATCT = systemParameter.MeralcoSubmissionTATCT;

                            if (data.TransferElectricServTAT == systemParameter.TransferElectricServTAT)
                                etm.TransferElectricServTAT = data.TransferElectricServTAT;
                            else
                                etm.TransferElectricServTAT = systemParameter.TransferElectricServTAT;

                            if (data.TransferElectricServTATCT == systemParameter.TransferElectricServTATCT)
                                etm.TransferElectricServTATCT = data.TransferElectricServTATCT;
                            else
                                etm.TransferElectricServTATCT = systemParameter.TransferElectricServTATCT;

                            // After 15 working days from the actual date posting/ tagging of “Date Receipt of Unit Owner” field for 
                            // the new service contract, system will NO longer allow editing / updating of any information on the said 
                            // transaction tab or editing will already be locked.  
                            if (data.UnitOwnerReceiptTAT == systemParameter.UnitOwnerReceiptTAT)
                                etm.UnitOwnerReceiptTAT = data.UnitOwnerReceiptTAT;
                            else
                                etm.UnitOwnerReceiptTAT = systemParameter.UnitOwnerReceiptTAT;

                            if (data.UnitOwnerReceiptTATCT == systemParameter.UnitOwnerReceiptTATCT)
                                etm.UnitOwnerReceiptTATCT = data.UnitOwnerReceiptTATCT;
                            else
                                etm.UnitOwnerReceiptTATCT = systemParameter.UnitOwnerReceiptTATCT;

                        } else
                        {
                            etm.DocCompletionTAT = data.DocCompletionTAT;
                            etm.DocCompletionTATCT = data.DocCompletionTATCT;
                            etm.RFPCreationTAT = data.RFPCreationTAT;
                            etm.RFPCreationTATCT = data.RFPCreationTATCT;
                            etm.CheckPaymentReleaseTAT = data.CheckPaymentReleaseTAT;
                            etm.CheckPaymentReleaseTATCT = data.CheckPaymentReleaseTATCT;
                            etm.MeralcoSubmissionTAT = data.MeralcoSubmissionTAT;
                            etm.MeralcoSubmissionTATCT = data.MeralcoSubmissionTATCT;
                            etm.TransferElectricServTAT = data.TransferElectricServTAT;
                            etm.TransferElectricServTATCT = data.TransferElectricServTATCT;
                            etm.UnitOwnerReceiptTAT = data.UnitOwnerReceiptTAT;
                            etm.UnitOwnerReceiptTATCT = data.UnitOwnerReceiptTATCT;
                        }

                        etm.ModifiedByPK = cId;
                        etm.ModifiedDate = DateTime.Now;
                        etm.CreatedByPK = data.CreatedByPK;
                        etm.CreatedDate = data.CreatedDate;

                        if (etm.Id == 0)
                        {
                            etm.CreatedByPK = cId;
                            etm.CreatedDate = DateTime.Now;

                            db.ElectricMeters.Add(etm);
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            db.Entry(etm).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }

                        // Document List
                        if (!isEqual && etm.Id != 0)
                        {
                            db.ElectricMeterDocuments.RemoveRange(db.ElectricMeterDocuments.Where(x => x.ElectricMeterID == etm.Id));
                            await db.SaveChangesAsync();

                            foreach (var dl in data.DocumentList.Where(x => x.isChecked == true))
                            {
                                ElectricMeterDocument emd = new ElectricMeterDocument();

                                emd.Id = Guid.NewGuid().ToString();
                                emd.DocumentID = dl.Id;
                                emd.ElectricMeterID = etm.Id;
                                emd.FileName = "";
                                emd.FilePath = "";
                                emd.Remarks = "";
                                emd.CreatedByPK = cId;
                                emd.CreatedDate = DateTime.Now;
                                emd.ModifiedByPK = cId;
                                emd.ModifiedDate = DateTime.Now;

                                db.ElectricMeterDocuments.Add(emd);
                                await db.SaveChangesAsync();
                            }
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = (string.IsNullOrEmpty(data.ReasonForChange)) ? "ADD" : "UPDATE";
                        log.EventName = this.ApiName;
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.Remarks = data.ReasonForChange;

                        if (data.Transaction == "Document Status")
                        {
                            var logObject = new
                            {
                                CompanyCode = etm.CompanyCode,
                                CustomerNos = etm.CustomerNos,
                                ProjectCode = etm.ProjectCode,
                                UnitNos = data.RefNos,
                                UnitCategory = etm.UnitCategory,
                                SalesDocNos = etm.SalesDocNos,
                                QuotDocNos = etm.QuotDocNos,
                                MeterDepositAmount = etm.MeterDepositAmount,
                                ApplicationProcessStatus = etm.ApplicationProcessStatus,
                                DocumentaryCompletedDate = (etm.DocumentaryCompletedDate == null)? "" : etm.DocumentaryCompletedDate.GetValueOrDefault().ToString("MM/dd/yyyy hh:mm:ss tt"),
                                DocumentaryLastModifedDate = (etm.DocumentaryLastModifedDate == null) ? "" : etm.DocumentaryLastModifedDate.GetValueOrDefault().ToString("MM/dd/yyyy hh:mm:ss tt"),
                                DocumentList = db.ElectricMeterDocuments.Where(x => x.ElectricMeterID == etm.Id).Select(x => new { x.DocumentaryRequirement.Name }).ToList(),
                                ModifiedDate = etm.ModifiedDate.ToString("MM/dd/yyyy hh:mm:ss tt"),
                                ModifiedByPK = etm.ModifiedByPK,
                                ReasonForChange = data.ReasonForChange,
                            };

                            log.Description = ((string.IsNullOrEmpty(data.ReasonForChange)) ? "ADD " : "UPDATE ") + this.ApiName + " - Document Requirements & Application Status";
                            log.ContentDetail = JsonConvert.SerializeObject(logObject);
                        }
                        else if (data.Transaction == "RFP Status")
                        {
                            var logObject = new
                            {
                                CompanyCode = etm.CompanyCode,
                                CustomerNos = etm.CustomerNos,
                                ProjectCode = etm.ProjectCode,
                                UnitNos = data.RefNos,
                                UnitCategory = etm.UnitCategory,
                                SalesDocNos = etm.SalesDocNos,
                                QuotDocNos = etm.QuotDocNos,
                                RFPRushTicketNos = etm.RFPRushTicketNos,
                                RFPRushTicketDate = (etm.RFPRushTicketDate == null) ? "" : etm.RFPRushTicketDate.GetValueOrDefault().ToString("MM/dd/yyyy hh:mm:ss tt"),
                                RFPRushTicketRemarks = etm.RFPRushTicketRemarks,
                                IsReceivedCheck = etm.IsReceivedCheck,
                                ReceivedCheckDate = (etm.ReceivedCheckDate == null) ? "" : etm.ReceivedCheckDate.GetValueOrDefault().ToString("MM/dd/yyyy hh:mm:ss tt"),
                                ReceivedCheckRemarks = etm.ReceivedCheckRemarks,
                                ModifiedDate = etm.ModifiedDate.ToString("MM/dd/yyyy hh:mm:ss tt"),
                                ModifiedByPK = etm.ModifiedByPK,
                                ReasonForChange = data.ReasonForChange,
                            };

                            log.Description = ((string.IsNullOrEmpty(data.ReasonForChange)) ? "ADD " : "UPDATE ") + this.ApiName + " - RFP Status";
                            log.ContentDetail = JsonConvert.SerializeObject(logObject);
                        }
                        else if (data.Transaction == "Electric Status")
                        {
                            var logObject = new
                            {
                                CompanyCode = etm.CompanyCode,
                                CustomerNos = etm.CustomerNos,
                                ProjectCode = etm.ProjectCode,
                                UnitNos = data.RefNos,
                                UnitCategory = etm.UnitCategory,
                                SalesDocNos = etm.SalesDocNos,
                                QuotDocNos = etm.QuotDocNos,
                                WithUnpaidBills = etm.WithUnpaidBills,
                                UnpaidBillPostedDate = (etm.UnpaidBillPostedDate == null) ? "" : etm.UnpaidBillPostedDate.GetValueOrDefault().ToString("MM/dd/yyyy hh:mm:ss tt"),
                                IsPaidSettled = etm.IsPaidSettled,
                                PaidSettledPostedDate = (etm.PaidSettledPostedDate == null) ? "" : etm.PaidSettledPostedDate.GetValueOrDefault().ToString("MM/dd/yyyy hh:mm:ss tt"),
                                DepositApplicationRemarks = etm.DepositApplicationRemarks,
                                MeralcoSubmittedDate = (etm.MeralcoSubmittedDate == null) ? "" : etm.MeralcoSubmittedDate.GetValueOrDefault().ToString("MM/dd/yyyy"),
                                MeralcoSubmittedRemarks = etm.MeralcoSubmittedRemarks,
                                MeralcoReceiptDate = (etm.MeralcoReceiptDate == null) ? "" : etm.MeralcoReceiptDate.GetValueOrDefault().ToString("MM/dd/yyyy"),
                                MeralcoReceiptRemarks = etm.MeralcoReceiptRemarks,
                                UnitOwnerReceiptDate = (etm.UnitOwnerReceiptDate == null) ? "" : etm.UnitOwnerReceiptDate.GetValueOrDefault().ToString("MM/dd/yyyy"),
                                UnitOwnerReceiptRemarks = etm.UnitOwnerReceiptRemarks,
                                ModifiedDate = etm.ModifiedDate.ToString("MM/dd/yyyy hh:mm:ss tt"),
                                ModifiedByPK = etm.ModifiedByPK,
                                ReasonForChange = data.ReasonForChange,
                            };

                            log.Description = ((string.IsNullOrEmpty(data.ReasonForChange)) ? "ADD " : "UPDATE ") + this.ApiName + " - Meralco Service Transfer Status";
                            log.ContentDetail = JsonConvert.SerializeObject(logObject);
                        }
                        // ---------------- End Transaction Activity Logs -------------------- //

                        log.SaveTransactionLogs();
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