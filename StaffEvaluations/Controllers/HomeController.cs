using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StaffEvaluations.Models;
using System.Net.Mail;
using LibDirectoryIntegration;

namespace StaffEvaluations.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private Models.StaffEvaluationsEntities db = new Models.StaffEvaluationsEntities();
        private Models.HR_DataEntities db1 = new Models.HR_DataEntities();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
                db1.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult Index()
        {
            Supervisor super = LibDirectoryFactory.GetSupervisor(User.Identity.Name.Substring(5));

            IndexViewModel vm = new IndexViewModel();
            vm.Super = super;

            if (vm.Super.direct_reports != null)
            {
                foreach (LibDirectoryPerson lp in vm.Super.direct_reports)
                {
                    var eclass = (from e in db1.employees where e.NETID == lp.netid select e.ECLASS).FirstOrDefault();
                    lp.employee_type_code = eclass;
                }
            }

            // next 5 lines added for testing purposes
            vm.Super.direct_reports = new List<DirectReport>();
            vm.Super.direct_reports.Add(new DirectReport() { netid = "yoskye", name = "Skye Arseneau", employee_type_code = "CC" });
            vm.Super.direct_reports.Add(new DirectReport { netid = "atJohnsn", name = "Anietre Johnson", employee_type_code = "CA" });
            vm.Super.direct_reports.Add(new DirectReport { netid = "mikesweb", name = "Mike Nelson", employee_type_code = "BA" });
            vm.Super.direct_reports.Add(new DirectReport { netid = "strutz", name = "Jason Strutz", employee_type_code = "BA" });

            var myEvals = from e in db.StaffPerformanceEvaluations where (e.Status == Constants.Submitted || e.Status == Constants.Complete) && e.NetId == HttpContext.User.Identity.Name.Substring(5) select e;

            var myStaffEvals = (from e in db.StaffPerformanceEvaluations where e.EvaluatorNetid == vm.Super.netid || e.EvaluatorNetid == HttpContext.User.Identity.Name.Substring(5) select e).ToList();

            vm.MyEvaluations = myEvals.ToList();
            vm.MyStaffEvaluations = myStaffEvals;


            return View(vm);
        }

        public ActionResult CreateEval(string id, string type)
        {
            StaffPerformanceEvaluation newEval = new StaffPerformanceEvaluation();
            newEval.NetId = id;
            newEval.EvalCode = type;

            var reportinfo = LibDirectoryFactory.GetPerson(id);

            var superinfo = LibDirectoryFactory.GetPerson(User.Identity.Name.Substring(5));

            var lsdate = (from e in db1.employees where e.NETID == id select e.LIBRARY_START_DATE).FirstOrDefault().ToString();

            CreateEditEvalViewModel crvm = new CreateEditEvalViewModel();

            crvm.person = new LibDirectoryPerson();
            crvm.person = reportinfo;
            crvm.person.LibraryStartDate = lsdate;

            crvm.super = new LibDirectoryPerson();
            crvm.super = superinfo;

            crvm.eval = newEval;
            crvm.questions = QuestionHelper.GetQuestions(db, type);

            ViewData["RatingList"] = QuestionHelper.GetRatings(type);

            return View(crvm);
        }

        [HttpPost]
        public ActionResult CreateEval(string id, string type, string title, List<Question> question)
        {
            StaffPerformanceEvaluation newEval = new StaffPerformanceEvaluation();
            newEval.NetId = id;
            newEval.EvaluatorNetid = HttpContext.User.Identity.Name.Substring(5);
            newEval.Year = DateTime.Now.Year;
            newEval.EvalCode = type;
            newEval.Status = "In-Work";
            newEval.Title = title;
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

            var reportinfo = LibDirectoryFactory.GetPerson(getEval.NetId);

            var superinfo = LibDirectoryFactory.GetPerson(User.Identity.Name.Substring(5));

            var lsdate = (from e in db1.employees where e.NETID == getEval.NetId select e.LIBRARY_START_DATE).FirstOrDefault().ToString();

            CreateEditEvalViewModel crvm = new CreateEditEvalViewModel();

            crvm.person = new LibDirectoryPerson();
            crvm.person = reportinfo;
            crvm.person.LibraryStartDate = lsdate;

            crvm.super = new LibDirectoryPerson();
            crvm.super = superinfo;

            crvm.eval = getEval;
            crvm.questions = QuestionHelper.GetQuestions(db, getEval.EvalCode, id, getEval.StaffPerformanceQuestions.ToList());

            ViewData["RatingList"] = QuestionHelper.GetRatings(getEval.EvalCode);

            return View(crvm);
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> EditEval(int id, string EmployeeComments, string EvaluatorComments, string button, List<Question> question)
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

                var body = "<p> " + eval.EvaluatorNetid + " has accepted his/her " + eval.Year + " Performance Evaluation</p>";
                var message = new MailMessage();

                message.To.Add(new MailAddress(eval.EvaluatorNetid + "@illinois.edu"));
                message.From = new MailAddress(eval.EvaluatorNetid + "@illinois.edu");
                message.Subject = eval.Year + " Performance Evaluation Accepted";
                message.Body = body;
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    smtp.Host = "Express-SMTP.cites.illinois.edu ";
                    await smtp.SendMailAsync(message);
                }
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

            message.To.Add(new MailAddress(eval.EvaluatorNetid + "@illinois.edu")); //change this to eval.NetId in production
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