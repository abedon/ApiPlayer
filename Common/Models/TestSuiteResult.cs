using System;
using System.Collections.Generic;

namespace AutoTest.Common
{
    public class TestSuiteResult
    {
        public Guid ID;                     //each test suite excution has a unique id
        public int TestSuiteID;             //the associated test suite id
        public DateTime StartTime;
        public DateTime EndTime;
        public long TotalTime;
        public long LongestTime;
        public long ShortestTime;
        public long AverageTime;
        public short TotalRequests;
        public string TesterIP;
        public string FileLocation;
        public TestCaseResult[] TestCaseResults;

        public TestSuiteResult(int testSuiteID)
        {
            ID = Guid.NewGuid();
            TestSuiteID = testSuiteID;
        }
    }
}
