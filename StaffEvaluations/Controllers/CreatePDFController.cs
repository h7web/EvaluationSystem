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
            
            SelectPdf.GlobalProperties.LicenseKey = "CSI4KTs8OCk4KTgxJzkpOjgnODsnMDAwMA==";

            HtmlToPdf converter = new HtmlToPdf();

            converter.Options.MarginBottom = 20;
            converter.Options.MarginLeft = 20;
            converter.Options.MarginRight = 20;
            converter.Options.MarginTop = 20;

            PdfDocument doc = converter.ConvertHtmlString(htmlString);

            var eval = (from ev in db.StaffPerformanceEvaluations where ev.EvalId == id select ev).SingleOrDefault();
            var posn_alt = (from p in db.JobDescriptions where p.netid == eval.NetId && p.supervisorNetid == eval.EvaluatorNetid select p.posn_number).SingleOrDefault();
            var reportinfo = LibDirectoryFactory.GetPerson(eval.NetId);
            var posn = eval.posn_number;

            if (posn == null)
            {
                posn = posn_alt;
            }

            string filename = "PerformEval_" + posn + "_" + reportinfo.last.Replace(" ", "_") + "_" + eval.Year + ".pdf";

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
            var yr = eval.Year - 1;

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

            preparedpdf = "<html><head><style>body, td, td p { font-size:20; }</style></head><body><table width='100%'><tr><td colspan='2'>";
            preparedpdf = preparedpdf + "<H2>" + eval.Year + " " + evaltypedesc + " Performance Evaluation</H2></td></tr>";

            preparedpdf = preparedpdf + "<tr><td><p>" + eval.Name + " - " + eval.Title + "<br />";
            preparedpdf = preparedpdf + "Library Start Date: " + eval.LibraryStartDate?.ToShortDateString() + "<br />";
            preparedpdf = preparedpdf + "Supervisor: " + eval.EvaluatorName + " - " + eval.EvaluatorTitle + "</p>";

            preparedpdf = preparedpdf + "<p>Review period: January 1 " + yr + " to December 31 " + yr + "</p></td>";
            if (eval.Status == "Processed")
            {
                preparedpdf = preparedpdf + "<td align='right'><img src='http://iisdev1.library.illinois.edu/StaffEvaluations/Content/final.jpg' border='0' width='200' height='134' /></td>";
            }
            else
            {
                preparedpdf = preparedpdf + "<td align='right'><img src='http://iisdev1.library.illinois.edu/StaffEvaluations/Content/pending.jpg' border='0' width='200' height='134' /></td>";
            }
            preparedpdf = preparedpdf + "</tr></table>";

            preparedpdf = preparedpdf + "<p>Date Started: " + eval.StartDate.ToString("MM/dd/yyyy") + " (" + eval.EvaluatorNetid + ")" + "<br />";
            if (eval.SubmittedDate != null)
            {
                preparedpdf = preparedpdf + "Date Submitted to Employee: " + eval.SubmittedDate?.ToString("MM/dd/yyyy") + " (" + eval.EvaluatorNetid + ")" + "<br />";
            }
            if (eval.AcceptedDate != null)
            {
            preparedpdf = preparedpdf + "Date Accepted: " + eval.AcceptedDate?.ToString("MM/dd/yyyy") + " (" + eval.NetId + ")" + "<br />";
            }
            if (eval.ContestedDate != null)
            {
                preparedpdf = preparedpdf + "Date Contested: " + eval.ContestedDate?.ToString("MM/dd/yyyy") + " (" + eval.NetId + ")" + "<br />";
            }
            if (eval.CompleteDate != null)
            {
                preparedpdf = preparedpdf + "Date Submitted to HR: " + eval.CompleteDate?.ToString("MM/dd/yyyy") + " (" + eval.EvaluatorNetid + ")" + "<br />";
            }
            if (eval.ProcessedDate != null)
            {
                preparedpdf = preparedpdf + "Date Processed by HR: " + eval.ProcessedDate?.ToString("MM/dd/yyyy") + "</p>";
            }

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