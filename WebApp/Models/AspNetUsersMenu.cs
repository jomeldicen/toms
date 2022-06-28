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
    
    public partial class AspNetUsersMenu
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public AspNetUsersMenu()
        {
            this.AspNetUsersMenu1 = new HashSet<AspNetUsersMenu>();
            this.AspNetUsersMenuPermissions = new HashSet<AspNetUsersMenuPermission>();
            this.AspNetUsersMenuControls = new HashSet<AspNetUsersMenuControl>();
            this.ApprovalTemplateModules = new HashSet<ApprovalTemplateModule>();
            this.ChangeLogs = new HashSet<ChangeLog>();
        }
    
        public string vMenuID { get; set; }
        public string nvMenuName { get; set; }
        public int iSerialNo { get; set; }
        public string nvFabIcon { get; set; }
        public string vParentMenuID { get; set; }
        public string nvPageUrl { get; set; }
        public string PrefixCode { get; set; }
        public bool Published { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AspNetUsersMenu> AspNetUsersMenu1 { get; set; }
        public virtual AspNetUsersMenu AspNetUsersMenu2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AspNetUsersMenuPermission> AspNetUsersMenuPermissions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AspNetUsersMenuControl> AspNetUsersMenuControls { get; set; }
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual AspNetUser AspNetUser1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ApprovalTemplateModule> ApprovalTemplateModules { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChangeLog> ChangeLogs { get; set; }
    }
}
