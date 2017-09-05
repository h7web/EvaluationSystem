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

    }
}
