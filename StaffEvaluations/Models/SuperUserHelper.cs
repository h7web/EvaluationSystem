using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.Mvc;

namespace StaffEvaluations.Models
{
    public static class SuperUserHelper
    {

        public static bool IsAdSuperUser(string id)
        {
            var ret = false;

            string sulist = ConfigurationManager.AppSettings["AdSuperUsers"].ToString();

            if (sulist.IndexOf(id, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                ret = true;
            }
            return ret;
        }

        public static List<SelectListItem> GetSupervisors()
        {
            List<SelectListItem> ret = new List<SelectListItem>();

            var splist = LibDirectoryIntegration.LibDirectoryFactory.GetAllSupervisors();

            splist = (splist.OrderBy(w => w.supervisor.last)).ToList();

            foreach (LibDirectoryIntegration.ReportingLine rp in splist)
            {
                ret.Add(new SelectListItem { Text = rp.supervisor.name, Value = rp.supervisor.netid });
            }
            return ret;

        }
    }
}