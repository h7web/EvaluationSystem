using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StaffEvaluations.Models;

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

            reportees.Add(new Models.Person { NetId = "jmj", Name = "Jenny Johnson", EmployeeType = "Faculty" });
            reportees.Add(new Models.Person { NetId = "mikesweb", Name = "Mike Nelson", EmployeeType = "AP" });
            reportees.Add(new Models.Person { NetId = "strutz", Name = "Jason Strutz", EmployeeType = "AP" });


            IndexViewModel vm = new IndexViewModel();
            vm.DirectReports = reportees;
            vm.NetId = HttpContext.User.Identity.Name.Substring(5);

            var myEvals = from e in db.StaffPerformanceEvaluations where e.Status == Constants.ReadyForReview && e.NetId == vm.NetId select e;

            var myStaffEvals = from e in db.StaffPerformanceEvaluations where e.EvaluatorNetid == vm.NetId select e;

            vm.MyEvaluations = myEvals.ToList();
            vm.MyStaffEvaluations = myStaffEvals.ToList();


            return View(vm);
        }

        public ActionResult CreateEval(string id, string type)
        {
            StaffPerformanceEvaluationxxx newEval = new StaffPerformanceEvaluationxxx();
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
            StaffPerformanceEvaluationxxx newEval = new StaffPerformanceEvaluationxxx();
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
        public ActionResult EditEval(StaffPerformanceEvaluation eval, List<Question> question)
        {

            foreach (Question q in question)
            {
                var orig = db.StaffPerformanceQuestions.Find(q.QuestionId);
                    orig.Comment = q.QuestionComment;
                    orig.Rating = q.QuestionRating;
                    orig.LastUpdateDate = DateTime.Now;
                db.SaveChanges();
            }



            return RedirectToAction("Index");
        }


    }
}