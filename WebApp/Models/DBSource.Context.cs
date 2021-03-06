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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class WebAPPv0MainEntities : DbContext
    {
        public WebAPPv0MainEntities()
            : base("name=WebAPPv0MainEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Setting> Settings { get; set; }
        public virtual DbSet<Company> Companies { get; set; }
        public virtual DbSet<UnitInventory> UnitInventories { get; set; }
        public virtual DbSet<UnitType> UnitTypes { get; set; }
        public virtual DbSet<SalesInventory> SalesInventories { get; set; }
        public virtual DbSet<Phase> Phases { get; set; }
        public virtual DbSet<ZresCsHeader> ZresCsHeaders { get; set; }
        public virtual DbSet<ZresToExtend> ZresToExtends { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<CustomerConPerson> CustomerConPersons { get; set; }
        public virtual DbSet<CustomerContact> CustomerContacts { get; set; }
        public virtual DbSet<CustomerEmail> CustomerEmails { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
    
        public virtual int SP_PermissionControls(string roleID, string nvPageUrl)
        {
            var roleIDParameter = roleID != null ?
                new ObjectParameter("RoleID", roleID) :
                new ObjectParameter("RoleID", typeof(string));
    
            var nvPageUrlParameter = nvPageUrl != null ?
                new ObjectParameter("nvPageUrl", nvPageUrl) :
                new ObjectParameter("nvPageUrl", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("SP_PermissionControls", roleIDParameter, nvPageUrlParameter);
        }
    }
}
