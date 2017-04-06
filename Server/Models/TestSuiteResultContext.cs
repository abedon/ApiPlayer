using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Xml.Linq;
using AutoTest.Common;

namespace AutoTest.Server.Models
{
    public class TestSuiteResultContext
    {
        public List<TestSuiteResult> TestSuiteResults { get; set; }

        public TestSuiteResultContext()
        {
            TestSuiteResults = new List<TestSuiteResult>();

            string resFolder = HttpContext.Current.Server.MapPath(AutoTestingConstants.TEST_SUITE_RESULT_SUB_FOLDER);

            if (!Directory.Exists(resFolder))
            {
                Directory.CreateDirectory(resFolder);
            }

            var testSuiteResultFiles = Directory.GetFiles(resFolder);

            foreach (var testSuiteResultFile in testSuiteResultFiles)
            {
                var testSuiteResult = ModelProcessor.GetTestSuiteResultFromFile(testSuiteResultFile);
                if (testSuiteResult == null)
                    continue;

                TestSuiteResults.Add(testSuiteResult);
            }
        }
    }
}