using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using AutoTest.Common;
using AutoTest.Server.Models;
using System.Collections.Generic;

namespace AutoTest.Server.Controllers
{ 
    public class TestSuiteController : Controller
    {
        private TestSuiteContext context = new TestSuiteContext();
        private readonly string testSuiteRootFolder;

        public TestSuiteController()
        {
            testSuiteRootFolder = context.TestSuiteRootFolder;
        }

        public ViewResult Index()
        {
            return View(context.TestSuites.ToList());
        }

        public ViewResult Details(int id)
        {
            TestSuite testSuite = context.TestSuites.Find(tc => tc.ID == id);

            return View(testSuite);
        }

        public ActionResult Create()
        {
            string TestCaseRootFolder = System.Web.HttpContext.Current.Server.MapPath(AutoTestingConstants.TEST_CASE_SUB_FOLDER);

            if (Directory.Exists(TestCaseRootFolder))
            {
                string[] testCases = Directory.GetDirectories(TestCaseRootFolder);

                for(int i = 0; i < testCases.Length; ++i)
                {
                    testCases[i] = testCases[i].Substring(testCases[i].LastIndexOf("\\") + 1);
                }

                int[] testCaseIDs = Utils.StringArrayToIntegerArray(testCases);

                List<SelectListItem> items = new List<SelectListItem>();

                foreach (int testCaseID in testCaseIDs)
                {
                    items.Add(new SelectListItem
                    {
                        Value = testCaseID.ToString(),
                        Text = testCaseID.ToString()
                    });
                }

                SelectList testCaseList = new SelectList(items, "Value", "Text");

                ViewData["test_cases"] = testCaseList;
            }

            return View();
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Create(TestSuite testSuite)
        {
             if (ModelState.IsValid)
             {
                //create a new test case id
                if (context.TestSuites.Count > 0)
                {
                    testSuite.ID = context.TestSuites.Select(tc => tc.ID).Max() + 1;
                }
                else
                {
                    testSuite.ID = 1;
                }

                if (Directory.Exists(Path.Combine(testSuiteRootFolder, testSuite.ID.ToString())))
                {
                    //in case the testcase id exists, we go back to create page
                    return View(testSuite);
                }
                else
                {
                    //create the test case folder
                    DirectoryInfo testSuiteDir = Directory.CreateDirectory(Path.Combine(testSuiteRootFolder, testSuite.ID.ToString()));

                    System.IO.File.WriteAllText(Path.Combine(testSuiteDir.FullName, "TestSuite.xml"), ModelProcessor.GenerateTestSuiteXml(testSuite));
                }

                return RedirectToAction("Index");
            }

            return View(testSuite);
        }

        public ActionResult Edit(int id)
        {
            TestSuite testSuite = context.TestSuites.Find(tc => tc.ID == id);

            string TestCaseRootFolder = System.Web.HttpContext.Current.Server.MapPath(AutoTestingConstants.TEST_CASE_SUB_FOLDER);

            if (Directory.Exists(TestCaseRootFolder))
            {
                string[] testCases = Directory.GetDirectories(TestCaseRootFolder);

                for (int i = 0; i < testCases.Length; ++i)
                {
                    testCases[i] = testCases[i].Substring(testCases[i].LastIndexOf("\\") + 1);
                }

                int[] testCaseIDs = Utils.StringArrayToIntegerArray(testCases);

                List<SelectListItem> items = new List<SelectListItem>();

                foreach (int testCaseID in testCaseIDs)
                {
                    items.Add(new SelectListItem
                    {
                        Value = testCaseID.ToString(),
                        Text = testCaseID.ToString()
                    });
                }

                SelectList testCaseList = new SelectList(items, "Value", "Text");

                ViewData["test_cases"] = testCaseList;
            }

            return View(testSuite);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Edit(TestSuite testSuite)
        {
            if (ModelState.IsValid)
            {
                var testSuiteDir = new DirectoryInfo(Path.Combine(testSuiteRootFolder, testSuite.ID.ToString()));

                if (!testSuiteDir.Exists)
                    testSuiteDir.Create();

                System.IO.File.WriteAllText(Path.Combine(testSuiteDir.FullName, "TestSuite.xml"), ModelProcessor.GenerateTestSuiteXml(testSuite));
 
                return RedirectToAction("Index");
            }

            return View(testSuite);
        }

        public ActionResult Delete(int id)
        {
            TestSuite testSuite = context.TestSuites.Find(tc => tc.ID == id);

            return View(testSuite);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            TestSuite testSuite = context.TestSuites.Find(tc => tc.ID == id);
            DirectoryInfo testSuiteDir = new DirectoryInfo(Path.Combine(testSuiteRootFolder, testSuite.ID.ToString()));

            if (testSuiteDir.Exists)
            {
                testSuiteDir.Delete(true);
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}