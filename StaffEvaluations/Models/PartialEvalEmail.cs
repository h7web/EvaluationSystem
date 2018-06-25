using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace StaffEvaluations.Models
{
    [MetadataType(typeof(EvalEmailMD))]
    public partial class EvalEmail
    {
        public string Order { get; set; }
    }

    public class EvalEmailMD
    {
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public Nullable<System.DateTime> send_date { get; set; }

    }
}