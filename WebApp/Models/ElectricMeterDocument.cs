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
    
    public partial class ElectricMeterDocument
    {
        public string Id { get; set; }
        public int ElectricMeterID { get; set; }
        public int DocumentID { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string Remarks { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
    
        public virtual DocumentaryRequirement DocumentaryRequirement { get; set; }
        public virtual ElectricMeter ElectricMeter { get; set; }
    }
}
