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
    
    public partial class ApprovalTemplateStage
    {
        public string Id { get; set; }
        public string ApprovalTemplateID { get; set; }
        public string ApprovalStageID { get; set; }
    
        public virtual ApprovalStage ApprovalStage { get; set; }
        public virtual ApprovalTemplate ApprovalTemplate { get; set; }
    }
}
