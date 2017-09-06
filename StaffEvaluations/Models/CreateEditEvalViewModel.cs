using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StaffEvaluations.Models
{
    public class CreateEditEvalViewModel
    {

        public StaffPerformanceEvaluation eval { get; set; }

        public LibDirectoryIntegration.LibDirectoryPerson person { get; set; }

        public LibDirectoryIntegration.LibDirectoryPerson super { get; set; }

        public List<Question> questions { get; set; }

        public CreateEditEvalViewModel()
        {
            questions = new List<Question>();
        }
    }
}