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
    
    public partial class VW_HistoricalUnit
    {
        public long Id { get; set; }
        public string CompanyCode { get; set; }
        public int ProjectId { get; set; }
        public string ProjectCode { get; set; }
        public string BusinessEntity { get; set; }
        public string ProjectLocation { get; set; }
        public string UnitNos { get; set; }
        public string RefNos { get; set; }
        public string UnitCategoryCode { get; set; }
        public string CustomerNos { get; set; }
        public string CustomerName1 { get; set; }
        public string CustomerHash { get; set; }
        public string Phase { get; set; }
        public Nullable<System.DateTime> TOAS { get; set; }
        public Nullable<System.DateTime> SAPTurnoverDate { get; set; }
        public Nullable<System.DateTime> SalesDocDate { get; set; }
        public Nullable<bool> EnableTOCutOffDate { get; set; }
        public Nullable<System.DateTime> TOCutOffDate { get; set; }
        public string TranClass { get; set; }
        public string OccupancyPermitDate { get; set; }
        public string QCDAcceptanceDate { get; set; }
        public string FPMCAcceptanceDate { get; set; }
        public string HasOccupancyPermit { get; set; }
        public string AccountTypeCode { get; set; }
        public string AccountTypeDesc { get; set; }
        public int HistoricalID { get; set; }
        public Nullable<System.DateTime> EmailDateNoticeSent { get; set; }
        public Nullable<System.DateTime> EmailTurnoverDate { get; set; }
        public string EmailNoticeRemarks { get; set; }
        public string EmailNoticeAttachment { get; set; }
        public Nullable<System.DateTime> CourierDateNoticeSent { get; set; }
        public Nullable<System.DateTime> CourierDateNoticeReceived { get; set; }
        public string CourierReceivedBy { get; set; }
        public string CourierNoticeRemarks { get; set; }
        public string CourierNoticeAttachment { get; set; }
        public string HandoverAssociate { get; set; }
        public Nullable<System.DateTime> TurnoverDate1 { get; set; }
        public Nullable<System.TimeSpan> TurnoverTime1 { get; set; }
        public string TurnoverOption1 { get; set; }
        public string TurnoverAttachment1 { get; set; }
        public string TurnoverRemarks1 { get; set; }
        public Nullable<System.DateTime> TurnoverDate2 { get; set; }
        public Nullable<System.TimeSpan> TurnoverTime2 { get; set; }
        public string TurnoverOption2 { get; set; }
        public string TurnoverAttachment2 { get; set; }
        public string TurnoverRemarks2 { get; set; }
        public string FinalTurnoverOption { get; set; }
        public Nullable<System.DateTime> FinalTurnoverDate { get; set; }
        public Nullable<System.DateTime> FinalTurnoverTime { get; set; }
        public string TurnoverStatus { get; set; }
        public Nullable<System.DateTime> TurnoverStatusDate { get; set; }
        public string PunchlistCategory { get; set; }
        public string PunchlistItem { get; set; }
        public string OtherIssues { get; set; }
        public string TSRemarks { get; set; }
        public string TSAttachment { get; set; }
        public Nullable<System.DateTime> UnitAcceptanceDate { get; set; }
        public Nullable<System.DateTime> KeyTransmittalDate { get; set; }
        public Nullable<System.DateTime> ReinspectionDate { get; set; }
        public Nullable<System.DateTime> AdjReinspectionDate { get; set; }
        public string RushTicketNos { get; set; }
        public string SRRemarks { get; set; }
        public Nullable<System.DateTime> DeemedAcceptanceDate { get; set; }
        public string DeemedAcceptanceRemarks { get; set; }
        public Nullable<System.DateTime> DAEmailDateNoticeSent { get; set; }
        public string DAEmailNoticeRemarks { get; set; }
        public string DAEmailNoticeAttachment { get; set; }
        public Nullable<System.DateTime> DACourierDateNoticeSent { get; set; }
        public Nullable<System.DateTime> DACourierDateNoticeReceived { get; set; }
        public string DACourierReceivedBy { get; set; }
        public string DACourierNoticeRemarks { get; set; }
        public string DACourierNoticeAttachment { get; set; }
        public string DAHandoverAssociate { get; set; }
        public string UnitType { get; set; }
        public string UnitTypeCode { get; set; }
        public string UnitTypeDesc { get; set; }
        public string UnitArea { get; set; }
        public string UnitMsrmnt { get; set; }
        public string UnitCategoryDesc { get; set; }
        public string CompanyName { get; set; }
    }
}
