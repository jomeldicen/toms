using WebApp.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Collections.Generic;
using Microsoft.Reporting.WebForms;
using Newtonsoft.Json;

namespace WebApp.Controllers
{
    public class ReportController : Controller
    {
        private string RedirectAction { get; set; }

        [CustomAuthorize]
        public bool isAuthorized(string action)
        {
            using (WebAppEntities dbcon = new WebAppEntities())
            {
                var currentUserId = User.Identity.GetUserId();
                if (currentUserId == null)
                    return false;
                else
                {
                    var roleId = dbcon.AspNetUserRoles.Where(x => x.UserId == currentUserId).FirstOrDefault().RoleId;
                    var url = dbcon.AspNetRoles.Where(x => x.Id == roleId).FirstOrDefault().IndexPage;
                    RedirectAction = url.Split('/')[2];
                    action = "/Admin/" + action;
                    bool isUserAuthorized = dbcon.AspNetUsersMenuPermissions.Where(x => x.AspNetUsersMenu.nvPageUrl == action && x.Id == roleId).Any();
                    if (isUserAuthorized)
                    {
                        VisitCount();
                        return true;
                    }
                    else
                        return false;
                }
            }
        }

        public string GetModuleName()
        {
            using (WebAppEntities dbcon = new WebAppEntities())
            {
                try
                {
                    var action = string.Concat("/Admin/", ControllerContext.RouteData.Values["action"].ToString());
                    var menu = dbcon.AspNetUsersMenus.Where(x => x.nvPageUrl == action).FirstOrDefault();
                    if (menu != null) return menu.nvMenuName;

                    return "";
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        [CustomAuthorize]
        public void VisitCount()
        {
            using (WebAppEntities dbcon = new WebAppEntities())
            {
                using (var dbContextTransaction = dbcon.Database.BeginTransaction())
                {
                    try
                    {
                        var path = System.Web.HttpContext.Current.Request.Url.AbsolutePath;
                        string ip = "";
                        System.Web.HttpContext cont = System.Web.HttpContext.Current;
                        string ipAddress = cont.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                        if (!string.IsNullOrEmpty(ipAddress))
                        {
                            string[] addresses = ipAddress.Split(',');
                            if (addresses.Length != 0)
                            {
                                ip = addresses[0];
                            }
                        }
                        ip = cont.Request.ServerVariables["REMOTE_ADDR"];
                        AspNetUsersPageVisited anupv = new AspNetUsersPageVisited();
                        anupv.vPageVisitedID = Guid.NewGuid().ToString();
                        anupv.Id = User.Identity.GetUserId();
                        anupv.nvPageName = path;
                        anupv.dDateVisited = DateTime.UtcNow;
                        anupv.nvIPAddress = ip;
                        dbcon.AspNetUsersPageVisiteds.Add(anupv);
                        dbcon.SaveChanges();
                        dbContextTransaction.Commit();
                    }
                    catch
                    {
                        dbContextTransaction.Rollback();
                    }
                }
            }
        }


        protected string LinkRedirect(string addr, int df)
        {
            if (string.IsNullOrEmpty(addr))
            {
                if (df == 1)
                    addr = "/Auth/Login";
                else
                    addr = "/BadRequest";
            }
            return addr;
        }

        public ActionResult ExportReport(string rep, string contype, string json)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    DateTime dt = DateTime.Today;

                    var arr = JsonConvert.DeserializeObject<ControllerParam[]>(json).SingleOrDefault();
                    
                    // object is used to load and export a report
                    LocalReport localReport = new LocalReport();
                    string fileName = "";
                    string reportType = "";
                    string extension = "";

                    // classification of Report Type
                    if (contype == "001x") {
                        reportType = "EXCEL";
                        extension = ".xls";
                    } else if (contype == "002p") {
                        reportType = "PDF";
                        extension = ".pdf";
                    }

                    // Check if system parameter is properly set
                    var systemParameter = db.SystemParameters.Where(x => x.Published == true).FirstOrDefault();
                    if (systemParameter == null)
                        return View("~/Views/Admin/BadRequest.cshtml");

                    // Report for TO Pipeline Report                     
                    if (rep == "t1xh8G40aeqxX02312llkh")
                    {
                        var ProjectCode = arr.param1.Split('|').Select(Int32.Parse).ToList();

                        // Get Selected Projects
                        if (ProjectCode == null || ProjectCode.Count == 0)
                            ProjectCode = new List<int> { 0 };

                        var projCodes = db.VW_Projects.Where(x => ProjectCode.Contains(x.Id)).Select(x => x.CompanyCode + "-" + x.ProjectCode).ToArray();
                        if ((projCodes == null || projCodes.Count() == 0) && string.IsNullOrWhiteSpace(arr.param2))
                            return View("~/Views/Admin/BadRequest.cshtml");

                        IEnumerable<CustomerDashboard_Pipeline> source = null;
                        var pipeline = (from qt in db.VW_QualifiedTurnover
                                        where (qt.TOAS != null) // && qt.FinalTurnoverDate >= DateTime.Today || qt.FinalTurnoverDate == null)
                                        orderby qt.EmailNoticeSentAging descending, qt.Phase, qt.RefNos, qt.EmailDateNoticeSent, qt.FinalTurnoverDate
                                        select new
                                        {
                                            qt.CompanyCode,
                                            qt.UnitNos,
                                            qt.ProjectCode,
                                            qt.RefNos,
                                            qt.BusinessEntity,
                                            qt.UnitCategoryDesc,
                                            qt.UnitTypeDesc,
                                            qt.CustomerNos,
                                            qt.CustomerName1,
                                            qt.AccountTypeDesc,
                                            qt.EmailAdd,
                                            qt.ContactPerson,
                                            qt.TelNos,
                                            qt.QCDAcceptanceDate,
                                            qt.FPMCAcceptanceDate,
                                            qt.HasOccupancyPermit,
                                            qt.TOAS,
                                            qt.HandoverAssociate,
                                            qt.EmailNoticeSentAging,
                                            qt.EmailNoticeSentAgingDays,
                                            qt.EmailDateNoticeSent,
                                            qt.CourierDateNoticeSent,
                                            qt.CourierDateNoticeReceived,
                                            qt.CourierReceivedBy,
                                            qt.EmailTurnoverDate,
                                            qt.FinalTurnoverDate,
                                            qt.FinalTurnoverTime,
                                            qt.FinalTurnoverOption,
                                            qt.QualificationDate,
                                            qt.ScheduleEmailNotifDate1,
                                            qt.EmailNoticeNotifDate2,
                                            qt.TurnoverDate1,
                                            qt.TranClass
                                        }).ToList();

                        // Business Rule with SAP Cut-off Date based on System Parameter
                        if (systemParameter.EnableTOCutOffDate == true)
                            pipeline = pipeline.Where(x => x.TranClass == "Business Rule 1").ToList();

                        switch (arr.param2)
                        {
                            case "Pwt": //Processing within TAT
                                pipeline = pipeline.Where(x => x.EmailNoticeNotifDate2 > dt && x.EmailDateNoticeSent == null).ToList();
                                break;
                            case "Pbt": //Processing beyond TAT
                                pipeline = pipeline.Where(x => x.EmailNoticeNotifDate2 <= dt && x.EmailDateNoticeSent == null).ToList();
                                break;
                            case "Ptl": //Qualified within TAT (Pwt + Pbt)
                                pipeline = pipeline.Where(x => (x.EmailNoticeNotifDate2 > dt && x.EmailDateNoticeSent == null) || (x.EmailNoticeNotifDate2 <= dt && x.EmailDateNoticeSent == null)).ToList();
                                break;

                            case "Cwt": //Confirmed within TAT
                                pipeline = pipeline.Where(x => x.ScheduleEmailNotifDate1 > dt && x.EmailDateNoticeSent != null && x.TurnoverDate1 == null).ToList(); // && x.TurnoverDate1 != null && x.TurnoverDate1 >= dt
                                break;
                            case "Cbt": //Confirmed beyond TAT
                                pipeline = pipeline.Where(x => x.ScheduleEmailNotifDate1 <= dt && x.EmailDateNoticeSent != null && x.TurnoverDate1 == null).ToList();
                                break;
                            case "Ctl": //Qualified beyond TAT (Cwt + Cbt)
                                pipeline = pipeline.Where(x => (x.ScheduleEmailNotifDate1 > dt && x.EmailDateNoticeSent != null && x.TurnoverDate1 == null) || (x.ScheduleEmailNotifDate1 <= dt && x.EmailDateNoticeSent != null && x.TurnoverDate1 == null)).ToList();
                                break;

                            case "Qwt": //Qualified within TAT (Pwt + Cwt)
                                pipeline = pipeline.Where(x => (x.EmailNoticeNotifDate2 > dt && x.EmailDateNoticeSent == null) || (x.ScheduleEmailNotifDate1 > dt && x.TurnoverDate1 != null && x.TurnoverDate1 >= dt)).ToList();
                                break;
                            case "Qbt": //Qualified beyond TAT (Pbt + Cbt)
                                pipeline = pipeline.Where(x => (x.EmailNoticeNotifDate2 <= dt && x.EmailDateNoticeSent == null) || (x.ScheduleEmailNotifDate1 <= dt && x.TurnoverDate1 != null && x.TurnoverDate1 >= dt)).ToList();
                                break;
                            case "Qtl": //Qualified within TAT (Qwt + Qbt)
                                pipeline = pipeline.Where(x => ((x.EmailNoticeNotifDate2 > dt && x.EmailDateNoticeSent == null) || (x.ScheduleEmailNotifDate1 > dt && x.TurnoverDate1 != null && x.TurnoverDate1 >= dt)) || ((x.EmailNoticeNotifDate2 < dt && x.EmailDateNoticeSent == null) || (x.ScheduleEmailNotifDate1 < dt && x.TurnoverDate1 != null && x.TurnoverDate1 >= dt))).ToList();
                                break;
                            default:
                                pipeline = pipeline.Where(x => projCodes.Contains(x.CompanyCode + "-" + x.ProjectCode)).ToList();
                                break;
                        }

                        source = pipeline.Select(x => new CustomerDashboard_Pipeline
                        {
                            CompanyCode = x.CompanyCode,
                            ProjectCode = x.ProjectCode,
                            RefNos = x.RefNos,
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
                            EmailDateNoticeSent = (x.EmailDateNoticeSent == null) ? null : x.EmailDateNoticeSent,
                            CourierDateNoticeSent = (x.CourierDateNoticeSent == null) ? null : x.CourierDateNoticeSent,
                            CourierDateNoticeReceived = (x.CourierDateNoticeReceived == null) ? null : x.CourierDateNoticeReceived,
                            CourierReceivedBy = x.CourierReceivedBy,
                            EmailTurnoverDate = (x.EmailTurnoverDate == null) ? "Null" : x.EmailTurnoverDate.GetValueOrDefault().ToString("MM/dd/yyyy"),
                            FinalTurnoverDate = (x.FinalTurnoverDate == null) ? "Null" : x.FinalTurnoverDate.GetValueOrDefault().ToString("MM/dd/yyyy"),
                            FinalTurnoverTime = (x.FinalTurnoverDate == null) ? "Null" : x.FinalTurnoverDate.Value.Add(x.FinalTurnoverTime.Value).ToString("hh:mm a"),
                            FinalTurnoverOption = x.FinalTurnoverOption,
                            QualificationDate = x.QualificationDate
                        }).AsEnumerable();

                        //searching
                        if (!string.IsNullOrWhiteSpace(arr.param4))
                        {
                            arr.param4 = arr.param4.ToLower();
                            // Search by on selected column
                            switch (arr.param3)
                            {
                                case "1": // Project Code and Name
                                    source = source.Where(x => x.ProjectCode.ToLower().Contains(arr.param4) || x.BusinessEntity.ToLower().Contains(arr.param4));
                                    break;
                                case "2": // Unit Category
                                    source = source.Where(x => x.UnitCategoryDesc.ToLower().Contains(arr.param4));
                                    break;
                                case "3": // Unit Type
                                    source = source.Where(x => x.UnitTypeDesc.ToLower().Contains(arr.param4));
                                    break;
                                case "4": // Unit Type
                                    source = source.Where(x => x.RefNos.ToLower().Contains(arr.param4));
                                    break;
                                case "5": // Customer Name
                                    source = source.Where(x => x.CustomerNos.ToLower().Contains(arr.param4) || x.CustomerName1.ToLower().Contains(arr.param4));
                                    break;
                                case "6": // Account Type
                                    source = source.Where(x => x.AccountTypeDesc.ToLower().Contains(arr.param4));
                                    break;
                                case "7": // QCD Date
                                    source = source.Where(x => (x.QCDAcceptanceDate != null && x.QCDAcceptanceDate.ToString() == arr.param4) || (x.FPMCAcceptanceDate != null && x.FPMCAcceptanceDate.ToString() == arr.param4));
                                    break;
                                case "8": // TOAS Date
                                    source = source.Where(x => x.TOAS != null && x.TOAS.GetValueOrDefault().ToString("MM/dd/yyyy") == arr.param4);
                                    break;
                                case "9": // Handover Associate
                                    source = source.Where(x => x.HandoverAssociate != null && x.HandoverAssociate.ToLower().Contains(arr.param4));
                                    break;
                                case "10": // Email Notice Date Sent
                                    source = source.Where(x => x.EmailDateNoticeSent != null && x.EmailDateNoticeSent.ToString() == arr.param4);
                                    break;
                                case "11": // Email Notice Sent Aging
                                    source = source.Where(x => x.EmailNoticeSentAging.ToString() == arr.param4);
                                    break;
                                case "12": // Final turnover date
                                    source = source.Where(x => x.FinalTurnoverDate != null && x.FinalTurnoverDate == arr.param4);
                                    break;
                                case "13": // Final turnover option
                                    source = source.Where(x => x.FinalTurnoverOption != null && x.FinalTurnoverOption.ToLower().Contains(arr.param4));
                                    break;
                                default:
                                    source = source.Where(x => x.ProjectCode.ToLower().Contains(arr.param4) || x.BusinessEntity.ToLower().Contains(arr.param4) || x.UnitCategoryDesc.ToLower().Contains(arr.param4) || x.UnitTypeDesc.ToLower().Contains(arr.param4) ||
                                                        x.RefNos.ToLower().Contains(arr.param4) || x.CustomerNos.ToLower().Contains(arr.param4) || x.CustomerName1.ToLower().Contains(arr.param4) || x.AccountTypeDesc.ToLower().Contains(arr.param4) ||
                                                        (x.HandoverAssociate != null && x.HandoverAssociate.ToLower().Contains(arr.param4)) || (x.FinalTurnoverOption != null && x.FinalTurnoverOption.ToLower().Contains(arr.param4)) ||
                                                        (x.QCDAcceptanceDate != null && x.QCDAcceptanceDate.ToString() == arr.param4) || (x.FPMCAcceptanceDate != null && x.FPMCAcceptanceDate.ToString() == arr.param4) ||
                                                        (x.TOAS != null && x.TOAS.GetValueOrDefault().ToString("MM/dd/yyyy") == arr.param4) || (x.EmailDateNoticeSent != null && x.EmailDateNoticeSent.ToString() == arr.param4) ||
                                                        (x.FinalTurnoverDate != null && x.FinalTurnoverDate == arr.param4));
                                    break;
                            }
                        }

                        source = source.OrderByDescending(s => s.EmailNoticeSentAging).ThenBy(s => s.Phase).ThenBy(s => s.RefNos).ThenBy(s => s.EmailDateNoticeSent).ThenBy(s => s.FinalTurnoverDate);

                        // Report Path and DataSource Configuration
                        localReport.ReportPath = @"Reports/Rdlc/TOPipelineDashboard1.rdlc";
                        localReport.DataSources.Clear();
                        localReport.DataSources.Add(new ReportDataSource("DSTOPipelineDashboard", source));

                        fileName = String.Concat("TOPipeline", dt.ToString("yyyyMMdd"), extension);
                    }
                    // Report for TO Schedule Report    
                    else if (rep == "t2xf1F10jklxM30923llkj")
                    {
                        string[] HandoverAssoc = arr.param1.Split('|');
                        string[] AccountTypeCode = arr.param2.Split('|');
                        string[] FinalTurnoverOption = arr.param3.Split('|');

                        IEnumerable<CustomDashboard_TOSchedule> source = null;
                        var toSchedule = (from qt in db.VW_QualifiedTurnover
                                          where qt.TOAS != null && qt.FinalTurnoverOption != null && HandoverAssoc.Contains(qt.HandoverAssociate) &&
                                          AccountTypeCode.Contains(qt.AccountTypeCode) && FinalTurnoverOption.Contains(qt.FinalTurnoverOption) &&
                                          (qt.FinalTurnoverDate >= arr.dt1.Date && qt.FinalTurnoverDate <= arr.dt2.Date)
                                          orderby qt.FinalTurnoverDate, qt.FinalTurnoverOption, qt.AccountTypeDesc, qt.BusinessEntity, qt.Phase, qt.RefNos, qt.HandoverAssociate
                                          select new
                                          {
                                              qt.FinalTurnoverDate,
                                              qt.FinalTurnoverTime,
                                              qt.FinalTurnoverOption,
                                              qt.CustomerNos,
                                              qt.CustomerName1,
                                              qt.UnitType,
                                              qt.ProjectCode,
                                              qt.BusinessEntity,
                                              qt.RefNos,
                                              qt.Phase,
                                              qt.HandoverAssociate,
                                              qt.AccountTypeDesc,
                                              qt.TranClass
                                          }).ToList();

                        // Business Rule with SAP Cut-off Date based on System Parameter
                        if (systemParameter.EnableTOCutOffDate == true)
                            toSchedule = toSchedule.Where(x => x.TranClass == "Business Rule 1").ToList();

                        source = toSchedule.Select(x => new CustomDashboard_TOSchedule
                        {
                            FinalTurnoverDate = x.FinalTurnoverDate.Value.Add(x.FinalTurnoverTime.Value),
                            FinalTurnoverOption = x.FinalTurnoverOption,
                            CustomerNos = x.CustomerNos,
                            CustomerName = x.CustomerName1,
                            UnitType = x.UnitType,
                            ProjectCode = x.ProjectCode,
                            BusinessEntity = x.BusinessEntity,
                            RefNos = x.RefNos,
                            HandoverAssociate = x.HandoverAssociate,
                            AccountTypeDesc = x.AccountTypeDesc
                        }).AsEnumerable();

                        //searching
                        if (!string.IsNullOrWhiteSpace(arr.param4))
                        {
                            arr.param4 = arr.param4.ToLower();
                            source = source.Where(x => x.ProjectCode.ToLower().Contains(arr.param4) || x.RefNos.ToLower().Contains(arr.param4) ||
                                                x.UnitType.ToLower().Contains(arr.param4) || x.CustomerNos.ToLower().Contains(arr.param4) ||
                                                x.CustomerName.ToLower().Contains(arr.param4) || x.FinalTurnoverOption.ToLower().Contains(arr.param4) ||
                                                x.AccountTypeDesc.ToLower().Contains(arr.param4) || x.HandoverAssociate.ToLower().Contains(arr.param4));
                        }

                        source = source.OrderBy(s => s.FinalTurnoverDate).ThenBy(s => s.FinalTurnoverOption).ThenBy(s => s.AccountTypeDesc).ThenBy(s => s.BusinessEntity).ThenBy(s => s.Phase).ThenBy(s => s.RefNos).ThenBy(s => s.HandoverAssociate);

                        // Report Path and DataSource Configuration
                        localReport.ReportPath = @"Reports/Rdlc/TOScheduleDashboard.rdlc";
                        localReport.DataSources.Clear();
                        localReport.DataSources.Add(new ReportDataSource("DSTOScheduleDashboard", source));

                        fileName = String.Concat("TOSchedule", dt.ToString("yyyyMMdd"), extension);
                    }
                    // Report for TO Status Report    
                    else if (rep == "t3xf2E20iklxL40123lssj")
                    {
                        var ProjectCode = arr.param1.Split('|').Select(Int32.Parse).ToList();

                        // Get Selected Projects
                        if (ProjectCode == null || ProjectCode.Count == 0)
                            ProjectCode = new List<int> { 0 };

                        var projCodes = db.VW_Projects.Where(x => ProjectCode.Contains(x.Id)).Select(x => x.CompanyCode + "-" + x.ProjectCode).ToArray();
                        if ((projCodes == null || projCodes.Count() == 0) && string.IsNullOrWhiteSpace(arr.param2))
                            return View("~/Views/Admin/BadRequest.cshtml");

                        IEnumerable<CustomerDashboard_Status> source = null;
                        var status = (from qt in db.VW_QualifiedTurnover
                                    where (qt.TOAS != null && qt.FinalTurnoverDate != null && qt.FinalTurnoverOption != null)
                                    orderby qt.TurnoverStatus, qt.TOStatusAging descending, qt.DAEmailAging descending, qt.PunchlistAging descending
                                    select new
                                    {
                                        qt.CompanyCode,
                                        qt.UnitNos,
                                        qt.ProjectCode,
                                        qt.RefNos,
                                        qt.BusinessEntity,
                                        qt.UnitCategoryCode,
                                        qt.UnitCategoryDesc,
                                        qt.UnitTypeDesc,
                                        qt.CustomerNos,
                                        qt.CustomerName1,
                                        qt.AccountTypeCode,
                                        qt.AccountTypeDesc,
                                        qt.EmailAdd,
                                        qt.ContactPerson,
                                        qt.TelNos,
                                        qt.QCDAcceptanceDate,
                                        qt.FPMCAcceptanceDate,
                                        qt.HasOccupancyPermit,
                                        qt.TOAS,
                                        qt.QualificationDate,
                                        qt.HandoverAssociate,
                                        qt.FinalTurnoverDate,
                                        qt.FinalTurnoverTime,
                                        qt.FinalTurnoverOption,
                                        qt.TurnoverStatus,
                                        qt.PunchlistCategory,
                                        qt.PunchlistItem,
                                        qt.OtherIssues,
                                        qt.RushTicketNos,
                                        qt.ReinspectionDate,
                                        qt.UnitAcceptanceDate,
                                        qt.KeyTransmittalDate,
                                        qt.DeemedAcceptanceDate,
                                        qt.DAEmailDateNoticeSent,
                                        qt.DACourierDateNoticeSent,
                                        qt.DACourierDateNoticeReceived,
                                        qt.DACourierReceivedBy,
                                        qt.TurnoverStatusTAT,
                                        qt.DAEmailDateNoticeSentMaxDate,
                                        qt.LastReinspectionDate,
                                        qt.LastReinspectionDateTAT,
                                        qt.TurnoverStatusTATNos,
                                        qt.PunchlistDateTATNos,
                                        qt.DeemedEmailDateSentTATNos,
                                        qt.TOStatusAging,
                                        qt.PunchlistAging,
                                        qt.DAEmailAging,
                                        qt.TranClass,
                                        qt.TurnoverStatusDate,
                                        qt.UnitAcceptanceDateTAT
                                    }).ToList();

                        // Business Rule with SAP Cut-off Date based on System Parameter
                        if (systemParameter.EnableTOCutOffDate == true)
                            status = status.Where(x => x.TranClass == "Business Rule 1").ToList();

                        switch (arr.param2)
                        {
                            case "Ttl": //For TO Status TAT (Twt + Tbt)
                                status = status.Where(x => (x.TurnoverStatusTAT > dt && (x.TurnoverStatus == null || x.TurnoverStatus == "")) || (x.TurnoverStatusTAT <= dt && (x.TurnoverStatus == null || x.TurnoverStatus == ""))).ToList();
                                break;
                            case "Twt": //For TO Status within TAT
                                status = status.Where(x => x.TurnoverStatusTAT > dt && (x.TurnoverStatus == null || x.TurnoverStatus == "")).ToList();
                                break;
                            case "Tbt": //For TO Status beyond TAT
                                status = status.Where(x => x.TurnoverStatusTAT <= dt && (x.TurnoverStatus == null || x.TurnoverStatus == "")).ToList();
                                break;
                            case "Rtl": //For Re-Inspections TAT (Rwt + Rbt)
                                status = status.Where(x => (((x.TurnoverStatus == "NAPL" && x.LastReinspectionDateTAT > dt) || (x.UnitAcceptanceDate == null && x.UnitAcceptanceDateTAT > dt)) && !String.IsNullOrWhiteSpace(x.TurnoverStatus) && x.LastReinspectionDate == null && x.UnitAcceptanceDate == null && x.DeemedAcceptanceDate == null) || (((x.TurnoverStatus == "NAPL" && x.LastReinspectionDateTAT <= dt) || (x.UnitAcceptanceDate == null && x.UnitAcceptanceDateTAT <= dt)) && !String.IsNullOrWhiteSpace(x.TurnoverStatus) && x.LastReinspectionDate == null && x.UnitAcceptanceDate == null && x.DeemedAcceptanceDate == null)).ToList();
                                break;
                            case "Rwt": //For Re-Inspection within TAT
                                status = status.Where(x => ((x.TurnoverStatus == "NAPL" && x.LastReinspectionDateTAT > dt) || (x.UnitAcceptanceDate == null && x.UnitAcceptanceDateTAT > dt)) && !String.IsNullOrWhiteSpace(x.TurnoverStatus) && x.LastReinspectionDate == null && x.UnitAcceptanceDate == null && x.DeemedAcceptanceDate == null).ToList();
                                break;
                            case "Rbt": //For Re-Inspection beyond TAT
                                status = status.Where(x => ((x.TurnoverStatus == "NAPL" && x.LastReinspectionDateTAT <= dt) || (x.UnitAcceptanceDate == null && x.UnitAcceptanceDateTAT <= dt)) && !String.IsNullOrWhiteSpace(x.TurnoverStatus) && x.LastReinspectionDate == null && x.UnitAcceptanceDate == null && x.DeemedAcceptanceDate == null).ToList();
                                break;
                            case "Dtl": //For DA Processing TAT (Dwt + Dbt)
                                status = status.Where(x => (x.DAEmailDateNoticeSentMaxDate > dt && x.DeemedAcceptanceDate != null && x.DAEmailDateNoticeSent == null) || (x.DAEmailDateNoticeSentMaxDate <= dt && x.DeemedAcceptanceDate != null && x.DAEmailDateNoticeSent == null)).ToList();
                                break;
                            case "Dwt": //For DA Processing within TAT
                                status = status.Where(x => x.DAEmailDateNoticeSentMaxDate > dt && x.DeemedAcceptanceDate != null && x.DAEmailDateNoticeSent == null).ToList();
                                break;
                            case "Dbt": //For DA Processing beyond TAT
                                status = status.Where(x => x.DAEmailDateNoticeSentMaxDate <= dt && x.DeemedAcceptanceDate != null && x.DAEmailDateNoticeSent == null).ToList();
                                break;
                            default:
                                status = status.Where(x => projCodes.Contains(x.CompanyCode + "-" + x.ProjectCode)).ToList();
                                break;
                        }

                        source = status.Select(x => new CustomerDashboard_Status
                        {
                            CompanyCode = x.CompanyCode,
                            ProjectCode = x.ProjectCode,
                            RefNos = x.RefNos,
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
                            DAEmailDateNoticeSent = (x.DAEmailDateNoticeSent == null) ? "Null" : x.DAEmailDateNoticeSent.GetValueOrDefault().ToString("MM/dd/yyyy"),
                            DACourierDateNoticeSent = (x.DACourierDateNoticeSent == null) ? "Null" : x.DACourierDateNoticeSent.GetValueOrDefault().ToString("MM/dd/yyyy"),
                            DACourierDateNoticeReceived = (x.DACourierDateNoticeReceived == null) ? "Null" : x.DACourierDateNoticeReceived.GetValueOrDefault().ToString("MM/dd/yyyy"),
                            DACourierReceivedBy = x.DACourierReceivedBy,
                            FinalTurnoverDate = (x.FinalTurnoverDate == null) ? x.FinalTurnoverDate : x.FinalTurnoverDate.Value.Add(x.FinalTurnoverTime.Value),
                            FinalTurnoverOption = x.FinalTurnoverOption,
                            QualificationDate = x.QualificationDate,
                            TurnoverStatus = string.IsNullOrWhiteSpace(x.TurnoverStatus) ? null : x.TurnoverStatus,
                            PunchlistCategory = x.PunchlistCategory,
                            PunchlistItem = x.PunchlistItem,
                            OtherIssues = x.OtherIssues,
                            RushTicketNos = x.RushTicketNos,
                            ReinspectionDate = (x.ReinspectionDate == null) ? "Null" : x.ReinspectionDate.GetValueOrDefault().ToString("MM/dd/yyyy"),
                            UnitAcceptanceDate = (x.UnitAcceptanceDate == null) ? "Null" : x.UnitAcceptanceDate.GetValueOrDefault().ToString("MM/dd/yyyy"),
                            KeyTransmittalDate = (x.KeyTransmittalDate == null) ? "Null" : x.KeyTransmittalDate.GetValueOrDefault().ToString("MM/dd/yyyy"),
                            DeemedAcceptanceDate = (x.DeemedAcceptanceDate == null) ? "Null" : x.DeemedAcceptanceDate.GetValueOrDefault().ToString("MM/dd/yyyy"),
                            TurnoverStatusTATNos = Convert.ToInt16(x.TurnoverStatusTATNos),
                            PunchlistDateTATNos = Convert.ToInt16(x.PunchlistDateTATNos),
                            DeemedEmailDateSentTATNos = Convert.ToInt16(x.DeemedEmailDateSentTATNos),
                            TOStatusAging = Convert.ToInt16(x.TOStatusAging),
                            PunchlistAging = Convert.ToInt16(x.PunchlistAging),
                            DAEmailAging = Convert.ToInt16(x.DAEmailAging)
                        }).AsEnumerable();

                        //searching
                        if (!string.IsNullOrWhiteSpace(arr.param4))
                        {
                            arr.param4 = arr.param4.ToLower();
                            // Search by on selected column
                            switch (arr.param3)
                            {
                                case "1": // Project Code and Name
                                    source = source.Where(x => x.ProjectCode.ToLower().Contains(arr.param4) || x.BusinessEntity.ToLower().Contains(arr.param4));
                                    break;
                                case "2": // Unit Category
                                    source = source.Where(x => x.UnitCategoryDesc.ToLower().Contains(arr.param4));
                                    break;
                                case "3": // Unit Type
                                    source = source.Where(x => x.UnitTypeDesc.ToLower().Contains(arr.param4));
                                    break;
                                case "4": // Unit Type
                                    source = source.Where(x => x.RefNos.ToLower().Contains(arr.param4));
                                    break;
                                case "5": // Customer Name
                                    source = source.Where(x => x.CustomerNos.ToLower().Contains(arr.param4) || x.CustomerName1.ToLower().Contains(arr.param4));
                                    break;
                                case "6": // Account Type
                                    source = source.Where(x => x.AccountTypeDesc.ToLower().Contains(arr.param4));
                                    break;
                                case "7": // Handover Associate
                                    source = source.Where(x => x.HandoverAssociate != null && x.HandoverAssociate.ToLower().Contains(arr.param4));
                                    break;
                                case "8": // Final turnover date
                                    source = source.Where(x => x.FinalTurnoverDate != null && x.FinalTurnoverDate.GetValueOrDefault().ToString("MM/dd/yyyy") == arr.param4);
                                    break;
                                case "9": // Final turnover option
                                    source = source.Where(x => x.FinalTurnoverOption != null && x.FinalTurnoverOption.ToLower().Contains(arr.param4));
                                    break;
                                case "10": // Turnover Status
                                    source = source.Where(x => x.TurnoverStatus != null && x.TurnoverStatus.ToLower().Contains(arr.param4));
                                    break;
                                case "11": // Punchlist Category
                                    source = source.Where(x => x.PunchlistCategory != null && x.PunchlistCategory.ToLower().Contains(arr.param4));
                                    break;
                                default:
                                    source = source.Where(x => x.ProjectCode.ToLower().Contains(arr.param4) || x.BusinessEntity.ToLower().Contains(arr.param4) || x.UnitCategoryDesc.ToLower().Contains(arr.param4) || x.UnitTypeDesc.ToLower().Contains(arr.param4) ||
                                                        x.RefNos.ToLower().Contains(arr.param4) || x.CustomerNos.ToLower().Contains(arr.param4) || x.CustomerName1.ToLower().Contains(arr.param4) || x.AccountTypeDesc.ToLower().Contains(arr.param4) ||
                                                        (x.HandoverAssociate != null && x.HandoverAssociate.ToLower().Contains(arr.param4)) || (x.FinalTurnoverOption != null && x.FinalTurnoverOption.ToLower().Contains(arr.param4)) ||
                                                        (x.TurnoverStatus != null && x.TurnoverStatus.ToLower().Contains(arr.param4)) || (x.PunchlistCategory != null && x.PunchlistCategory.ToLower().Contains(arr.param4)) ||
                                                        (x.FinalTurnoverDate != null && x.FinalTurnoverDate.GetValueOrDefault().ToString("MM/dd/yyyy") == arr.param4));
                                    break;
                            }
                        }

                        source = source.OrderByDescending(s => s.TurnoverStatus == "Null").ThenBy(s => s.TurnoverStatus).ThenByDescending(s => s.TOStatusAging).ThenByDescending(s => s.DAEmailAging).ThenByDescending(s => s.PunchlistAging);

                        // Report Path and DataSource Configuration
                        localReport.ReportPath = @"Reports/Rdlc/TOStatusDashboard1.rdlc";
                        localReport.DataSources.Clear();
                        localReport.DataSources.Add(new ReportDataSource("DSTOStatusDashboard", source));

                        fileName = String.Concat("TOStatus", dt.ToString("yyyyMMdd"), extension);
                    }
                    else if (rep == "z2xh9999aeqxX023131fgh") // Report for Titling Status
                    {
                        var ProjectCode = arr.param1.Split('|').Select(Int32.Parse).ToList();

                        // Get Selected Projects
                        if (ProjectCode == null || ProjectCode.Count == 0)
                            ProjectCode = new List<int> { 0 };

                        var projCodes = db.VW_Projects.Where(x => ProjectCode.Contains(x.Id)).Select(x => x.CompanyCode + "-" + x.ProjectCode).ToArray();
                        if ((projCodes == null || projCodes.Count() == 0) && string.IsNullOrWhiteSpace(arr.param2))
                            return View("~/Views/Admin/BadRequest.cshtml");

                        IEnumerable<CustomerDashboard_TitlingStatus> source = null;
                        var pipeline = (from qt in db.VW_TitlingStatus
                                        where projCodes.Contains(qt.CompanyCode + "-" + qt.ProjectCode)
                                        //where (qt.TOAS != null) // && qt.FinalTurnoverDate >= DateTime.Today || qt.FinalTurnoverDate == null)
                                        orderby qt.TitleInProcessDate descending, qt.TitleTransferredDate descending, qt.TitleClaimedDate descending, qt.TaxDeclarationTransferredDate descending, qt.TaxDeclarationClaimedDate descending
                                        select new
                                        {
                                            qt.CompanyCode,
                                            qt.UnitNos,
                                            qt.ProjectCode,
                                            qt.RefNos,
                                            qt.Phase,
                                            qt.BusinessEntity,
                                            qt.UnitCategoryCode,
                                            qt.UnitCategoryDesc,
                                            qt.UnitTypeDesc,
                                            qt.CustomerNos,
                                            qt.CustomerName1,
                                            qt.AccountTypeDesc,
                                            qt.EmailAdd,
                                            qt.ContactPerson,
                                            qt.TelNos,
                                            qt.TitleInProcessDate,
                                            qt.TitleTransferredDate,
                                            qt.TitleClaimedDate,
                                            qt.TaxDeclarationTransferredDate,
                                            qt.TaxDeclarationClaimedDate,
                                            qt.TOAS,
                                            qt.SAPTurnoverDate,
                                            qt.TurnoverDate,
                                            qt.QualificationDate,
                                            qt.TaxDecNos,
                                            qt.LiquidationEndorsedDate,
                                            qt.LiquidationRushTicketNos,
                                            qt.LiquidationEndorsedRemarks,
                                            qt.TitleReleaseEndorsedDate,
                                            qt.TitleReleaseRushTicketNos,
                                            qt.TitleReleaseEndorsedRemarks,
                                            qt.TitleLocationName,
                                            qt.TitleNos,
                                            qt.TitleRemarks,
                                            qt.BankReleasedDate,
                                            qt.BankReleasedRemarks,
                                            qt.BuyerReleasedDate,
                                            qt.BuyerReleasedRemarks,
                                            qt.TranClass
                                        }).ToList();

                        //// Business Rule with SAP Cut-off Date based on System Parameter
                        //if (systemParameter.EnableTOCutOffDate == true)
                        //    pipeline = pipeline.Where(x => x.TranClass == "Business Rule 1").ToList();

                        //switch (arr.param2)
                        //{
                        //    case "Pwt": //Processing within TAT
                        //        pipeline = pipeline.Where(x => x.EmailNoticeNotifDate2 > dt && x.EmailDateNoticeSent == null).ToList();
                        //        break;
                        //    case "Pbt": //Processing beyond TAT
                        //        pipeline = pipeline.Where(x => x.EmailNoticeNotifDate2 <= dt && x.EmailDateNoticeSent == null).ToList();
                        //        break;
                        //    case "Ptl": //Qualified within TAT (Pwt + Pbt)
                        //        pipeline = pipeline.Where(x => (x.EmailNoticeNotifDate2 > dt && x.EmailDateNoticeSent == null) || (x.EmailNoticeNotifDate2 <= dt && x.EmailDateNoticeSent == null)).ToList();
                        //        break;

                        //    case "Cwt": //Confirmed within TAT
                        //        pipeline = pipeline.Where(x => x.ScheduleEmailNotifDate1 > dt && x.EmailDateNoticeSent != null && x.TurnoverDate1 == null).ToList(); // && x.TurnoverDate1 != null && x.TurnoverDate1 >= dt
                        //        break;
                        //    case "Cbt": //Confirmed beyond TAT
                        //        pipeline = pipeline.Where(x => x.ScheduleEmailNotifDate1 <= dt && x.EmailDateNoticeSent != null && x.TurnoverDate1 == null).ToList();
                        //        break;
                        //    case "Ctl": //Qualified beyond TAT (Cwt + Cbt)
                        //        pipeline = pipeline.Where(x => (x.ScheduleEmailNotifDate1 > dt && x.EmailDateNoticeSent != null && x.TurnoverDate1 == null) || (x.ScheduleEmailNotifDate1 <= dt && x.EmailDateNoticeSent != null && x.TurnoverDate1 == null)).ToList();
                        //        break;

                        //    case "Qwt": //Qualified within TAT (Pwt + Cwt)
                        //        pipeline = pipeline.Where(x => (x.EmailNoticeNotifDate2 > dt && x.EmailDateNoticeSent == null) || (x.ScheduleEmailNotifDate1 > dt && x.TurnoverDate1 != null && x.TurnoverDate1 >= dt)).ToList();
                        //        break;
                        //    case "Qbt": //Qualified beyond TAT (Pbt + Cbt)
                        //        pipeline = pipeline.Where(x => (x.EmailNoticeNotifDate2 <= dt && x.EmailDateNoticeSent == null) || (x.ScheduleEmailNotifDate1 <= dt && x.TurnoverDate1 != null && x.TurnoverDate1 >= dt)).ToList();
                        //        break;
                        //    case "Qtl": //Qualified within TAT (Qwt + Qbt)
                        //        pipeline = pipeline.Where(x => ((x.EmailNoticeNotifDate2 > dt && x.EmailDateNoticeSent == null) || (x.ScheduleEmailNotifDate1 > dt && x.TurnoverDate1 != null && x.TurnoverDate1 >= dt)) || ((x.EmailNoticeNotifDate2 < dt && x.EmailDateNoticeSent == null) || (x.ScheduleEmailNotifDate1 < dt && x.TurnoverDate1 != null && x.TurnoverDate1 >= dt))).ToList();
                        //        break;
                        //    default:
                        //        pipeline = pipeline.Where(x => projCodes.Contains(x.CompanyCode + "-" + x.ProjectCode)).ToList();
                        //        break;
                        //}

                        source = pipeline.Select(x => new CustomerDashboard_TitlingStatus
                        {
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
                            TitleInProcessDate = x.TitleInProcessDate,
                            TitleTransferredDate = x.TitleTransferredDate,
                            TitleClaimedDate = x.TitleClaimedDate,
                            TaxDeclarationTransferredDate = x.TaxDeclarationTransferredDate,
                            TaxDeclarationClaimedDate = x.TaxDeclarationClaimedDate,
                            TOAS = x.TOAS,
                            TurnoverDate = x.TurnoverDate,
                            SAPTurnoverDate = x.SAPTurnoverDate,
                            QualificationDate = x.QualificationDate,
                            TaxDecNos = x.TaxDecNos,
                            LiquidationEndorsedDate = x.LiquidationEndorsedDate,
                            LiquidationRushTicketNos = x.LiquidationRushTicketNos,
                            LiquidationEndorsedRemarks = x.LiquidationEndorsedRemarks,
                            TitleReleaseEndorsedDate = x.TitleReleaseEndorsedDate,
                            TitleReleaseRushTicketNos = x.TitleReleaseRushTicketNos,
                            TitleReleaseEndorsedRemarks = x.TitleReleaseEndorsedRemarks,
                            TitleLocationName = x.TitleLocationName,
                            TitleNos = x.TitleNos,
                            TitleRemarks = x.TitleRemarks,
                            BankReleasedDate = x.BankReleasedDate,
                            BankReleasedRemarks = x.BankReleasedRemarks,
                            BuyerReleasedDate = x.BuyerReleasedDate,
                            BuyerReleasedRemarks = x.BankReleasedRemarks
                        }).AsEnumerable();

                        //searching
                        if (!string.IsNullOrWhiteSpace(arr.param4))
                        {
                            arr.param4 = arr.param4.ToLower();

                            // Search by on selected column
                            switch (arr.param3)
                            {
                                case "1": // Project Code and Name
                                    source = source.Where(x => x.ProjectCode.ToLower().Contains(arr.param4) || x.BusinessEntity.ToLower().Contains(arr.param4));
                                    break;
                                case "2": // Unit Category
                                    source = source.Where(x => x.UnitCategoryDesc.ToLower().Contains(arr.param4));
                                    break;
                                case "3": // Unit Type
                                    source = source.Where(x => x.UnitTypeDesc.ToLower().Contains(arr.param4));
                                    break;
                                case "4": // Unit Type
                                    source = source.Where(x => x.RefNos.ToLower().Contains(arr.param4));
                                    break;
                                case "5": // Customer Name
                                    source = source.Where(x => x.CustomerNos.ToLower().Contains(arr.param4) || x.CustomerName1.ToLower().Contains(arr.param4));
                                    break;
                                case "6": // Account Type
                                    source = source.Where(x => x.AccountTypeDesc.ToLower().Contains(arr.param4));
                                    break;
                                case "7": // TOAS Date
                                    source = source.Where(x => x.TOAS != null && x.TOAS.GetValueOrDefault().ToString("MM/dd/yyyy") == arr.param4);
                                    break;
                                case "8": // Title In Process Date
                                    source = source.Where(x => (x.TitleInProcessDate != null && x.TitleInProcessDate.ToString() == arr.param4));
                                    break;
                                case "9": // Title Transferred Date
                                    source = source.Where(x => (x.TitleTransferredDate != null && x.TitleTransferredDate.ToString() == arr.param4));
                                    break;
                                case "10": // Title Claimed Date
                                    source = source.Where(x => (x.TitleClaimedDate != null && x.TitleClaimedDate.ToString() == arr.param4));
                                    break;
                                case "11": // Tax Declaration Transferred Date
                                    source = source.Where(x => (x.TaxDeclarationTransferredDate != null && x.TitleClaimedDate.ToString() == arr.param4));
                                    break;
                                case "12": // Tax Declaration Claimed Date
                                    source = source.Where(x => (x.TaxDeclarationClaimedDate != null && x.TitleClaimedDate.ToString() == arr.param4));
                                    break;
                                case "13": // Tax Declaration Nos
                                    source = source.Where(x => x.TaxDecNos != null && x.TaxDecNos.ToLower().Contains(arr.param4));
                                    break;
                                case "14": // Title Location
                                    source = source.Where(x => x.TitleLocationName != null && x.TitleLocationName.ToLower().Contains(arr.param4));
                                    break;
                                default:
                                    source = source.Where(x => x.ProjectCode.ToLower().Contains(arr.param4) || x.BusinessEntity.ToLower().Contains(arr.param4) || x.UnitCategoryDesc.ToLower().Contains(arr.param4) || x.UnitTypeDesc.ToLower().Contains(arr.param4) ||
                                                        x.RefNos.ToLower().Contains(arr.param4) || x.CustomerNos.ToLower().Contains(arr.param4) || x.CustomerName1.ToLower().Contains(arr.param4) || x.AccountTypeDesc.ToLower().Contains(arr.param4) ||
                                                        (x.TitleInProcessDate != null && x.TitleInProcessDate.ToString() == arr.param4) || (x.TitleTransferredDate != null && x.TitleTransferredDate.ToString() == arr.param4) || (x.TitleClaimedDate != null && x.TitleClaimedDate.ToString() == arr.param4) ||
                                                        (x.TaxDeclarationTransferredDate != null && x.TaxDeclarationTransferredDate.ToString() == arr.param4) || (x.TaxDeclarationClaimedDate != null && x.TaxDeclarationClaimedDate.ToString() == arr.param4) ||
                                                        (x.TOAS != null && x.TOAS.GetValueOrDefault().ToString("MM/dd/yyyy") == arr.param4) ||
                                                        x.TaxDecNos.ToLower().Contains(arr.param4) || x.TitleLocationName.ToLower().Contains(arr.param4));
                                    break;
                            }
                        }

                        source = source.OrderByDescending(s => s.TitleInProcessDate).ThenByDescending(s => s.TitleTransferredDate).ThenByDescending(s => s.TitleClaimedDate).ThenByDescending(s => s.TaxDeclarationTransferredDate).ThenByDescending(s => s.TaxDeclarationClaimedDate);

                        // Report Path and DataSource Configuration
                        localReport.ReportPath = @"Reports/Rdlc/TitlingStatusDashboard1.rdlc";
                        localReport.DataSources.Clear();
                        localReport.DataSources.Add(new ReportDataSource("DSTitlingStatusDashboard", source));

                        fileName = String.Concat("TitlingStatus", dt.ToString("yyyyMMdd"), extension);
                    }
                    else
                    {
                        return View("~/Views/Admin/BadRequest.cshtml");
                    }

                    string mimeType;
                    string encoding;
                    string fileNameExtension;
                    Warning[] warnings;
                    string[] streams;
                    byte[] renderedBytes;

                    // Render the report to bytes
                    renderedBytes = localReport.Render(reportType, null, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
                    localReport.Dispose();

                    return File(renderedBytes, mimeType, fileName);
                }
            }           
        }
    }
}
