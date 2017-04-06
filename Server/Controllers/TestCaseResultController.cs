using System;
using System.Linq;
using System.Web.Mvc;
using AutoTest.Server.Models;
using AutoTest.Common;

namespace AutoTest.Server.Controllers
{ 
    public class TestCaseResultController : Controller
    {
        private TestCaseResultContext db = new TestCaseResultContext();

        public TestCaseResultController()
        {
            var clientAddress = System.Web.HttpContext.Current.Request.UserHostAddress;
            clientAddress = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        }

        //
        // GET: /TestResult/

        public ViewResult Index()
        {
            return View(db.TestCaseResults.ToList());
        }

        //
        // GET: /TestResult/Details/5

        public ViewResult Details(Guid id)
        {
            var testResult = db.TestCaseResults.Find(tr => tr.ID == id);
            return View(testResult);
        }

        //
        // GET: /TestResult/Delete/5

        public ActionResult Delete(Guid id)
        {
            var testResult = db.TestCaseResults.Find(tr => tr.ID == id);
            return View(testResult);
        }

        //
        // POST: /webdav/TestCaseResult/Delete/{guid}

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(Guid id)
        {
            //delete the test result file
            var testCaseResult = db.TestCaseResults.Find(tr => tr.ID == id);

            if (testCaseResult != null)
            {                
                var res = ApiConnector.CleanSingleTestCaseResult(testCaseResult.ID);
                if (res)
                    return RedirectToAction("Index");
                else
                    return View("Delete", testCaseResult);
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            //db.Dispose();
            base.Dispose(disposing);
        }
    }
}