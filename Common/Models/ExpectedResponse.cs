using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoTest.Common
{
    /// <summary>
    /// Expected response
    /// </summary>
    public class ExpectedResponse
    {
        public string MatchText { get; set; }
        public MatchOption MatchType { get; set; }
        public int Amount { get; set; }
    }

    public enum MatchOption
    {        
        PartialMatch,       //MatchText will be the delimited keywords or snippets that expected response must contain (match all)
        ExactMatch,         //MatchText will be the complete response body
        StatusCode,         //MatchText will be the expected status code
        RegularExpression   //MatchText will be the regular expression that expected response must match
    }
}
