using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace StaffEvaluations.Models
{
    public class SuperUserHelper
    {

        public string IsAdSuperUser(string id)
        {
            var ret = "";

            string sulist = ConfigurationManager.AppSettings["AdSuperUsers"].ToString();

            if (sulist.Contains(id))
            {
                ret = "true";
            }
            return ret;
        }
    }
}