//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace StaffEvaluations.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class EvalEmail
    {
        public int id { get; set; }
        public string list { get; set; }
        public string interval { get; set; }
        public System.DateTime send_date { get; set; }
        public string email_subject { get; set; }
        public string email_body { get; set; }
        public Nullable<System.DateTime> last_run_date { get; set; }
        public string email_desc { get; set; }
        public Nullable<bool> deptheadsonly { get; set; }
    }
}
