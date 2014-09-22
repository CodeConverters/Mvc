using System.Collections.Generic;
using System.Linq;
using CodeConverters.Mvc.Diagnostics;
using CodeConverters.MvcTests.Diagnostics.Helpers;
using Xunit;

namespace CodeConverters.MvcTests.Diagnostics
{
    public class MvcErrorLogEventFixtures
    {
        private readonly MvcErrorLogEvent _sut;
        private const string ExpectedUrl = "http://mytesturl.com/mycontroller/myaction/123";

        public MvcErrorLogEventFixtures()
        {
            _sut = new MvcErrorLogEvent(ObjectMother.CreateExceptionContextFake());
        }
        [Fact]
        public void LogsUrl()
        {
            var url = _sut.ToString().Split('|').GetLogValue("URL");
            Assert.Equal(ExpectedUrl, url);
        }

        [Fact]
        public void LogsScrubbedHeaders()
        {
            var headers = _sut.ToString().Split('|').GetLogValue("Headers").Split(',').ToDictionary(k => k.Split(':')[0], v => v.Split(':')[1]);
            Assert.Equal(headers, new Dictionary<string, string> { { "header1", "value1" }, { "header2", "value2" }, { "secret", "**********" } });
        }
        [Fact]
        public void LogsScrubbedFormData()
        {
            var formData = _sut.ToString().Split('|').GetLogValue("FormData").Split(',').ToDictionary(k => k.Split(':')[0], v => v.Split(':')[1]);
            Assert.Equal(formData, new Dictionary<string, string> { { "Form1", "valueA" }, { "form2", "valueB" }, { "password", "**********" } });
        }
    }
}