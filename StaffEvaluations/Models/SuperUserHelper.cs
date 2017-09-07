using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace StaffEvaluations.Models
{
    public static class SuperUserHelper
    {

        public static bool IsAdSuperUser(string id)
        {
            var ret = false;

            string sulist = ConfigurationManager.AppSettings["AdSuperUsers"].ToString();

            if (sulist.IndexOf(id,StringComparison.OrdinalIgnoreCase)>=0)
            {
                ret = true;
            }
            return ret;
        }
    }
}