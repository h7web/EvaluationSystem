﻿using System;
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

        static string LoggedInUser = System.Web.HttpContext.Current.User.Identity.Name.Substring(5);

        public string GetUser()
        {
            string ret = LoggedInUser;

            if (Session["MasqueradeUser"].ToString() != "")
            {
                ret = Session["MasqueradeUser"].ToString();
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

            return RedirectToAction("Index");
        }

        public ActionResult StopMasquerade()
        {
            Session["Masquerade"] = false;
            Session["MasqueradeUser"] = "";

            return RedirectToAction("Index");
        }

        public ActionResult Index()
        {
            if (Session["Masquerade"] == null)
            {
                Session["Masquerade"] = false;
                Session["MasqueradeUser"] = "";
            }

            string usethisnetid = GetUser();

            Supervisor super = LibDirectoryFactory.GetSupervisor(GetUser());

            IndexViewModel vm = new IndexViewModel();
            vm.Super = super;

            if (User.Identity.Name.Substring(5) != "gknott63")
            {

                //new way to check for multiple null posibilities 
                // if (vm?.Super?.direct_reports != null) 

                if (vm.Super != null)
                {
                    if (vm.Super.direct_reports != null)
                    {
                        foreach (LibDirectoryPerson lp in vm.Super.direct_reports)
                        {
                            var emp = (from e in db1.employees where e.NETID == lp.netid select e).FirstOrDefault();
                            lp.employee_type_code = emp.ECLASS;
                            lp.LibraryStartDate = String.Format("0: MM/dd/yyyy", emp.LIBRARY_START_DATE.ToString());
                        }
                    }
                }
                else
                {
                    vm.Super = new Supervisor();
                    vm.Super.direct_reports = new List<DirectReport>();
                }


                // next 5 lines added for testing purposes
                vm.Super.direct_reports.Add(new DirectReport() { netid = "yoskye", name = "Skye Arseneau", employee_type_code = "CC", LibraryStartDate = "02/01/2001" });
                vm.Super.direct_reports.Add(new DirectReport { netid = "atJohnsn", name = "Anietre Johnson", employee_type_code = "CA", LibraryStartDate = "08/01/2014" });
                vm.Super.direct_reports.Add(new DirectReport { netid = "mikesweb", name = "Mike Nelson", employee_type_code = "BA", LibraryStartDate = "06/24/2016" });
                vm.Super.direct_reports.Add(new DirectReport { netid = "strutz", name = "Jason Strutz", employee_type_code = "BA", LibraryStartDate = "09/01/2002" });
                vm.Super.direct_reports.Add(new DirectReport { netid = "gknott63", name = "Greg Knott", employee_type_code = "BA", LibraryStartDate = "09/01/2010" });
                vm.Super.direct_reports.Add(new DirectReport { netid = "jlockmil", name = "John Lockmiller", employee_type_code = "BA", LibraryStartDate = "06/01/2017" });

                var myStaffEvals = (from e in db.StaffPerformanceEvaluations where e.EvaluatorNetid == vm.Super.netid || e.EvaluatorNetid == usethisnetid select e).ToList();

                vm.MyStaffEvaluations = myStaffEvals;
            }
            else
            {
                vm.Super = new Supervisor();
                vm.Super.direct_reports = new List<DirectReport>();
            }
            var myEvals = from e in db.StaffPerformanceEvaluations where (e.Status == Constants.Submitted || e.Status == Constants.Complete) && e.NetId == usethisnetid select e;

            vm.MyEvaluations = myEvals.ToList();

            return View(vm);
        }

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
                eval.CompleteDate = DateTime.Now;
                eval.Status = "Complete";
            }
            var body = "";
            var message = new MailMessage();

            if (eval.NetId == GetUser() && button == "Contest")
            {
                eval.AcceptedDate = DateTime.Now;
                eval.Status = "Contested";
                var name = LibDirectoryIntegration.LibDirectoryFactory.GetPerson(eval.NetId).name;

                body = "<p> " + name + " has contested his/her " + eval.Year + " Performance Evaluation</p>";

                message.To.Add(new MailAddress(eval.EvaluatorNetid + "@illinois.edu"));
                message.From = new MailAddress(eval.EvaluatorNetid + "@illinois.edu");
                message.Subject = eval.Year + " Performance Evaluation Contested";
                message.Body = body;
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    smtp.Host = "Express-SMTP.cites.illinois.edu ";
                    await smtp.SendMailAsync(message);
                }
            }

            if (eval.NetId == HttpContext.User.Identity.Name.Substring(5) && button == "Accept")
            {
                eval.AcceptedDate = DateTime.Now;
                eval.Status = "Accepted";
                var evalname = LibDirectoryIntegration.LibDirectoryFactory.GetPerson(eval.EvaluatorNetid).name;

                body = "<p> " + evalname + " has accepted his/her " + eval.Year + " Performance Evaluation</p>";

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
        [SessionTimeout]
        public async System.Threading.Tasks.Task<ActionResult> SubmitEval(int id)
        {
            var eval = db.StaffPerformanceEvaluations.Find(id);

            var answers = from a in db.StaffPerformanceQuestions where a.Rating == "* You must select a value *" select a;

            if (answers.Count() > 0)
            {
                ModelState.AddModelError("submit", "You must select a rating for all questions.");
            }
            else
            {
                eval.Status = "Submitted";
                eval.SubmittedDate = DateTime.Now;
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
            db.SaveChanges();
            var evalname = LibDirectoryIntegration.LibDirectoryFactory.GetPerson(newEval.EvaluatorNetid).name;
            var name = LibDirectoryIntegration.LibDirectoryFactory.GetPerson(newEval.EvaluatorNetid).name;

            var body = "<p> " + newEval.Year + " Performance Evaluation for " + name + " has been deferrede by " + evalname + " on " + newEval.DeferredDate + "</p>";
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