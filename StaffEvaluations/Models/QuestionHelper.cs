using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StaffEvaluations.Models
{
    public class QuestionHelper
    {

        public static List<Question> GetQuestions(Models.Entities db, string type)
        {
            List<Question> ret = new List<Question>();

            var yr = 2017;

            var questionlist = (from q in db.EvaluationQuestionSets where (q.QuestionType == type && q.Year == yr) select q).ToList();

            var convquestionlist = questionlist.Select(x => new Question() { QuestionText = x.QuestionText, QuestionCode = x.QuestionCode });

            foreach (Question q in convquestionlist)
            {
                ret.Add(new Models.Question { QuestionCode = q.QuestionCode, QuestionText = q.QuestionText });
            }

            //ret.

            return ret;
        }

        public static List<Question> GetQuestions(Models.Entities db, string type, int EvalId, List<StaffPerformanceQuestion> answers)
        {
            List<Question> qs = QuestionHelper.GetQuestions(db, type);

            foreach (StaffPerformanceQuestion q in answers)
            {
                Question current = qs.Where(qq => qq.QuestionCode == q.QuestionCode && EvalId == q.EvalId).Single();
                current.QuestionRating = q.Rating;
                current.QuestionComment = q.Comment;
                current.EvalId = q.EvalId;
                current.QuestionId = q.QuestionId;
            }

            return qs;
        }


        public static List<SelectListItem> GetRatings( string type, string selected = "")
        {
            List<SelectListItem> ret = new List<SelectListItem>();

            string val;

            if (type == "BA")
            {

                val = "Outstanding";
                ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

                val = "Solid Performer";
                ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

                val = "Needs Improvement";
                ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

                val = "Not Acceptable";
                ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

            }
            else if (type == "CA")
            {
                val = "Excellent";
                ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

                val = "Above Average";
                ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

                val = "Satisfactory";
                ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

                val = "Needs Improvement";
                ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

                val = "Unsatisfactory";
                ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

                val = "Not Applicable";
                ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

            }
            else if (type == "CC")
            {
                val = "Outstanding";
                ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

                val = "Exceeds Expectations";
                ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

                val = "Meets Expectations";
                ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

                val = "Needs Improvement";
                ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

                val = "Unsatisfactory";
                ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

            }

            val = "* You must select a value *";
            ret.Insert(0, new SelectListItem { Text = val, Value = val });

            return ret;
        }
    }
}