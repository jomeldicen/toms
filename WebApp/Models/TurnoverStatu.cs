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
    
    public partial class TurnoverStatu
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Published { get; set; }
        public string Applicability { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual AspNetUser AspNetUser1 { get; set; }
    }
}
