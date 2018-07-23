using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StaffEvaluations.Helpers
{
    public class Logger
    {
        static log4net.ILog _log = null;


        public static log4net.ILog Log
        {
            get
            {
                if (_log == null)
                {
                    _log = log4net.LogManager.GetLogger("FileAndEmail");
                }
                return _log;

            }
        }
    }
}