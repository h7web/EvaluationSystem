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
    
    public partial class EvaluationQuestionSet
    {
        public int QuestionSetId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionCode { get; set; }
        public string QuestionType { get; set; }
        public bool CommentOnly { get; set; }
        public int Year { get; set; }
    }
}
