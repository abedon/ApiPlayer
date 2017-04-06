using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using AutoTest.Common;

namespace AutoTest.Server.Models
{
    public class TestSuiteContext
    {
        public List<TestSuite> TestSuites { get; set; }
        public string TestSuiteRootFolder { get; set; }

        public TestSuiteContext()
        {
            TestSuites = new List<TestSuite>();
            TestSuiteRootFolder = HttpContext.Current.Server.MapPath(AutoTestingConstants.TEST_SUITE_SUB_FOLDER);

            if (!Directory.Exists(TestSuiteRootFolder))
            {
                Directory.CreateDirectory(TestSuiteRootFolder);
            }

            //initialize the list by loading XML folders/files
            string[] testSuiteFolders = Directory.GetDirectories(TestSuiteRootFolder);

            foreach (string testCaseFolder in testSuiteFolders)
            {
                //create a testcase object
                TestSuite testSuite = new TestSuite();

                testSuite.ID = int.Parse(testCaseFolder.Substring(testCaseFolder.LastIndexOf("\\") + 1));

                string[] testSuiteFiles = Directory.GetFiles(testCaseFolder);

                foreach (string testSuiteFile in testSuiteFiles)
                {
                    FileInfo fileInfo = new FileInfo(testSuiteFile);

                    switch (fileInfo.Name.ToLower())
                    {
                        case "testsuite.xml":
                            testSuite = ModelProcessor.GetTestSuiteFromFile(testSuiteFile);
                            break;
                    }
                }

                TestSuites.Add(testSuite);
            }
        }
    }
}