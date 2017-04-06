using System.Web.Http;
using AutoTest.Common;
using System.Web.Http.ModelBinding;
using AutoTest.API.Areas.WebDAV.Models;
using System.Web;

namespace AutoTest.API.Areas.WebDAV.Controllers
{
    public class TestCaseController : ApiController
    {
        WebDavConnector webDavConnector;

        public TestCaseController()
        {
            webDavConnector = new WebDavConnector();
            var clientAddress = HttpContext.Current.Request.UserHostAddress;
            webDavConnector.Initialize(clientAddress);
        }

        [HttpGet]
        [Route("api/{area}/TestCase/List")]
        public int[] List()
        {
            //list all executable test case IDs
            return new int[] { 1, 2, 3, 4, 5 };
        }

        [HttpGet]
        [Route("api/{area}/TestCase/CleanAll")]
        public void CleanAll()
        {
            //clean all test cases
            
        }

        // GET: api/WebDAV/TestCase/Clean/?id=1,2,3,4,5
        [HttpGet]
        [Route("api/{area}/TestCase/Clean")]
        public void Clean([ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] id)
        {
            //clean the selected test cases

        }

        // GET: api/WebDAV/TestCase/?id=1,2,3,4,5
        public TestCase[] Get([ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] id)
        {
            var testCases = webDavConnector.LoadSelectTestCases(id);
            return testCases;
        }

        // GET: api/WebDAV/TestCase/All
        [HttpGet]
        [Route("api/{area}/TestCase/all")]
        public TestCase[] All()
        {
            var testCases = webDavConnector.LoadAllTestCases(true);
            return testCases;
        }

        // POST: api/TestCase
        public bool Post([FromBody]TestCase testCase)
        {
            var res = webDavConnector.CreateTestCase(testCase);
            return res;            
        }

        // PUT: api/webdav/TestCase/5
        public bool Put(int id, [FromBody]TestCase testCase)
        {
            var res = webDavConnector.UpdateTestCase(testCase);
            return res;
        }

        // DELETE: api/TestCase/5
        public bool Delete(int id)
        {
            var res = webDavConnector.DeleteTestCase(id);
            return res;
        }
    }    
}
