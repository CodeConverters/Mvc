using CodeConverters.Core.Diagnostics;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CodeConverters.MvcTests.Diagnostics
{
    public class ScrubJson
    {
        [Fact]
        public void WillScrubNestedPropertyValues()
        {
            string[] scrubParams = { "value" };
            const string jsonText = "{'InputFields':[{'Name':'LOGIN','DisplayName':'Username','Value':'aaabbbccc','ValueIdentifier':'LOGIN','ValueMask':'LOGIN_FIELD','FieldType':{'TypeName':'text'}},{'Name':'PASSWORD1','DisplayName':'Password','Value':'111222333','ValueIdentifier':'PASSWORD1','ValueMask':'LOGIN_FIELD','FieldType':{'TypeName':'password'}}]}";
            var json = JObject.Parse(jsonText);
            json.ScrubValues(scrubParams);
            Assert.Equal("**********", json["InputFields"][0]["Value"].Value<string>());
            Assert.Equal("**********", json["InputFields"][1]["Value"].Value<string>());
        }
    }
}