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
    
    public partial class VW_SalesInventory
    {
        public long Id { get; set; }
        public string SalesDocNos { get; set; }
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
        public string AddrNos { get; set; }
        public string Addrs { get; set; }
        public string SalesDocStatus { get; set; }
        public Nullable<System.DateTime> TOAS { get; set; }
        public Nullable<System.DateTime> SAPTurnoverDate { get; set; }
        public Nullable<System.DateTime> SalesDocDate { get; set; }
        public string AccountTypeCode { get; set; }
        public string AccountTypeDesc { get; set; }
        public string EmailAdd { get; set; }
        public string TelNos { get; set; }
        public Nullable<bool> EnableTOCutOffDate { get; set; }
        public Nullable<System.DateTime> TOCutOffDate { get; set; }
        public string TranClass { get; set; }
        public string CustomerHash { get; set; }
        public Nullable<System.DateTime> TitleInProcessDate { get; set; }
        public Nullable<System.DateTime> TitleTransferredDate { get; set; }
        public Nullable<System.DateTime> TitleClaimedDate { get; set; }
        public Nullable<System.DateTime> TaxDeclarationTransferredDate { get; set; }
        public Nullable<System.DateTime> TaxDeclarationClaimedDate { get; set; }
        public string QuotDocNos { get; set; }
        public bool TOM { get; set; }
        public bool TSM { get; set; }
        public bool EMM { get; set; }
    }
}
