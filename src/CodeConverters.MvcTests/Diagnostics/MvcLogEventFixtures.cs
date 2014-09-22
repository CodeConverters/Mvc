using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CodeConverters.Mvc.Diagnostics;
using CodeConverters.MvcTests.Diagnostics.Helpers;
using Moq;
using Xunit;

namespace CodeConverters.MvcTests.Diagnostics
{
    public class MvcLogEventFixtures
    {
        private readonly MvcLogEvent _sut;
        private readonly string _expectedUrl;
        private readonly string _expectedControllerName;
        private readonly string _expectedActionName;
        private readonly ActionExecutedContext _context;

        public MvcLogEventFixtures()
        {
            _context = ObjectMother.CreateActionActionExecutedContextFake();
            _expectedUrl = _context.RequestContext.HttpContext.Request.RawUrl;
            _expectedControllerName = _context.ActionDescriptor.ControllerDescriptor.ControllerName;
            _expectedActionName = _context.ActionDescriptor.ActionName;

            _sut = new MvcLogEvent(_context);
        }
        [Fact]
        public void LogsUrl()
        {
            var url = _sut.ToString().Split('|').GetLogValue("URL");
            Assert.Equal(_expectedUrl, url);
        }
        [Fact]
        public void LogsControllerActionAndRouteId()
        {
            var logEntries = _sut.ToString().Split('|');
            var controller = logEntries.GetLogValue("Controller");
            var action = logEntries.GetLogValue("Action");
            var id = logEntries.GetLogValue("RouteId");
            Assert.Equal(_expectedControllerName, controller);
            Assert.Equal(_expectedActionName, action);
            Assert.Equal("123", id);
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
        [Fact]
        public void IsGetIsTrueOnHttpGet()
        {
            Assert.True(_sut.IsHttpGet);
            Mock.Get(_context.HttpContext.Request).SetupGet(r => r.HttpMethod).Returns("POST");
            Assert.False(new MvcLogEvent(_context).IsHttpGet);
        }
    }
}
