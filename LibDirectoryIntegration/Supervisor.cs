using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LibDirectoryIntegration
{
    public class Supervisor : LibDirectoryPerson
    {

        public List<DirectReport> direct_reports { get; set; }

        public List<DirectReport> eval_direct_reports
        {
            get
            {
                var types = new[] { "B", "C" };
                List<DirectReport> ret = new List<DirectReport>();

                if (this.direct_reports != null)
                {
                    ret = this.direct_reports.Where(d => types.Contains(d.employee_type_code)).DefaultIfEmpty().ToList();
                }

                return ret;
            }
            set
            {

            }

        }
        public List<DirectReport> civil_service_direct_reports
        {
            get
            {
                List<DirectReport> ret = this.direct_reports.Where(d => d.employee_type_code == "C").ToList();
                return ret;
            }

        }

    }
}
