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
            else if (type == "CA")
            {
                ret.Add(new Models.Question { QuestionCode = "CA1", QuestionText = "Quantity of work <br> extent to which the employee meets job requirements on a timely basis" });
                ret.Add(new Models.Question { QuestionCode = "CA2", QuestionText = "Quality of work <br> extent to which the employee's work is thorough, effective and accurate" });
                ret.Add(new Models.Question { QuestionCode = "CA3", QuestionText = "Knowledge of job <Br> extent to which the employee knows and demonstrates all phases of assigned work" });
                ret.Add(new Models.Question { QuestionCode = "CA4", QuestionText = "Cooperation with others <br> extent to which the employee gets along well with others; responds positively to direction and adapts well to changes; shows tact, courtesy and effectiveness in dealing with others" });
                ret.Add(new Models.Question { QuestionCode = "CA5", QuestionText = "Judgment <br> extent to which the employee makes sound job - related decisions, develops alternative solutions and recommendations and selects proper course of action; understands impact of decisions and actions" });
            }
            else if (type == "CC")
            {
                ret.Add(new Models.Question { QuestionCode = "AP1", QuestionText = "Job Knowledge: Demonstrates knowledge and skills necessary to perform the job effectively including language, grammar, spelling, mathematics, reasoning, and any job-specific technical procedural competencies.  In certain positions, this includes knowledge of university policies, rules, procedures, and their supporting statutes. Understands the expectations of the job and remains current regarding new developments, technologies, methods, theories, approaches, and processes in area of responsibility." });
                ret.Add(new Models.Question { QuestionCode = "AP1", QuestionText = "Judgment:  Anticipates and identifies problems; evaluates alternative solutions; is open to new or different solutions.  Demonstrates maturity in taking or recommending appropriate actions and in determining which problems to handle independently and which to refer to more senior personnel; follows up on problems and helps to bring about resolution.  If the employee is serving in a supervisory capacity, delegates tasks wisely, and follows up on tasks assigned to others." });
                ret.Add(new Models.Question { QuestionCode = "AP1", QuestionText = "Reliability; Commitment to the Job: Works efficiently; uses time effectively; takes initiative in addressing problems.  Meets promised deadlines without sacrifice of accuracy, quality, or service recipient satisfaction; reports unavoidable delays well in advance of deadline.  Meets the work schedule expectations of the position.  Demonstrates flexibility and willingness to assist by taking on difficult or inconvenient responsibilities.  Complies with University and unit policies and procedures." });
                ret.Add(new Models.Question { QuestionCode = "AP1", QuestionText = "Quality/Quantity of Work; Customer Service: Meets service recipient and departmental expectations for quality/quantity of work; accurately and thoroughly completes work.  Listens to and understands the needs of the service recipient, whether inside or outside the University, and responds to those needs.  Uses collaborative solutions in problem-solving as appropriate.  Delivers work product and services to consumers in a way that reflects credit upon the unit and the University." });
                ret.Add(new Models.Question { QuestionCode = "AP1", QuestionText = "Interpersonal and Communication Skills: Effectively conveys ideas and information in writing and/or orally; productively participates in meetings.  Contributes to a suitable communication environment.  Demonstrates respect for all individuals regardless of their background or culture; participates in processes that encourage diversity and equal opportunity." });
            }

            //ret.

            return ret;
        }

        public static List<Question> GetQuestions(string type, List<StaffPerformanceQuestion> answers)
        {
            List<Question> qs = QuestionHelper.GetQuestions(type);

            foreach (StaffPerformanceQuestion q in answers)
            {
                Question current = qs.Where(qq => qq.QuestionCode == q.QuestionCode).Single();
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

            if (type == "AP")
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