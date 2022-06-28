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
    [RoutePrefix("api/UnitInspectionAcceptanceMgmnt")]
    public class UnitInspectionAcceptanceMgmntController : ApiController
    {
        private string PageUrl = "/Admin/UnitInspectionAcceptanceMgmnt";
        private string ApiName = "Unit Inspection and Acceptance Management";

        string timezone = "";

        private UnitInspectionAcceptanceMgmntController () 
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

        [Route("GetUnitInspectionAcceptanceMgmnt")]
        public async Task<IHttpActionResult> GetUnitInspectionAcceptanceMgmnt([FromUri] SearchData item)
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
                    var punchlistcategory = await db.PunchlistCategories.Where(x => x.Published == true).Select(x => new { x.Id, x.Name, x.TurnaroundTime, x.CalendarType }).OrderBy(x => x.Id).ToListAsync();

                    // -------------- INSPECTION AND ACCEPTANCE --------------- //
                    // Get List of Qualified for Inspection and Acceptance
                    var source = await db.VW_InspectionAcceptance.Where(x => x.TOAS != null && x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitCategoryCode == item.UnitCategory).ToListAsync();
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
                    var acceptanceInfo = source.FirstOrDefault();

                    // if doesn't exist display error message
                    if (acceptanceInfo == null)
                        return BadRequest("Record not found");
                    
                    // Get List of Turnover Status
                    var turnoverstatus = await db.TurnoverStatus.Where(x => x.Published == true).Select(x => new { x.Id, x.Name, x.Description }).OrderBy(x => x.Name).ToListAsync();
                    turnoverstatus = await (from at in db.TurnoverStatusAcctTypes
                                            join ts in db.TurnoverStatus on at.StatusID equals ts.Id
                                            join op in db.Options on at.OptionID equals op.Id
                                            where ts.Published == true && op.Name == acceptanceInfo.AccountTypeCode
                                            select new
                                            {
                                                ts.Id,
                                                ts.Name,
                                                ts.Description
                                            }).OrderBy(x => x.Name).ToListAsync();
                    // -------------- INSPECTION AND ACCEPTANCE --------------- //

                    // Turnover & Acceptance Status
                    var turnoverAcceptance = await db.UnitID_TOAcceptance.Where(x => x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitCategory == item.UnitCategory && x.UnitNos == acceptanceInfo.UnitNos && x.CustomerNos == acceptanceInfo.CustomerNos).FirstOrDefaultAsync();
                    if(turnoverAcceptance != null)
                    {
                        // If posted turnover status is AWOP and within thirty(30) calendar days from the
                        // Turnover Date field, system will enable the Turnover Status field.
                        if (turnoverAcceptance.TurnoverStatus == "AWOP" && turnoverAcceptance.TOAllowableDate >= DateTime.Now)
                        {
                            string[] arr2 = { "AWOP", "AWP" };
                            turnoverstatus = turnoverstatus.Where(x => arr2.Contains(x.Name)).ToList();
                        }
                    }

                    // Deemed Acceptance Details
                    var deemedAcceptance = await db.UnitID_DeemedAcceptance.Where(x => x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitCategory == item.UnitCategory && x.UnitNos == acceptanceInfo.UnitNos && x.CustomerNos == acceptanceInfo.CustomerNos).FirstOrDefaultAsync();

                    // Client Information
                    IEnumerable<CustomCustomerProfile> clientinfo = await (from csp in db.CustomerProfiles
                                                                           where csp.CustomerNos == acceptanceInfo.CustomerNos
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

                    var data = new { CURUSER = user, EXCEPTIONDAYS = exceptionDays, TURNOVEROPTIONLIST = turnoverstatus, PUNCHLIST = punchlistcategory, ACCEPTANCEINFO = acceptanceInfo, TOACCEPTANCEINFO = turnoverAcceptance, DEEMEDACCEPTANCEINFO = deemedAcceptance, CLIENTINFO = clientinfo, SYSPARAM = systemParameter, CONTROLS = permissionCtrl };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("GetWorkingDate")]
        public async Task<IHttpActionResult> GetWorkingDate([FromUri] int TAT)
        {  
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var sql = "SELECT dbo.fnAdjustmentDate({0}, {1})";
                    string adjDate = await db.Database.SqlQuery<string>(sql, DateTime.Today, TAT + 1).SingleAsync();

                    var data = new { ADJDATE = Convert.ToDateTime(adjDate) };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("SaveTurnoverAcceptance")]
        public async Task<IHttpActionResult> SaveTurnoverAcceptance(CustomUnitID_TOAcceptance data)
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
                        DateTime dt = (data.Id == 0)? DateTime.Now : data.TurnoverStatusDate;

                        // Check if system parameter is properly set
                        var systemParameter = db.SystemParameters.Where(x => x.Published == true).FirstOrDefault();
                        if (systemParameter == null)
                            return BadRequest("Please configure system parameter");

                        int punchlistTAT = 0;
                        string punchlistCT = "CD";
                        string[] arr1 = { "AWP", "NAPL"};
                        if (arr1.Contains(data.TurnoverStatus))
                        {
                            // Punchlist Category Settings
                            var punchlistCategory = db.PunchlistCategories.Where(x => x.Published == true && x.Name == data.PunchlistCategory).FirstOrDefault();
                            if (punchlistCategory == null)
                                return BadRequest("Please configure Punchlist Category");

                            punchlistTAT = punchlistCategory.TurnaroundTime;
                            punchlistCT = punchlistCategory.CalendarType;
                        }                        

                        // If Account Type of posted unit is LTO, the UPDATE and APPLY buttons will be disabled. Transaction cannot be updated anymore and for viewing of user only. 
                        // Reference FRD: Transaction > Unit Inspection & Acceptance Management > Turnover & Acceptance Status (5.4.9.1.1.6)
                        // if (data.AccountTypeCode == "L" && data.TurnoverStatus == "No Show" && data.Id != 0)
                        // return BadRequest("Transaction cannot be updated");

                        UnitID_TOAcceptance toa = new UnitID_TOAcceptance();

                        toa.Id = data.Id;
                        toa.SalesDocNos = data.SalesDocNos;
                        toa.QuotDocNos = data.QuotDocNos;
                        toa.CompanyCode = data.CompanyCode;
                        toa.CustomerNos = data.CustomerNos;
                        toa.ProjectCode = data.ProjectCode;
                        toa.UnitNos = data.UnitNos;
                        toa.UnitCategory = data.UnitCategory;

                        toa.TurnoverStatus = data.TurnoverStatus;
                        toa.PunchlistCategory = data.PunchlistCategory;

                        toa.PunchlistItem = data.PunchlistItem;
                        toa.OtherIssues = data.OtherIssues;
                        toa.TSRemarks = data.TSRemarks;
                        toa.TSAttachment = data.TSAttachment;
                        toa.RushTicketNos = data.RushTicketNos;
                        toa.SRRemarks = data.SRRemarks;

                        if (data.UnitAcceptanceDate != null)
                            toa.UnitAcceptanceDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.UnitAcceptanceDate.GetValueOrDefault().ToLocalTime(), timezone);

                        // -Applicable if posted Turnover Status is NAPL or NAOI
                        // -will turn into red font if no tagging on this fields after 30 days from Turnover Status posted date
                        string[] arr = { "NAPL", "NAOI" };
                        if (arr.Contains(data.TurnoverStatus))
                        {
                            if (systemParameter.UnitAcceptanceTATCT == "CD")
                                toa.UnitAcceptanceDateTAT = dt.AddDays(systemParameter.UnitAcceptanceTAT); // Get only date based on calendar days 
                            else
                                toa.UnitAcceptanceDateTAT = Convert.ToDateTime(db.Database.SqlQuery<string>("SELECT dbo.fnAdjustmentDate({0}, {1})", dt, systemParameter.UnitAcceptanceTAT + 1).Single()); // Get only date based on working days 
                        }

                        if (data.KeyTransmittalDate != null)
                            toa.KeyTransmittalDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.KeyTransmittalDate.GetValueOrDefault().ToLocalTime(), timezone);

                        if (data.UnitAcceptanceDateSyncDate != null)
                            toa.UnitAcceptanceDateSyncDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.UnitAcceptanceDateSyncDate.GetValueOrDefault().ToLocalTime(), timezone);

                        // If posted turnover status is AWOP and within thirty (x) nos days from the
                        // Turnover Date field, system will enable the Turnover Status field.
                        if (systemParameter.EnableTOStatusMaxCT == "CD")
                            toa.TOAllowableDate = data.FinalTurnoverDate.AddDays(systemParameter.EnableTOStatusMax); // Get only date based on calendar days 
                        else
                            toa.TOAllowableDate = Convert.ToDateTime(db.Database.SqlQuery<string>("SELECT dbo.fnAdjustmentDate({0}, {1})", data.FinalTurnoverDate, systemParameter.EnableTOStatusMax + 1).Single()); // Get only date based on working days 

                        // Applicable, if value on Turnover Option field is Standard and selected value on Turnover Status field is NAPL
                        // System-generated date based on the TAT for the selected value on the Punchlist Category field, for NAPL Status       
                        // Reference FRD: Unit Inspection & Acceptance Management > Turnover & Acceptance Status TAB (5.2)  
                        DateTime reinspectDate = DateTime.Today;
                        DateTime punchCategoryTAT = DateTime.Today;
                        if (data.FinalTurnoverOption == "Standard" && toa.TurnoverStatus == "NAPL")
                        {
                            // System Generated Reinspection Date based on Turnover Option = "Standard" and Turnover Status = "NAPL"
                            if (punchlistCT == "CD")
                                reinspectDate = dt.AddDays(punchlistTAT); // Get only date based on calendar days 
                            else
                                reinspectDate = Convert.ToDateTime(db.Database.SqlQuery<string>("SELECT dbo.fnAdjustmentDate({0}, {1})", dt, punchlistTAT + 1).Single()); // Get only date based on working days 

                            toa.ReinspectionDate = reinspectDate;

                            // System-computed calendar days from the posting of Punchlist Items
                            if (systemParameter.PunchlistDateTATCT == "CD")
                                punchCategoryTAT = reinspectDate.AddDays(systemParameter.PunchlistDateTAT); // Get only date based on calendar days 
                            else
                                punchCategoryTAT = Convert.ToDateTime(db.Database.SqlQuery<string>("SELECT dbo.fnAdjustmentDate({0}, {1})", reinspectDate, systemParameter.PunchlistDateTAT + 1).Single()); // Get only date based on working days 

                            toa.PunchlistCategoryTAT = punchCategoryTAT;

                            // Compute Reinspection Date Turnaround Time based on System Parameter
                            DateTime adjTATDate = DateTime.Today;
                            if (systemParameter.ReinspectionDateTATCT == "CD")
                                adjTATDate = reinspectDate.AddDays(systemParameter.ReinspectionDateTAT); // Get only date based on calendar days 
                            else
                                adjTATDate = Convert.ToDateTime(db.Database.SqlQuery<string>("SELECT dbo.fnAdjustmentDate({0}, {1})", reinspectDate, systemParameter.ReinspectionDateTAT + 1).Single()); // Get only date based on working days 

                            toa.ReinspectionDateTAT = adjTATDate;

                            // Compute Reinspection Date Turnaround Time from Reinspection Date
                            if (systemParameter.AdjReinspectionTATCT == "CD")
                                toa.AdjReinspectionDateTAT = reinspectDate.AddDays(systemParameter.AdjReinspectionTAT); // Get only date based on calendar days 
                            else
                                toa.AdjReinspectionDateTAT = Convert.ToDateTime(db.Database.SqlQuery<string>("SELECT dbo.fnAdjustmentDate({0}, {1})", reinspectDate, systemParameter.AdjReinspectionTAT + 1).Single()); // Get only date based on working days 
   
                            // Adj. Re-inspection date should not be more than x (calendar Type) days from Reinspection Date
                            if (systemParameter.AdjReinspectionMaxDaysCT == "CD")
                                toa.AdjReinspectionMaxDate = reinspectDate.AddDays(systemParameter.AdjReinspectionMaxDays); // Get only date based on calendar days 
                            else
                                toa.AdjReinspectionMaxDate = Convert.ToDateTime(db.Database.SqlQuery<string>("SELECT dbo.fnAdjustmentDate({0}, {1})", reinspectDate, systemParameter.AdjReinspectionMaxDays + 1).Single()); // Get only date based on working days 
                            

                            List<DateTime> listDates = new List<DateTime>();
                            if (data.AdjReinspectionDate != null)
                            {
                                DateTime AdjReinspectionDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.AdjReinspectionDate.GetValueOrDefault().ToLocalTime(), timezone);
                                toa.AdjReinspectionDate = AdjReinspectionDate;
                                listDates.Add(AdjReinspectionDate);
                            }
                            listDates.Add(reinspectDate);

                            // For unit with Turnover Status as NAPL and Unit   Acceptance Date field was not tagged/ posted within x (calendar Type) days from the Re-inspection Date or Adjusted Re-inspection Date, whichever comes later
                            DateTime deemDates2 = listDates.Max(p => p);
                            if (systemParameter.DeemedDaysTAT2CT == "CD")
                                toa.DeemedDateTAT2 = deemDates2.AddDays(systemParameter.DeemedDaysTAT2); // Get only date based on calendar days 
                            else
                                toa.DeemedDateTAT2 = Convert.ToDateTime(db.Database.SqlQuery<string>("SELECT dbo.fnAdjustmentDate({0}, {1})", deemDates2, systemParameter.DeemedDaysTAT2 + 1).Single()); // Get only date based on working days 

                        }

                        // Enable if selected value on Turnover Status field is AWP, NAPL or NAOI
                        // For Getting RushTicketNosTAT
                        if (data.FinalTurnoverOption != "Express")
                        {
                            string[] arr2 = { "AWP", "NAPL", "NAOI" };
                            if (arr2.Contains(data.TurnoverStatus))
                            {
                                if (systemParameter.RushTicketMaxTATCT == "CD")
                                    toa.RushTicketNosTAT = dt.AddDays(systemParameter.RushTicketMaxTAT); // Get only date based on calendar days 
                                else
                                    toa.RushTicketNosTAT = Convert.ToDateTime(db.Database.SqlQuery<string>("SELECT dbo.fnAdjustmentDate({0}, {1})", dt, systemParameter.RushTicketMaxTAT + 1).Single()); // Get only date based on working days 
                            }
                        }

                        // For Regular and In-house Financing (IHF) account types, units’ Turnover Status is tagged as No Show. Not applicable for LTO accounts.
                        if (data.AccountTypeCode != "L" && data.TurnoverStatus == "No Show")
                        {
                            // Deemed Acceptance date should not be more than x (calendar Type) days from Turnover Date
                            DateTime deemDate1 = DateTime.Today;
                            if (systemParameter.DeemedDaysTAT1CT == "CD")
                                deemDate1 = data.FinalTurnoverDate.AddDays(systemParameter.DeemedDaysTAT1 + 1); // Get only date based on calendar days 
                            else
                                deemDate1 = Convert.ToDateTime(db.Database.SqlQuery<string>("SELECT dbo.fnAdjustmentDate({0}, {1})", data.FinalTurnoverDate, systemParameter.DeemedDaysTAT1 + 2).Single()); // Get only date based on working days 

                            toa.DeemedDateTAT1 = deemDate1;
                        }                        

                        toa.ModifiedByPK = cId;
                        toa.ModifiedDate = dt;

                        if (toa.Id == 0)
                        {
                            toa.CreatedByPK = cId;
                            toa.CreatedDate = toa.TurnoverStatusDate = dt;

                            // We need to check if the criteria has met
                            // On Rescheduled / Changed Turnover Schedule & Option: NO reschedule or change on the
                            // Turnover Schedule and / or Turnover Option after (x) nos days from the Date of E-mail Notice Sent
                            // if Rescheduled / Changed Turnover Schedule & Option has value and TAT has met (TORule2) then the value is 1 (indicates that final schedule has me) else 0
                            toa.IsUnitAcceptanceDateSAPSync = 0; //(data.TORule2 == 1 && data.AccountTypeCode != "L")? 0 : 2; // 0 means ready for SAP Sync, 2 means wait for Business Rule Trigger (TORule 2)

                            db.UnitID_TOAcceptance.Add(toa);
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            toa.CreatedByPK = data.CreatedByPK;
                            toa.CreatedDate = data.CreatedDate;
                            toa.TurnoverStatusDate = data.TurnoverStatusDate;
                            toa.IsUnitAcceptanceDateSAPSync = 0; //(data.TORule2 == 1 && data.AccountTypeCode != "L") ? 0 : 2;

                            // If posted turnover status is AWOP and within thirty (30) calendar days from the
                            // Turnover Date field, system will enable the Turnover Status field.
                            // Reference FRD: Transaction > Unit Inspection & Acceptance Management > Turnover & Acceptance Status (5.5.3.1)
                            var toAcceptance = db.VW_InspectionAcceptance.Where(x => x.TOAS != null && x.TranClass == "Business Rule 1" && x.TOAcceptanceID == data.Id && x.AllowedTOStatusUpdate == 0).FirstOrDefault();
                            
                            // Business Rule with SAP Cut-off Date based on System Parameter
                            if (systemParameter.EnableTOCutOffDate == false)
                                toAcceptance = db.VW_InspectionAcceptance.Where(x => x.TOAS != null && x.TOAcceptanceID == data.Id && x.AllowedTOStatusUpdate == 0).FirstOrDefault();

                            if (toAcceptance != null)
                            {
                                string[] arr2 = { "AWOP", "AWP"};
                                if (toAcceptance.TurnoverStatus == "AWOP" && !arr2.Contains(data.TurnoverStatus))
                                    return BadRequest(data.TurnoverStatus + " is not allowed TO Status");
                            }

                            db.Entry(toa).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "UPDATE";
                        log.Description = "Update " + this.ApiName + " - Turnover and Acceptance Status";
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.EventName = this.ApiName;
                        log.ContentDetail = JsonConvert.SerializeObject(toa);
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

        [Route("SaveDeemedAcceptance")]
        public async Task<IHttpActionResult> SaveDeemedAcceptance(CustomUnitID_DeemedAcceptance data)
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

                        UnitID_DeemedAcceptance dap = new UnitID_DeemedAcceptance();

                        dap.Id = data.Id;
                        dap.SalesDocNos = data.SalesDocNos;
                        dap.QuotDocNos = data.QuotDocNos;
                        dap.CompanyCode = data.CompanyCode;
                        dap.CustomerNos = data.CustomerNos;
                        dap.ProjectCode = data.ProjectCode;
                        dap.UnitNos = data.UnitNos;
                        dap.UnitCategory = data.UnitCategory;

                        dap.EmailDateNoticeSent = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.EmailDateNoticeSent, timezone);
                        dap.EmailNoticeRemarks = data.EmailNoticeRemarks;
                        dap.EmailNoticeAttachment = data.EmailNoticeAttachment;

                        if (data.CourierDateNoticeSent != null)
                            dap.CourierDateNoticeSent = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.CourierDateNoticeSent.GetValueOrDefault().ToLocalTime(), timezone);

                        if (data.CourierDateNoticeReceived != null)
                            dap.CourierDateNoticeReceived = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.CourierDateNoticeReceived.GetValueOrDefault().ToLocalTime(), timezone);

                        dap.CourierReceivedBy = data.CourierReceivedBy;
                        dap.CourierNoticeRemarks = data.CourierNoticeRemarks;
                        dap.CourierNoticeAttachment = data.CourierNoticeAttachment;
                        dap.ModifiedByPK = cId;
                        dap.ModifiedDate = DateTime.Now;
                        dap.HandoverAssociate = data.HandoverAssociate;

                        if (dap.Id == 0)
                        {
                            dap.CreatedByPK = cId;
                            dap.CreatedDate = DateTime.Now;
                            dap.IsDeemedAcceptanceDateSAPSync = 0;

                            db.UnitID_DeemedAcceptance.Add(dap);
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            dap.CreatedByPK = data.CreatedByPK;
                            dap.CreatedDate = data.CreatedDate;
                            dap.IsDeemedAcceptanceDateSAPSync = data.IsDeemedAcceptanceDateSAPSync;

                            if (data.DeemedAcceptanceDate != null)
                                dap.DeemedAcceptanceDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.DeemedAcceptanceDate.GetValueOrDefault().ToLocalTime(), timezone);

                            if (data.EmailDateNoticeSentMaxDate != null)
                                dap.EmailDateNoticeSentMaxDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.EmailDateNoticeSentMaxDate.GetValueOrDefault().ToLocalTime(), timezone);

                            if (data.DeemedAcceptanceDateSyncDate != null)
                                dap.DeemedAcceptanceDateSyncDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.DeemedAcceptanceDateSyncDate.GetValueOrDefault().ToLocalTime(), timezone);

                            db.Entry(dap).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "UPDATE";
                        log.Description = "Update " + this.ApiName + " - Deemed Acceptance Details";
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.EventName = this.ApiName;
                        log.ContentDetail = JsonConvert.SerializeObject(dap);
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