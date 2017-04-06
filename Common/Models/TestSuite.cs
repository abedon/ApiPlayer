using System.Collections.Generic;

namespace AutoTest.Common
{
    public class TestSuite
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<int> TestCases { get; set; }
        public SuiteExecutionSettings Settings { get; set; }
    }    
}
