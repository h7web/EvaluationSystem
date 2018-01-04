using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace StaffEvaluations.Models
{
    [MetadataType(typeof(JobDescriptionMD))]
    public partial class JobDescription
    {
        public string JDName { get; set; }

        public string JDSuper { get; set; }
    }

    public class JobDescriptionMD
    {
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public Nullable<System.DateTime> lastUpdatedDate { get; set; }

    }
}