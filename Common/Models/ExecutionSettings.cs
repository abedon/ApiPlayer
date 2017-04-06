using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoTest.Common
{
    public class ExecutionSettings
    {
        public ExecutionMode Mode { get; set; }
        public bool IsDebugEnabled { get; set; }
        public int Interval { get; set; } //seconds
        public int Amount { get; set; }
        public int LoopsOfMixedModeRunning { get; set; }
        public bool ResetServerCacheFirst { get; set; }
    }

    public enum ExecutionMode
    {
        Interval,       //send one request every interval
        Simultaneous,   //send Amount of requests concurrently at once
        Mixed           //send Amount of requests concurrently every interval for LoopsOfMixedModeRunning times
    }
}
