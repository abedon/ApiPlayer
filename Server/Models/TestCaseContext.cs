using System.Collections.Generic;
using System.IO;
using System.Web;
using AutoTest.Common;
using System;
using System.Diagnostics;

namespace AutoTest.Server.Models
{
    public class TestCaseContext
    {
        public List<TestCase> TestCases { get; set; }
        public string TestCaseRootFolder { get; set; }
        public string AutoTestApiRoot { get; set; }

        public TestCaseContext()
        {
            TestCases = new List<TestCase>();
            var testCases = ApiConnector.LoadAllTestCases(true);
            if (testCases != null)
            {
                foreach(var testCase in testCases)
                    TestCases.Add(testCase);
            }
        }
    }
}