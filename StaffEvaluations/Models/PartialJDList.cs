﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StaffEvaluations.Models
{
    public partial class JDList
    {
        public string EmployeeLast { get; set; }
        public string EmployeeFirst { get; set; }
        public string SuperLast { get; set; }
        public string EmployeeUnit { get; set; }
        public string SupervisorUnit { get; set; }
        public string Order { get; set; }
        public string Fix { get; set; }
    }
}