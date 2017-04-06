using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AutoTest.Server.Models
{
    public class CustomHeadersBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var customHeaders = new Dictionary<string, string>();
            var strCustomHeaders = bindingContext.ValueProvider.GetValue("Request.CustomHeaders").AttemptedValue;
            var strKeyValuePairs = strCustomHeaders.Split("&".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            foreach(var strKeyValuePair in strKeyValuePairs)
            {
                var KeyValuePair = strKeyValuePair.Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var key = KeyValuePair[0];
                var val = KeyValuePair[1];
                customHeaders.Add(key, val);
            }

            return customHeaders;
        }
    }
}