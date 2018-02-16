using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SelectPdf;
using LibDirectoryIntegration;
using StaffEvaluations.Models;

namespace StaffEvaluations.Controllers
{
    public class CreatePDFController : Controller
    {
        private Models.Entities db = new Models.Entities();
        private Models.HR_DataEntities db1 = new Models.HR_DataEntities();
        public ActionResult Print2Pdf(int id, bool e = false)
        {
            string htmlString = PrepareEval(id, e);

            HtmlToPdf converter = new HtmlToPdf();

            converter.Options.MarginTop = 20;
            converter.Options.MarginBottom = 20;
            converter.Options.MarginLeft = 20;
            converter.Options.MarginRight = 20;

            PdfDocument doc = converter.ConvertHtmlString(htmlString);

            var eval = (from ev in db.StaffPerformanceEvaluations where ev.EvalId == id select ev).SingleOrDefault();
            var reportinfo = LibDirectoryFactory.GetPerson(eval.NetId);

            string filename = "PerformEval_" + reportinfo.name.Replace(" ", "_") + "_" + eval.Year + ".pdf";

            byte[] pdf = doc.Save();
            FileResult fileResult = new FileContentResult(pdf, "application/pdf");
            fileResult.FileDownloadName = filename;

            return fileResult;
        }

        public string PrepareEval(int id, bool ep)
        {
            string preparedpdf = "";
            string jd = "";
            string evaltypedesc = "";

            var eval = (from e in db.StaffPerformanceEvaluations where e.EvalId == id select e).SingleOrDefault();
            var reportinfo = LibDirectoryFactory.GetPerson(eval.NetId);
            var supinfo = LibDirectoryFactory.GetPerson(eval.EvaluatorNetid);

            var qa = QuestionHelper.GetQuestions(db, eval.EvalCode, eval.EvalId, eval.StaffPerformanceQuestions.ToList() );

            var lsdate = (from e in db1.employees where e.NETID == eval.NetId select e.LIBRARY_START_DATE).FirstOrDefault();

            reportinfo.LibraryStartDate = lsdate?.ToString("MM/dd/yyyy");

            if (eval.EvalCode == "BA")
            {
                evaltypedesc = "Academic Professional";
            }
            else if (eval.EvalCode == "CA")
            {
                evaltypedesc = "Civil Service";
            }
            else if (eval.EvalCode == "CC")
            {
                evaltypedesc = "Civil Service - Exempt";
            }

            preparedpdf = "<html><head><style>body { font-size:20; }</style></head><body>";
            preparedpdf = preparedpdf + "<H2>" + eval.Year + " " + evaltypedesc + " Performance Evaluation</H2>";

            preparedpdf = preparedpdf + "<p>" + reportinfo.name + " - " + reportinfo.banner_title + "<br />";
            preparedpdf = preparedpdf + "Library Start Date: " + reportinfo.LibraryStartDate + "<br />";
            preparedpdf = preparedpdf + "Supervisor: " + supinfo.name + " - " + supinfo.banner_title + "</p>";

            preparedpdf = preparedpdf + "<p>Date Started: " + eval.StartDate.ToString("MM/dd/yyyy") + "<br />";
            preparedpdf = preparedpdf + "Date Submitted to Employee: " + eval.SubmittedDate?.ToString("MM/dd/yyyy") + "<br />";
            preparedpdf = preparedpdf + "Date Accepted: " + eval.AcceptedDate?.ToString("MM/dd/yyyy") + "<br />";
            preparedpdf = preparedpdf + "Date Contested: " + eval.ContestedDate?.ToString("MM/dd/yyyy") + "<br />";
            preparedpdf = preparedpdf + "Date Completed: " + eval.CompleteDate?.ToString("MM/dd/yyyy") + "<br />";
            preparedpdf = preparedpdf + "Date Received by HR: " + eval.ProcessedDate?.ToString("MM/dd/yyyy") + "</p>";

            preparedpdf = preparedpdf + "<ol>";
            foreach (Question q in qa)
            {
                if (q.QuestionText == "Job Description")
                {
                    jd = q.QuestionComment;
                }
                else
                {
                    preparedpdf = preparedpdf + "<li>" + q.QuestionText + "<br />";
                    preparedpdf = preparedpdf + "Rating: " + q.QuestionRating + "<br />";
                    preparedpdf = preparedpdf + "Comments:<br/>" + q.QuestionComment + "<br />&nbsp;</li>";
                }
            }
            preparedpdf = preparedpdf + "</ol>";

            preparedpdf = preparedpdf + "<p>Employee Comments:<br/>" + eval.EmployeeComments + "</p>";

            if (ep != true)
            {
                preparedpdf = preparedpdf + "<p>Supervisor Comments:<br/>" + eval.EvaluatorComments + "</p>";
            }

            preparedpdf = preparedpdf + "<div style='page-break-before: always'><h2>Job Description</h2>" + jd + "</div>";

            preparedpdf = preparedpdf + "</body></html>";

            return preparedpdf;
        }
    }
}