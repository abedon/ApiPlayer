using System.Web.Http;
using AutoTest.Common;
using System.Web.Http.ModelBinding;
using AutoTest.API.Areas.WebDAV.Models;
using System;
using System.Web;

namespace AutoTest.API.Areas.WebDAV.Controllers
{
    public class TestCaseResultController : ApiController
    {
        WebDavConnector webDavConnector;

        public TestCaseResultController()
        {
            webDavConnector = new WebDavConnector();
            //var clientAddress = HttpContext.Current.Request.UserHostAddress;
            var clientAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            webDavConnector.Initialize(clientAddress);
        }
        
        [HttpGet]
        [Route("api/{area}/TestCaseResult/List")]
        public int[] List()
        {
            //list all executable test case IDs
            return new int[] { 1, 2, 3, 4, 5 };
        }

        [HttpGet]
        [Route("api/{area}/TestCaseResult/CleanAll")]
        public void CleanAll()
        {
            //clean all test cases
            
        }

        // GET: api/WebDAV/TestCase/Clean/?id=1,2,3,4,5
        [HttpGet]
        [Route("api/{area}/TestCaseResult/Clean")]
        public void Clean([ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] id)
        {
            //clean the selected test cases

        }

        // GET: api/WebDAV/TestCase/?id=1,2,3,4,5
        public TestCase[] Get([ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] id)
        {
            throw new NotImplementedException();
            //var testCases = WebDavConnector.LoadSelectTestCaseResults(id);
            //return testCases;
        }

        [HttpGet]
        [Route("api/{area}/TestCaseResult/All")]
        public TestCaseResult[] All()
        {
            var testCaseResults = webDavConnector.LoadAllTestCaseResults();
            return testCaseResults;
        }

        public bool Post([FromBody]TestCaseResult[] testCaseResults)
        {
            if (webDavConnector.UploadTestCaseResults(testCaseResults))
                return true;

            return false;
        }

        // PUT: api/TestCase/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/webdav/TestCaseResult/{guid}
        public bool Delete(Guid id)
        {
            var res = webDavConnector.DeleteTestCaseResult(id);
            return res;
        }
    }
}
