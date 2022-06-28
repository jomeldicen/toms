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
    
    public partial class VW_TitlingStatus
    {
        public long Id { get; set; }
        public string SalesDocNos { get; set; }
        public string QuotDocNos { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public int ProjectId { get; set; }
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
        public Nullable<System.DateTime> InProcessRefDate { get; set; }
        public string InProcessRefField { get; set; }
        public Nullable<System.DateTime> TitleInProcessDate { get; set; }
        public string TitleInProcessRemarks { get; set; }
        public Nullable<int> TitleInProcessTAT { get; set; }
        public Nullable<int> TitleInProcessSysTAT { get; set; }
        public string TitleInProcessGroup { get; set; }
        public Nullable<System.DateTime> TitleTransferredDate { get; set; }
        public string TitleTransferredRemarks { get; set; }
        public Nullable<int> TitleTransferredTAT { get; set; }
        public Nullable<int> TitleTransferredSysTAT { get; set; }
        public string TitleTransferredGroup { get; set; }
        public Nullable<System.DateTime> TaxDeclarationTransferredDate { get; set; }
        public string TaxDeclarationTransferredRemarks { get; set; }
        public Nullable<int> TaxDeclarationTransferredTAT { get; set; }
        public Nullable<int> TaxDeclarationTransferredSysTAT { get; set; }
        public string TaxDeclarationTransferredGroup { get; set; }
        public string TaxDecNos { get; set; }
        public Nullable<System.DateTime> LiquidationEndorsedDate { get; set; }
        public Nullable<int> LiquidationEndorsedTAT { get; set; }
        public Nullable<int> LiquidationEndorsedSysTAT { get; set; }
        public string LiquidationEndorsedGroup { get; set; }
        public string LiquidationRushTicketNos { get; set; }
        public string LiquidationEndorsedRemarks { get; set; }
        public Nullable<System.DateTime> TitleReleaseEndorsedDate { get; set; }
        public Nullable<int> TitleReleaseEndorsedTAT { get; set; }
        public Nullable<int> TitleReleaseEndorsedSysTAT { get; set; }
        public string TitleReleaseEndorsedGroup { get; set; }
        public string TitleReleaseRushTicketNos { get; set; }
        public string TitleReleaseEndorsedRemarks { get; set; }
        public Nullable<System.DateTime> TitleClaimedDate { get; set; }
        public string TitleClaimedRemarks { get; set; }
        public Nullable<int> TitleClaimedTAT { get; set; }
        public Nullable<int> TitleClaimedSysTAT { get; set; }
        public string TitleClaimedGroup { get; set; }
        public Nullable<System.DateTime> TaxDeclarationClaimedDate { get; set; }
        public string TaxDeclarationClaimedRemarks { get; set; }
        public Nullable<int> TaxDeclarationClaimedTAT { get; set; }
        public Nullable<int> TaxDeclarationClaimedSysTAT { get; set; }
        public string TaxDeclarationClaimedGroup { get; set; }
        public string TitleLocationName { get; set; }
        public string TitleNos { get; set; }
        public string TitleRemarks { get; set; }
        public Nullable<System.DateTime> BuyerReleasedDate { get; set; }
        public Nullable<int> BuyerReleasedTAT { get; set; }
        public Nullable<int> BuyerReleasedSysTAT { get; set; }
        public string BuyerReleasedGroup { get; set; }
        public string BuyerReleasedRemarks { get; set; }
        public Nullable<System.DateTime> BankReleasedDate { get; set; }
        public Nullable<int> BankReleasedTAT { get; set; }
        public Nullable<int> BankReleasedSysTAT { get; set; }
        public string BankReleasedGroup { get; set; }
        public string BankReleasedRemarks { get; set; }
        public string TitleStatus { get; set; }
        public Nullable<System.DateTime> MeralcoReceiptDate { get; set; }
        public Nullable<System.DateTime> UnitOwnerReceiptDate { get; set; }
        public Nullable<int> AgingTOASTaxDecTransfer { get; set; }
        public Nullable<int> AgingQualifTaxDecTransfer { get; set; }
        public Nullable<int> AgingTTransferReleaseBank { get; set; }
        public Nullable<int> AgingTTransferReleaseBuyer { get; set; }
        public Nullable<System.DateTime> TSCutOffDate { get; set; }
        public string TranClass { get; set; }
        public Nullable<System.DateTime> MeralcoSubmittedDate { get; set; }
    }
}
