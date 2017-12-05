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
    
    public partial class StaffPerformanceEvaluation
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public StaffPerformanceEvaluation()
        {
            this.StaffPerformanceQuestions = new HashSet<StaffPerformanceQuestion>();
        }
    
        public int EvalId { get; set; }
        public string EvalCode { get; set; }
        public int Year { get; set; }
        public string NetId { get; set; }
        public string Title { get; set; }
        public string EvaluatorNetid { get; set; }
        public string Status { get; set; }
        public System.DateTime StartDate { get; set; }
        public Nullable<System.DateTime> DeferredDate { get; set; }
        public Nullable<System.DateTime> SubmittedDate { get; set; }
        public Nullable<System.DateTime> AcceptedDate { get; set; }
        public Nullable<System.DateTime> ContestedDate { get; set; }
        public Nullable<System.DateTime> CompleteDate { get; set; }
        public Nullable<System.DateTime> ProcessedDate { get; set; }
        public string EmployeeComments { get; set; }
        public string EvaluatorComments { get; set; }
        public string TouchedByMasqeradeNetID { get; set; }
        public Nullable<System.DateTime> TouchedByMasqeradeDate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StaffPerformanceQuestion> StaffPerformanceQuestions { get; set; }
    }
}
