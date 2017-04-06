using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace AutoTest.Common
{
    public static class AutoTestingConstants
    {
        public const string TEST_CASE_SUB_FOLDER = "/tests/cases/";
        public const string TEST_SUITE_SUB_FOLDER = "/tests/suites/";
        public const string TEST_CASE_RESULT_SUB_FOLDER = "/results/cases/";
        public const string TEST_SUITE_RESULT_SUB_FOLDER = "/results/suites/";
        public const string WEB_CONFIG = "web.config";
        public const string TEST_MANAGEMENT_SERVER = "TestManagementServer";

        public static string ApiRoot => ConfigurationManager.AppSettings["ApiRoot"];
    }
}
