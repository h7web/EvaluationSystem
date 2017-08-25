using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StaffEvaluations.Models
{
    public class QuestionHelper
    {

        public static List<Question> GetQuestions(string type)
        {
            List<Question> ret = new List<Question>();

            if (type=="AP")
            {
                ret.Add(new Models.Question { QuestionCode = "AP1", QuestionText = "How was productivity?" });
                ret.Add(new Models.Question { QuestionCode = "AP2", QuestionText = "How was interpersonal skills?" });
                ret.Add(new Models.Question { QuestionCode = "AP3", QuestionText = "How was job knowledge?" });
                //etc
            }
            else if( type == "Civil Service")
            {
                //etc
            }

            return ret;
        }
    }
}