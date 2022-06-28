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
    
    public partial class ApprovalStage
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ApprovalStage()
        {
            this.ApprovalStagesAuthorizers = new HashSet<ApprovalStagesAuthorizer>();
            this.ApprovalTemplateStages = new HashSet<ApprovalTemplateStage>();
        }
    
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ApprovalNos { get; set; }
        public int RejectionNos { get; set; }
        public bool Published { get; set; }
        public bool Applicability { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ApprovalStagesAuthorizer> ApprovalStagesAuthorizers { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ApprovalTemplateStage> ApprovalTemplateStages { get; set; }
    }
}
