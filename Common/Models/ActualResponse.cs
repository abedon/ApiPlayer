using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoTest.Common
{
    public class ActualResponse
    {
        public string Body { get; set; }
        public string StatusCode { get; set; }
        public string Headers { get; set; }
        public int Amount { get; set; }
        public bool IsExpected { get; set; } //is body expected
        public bool IsAmountExpected { get; set; } //is amount expected
    }
}
