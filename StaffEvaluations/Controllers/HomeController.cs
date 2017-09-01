using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StaffEvaluations.Models;
using System.Net.Mail;

namespace StaffEvaluations.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {

        private Models.StaffEvaluationsEntities db = new Models.StaffEvaluationsEntities();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult Index()
        {
            List<Person> reportees = new List<Person>();

            reportees.Add(new Models.Person { NetId = "yoskye", Name = "Skye Arseneau", EmployeeType = "CC" });
            reportees.Add(new Models.Person { NetId = "atJohnsn", Name = "Anietre Johnson", EmployeeType = "CA" });
            reportees.Add(new Models.Person { NetId = "mikesweb", Name = "Mike Nelson", EmployeeType = "AP" });
            reportees.Add(new Models.Person { NetId = "strutz", Name = "Jason Strutz", EmployeeType = "AP" });


            IndexViewModel vm = new IndexViewModel();
            vm.DirectReports = reportees;
            vm.NetId = HttpContext.User.Identity.Name.Substring(5);

            var myEvals = from e in db.StaffPerformanceEvaluations where e.Status == Constants.Submitted && e.NetId == vm.NetId select e;

            var myStaffEvals = from e in db.StaffPerformanceEvaluations where e.EvaluatorNetid == vm.NetId select e;

            vm.MyEvaluations = myEvals.ToList();
            vm.MyStaffEvaluations = myStaffEvals.ToList();


            return View(vm);
        }

        public ActionResult CreateEval(string id, string type)
        {
            StaffPerformanceEvaluation newEval = new StaffPerformanceEvaluation();
            newEval.NetId = id;
            newEval.EvalCode = type;

            CreateEditEvalViewModel crvm = new CreateEditEvalViewModel();

            crvm.eval = newEval;
            crvm.questions = QuestionHelper.GetQuestions(type);

            ViewData["RatingList"] = QuestionHelper.GetRatings(type);

            return View(crvm);
        }

        [HttpPost]
        public ActionResult CreateEval(string id, string type, List<Question> question)
        {
            StaffPerformanceEvaluation newEval = new StaffPerformanceEvaluation();
            newEval.NetId = id;
            newEval.EvaluatorNetid = HttpContext.User.Identity.Name.Substring(5);
            newEval.Year = DateTime.Now.Year;
            newEval.EvalCode = type;
            newEval.Status = "In-Work";
            newEval.StartDate = DateTime.Now;
            db.StaffPerformanceEvaluations.Add(newEval);
            db.SaveChanges();

            foreach (Question myQuestion in question)
            {
                StaffPerformanceQuestion newQuestion = new Models.StaffPerformanceQuestion()
                {
                    EvalId = newEval.EvalId,
                    Comment = myQuestion.QuestionComment,
                    Rating = myQuestion.QuestionRating,
                    QuestionCode = myQuestion.QuestionCode,
                    FirstAnsweredDate = DateTime.Now
                };
                db.StaffPerformanceQuestions.Add(newQuestion);
            }

            db.SaveChanges();


            return RedirectToAction("Index");
        }

        public ActionResult EditEval(int id)
        {
            var getEval = (from e in db.StaffPerformanceEvaluations where e.EvalId == id select e).Single();

            CreateEditEvalViewModel crvm = new CreateEditEvalViewModel();

            crvm.eval = getEval;
            crvm.questions = QuestionHelper.GetQuestions(getEval.EvalCode,getEval.StaffPerformanceQuestions.ToList());

            ViewData["RatingList"] = QuestionHelper.GetRatings(getEval.EvalCode);

            return View(crvm);
        }

        [HttpPost]
        public ActionResult EditEval(int id, string EmployeeComments, string EvaluatorComments, string button, List<Question> question)
        {

            foreach (Question q in question)
            {
                var orig = db.StaffPerformanceQuestions.Find(q.QuestionId);
                if (orig.Comment != q.QuestionComment || orig.Rating != q.QuestionRating)
                    {
                        orig.Comment = q.QuestionComment;
                        orig.Rating = q.QuestionRating;
                        orig.LastUpdateDate = DateTime.Now;
                        db.SaveChanges();
                    }
            }

            var eval = db.StaffPerformanceEvaluations.Find(id);
            if (eval.EvaluatorComments != EvaluatorComments || eval.EmployeeComments != EmployeeComments)
            {
                eval.EmployeeComments = EmployeeComments;
                eval.EvaluatorComments = EvaluatorComments;

                db.SaveChanges();
            }

            if (button.Equals("Complete"))
            {
                eval.Status = "Complete";
            }

            if (eval.NetId == HttpContext.User.Identity.Name.Substring(5) && button == "Accept")
            {
                eval.Status = "Accepted";
            }

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public async System.Threading.Tasks.Task<ActionResult> SubmitEval(int id)
        {
            var eval = db.StaffPerformanceEvaluations.Find(id);
            eval.Status = "Submitted";
            eval.SubmittedDate = DateTime.Now;
            db.SaveChanges();

            var body = "<p>Your " + eval.Year + " Performance Evaluation prepared by " + eval.EvaluatorNetid + " is available for you to review and comment at the following URL:</p>";
            body = body + "http://iisdev1.library.illinois.edu/StaffEvaluations/";
            var message = new MailMessage();

            message.To.Add(new MailAddress(eval.EvaluatorNetid + "@illinois.edu"));
            message.From = new MailAddress(eval.EvaluatorNetid + "@illinois.edu");
            message.Subject = eval.Year + " Performance Evaluation";
            message.Body = body;
            message.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                smtp.Host = "Express-SMTP.cites.illinois.edu ";
                await smtp.SendMailAsync(message);
            }

            return RedirectToAction("Index");
        }


    }
}