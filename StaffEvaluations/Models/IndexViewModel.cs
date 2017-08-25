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

        public List<Person> DirectReports {
            get; set;
        }

        public StaffPerformanceEvaluation MyEvaluation
        {
            get; set;
        }

        public List<StaffPerformanceEvaluation> MyStaffEvaluations
        {
            get; set;
        }

    }
}