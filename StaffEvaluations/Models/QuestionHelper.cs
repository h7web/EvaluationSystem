using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace StaffEvaluations.Models
{
    public class QuestionHelper
    {

        public static List<Question> GetQuestions(Models.Entities db, string type, string netid, string supervisorNetid, Int32 year)
        {
            List<Question> ret = new List<Question>();

            var yr = year;

            var unorderedquestionlist = (from q in db.EvaluationQuestionSets where (q.QuestionType == type && q.Year == yr) select q).ToList();

            var questionlist = unorderedquestionlist
                .Select(item => new
                {
                    value = item,
                    match = Regex.Match(item.QuestionCode, @"^(?<name>.*?)\s*(?<number>[0-9]*)$"),
                })
                .OrderBy(item => item.match.Groups["name"].Value)
                .ThenBy(item => item.match.Groups["number"].Value.Length)
                .ThenBy(item => item.match.Groups["number"].Value)
                .Select(item => item.value);

            var convquestionlist = questionlist.Select(x => new Question() { QuestionText = x.QuestionText, QuestionCode = x.QuestionCode, CommentOnly = x.CommentOnly });

            var jd = GetJD(db, netid, supervisorNetid);
                
            foreach (Question q in convquestionlist)
            {
                if (q.QuestionText == "Job Description")
                {
                    ret.Add(new Models.Question { QuestionCode = q.QuestionCode, QuestionText = q.QuestionText, QuestionComment = jd, CommentOnly = q.CommentOnly, highlight = q.highlight });
                }
                else
                {
                    ret.Add(new Models.Question { QuestionCode = q.QuestionCode, QuestionText = q.QuestionText, CommentOnly = q.CommentOnly, highlight = q.highlight });
                }
            }

            //ret.

            return ret;
        }

        public static List<Question> GetQuestions(Models.Entities db, string type, int EvalId, List<StaffPerformanceQuestion> answers)
        {
            var eval = (from e in db.StaffPerformanceEvaluations where e.EvalId == EvalId select e).SingleOrDefault();
            string netid = eval.NetId;
            string supervisorNetid = eval.EvaluatorNetid;
            string nalist = "AP1AP2AP3AP4AP5AP6AP7AP8";

            var needshighlighted = (from r in db.Ratings where r.CommentRequired == true select r.Rating1).ToList();

            List<Question> qs = QuestionHelper.GetQuestions(db, type, netid, supervisorNetid, eval.Year);

            foreach (StaffPerformanceQuestion q in answers)
            {
                Question current = qs.Where(qq => qq.QuestionCode == q.QuestionCode && EvalId == q.EvalId).Single();
                current.QuestionRating = q.Rating;
                current.QuestionComment = q.Comment;
                current.EvalId = q.EvalId;
                current.QuestionId = q.QuestionId;

                var qr = q.Rating;

                if (qr == null)
                {
                    qr = "notapplicable";
                }

                if (needshighlighted.Contains(qr)) //if the rating requires a comment
                {
                    current.highlight = "true";
                }
                else if (current.QuestionText != "Job Description" && current.QuestionRating == "* You must select a value *") //if rating not selected
                {
                    current.highlight = "true";
                }
                else if (current.QuestionText == "Job Description" && current.QuestionComment == null) //if job desc not entered
                {
                    current.highlight = "true";
                }
                else if (current.QuestionRating != null && nalist.Contains(current.QuestionRating)) //if rating of Not Applicable chosen for question that is NOT optional
                {
                    current.highlight = "true";
                }
                else
                {
                    current.highlight = "false";
                }
            }

            return qs;
        }

        public static string GetJD(Models.Entities db, string netid, string supervisorNetid)
        {
            var JD = "";
            var JDEntry = (from j in db.JobDescriptions where j.netid == netid && j.supervisorNetid == supervisorNetid select j).SingleOrDefault();

            if (JDEntry != null)
            {
                JD = JDEntry.description;
            }
            return JD;
        }

        public static List<SelectListItem> GetRatings(Models.Entities db, string type, string selected = "")
        {
            List<SelectListItem> ret = new List<SelectListItem>();

            var val = from r in db.Ratings where r.EvalCode == type select r;

            foreach (Rating r in val)
            {
                ret.Add(new SelectListItem { Text = r.Rating1, Value = r.Rating1, Selected = (r.Rating1 == selected) });
            }

            //if (type == "BA")
            //{

            //    val = "Outstanding";
            //    ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

            //    val = "Solid Performer";
            //    ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

            //    val = "Needs Improvement";
            //    ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

            //    val = "Not Acceptable";
            //    ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

            //}
            //else if (type == "CA")
            //{
            //    val = "Excellent";
            //    ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

            //    val = "Above Average";
            //    ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

            //    val = "Satisfactory";
            //    ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

            //    val = "Needs Improvement";
            //    ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

            //    val = "Unsatisfactory";
            //    ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

            //    val = "Not Applicable";
            //    ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

            //}
            //else if (type == "CC")
            //{
            //    val = "Outstanding";
            //    ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

            //    val = "Exceeds Expectations";
            //    ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

            //    val = "Meets Expectations";
            //    ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

            //    val = "Needs Improvement";
            //    ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

            //    val = "Unsatisfactory";
            //    ret.Add(new SelectListItem { Text = val, Value = val, Selected = (val == selected) });

            //}

            ret.Insert(0, new SelectListItem { Text = "* You must select a value *", Value = "* You must select a value *" });

            return ret;
        }
    }
}