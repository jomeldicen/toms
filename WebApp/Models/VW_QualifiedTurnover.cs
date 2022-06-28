//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebApp.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class VW_QualifiedTurnover
    {
        public long Id { get; set; }
        public string SalesDocNos { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string ProjectCode { get; set; }
        public string BusinessEntity { get; set; }
        public string ProjectLocation { get; set; }
        public string UnitNos { get; set; }
        public string RefNos { get; set; }
        public string Phase { get; set; }
        public string UnitType { get; set; }
        public string UnitTypeCode { get; set; }
        public string UnitTypeDesc { get; set; }
        public string UnitArea { get; set; }
        public string UnitMsrmnt { get; set; }
        public string UnitCategoryCode { get; set; }
        public string UnitCategoryDesc { get; set; }
        public string CustomerNos { get; set; }
        public string CustomerName1 { get; set; }
        public string AddrNos { get; set; }
        public string Addrs { get; set; }
        public string SalesDocStatus { get; set; }
        public Nullable<System.DateTime> TOAS { get; set; }
        public Nullable<System.DateTime> SAPTurnoverDate { get; set; }
        public Nullable<System.DateTime> SalesDocDate { get; set; }
        public Nullable<bool> EnableTOCutOffDate { get; set; }
        public Nullable<System.DateTime> TOCutOffDate { get; set; }
        public string TranClass { get; set; }
        public string AccountTypeCode { get; set; }
        public string AccountTypeDesc { get; set; }
        public string EmailAdd { get; set; }
        public string TelNos { get; set; }
        public string ContactPerson { get; set; }
        public Nullable<System.DateTime> QualificationDate { get; set; }
        public string QCDAcceptanceDate { get; set; }
        public string FPMCAcceptanceDate { get; set; }
        public Nullable<System.DateTime> CMGAcceptanceDate { get; set; }
        public string UnitAcceptanceRemarks { get; set; }
        public string HasOccupancyPermit { get; set; }
        public string OccupancyPermitDate { get; set; }
        public string OccupancyPermitRemarks { get; set; }
        public int NoticeTOID { get; set; }
        public string EmailDateNoticeSent { get; set; }
        public Nullable<System.DateTime> EmailTurnoverDate { get; set; }
        public string EmailNoticeRemarks { get; set; }
        public string EmailNoticeAttachment { get; set; }
        public string CourierDateNoticeSent { get; set; }
        public string CourierDateNoticeReceived { get; set; }
        public string CourierReceivedBy { get; set; }
        public string CourierNoticeRemarks { get; set; }
        public string CourierNoticeAttachment { get; set; }
        public string HandoverAssociate { get; set; }
        public Nullable<System.DateTime> ScheduleEmailNotifDate1 { get; set; }
        public Nullable<System.DateTime> ScheduleEmailNotifDate2 { get; set; }
        public int TOScheduleID { get; set; }
        public Nullable<System.DateTime> TurnoverDate1 { get; set; }
        public Nullable<System.TimeSpan> TurnoverTime1 { get; set; }
        public string TurnoverOption1 { get; set; }
        public string TurnoverAttachment1 { get; set; }
        public string TurnoverRemarks1 { get; set; }
        public Nullable<System.DateTime> TurnoverDate2 { get; set; }
        public Nullable<System.TimeSpan> TurnoverTime2 { get; set; }
        public string TurnoverOption2 { get; set; }
        public string FinalTurnoverOption { get; set; }
        public Nullable<System.DateTime> FinalTurnoverDate { get; set; }
        public Nullable<System.TimeSpan> FinalTurnoverTime { get; set; }
        public string TurnoverAttachment2 { get; set; }
        public string TurnoverRemarks2 { get; set; }
        public int TORule1 { get; set; }
        public int TORule2 { get; set; }
        public int wAcceptance { get; set; }
        public Nullable<bool> TOIsPosted { get; set; }
        public Nullable<System.DateTime> TurnoverStatusTAT { get; set; }
        public Nullable<int> EmailNoticeSentAgingDays { get; set; }
        public Nullable<System.DateTime> EmailNoticeNotifDate2 { get; set; }
        public int TOAcceptanceID { get; set; }
        public string TurnoverStatus { get; set; }
        public Nullable<System.DateTime> TurnoverStatusDate { get; set; }
        public string PunchlistCategory { get; set; }
        public string PunchlistItem { get; set; }
        public Nullable<System.DateTime> PunchlistCategoryTAT { get; set; }
        public string OtherIssues { get; set; }
        public string RushTicketNos { get; set; }
        public Nullable<System.DateTime> UnitAcceptanceDate { get; set; }
        public Nullable<System.DateTime> KeyTransmittalDate { get; set; }
        public Nullable<System.DateTime> ReinspectionDate { get; set; }
        public Nullable<System.DateTime> ReinspectionDateTAT { get; set; }
        public Nullable<System.DateTime> AdjReinspectionDate { get; set; }
        public Nullable<System.DateTime> AdjReinspectionDateTAT { get; set; }
        public Nullable<System.DateTime> LastReinspectionDate { get; set; }
        public Nullable<System.DateTime> LastReinspectionDateTAT { get; set; }
        public Nullable<int> IsUnitAcceptanceDateSAPSync { get; set; }
        public int DeemedAcceptanceID { get; set; }
        public Nullable<System.DateTime> DeemedAcceptanceDate { get; set; }
        public Nullable<System.DateTime> DAEmailDateNoticeSent { get; set; }
        public Nullable<System.DateTime> DAEmailDateNoticeSentMaxDate { get; set; }
        public Nullable<int> IsDeemedAcceptanceDateSAPSync { get; set; }
        public Nullable<System.DateTime> DACourierDateNoticeSent { get; set; }
        public Nullable<System.DateTime> DACourierDateNoticeReceived { get; set; }
        public string DACourierReceivedBy { get; set; }
        public Nullable<int> TurnoverStatusTATNos { get; set; }
        public Nullable<int> PunchlistDateTATNos { get; set; }
        public Nullable<int> DeemedEmailDateSentTATNos { get; set; }
        public string EmailNoticeSentAging { get; set; }
        public string TOStatusAging { get; set; }
        public Nullable<int> PunchlistAging { get; set; }
        public string DAEmailAging { get; set; }
        public Nullable<System.DateTime> UnitAcceptanceDateTAT { get; set; }
        public string QuotDocNos { get; set; }
    }
}
