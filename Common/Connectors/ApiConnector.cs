using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AutoTest.Common
{
    public class ApiConnector
    {
        #region TestCase Loading
        public static int[] ListAllTestCases(bool _forceAll = false)
        {
            var uri = new Uri(AutoTestingConstants.ApiRoot + "/webdav/testcase/list");
            var strResp = Utils.GetBaseResponse(uri, RequestType.GET, null, "");
            strResp = GetJsonFromResponse(strResp);
            var testCaseIDs = Newtonsoft.Json.JsonConvert.DeserializeObject<int[]>(strResp);

            if (testCaseIDs == null || testCaseIDs.Length == 0)
            {
                Debug.WriteLine("Test case (all): Failed to load or no executable test case is available.");
                return null;
            }

            return testCaseIDs;
        }

        public static int[] ListAllTestSuites(bool _forceAll = false)
        {
            var uri = new Uri(AutoTestingConstants.ApiRoot + "/webdav/testsuite/list");
            var strResp = Utils.GetBaseResponse(uri, RequestType.GET, null, "");
            strResp = GetJsonFromResponse(strResp);
            var testSuiteIDs = Newtonsoft.Json.JsonConvert.DeserializeObject<int[]>(strResp);

            if (testSuiteIDs == null || testSuiteIDs.Length == 0)
            {
                Debug.WriteLine("Test suites (all): Failed to load or no executable test suite is available.");
                return null;
            }

            return testSuiteIDs;
        }

        public static TestCase[] LoadAllTestCases(bool _forceAll = false)
        {
            var uri = new Uri(AutoTestingConstants.ApiRoot + "/webdav/testcase/all");
            var strTestCases = Utils.GetBaseResponse(uri, RequestType.GET, null, "");
            strTestCases = GetJsonFromResponse(strTestCases);
            var testCases = ModelProcessor.GetTestCasesFromJson(strTestCases);

            if (testCases == null || testCases.Length == 0)
            {
                Debug.WriteLine("Test case (all): Failed to load or no executable test case is available.");
                return null;
            }

            return testCases;
        }

        public static TestCase[] LoadSelectTestCases(int[] _testCaseIDs, bool _forceAll = false)
        {
            var testCaseIDs = "";
            foreach (var _testCaseID in _testCaseIDs)
            {
                testCaseIDs += _testCaseID + ",";
            }
            testCaseIDs = testCaseIDs.Remove(testCaseIDs.LastIndexOf(","));

            var uri = new Uri(AutoTestingConstants.ApiRoot + "/webdav/testcase/?id=" + testCaseIDs);
            var strTestCases = Utils.GetBaseResponse(uri, RequestType.GET, null, "");
            strTestCases = GetJsonFromResponse(strTestCases);
            var testCases = ModelProcessor.GetTestCasesFromJson(strTestCases);
            if (testCases == null || testCases.Length == 0)
            {
                Debug.WriteLine("Test case (" + testCaseIDs + "): Failed to load");                
            }

            return testCases;
        }

        public static TestSuite[] LoadAllTestSuites(bool _forceAll = false)
        {
            var uri = new Uri(AutoTestingConstants.ApiRoot + "/webdav/testsuite/all");
            var strTestSuites = Utils.GetBaseResponse(uri, RequestType.GET, null, "");
            strTestSuites = GetJsonFromResponse(strTestSuites);
            var testSuites = ModelProcessor.GetTestSuitesFromJson(strTestSuites);

            if (testSuites == null || testSuites.Length == 0)
            {
                Debug.WriteLine("Test Suite (all): Failed to load or no executable test suite is available.");
            }

            return testSuites;
        }

        public static TestSuite[] LoadSelectTestSuites(int[] _testSuiteIDs, bool _forceAll = false)
        {
            var testSuiteIDs = "";
            foreach (var _testSuiteID in _testSuiteIDs)
            {
                testSuiteIDs += _testSuiteID + ",";
            }
            testSuiteIDs = testSuiteIDs.Remove(testSuiteIDs.LastIndexOf(","));

            var uri = new Uri(AutoTestingConstants.ApiRoot + "/webdav/testsuite/?id=" + testSuiteIDs);
            var strTestSuites = Utils.GetBaseResponse(uri, RequestType.GET, null, "");
            strTestSuites = GetJsonFromResponse(strTestSuites);
            var testSuites = ModelProcessor.GetTestSuitesFromJson(strTestSuites);

            if (testSuites == null || testSuites.Length == 0)
            {
                Debug.WriteLine("Test suite (" + testSuiteIDs + "): Failed to load");
            }

            return testSuites;
        }
        #endregion

        #region TestCase Creating
        public static bool CreateTestCase(TestCase testCase)
        {
            //submit the test case to API
            var uri = new Uri(AutoTestingConstants.ApiRoot + "/webdav/testcase");
            var strReqXml = Newtonsoft.Json.JsonConvert.SerializeObject(testCase);
            var strResp = Utils.GetBaseResponse(uri, RequestType.POST, null, strReqXml);
            return GetJsonBoolFromResponse(strResp);
        }

        #endregion

        #region TestCase Editing
        public static bool UpdateTestCase(TestCase testCase)
        {
            //submit the test case to API
            var uri = new Uri(AutoTestingConstants.ApiRoot + "/webdav/testcase/" + testCase.ID);
            var strReqXml = Newtonsoft.Json.JsonConvert.SerializeObject(testCase);
            var strResp = Utils.GetBaseResponse(uri, RequestType.PUT, null, strReqXml);
            
            try
            {
                return GetJsonBoolFromResponse(strResp);
            }
            catch
            {
                throw new ApplicationException(strResp);
            }
        }

        #endregion

        #region TestCase Cleaning
        public static bool CleanSingleTestCase(int _testCaseID)
        {
            try
            {
                var uri = new Uri(AutoTestingConstants.ApiRoot + "/webdav/testcase/?id=" + _testCaseID);
                var strResp = Utils.GetBaseResponse(uri, RequestType.DELETE, null, "");
                return GetJsonBoolFromResponse(strResp);
            }
            catch (Exception)
            {
                Debug.WriteLine("  There was an issue cleaning test case" + _testCaseID);
            }

            return false;
        }

        public static bool CleanSingleTestSuite(int _testSuiteID)
        {
            try
            {
                var uri = new Uri(AutoTestingConstants.ApiRoot + "/webdav/testsuite/?id=" + _testSuiteID);
                var strResp = Utils.GetBaseResponse(uri, RequestType.DELETE, null, "");
                return GetJsonBoolFromResponse(strResp);
            }
            catch (Exception)
            {
                Debug.WriteLine("  There was an issue cleaning test case" + _testSuiteID);
            }

            return false;
        }
        #endregion

        #region Test Case/Result Validation
        public static bool CheckValidTestCase(int _testCaseID)
        {
            var uri = new Uri(AutoTestingConstants.ApiRoot + "/webdav/testcase/validate/?id=" + _testCaseID);
            var strResp = Utils.GetBaseResponse(uri, RequestType.GET, null, "");
            return GetJsonBoolFromResponse(strResp);
        }

        public static bool CheckValidTestSuite(int _testSuiteID)
        {
            var uri = new Uri(AutoTestingConstants.ApiRoot + "/webdav/testsuite/validate/?id=" + _testSuiteID);
            var strResp = Utils.GetBaseResponse(uri, RequestType.GET, null, "");
            return GetJsonBoolFromResponse(strResp);
        }
        #endregion

        #region TestCaseResults loading
        public static TestCaseResult[] LoadAllTestCaseResults(bool _forceAll = false)
        {
            var uri = new Uri(AutoTestingConstants.ApiRoot + "/webdav/testcaseresult/all");
            var strTestCaseResults = Utils.GetBaseResponse(uri, RequestType.GET, null, "");
            strTestCaseResults = GetJsonFromResponse(strTestCaseResults);
            var testCaseResults = ModelProcessor.GetTestCaseResultsFromJson(strTestCaseResults);

            if (testCaseResults == null || testCaseResults.Length == 0)
            {
                Debug.WriteLine("Test case result (all): Failed to load or no test case result is available.");
                return null;
            }

            return testCaseResults;
        }
        #endregion

        #region TestCaseResult cleaning
        public static bool CleanSingleTestCaseResult(Guid testCaseResultID)
        {
            try
            {
                var uri = new Uri(AutoTestingConstants.ApiRoot + "/webdav/testcaseresult/" + testCaseResultID);
                var strResp = Utils.GetBaseResponse(uri, RequestType.DELETE, null, "");
                return GetJsonBoolFromResponse(strResp);
            }
            catch (Exception)
            {
                Debug.WriteLine("  There was an issue cleaning test case result " + testCaseResultID);
            }

            return false;
        }
        #endregion

        #region TestCaseResults Uploading
        //submit the test case result to API
        public static bool UploadTestCaseResults(TestCaseResult[] _testCaseResults)
        {
            if (_testCaseResults != null && _testCaseResults.Length > 0)
            {
                var uri = new Uri(AutoTestingConstants.ApiRoot + "/webdav/testcaseresult/");
                var strReqXml = Newtonsoft.Json.JsonConvert.SerializeObject(_testCaseResults);
                var strResp = Utils.GetBaseResponse(uri, RequestType.POST, null, strReqXml);
                return GetJsonBoolFromResponse(strResp);
            }
            else
                return false;
        }

        #endregion

        #region TestSuiteResults Uploading
        //submit the test suite result to API
        public static bool UploadTestSuiteResults(TestSuiteResult[] _testSuiteResults)
        {
            if (_testSuiteResults != null && _testSuiteResults.Length > 0)
            {
                var uri = new Uri(AutoTestingConstants.ApiRoot + "/webdav/testsuiteresult/");
                var strReqXml = Newtonsoft.Json.JsonConvert.SerializeObject(_testSuiteResults);
                var strResp = Utils.GetBaseResponse(uri, RequestType.POST, null, strReqXml);
                return GetJsonBoolFromResponse(strResp);
            }
            else
                return false;
        }
        #endregion

        private static string GetJsonFromResponse(string strResp)
        {
            var body = strResp.Substring(strResp.IndexOf("\r\n\r\n")).TrimStart();
            string resp = "";

            if (body.IndexOf("[{") > -1)
            {
                resp = body.Substring(body.IndexOf("[{"));
                resp = resp.Substring(0, resp.LastIndexOf("}]") + 2);
            }
            else if (body.IndexOf("[") > -1)
            {
                resp = body.Substring(body.IndexOf("["));
                resp = resp.Substring(0, resp.LastIndexOf("]") + 1);
            }
            
            return resp;
        }

        private static bool GetJsonBoolFromResponse(string strResp)
        {
            var body = strResp.Substring(strResp.IndexOf("\r\n\r\n")).TrimStart().ToLower();
            if (body.Contains("true"))
                return true;
            else
                return false;
        }
    }
}
