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
    
    public partial class VW_QualifyList
    {
        public int Id { get; set; }
        public string CompanyCode { get; set; }
        public string ProjectCode { get; set; }
        public string BusinessEntity { get; set; }
        public string UnitCategory { get; set; }
        public string UnitNos { get; set; }
        public string RefNos { get; set; }
        public string CustomerNos { get; set; }
        public string CustomerName1 { get; set; }
        public System.DateTime TOAS { get; set; }
        public System.DateTime OccupancyPermitDate { get; set; }
        public System.DateTime CMGAcceptanceDate { get; set; }
        public System.DateTime QualificationDate { get; set; }
        public string SalesDocNos { get; set; }
        public string QuotDocNos { get; set; }
        public string SalesDocStatus { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public int NoticeTOID { get; set; }
    }
}