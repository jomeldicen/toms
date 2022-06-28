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
    
    public partial class VW_DashboardProcessingJobElectric
    {
        public int ProcessJobID { get; set; }
        public string ObjectType { get; set; }
        public string SalesDocNos { get; set; }
        public string QuotDocNos { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public Nullable<int> ProjectId { get; set; }
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
        public string CustomerHash { get; set; }
        public string EmailAdd { get; set; }
        public string ContactPerson { get; set; }
        public string TelNos { get; set; }
        public string AddrNos { get; set; }
        public string Addrs { get; set; }
        public string AccountTypeCode { get; set; }
        public string AccountTypeDesc { get; set; }
        public string SalesDocStatus { get; set; }
        public Nullable<System.DateTime> TOAS { get; set; }
        public Nullable<System.DateTime> QualificationDate { get; set; }
        public Nullable<System.DateTime> SAPTurnoverDate { get; set; }
        public Nullable<System.DateTime> TurnoverDate { get; set; }
        public Nullable<System.DateTime> TitleInProcessDate { get; set; }
        public Nullable<System.DateTime> TitleTransferredDate { get; set; }
        public Nullable<System.DateTime> TaxDeclarationTransferredDate { get; set; }
        public Nullable<System.DateTime> LiquidationEndorsedDate { get; set; }
        public Nullable<System.DateTime> TitleReleaseEndorsedDate { get; set; }
        public Nullable<System.DateTime> TitleClaimedDate { get; set; }
        public Nullable<System.DateTime> TaxDeclarationClaimedDate { get; set; }
        public Nullable<System.DateTime> BuyerReleasedDate { get; set; }
        public Nullable<System.DateTime> BankReleasedDate { get; set; }
        public Nullable<decimal> MeterDepositAmount { get; set; }
        public string ApplicationProcessStatus { get; set; }
        public Nullable<System.DateTime> DocumentaryCompletedDate { get; set; }
        public Nullable<System.DateTime> DocumentaryLastModifedDate { get; set; }
        public string DocumentaryRemarks { get; set; }
        public Nullable<System.DateTime> CCTReceivedDate { get; set; }
        public Nullable<int> DocCompletionTAT { get; set; }
        public Nullable<int> DocCompletionSysTAT { get; set; }
        public string DocCompletionGroup { get; set; }
        public Nullable<System.DateTime> RFPRushTicketDate { get; set; }
        public Nullable<int> RFPCreationTAT { get; set; }
        public Nullable<int> RFPCreationSysTAT { get; set; }
        public string RFPCreationGroup { get; set; }
        public string RFPRushTicketNos { get; set; }
        public Nullable<bool> IsReceivedCheck { get; set; }
        public Nullable<System.DateTime> ReceivedCheckDate { get; set; }
        public Nullable<int> CheckPaymentReleaseTAT { get; set; }
        public Nullable<int> CheckPaymentReleaseSysTAT { get; set; }
        public string CheckPaymentReleaseGroup { get; set; }
        public string RFPRushTicketRemarks { get; set; }
        public Nullable<bool> WithUnpaidBills { get; set; }
        public Nullable<System.DateTime> UnpaidBillPostedDate { get; set; }
        public Nullable<bool> IsPaidSettled { get; set; }
        public Nullable<System.DateTime> PaidSettledPostedDate { get; set; }
        public string DepositApplicationRemarks { get; set; }
        public Nullable<System.DateTime> MeralcoSubmittedDate { get; set; }
        public Nullable<int> MeralcoSubmissionTAT { get; set; }
        public Nullable<int> MeralcoSubmissionSysTAT { get; set; }
        public string MeralcoSubmissionGroup { get; set; }
        public string MeralcoSubmittedRemarks { get; set; }
        public Nullable<System.DateTime> MeralcoReceiptDate { get; set; }
        public Nullable<int> TransferElectricServTAT { get; set; }
        public Nullable<int> TransferElectricServSysTAT { get; set; }
        public string TransferElectricServGroup { get; set; }
        public string MeralcoReceiptRemarks { get; set; }
        public Nullable<System.DateTime> UnitOwnerReceiptDate { get; set; }
        public string UnitOwnerReceiptRemarks { get; set; }
        public Nullable<int> AgingRFPRushToCheckRelease { get; set; }
        public Nullable<int> AgingMeralcoSubmittedToReceipt { get; set; }
        public string ElectricMeterStatus { get; set; }
        public Nullable<System.DateTime> EMCutOffDate { get; set; }
        public string TranClass { get; set; }
        public string ReceivedCheckRemarks { get; set; }
    }
}