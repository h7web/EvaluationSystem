using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LibDirectoryIntegration;

namespace StaffEvaluations.Models
{

    public class IndexViewModel
    {
        public Supervisor Super { get; set; }

        public int EvalId { get; set; }

        public List<StaffPerformanceEvaluation> MyEvaluations
        {
            get; set;
        }

        public List<StaffPerformanceEvaluation> MyStaffEvaluations
        {
            get; set;
        }

    }
}