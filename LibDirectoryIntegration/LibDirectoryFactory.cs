using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace LibDirectoryIntegration
{
    public static class LibDirectoryFactory
    {

        static object locker = new Object();

        static List<ReportingLine> _lines = null;

        /// <summary>
        /// Return a list of all supervisors and their direct reports
        /// </summary>
        /// <param name="endPointUrl"></param>
        /// <returns></returns>
        public static List<ReportingLine> GetAllSupervisors()
        {


            List<ReportingLine> ret = _lines;

            if (ret == null)
            {
                lock (locker)
                {
                    ret = _lines;

                    if (ret == null)
                    {
                        string src = "";

                        try
                        {
                            string endPointUrl = ConfigurationManager.AppSettings["AllSupervisorsUrl"];
                            HttpClient httpc = new HttpClient();
                            Task<string> t = httpc.GetStringAsync(endPointUrl);
                            src = t.Result;
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }

                        ret = JsonConvert.DeserializeObject<List<ReportingLine>>(src);
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Return the supervisor with the given netid
        /// </summary>
        /// <param name="netid"></param>
        /// <returns></returns>
        public static Supervisor GetSupervisor(string netid)
        {
            Supervisor ret = null;
            try
            {
                ret = LibDirectoryFactory.GetAllSupervisors().SingleOrDefault(s => s.supervisor.netid.Equals(netid, StringComparison.OrdinalIgnoreCase)).supervisor;
            }
            catch
            {
                //ignore and return null;
            }
            return ret;
        }

        /// <summary>
        /// Return all supervisors for the given person
        /// </summary>
        /// <param name="netid"></param>
        /// <returns></returns>
        public static List<Supervisor> GetPersonsSupervisors(string netid)
        {
            List<Supervisor> ret = null;

            try
            {
                ret = LibDirectoryFactory.GetAllSupervisors().Where(rl => rl.supervisor.direct_reports.Any(dr => dr.netid.Equals(netid, StringComparison.OrdinalIgnoreCase))).Select(rl => rl.supervisor).ToList();
            }
            catch
            {
                //ignore and return null
            }

            return ret;
        }

        /// <summary>
        /// Return the LibDirectoryPerson for the given netid
        /// Performs a brute force O(n) search
        /// </summary>
        /// <param name="netid"></param>
        /// <returns></returns>
        public static LibDirectoryPerson GetPerson(string netid)
        {
            LibDirectoryPerson ret = null;
            foreach (ReportingLine rl in LibDirectoryFactory.GetAllSupervisors())
            {
                if (rl.supervisor.netid.Equals(netid, StringComparison.OrdinalIgnoreCase))
                {
                    ret = rl.supervisor;
                    break;
                }
                else
                {
                    foreach (DirectReport dr in rl.supervisor.direct_reports)
                    {
                        if (dr.netid.Equals(netid, StringComparison.OrdinalIgnoreCase))
                        {
                            ret = dr;
                            break;
                        }
                    }
                    if (ret != null) break;
                }
            }

            return ret;
        }

    }
}
