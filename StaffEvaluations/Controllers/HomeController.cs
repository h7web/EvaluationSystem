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
using System.Data.Entity.Validation;
using StaffEvaluations.Helpers;
using StaffEvaluations.Controllers;

namespace StaffEvaluations.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private Models.Entities db = new Models.Entities();
        private Models.HR_DataEntities db1 = new Models.HR_DataEntities();

        int currentYear = DateTime.Now.Year;

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

        public ActionResult Index(string go)
        {
            ViewData["go"] = go;

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
            // string unit = "";

            if (User.Identity.Name.Substring(5) != "gknott6")
            {

                //new way to check for multiple null posibilities 
                // if (vm?.Super?.direct_reports != null) 

                if (vm.Super != null)
                {
                    if (vm.Super.eval_direct_reports != null)
                    {
                        mylist = vm.Super.eval_direct_reports.OrderBy(dr => dr.unit_name).ThenBy(dr => dr.last).ToList();

                        foreach (LibDirectoryPerson lp in mylist)
                        {
                            var emp = (from e in db1.employees where e.NETID == lp.netid select e).FirstOrDefault();
                            lp.employee_type_code = emp?.ECLASS;
                            lp.LibraryStartDate = String.Format("{0:MM-dd-yyyy}", emp?.LIBRARY_START_DATE.ToString());
                        }

                        // mylist.Sort((x, y) => string.Compare(x.last, y.last));
                    }
                }
                else
                {
                    vm.Super = new Supervisor();
                    mylist = new List<DirectReport>();
                }


                if ((bool)Session["Masquerade"] != true)
                {
                    // next 5 lines added for testing purposes
                    //mylist.Add(new DirectReport() { netid = "yoskye", name = "Skye Arseneau", employee_type_code = "CC", LibraryStartDate = "02/01/2001" });
                    //mylist.Add(new DirectReport { netid = "atJohnsn", name = "Aneitre Johnson", employee_type_code = "CA", LibraryStartDate = "08/01/2014" });
                    //mylist.Add(new DirectReport { netid = "mikesweb", name = "Mike Nelson", employee_type_code = "BA", LibraryStartDate = "06/24/2016" });
                    //mylist.Add(new DirectReport { netid = "strutz", name = "Jason Strutz", employee_type_code = "BA", LibraryStartDate = "09/01/2002" });
                    //mylist.Add(new DirectReport { netid = "gknott63", name = "Greg Knott", employee_type_code = "BA", LibraryStartDate = "09/01/2010" });
                    //mylist.Add(new DirectReport { netid = "jlockmil", name = "John Lockmiller", employee_type_code = "BA", LibraryStartDate = "06/01/2017" });
                }
                var myStaffEvals = from e in db.StaffPerformanceEvaluations where (e.EvaluatorNetid == usethisnetid) && (e.Year == currentYear) select e;

                if (myStaffEvals != null)
                {
                    vm.MyStaffEvaluations = myStaffEvals.ToList();
                }
            }
            else
            {
                vm.Super = new Supervisor();
                mylist = new List<DirectReport>();
            }
            var myEvals = from e in db.StaffPerformanceEvaluations where e.NetId == usethisnetid select e;

            vm.MyEvaluations = myEvals.ToList();
            vm.Super.direct_reports = mylist;

            if (mylist.Count >= 1) {
                var myPreviousStaffEvals = from e in db.StaffPerformanceEvaluations where (e.EvaluatorNetid == usethisnetid) && (e.Year != currentYear) select e;

                if (myPreviousStaffEvals != null)
                {
                    vm.MyPreviousStaffEvaluations = myPreviousStaffEvals.ToList();
                }
            }
            return View(vm);
        }

        [SessionTimeout]
        public ActionResult CreateEval(string id, string type)
        {
            if (type == null)
            {
                type = "BA";
            }

            StaffPerformanceEvaluation newEval = new StaffPerformanceEvaluation();
            newEval.NetId = id;
            newEval.EvalCode = type;
            newEval.Year = DateTime.Now.Year;

            var reportinfo = LibDirectoryFactory.GetPerson(id);

            var superinfo = LibDirectoryFactory.GetPerson(GetUser());

            var getsup = LibDirectoryFactory.GetPersonsSupervisors(newEval.NetId);

            var supervisorNetid = superinfo.netid;

            var suplist = "";

            if (getsup != null)
            {
                foreach (LibDirectoryPerson s in getsup)
                {
                    suplist = suplist + s.netid;
                }
            }

            if (suplist.ToString().Contains(GetUser()) || newEval.NetId == GetUser() || newEval.NetId == "yoskye" || newEval.NetId == "atJohnsn" || newEval.NetId == "mikesweb" || newEval.NetId == "strutz" || newEval.NetId == "gknott63" || newEval.NetId == "jlockmil")
            {
                var lsdate = (from e in db1.employees where e.NETID == id select e.LIBRARY_START_DATE).FirstOrDefault().ToString();

                CreateEditEvalViewModel crvm = new CreateEditEvalViewModel();

                crvm.person = new LibDirectoryPerson();
                crvm.person = reportinfo;
                crvm.person.LibraryStartDate = lsdate;

                crvm.super = new LibDirectoryPerson();
                crvm.super = superinfo;

                crvm.eval = newEval;
                crvm.questions = QuestionHelper.GetQuestions(db, type, reportinfo.netid, supervisorNetid, currentYear);

                if (crvm.questions.Count < 1) {
                    int lastYear = currentYear - 1;
                    var maxyrs = from y in db.EvaluationQuestionSets where y.QuestionType == type select y;
                    lastYear = maxyrs.Max(y => y.Year);

                    crvm.questions = QuestionHelper.GetQuestions(db, type, reportinfo.netid, supervisorNetid, lastYear);
                }

                ViewData["RatingList"] = QuestionHelper.GetRatings(db, type, currentYear);

                return View(crvm);
            }
            else
            {
                TempData["error"] = "You do not have authorization to view this record.";
                return RedirectToAction("Index", new { go = id });
            }
        }

        [SessionTimeout]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateEval(string id, string type, string title, string name, string EvaluatorNetid, string EvaluatorName, string EvaluatorTitle, DateTime libraryStartDate, List<Question> question)
        {
            var evalexists = from e in db.StaffPerformanceEvaluations where e.NetId == id && e.EvaluatorNetid == EvaluatorNetid && e.Year == currentYear select e;

            if (evalexists == null  || evalexists.Count() == 0)
            {
                StaffPerformanceEvaluation newEval = new StaffPerformanceEvaluation();
                newEval.NetId = id;
                newEval.Name = name;
                newEval.LibraryStartDate = libraryStartDate;
                newEval.EvaluatorNetid = EvaluatorNetid;
                newEval.EvaluatorName = EvaluatorName;
                newEval.EvaluatorTitle = EvaluatorTitle;
                newEval.Year = DateTime.Now.Year;
                newEval.EvalCode = type;
                newEval.Status = "In-Work";
                newEval.Title = title;
                newEval.StartDate = DateTime.Now;
                newEval.StartNetid = GetUser();

                var msgflag = false;

                if (Session["Masquerade"] != null)
                {
                    if (Session["Masquerade"].Equals(true))
                    {
                        newEval.StartProxy = System.Web.HttpContext.Current.User.Identity.Name.Substring(5);
                    }
                }

                try
                {
                    db.StaffPerformanceEvaluations.Add(newEval);

                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("eval err is " + ex.Message);
                    Logger.Log.Error("eval err is " + ex.InnerException);
                }

                string CommentList = "";
                var CommentReq = from r in db.Ratings where r.EvalCode == type && r.CommentRequired == true select r;
                foreach (Rating r in CommentReq)
                {
                    CommentList += r.Rating1;
                }

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
                    if (myQuestion.QuestionCode != "AP14" && myQuestion.QuestionCode != "CA11" && myQuestion.QuestionCode != "CC9")
                    {
                        if (CommentList.Contains(myQuestion.QuestionRating) && myQuestion.QuestionComment == null)
                        {
                            msgflag = true;
                        }
                    }
                    db.StaffPerformanceQuestions.Add(newQuestion);
                }

                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("save err is " + ex.Message);
                }

                //if (msgflag == true)
                //{
                //    TempData["error"] = "Comments must be provided for all ratings other than SOLID PERFORMER (AP), SATISFACTORY (CS), or NOT APPLICABLE (CS) before submission.";
                //}

            }
            return RedirectToAction("Index", new { go = id });
        }

        [SessionTimeout]
        public ActionResult EditEval(int id, bool? hr)
        {
            var getEval = (from e in db.StaffPerformanceEvaluations where e.EvalId == id select e).Single();
            var getsup = LibDirectoryFactory.GetPersonsSupervisors(getEval.NetId);

            if (getEval.Status == "Deferred")
            {
                var netid = getEval.NetId;
                var type = getEval.EvalCode;

                db.StaffPerformanceEvaluations.Remove(getEval);
                db.SaveChanges();

                return RedirectToAction("CreateEval", new { @id=netid, @type=type });
            }

            var suplist = "";

            if (getsup != null)
            {
                foreach (LibDirectoryPerson s in getsup)
                {
                    suplist = suplist + s.netid;
                }
            }

            if ((suplist.ToString().Contains(GetUser()) || getEval.NetId == GetUser()) || hr == true)
            {
                var reportinfo = LibDirectoryFactory.GetPerson(getEval.NetId);

                var superinfo = LibDirectoryFactory.GetPerson(GetUser());

                CreateEditEvalViewModel crvm = new CreateEditEvalViewModel();

                crvm.person = new LibDirectoryPerson();
                crvm.person = reportinfo;
                crvm.person.LibraryStartDate = getEval.LibraryStartDate.ToString();

                crvm.super = new LibDirectoryPerson();
                crvm.super = superinfo;

                crvm.eval = getEval;
                crvm.questions = QuestionHelper.GetQuestions(db, getEval.EvalCode, id, getEval.StaffPerformanceQuestions.ToList());

                ViewData["RatingList"] = QuestionHelper.GetRatings(db, getEval.EvalCode, getEval.Year);

                if (hr == true)
                {
                    crvm.hr = true;
                }

                return View(crvm);
            }
            else
            {
                TempData["error"] = "You do not have authorization to view this record.";
                return RedirectToAction("Index", new { go = getEval.NetId });
            }
        }

        [SessionTimeout]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditEval(int id, string EmployeeComments, string EvaluatorComments, string button, List<Question> question)
        {

            var eval = db.StaffPerformanceEvaluations.Find(id);
            var msgflag = false;
            var jdflag = false;
            var subflag = false;

            var answers = from a in db.StaffPerformanceQuestions where a.Rating == "* You must select a value *" && a.EvalId == id select a;

            if (eval.Status == "In-Work" || eval.Status == "Deferred")
            {
                string CommentList = "";
                var CommentReq = from r in db.Ratings where r.EvalCode == eval.EvalCode && r.CommentRequired == true select r;
                foreach (Rating r in CommentReq)
                {
                    CommentList += r.Rating1;
                }

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
                        if (orig.QuestionCode != "AP14" && orig.QuestionCode != "CA11" && orig.QuestionCode != "CC9" && q.QuestionRating != null)
                        {
                            if (CommentList.Contains(q.QuestionRating) && q.QuestionComment == null)
                            {
                                msgflag = true;
                            }
                        }
                        else if (orig.QuestionCode == "AP14" && orig.QuestionCode == "CA11" && orig.QuestionCode == "CC9")
                        {
                            if (q.QuestionComment == null)
                            {
                                jdflag = true;
                            }
                        }
                    }
                }
            }

            if ((eval.EvaluatorComments != EvaluatorComments && EvaluatorComments != "") || (eval.EmployeeComments != EmployeeComments && EmployeeComments != ""))
            {
                eval.EmployeeComments = EmployeeComments;
                eval.EvaluatorComments = EvaluatorComments;

            }

            if (button.Equals("Complete"))
            {
                eval.CompleteDate = DateTime.Now;
                eval.CompleteNetid = GetUser();
                eval.Status = "Complete";

                if (Session["Masquerade"].Equals(true))
                {
                    eval.CompleteProxy = System.Web.HttpContext.Current.User.Identity.Name.Substring(5);
                }

            }


            //the variable answers is a list of questions with the value "You must select a value"
            //if (answers.Count() > 0)
            //{
            //    TempData["error"] = "You must answer all questions in order to Submit an Evaluation.";
            //}
            //else if (msgflag == true)
            //{
            //    TempData["error"] = "Comments must be provided for all ratings other than SOLID PERFORMER (AP), SATISFACTORY (CS), or NOT APPLICABLE (CS) before submission.";
            //    TempData["submittedevalid"] = id;
            //    TempData["editmode"] = "check";
            //}
            //else if (jdflag == true)
            //{
            //    TempData["error"] = "A Job Description is required before submission.";
            //}
            //else if (eval.NetId != GetUser())
            //{
            //    subflag = true;
            //}

            db.SaveChanges();

            //if (button.Equals("Submit"))
            //    {
            //    return RedirectToAction("SubmitEval", new { id = eval.EvalId });
            //}
            //else if (subflag == true)
            //{
            //    return RedirectToAction("EditEval", new { id = eval.EvalId, sub = true });
            //}
            //else {
            return RedirectToAction("Index", new { go = eval.NetId });
            //}
        }

        public ActionResult DeleteEval(int id, string Order = null)
        {
            var Eval = db.StaffPerformanceEvaluations.Find(id);

            db.StaffPerformanceEvaluations.Remove(Eval);
            db.SaveChanges();

            return RedirectToAction("Index", new { sortOrder = Order });
        }

        //this is to perform validation on the eval
        public bool CheckEval(int id)
        {

            var eval = db.StaffPerformanceEvaluations.Find(id);
            var msgflag = false;
            var question = from q in db.StaffPerformanceQuestions where q.EvalId == id select q;

            string CommentList = "";
            var CommentReq = from r in db.Ratings where r.EvalCode == eval.EvalCode && r.CommentRequired == true select r;
            foreach (Rating r in CommentReq)
            {
                CommentList += r.Rating1;
            }

            foreach (StaffPerformanceQuestion q in question)
            {
                var orig = db.StaffPerformanceQuestions.Find(q.QuestionId);
                if (orig != null)
                {
                    if (q.QuestionCode != "AP14" && q.QuestionCode != "CA11" && q.QuestionCode != "CC9" && q.Rating != null)
                    {
                        if (CommentList.Contains(q.Rating) && q.Comment == null)
                        {
                            msgflag = true;
                        }
                    }
                }
            }
            return msgflag;
        }

        //this is for the employee to ensure they have looked at the eval before the accept/contest it
        public ActionResult DoubleCheckEval(int id, string button)
        {
            var eval = db.StaffPerformanceEvaluations.Find(id);

            TempData["evaltoreview"] = new CreatePDFController().PrepareEval(eval.EvalId, false);
            TempData["showeval"] = true;
            TempData["submittedevalid"] = eval.EvalId;
            TempData["evalbutton"] = button;

            return RedirectToAction("Index");
        }

        //accept and contest use this same method
        [SessionTimeout]
        public async System.Threading.Tasks.Task<ActionResult> AcceptEval(int id, string button)
        {
            var eval = db.StaffPerformanceEvaluations.Find(id);
            var body = "";
            var message = new MailMessage();

            if (eval.NetId == GetUser() && button == "Contest")
            {
                eval.ContestedDate = DateTime.Now;
                eval.ContestedNetid = eval.NetId;
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
                    eval.AcceptedNetid = eval.NetId;
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

            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
            return RedirectToAction("Index");
        }

        [SessionTimeout]
        public async System.Threading.Tasks.Task<ActionResult> SubmitEval(int id)
        {
            var eval = db.StaffPerformanceEvaluations.Find(id);
            var msgflag = false;
            var jdflag = false;
            var naflag = false;
            var validate = true;

            string CommentList = "";
            var CommentReq = from r in db.Ratings where r.EvalCode == eval.EvalCode && r.CommentRequired == true select r;
            foreach (Rating r in CommentReq)
            {
                CommentList += r.Rating1;
            }

            var answers = from a in db.StaffPerformanceQuestions where a.Rating == "* You must select a value *" && a.EvalId == id select a;
            var answers2 = from a in db.StaffPerformanceQuestions where a.EvalId == id select a;

            string nalist = "AP1AP2AP3AP4AP5AP6AP7AP8";

            foreach (StaffPerformanceQuestion q in answers2)
            {
                if (q.QuestionCode != "AP14" && q.QuestionCode != "CA11" && q.QuestionCode != "CC9" && q.Rating != null)
                {
                    if (CommentList.Contains(q.Rating) && q.Comment == null)
                    {
                        msgflag = true;
                        validate = false;
                    }
                }
                if (q.QuestionCode == "AP14" && q.QuestionCode == "CA11" && q.QuestionCode == "CC9")
                {
                    if (q.Comment == null)
                    {
                        jdflag = true;
                        validate = false;
                    }
                }
                if (nalist.Contains(q.QuestionCode) && q.Rating == "Not Applicable")
                {
                    naflag = true;
                    validate = false;
                }
            }

            TempData["submittedevalid"] = id;
            TempData["editmode"] = "check";
            TempData["error"] = "";

            //the variable answers is a list of questions with the value "You must select a value"
            if (answers.Count() > 0)
            {
                TempData["error"] += "* You must answer all questions in order to Submit an Evaluation.<br />";
                validate = false;
            }
            if (msgflag == true)
            {
                TempData["error"] += "* Comments must be provided for all ratings other than SOLID PERFORMER (AP), SATISFACTORY (CS), or NOT APPLICABLE (CS) before submission.<br />";
            }
            if (jdflag == true)
            {
                TempData["error"] += "* A Job Description is required before submission.<br />";
            }
            if (naflag == true)
            {
                TempData["error"] += "* You may only select 'Not Applicable' as a rating for optional questions.";
            }

            if (validate == true)
            {
                eval.Status = "Submitted";
                eval.SubmittedDate = DateTime.Now;
                eval.SubmittedNetid = GetUser();

                if (Session["Masquerade"].Equals(true))
                {
                    eval.SubmittedProxy = System.Web.HttpContext.Current.User.Identity.Name.Substring(5);
                }

                db.SaveChanges();
                var evalname = LibDirectoryIntegration.LibDirectoryFactory.GetPerson(eval.EvaluatorNetid).name;

                var body = "<p>Your " + eval.Year + " Performance Evaluation prepared by " + evalname + " is <b>available</b> for you to review and comment at the following URL:</p>";
                body = body + "http://quest.library.illinois.edu/StaffEvaluations/";
                var message = new MailMessage();

                message.To.Add(new MailAddress(eval.NetId + "@illinois.edu"));
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
            return RedirectToAction("Index", new { go = eval.NetId });
        }

        [SessionTimeout]
        public async System.Threading.Tasks.Task<ActionResult> DeferEval(string id, string type)
        {

            StaffPerformanceEvaluation newEval = new StaffPerformanceEvaluation();

            var evalnetid = GetUser();

            var evalexists = from e in db.StaffPerformanceEvaluations where e.NetId == id && e.EvaluatorNetid == evalnetid select e;

            if (evalexists == null || evalexists.Count() == 0)
            {
                var reportinfo = LibDirectoryFactory.GetPerson(id);

                if (type == null)
                {
                    type = "BA";
                }

                newEval.NetId = id;
                newEval.EvaluatorNetid = GetUser();
                newEval.Year = DateTime.Now.Year;
                newEval.EvalCode = type;
                newEval.Status = "In-Work";
                newEval.Title = reportinfo.banner_title;
                newEval.StartDate = DateTime.Now;
                db.StaffPerformanceEvaluations.Add(newEval);


                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("err is " + ex.Message);
                }
                newEval.Status = "Deferred";
                newEval.DeferredDate = DateTime.Now;
                newEval.DeferredNetid = GetUser();

                if (Session["Masquerade"].Equals(true))
                {
                    newEval.DeferredProxy = System.Web.HttpContext.Current.User.Identity.Name.Substring(5);
                }

                db.SaveChanges();
                var evalname = LibDirectoryIntegration.LibDirectoryFactory.GetPerson(newEval.EvaluatorNetid).name;
                var name = LibDirectoryIntegration.LibDirectoryFactory.GetPerson(newEval.NetId).name;

                var body = "<p> " + newEval.Year + " Performance Evaluation for " + name + " has been deferred by " + evalname + " on " + newEval.DeferredDate + "</p>";
                body = body + "Here is the Application URL: http://quest.library.illinois.edu/StaffEvaluations/";
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
            }
            return RedirectToAction("Index", new { go = newEval.NetId });
        }

        public async System.Threading.Tasks.Task<ActionResult> ReturnEvaltoSupervisor(int id)
        {
            var eval = db.StaffPerformanceEvaluations.Find(id);

            eval.Status = "In-Work";
            eval.ReturntoSupervisorDate = DateTime.Now;
            eval.ReturntoSupervisorNetid = GetUser();
            if (Session["Masquerade"].Equals(true))
            {
                eval.ReturntoSupervisorProxy = System.Web.HttpContext.Current.User.Identity.Name.Substring(5);

                eval.CompleteDate = null;
                eval.CompleteNetid = null;
                eval.CompleteProxy = null;
            }

            eval.SubmittedDate = null;
            eval.SubmittedNetid = null;
            eval.AcceptedDate = null;
            eval.AcceptedNetid = null;
            eval.AcceptedProxy = null;
            eval.ContestedDate = null;
            eval.ContestedNetid = null;
            eval.ContestedProxy = null;

            db.SaveChanges();

            var evalname = LibDirectoryIntegration.LibDirectoryFactory.GetPerson(eval.NetId).name;

            var body = "<p>The " + eval.Year + " Performance Evaluation you prepared for " + evalname + " has been returned for you to review and comment at the following URL:</p>";
            body = body + "http://quest.library.illinois.edu/StaffEvaluations/";
            var message = new MailMessage();

            message.To.Add(new MailAddress(eval.EvaluatorNetid + "@illinois.edu"));
            message.From = new MailAddress(eval.NetId + "@illinois.edu");
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

        [SessionTimeout]
        public async System.Threading.Tasks.Task<ActionResult> ReturnEvaltoEmployee(int id)
        {

            var eval = db.StaffPerformanceEvaluations.Find(id);

            eval.Status = "Submitted";
            eval.ReturntoEmployeeDate = DateTime.Now;
            eval.ReturntoEmployeeNetid = GetUser();
            if (Session["Masquerade"].Equals(true))
            {
                eval.ReturntoEmployeeProxy = System.Web.HttpContext.Current.User.Identity.Name.Substring(5);

                eval.CompleteDate = null;
                eval.CompleteNetid = null;
                eval.CompleteProxy = null;
            }

            eval.AcceptedDate = null;
            eval.AcceptedNetid = null;
            eval.AcceptedProxy = null;
            eval.ContestedDate = null;
            eval.ContestedNetid = null;
            eval.ContestedProxy = null;

            db.SaveChanges();
            var evalname = LibDirectoryIntegration.LibDirectoryFactory.GetPerson(eval.EvaluatorNetid).name;

            var body = "<p>Your " + eval.Year + " Performance Evaluation prepared by " + evalname + " has been <b>returned</b> for you to review and comment at the following URL:</p>";
            body = body + "http://quest.library.illinois.edu/StaffEvaluations/";
            var message = new MailMessage();

            message.To.Add(new MailAddress(eval.NetId + "@illinois.edu"));
            message.From = new MailAddress(eval.EvaluatorNetid + "@illinois.edu");
            message.Subject = eval.Year + " Performance Evaluation";
            message.Body = body;
            message.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                smtp.Host = "Express-SMTP.cites.illinois.edu ";
                await smtp.SendMailAsync(message);
            }

            return RedirectToAction("Index", new { go = eval.NetId });
        }

        public ActionResult CompleteEval(int id)
        {
            var eval = db.StaffPerformanceEvaluations.Find(id);

            eval.Status = "Complete";
            eval.CompleteDate = DateTime.Now;
            eval.CompleteNetid = GetUser();
            if (Session["Masquerade"].Equals(true))
            {
                eval.CompleteProxy = System.Web.HttpContext.Current.User.Identity.Name.Substring(5);
            }

            db.SaveChanges();

            return RedirectToAction("Index", new { go = eval.NetId });
        }

        public ActionResult SearchEvals(string query, int fy)
        {
            var results = from n in db.StaffPerformanceEvaluations where (n.NetId.Contains(query) || n.Name.Contains(query)) && n.Year.Equals(fy) select n;

            return PartialView(results);
        }

        //This is the list of JDs
        public ActionResult EditJDs(string sortOrder, string gojd)
        {
            ViewData["gojd"] = gojd;

            var suplist = LibDirectoryFactory.GetAllSupervisors();

            List<JDList> jds1 = new List<JDList>();

            foreach (var item in suplist)
            {
                foreach (var item2 in item.supervisor.eval_direct_reports)
                {
                    var newJD = new JDList();
                    if (item2.unit_name != "Law Library") {
                        newJD.supNetId = item.supervisor.netid;
                        newJD.JDSuper = item.supervisor.name;
                        newJD.SuperLast = item.supervisor.last;
                        newJD.empNetId = item2.netid;
                        newJD.JDname = item2.name;
                        newJD.EmployeeFirst = item2.first;
                        newJD.EmployeeLast = item2.last;
                        newJD.EmployeeUnit = item2.unit_name;
                        newJD.Order = sortOrder;
                        jds1.Add(newJD);
                    }
                }
            }

            foreach (var item in jds1)
            {
                item.Order = sortOrder;

                var jds = (from j in db.JobDescriptions where j.netid == item.empNetId && j.supervisorNetid == item.supNetId select j).FirstOrDefault();

                if (jds != null)
                {
                    if (jds.description != null)
                    {
                        item.description = jds.description;
                    }
                    if (jds.posn_number != null)
                    {
                        item.posn_number = jds.posn_number;
                    }
                    item.lastUpdatedDate = jds.lastUpdatedDate;
                    item.jdid = jds.jdid;
                }
            }

            var jds2 = from j in db.JobDescriptions select j;

            foreach (var item in jds2)
            {
                var jdo = jds1.Where(j => j.empNetId == item.netid && j.supNetId == item.supervisorNetid).SingleOrDefault();
                if (jdo == null)
                {
                    var newJD = new JDList();
                    newJD.jdid = item.jdid;
                    newJD.supNetId = item.supervisorNetid;
                    newJD.JDSuper = item.JDSuper;
                    var gs = LibDirectoryFactory.GetPerson(item.netid);
                    if (gs != null)
                    {
                        newJD.SuperLast = gs.last;
                    }
                    newJD.empNetId = item.netid;
                    newJD.JDname = item.JDName;
                    var gp = LibDirectoryFactory.GetPerson(item.netid);
                    if (gp != null)
                    {
                        newJD.EmployeeFirst = gp.first;
                        newJD.EmployeeLast = gp.last;
                    }
                    newJD.Fix = "true";
                    newJD.Order = sortOrder;
                    jds1.Add(newJD);
                }
            }


            IOrderedEnumerable<JDList> sorted;

            switch (sortOrder)
            {
                case "employee":
                    sorted = jds1.OrderBy(j => j.EmployeeLast);
                    break;
                case "employeedesc":
                    sorted = jds1.OrderByDescending(j => j.EmployeeLast);
                    break;
                case "super":
                    sorted = jds1.OrderBy(j => j.SuperLast).ThenBy(j => j.EmployeeLast);
                    break;
                case "superdesc":
                    sorted = jds1.OrderByDescending(j => j.SuperLast).ThenBy(j => j.EmployeeLast);
                    break;
                case "date":
                    sorted = jds1.OrderByDescending(j => j.lastUpdatedDate);
                    break;
                case "datedesc":
                    sorted = jds1.OrderBy(j => j.lastUpdatedDate);
                    break;
                default:
                    sorted = jds1.OrderBy(j => j.EmployeeLast);
                    break;
            }

            if (sortOrder == null)
            {
                ViewData["jdsort"] = "employee";
            }
            else
            {
                ViewData["jdsort"] = sortOrder;
            }

            return View(sorted.ToList());
        }

        public ActionResult CreateJD(string netid, string supervisorNetid, string JDname, string JDSuper, string Order = null)
        {
            JobDescription newJD = new JobDescription();
            var lsdate = DateTime.Now;

            newJD.lastUpdatedDate = lsdate;

            newJD.netid = netid;
            newJD.supervisorNetid = supervisorNetid;
            newJD.JDName = JDname;
            newJD.JDSuper = JDSuper;
            newJD.Order = Order;

            if (StaffEvaluations.Models.SuperUserHelper.IsAdSuperUser(User.Identity.Name.Substring(5)))
            {
                return View(newJD);
            }
            else
            {
                TempData["error"] = "You do not have authorization work with Job Descriptions.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateJD(string netid, string supervisorNetid, string description, string posn_number, string submit = null, string Order = null)
        {

            var exists = from d in db.JobDescriptions where d.netid == netid && d.supervisorNetid == supervisorNetid select d;

            if (exists.Count() == 0)
            {
                JobDescription newJD = new JobDescription();
                var lsdate = DateTime.Now;

                newJD.lastUpdatedDate = lsdate;

                newJD.netid = netid;
                newJD.supervisorNetid = supervisorNetid;
                newJD.description = description;
                newJD.posn_number = posn_number;


                db.JobDescriptions.Add(newJD);

                if (submit == "Formatting")
                {
                    newJD.description = FormatHelper.JDFormat(description);

                    db.SaveChanges();

                    return RedirectToAction("EditJD", new { @id = newJD.jdid });
                }
                else
                {
                    db.JobDescriptions.Add(newJD);

                    db.SaveChanges();

                    return RedirectToAction("EditJDs", new { sortOrder = Order, gojd = newJD.netid });
                }
            }
            else
            {
                return RedirectToAction("EditJDs", new { sortOrder = Order });
            }
        }

        public ActionResult EditJD(int id, string Order = null)
        {
            if (StaffEvaluations.Models.SuperUserHelper.IsAdSuperUser(User.Identity.Name.Substring(5)))
            {
                var getJD = (from e in db.JobDescriptions where e.jdid == id select e).SingleOrDefault();
                var name = (from e in db1.employees where e.NETID == getJD.netid select e.FULLNAME).FirstOrDefault();
                var sup = (from e in db1.employees where e.NETID == getJD.supervisorNetid select e.FULLNAME).FirstOrDefault();
                if (name != null)
                {
                    getJD.JDName = (from e in db1.employees where e.NETID == getJD.netid select e.FULLNAME).FirstOrDefault().ToString();
                }
                if (sup != null)
                {
                    getJD.JDSuper = (from e in db1.employees where e.NETID == getJD.supervisorNetid select e.FULLNAME).FirstOrDefault().ToString();
                }
                getJD.Order = Order;

                return View(getJD);
            }
            else
            {
                TempData["error"] = "You do not have authorization to work with Job Descriptions.";
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditJD(int id, string netid, string supervisorNetid, string description, string posn_number, string submit, string Order = null)
        {
            var JD = db.JobDescriptions.Find(id);

            var lsdate = DateTime.Now;

            JD.lastUpdatedDate = lsdate;
            JD.netid = netid;
            JD.supervisorNetid = supervisorNetid;
            JD.description = description;
            JD.posn_number = posn_number;

            if (submit == "Formatting")
            {
                JD.description = FormatHelper.JDFormat(description);

                db.SaveChanges();

                return RedirectToAction("EditJD", new { Order = Order, id = id });
            }
            else
            {

                db.SaveChanges();

                return RedirectToAction("EditJDs", new { sortOrder = Order, gojd = JD.jdid });
            }
        }

        public ActionResult DeleteJD(int id, string Order = null)
        {
            var JD = db.JobDescriptions.Find(id);

            db.JobDescriptions.Remove(JD);
            db.SaveChanges();

            return RedirectToAction("EditJDs", new { sortOrder = Order });
        }

        public ActionResult EditEmails(string sortOrder)
        {
            List<EvalEmail> emllist = (from e in db.EvalEmails select e).ToList();

            List<JDList> jds1 = new List<JDList>();

            IOrderedEnumerable<EvalEmail> sorted;

            switch (sortOrder)
            {
                case "subject":
                    sorted = emllist.OrderBy(j => j.email_subject);
                    break;
                case "senddate":
                    sorted = emllist.OrderBy(j => j.send_date);
                    break;
                default:
                    sorted = emllist.OrderBy(j => j.id);
                    break;
            }

            if (sortOrder == null)
            {
                ViewData["emlsort"] = "id";
            }
            else
            {
                ViewData["emlsort"] = sortOrder;
            }

            return View(sorted.ToList());
        }

        public ActionResult EditEmail(int id, string Order = null)
        {
            if (StaffEvaluations.Models.SuperUserHelper.IsAdSuperUser(User.Identity.Name.Substring(5)))
            {
                var getE = (from e in db.EvalEmails where e.id == id select e).SingleOrDefault();
                ViewData["EmlList"] = EmailHelper.GetEmlList(db, getE.list);

                return View(getE);
            }
            else
            {
                TempData["error"] = "You do not have authorization to work with Scheduled Emails.";
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditEmail(EvalEmail eml)
        {
            var geteml = db.EvalEmails.Find(eml.id);

            var lsdate = DateTime.Now;

            geteml.list = eml.list;
            geteml.send_date = eml.send_date;
            geteml.email_subject = eml.email_subject;
            geteml.email_body = eml.email_body;

            db.SaveChanges();

            return RedirectToAction("EditEmails", new { sortOrder = eml.Order });
        }

        public ActionResult DeleteEmail(int id, string Order = null)
        {
            var E = db.EvalEmails.Find(id);

            db.EvalEmails.Remove(E);
            db.SaveChanges();

            return RedirectToAction("EditEmails", new { sortOrder = Order });
        }

        public ActionResult CreateEmail()
        {
            if (StaffEvaluations.Models.SuperUserHelper.IsAdSuperUser(User.Identity.Name.Substring(5)))
            {
                ViewData["EmlList"] = EmailHelper.GetEmlList(db, null);

                return View();
            }
            else
            {
                TempData["error"] = "You do not have authorization to work with Scheduled Emails.";
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateEmail(EvalEmail eml, string Order = null)
        {
            eml.send_date = (DateTime)eml.send_date;
            db.EvalEmails.Add(eml);

            db.SaveChanges();

            return RedirectToAction("EditEmails", new { sortOrder = Order });
        }

        public ActionResult ListFlaggedevals(string sortOrder)
        {
            List<string> list = new List<string>() { "Needs Improvement", "Not Acceptable", "Unsatisfactory" };
            var qs = from q in db.StaffPerformanceQuestions where list.Contains(q.Rating) select q.EvalId;
            var evals = from e in db.StaffPerformanceEvaluations where qs.Contains(e.EvalId) && (e.Status != "Processed") select e;

            IQueryable<StaffPerformanceEvaluation> sorted;

            switch (sortOrder)
            {
                case "eclass":
                    sorted = evals.OrderBy(j => j.EvalCode);
                    break;
                case "year":
                    sorted = evals.OrderBy(j => j.Year).ThenBy(j => j.Name);
                    break;
                case "name":
                    sorted = evals.OrderByDescending(j => j.Name);
                    break;
                case "evaluatorname":
                    sorted = evals.OrderByDescending(j => j.EvaluatorName).ThenBy(j => j.Name);
                    break;
                case "status":
                    sorted = evals.OrderByDescending(j => j.Status).ThenBy(j => j.Name);
                    break;
                case "released":
                    sorted = evals.OrderByDescending(j => j.released).ThenBy(j => j.Name);
                    break;
                default:
                    sorted = evals.OrderBy(j => j.released).ThenBy(j => j.Name);
                    break;
            }

            if (sortOrder == null)
            {
                ViewData["fesort"] = "released";
            }
            else
            {
                ViewData["fesort"] = sortOrder;
            }

            return View(sorted.ToList());
        }

        public ActionResult ReleaseEval(int id, string sortOrder)
        {
            var eval = (from e in db.StaffPerformanceEvaluations where e.EvalId == id select e).FirstOrDefault();

            if (eval.released == false || eval.released == null)
            {
                eval.released = true;
            }
            else
            {
                eval.released = false;
            }

            db.SaveChanges();

            return RedirectToAction("ListFlaggedevals", new { @sortOrder = sortOrder });
        }

        public ActionResult Logoff()
        {
            return View();
        }
    }
}