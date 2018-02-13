using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StaffEvaluations.Models
{
    public class Question
    {
        public int QuestionId { get; set; }

        public int QuestionSetId { get; set; }

        public int EvalId { get; set; }

        public string QuestionCode { get; set; }

        public string QuestionType { get; set; }

        public string QuestionText { get; set; }

        public int Year { get; set; }

        public string QuestionRating { get; set; }

        public string QuestionComment { get; set; }

        public bool CommentOnly { get; set; }

        public string Index {get; }

        public string namePrefix
        {
            get
            {
                return String.Format("Question[{0}].", Index);
            }
        }

        public string namePrefixId
        {
            get
            {
                return namePrefix.Replace("[", "_").Replace("]", "_").Replace(".", "_");
            }
        }

        public Question()
        {
            Index = Guid.NewGuid().ToString();
        }

        public string highlight { get; set; }
    }
}