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
    
    public partial class VW_UnitInventory
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
        public Nullable<System.DateTime> QCDAcceptanceDate { get; set; }
        public Nullable<System.DateTime> FPMCAcceptanceDate { get; set; }
        public string UnitAcceptanceRemarks { get; set; }
        public Nullable<bool> TOM { get; set; }
        public Nullable<bool> TSM { get; set; }
        public Nullable<bool> EMM { get; set; }
    }
}
