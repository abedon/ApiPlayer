using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Xml.Linq;
using AutoTest.Common;

namespace AutoTest.Server.Models
{
    public class TestCaseResultContext
    {
        public List<TestCaseResult> TestCaseResults { get; set; }

        public TestCaseResultContext()
        {
            TestCaseResults = new List<TestCaseResult>();
            var testResults = ApiConnector.LoadAllTestCaseResults(true);
            if (testResults != null)
            {
                foreach (var testResult in testResults)
                    TestCaseResults.Add(testResult);
            }
        }
    }
}