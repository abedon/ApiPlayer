using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoTest.Common
{
    public class SuiteExecutionSettings
    {
        public SuiteExecutionMode ExecutionMode { get; set; }
    }

    public enum SuiteExecutionMode
    {
        Sequential, // run a test one after another
        Simultaneous // run all tests at the same time
    }
}
