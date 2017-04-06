using AutoTest.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AutoTest.Server.Models
{
    public class MatchTextBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var expectedResponses = new List<ExpectedResponse>();
            var matchText = bindingContext.ValueProvider.GetValue("Expectations.MatchText");
            var matchType = bindingContext.ValueProvider.GetValue("Expectations.MatchType");
            var amount = bindingContext.ValueProvider.GetValue("Expectations.Amount");

            if (matchText != null)
            {
                return GenerateExpectations(matchText, matchType, amount);
            }
            else
            {
                matchText = bindingContext.ValueProvider.GetValue("Expectations.item.MatchText");
                matchType = bindingContext.ValueProvider.GetValue("Expectations.item.MatchType");
                amount = bindingContext.ValueProvider.GetValue("Expectations.item.Amount");

                return GenerateExpectations(matchText, matchType, amount);
            }
        }

        private List<ExpectedResponse> GenerateExpectations(ValueProviderResult matchText, ValueProviderResult matchType, ValueProviderResult amount)
        {
            var expectedResponses = new List<ExpectedResponse>();
            var arrMatchText = (string[])matchText.RawValue;
            var arrMatchType = (string[])matchType.RawValue;
            var arrAmount = (string[])amount.RawValue;

            for (var i = 0; i < arrMatchText.Length; i++)
            {
                var expectedResponse = new ExpectedResponse()
                {
                    MatchText = arrMatchText[i],
                    MatchType = (MatchOption)Enum.Parse(typeof(MatchOption), arrMatchType[i], true),
                    Amount = int.Parse(arrAmount[i])
                };
                expectedResponses.Add(expectedResponse);
            }

            return expectedResponses;
        }
    }
}