using System;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;
using Newtonsoft.Json;
using WebApp.Models;

namespace WebApp.Views.Shared
{
    public partial class ReportViewer : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                var cId = User.Identity.GetUserId();
            }
        }

        private void render2PDF(string _paperType, string _report)
        {
            try
            {
                string _deviceInfo = @"<DeviceInfo><MarginLeft>0in</MarginLeft><MarginRight>0in</MarginRight><MarginTop>0in</MarginTop><MarginBottom>0in</MarginBottom>";
                if (_paperType == "PortraitA4")
                    _deviceInfo = _deviceInfo + @"<PageWidth>8.3in</PageWidth><PageHeight>11.7in</PageHeight>";
                else if (_paperType == "LandscapeA4")
                    _deviceInfo = _deviceInfo + @"<PageWidth>11.7in</PageWidth><PageHeight>8.3in</PageHeight>";
                else if (_paperType == "PortraitLegal")
                    _deviceInfo = _deviceInfo + @"<PageWidth>8.5in</PageWidth><PageHeight>14in</PageHeight>";
                else if (_paperType == "LandscapeLegal")
                    _deviceInfo = _deviceInfo + @"<PageWidth>14in</PageWidth><PageHeight>8.5in</PageHeight>";
                else if (_paperType == "PortraitLetter")
                    _deviceInfo = _deviceInfo + @"<PageWidth>8.5in</PageWidth><PageHeight>11in</PageHeight>";
                else if (_paperType == "LandscapeLetter")
                    _deviceInfo = _deviceInfo + @"<PageWidth>11in</PageWidth><PageHeight>8.5in</PageHeight>";
                _deviceInfo = _deviceInfo + @"</DeviceInfo>";

                this.Title = _report;

                string[] _streams = { "" };
                Warning[] _warnings = null;
                string _mimeType = null;
                string _encoding = null;
                string _fileExtension = null;
                byte[] bytes;

                bytes = this.ReportViewer1.LocalReport.Render("PDF", _deviceInfo, out _mimeType, out _encoding, out _fileExtension, out _streams, out _warnings);
                Response.Buffer = false;
                Response.BufferOutput = false;
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.AppendHeader("Content-Disposition", "inline;filename=" + _report + "." + _fileExtension);
                Response.ContentType = _mimeType;
                Response.BinaryWrite(bytes);
                Response.End();
              
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected void Page_LoadComplete(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                try
                {
                    string rep = Request.QueryString["rep"].ToString();
                    string json = Request.QueryString["json"].ToString();

                    DateTime dt = DateTime.Today;
                    var arr = JsonConvert.DeserializeObject<ControllerParam[]>(json).SingleOrDefault();

                    //string fileName = "";

                    using (WebAppEntities db = new WebAppEntities())
                    {
                        using (var dbContextTransaction = db.Database.BeginTransaction())
                        {
                            // Check if system parameter is properly set
                            var systemParameter = db.SystemParameters.Where(x => x.Published == true).FirstOrDefault();
                            if (systemParameter != null)
                            {
                                // **************************************************************** //
                                // Report for TO Pipeline Report    
                                // **************************************************************** //                        
                                if (rep == "t1xh8G40aeqxX02312llkh")
                                {
                                    var ProjectCode = arr.param1.Split('|').Select(Int32.Parse).ToList();

                                    // Get Selected Projects
                                    if (ProjectCode == null || ProjectCode.Count == 0)
                                        ProjectCode = new List<int> { 0 };

                                    var projCodes = db.VW_Projects.Where(x => x.TOM == true && ProjectCode.Contains(x.Id)).Select(x => x.CompanyCode + "-" + x.ProjectCode).ToArray();
                                    if ((projCodes == null || projCodes.Count() == 0) && string.IsNullOrWhiteSpace(arr.param2))
                                    {
                                        this.ReportViewer1.Visible = false;
                                        lblMessage.Visible = true;
                                        lblMessage.Text = "Invalid Report Parameter!";
                                        return;
                                    }

                                    // Get List of Qualified Clients for Scheduling
                                    IQueryable<VW_QualifiedTurnover> source = db.VW_QualifiedTurnover.Where(x => x.TOAS != null).OrderByDescending(x => x.EmailNoticeSentAging).OrderBy(x => x.Phase).OrderBy(x => x.RefNos).OrderBy(x => x.EmailDateNoticeSent).OrderBy(x => x.FinalTurnoverDate);

                                    // Business Rule with SAP Cut-off Date based on System Parameter
                                    if (systemParameter.EnableTOCutOffDate == true)
                                        source = source.Where(x => x.TranClass == "Business Rule 1");

                                    switch (arr.param2)
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
                                                source = source.Where(x => x.FinalTurnoverDate != null && x.FinalTurnoverDate.GetValueOrDefault().ToString("MM/dd/yyyy") == arr.param4);
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
                                                                    (x.FinalTurnoverDate != null && x.FinalTurnoverDate.GetValueOrDefault().ToString("MM/dd/yyyy") == arr.param4));
                                                break;
                                        }
                                    }

                                    // Get the final list base on the define linq queryable parameter
                                    var results = source.Select(x => new
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

                                    this.ReportViewer1.ProcessingMode = ProcessingMode.Local;
                                    this.ReportViewer1.LocalReport.ReportPath = @"Reports/Rdlc/TOPipelineDashboard1.rdlc";
                                    this.ReportViewer1.LocalReport.DataSources.Clear();
                                    this.ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DSTOPipelineDashboard", pipeline));
                                    this.ReportViewer1.LocalReport.Refresh();

                                    //fileName = String.Concat("TOPipeline", dt.ToString("yyyyMMdd"));
                                    //render2PDF("LandscapeLegal", fileName);
                                }
                                // **************************************************************** //
                                // Report for TO Schedule Report 
                                // **************************************************************** //
                                else if (rep == "t2xf1F10jklxM30923llkj")
                                {
                                    string[] HandoverAssoc = arr.param1.Split('|');
                                    string[] AccountTypeCode = arr.param2.Split('|');
                                    string[] FinalTurnoverOption = arr.param3.Split('|');

                                    IQueryable<VW_QualifiedTurnover> source = db.VW_QualifiedTurnover.
                                    Where(x => x.TOAS != null && x.FinalTurnoverOption != null && HandoverAssoc.Contains(x.HandoverAssociate) && AccountTypeCode.Contains(x.AccountTypeCode) &&
                                                FinalTurnoverOption.Contains(x.FinalTurnoverOption) && (x.FinalTurnoverDate >= arr.dt1.Date && x.FinalTurnoverDate <= arr.dt2.Date)).
                                    OrderBy(x => x.FinalTurnoverDate).OrderBy(x => x.FinalTurnoverOption).OrderBy(x => x.AccountTypeDesc).OrderBy(x => x.BusinessEntity).OrderBy(x => x.Phase).OrderBy(x => x.RefNos).OrderBy(x => x.HandoverAssociate);

                                    // Business Rule with SAP Cut-off Date based on System Parameter
                                    if (systemParameter.EnableTOCutOffDate == true)
                                        source = source.Where(x => x.TranClass == "Business Rule 1");

                                    //searching
                                    if (!string.IsNullOrWhiteSpace(arr.param4))
                                    {
                                        arr.param4 = arr.param4.ToLower();
                                        source = source.Where(x => x.ProjectCode.ToLower().Contains(arr.param4) || x.RefNos.ToLower().Contains(arr.param4) ||
                                                            x.UnitType.ToLower().Contains(arr.param4) || x.CustomerNos.ToLower().Contains(arr.param4) ||
                                                            x.CustomerName1.ToLower().Contains(arr.param4) || x.FinalTurnoverOption.ToLower().Contains(arr.param4) ||
                                                            x.AccountTypeDesc.ToLower().Contains(arr.param4) || x.HandoverAssociate.ToLower().Contains(arr.param4));
                                    }

                                    // Get the final list base on the define linq queryable parameter
                                    var results = source.Select(x => new
                                    {
                                        x.FinalTurnoverDate,
                                        x.FinalTurnoverTime,
                                        x.FinalTurnoverOption,
                                        x.CustomerNos,
                                        x.CustomerName1,
                                        x.UnitType,
                                        x.ProjectCode,
                                        x.BusinessEntity,
                                        x.RefNos,
                                        x.Phase,
                                        x.HandoverAssociate,
                                        x.AccountTypeDesc,
                                        x.TranClass
                                    }).ToList();

                                    IEnumerable<CustomDashboard_TOSchedule> toSchedule = null;
                                    toSchedule = results.Select(x => new CustomDashboard_TOSchedule
                                    {
                                        FinalTurnoverDate = x.FinalTurnoverDate.Value.Add(x.FinalTurnoverTime.Value),
                                        FinalTurnoverOption = x.FinalTurnoverOption,
                                        CustomerNos = x.CustomerNos,
                                        CustomerName = x.CustomerName1,
                                        UnitType = x.UnitType,
                                        ProjectCode = x.ProjectCode,
                                        BusinessEntity = x.BusinessEntity,
                                        RefNos = x.RefNos,
                                        Phase = x.Phase,
                                        HandoverAssociate = x.HandoverAssociate,
                                        AccountTypeDesc = x.AccountTypeDesc
                                    }).AsEnumerable();

                                    this.ReportViewer1.ProcessingMode = ProcessingMode.Local;
                                    this.ReportViewer1.LocalReport.ReportPath = @"Reports/Rdlc/TOScheduleDashboard.rdlc";
                                    this.ReportViewer1.LocalReport.DataSources.Clear();
                                    this.ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DSTOScheduleDashboard", toSchedule));
                                    this.ReportViewer1.LocalReport.Refresh();

                                    //fileName = String.Concat("TOSchedule", dt.ToString("yyyyMMdd"));
                                    //render2PDF("PortraitLetter", fileName);
                                }
                                // **************************************************************** //
                                // Report for TO Status Report 
                                // **************************************************************** //
                                else if (rep == "t3xf2E20iklxL40123lssj")
                                {
                                    var ProjectCode = arr.param1.Split('|').Select(Int32.Parse).ToList();

                                    // Get Selected Projects
                                    if (ProjectCode == null || ProjectCode.Count == 0)
                                        ProjectCode = new List<int> { 0 };

                                    var projCodes = db.VW_Projects.Where(x => x.TOM == true && ProjectCode.Contains(x.Id)).Select(x => x.CompanyCode + "-" + x.ProjectCode).ToArray();
                                    if ((projCodes == null || projCodes.Count() == 0) && string.IsNullOrWhiteSpace(arr.param2))
                                    {
                                        this.ReportViewer1.Visible = false;
                                        lblMessage.Visible = true;
                                        lblMessage.Text = "Invalid Report Parameter!";
                                        return;
                                    }

                                    // Get List of Qualified Clients for Scheduling
                                    IQueryable<VW_QualifiedTurnover> source = db.VW_QualifiedTurnover.Where(x => x.TOAS != null && x.FinalTurnoverDate != null && x.FinalTurnoverOption != null).
                                        OrderBy(x => x.TurnoverStatus).OrderByDescending(x => x.TOStatusAging).OrderByDescending(x => x.DAEmailAging).OrderByDescending(x => x.PunchlistAging);

                                    // Business Rule with SAP Cut-off Date based on System Parameter
                                    if (systemParameter.EnableTOCutOffDate == true)
                                        source = source.Where(x => x.TranClass == "Business Rule 1");

                                    switch (arr.param2)
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

                                    // Get the final list base on the define linq queryable parameter
                                    var results = source.Select(x => new
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


                                    this.ReportViewer1.ProcessingMode = ProcessingMode.Local;
                                    this.ReportViewer1.LocalReport.ReportPath = @"Reports/Rdlc/TOStatusDashboard1.rdlc";
                                    this.ReportViewer1.LocalReport.DataSources.Clear();
                                    this.ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DSTOStatusDashboard", tostatus));
                                    this.ReportViewer1.LocalReport.Refresh();

                                    //fileName = String.Concat("TOStatus", dt.ToString("yyyyMMdd"));
                                    //render2PDF("LandscapeLegal", fileName);
                                }
                                // **************************************************************** //
                                // Report for Titling Status
                                // **************************************************************** //
                                else if (rep == "z2xh9999aeqxX023131fgh")
                                {
                                    var ProjectCode = arr.param1.Split('|').Select(Int32.Parse).ToList();

                                    // Get Selected Projects
                                    if (ProjectCode == null || ProjectCode.Count == 0)
                                        ProjectCode = new List<int> { 0 };

                                    var projCodes = db.VW_Projects.Where(x => x.TSM == true && ProjectCode.Contains(x.Id)).Select(x => x.CompanyCode + "-" + x.ProjectCode).ToArray();
                                    if ((projCodes == null || projCodes.Count() == 0) && string.IsNullOrWhiteSpace(arr.param2))
                                    {
                                        this.ReportViewer1.Visible = false;
                                        lblMessage.Visible = true;
                                        lblMessage.Text = "Invalid Report Parameter!";
                                        return;
                                    }

                                    // Get List of Titling Status
                                    IQueryable<VW_DashboardProcessingJob> source = db.VW_DashboardProcessingJob.OrderByDescending(x => x.TitleStatus).OrderBy(x => x.BusinessEntity).OrderBy(x => x.RefNos);

                                    //param2: type of column in Summary ex (Title In-Process, Title Transferred, etc..)
                                    switch (arr.param2)
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
                                    if (!string.IsNullOrWhiteSpace(arr.param4) || (arr.dt1.Year != 1 && arr.dt2.Year != 1))
                                    {
                                        string[] set1 = { "1", "2", "3", "4", "5", "6" };
                                        string[] set2 = { "7", "8", "9", "10", "11", "12", "13", "14", "15", "16" };

                                        //searching
                                        if (!string.IsNullOrWhiteSpace(arr.param4) && set1.Contains(arr.param3))
                                        {
                                            //param4: string data if searchcol(param3) not date
                                            arr.param4 = arr.param4.ToLower();

                                            // search by on selected column
                                            switch (arr.param3)
                                            {
                                                case "1": // Company
                                                    source = source.Where(x => x.CompanyCode.ToLower().Contains(arr.param4) || x.CompanyName.ToLower().Contains(arr.param4));
                                                    break;
                                                case "2": // Project Code and Name
                                                    source = source.Where(x => x.ProjectCode.ToLower().Contains(arr.param4) || x.BusinessEntity.ToLower().Contains(arr.param4));
                                                    break;
                                                case "3": // Unit Type
                                                    source = source.Where(x => x.UnitTypeDesc.ToLower().Contains(arr.param4));
                                                    break;
                                                case "4": // Unit Nos
                                                    source = source.Where(x => x.RefNos.ToLower().Contains(arr.param4));
                                                    break;
                                                case "5": // Customer Name
                                                    source = source.Where(x => x.CustomerNos.ToLower().Contains(arr.param4) || x.CustomerName1.ToLower().Contains(arr.param4));
                                                    break;
                                                case "6": // Title Status
                                                    source = source.Where(x => x.TitleStatus.ToLower().Contains(arr.param4));
                                                    break;
                                            }
                                        }
                                        else if ((arr.dt1.Year != 1 && arr.dt2.Year != 1) && set2.Contains(arr.param3))
                                        {
                                            // Search by on selected column
                                            switch (arr.param3)
                                            {
                                                case "7": // TOAS Date
                                                    source = source.Where(x => x.TOAS >= arr.dt1.Date && x.TOAS <= arr.dt2.Date);
                                                    break;
                                                case "8": // Title In Process Date
                                                    source = source.Where(x => x.TitleInProcessDate >= arr.dt1.Date && x.TitleInProcessDate <= arr.dt2.Date);
                                                    break;
                                                case "9": // Title Transferred Date
                                                    source = source.Where(x => x.TitleTransferredDate >= arr.dt1.Date && x.TitleTransferredDate <= arr.dt2.Date);
                                                    break;
                                                case "10": // Title Claimed Date
                                                    source = source.Where(x => x.TitleClaimedDate >= arr.dt1.Date && x.TitleClaimedDate <= arr.dt2.Date);
                                                    break;
                                                case "11": // Tax Declaration Transferred Date
                                                    source = source.Where(x => x.TaxDeclarationTransferredDate >= arr.dt1.Date && x.TaxDeclarationTransferredDate <= arr.dt2.Date);
                                                    break;
                                                case "12": // Tax Declaration Claimed Date
                                                    source = source.Where(x => x.TaxDeclarationClaimedDate >= arr.dt1.Date && x.TaxDeclarationClaimedDate <= arr.dt2.Date);
                                                    break;
                                                case "13": // Liquidation Endorsement
                                                    source = source.Where(x => x.LiquidationEndorsedDate >= arr.dt1.Date && x.LiquidationEndorsedDate <= arr.dt2.Date);
                                                    break;
                                                case "14": // Title Release Endorsement
                                                    source = source.Where(x => x.TitleReleaseEndorsedDate >= arr.dt1.Date && x.TitleReleaseEndorsedDate <= arr.dt2.Date);
                                                    break;
                                                case "15": // Title Release to Buyer
                                                    source = source.Where(x => x.BuyerReleasedDate >= arr.dt1.Date && x.BuyerReleasedDate <= arr.dt2.Date);
                                                    break;
                                                case "16": // Title Release to Bank
                                                    source = source.Where(x => x.BankReleasedDate >= arr.dt1.Date && x.BankReleasedDate <= arr.dt2.Date);
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                    }

                                    // Get the final list base on the define linq queryable parameter
                                    var results = source.Select(x => new
                                    {
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
                                        x.UnitOwnerReceiptDate,
                                        x.AgingTOASTaxDecTransfer,
                                        x.AgingQualifTaxDecTransfer,
                                        x.AgingTTransferReleaseBank,
                                        x.AgingTTransferReleaseBuyer
                                    }).ToList();

                                    IEnumerable<CustomerDashboard_TitlingStatus> titling = null;
                                    titling = results.Select(x => new CustomerDashboard_TitlingStatus
                                    {
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
                                        AgingTOASTaxDecTransfer = x.AgingTOASTaxDecTransfer,
                                        AgingQualifTaxDecTransfer = x.AgingQualifTaxDecTransfer,
                                        AgingTTransferReleaseBank = x.AgingTTransferReleaseBank,
                                        AgingTTransferReleaseBuyer = x.AgingTTransferReleaseBuyer
                                    }).AsEnumerable();

                                    this.ReportViewer1.ProcessingMode = ProcessingMode.Local;
                                    this.ReportViewer1.LocalReport.ReportPath = @"Reports/Rdlc/TitlingStatusDashboard1.rdlc";
                                    this.ReportViewer1.LocalReport.DataSources.Clear();
                                    this.ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DSTitlingStatusDashboard", titling));
                                    this.ReportViewer1.LocalReport.Refresh();

                                    //fileName = String.Concat("TitlingStatus", dt.ToString("yyyyMMdd"));
                                    //render2PDF("LandscapeLegal", fileName);
                                }
                                // **************************************************************** //
                                // Report for Electric Meter Status
                                // **************************************************************** //
                                else if (rep == "k2rhh1234aeqxX0432ihfgh")
                                {
                                    var ProjectCode = arr.param1.Split('|').Select(Int32.Parse).ToList();

                                    // Get Selected Projects
                                    if (ProjectCode == null || ProjectCode.Count == 0)
                                        ProjectCode = new List<int> { 0 };

                                    var projCodes = db.VW_Projects.Where(x => x.EMM == true && ProjectCode.Contains(x.Id)).Select(x => x.CompanyCode + "-" + x.ProjectCode).ToArray();
                                    if ((projCodes == null || projCodes.Count() == 0) && string.IsNullOrWhiteSpace(arr.param2))
                                    {
                                        this.ReportViewer1.Visible = false;
                                        lblMessage.Visible = true;
                                        lblMessage.Text = "Invalid Report Parameter!";
                                        return;
                                    }

                                    // Get List of Electric Meter Status
                                    IQueryable<VW_DashboardProcessingJobElectric> source = db.VW_DashboardProcessingJobElectric.OrderByDescending(x => x.ApplicationProcessStatus).OrderBy(x => x.DocumentaryCompletedDate).OrderBy(x => x.ElectricMeterStatus);

                                    //param2: type of column in Summary ex (Title In-Process, Title Transferred, etc..)
                                    switch (arr.param2)
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
                                    if (!string.IsNullOrWhiteSpace(arr.param4) || (arr.dt1.Year != 1 && arr.dt2.Year != 1))
                                    {
                                        string[] set1 = { "1", "2", "3", "4", "5", "6", "7" };
                                        string[] set2 = { "8", "9", "10", "11", "12", "13", "14", "15" };

                                        //searching
                                        if (!string.IsNullOrWhiteSpace(arr.param4) && set1.Contains(arr.param3))
                                        {
                                            //param4: string data if searchcol(param3) not date
                                            arr.param4 = arr.param4.ToLower();

                                            // search by on selected column
                                            switch (arr.param3)
                                            {
                                                case "1": // Company
                                                    source = source.Where(x => x.CompanyCode.ToLower().Contains(arr.param4) || x.CompanyName.ToLower().Contains(arr.param4));
                                                    break;
                                                case "2": // Project Code and Name
                                                    source = source.Where(x => x.ProjectCode.ToLower().Contains(arr.param4) || x.BusinessEntity.ToLower().Contains(arr.param4));
                                                    break;
                                                case "3": // Unit Type
                                                    source = source.Where(x => x.UnitTypeDesc.ToLower().Contains(arr.param4));
                                                    break;
                                                case "4": // Unit Nos
                                                    source = source.Where(x => x.RefNos.ToLower().Contains(arr.param4));
                                                    break;
                                                case "5": // Customer Name
                                                    source = source.Where(x => x.CustomerNos.ToLower().Contains(arr.param4) || x.CustomerName1.ToLower().Contains(arr.param4));
                                                    break;
                                                case "6": // Application Process Status
                                                    source = source.Where(x => x.ApplicationProcessStatus.ToLower().Contains(arr.param4));
                                                    break;
                                                case "7": // Electric Meter Status
                                                    source = source.Where(x => x.ElectricMeterStatus.ToLower().Contains(arr.param4));
                                                    break;
                                            }
                                        }
                                        else if ((arr.dt1.Year != 1 && arr.dt2.Year != 1) && set2.Contains(arr.param3))
                                        {
                                            // Search by on selected column
                                            switch (arr.param3)
                                            {
                                                case "8": // Documentary Completed Date
                                                    source = source.Where(x => x.DocumentaryCompletedDate >= arr.dt1.Date && x.DocumentaryCompletedDate <= arr.dt2.Date);
                                                    break;
                                                case "9": // RFP Rush Ticket Date
                                                    source = source.Where(x => x.RFPRushTicketDate >= arr.dt1.Date && x.RFPRushTicketDate <= arr.dt2.Date);
                                                    break;
                                                case "10": // Received Check Date
                                                    source = source.Where(x => x.ReceivedCheckDate >= arr.dt1.Date && x.ReceivedCheckDate <= arr.dt2.Date);
                                                    break;
                                                case "11": // Unpaid Bill Posted Date
                                                    source = source.Where(x => x.UnpaidBillPostedDate >= arr.dt1.Date && x.UnpaidBillPostedDate <= arr.dt2.Date);
                                                    break;
                                                case "12": // Paid Settled Posted Date
                                                    source = source.Where(x => x.PaidSettledPostedDate >= arr.dt1.Date && x.PaidSettledPostedDate <= arr.dt2.Date);
                                                    break;
                                                case "13": // Meralco Submitted Date
                                                    source = source.Where(x => x.MeralcoSubmittedDate >= arr.dt1.Date && x.MeralcoSubmittedDate <= arr.dt2.Date);
                                                    break;
                                                case "14": // Meralco Receipt Date
                                                    source = source.Where(x => x.MeralcoReceiptDate >= arr.dt1.Date && x.MeralcoReceiptDate <= arr.dt2.Date);
                                                    break;
                                                case "15": // Unit Owner Receipt Date
                                                    source = source.Where(x => x.UnitOwnerReceiptDate >= arr.dt1.Date && x.UnitOwnerReceiptDate <= arr.dt2.Date);
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                    }

                                    // Get the final list base on the define linq queryable parameter
                                    var results = source.Select(x => new
                                    {
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

                                    this.ReportViewer1.ProcessingMode = ProcessingMode.Local;
                                    this.ReportViewer1.LocalReport.ReportPath = @"Reports/Rdlc/ElectricMeterDashboard1.rdlc";
                                    this.ReportViewer1.LocalReport.DataSources.Clear();
                                    this.ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DSElectricMeterDashboard", electric));
                                    this.ReportViewer1.LocalReport.Refresh();
                                }
                            } else
                            {
                                this.ReportViewer1.Visible = false;
                                lblMessage.Visible = true;
                                lblMessage.Text = "Invalid Report Parameter!";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.ReportViewer1.Visible = false;
                    lblMessage.Visible = true;
                    lblMessage.Text = String.Concat("Invalid Report Parameter! ", ex.Message);
                }                
            }
        }
    }
}