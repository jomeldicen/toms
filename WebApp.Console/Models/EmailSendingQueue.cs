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
    
    public partial class EmailSendingQueue
    {
        public int Id { get; set; }
        public string Module { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public bool IsEmailSent { get; set; }
        public System.DateTime DateProcess { get; set; }
        public Nullable<System.DateTime> DateSent { get; set; }
        public string Reference { get; set; }
        public string Remarks { get; set; }
    }
}
