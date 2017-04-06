using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoTest.Common
{
    /// <summary>
    /// An HTTP request
    /// </summary>
    public class TestRequest
    {
        public RequestType Verb { get; set; }
        public string TargetDomain { get; set; }
        public string TargetIp { get; set; }
        public int Port { get; set; }
        public bool IsSSL { get; set; }
        public Dictionary<string, string> CustomHeaders { get; set; }
        public string AppVersion { get; set; }
        public string QueryString { get; set; }
        public string Body { get; set; }
        public string DebugData { get; set; }

        public TestRequest()
        {
            CustomHeaders = new Dictionary<string, string>();
        }
    }    

    public enum RequestType
    {
        GET,
        POST,
        PUT,
        DELETE
    }
}
