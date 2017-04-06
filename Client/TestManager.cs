using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AutoTest.Common;
using System.Net;
using System.Configuration;
using System.Net.Sockets;
using System.Xml;
using System.Threading;
using System.Xml.Linq;
using System.IO;
using System.Web;

namespace AutoTestClient
{
    internal static class TestManager
    {
        private static string LocalIP;

        private static object testCaseLock = new object();

        private delegate void AsyncDelegate(TestCase _testCase, ref TestCaseResult _testCaseResult);

        private static string WEB_CONFIG = "web.config";

        #region Public Members

        public static void Initialize()
        {
            LocalIP = GetLocalIP();
        }

        #region List Members

        public static void ListAll()
        {
            ListTestCases();
            Console.WriteLine("");
            ListTestSuites();
        }

        public static void ListTestCases()
        {
            Console.WriteLine(" Test Cases:");

            var testCaseIDs = new int[] { 1, 2, 3 };

            if (testCaseIDs != null && testCaseIDs.Length > 0)
            {
                foreach (var testCaseID in testCaseIDs)
                {
                    Console.WriteLine("  " + testCaseID);
                }
            }
            else
            {
                Console.WriteLine("  There are no executable tests cases available");
            }
        }

        public static void ListTestSuites()
        {
            Console.WriteLine(" Test Suites:");

            var testSuiteIDs = new int[] { 1, 2, 3 };

            if (testSuiteIDs != null && testSuiteIDs.Length > 0)
            {
                foreach (var testSuiteID in testSuiteIDs)
                {
                    Console.WriteLine("  " + testSuiteIDs);
                }
            }
            else
            {
                Console.WriteLine("  There are no tests suites available");
            }
        }

        #endregion

        #region Run Members

        public static void RunAll()
        {
            RunAllTestSuites();
            Console.WriteLine("");
            RunAllTestCases();
        }

        public static void RunAllTestCases()
        {

            Console.WriteLine(" Loading Test Cases");

            TestCase[] testCases = LoadAllTestCases();

            if (testCases.Length > 0)
            {
                Console.WriteLine(" Running Test Cases");

                TestCaseResult[] testCaseResults = RunTestCases(testCases);

                if (testCaseResults.Length > 0)
                {
                    Console.WriteLine(" Generating Results");

                    //if (ModelProcessor.GenerateTestCaseResultXML(testCaseResults))
                    //{
                        Console.WriteLine(" Uploading Results");

                        if (!UploadTestCaseResults(testCaseResults))
                        {
                            Console.WriteLine("Failed to upload generated test case result data.");
                        }
                    //}
                }
            }
        }

        public static void RunAllTestSuites()
        {
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

        public static void RunSelectTestCases(int[] _testCaseIDs)
        {
            Console.WriteLine(" Loading Test Cases");

            TestCase[] TestCases = LoadSelectTestCases(_testCaseIDs);

            if (TestCases.Length > 0)
            {
                Console.WriteLine(" Running Test Cases");

                TestCaseResult[] TestCaseResults = RunTestCases(TestCases);

                if (TestCaseResults.Length > 0)
                {
                    Console.WriteLine(" Generating Results");

                    //if (ModelProcessor.GenerateTestCaseResultXML(TestCaseResults))
                    //{
                        Console.WriteLine(" Uploading Results");

                        if (!UploadTestCaseResults(TestCaseResults))
                        {
                            Console.WriteLine("Failed to upload generated test case result data.");
                        }
                    //}
                }
            }
        }

        public static void RunSelectTestSuites(int[] _testSuiteIDs)
        {
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

        public static void RunSelectTestCasesForTMS(int[] _testCaseIDs)
        {
            TestCase[] TestCases = LoadSelectTestCases(_testCaseIDs);

            if (TestCases.Length > 0)
            {
                TestCaseResult[] TestCaseResults = RunTestCases(TestCases, true);

                if (TestCaseResults.Length > 0)
                {
                    if (ModelProcessor.GenerateTestCaseResultXMLForTMS(TestCaseResults))
                    {
                        Console.WriteLine(Utils.GetFormattedXml(ModelProcessor.GetTestCaseResultXMLForTMS(TestCaseResults)));
                        return;
                    }
                }
            }

            Console.WriteLine("<results></results>");
        }

        public static void RunSelectTestSuitesForTMS(int[] _testSuiteIDs)
        {
            TestSuite[] TestSuites = LoadSelectTestSuites(_testSuiteIDs);

            if (TestSuites.Length > 0)
            {
                TestSuiteResult[] TestSuiteResults = RunTestSuites(TestSuites, true);

                if (TestSuiteResults.Length > 0)
                {
                    if (ModelProcessor.GenerateTestSuiteResultXMLForTMS(TestSuiteResults))
                    {
                        Console.WriteLine(Utils.GetFormattedXml(ModelProcessor.GetTestSuiteResultXMLForTMS(TestSuiteResults)));
                        return;
                    }
                }
            }

            Console.WriteLine("<results></results>");
        }

        #endregion

        #region Clean Members

        public static void CleanAll()
        {
            CleanAllTestSuites();
            Console.WriteLine("");
            CleanAllTestCases();
        }

        public static void CleanAllTestCases()
        {
            Console.WriteLine(" Cleaning Test Cases:");

            var testCaseIDs = ApiConnector.ListAllTestCases();
            if (testCaseIDs == null)
            {
                Console.WriteLine("  There was an issue loading test case IDs");
                return;
            }

            bool cleaned = false;

            foreach (var testCaseID in testCaseIDs)
            {
                try
                {
                    if (CleanSingleTestCase(testCaseID))
                    {
                        cleaned = true;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("  There was an issue cleaning test case " + testCaseID);
                }
            }

            if (!cleaned)
            {
                Console.WriteLine("  There were no test cases in need of cleaning");
            }

            Console.WriteLine(" Done");
        }

        public static void CleanAllTestSuites()
        {
            Console.WriteLine(" Cleaning Test Suites:");

            var testSuiteIDs = ApiConnector.ListAllTestSuites();
            if (testSuiteIDs == null)
            {
                Console.WriteLine("  There was an issue loading test case IDs");
                return;
            }

            bool cleaned = false;

            foreach (var testSuiteID in testSuiteIDs)
            {
                try
                {
                    if (CleanSingleTestSuite(testSuiteID))
                    {
                        cleaned = true;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("  There was an issue cleaning test suite " + testSuiteID);
                }
            }

            if (!cleaned)
            {
                Console.WriteLine("  There were no test suites in need of cleaning");
            }

            Console.WriteLine(" Done");
        }

        public static void CleanSelectTestCases(int[] _testCaseIDs)
        {
            Console.WriteLine(" Cleaning Test Cases:");

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

            Console.WriteLine(" Done");
        }

        public static void CleanSelectTestSuites(int[] _testSuiteIDs)
        {
            Console.WriteLine(" Cleaning Test Suites:");

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

            Console.WriteLine(" Done");
        }

        #endregion

        #endregion

        #region Private Members

        private static bool CleanSingleTestCase(int _testCaseID)
        {
            try
            {
                var res = ApiConnector.CleanSingleTestCase(_testCaseID);
                
                if (res)
                {
                    Console.WriteLine("  Test Case " + _testCaseID + " deleted");
                    return true;
                }

                Console.WriteLine("  Test " + _testCaseID + " had nothing to clean");
            }
            catch (Exception)
            {
                Console.WriteLine("  There was an issue cleaning test case" + _testCaseID);
            }

            return false;
        }

        private static bool CleanSingleTestSuite(int _testSuiteID)
        {
            try
            {
                var res = ApiConnector.CleanSingleTestSuite(_testSuiteID);

                if (res)
                {
                    Console.WriteLine("  Test Suite " + _testSuiteID + " deleted");
                    return true;
                }

                Console.WriteLine("  Test Suite " + _testSuiteID + " had nothing to clean");
            }
            catch (Exception)
            {
                Console.WriteLine("  There was an issue cleaning test " + _testSuiteID);
            }

            return false;
        }

        private static bool CheckValidTestCase(int _testCaseID)
        {
            var res = ApiConnector.CheckValidTestCase(_testCaseID);            
            if (res)
                return true;

            Console.WriteLine("  Test case " + _testCaseID + " is not a valid test case");

            return false;
        }

        private static bool CheckValidTestSuite(int _testSuiteID)
        {
            var res = ApiConnector.CheckValidTestSuite(_testSuiteID);
            if (res)
                return true;

            Console.WriteLine("  Test Suite " + _testSuiteID + " is not a valid test suite");

            return false;
        }

        private static TestCase[] LoadAllTestCases(bool _forceAll = false)
        {
            var testCases = ApiConnector.LoadAllTestCases();
            if (testCases == null || testCases.Length == 0)
            {
                Console.WriteLine("Test case (all): Failed to load or no executable test case is available.");
            }

            return testCases;
        }

        private static TestSuite[] LoadAllTestSuites(bool _forceAll = false)
        {
            var testSuites = ApiConnector.LoadAllTestSuites();
            if (testSuites == null || testSuites.Length == 0)
            {
                Console.WriteLine("Test Suite (all): Failed to load or no executable test suite is available.");
            }

            return testSuites;
        }

        private static TestCase[] LoadSelectTestCases(int[] _testCaseIDs, bool _forceAll = false)
        {            
            var testCases = ApiConnector.LoadSelectTestCases(_testCaseIDs);
            if (testCases == null || testCases.Length == 0)
            {
                Console.WriteLine("Test case (" + _testCaseIDs + "): Failed to load");                
            }

            return testCases;
        }

        private static TestSuite[] LoadSelectTestSuites(int[] _testSuiteIDs, bool _forceAll = false)
        {            
            var testSuites = ApiConnector.LoadSelectTestSuites(_testSuiteIDs);

            if (testSuites == null || testSuites.Length == 0)
            {
                Console.WriteLine("Test suite (" + _testSuiteIDs + "): Failed to load");
            }

            return testSuites;
        }
        
        private static TestCaseResult[] RunTestCases(TestCase[] _testCases, bool _suppressMessages = false)
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

        private static TestSuiteResult[] RunTestSuites(TestSuite[] _testSuites, bool _suppressMessages = false)
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
        
        private static bool UploadTestCaseResults(TestCaseResult[] _testCaseResults)
        {
            if (_testCaseResults.Length > 0)
            {
                return ApiConnector.UploadTestCaseResults(_testCaseResults);
            }
            else
            {
                Console.WriteLine(" Nothing to upload");
            }

            return false;
        }

        private static bool UploadTestSuiteResults(TestSuiteResult[] _testSuiteResults)
        {
            if (_testSuiteResults.Length > 0)
            {
                return ApiConnector.UploadTestSuiteResults(_testSuiteResults);
            }
            else
            {
                Console.WriteLine(" Nothing to upload");
            }

            return false;
        }

        private static void ExecuteTestCaseFromTestSuite(TestCase _testCase, ref TestCaseResult _testCaseResult)
        {
            TestCase[] testCases = new TestCase[] { _testCase };

            _testCaseResult = RunTestCases(testCases, true)[0];
        }

        private static void ExecuteTestCase(TestCase _testCase, ref TestCaseResult _testCaseResult)
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

        private static void VerifyResponse(string _response, TestCase _testCase, ref TestCaseResult _testCaseResult)
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
                    l_strPageAddress += (l_strPageAddress.Contains("?")) ? "&": "?";
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

        //private static void RefreshResources()
        //{
        //    List<WebDavResource> testCases = WebDavManager.List2("http://" + ConfigurationManager.AppSettings[TEST_MANAGEMENT_SERVER] +
        //                                                           AutoTestingConstants.TEST_CASE_SUB_FOLDER);

        //    List<WebDavResource> testSuites = WebDavManager.List2("http://" + ConfigurationManager.AppSettings[TEST_MANAGEMENT_SERVER] +
        //                                                           AutoTestingConstants.TEST_SUITE_SUB_FOLDER);

        //    List<WebDavResource> testCaseResults = WebDavManager.List2("http://" + ConfigurationManager.AppSettings[TEST_MANAGEMENT_SERVER] +
        //                                                           AutoTestingConstants.TEST_CASE_RESULT_SUB_FOLDER);

        //    List<WebDavResource> testSuiteResults = WebDavManager.List2("http://" + ConfigurationManager.AppSettings[TEST_MANAGEMENT_SERVER] +
        //                                                           AutoTestingConstants.TEST_SUITE_RESULT_SUB_FOLDER);

        //    int count = 0;

        //    while (count < testCases.Count)
        //    {
        //        if ((testCases[count].IsDirectory && testCases[count].Name.ToLower() == "cases") || testCases[count].Name.ToLower() == WEB_CONFIG)
        //        {
        //            testCases.RemoveAt(count);
        //        }
        //        else
        //        {
        //            count++;
        //        }
        //    }

        //    count = 0;

        //    while (count < testSuites.Count)
        //    {
        //        if ((testSuites[count].IsDirectory && testSuites[count].Name.ToLower() == "suites") || testSuites[count].Name.ToLower() == WEB_CONFIG)
        //        {
        //            testSuites.RemoveAt(count);
        //        }
        //        else
        //        {
        //            count++;
        //        }
        //    }

        //    count = 0;

        //    while (count < testCaseResults.Count)
        //    {
        //        if (testCaseResults[count].IsDirectory || testCaseResults[count].Name.ToLower() == WEB_CONFIG || !testCaseResults[count].Name.Contains(LocalIP))
        //        {
        //            testCaseResults.RemoveAt(count);
        //        }
        //        else
        //        {
        //            count++;
        //        }
        //    }

        //    count = 0;

        //    while (count < testSuiteResults.Count)
        //    {
        //        if (testSuiteResults[count].IsDirectory || testSuiteResults[count].Name.ToLower() == WEB_CONFIG || !testSuiteResults[count].Name.Contains(LocalIP))
        //        {
        //            testSuiteResults.RemoveAt(count);
        //        }
        //        else
        //        {
        //            count++;
        //        }
        //    }

        //    TestCaseResources = testCases;
        //    TestSuiteResources = testSuites;
        //    TestCaseResultResources = testCaseResults;
        //    TestSuiteResultResources = testSuiteResults;
        //}

        private static string GetLocalIP()
        {
            IPAddress[] ipAddresses = Dns.GetHostAddresses(Dns.GetHostName());

            foreach (IPAddress ipAddress in ipAddresses)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork && !ipAddress.IsIPv6LinkLocal)
                {
                    return ipAddress.ToString();
                }
            }

            return "";
        }
                
        #endregion
    }
}
