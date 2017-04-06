using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using AutoTest.Common;
using AutoTest.Server.Models;
using System.Collections.Generic;
using System;

namespace AutoTest.Server.Controllers
{ 
    public class TestCaseController : Controller
    {
        private TestCaseContext context = new TestCaseContext();

        public TestCaseController()
        {

        }

        //
        // GET: /TestCase/

        public ViewResult Index()
        {
            return View(context.TestCases.ToList());
        }

        //
        // GET: /TestCase/Details/5

        public ViewResult Details(int id)
        {
            TestCase testcase = context.TestCases.Find(tc => tc.ID == id);
            return View(testcase);
        }

        //
        // GET: /TestCase/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /TestCase/Create

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Create(TestCase testcase)
        {
            if (ModelState.IsValid)
            {
                //create a new test case id
                if (context.TestCases.Count > 0)
                {
                    testcase.ID = context.TestCases.Select(tc => tc.ID).Max() + 1;
                }
                else
                {
                    testcase.ID = 1;
                }

                //submit the test case to API
                var res = ApiConnector.CreateTestCase(testcase);

                if (res)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(testcase);
                }
            }

            return View(testcase);
        }

        //
        // GET: /TestCase/Edit/5

        public ActionResult Edit(int id)
        {
            var testcase = context.TestCases.Find(tc => tc.ID == id);
            return View(testcase);
        }

        //
        // POST: /TestCase/Edit/5
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Edit(TestCase testcase)
        {
            if (ModelState.IsValid)
            {
                //submit the test case to API
                var res = ApiConnector.UpdateTestCase(testcase);

                if (res)
                    return RedirectToAction("Index");                
            }

            return View(testcase);
        }

        //
        // GET: /TestCase/Delete/5

        public ActionResult Delete(int id)
        {
            var testcase = context.TestCases.Find(tc => tc.ID == id);
            return View(testcase);
        }

        //
        // POST: /TestCase/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var testcase = context.TestCases.Find(tc => tc.ID == id);

            var res = ApiConnector.CleanSingleTestCase(testcase.ID);
            if (res)
                return RedirectToAction("Index");
            else
                return View("Delete", testcase);

        }

        protected override void Dispose(bool disposing)
        {
            //db.Dispose();
            base.Dispose(disposing);
        }
    }
}