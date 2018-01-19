using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace StaffEvaluations.Helpers
{
    public class FormatHelper
    {
        static public string JDFormat(string JD)
        {
            string formattedJD = JD;
            string[,] formats = new string[48, 3]{
                {"Unit Name:", "<b>", "</b>&nbsp;" },
                {"Employee Name:", "<b>", "</b>&nbsp;" },
                {"Position Number:", "<b>", "</b>&nbsp;" },
                {"PAPE Number:", "<b>", "</b>&nbsp;" },
                {"Incumbent:", "<b>", "</b>&nbsp;" },
                {"TSO:", "<b>", "</b>&nbsp;" },
                {"Department:", "<b>", "</b>&nbsp;" },
                {"Administrative Unit:", "<b>", "</b>&nbsp;" },
                {"Present Classification:", "<b>", "</b>&nbsp;" },
                {"Reason for Position Description: (please check one)", "</p><p><b>", "</b>" },
                {"Major function within the unit.", "</p><p><b>", "</b>" },
                {"Reporting line information.", "</p><p><b>", "</b>" },
                {"Core Responsibilities.", "</p><p><b>", "</b>" },
                {"Position Requirements.", "</p><p><b>", "</b>" },
                {"University Job Title:", "</p><p><b>", "</b>" },
                {"University Position Title:", "</p><p><b>", "</b>" },
                {"Primary Position Function/Summary:", "</p><p><b>", "</b>" },
                {"Major Duties and Responsibilities", "</p><p><b>", "</b>" },
                {"Major Duties and Responsibilities.", "</p><p><b>", "</b>" },
                {"Major Duties and Responsibilities:", "</p><p><b>", "</b>" },
                {"Organizational Chart:", "</p><p><b>", "</b>" },
                {"Position Requirements and Qualifications", "</p><p><b>", "</b>" },
                {"Position Requirements and Qualifications.", "</p><p><b>", "</b>" },
                {"Position Requirements and Qualifications:", "</p><p><b>", "</b>" },
                {"Education:", "</p><p><b>", "</b>" },
                {"Experience:", "</p><p><b>", "</b>" },
                {"Training:", "</p><p><b>", "</b>" },
                {"Knowledge:", "</p><p><b>", "</b>" },
                {"Knowledge Requirements:", "</p><p><b>", "</b>" },
                {"Supervisor:", "</p><p><b>", "</b>" },
                {"Supervisor Control:", "</p><p><b>", "</b>" },
                {"FUNCTION", "</p><p><b><UCASE>","</UCASE></b>&nbsp;" },
                {"FUNCTION:", "</p><p><b><UCASE>","</UCASE></b>&nbsp;" },
                {"ORGANIZATIONAL RELATIONSHIP:", "</p><p><b><UCASE>","</UCASE></b>&nbsp;" },
                {"DUTIES AND RESPONSIBILITIES:", "</p><p><b><UCASE>","</UCASE></b>&nbsp;" },
                {"ENVIRONMENTAL DEMANDS", "</p><p><b><UCASE>","</UCASE></b>&nbsp;" },
                {"1. Knowledge required for the job", "</p><p><b>","</b>&nbsp;" },
                {"2. Responsibility", "</p><p><b>","</b>&nbsp;" },
                {"a. Supervisory Controls", "</p><p><b>","</b>&nbsp;" },
                {"b. Guidelines", "</p><p><b>","</b>&nbsp;" },
                {"3. Difficulty", "</p><p><b>","</b>&nbsp;" },
                {"a. Complexity", "</p><p><b>","</b>&nbsp;" },
                {"b. Scope and Effect", "</p><p><b>","</b>&nbsp;" },
                {"4. Personal relationships", "</p><p><b>","</b>&nbsp;" },
                {"a. Personal Contact", "</p><p><b>","</b>&nbsp;" },
                {"b. Purpose for Contact", "</p><p><b>","</b>&nbsp;" },
                {"1. Physical Requirements", "</p><p><b>","</b>&nbsp;" },
                {"2. Work Environments", "</p><p><b>","</b>&nbsp;" }
                };

            formattedJD = Regex.Replace(formattedJD, @"[\d\.]+%", "<b>$0</b>");

            int i;
            for (i=0; i < 48; i++ )
            {
                var oldval = formats[i, 0];
                var newval = formats[i, 1] + formats[i,0]+formats[i,2];
                formattedJD = formattedJD.Replace(oldval, newval);
            }

            return formattedJD;
        }
    }
}