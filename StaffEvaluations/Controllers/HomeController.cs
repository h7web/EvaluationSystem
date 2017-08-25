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
            List<Person> reportees = new List<Person>() ;

            reportees.Add(new Models.Person { NetId = "thabing", Name = "Tom Habing", EmployeeType = "AP" });
            reportees.Add(new Models.Person { NetId = "jmj", Name = "Jenny Johnson", EmployeeType = "Faculty" });
            reportees.Add(new Models.Person { NetId = "mikesweb", Name = "Mike Nelson", EmployeeType = "AP" });
            reportees.Add(new Models.Person { NetId = "strutz", Name = "Jason Strutz", EmployeeType = "AP" });


            IndexViewModel vm = new IndexViewModel();
            vm.DirectReports = reportees;
            vm.NetId = HttpContext.User.Identity.Name.Substring(5);

            var myEval = (from e in db.StaffPerformanceEvaluations where e.NetId == vm.NetId select e).SingleOrDefault();

            var myStaffEvals = from e in db.StaffPerformanceEvaluations where e.EvaluatorNetid == vm.NetId select e;

            vm.MyEvaluation = myEval;
            vm.MyStaffEvaluations = myStaffEvals.ToList();


            return View(vm);
        }

        public ActionResult CreateEval( string id, string type)
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

            CreateEvalViewModel crvm = new CreateEvalViewModel();

            crvm.eval = newEval;
            crvm.questions = QuestionHelper.GetQuestions(type);

            return View(crvm);
        }

    }
}