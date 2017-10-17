using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StaffEvaluations.Models;
using System.Net.Mail;
using LibDirectoryIntegration;
using System.Configuration;
using Mayur.Web.Attributes;

namespace StaffEvaluations.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private Models.StaffEvaluationsEntities db = new Models.StaffEvaluationsEntities();
        private Models.HR_DataEntities db1 = new Models.HR_DataEntities();

        public string GetUser()
        {
            string LoggedInUser = System.Web.HttpContext.Current.User.Identity.Name.Substring(5);

            string ret = LoggedInUser;

            if (Session["MasqueradeUser"] != null)
            {
                if (Session["MasqueradeUser"].ToString() != "")
                {
                    ret = Session["MasqueradeUser"].ToString();
                }
                else
                {
                    ret = LoggedInUser;
                }
            }
            else
            {
                ret = LoggedInUser;
            }
            return ret;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
                db1.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult MasqueradeAs(string netid)
        {
            Session["Masquerade"] = true;
            Session["MasqueradeUser"] = netid;

            HttpCookie cookie = new HttpCookie("Masquerading");

            cookie["Masquerade"] = "true";
            cookie["MasqueradeUser"] = netid;
            
            cookie.Expires = DateTime.Now.AddDays(365);

            Response.Cookies.Add(cookie);

            return RedirectToAction("Index");
        }

        public ActionResult StopMasquerade()
        {
            Session["Masquerade"] = false;
            Session["MasqueradeUser"] = "";

            Response.Cookies["Masquerading"]["Masquerade"] = "false";

            return RedirectToAction("Index");
        }

        public ActionResult Index()
        {
            if (Session["Masquerade"] == null)
            {
                Session["Masquerade"] = false;
                Session["MasqueradeUser"] = "";
            }

            if (Session["SessionTimeout"] != null)
            {
                if ((bool)Session["SessionTimeout"] == true)
                {
                    TempData["warning"] = "Your Masquerade session has timed out.";
                }
                else
                {
                    TempData["warning"] = "";
                }
            }

            string usethisnetid = GetUser();

            Supervisor super = LibDirectoryFactory.GetSupervisor(GetUser());

            IndexViewModel vm = new IndexViewModel();
            vm.Super = super;

            List<DirectReport> mylist = null;

            if (User.Identity.Name.Substring(5) != "gknott6")
            {

                //new way to check for multiple null posibilities 
                // if (vm?.Super?.direct_reports != null) 

                if (vm.Super != null)
                {
                    if (vm.Super.eval_direct_reports != null)
                    {
                        mylist = vm.Super.eval_direct_reports;

                        foreach (LibDirectoryPerson lp in mylist)
                        {
                            var emp = (from e in db1.employees where e.NETID == lp.netid select e).FirstOrDefault();
                            lp.employee_type_code = emp?.ECLASS;
                            lp.LibraryStartDate = String.Format("{0:MM-dd-yyyy}", emp?.LIBRARY_START_DATE.ToString());
                        }

                    }
                }
                else
                {
                    vm.Super = new Supervisor();
                    mylist = new List<DirectReport>();
                }


                // next 5 lines added for testing purposes
                mylist.Add(new DirectReport() { netid = "yoskye", name = "Skye Arseneau", employee_type_code = "CC", LibraryStartDate = "02/01/2001" });
                mylist.Add(new DirectReport { netid = "atJohnsn", name = "Anietre Johnson", employee_type_code = "CA", LibraryStartDate = "08/01/2014" });
                mylist.Add(new DirectReport { netid = "mikesweb", name = "Mike Nelson", employee_type_code = "BA", LibraryStartDate = "06/24/2016" });
                mylist.Add(new DirectReport { netid = "strutz", name = "Jason Strutz", employee_type_code = "BA", LibraryStartDate = "09/01/2002" });
                mylist.Add(new DirectReport { netid = "gknott63", name = "Greg Knott", employee_type_code = "BA", LibraryStartDate = "09/01/2010" });
                mylist.Add(new DirectReport { netid = "jlockmil", name = "John Lockmiller", employee_type_code = "BA", LibraryStartDate = "06/01/2017" });

                var myStaffEvals = (from e in db.StaffPerformanceEvaluations where e.EvaluatorNetid == usethisnetid select e).ToList();

                vm.MyStaffEvaluations = myStaffEvals;
            }
            else
            {
                vm.Super = new Supervisor();
                mylist = new List<DirectReport>();
            }
            var myEvals = from e in db.StaffPerformanceEvaluations where e.NetId == usethisnetid select e;

            vm.MyEvaluations = myEvals.ToList();
            vm.Super.direct_reports = mylist;

            return View(vm);
        }

        [SessionTimeout]
        public ActionResult CreateEval(string id, string type)
        {
            StaffPerformanceEvaluation newEval = new StaffPerformanceEvaluation();
            newEval.NetId = id;
            newEval.EvalCode = type;

            var reportinfo = LibDirectoryFactory.GetPerson(id);

            var superinfo = LibDirectoryFactory.GetPerson(GetUser());

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

        [SessionTimeout]
        [HttpPost]
        public ActionResult CreateEval(string id, string type, string title, List<Question> question)
        {
            StaffPerformanceEvaluation newEval = new StaffPerformanceEvaluation();
            newEval.NetId = id;
            newEval.EvaluatorNetid = GetUser();
            newEval.Year = DateTime.Now.Year;
            newEval.EvalCode = type;
            newEval.Status = "In-Work";
            newEval.Title = title;
            newEval.StartDate = DateTime.Now;

            if (Session["Masquerade"].Equals(true))
            {
                newEval.TouchedByMasqeradeNetID = System.Web.HttpContext.Current.User.Identity.Name.Substring(5);
                newEval.TouchedByMasqeradeDate = DateTime.Now;
            }

            db.StaffPerformanceEvaluations.Add(newEval);
 //           db.SaveChanges();

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

        [SessionTimeout]
        public ActionResult EditEval(int id)
        {
            var getEval = (from e in db.StaffPerformanceEvaluations where e.EvalId == id select e).Single();

            var reportinfo = LibDirectoryFactory.GetPerson(getEval.NetId);

            var superinfo = LibDirectoryFactory.GetPerson(GetUser());

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

        [SessionTimeout]
        [HttpPost]
        public ActionResult EditEval(int id, string EmployeeComments, string EvaluatorComments, string button, List<Question> question)
        {

            foreach (Question q in question)
            {
                var orig = db.StaffPerformanceQuestions.Find(q.QuestionId);
                if (orig != null)
                {
                    if (orig.Comment != q.QuestionComment || orig.Rating != q.QuestionRating)
                    {
                        orig.Comment = q.QuestionComment;
                        orig.Rating = q.QuestionRating;
                        orig.LastUpdateDate = DateTime.Now;
                        db.SaveChanges();
                    }
                }
            }

            var eval = db.StaffPerformanceEvaluations.Find(id);
            if (eval.EvaluatorComments != EvaluatorComments || eval.EmployeeComments != EmployeeComments)
            {
                eval.EmployeeComments = EmployeeComments;
                eval.EvaluatorComments = EvaluatorComments;

//                db.SaveChanges();
            }

            if (button.Equals("Complete"))
            {
                eval.CompleteDate = DateTime.Now;
                eval.Status = "Complete";
            }

            if (Session["Masquerade"].Equals(true))
            {
                eval.TouchedByMasqeradeNetID = System.Web.HttpContext.Current.User.Identity.Name.Substring(5);
                eval.TouchedByMasqeradeDate = DateTime.Now;
            }

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [SessionTimeout]
        public async System.Threading.Tasks.Task<ActionResult> AcceptEval(int id, string button)
        {
            var eval = db.StaffPerformanceEvaluations.Find(id);
            var body = "";
            var message = new MailMessage();

            if (eval.NetId == GetUser() && button == "Contest")
            {
                eval.ContestedDate = DateTime.Now;
                eval.Status = "Contested";
                var name = LibDirectoryIntegration.LibDirectoryFactory.GetPerson(eval.NetId).name;

                body = "<p> " + name + " has contested his/her " + eval.Year + " Performance Evaluation</p>";

                message.To.Add(new MailAddress(eval.EvaluatorNetid + "@illinois.edu"));
                message.From = new MailAddress(eval.NetId + "@illinois.edu");
                message.Subject = eval.Year + " Performance Evaluation Contested";
                message.Body = body;
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    smtp.Host = "Express-SMTP.cites.illinois.edu ";
                    await smtp.SendMailAsync(message);
                }
            }

            if (eval.NetId == GetUser() && button == "Accept")
            {
                try
                {
                    eval.AcceptedDate = DateTime.Now;
                    eval.Status = "Accepted";
                    var evalname = LibDirectoryIntegration.LibDirectoryFactory.GetPerson(eval.NetId).name;

                    body = "<p> " + evalname + " has accepted his/her " + eval.Year + " Performance Evaluation</p>";

                    message.To.Add(new MailAddress(eval.EvaluatorNetid + "@illinois.edu"));
                    message.From = new MailAddress(eval.NetId + "@illinois.edu");
                    message.Subject = eval.Year + " Performance Evaluation Accepted";
                    message.Body = body;
                    message.IsBodyHtml = true;

                    using (var smtp = new SmtpClient())
                    {
                        smtp.Host = "Express-SMTP.cites.illinois.edu ";
                        await smtp.SendMailAsync(message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error is " + ex.Message);
                }
            }

            if (Session["Masquerade"].Equals(true))
            {
                eval.TouchedByMasqeradeNetID = System.Web.HttpContext.Current.User.Identity.Name.Substring(5);
                eval.TouchedByMasqeradeDate = DateTime.Now;
            }

            db.SaveChanges();

            return RedirectToAction("Index");
        }
 
        [SessionTimeout]
        public async System.Threading.Tasks.Task<ActionResult> SubmitEval(int id)
        {
            var eval = db.StaffPerformanceEvaluations.Find(id);

            var answers = from a in db.StaffPerformanceQuestions where a.Rating == "* You must select a value *" && a.EvalId == id select a;

            if (answers.Count() > 0)
            {
                TempData["error"] = "You must select a rating for all questions in order to Submit an Evaluation.";
            }
            else
            {
                eval.Status = "Submitted";
                eval.SubmittedDate = DateTime.Now;

                if (Session["Masquerade"].Equals(true))
                {
                    eval.TouchedByMasqeradeNetID = System.Web.HttpContext.Current.User.Identity.Name.Substring(5);
                    eval.TouchedByMasqeradeDate = DateTime.Now;
                }

                db.SaveChanges();
                var evalname = LibDirectoryIntegration.LibDirectoryFactory.GetPerson(eval.EvaluatorNetid).name;

                var body = "<p>Your " + eval.Year + " Performance Evaluation prepared by " + evalname + " is available for you to review and comment at the following URL:</p>";
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
            }
            return RedirectToAction("Index");
        }

        [SessionTimeout]
        public async System.Threading.Tasks.Task<ActionResult> DeferEval(string id, string type, string title)
        {
            StaffPerformanceEvaluation newEval = new StaffPerformanceEvaluation();
            newEval.NetId = id;
            newEval.EvaluatorNetid = GetUser();
            newEval.Year = DateTime.Now.Year;
            newEval.EvalCode = type;
            newEval.Status = "In-Work";
            newEval.Title = title;
            newEval.StartDate = DateTime.Now;
            db.StaffPerformanceEvaluations.Add(newEval);
            db.SaveChanges();
            newEval.Status = "Deferred";
            newEval.DeferredDate = DateTime.Now;

            if (Session["Masquerade"].Equals(true))
            {
                newEval.TouchedByMasqeradeNetID = System.Web.HttpContext.Current.User.Identity.Name.Substring(5);
                newEval.TouchedByMasqeradeDate = DateTime.Now;
            }

            db.SaveChanges();
            var evalname = LibDirectoryIntegration.LibDirectoryFactory.GetPerson(newEval.EvaluatorNetid).name;
            var name = LibDirectoryIntegration.LibDirectoryFactory.GetPerson(newEval.EvaluatorNetid).name;

            var body = "<p> " + newEval.Year + " Performance Evaluation for " + name + " has been deferred by " + evalname + " on " + newEval.DeferredDate + "</p>";
            body = body + "Here is the Application URL: http://iisdev1.library.illinois.edu/StaffEvaluations/";
            var message = new MailMessage();

            message.To.Add(new MailAddress(newEval.EvaluatorNetid + "@illinois.edu")); //change this to BHSRC address in production
            message.From = new MailAddress(newEval.EvaluatorNetid + "@illinois.edu");
            message.Subject = newEval.Year + " Performance Evaluation Deferred";
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