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
    
    public partial class ElectricMeterInvServiceDeposit
    {
        public int Id { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string UnitCategory { get; set; }
        public string UnitCategoryDesc { get; set; }
        public string UnitNos { get; set; }
        public string RefNos { get; set; }
        public decimal MeterDepositAmount { get; set; }
        public bool Published { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public string TowerCode { get; set; }
        public string TowerName { get; set; }
    }
}
