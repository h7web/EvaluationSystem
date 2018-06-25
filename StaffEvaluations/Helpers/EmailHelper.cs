using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace StaffEvaluations.Helpers
{
    public class EmailHelper
    {
        public static List<SelectListItem> GetEmlList(Models.Entities db, string selected = null)
        {
            List<SelectListItem> ret = new List<SelectListItem>();

            var val = "Employee List";
            ret.Add(new SelectListItem { Text = val, Value = "emplist", Selected = (val == selected) });

            val = "Employee List w Status";
            ret.Add(new SelectListItem { Text = val, Value = "empstatuslist", Selected = (val == selected) });

            ret.Insert(0, new SelectListItem { Text = "* You must select a value *", Value = "* You must select a value *" });

            return ret;
        }
    }
}