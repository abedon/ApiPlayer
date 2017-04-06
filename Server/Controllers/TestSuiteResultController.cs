using System;
using System.Linq;
using System.Web.Mvc;
using AutoTest.Server.Models;
using System.IO;

namespace AutoTest.Server.Controllers
{ 
    public class TestSuiteResultController : Controller
    {
        private TestSuiteResultContext db = new TestSuiteResultContext();

        //
        // GET: /TestResult/

        public ViewResult Index()
        {
            return View(db.TestSuiteResults.ToList());
        }

        //
        // GET: /TestResult/Details/5

        public ViewResult Details(Guid id)
        {
            var testResult = db.TestSuiteResults.Find(tr => tr.ID == id);
            return View(testResult);
        }

        //
        // GET: /TestResult/Delete/5

        public ActionResult Delete(Guid id)
        {
            var testResult = db.TestSuiteResults.Find(tr => tr.ID == id);
            return View(testResult);
        }

        //
        // POST: /TestResult/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(Guid id)
        {
            var testResult = db.TestSuiteResults.Find(tr => tr.ID == id);
            
            //delete the test result file
            if (testResult != null)
            {
                var fileInfo = new FileInfo(testResult.FileLocation);
                if (fileInfo.Exists)
                    fileInfo.Delete();
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