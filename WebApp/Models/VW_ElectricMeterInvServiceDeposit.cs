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
    
    public partial class VW_ElectricMeterInvServiceDeposit
    {
        public long Id { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public Nullable<int> ProjectId { get; set; }
        public string ProjectCode { get; set; }
        public string BusinessEntity { get; set; }
        public string ProjectLocation { get; set; }
        public string UnitNos { get; set; }
        public string RefNos { get; set; }
        public string Intreno { get; set; }
        public string UnitType { get; set; }
        public string UnitTypeDesc { get; set; }
        public string UnitArea { get; set; }
        public string UnitCategoryCode { get; set; }
        public string UnitCategoryDesc { get; set; }
        public string ZoneNos { get; set; }
        public string ZoneType { get; set; }
        public string Phase { get; set; }
        public decimal MeterDepositAmount { get; set; }
        public Nullable<int> MeterDepositId { get; set; }
        public Nullable<bool> Published { get; set; }
        public string CreatedByPK { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CustomerNos { get; set; }
        public Nullable<System.DateTime> MeralcoSubmittedDate { get; set; }
        public Nullable<System.DateTime> MeralcoReceiptDate { get; set; }
        public Nullable<System.DateTime> UnitOwnerReceiptDate { get; set; }
        public string SalesDocNos { get; set; }
        public string QuotDocNos { get; set; }
        public Nullable<int> ElectricMeterId { get; set; }
    }
}