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
    
    public partial class Rating
    {
        public int rid { get; set; }
        public string EvalCode { get; set; }
        public string Rating1 { get; set; }
        public bool CommentRequired { get; set; }
        public int Year { get; set; }
    }
}
