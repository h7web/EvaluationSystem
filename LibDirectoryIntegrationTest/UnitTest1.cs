using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LibDirectoryIntegration;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;

namespace LibDirectoryIntegrationTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestGetAllSupervisors()
        {
            List<ReportingLine> reportingLines = LibDirectoryFactory.GetAllSupervisors();

            Assert.IsTrue(reportingLines.Count > 0);

            ReportingLine oneLine = reportingLines.FirstOrDefault();

            Assert.IsFalse(String.IsNullOrWhiteSpace(oneLine.supervisor.netid));

        }

        [TestMethod]
        public void TestGetSupervisor()
        {
            Supervisor super = LibDirectoryFactory.GetSupervisor("schlemba");


            Assert.AreEqual("schlemba", super.netid);

            Assert.IsTrue(super.direct_reports.Count > 0);

        }

        [TestMethod]
        public void TestGetPersonsSupervisors()
        {
            List<Supervisor> supers = LibDirectoryFactory.GetPersonsSupervisors("jschrade");

            Assert.AreEqual("w-mischo", supers.FirstOrDefault().netid);

        }

        [TestMethod]
        public void TestGetPerson()
        {
            LibDirectoryPerson pers = LibDirectoryFactory.GetPerson("atjohnsn");

            Assert.AreEqual("Jenny Marie", pers.first);
            Assert.AreEqual("Johnson", pers.last);

            pers = LibDirectoryFactory.GetPerson("abc123xyz");

            Assert.IsNull(pers);

        }

    }
}
