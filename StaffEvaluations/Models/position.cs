//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace StaffEvaluations.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class position
    {
        public int id { get; set; }
        public int EDW_PERS_ID { get; set; }
        public string EMPEE_CLS_CD { get; set; }
        public string EMPEE_CLS_LONG_DESC { get; set; }
        public Nullable<double> FAC_TENURE_TRACK_YR { get; set; }
        public string FAC_RANK_DESC { get; set; }
        public string POSN_NBR { get; set; }
        public string JOB_DETL_TITLE { get; set; }
        public Nullable<System.DateTime> JOB_DETL_EFF_DT { get; set; }
        public Nullable<System.DateTime> JOB_DETL_DATA_EXP_DT { get; set; }
        public string ORG_CD { get; set; }
        public string ORG_TITLE { get; set; }
        public string JOB_DETL_SUB_DEPT_LEVEL_6_NAME { get; set; }
        public string JOB_DETL_SUB_DEPT_LEVEL_7_NAME { get; set; }
        public string PAPE_CIV_SVC_ID { get; set; }
        public string POSN_EXEMPT_IND { get; set; }
        public Nullable<double> JOB_DETL_FTE { get; set; }
        public Nullable<double> JOB_DETL_ANNL_SAL { get; set; }
        public string JOB_PROBN_BGN_DT { get; set; }
        public string JOB_PROBN_END_DT { get; set; }
        public string JOB_DETL_STATUS_DESC { get; set; }
        public string POSN_SAL_GROUP_DESC { get; set; }
        public string ADROLE { get; set; }
        public string JOB_CNTRCT_TYPE_DESC { get; set; }
        public string CFOAP { get; set; }
        public string CFOAP_AMT { get; set; }
        public Nullable<int> GRANTFUNDS { get; set; }
    }
}
