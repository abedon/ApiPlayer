using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WebDav;
using System.Net;
using System.Configuration;
using System.Net.Sockets;
using System.Xml;
using System.Threading;
using System.Xml.Linq;
using System.IO;
using System.Web;

namespace AutoTest.Common
{
    public class WebDavConnector
    {
        private WebDavManager WebDavManager;

        private List<WebDavResource> TestCaseResources;
        private List<WebDavResource> TestSuiteResources;
        private List<WebDavResource> TestCaseResultResources;
        private List<WebDavResource> TestSuiteResultResources;

        private string LocalIP;

        private object testCaseLock = new object();

        private delegate void AsyncDelegate(TestCase _testCase, ref TestCaseResult _testCaseResult);

        private string TestCaseRootFolder;

        #region Public Members

        public void Initialize(string clientIP = null)
        {
            if (clientIP != null)
                LocalIP = clientIP; // Utils.GetLocalIP();
            else
                LocalIP = ""; 

            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["Domain"]))
            {
                WebDavManager = new WebDavManager(new WebDavCredential(ConfigurationManager.AppSettings["Username"],
                                                                        ConfigurationManager.AppSettings["Password"]));
            }
            else
            {
                WebDavManager = new WebDavManager(new WebDavCredential(ConfigurationManager.AppSettings["Username"],
                                                                        ConfigurationManager.AppSettings["Password"],
                                                                        ConfigurationManager.AppSettings["Domain"],
                                                                        AuthType.Ntlm));
            }

            //Utils.Username = ConfigurationManager.AppSettings["Username"];
            //Utils.Password = ConfigurationManager.AppSettings["Password"];
            //Utils.Domain = ConfigurationManager.AppSettings["Domain"];

            TestCaseRootFolder = HttpContext.Current.Server.MapPath(AutoTestingConstants.TEST_CASE_SUB_FOLDER);

            RefreshResources();
        }

        #region List Members

        public void ListAll()
        {
            ListTestCases();
            Console.WriteLine("");
            ListTestSuites();
        }

        public void ListTestCases()
        {
            Console.WriteLine(" Test Cases:");

            RefreshResources();

            if (TestCaseResources.Count != 0)
            {
                foreach (WebDavResource testCaseResource in TestCaseResources)
                {
                    int testID = int.Parse(testCaseResource.Name);
                    bool found = false;

                    foreach (WebDavResource testCaseResultResource in TestCaseResultResources)
                    {
                        string[] testInformation = testCaseResultResource.Name.ToLower().Split('_');

                        testInformation[2] = testInformation[2].Split('.')[0];

                        int currentTestID = int.Parse(testInformation[0]);
                        Guid currentResultID = Guid.Parse(testInformation[2]);

                        if (testID == currentTestID)
                        {
                            found = true;

                            Console.WriteLine("  " + currentTestID + " (" + currentResultID + ")");
                        }
                    }

                    if (!found)
                    {
                        Console.WriteLine("  " + testID + " (No Results)");
                    }
                }
            }
            else
            {
                Console.WriteLine("  There are no tests cases available");
            }
        }

        public void ListTestSuites()
        {
            Console.WriteLine(" Test Suites:");

            RefreshResources();

            if (TestSuiteResources.Count != 0)
            {
                foreach (WebDavResource testSuiteResource in TestSuiteResources)
                {
                    int testID = int.Parse(testSuiteResource.Name);
                    bool found = false;

                    foreach (WebDavResource testSuiteResultResource in TestSuiteResultResources)
                    {
                        string[] testInformation = testSuiteResultResource.Name.ToLower().Split('_');

                        testInformation[2] = testInformation[2].Split('.')[0];

                        int currentTestID = int.Parse(testInformation[0]);
                        Guid currentResultID = Guid.Parse(testInformation[2]);

                        if (testID == currentTestID)
                        {
                            found = true;

                            Console.WriteLine("  " + currentTestID + " (" + currentResultID + ")");
                        }
                    }

                    if (!found)
                    {
                        Console.WriteLine("  " + testID + " (No Results)");
                    }
                }
            }
            else
            {
                Console.WriteLine("  There are no tests suites available");
            }
        }

        #endregion

        #region Run Members
        public void RunAll()
        {
            RunAllTestSuites();
            Console.WriteLine("");
            RunAllTestCases();
        }

        public void RunAllTestCases()
        {
            RefreshResources();

            Console.WriteLine(" Loading Test Cases");

            TestCase[] testCases = LoadAllTestCases();

            if (testCases.Length > 0)
            {
                Console.WriteLine(" Running Test Cases");

                TestCaseResult[] testCaseResults = RunTestCases(testCases);

                if (testCaseResults.Length > 0)
                {
                    Console.WriteLine(" Generating Results");

                    if (ModelProcessor.GenerateTestCaseResultXML(testCaseResults))
                    {
                        Console.WriteLine(" Uploading Results");

                        if (!UploadTestCaseResults(testCaseResults))
                        {
                            Console.WriteLine("Failed to upload generated test case result data.");
                        }
                    }
                }
            }
        }

        public void RunAllTestSuites()
        {
            RefreshResources();

            Console.WriteLine(" Loading Test Suites");

            TestSuite[] testSuites = LoadAllTestSuites();

            if (testSuites.Length > 0)
            {
                Console.WriteLine(" Running Test Suites");

                TestSuiteResult[] testSuiteResults = RunTestSuites(testSuites);

                if (testSuiteResults.Length > 0)
                {
                    Console.WriteLine(" Generating Results");

                    if (ModelProcessor.GenerateTestSuiteResultXML(testSuiteResults))
                    {
                        Console.WriteLine(" Uploading Results");

                        if (!UploadTestSuiteResults(testSuiteResults))
                        {
                            Console.WriteLine(" Failed to upload generated test case result data.");
                        }
                    }
                }
                else
                {
                    Console.WriteLine(" Failed to run test suites.");
                }
            }
            else
            {
                Console.WriteLine(" No test suites loaded.");
            }
        }

        public void RunSelectTestCases(int[] _testCaseIDs)
        {
            RefreshResources();

            Console.WriteLine(" Loading Test Cases");

            TestCase[] TestCases = LoadSelectTestCases(_testCaseIDs);

            if (TestCases.Length > 0)
            {
                Console.WriteLine(" Running Test Cases");

                TestCaseResult[] TestCaseResults = RunTestCases(TestCases);

                if (TestCaseResults.Length > 0)
                {
                    Console.WriteLine(" Generating Results");

                    if (ModelProcessor.GenerateTestCaseResultXML(TestCaseResults))
                    {
                        Console.WriteLine(" Uploading Results");

                        if (!UploadTestCaseResults(TestCaseResults))
                        {
                            Console.WriteLine("Failed to upload generated test case result data.");
                        }
                    }
                }
            }
        }

        public void RunSelectTestSuites(int[] _testSuiteIDs)
        {
            RefreshResources();

            Console.WriteLine(" Loading Test Suites");

            TestSuite[] TestSuites = LoadSelectTestSuites(_testSuiteIDs);

            if (TestSuites.Length > 0)
            {
                Console.WriteLine(" Running Test Suites");

                TestSuiteResult[] TestSuiteResults = RunTestSuites(TestSuites);

                if (TestSuiteResults.Length > 0)
                {
                    Console.WriteLine(" Generating Results");

                    if (ModelProcessor.GenerateTestSuiteResultXML(TestSuiteResults))
                    {
                        Console.WriteLine(" Uploading Results");

                        if (!UploadTestSuiteResults(TestSuiteResults))
                        {
                            Console.WriteLine("Failed to upload generated test suite result data.");
                        }
                    }
                }
            }
        }

        public void RunSelectTestCasesForTMS(int[] _testCaseIDs)
        {
            RefreshResources();

            TestCase[] TestCases = LoadSelectTestCases(_testCaseIDs);

            if (TestCases.Length > 0)
            {
                TestCaseResult[] TestCaseResults = RunTestCases(TestCases, true);

                if (TestCaseResults.Length > 0)
                {
                    if (ModelProcessor.GenerateTestCaseResultXMLForTMS(TestCaseResults))
                    {
                        Console.WriteLine(GetFormattedXml(ModelProcessor.GetTestCaseResultXMLForTMS(TestCaseResults)));
                        return;
                    }
                }
            }

            Console.WriteLine("<results></results>");
        }

        public void RunSelectTestSuitesForTMS(int[] _testSuiteIDs)
        {
            RefreshResources();

            TestSuite[] TestSuites = LoadSelectTestSuites(_testSuiteIDs);

            if (TestSuites.Length > 0)
            {
                TestSuiteResult[] TestSuiteResults = RunTestSuites(TestSuites, true);

                if (TestSuiteResults.Length > 0)
                {
                    if (ModelProcessor.GenerateTestSuiteResultXMLForTMS(TestSuiteResults))
                    {
                        Console.WriteLine(GetFormattedXml(ModelProcessor.GetTestSuiteResultXMLForTMS(TestSuiteResults)));
                        return;
                    }
                }
            }

            Console.WriteLine("<results></results>");
        }

        public TestCaseResult[] RunTestCases(TestCase[] _testCases, bool _suppressMessages = false)
        {
            List<TestCaseResult> testCaseResults = new List<TestCaseResult>();

            foreach (TestCase testCase in _testCases)
            {
                if (!_suppressMessages)
                {
                    Console.Write("  " + testCase.ID + ": Running...");
                }

                TestCaseResult testResult = new TestCaseResult(testCase.ID);

                testResult.StartTime = DateTime.Now;
                testResult.TesterIP = LocalIP;
                testResult.LongestTime = 0;
                testResult.ShortestTime = Int32.MaxValue;
                testResult.TotalTime = 0;

                //check if we need to reset server cache first
                if (testCase.Settings.ResetServerCacheFirst)
                {
                    //send a special request with the reset command
                    var tcReset = new TestCase
                    {
                        ID = -1,
                        Description = "reset server cache",
                        Settings = new ExecutionSettings
                        {
                            IsDebugEnabled = true,
                            Interval = 0,
                            Amount = 1,
                            LoopsOfMixedModeRunning = 0,
                            Mode = ExecutionMode.Interval,
                            ResetServerCacheFirst = false
                        },
                        Request = new TestRequest
                        {
                            DebugData = testCase.Request.DebugData.Replace("<debugcontent>", "<debugcontent><add type=\"resetservervars\"/>"),
                            AppVersion = testCase.Request.AppVersion,
                            TargetIp = testCase.Request.TargetIp,
                            TargetDomain = testCase.Request.TargetDomain,
                            Verb = RequestType.POST,
                        }
                    };
                    var tcResult = new TestCaseResult(-1);
                    ExecuteTestCase(tcReset, ref tcResult);
                }

                if (testCase.Settings.Mode == ExecutionMode.Interval) // send one request every interval
                {
                    for (int i = 0; i < testCase.Settings.Amount; i++)
                    {
                        ExecuteTestCase(testCase, ref testResult);

                        Thread.Sleep(testCase.Settings.Interval * 1000);
                    }
                }
                else if (testCase.Settings.Mode == ExecutionMode.Simultaneous) // send requests concurrently at once
                {
                    AsyncDelegate asyncOperation = ExecuteTestCase;
                    List<IAsyncResult> asyncResults = new List<IAsyncResult>();

                    for (int i = 0; i < testCase.Settings.Amount; i++)
                    {
                        asyncResults.Add(asyncOperation.BeginInvoke(testCase, ref testResult, null, null));
                    }

                    foreach (IAsyncResult asyncResult in asyncResults)
                    {
                        asyncOperation.EndInvoke(ref testResult, asyncResult);
                    }
                }
                else if (testCase.Settings.Mode == ExecutionMode.Mixed) // send requests concurrently every interval for LoopsOfMixedModeRunning times
                {
                    for (int j = 0; j < testCase.Settings.LoopsOfMixedModeRunning; j++)
                    {
                        AsyncDelegate asyncOperation = ExecuteTestCase;
                        List<IAsyncResult> asyncResults = new List<IAsyncResult>();

                        for (int i = 0; i < testCase.Settings.Amount; i++)
                        {
                            asyncResults.Add(asyncOperation.BeginInvoke(testCase, ref testResult, null, null));
                        }

                        foreach (IAsyncResult asyncResult in asyncResults)
                        {
                            asyncOperation.EndInvoke(ref testResult, asyncResult);
                        }

                        Thread.Sleep(testCase.Settings.Interval * 1000);
                    }
                }

                testResult.EndTime = DateTime.Now;
                testResult.AverageTime = testResult.TotalTime / testResult.TotalRequests;

                testCaseResults.Add(testResult);

                if (!_suppressMessages)
                {
                    Console.WriteLine("Done (" + testResult.TotalTime + "ms)");
                }
            }

            return testCaseResults.ToArray();
        }

        public TestSuiteResult[] RunTestSuites(TestSuite[] _testSuites, bool _suppressMessages = false)
        {
            List<TestSuiteResult> testSuiteResults = new List<TestSuiteResult>();

            foreach (TestSuite testSuite in _testSuites)
            {
                if (!_suppressMessages)
                {
                    Console.Write("  " + testSuite.ID + ": Running...");
                }

                TestSuiteResult testSuiteResult = new TestSuiteResult(testSuite.ID);

                testSuiteResult.StartTime = DateTime.Now;
                testSuiteResult.TesterIP = LocalIP;
                testSuiteResult.TotalTime = 0;

                TestCaseResult[] testCaseResults;

                if (testSuite.Settings.ExecutionMode == SuiteExecutionMode.Simultaneous)
                {
                    TestCase[] testCases = LoadSelectTestCases(testSuite.TestCases.ToArray(), true);

                    AsyncDelegate asyncOperation = ExecuteTestCaseFromTestSuite;
                    List<IAsyncResult> asyncResults = new List<IAsyncResult>();

                    testCaseResults = new TestCaseResult[testCases.Length];

                    for (int i = 0; i < testCases.Length; ++i)
                    {
                        testCaseResults[i] = new TestCaseResult(testCases[i].ID);
                        asyncResults.Add(asyncOperation.BeginInvoke(testCases[i], ref testCaseResults[i], null, null));
                    }

                    for (int i = 0; i < testCases.Length; ++i)
                    {
                        asyncOperation.EndInvoke(ref testCaseResults[i], asyncResults[i]);
                    }
                }
                else
                {
                    TestCase[] testCases = LoadSelectTestCases(testSuite.TestCases.ToArray(), true);

                    testCaseResults = RunTestCases(testCases, true);
                }

                foreach (TestCaseResult testCaseResult in testCaseResults)
                {
                    testSuiteResult.TotalTime += testCaseResult.TotalTime;
                    testSuiteResult.TotalRequests += testCaseResult.TotalRequests;

                    if (testCaseResult.ShortestTime < testSuiteResult.ShortestTime)
                    {
                        testSuiteResult.ShortestTime = testCaseResult.ShortestTime;
                    }

                    if (testCaseResult.LongestTime > testSuiteResult.LongestTime)
                    {
                        testSuiteResult.LongestTime = testCaseResult.LongestTime;
                    }
                }

                if (testSuiteResult.TotalRequests > 0)
                {
                    testSuiteResult.AverageTime = testSuiteResult.TotalTime / testSuiteResult.TotalRequests;
                }

                testSuiteResult.EndTime = DateTime.Now;

                testSuiteResult.TestCaseResults = testCaseResults;

                testSuiteResults.Add(testSuiteResult);

                if (!_suppressMessages)
                {
                    Console.WriteLine("Done (" + testSuiteResult.TotalTime + "ms)");
                }
            }

            return testSuiteResults.ToArray();
        }

        #endregion

        #region Clean Members
        public void CleanAll()
        {
            CleanAllTestSuites();
            Console.WriteLine("");
            CleanAllTestCases();
        }

        public void CleanAllTestCases()
        {
            Console.WriteLine(" Cleaning Test Cases:");

            RefreshResources();

            if (TestCaseResources.Count > 0)
            {
                bool cleaned = false;

                foreach (WebDavResource testCaseResource in TestCaseResources)
                {
                    try
                    {
                        if (CleanSingleTestCase(int.Parse(testCaseResource.Name)))
                        {
                            cleaned = true;
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("  There was an issue cleaning test case " + testCaseResource.Name);
                    }
                }

                if (!cleaned)
                {
                    Console.WriteLine("  There were no test cases in need of cleaning");
                }
            }

            Console.WriteLine(" Done");
        }

        public void CleanAllTestSuites()
        {
            Console.WriteLine(" Cleaning Test Suites:");

            RefreshResources();

            if (TestSuiteResources.Count > 0)
            {
                bool cleaned = false;

                foreach (WebDavResource testCaseResource in TestSuiteResources)
                {
                    try
                    {
                        if (CleanSingleTestSuite(int.Parse(testCaseResource.Name)))
                        {
                            cleaned = true;
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("  There was an issue cleaning test suite " + testCaseResource.Name);
                    }
                }

                if (!cleaned)
                {
                    Console.WriteLine("  There were no test suites in need of cleaning");
                }
            }

            Console.WriteLine(" Done");
        }

        public void CleanSelectTestCases(int[] _testCaseIDs)
        {
            Console.WriteLine(" Cleaning Test Cases:");

            RefreshResources();

            if (TestCaseResources.Count > 0)
            {
                bool cleaned = false;
                bool valid = true;

                foreach (int testID in _testCaseIDs)
                {
                    if (!CheckValidTestCase(testID))
                    {
                        valid = false;
                    }
                }

                if (valid)
                {
                    foreach (int testID in _testCaseIDs)
                    {
                        if (CleanSingleTestCase(testID))
                        {
                            cleaned = true;
                        }
                    }

                    if (!cleaned)
                    {
                        Console.WriteLine("  No test cases were cleaned");
                    }
                }
            }

            Console.WriteLine(" Done");
        }

        public void CleanSelectTestSuites(int[] _testSuiteIDs)
        {
            Console.WriteLine(" Cleaning Test Suites:");

            RefreshResources();

            if (TestSuiteResources.Count > 0)
            {
                bool cleaned = false;
                bool valid = true;

                foreach (int testID in _testSuiteIDs)
                {
                    if (!CheckValidTestSuite(testID))
                    {
                        valid = false;
                    }
                }

                if (valid)
                {
                    foreach (int testID in _testSuiteIDs)
                    {
                        if (CleanSingleTestSuite(testID))
                        {
                            cleaned = true;
                        }
                    }

                    if (!cleaned)
                    {
                        Console.WriteLine("  No test suites were cleaned");
                    }
                }
            }

            Console.WriteLine(" Done");
        }

        public bool CleanSingleTestCase(int _testCaseID)
        {
            try
            {
                foreach (WebDavResource testCaseResultResource in TestCaseResultResources)
                {
                    string[] testInformation = testCaseResultResource.Name.ToLower().Split('_');

                    testInformation[2] = testInformation[2].Split('.')[0];

                    int currentTestCaseID = int.Parse(testInformation[0]);
                    Guid currentCaseResultID = Guid.Parse(testInformation[2]);

                    if (_testCaseID == currentTestCaseID)
                    {
                        Console.WriteLine("  " + currentTestCaseID + " (" + currentCaseResultID + ")");

                        if (WebDavManager.Exists2(testCaseResultResource.Uri, ConfigurationManager.AppSettings[AutoTestingConstants.TEST_MANAGEMENT_SERVER]))
                        {
                            WebDavManager.Delete2(testCaseResultResource.Uri, ConfigurationManager.AppSettings[AutoTestingConstants.TEST_MANAGEMENT_SERVER]);
                        }

                        return true;
                    }
                }

                Console.WriteLine("  Test " + _testCaseID + " had nothing to clean");
            }
            catch (Exception)
            {
                Console.WriteLine("  There was an issue cleaning test " + _testCaseID);
            }

            return false;
        }

        public bool CleanSingleTestSuite(int _testSuiteID)
        {
            try
            {
                foreach (WebDavResource testSuiteResultResource in TestSuiteResultResources)
                {
                    string[] testInformation = testSuiteResultResource.Name.ToLower().Split('_');

                    testInformation[2] = testInformation[2].Split('.')[0];

                    int currentTestSuiteID = int.Parse(testInformation[0]);
                    Guid currentTestSuiteResultID = Guid.Parse(testInformation[2]);

                    if (_testSuiteID == currentTestSuiteID)
                    {
                        Console.WriteLine("  " + currentTestSuiteID + " (" + currentTestSuiteResultID + ")");

                        if (WebDavManager.Exists2(testSuiteResultResource.Uri, ConfigurationManager.AppSettings[AutoTestingConstants.TEST_MANAGEMENT_SERVER]))
                        {
                            WebDavManager.Delete2(testSuiteResultResource.Uri, ConfigurationManager.AppSettings[AutoTestingConstants.TEST_MANAGEMENT_SERVER]);
                        }

                        return true;
                    }
                }

                Console.WriteLine("  Test " + _testSuiteID + " had nothing to clean");
            }
            catch (Exception)
            {
                Console.WriteLine("  There was an issue cleaning test " + _testSuiteID);
            }

            return false;
        }

        public bool DeleteTestCase(int testCaseID)
        {
            var filePath = "http://" +
                ConfigurationManager.AppSettings[AutoTestingConstants.TEST_MANAGEMENT_SERVER] +
                AutoTestingConstants.TEST_CASE_SUB_FOLDER + testCaseID;

            return WebDavManager.Delete2(filePath);
        }

        public bool DeleteTestCaseResult(Guid testCaseResultID)
        {
            try
            {
                foreach (WebDavResource testCaseResultResource in TestCaseResultResources)
                {
                    string[] testInformation = testCaseResultResource.Name.ToLower().Split('_');

                    testInformation[2] = testInformation[2].Split('.')[0];

                    int currentTestCaseID = int.Parse(testInformation[0]);
                    Guid currentCaseResultID = Guid.Parse(testInformation[2]);

                    if (testCaseResultID == currentCaseResultID)
                    {
                        Console.WriteLine("  " + currentTestCaseID + " (" + currentCaseResultID + ")");

                        if (WebDavManager.Exists2(testCaseResultResource.Uri, ConfigurationManager.AppSettings[AutoTestingConstants.TEST_MANAGEMENT_SERVER]))
                        {
                            WebDavManager.Delete2(testCaseResultResource.Uri, ConfigurationManager.AppSettings[AutoTestingConstants.TEST_MANAGEMENT_SERVER]);
                        }

                        return true;
                    }
                }

                Debug.WriteLine("  TestCaseResult" + testCaseResultID + " had nothing to clean");
            }
            catch (Exception)
            {
                Console.WriteLine("  There was an issue cleaning TestCaseResult " + testCaseResultID);
            }

            return false;
        }
        #endregion

        #region Create Members
        public bool CreateTestCase(TestCase testCase)
        {
            //create the xml in the temp folder
            var tempPath = Path.Combine(Path.GetTempPath(), "TestCase_" + Guid.NewGuid() + ".xml");
            File.WriteAllText(tempPath, ModelProcessor.GenerateTestCaseXml(testCase));

            //create the folder for the test case
            var folderPath = "http://" + ConfigurationManager.AppSettings[AutoTestingConstants.TEST_MANAGEMENT_SERVER] +
                                          AutoTestingConstants.TEST_CASE_SUB_FOLDER + testCase.ID;
            if (WebDavManager.CreateDirectory2(folderPath))
            {
                //upload the file
                var filePath = folderPath + "/TestCase.xml";
                return WebDavManager.UploadFile2(filePath, tempPath);
            }

            return false;            
        }
        #endregion

        #region Load Members
        public TestCase[] LoadAllTestCases(bool _forceAll = false)
        {
            List<TestCase> tempTestCases = new List<TestCase>();

            int[] ignoredTestCaseIDList = GetAlreadyRunTestCases();

            foreach (WebDavResource testCaseResource in TestCaseResources)
            {
                if (!_forceAll)
                {
                    int tempTestCaseID = int.Parse(testCaseResource.Name);

                    if (ignoredTestCaseIDList.Contains(tempTestCaseID))
                    {
                        Console.WriteLine("  " + testCaseResource.Name + ": Already Run");

                        continue;
                    }
                }

                TestCase testCase = LoadTestCaseFromResource(testCaseResource);

                if (testCase != null)
                {
                    tempTestCases.Add(testCase);
                }
                else
                {
                    Console.WriteLine("  " + testCaseResource.Name + ": Failed to Load");
                }
            }

            return tempTestCases.ToArray();
        }

        public TestSuite[] LoadAllTestSuites(bool _forceAll = false)
        {
            List<TestSuite> tempTestSuites = new List<TestSuite>();

            int[] ignoredTestSuiteIDList = GetAlreadyRunTestSuites();

            foreach (WebDavResource testSuiteResource in TestSuiteResources)
            {
                if (!_forceAll)
                {
                    int tempTestSuiteID = int.Parse(testSuiteResource.Name);

                    if (ignoredTestSuiteIDList.Contains(tempTestSuiteID))
                    {
                        Console.WriteLine("  " + testSuiteResource.Name + ": Already Run");

                        continue;
                    }
                }

                TestSuite testSuite = LoadTestSuiteFromResource(testSuiteResource);

                if (testSuite != null)
                {
                    tempTestSuites.Add(testSuite);
                }
                else
                {
                    Console.WriteLine("  " + testSuiteResource.Name + ": Failed to Load");
                }
            }

            return tempTestSuites.ToArray();
        }

        public TestCase[] LoadSelectTestCases(int[] _testCaseIDs, bool _forceAll = false)
        {
            List<TestCase> tempTestCases = new List<TestCase>();

            if (TestCaseResources.Count > 0)
            {
                bool valid = true;

                foreach (int testCaseID in _testCaseIDs)
                {
                    if (!CheckValidTestCase(testCaseID))
                    {
                        valid = false;
                    }
                }

                if (valid)
                {
                    int[] ignoredTestCaseIDList = GetAlreadyRunTestCases();

                    foreach (int testCaseID in _testCaseIDs)
                    {
                        foreach (WebDavResource testCaseResource in TestCaseResources)
                        {
                            int currentTestCaseResource = int.Parse(testCaseResource.Name);

                            if (testCaseID == currentTestCaseResource)
                            {
                                if (!_forceAll)
                                {
                                    int tempTestCaseID = int.Parse(testCaseResource.Name);

                                    if (ignoredTestCaseIDList.Contains(tempTestCaseID))
                                    {
                                        Console.WriteLine("  " + testCaseResource.Name + ": Already Run");

                                        continue;
                                    }
                                }

                                TestCase testCase = LoadTestCaseFromResource(testCaseResource);

                                if (testCase != null)
                                {
                                    tempTestCases.Add(testCase);
                                }
                                else
                                {
                                    Console.WriteLine("  " + testCaseResource.Name + ": Failed to Load");
                                }
                            }
                        }
                    }
                }
            }

            return tempTestCases.ToArray();
        }

        public TestSuite[] LoadSelectTestSuites(int[] _testSuiteIDs, bool _forceAll = false)
        {
            List<TestSuite> tempTestSuites = new List<TestSuite>();

            if (TestSuiteResources.Count > 0)
            {
                bool valid = true;

                foreach (int testSuiteID in _testSuiteIDs)
                {
                    if (!CheckValidTestSuite(testSuiteID))
                    {
                        valid = false;
                    }
                }

                if (valid)
                {
                    int[] ignoredTestSuiteIDList = GetAlreadyRunTestSuites();

                    foreach (int testSuiteID in _testSuiteIDs)
                    {
                        foreach (WebDavResource testSuiteResource in TestSuiteResources)
                        {
                            int currentTestCaseResource = int.Parse(testSuiteResource.Name);

                            if (testSuiteID == currentTestCaseResource)
                            {
                                if (!_forceAll)
                                {
                                    int tempTestCaseID = int.Parse(testSuiteResource.Name);

                                    if (ignoredTestSuiteIDList.Contains(tempTestCaseID))
                                    {
                                        Console.WriteLine("  " + testSuiteResource.Name + ": Already Run");

                                        continue;
                                    }
                                }

                                TestSuite testSuite = LoadTestSuiteFromResource(testSuiteResource);

                                if (testSuite != null)
                                {
                                    tempTestSuites.Add(testSuite);
                                }
                                else
                                {
                                    Console.WriteLine("  " + testSuiteResource.Name + ": Failed to Load");
                                }
                            }
                        }
                    }
                }
            }

            return tempTestSuites.ToArray();
        }

        public TestCase LoadTestCaseFromResource(WebDavResource _testCaseResource)
        {
            int tempTestCaseID = int.Parse(_testCaseResource.Name);

            if (CheckValidTestCase(tempTestCaseID))
            {
                TestCase testCase = new TestCase();

                testCase.ID = tempTestCaseID;

                List<WebDavResource> testCaseFiles = WebDavManager.List2(_testCaseResource.Uri.AbsoluteUri.Replace(_testCaseResource.Uri.Host, ConfigurationManager.AppSettings[AutoTestingConstants.TEST_MANAGEMENT_SERVER]));

                foreach (WebDavResource testCaseFile in testCaseFiles)
                {
                    if (testCaseFile.IsDirectory || testCaseFile.Name == AutoTestingConstants.WEB_CONFIG)
                    {
                        continue;
                    }

                    switch (testCaseFile.Name.ToLower())
                    {
                        case "testcase.xml":
                            //var temp = Utils.GetStringFromFile(testCaseFile.Uri.AbsoluteUri, ConfigurationManager.AppSettings[AutoTestingConstants.TEST_MANAGEMENT_SERVER]);
                            var strTestCase = WebDavManager.DownloadFile2(testCaseFile.Uri, ConfigurationManager.AppSettings[AutoTestingConstants.TEST_MANAGEMENT_SERVER]);
                            testCase = ModelProcessor.GetTestCaseFromXml(strTestCase);
                            break;
                    }
                }

                return testCase;
            }

            return null;
        }

        public TestCaseResult LoadTestCaseResultFromResource(WebDavResource _testCaseResultResource)
        {
            var strTestCaseResult = WebDavManager.DownloadFile2(_testCaseResultResource.Uri, ConfigurationManager.AppSettings[AutoTestingConstants.TEST_MANAGEMENT_SERVER]);
            var testCaseResult = ModelProcessor.GetTestCaseResultFromFile(_testCaseResultResource.Uri, strTestCaseResult);
            
            return testCaseResult;
        }

        public TestSuite LoadTestSuiteFromResource(WebDavResource _testSuiteResource)
        {
            int tempTestCaseID = int.Parse(_testSuiteResource.Name);

            if (CheckValidTestCase(tempTestCaseID))
            {
                TestSuite testSuite = new TestSuite();
                testSuite.ID = tempTestCaseID;

                List<WebDavResource> testSuiteFiles = WebDavManager.List2(_testSuiteResource.Uri.AbsoluteUri.Replace(_testSuiteResource.Uri.Host, ConfigurationManager.AppSettings[AutoTestingConstants.TEST_MANAGEMENT_SERVER]));

                foreach (WebDavResource testSuiteFile in testSuiteFiles)
                {
                    if (testSuiteFile.IsDirectory || testSuiteFile.Name == AutoTestingConstants.WEB_CONFIG)
                    {
                        continue;
                    }

                    switch (testSuiteFile.Name.ToLower())
                    {
                        case "testsuite.xml":
                            testSuite = ModelProcessor.GetTestSuiteFromFile(testSuiteFile.Uri.AbsoluteUri, ConfigurationManager.AppSettings[AutoTestingConstants.TEST_MANAGEMENT_SERVER]);

                            break;
                    }
                }

                return testSuite;
            }

            return null;
        }

        public TestCaseResult[] LoadAllTestCaseResults()
        {
            List<TestCaseResult> tempTestCaseResults = new List<TestCaseResult>();

            foreach (WebDavResource TestCaseResultResource in TestCaseResultResources)
            {
                TestCaseResult testCaseResult = LoadTestCaseResultFromResource(TestCaseResultResource);

                if (testCaseResult != null)
                {
                    tempTestCaseResults.Add(testCaseResult);
                }
                else
                {
                    Console.WriteLine("  " + TestCaseResultResource.Name + ": Failed to Load");
                }
            }

            return tempTestCaseResults.ToArray();
        }
        #endregion

        #region Upload Members
        public bool UploadTestCaseResults(TestCaseResult[] _testCaseResults)
        {
            if (_testCaseResults.Length > 0)
            {
                //generate the result xml file
                if (!ModelProcessor.GenerateTestCaseResultXML(_testCaseResults))
                    return false;

                foreach (TestCaseResult testCaseResult in _testCaseResults)
                {
                    var filePath = "http://" + ConfigurationManager.AppSettings[AutoTestingConstants.TEST_MANAGEMENT_SERVER] +
                                              AutoTestingConstants.TEST_CASE_RESULT_SUB_FOLDER +
                                              testCaseResult.TestCaseID + "_" + testCaseResult.TesterIP + "_" + testCaseResult.ID + ".xml";

                    WebDavManager.UploadFile2(filePath, testCaseResult.FileLocation);
                }

                return true;
            }
            else
            {
                Console.WriteLine(" Nothing to upload");
            }

            return false;
        }

        public bool UploadTestSuiteResults(TestSuiteResult[] _testSuiteResults)
        {
            if (_testSuiteResults.Length > 0)
            {
                foreach (TestSuiteResult testSuiteResult in _testSuiteResults)
                {
                    WebDavManager.UploadFile2("http://" + ConfigurationManager.AppSettings[AutoTestingConstants.TEST_MANAGEMENT_SERVER] +
                                                AutoTestingConstants.TEST_SUITE_RESULT_SUB_FOLDER +
                                                testSuiteResult.TestSuiteID + "_" + testSuiteResult.TesterIP + "_" + testSuiteResult.ID + ".xml",
                                                testSuiteResult.FileLocation);
                }

                return true;
            }
            else
            {
                Console.WriteLine(" Nothing to upload");
            }

            return false;
        }

        public bool UpdateTestCase(TestCase testCase)
        {
            try
            {
                //create the test case folder if it's not there
                var folderPath = "http://" + ConfigurationManager.AppSettings[AutoTestingConstants.TEST_MANAGEMENT_SERVER] +
                                              AutoTestingConstants.TEST_CASE_SUB_FOLDER + testCase.ID;
                if (!WebDavManager.Exists2(folderPath))
                {
                    if (!WebDavManager.CreateDirectory2(folderPath))
                        throw new ApplicationException("Error updating test case [" + testCase.ID + "]: Can not create directory " + folderPath);
                }

                //create the file that has updated info
                var tempPath = Path.Combine(Path.GetTempPath(), "TestCase_" + Guid.NewGuid() + ".xml");
                File.WriteAllText(tempPath, ModelProcessor.GenerateTestCaseXml(testCase));
                var filePath = folderPath + "/TestCase.xml";

                //delete the existing file
                WebDavManager.Delete2(filePath);

                //upload the file                
                if (WebDavManager.UploadFile2(filePath, tempPath))
                    return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return false;
        }
        #endregion

        #region Exec Members
        public void ExecuteTestCaseFromTestSuite(TestCase _testCase, ref TestCaseResult _testCaseResult)
        {
            TestCase[] testCases = new TestCase[] { _testCase };

            _testCaseResult = RunTestCases(testCases, true)[0];
        }

        public void ExecuteTestCase(TestCase _testCase, ref TestCaseResult _testCaseResult)
        {
            var response = "";

            if (string.IsNullOrWhiteSpace(_testCase.Request.TargetIp))
                _testCase.Request.TargetIp = "127.0.0.1";

            var regex = new Regex(@"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$");
            response = regex.Matches(_testCase.Request.TargetIp).Count > 0
                           ? GetResponse(_testCase, ref _testCaseResult)
                           : GetResponse2(_testCase, ref _testCaseResult);

            if (string.IsNullOrEmpty(response))
            {
                return;
            }

            VerifyResponse(response, _testCase, ref _testCaseResult);
        }
        #endregion

        #endregion

        #region Private Members
        private bool CheckValidTestCase(int _testCaseID)
        {
            foreach (WebDavResource testCaseResource in TestCaseResources)
            {
                try
                {
                    int testCaseID = int.Parse(testCaseResource.Name);

                    if (_testCaseID == testCaseID)
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("  Test case " + testCaseResource.Name + " is not a valid test case ID.");
                }
            }

            Console.WriteLine("  Test " + _testCaseID + " is not a valid tests");

            return false;
        }

        private bool CheckValidTestSuite(int _testSuiteID)
        {
            foreach (WebDavResource testSuiteResource in TestSuiteResources)
            {
                try
                {
                    int testSuiteID = int.Parse(testSuiteResource.Name);

                    if (_testSuiteID == testSuiteID)
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("  Test case " + testSuiteResource.Name + " is not a valid test case ID.");
                }
            }

            Console.WriteLine("  Test " + _testSuiteID + " is not a valid tests");

            return false;
        }

        private int[] GetAlreadyRunTestCases()
        {
            List<int> testCaseIDs = new List<int>();

            foreach (WebDavResource testCaseResultResource in TestCaseResultResources)
            {
                string[] testCaseResult = testCaseResultResource.Name.Split('_');
                int testCaseID = int.Parse(testCaseResult[0]);
                string TesterIP = testCaseResult[1];

                if (LocalIP == TesterIP && !testCaseIDs.Contains(testCaseID))
                {
                    testCaseIDs.Add(testCaseID);
                }
            }

            return testCaseIDs.ToArray();
        }

        private int[] GetAlreadyRunTestSuites()
        {
            List<int> testSuiteIDs = new List<int>();

            foreach (WebDavResource testSuiteResultResource in TestSuiteResultResources)
            {
                string[] testSuiteResult = testSuiteResultResource.Name.Split('_');
                int testSuiteID = int.Parse(testSuiteResult[0]);
                string TesterIP = testSuiteResult[1];

                if (LocalIP == TesterIP && !testSuiteIDs.Contains(testSuiteID))
                {
                    testSuiteIDs.Add(testSuiteID);
                }
            }

            return testSuiteIDs.ToArray();
        }

        private void VerifyResponse(string _response, TestCase _testCase, ref TestCaseResult _testCaseResult)
        {
            string strNewActualResp = "";
            XmlDocument respXml = new XmlDocument();
            respXml.PreserveWhitespace = false;
            ActualResponse actualResponse = null;

            try
            {
                respXml.LoadXml(_response);

                if (respXml.DocumentElement == null)
                {
                    strNewActualResp = _response;
                }
                else
                {
                    strNewActualResp = respXml.OuterXml;
                }
            }
            catch
            {
                strNewActualResp = _response;
            }

            lock (testCaseLock)
            {
                actualResponse = _testCaseResult.ActualResponses.SingleOrDefault(resp => resp.Body == strNewActualResp);
                if (actualResponse == null)
                {
                    // a new actual response
                    actualResponse = new ActualResponse
                    {
                        Body = strNewActualResp,
                        Amount = 1
                    };

                    _testCaseResult.ActualResponses.Add(actualResponse);
                }
                else
                {
                    // an existing actual response
                    actualResponse.Amount++;
                }

                //assert and validate the actual response amount
                var bMatch = false;
                foreach (var expectation in _testCase.Expectations)
                {
                    if (string.IsNullOrEmpty(expectation.MatchText))
                    {
                        continue;
                    }
                    switch (expectation.MatchType)
                    {
                        case MatchOption.ExactMatch:
                            if (actualResponse.Body == expectation.MatchText)
                            {
                                bMatch = true;
                            }
                            break;

                        case MatchOption.PartialMatch:
                            if (actualResponse.Body.Contains(expectation.MatchText))
                            {
                                bMatch = true;
                            }
                            break;

                        case MatchOption.StatusCode:
                            if (actualResponse.StatusCode == expectation.MatchText)
                            {
                                bMatch = true;
                            }
                            break;

                        case MatchOption.RegularExpression:
                            break;
                    }

                    if (bMatch)
                    {
                        actualResponse.IsExpected = true;

                        if (actualResponse.Amount == expectation.Amount)
                        {
                            actualResponse.IsAmountExpected = true;
                        }

                        break;
                    }
                }
            }
        }

        //via tcp socket
        private static string GetResponse(TestCase _testCase, ref TestCaseResult _testResult)
        {
            try
            {
                string l_strPageAddress = "/" +
                    (string.IsNullOrEmpty(_testCase.Request.AppVersion) ? "" : _testCase.Request.AppVersion + "/") +
                    (string.IsNullOrEmpty(_testCase.Request.QueryString) ? "" : _testCase.Request.QueryString);

                if (_testCase.Settings.IsDebugEnabled)
                {
                    l_strPageAddress += (l_strPageAddress.Contains("?")) ? "&" : "?";
                    l_strPageAddress += "debugmode=" + _testCase.Settings.IsDebugEnabled + "&debugcontent=" + HttpUtility.UrlEncode(_testCase.Request.DebugData.Replace("\\t", "").Replace("\\r", "").Replace("\\n", ""));
                }

                IPAddress[] IPs = Dns.GetHostAddresses(_testCase.Request.TargetIp);
                IPAddress ip = null;
                IPEndPoint rip = null;
                Socket s = null;

                if (IPs != null)
                    ip = IPs[0];

                if (ip != null)
                    rip = new IPEndPoint(ip, _testCase.Request.Port);

                try
                {
                    if (rip != null)
                    {
                        int bytesRecv = 0;
                        StringBuilder outputSB = new StringBuilder();
                        byte[] bytes;

                        string Command = _testCase.Request.Verb.ToString().ToUpper() + " " + l_strPageAddress + " HTTP/1.1\r\n";
                        Command += "Accept:*/*\r\n";
                        Command += "Host: " + _testCase.Request.TargetDomain + "\r\n";
                        Command += "Connection: Keep-Alive\r\n";
                        //Command += (_testCase.Settings.ReqType == RequestType.DWNMGR || _testCase.Environment.ReqType == RequestType.UPDMGR || _testCase.Environment.ReqType == RequestType.MIGRATION)
                        //               ? "Content-Type: application/x-www-form-urlencoded\r\n"
                        //               : "Content-Type: text/xml; charset=utf-8\r\n";
                        Command += "Content-Length: " + _testCase.Request.Body.Length + "\r\n";

                        Command += "\r\n";
                        Command += _testCase.Request.Body;

                        var msg = Encoding.Default.GetBytes(Command);
                        s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                        //create a stopwatch to record how long it takes to process this request
                        var stopwatch = Stopwatch.StartNew();

                        s.Connect(rip);
                        var intSend = s.Send(msg, msg.Length, 0);
                        //"Sent " + intSend.ToString() + " bytes to server"

                        do
                        {
                            bytes = new byte[8192];
                            bytesRecv = s.Receive(bytes, bytes.Length, SocketFlags.None);
                            outputSB.Append(Encoding.UTF8.GetString(bytes, 0, bytesRecv));
                            Thread.Sleep(100);
                        } while (s.Connected && s.Available > 0);

                        //time watch
                        long requestTime = stopwatch.ElapsedMilliseconds;

                        _testResult.TotalTime += requestTime;
                        _testResult.TotalRequests++;

                        if (requestTime > _testResult.LongestTime)
                        {
                            _testResult.LongestTime = requestTime;
                        }

                        if (requestTime < _testResult.ShortestTime)
                        {
                            _testResult.ShortestTime = requestTime;
                        }

                        var output = outputSB.ToString();
                        while (output.Contains("HTTP/1.1") && (output.IndexOf("\r\n\r\n") + 4) < output.Length)
                        {
                            output = output.Substring(output.IndexOf("\r\n\r\n") + 4);
                        }

                        if (output.Trim().Length == 0)
                            throw new ApplicationException("No response received for [" + ip + "|" + l_strPageAddress + "].");

                        return output;
                    }
                    else
                        throw new ApplicationException("Can't create IPEndPoint from " + _testCase.Request.TargetIp);
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    if (s != null && s.Connected)
                    {
                        s.Shutdown(SocketShutdown.Both);
                        s.Close();
                    }
                }
            }
            catch (Exception)
            {
                return string.Empty;
                //throw ex;
            }
        }

        //via http client
        private static string GetResponse2(TestCase _testCase, ref TestCaseResult _testResult)
        {
            var strResponseXml = "";
            try
            {
                var client = new WebClient();

                //if (_testCase.Environment.ReqType == RequestType.DWNMGR ||
                //    _testCase.Environment.ReqType == RequestType.UPDMGR ||
                //    _testCase.Environment.ReqType == RequestType.MIGRATION ||
                //    _testCase.Environment.ReqType == RequestType.DAC)
                //    client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                //else
                //    client.Headers.Add("Content-Type", "text/xml");

                client.Headers.Add("Host", _testCase.Request.TargetDomain);
                client.Encoding = Encoding.UTF8;

                var l_strPageAddress = "http://" + _testCase.Request.TargetIp + "/" +
                    (string.IsNullOrEmpty(_testCase.Request.AppVersion) ? "" : _testCase.Request.AppVersion + "/") +
                    (string.IsNullOrEmpty(_testCase.Request.QueryString) ? "" : "?" + _testCase.Request.QueryString + "?reqtype=") +
                    "&debugmode=" + _testCase.Settings.IsDebugEnabled + "&debugcontent=" + HttpUtility.UrlEncode(_testCase.Request.DebugData.Replace("\\t", "").Replace("\\r", "").Replace("\\n", ""));
                //create a stopwatch to record how long it takes to process this request
                var stopwatch = Stopwatch.StartNew();

                //send request
                strResponseXml = client.UploadString(l_strPageAddress, _testCase.Request.Body);

                //time watch
                long requestTime = stopwatch.ElapsedMilliseconds;

                _testResult.TotalTime += requestTime;
                _testResult.TotalRequests++;

                if (requestTime > _testResult.LongestTime)
                {
                    _testResult.LongestTime = requestTime;
                }

                if (requestTime < _testResult.ShortestTime)
                {
                    _testResult.ShortestTime = requestTime;
                }
            }
            catch (Exception ex)
            {
                strResponseXml = ex.Message;
            }

            return strResponseXml;
        }

        public void RefreshResources()
        {
            List<WebDavResource> testCases = WebDavManager.List2("http://" + ConfigurationManager.AppSettings[AutoTestingConstants.TEST_MANAGEMENT_SERVER] +
                                                                   AutoTestingConstants.TEST_CASE_SUB_FOLDER);

            List<WebDavResource> testSuites = WebDavManager.List2("http://" + ConfigurationManager.AppSettings[AutoTestingConstants.TEST_MANAGEMENT_SERVER] +
                                                                   AutoTestingConstants.TEST_SUITE_SUB_FOLDER);

            List<WebDavResource> testCaseResults = WebDavManager.List2("http://" + ConfigurationManager.AppSettings[AutoTestingConstants.TEST_MANAGEMENT_SERVER] +
                                                                   AutoTestingConstants.TEST_CASE_RESULT_SUB_FOLDER);

            List<WebDavResource> testSuiteResults = WebDavManager.List2("http://" + ConfigurationManager.AppSettings[AutoTestingConstants.TEST_MANAGEMENT_SERVER] +
                                                                   AutoTestingConstants.TEST_SUITE_RESULT_SUB_FOLDER);

            int count = 0;

            while (count < testCases.Count)
            {
                if ((testCases[count].IsDirectory && testCases[count].Name.ToLower() == "cases") || testCases[count].Name.ToLower() == AutoTestingConstants.WEB_CONFIG)
                {
                    testCases.RemoveAt(count);
                }
                else
                {
                    count++;
                }
            }

            count = 0;

            while (count < testSuites.Count)
            {
                if ((testSuites[count].IsDirectory && testSuites[count].Name.ToLower() == "suites") || testSuites[count].Name.ToLower() == AutoTestingConstants.WEB_CONFIG)
                {
                    testSuites.RemoveAt(count);
                }
                else
                {
                    count++;
                }
            }

            count = 0;

            while (count < testCaseResults.Count)
            {
                if (testCaseResults[count].IsDirectory || testCaseResults[count].Name.ToLower() == AutoTestingConstants.WEB_CONFIG) // || !testCaseResults[count].Name.Contains(LocalIP))
                {
                    testCaseResults.RemoveAt(count);
                }
                else
                {
                    count++;
                }
            }

            count = 0;

            while (count < testSuiteResults.Count)
            {
                if (testSuiteResults[count].IsDirectory || testSuiteResults[count].Name.ToLower() == AutoTestingConstants.WEB_CONFIG) // || !testSuiteResults[count].Name.Contains(LocalIP))
                {
                    testSuiteResults.RemoveAt(count);
                }
                else
                {
                    count++;
                }
            }

            TestCaseResources = testCases;
            TestSuiteResources = testSuites;
            TestCaseResultResources = testCaseResults;
            TestSuiteResultResources = testSuiteResults;
        }
        #endregion

        public static string GetFormattedXml(string strXml)
        {
            var sbXml = new StringBuilder();
            var strWriter = new StringWriter(sbXml);
            var xmlWriter = new XmlTextWriter(strWriter) { Formatting = Formatting.Indented };

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(strXml.Replace("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">", ""));
                xmlDoc.WriteTo(xmlWriter);
            }
            catch (Exception)
            {
                return strXml;
            }

            return sbXml.ToString();
        }
    }
}
