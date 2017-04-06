using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace AutoTest.Common
{
    public class ModelProcessor
    {
        #region Generate TestCase XML
        private static string GenerateEnvXml(ExecutionSettings env)
        {
            var oSB = new StringBuilder();
            oSB.AppendLine("<settings>");
            oSB.AppendLine("<mode>" + env.Mode + "</mode>");
            oSB.AppendLine("<isdebug>" + env.IsDebugEnabled + "</isdebug>");
            oSB.AppendLine("<interval>" + env.Interval + "</interval>");
            oSB.AppendLine("<amount>" + env.Amount + "</amount>");
            oSB.AppendLine("<mixedmodeloops>" + env.LoopsOfMixedModeRunning + "</mixedmodeloops>");
            oSB.AppendLine("<resetservercachefirst>" + env.ResetServerCacheFirst + "</resetservercachefirst>");
            oSB.AppendLine("</settings>");

            return oSB.ToString();
        }

        private static string GenerateReqXml(TestRequest req)
        {
            var oSB = new StringBuilder();
            oSB.AppendLine("<request>");
            oSB.AppendLine("<appversion>" + req.AppVersion + "</appversion>");
            oSB.AppendLine("<body>" + req.Body + "</body>");
            oSB.AppendLine("<customheaders>");
            foreach (var customHeader in req.CustomHeaders)
            {
                oSB.AppendLine("<customheader>");
                oSB.AppendLine("<name>" + customHeader.Key + "</name>");
                oSB.AppendLine("<value>" + customHeader.Value + "</value>");
                oSB.AppendLine("</customheader>");
            }
            oSB.AppendLine("</customheaders>");
            oSB.AppendLine("<isssl>" + req.IsSSL + "</isssl>");
            oSB.AppendLine("<port>" + req.Port + "</port>");
            oSB.AppendLine("<querystring>" + req.QueryString + "</querystring>");
            oSB.AppendLine("<domain>" + req.TargetDomain + "</domain>");
            oSB.AppendLine("<ip>" + req.TargetIp + "</ip>");
            oSB.AppendLine("<verb>" + req.Verb + "</verb>");
            oSB.AppendLine("<debugdata>" + req.DebugData + "</debugdata>");
            oSB.AppendLine("</request>");

            return oSB.ToString();
        }

        private static string GenerateRespXml(List<ExpectedResponse> respList)
        {
            var oSB = new StringBuilder();
            oSB.AppendLine("<expectations>");

            foreach (var resp in respList)
            {
                oSB.AppendLine("<response>");
                oSB.AppendLine("<text>" + resp.MatchText + "</text>");
                oSB.AppendLine("<type>" + resp.MatchType + "</type>");
                oSB.AppendLine("<amount>" + resp.Amount + "</amount>");
                oSB.AppendLine("</response>");
            }

            oSB.AppendLine("</expectations>");
            return oSB.ToString();
        }

        public static string GenerateTestCaseXml(TestCase testcase)
        {
            var oSB = new StringBuilder();
            oSB.AppendLine("<testcase>");
            oSB.AppendLine("<id>" + testcase.ID + "</id>");
            oSB.AppendLine("<name>" + testcase.Name + "</name>");
            oSB.AppendLine("<description>" + testcase.Description + "</description>");
            oSB.AppendLine(GenerateEnvXml(testcase.Settings));
            oSB.AppendLine(GenerateReqXml(testcase.Request));
            oSB.AppendLine(GenerateRespXml(testcase.Expectations));
            oSB.AppendLine("</testcase>");

            return oSB.ToString();
        }
        #endregion

        #region Generate TestSuite XML
        public static string GenerateTestSuiteXml(TestSuite testSuite)
        {
            var oSB = new StringBuilder();
            oSB.AppendLine("<testsuite>");
            oSB.AppendLine("<id>" + testSuite.ID + "</id>");
            oSB.AppendLine("<name>" + testSuite.Name + "</name>");
            oSB.AppendLine("<description>" + testSuite.Description + "</description>");
            oSB.AppendLine(GenerateSuiteEnvXml(testSuite.Settings));
            oSB.AppendLine(GenerateTestCaseList(testSuite.TestCases));
            oSB.AppendLine("</testsuite>");

            return oSB.ToString();
        }

        private static string GenerateSuiteEnvXml(SuiteExecutionSettings env)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("<settings>");
            stringBuilder.AppendLine("<executionmode>" + env.ExecutionMode + "</executionmode>");
            stringBuilder.AppendLine("</settings>");

            return stringBuilder.ToString();
        }
        #endregion

        #region Generate TestCaseResult XML
        public static bool GenerateTestCaseResultXML(TestCaseResult[] _testCaseResults)
        {
            if (_testCaseResults.Length > 0)
            {
                foreach (var testCaseResult in _testCaseResults)
                {
                    //write result into a file
                    var xResult = XElement.Parse("<result></result>");

                    var xStart = new XElement("timestamp_start");
                    xStart.Value = testCaseResult.StartTime.ToString();
                    xResult.Add(xStart);

                    var xEnd = new XElement("timestamp_end");
                    xEnd.Value = testCaseResult.EndTime.ToString();
                    xResult.Add(xEnd);

                    var xTotal = new XElement("total_time");
                    xTotal.Value = testCaseResult.TotalTime.ToString();
                    xResult.Add(xTotal);

                    var xAverage = new XElement("average_time");
                    xAverage.Value = testCaseResult.AverageTime.ToString();
                    xResult.Add(xAverage);

                    var xLongest = new XElement("longest_time");
                    xLongest.Value = testCaseResult.LongestTime.ToString();
                    xResult.Add(xLongest);

                    var xShortest = new XElement("shortest_time");
                    xShortest.Value = testCaseResult.ShortestTime.ToString();
                    xResult.Add(xShortest);

                    var xActualResponses = new XElement("actualresponses");
                    xResult.Add(xActualResponses);

                    foreach (var actualResponse in testCaseResult.ActualResponses)
                    {
                        var xResponse = new XElement("response");
                        xActualResponses.Add(xResponse);

                        var xBody = new XElement("body");
                        xBody.Value = actualResponse.Body;
                        xResponse.Add(xBody);

                        var xStatuscode = new XElement("statuscode");
                        xStatuscode.Value = actualResponse.StatusCode == null ? "" : actualResponse.StatusCode;
                        xResponse.Add(xStatuscode);

                        var xExpected = new XElement("isexpected");
                        xExpected.Value = actualResponse.IsExpected.ToString();
                        xResponse.Add(xExpected);

                        var xAmountExpected = new XElement("isamountexpected");
                        xAmountExpected.Value = actualResponse.IsAmountExpected.ToString();
                        xResponse.Add(xAmountExpected);

                        var xHeaders = new XElement("headers");
                        xHeaders.Value = actualResponse.Headers == null ? "" : actualResponse.Headers;
                        xResponse.Add(xHeaders);

                        var xAmount = new XElement("amount");
                        xAmount.Value = actualResponse.Amount.ToString();
                        xResponse.Add(xAmount);
                    }

                    testCaseResult.FileLocation = Path.GetTempPath() + testCaseResult.TestCaseID + "_" + testCaseResult.TesterIP + "_" + testCaseResult.ID + ".xml";

                    File.WriteAllText(testCaseResult.FileLocation, xResult.ToString());
                }

                return true;
            }
            else
            {
                Console.WriteLine(" Nothing to generate");
            }

            return false;
        }

        public static bool GenerateTestCaseResultXMLForTMS(TestCaseResult[] _testCaseResults, int testSuiteId = -1)
        {
            if (_testCaseResults.Length > 0)
            {
                foreach (var testCaseResult in _testCaseResults)
                {
                    //write result into a file
                    var xResult = XElement.Parse("<testcase id=\"" + testCaseResult.TestCaseID + "\"" + (testSuiteId == -1 ? "" : " testsuiteid=\"" + testSuiteId + "\"") + "></testcase>");

                    var xStart = new XElement("timestamp_start");
                    xStart.Value = testCaseResult.StartTime.ToString();
                    xResult.Add(xStart);

                    var xEnd = new XElement("timestamp_end");
                    xEnd.Value = testCaseResult.EndTime.ToString();
                    xResult.Add(xEnd);

                    var xTotal = new XElement("total_time");
                    xTotal.Value = testCaseResult.TotalTime.ToString();
                    xResult.Add(xTotal);

                    var xAverage = new XElement("average_time");
                    xAverage.Value = testCaseResult.AverageTime.ToString();
                    xResult.Add(xAverage);

                    var xLongest = new XElement("longest_time");
                    xLongest.Value = testCaseResult.LongestTime.ToString();
                    xResult.Add(xLongest);

                    var xShortest = new XElement("shortest_time");
                    xShortest.Value = testCaseResult.ShortestTime.ToString();
                    xResult.Add(xShortest);

                    var xActualResponses = new XElement("actualresponses");
                    xResult.Add(xActualResponses);

                    foreach (var actualResponse in testCaseResult.ActualResponses)
                    {
                        var xResponse = new XElement("response");
                        xActualResponses.Add(xResponse);

                        var xBody = new XElement("body");
                        xBody.Value = actualResponse.Body;
                        xResponse.Add(xBody);

                        var xStatuscode = new XElement("statuscode");
                        xStatuscode.Value = actualResponse.StatusCode;
                        xResponse.Add(xStatuscode);

                        var xExpected = new XElement("isexpected");
                        xExpected.Value = actualResponse.IsExpected.ToString();
                        xResponse.Add(xExpected);

                        var xAmountExpected = new XElement("isamountexpected");
                        xAmountExpected.Value = actualResponse.IsAmountExpected.ToString();
                        xResponse.Add(xAmountExpected);

                        var xHeaders = new XElement("headers");
                        xHeaders.Value = actualResponse.Headers;
                        xResponse.Add(xHeaders);

                        var xAmount = new XElement("amount");
                        xAmount.Value = actualResponse.Amount.ToString();
                        xResponse.Add(xAmount);
                    }

                    testCaseResult.FileLocation = Path.GetTempPath() + testCaseResult.TestCaseID + "_" + testCaseResult.TesterIP + "_" + testCaseResult.ID + ".xml";

                    File.WriteAllText(testCaseResult.FileLocation, xResult.ToString());
                }

                return true;
            }

            return false;
        }

        public static string GetTestCaseResultXMLForTMS(TestCaseResult[] _testCaseResults)
        {
            var xResults = "<results>";
            if (_testCaseResults.Length > 0)
            {
                foreach (var testCaseResult in _testCaseResults)
                {
                    var xResult = File.ReadAllText(testCaseResult.FileLocation);
                    xResults += xResult;
                }
            }
            xResults += "</results>";

            return xResults;
        }
        #endregion

        #region Generate TestSuiteResult XML
        public static bool GenerateTestSuiteResultXML(TestSuiteResult[] _testSuiteResults)
        {
            if (_testSuiteResults.Length > 0)
            {
                foreach (var testSuiteResult in _testSuiteResults)
                {
                    //write result into a file
                    var xResult = XElement.Parse("<results></results>");

                    var xStart = new XElement("timestamp_start");
                    xStart.Value = testSuiteResult.StartTime.ToString();
                    xResult.Add(xStart);

                    var xEnd = new XElement("timestamp_end");
                    xEnd.Value = testSuiteResult.EndTime.ToString();
                    xResult.Add(xEnd);

                    var xTotal = new XElement("total_time");
                    xTotal.Value = testSuiteResult.TotalTime.ToString();
                    xResult.Add(xTotal);

                    var xAverage = new XElement("average_time");
                    xAverage.Value = testSuiteResult.AverageTime.ToString();
                    xResult.Add(xAverage);

                    var xLongest = new XElement("longest_time");
                    xLongest.Value = testSuiteResult.LongestTime.ToString();
                    xResult.Add(xLongest);

                    var xShortest = new XElement("shortest_time");
                    xShortest.Value = testSuiteResult.ShortestTime.ToString();
                    xResult.Add(xShortest);

                    testSuiteResult.FileLocation = Path.GetTempPath() + testSuiteResult.TestSuiteID + "_" + testSuiteResult.TesterIP + "_" + testSuiteResult.ID + ".xml";

                    File.WriteAllText(testSuiteResult.FileLocation, xResult.ToString());
                }

                return true;
            }
            else
            {
                Console.WriteLine(" Nothing to generate");
            }

            return false;
        }

        public static bool GenerateTestSuiteResultXMLForTMS(TestSuiteResult[] _testSuiteResults)
        {
            bool res = true;
            if (_testSuiteResults.Length > 0)
            {
                foreach (var testSuiteResult in _testSuiteResults)
                {
                    res = res && GenerateTestCaseResultXMLForTMS(testSuiteResult.TestCaseResults, testSuiteResult.TestSuiteID);
                }
            }
            else
            {
                res = false;
            }

            return res;
        }

        public static string GetTestSuiteResultXMLForTMS(TestSuiteResult[] _testSuiteResults)
        {
            var xResults = "<results>";
            if (_testSuiteResults.Length > 0)
            {
                foreach (var testSuiteResult in _testSuiteResults)
                {
                    foreach (var testCaseResult in testSuiteResult.TestCaseResults)
                    {
                        var xResult = File.ReadAllText(testCaseResult.FileLocation);
                        xResults += xResult;
                    }
                }
            }
            xResults += "</results>";

            return xResults;
        }
        #endregion

        #region Get TestCase from file
        public static TestCase GetTestCaseFromFile(string testCaseFile, string tmServer = null)
        {
            return GetTestCaseFromXml(Utils.GetStringFromFile(testCaseFile, tmServer));
        }

        public static TestCase GetTestCaseFromXml(string testCaseContent)
        {
            var xTestCase = XElement.Parse(testCaseContent);
            if (xTestCase == null)
                return null;

            var xEnv = xTestCase.Element("settings");
            var xReq = xTestCase.Element("request");
            var xResp = xTestCase.Element("response");

            var xHeaders = xTestCase.Element("request").Element("customheaders").Elements("customheader");
            var customHeaders = new Dictionary<string, string>();
            if (xHeaders != null)
            {
                foreach (var xHeader in xHeaders)
                {
                    customHeaders.Add(xHeader.Element("name").Value, xHeader.Element("value").Value);
                }
            }

            var xExpectations = xTestCase.Element("expectations").Elements("response");
            var expectedResponses = new List<ExpectedResponse>();
            if (xExpectations != null)
            {
                foreach (var xExpectation in xExpectations)
                {
                    var expectedResponse = new ExpectedResponse
                    {
                        MatchText = xExpectation.Element("text").Value,
                        MatchType = (MatchOption)Enum.Parse(typeof(MatchOption), xExpectation.Element("type").Value, true),
                        Amount = int.Parse(xExpectation.Element("amount").Value)
                    };

                    expectedResponses.Add(expectedResponse);
                }
            }

            var testCase = new TestCase
            {
                ID = int.Parse(xTestCase.Element("id").Value),
                Name = xTestCase.Element("name").Value,
                Description = xTestCase.Element("description").Value,
                Settings = new ExecutionSettings
                {
                    Mode = (ExecutionMode)Enum.Parse(typeof(ExecutionMode), xEnv.Element("mode").Value, true),
                    IsDebugEnabled = xEnv.Element("isdebug") == null ? false : bool.Parse(xEnv.Element("isdebug").Value),
                    Interval = int.Parse(xEnv.Element("interval").Value),
                    Amount = int.Parse(xEnv.Element("amount").Value),
                    LoopsOfMixedModeRunning = int.Parse(xEnv.Element("mixedmodeloops").Value),
                    ResetServerCacheFirst = xEnv.Element("resetservercachefirst") == null ? false : bool.Parse(xEnv.Element("resetservercachefirst").Value),
                },

                Request = new TestRequest
                {
                    AppVersion = xReq.Element("appversion").Value,
                    Body = xReq.Element("body").Value,
                    CustomHeaders = customHeaders,
                    IsSSL = xReq.Element("isssl") == null ? false : bool.Parse(xReq.Element("isssl").Value),
                    Port = int.Parse(xReq.Element("port").Value),
                    QueryString = xReq.Element("querystring").Value,
                    TargetDomain = xReq.Element("domain").Value,
                    TargetIp = xReq.Element("ip").Value,
                    Verb = (RequestType)Enum.Parse(typeof(RequestType), xReq.Element("verb").Value, true),
                    DebugData = xReq.Element("debugdata").Value,
                },

                Expectations = expectedResponses
            };

            return testCase;
        }

        public static TestCase[] GetTestCasesFromJson(string testCasesContent)
        {
            var testCases = Newtonsoft.Json.JsonConvert.DeserializeObject<TestCase[]>(testCasesContent);
            return testCases;
        }
        #endregion

        #region Get TestSuite from file
        public static TestSuite GetTestSuiteFromFile(string testSuiteFile, string tmServer = null)
        {
            return GetTestSuiteFromXml(Utils.GetStringFromFile(testSuiteFile, tmServer));            
        }

        public static TestSuite GetTestSuiteFromXml(string testSuiteContent)
        {
            var xTestSuite = XElement.Parse(testSuiteContent);
            var xEnv = xTestSuite.Element("settings");
            var strTestCaseIDs = xTestSuite.Element("testcases").Value;

            var testCaseList = new List<int>();
            var testCaseIDs = strTestCaseIDs.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            foreach (var testCaseID in testCaseIDs)
            {
                testCaseList.Add(int.Parse(testCaseID.Trim()));
            }


            var testSuite = new TestSuite
            {
                ID = int.Parse(xTestSuite.Element("id").Value),
                Name = xTestSuite.Element("name").Value,
                Description = xTestSuite.Element("description").Value,

                Settings = new SuiteExecutionSettings
                {
                    ExecutionMode = (SuiteExecutionMode)Enum.Parse(typeof(SuiteExecutionMode), xEnv.Element("executionmode").Value, true),
                },

                TestCases = testCaseList
            };

            return testSuite;
        }

        public static TestSuite[] GetTestSuitesFromJson(string testSuitesContent)
        {
            var testSuites = Newtonsoft.Json.JsonConvert.DeserializeObject<TestSuite[]>(testSuitesContent);
            return testSuites;
        }
        #endregion

        #region Get TestCaseResult from file
        public static TestCaseResult[] GetTestCaseResultsFromJson(string testCaseResultsContent)
        {
            var testCaseResults = Newtonsoft.Json.JsonConvert.DeserializeObject<TestCaseResult[]>(testCaseResultsContent);
            return testCaseResults;
        }

        public static TestCaseResult GetTestCaseResultFromFile(Uri testCaseResultFile, string testCaseResultFileContent)
        {
            var strTestCaseResultFile = testCaseResultFile.Segments.Last();
            if (!strTestCaseResultFile.Contains("_"))
                return null;

            var testCaseId = strTestCaseResultFile.ToLower().Replace(".xml", "").Split("_".ToCharArray());
            //create a testresult object
            var testResult = new TestCaseResult(int.Parse(testCaseId[0]));
            testResult.TesterIP = testCaseId[1];
            testResult.ID = Guid.Parse(testCaseId[2]);
            testResult.FileLocation = testCaseResultFile.ToString();

            var xResult = XElement.Parse(testCaseResultFileContent);

            testResult.StartTime = DateTime.Parse(xResult.Element("timestamp_start").Value);
            testResult.EndTime = DateTime.Parse(xResult.Element("timestamp_end").Value);
            testResult.TotalTime = int.Parse(xResult.Element("total_time").Value);
            testResult.AverageTime = int.Parse(xResult.Element("average_time").Value);
            testResult.LongestTime = int.Parse(xResult.Element("longest_time").Value);
            testResult.ShortestTime = int.Parse(xResult.Element("shortest_time").Value);

            var xActualResponses = xResult.Element("actualresponses").Elements("response");
            foreach (var xActualResponse in xActualResponses)
            {
                var actualResponse = new ActualResponse
                {
                    Body = Utils.GetFormattedXml(xActualResponse.Element("body").Value),
                    StatusCode = xActualResponse.Element("statuscode").Value,
                    IsExpected = bool.Parse(xActualResponse.Element("isexpected").Value),
                    IsAmountExpected = bool.Parse(xActualResponse.Element("isamountexpected").Value),
                    Headers = xActualResponse.Element("headers").Value,
                    Amount = int.Parse(xActualResponse.Element("amount").Value)
                };

                testResult.ActualResponses.Add(actualResponse);
            }

            return testResult;
        }
        #endregion

        #region Get TestSuiteResult from file
        public static TestSuiteResult GetTestSuiteResultFromJson(string testSuiteResultContent)
        {
            var testSuiteResult = new TestSuiteResult(1);
            return testSuiteResult;
        }

        public static TestSuiteResult GetTestSuiteResultFromFile(string testSuiteResultFile)
        {
            var testResultFileInfo = new FileInfo(testSuiteResultFile);

            if (!testResultFileInfo.Name.Contains("_"))
                return null;

            var testSuiteInfo = testResultFileInfo.Name.ToLower().Replace(".xml", "").Split("_".ToCharArray());

            //create a testresult object
            TestSuiteResult testSuiteResult = new TestSuiteResult(int.Parse(testSuiteInfo[0]));

            testSuiteResult.TesterIP = testSuiteInfo[1];
            testSuiteResult.ID = Guid.Parse(testSuiteInfo[2]);
            testSuiteResult.FileLocation = testResultFileInfo.FullName;

            XElement xResult = XElement.Parse(File.ReadAllText(testSuiteResultFile));

            testSuiteResult.StartTime = DateTime.Parse(xResult.Element("timestamp_start").Value);
            testSuiteResult.EndTime = DateTime.Parse(xResult.Element("timestamp_end").Value);
            testSuiteResult.TotalTime = int.Parse(xResult.Element("total_time").Value);
            testSuiteResult.AverageTime = int.Parse(xResult.Element("average_time").Value);
            testSuiteResult.LongestTime = int.Parse(xResult.Element("longest_time").Value);
            testSuiteResult.ShortestTime = int.Parse(xResult.Element("shortest_time").Value);

            return testSuiteResult;
        }
        #endregion

        private static string GenerateTestCaseList(List<int> testCaseIDs)
        {
            var stringBuilder = new StringBuilder();

            string strTestCases = "";

            if (testCaseIDs != null)
            {
                int count = 0;
                int maxCount = testCaseIDs.Count;

                while (count < maxCount)
                {
                    int testCaseID = testCaseIDs[count];

                    strTestCases += testCaseID.ToString().Trim();
                    count++;

                    if (count != maxCount)
                    {
                        strTestCases += ", ";
                    }
                }
            }

            stringBuilder.AppendLine("<testcases>" + strTestCases + "</testcases>");

            return stringBuilder.ToString();
        }


    }
}
