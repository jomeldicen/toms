//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebApp.Console.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class UnitID_DeemedAcceptance
    {
        public int Id { get; set; }
        public string CompanyCode { get; set; }
        public string ProjectCode { get; set; }
        public string UnitCategory { get; set; }
        public string UnitNos { get; set; }
        public string CustomerNos { get; set; }
        public Nullable<System.DateTime> DeemedAcceptanceDate { get; set; }
        public string DeemedAcceptanceRemarks { get; set; }
        public Nullable<System.DateTime> EmailDateNoticeSent { get; set; }
        public Nullable<System.DateTime> EmailDateNoticeSentMaxDate { get; set; }
        public string EmailNoticeRemarks { get; set; }
        public string EmailNoticeAttachment { get; set; }
        public Nullable<System.DateTime> CourierDateNoticeSent { get; set; }
        public Nullable<System.DateTime> CourierDateNoticeReceived { get; set; }
        public string CourierReceivedBy { get; set; }
        public string CourierNoticeRemarks { get; set; }
        public string CourierNoticeAttachment { get; set; }
        public string HandoverAssociate { get; set; }
        public Nullable<int> IsDeemedAcceptanceDateSAPSync { get; set; }
        public Nullable<System.DateTime> DeemedAcceptanceDateSyncDate { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
    }
}
