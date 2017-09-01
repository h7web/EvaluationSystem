using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StaffEvaluations.Models
{

    public class IndexViewModel
    {
        public string NetId {
            get; set;
        }

        public int EvalId { get; set; }

        public List<Person> DirectReports {
            get; set;
        }

        public List<StaffPerformanceEvaluationxxx> MyEvaluations
        {
            get; set;
        }

        public List<StaffPerformanceEvaluationxxx> MyStaffEvaluations
        {
            get; set;
        }

    }
}