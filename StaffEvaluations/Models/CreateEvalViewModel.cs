using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StaffEvaluations.Models
{
    public class CreateEvalViewModel
    {

        public StaffPerformanceEvaluation eval { get; set; }

        public List<Question> questions { get; set; }

        public CreateEvalViewModel()
        {
            questions = new List<Question>();
        }
    }
}