using System;
using System.Collections.Generic;

namespace AutoTest.Common
{
    public class TestCaseResult
    {
        public Guid ID;                                       //each test case excution has a unique id
        public int TestCaseID;                                //the associated test cases id
        public DateTime StartTime;
        public DateTime EndTime;
        public long TotalTime;
        public long LongestTime;
        public long ShortestTime;
        public long AverageTime;
        public short TotalRequests;
        public string TesterIP;
        public string FileLocation;
        public List<ActualResponse> ActualResponses;

        public TestCaseResult(int testCaseID)
        {
            ID = Guid.NewGuid();
            TestCaseID = testCaseID;
            ActualResponses = new List<ActualResponse>();
        }
    }
}
