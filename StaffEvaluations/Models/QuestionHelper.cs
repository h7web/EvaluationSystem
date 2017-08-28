using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StaffEvaluations.Models
{
    public class QuestionHelper
    {

        public static List<Question> GetQuestions(string type)
        {
            List<Question> ret = new List<Question>();

            if (type == "AP")
            {
                ret.Add(new Models.Question { QuestionCode = "AP1", QuestionText = "How was productivity?" });
                ret.Add(new Models.Question { QuestionCode = "AP2", QuestionText = "How was interpersonal skills?" });
                ret.Add(new Models.Question { QuestionCode = "AP3", QuestionText = "How was job knowledge?" });
                //etc
            }
            else if (type == "Civil Service")
            {
                //etc
            }

            return ret;
        }

        public static List<SelectListItem> GetRatings(string type, string selected = "")
        {
            List<SelectListItem> ret = new List<SelectListItem>();

            string val;

            if (type == "AP")
            {

                val = "Outstanding";
                ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

                val = "Solid Performer";
                ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

                val = "Needs Improvement";
                ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

                val = "Not Applicable";
                ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

            }
            else if (type == "todo")
            {
                //etc.
            }

            val = "* You must select a value *";
            ret.Insert(0, new SelectListItem { Text = val, Value = val });

            return ret;
        }
    }
}