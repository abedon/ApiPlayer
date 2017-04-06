using System.Collections.Generic;

namespace AutoTest.Common
{
    public class TestCase
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TestRequest Request { get; set; }
        public ExecutionSettings Settings { get; set; }
        public List<ExpectedResponse> Expectations { get; set; }

        public TestCase()
        {
            Expectations = new List<ExpectedResponse>();
            //{ 
            //    new ExpectedResponse() 
            //}
        }
    } 
}
