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
    
    public partial class StaffPerformanceQuestion
    {
        public int QuestionId { get; set; }
        public string QuestionCode { get; set; }
        public int EvalId { get; set; }
        public string Rating { get; set; }
        public string Comment { get; set; }
        public System.DateTime FirstAnsweredDate { get; set; }
        public Nullable<System.DateTime> LastUpdateDate { get; set; }
    
        public virtual StaffPerformanceEvaluation StaffPerformanceEvaluation { get; set; }
    }
}
