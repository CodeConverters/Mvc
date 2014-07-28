using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CodeConverters.Core.Diagnostics;

namespace CodeConverters.Mvc.Diagnostics
{
    public class MvcLogEvent
    {
        private readonly string _controllerName;
        private readonly string _actionName;
        private readonly string _httpMethod;
        private readonly NameValueCollection _headers;
        private readonly string _routeId = string.Empty;
        private readonly NameValueCollection _formCollection = new NameValueCollection();
        private readonly string _url;
        private readonly string _jsonPayload;

        public MvcLogEvent(ActionExecutedContext filterContext)
        {
            _httpMethod = filterContext.HttpContext.Request.HttpMethod;
            _headers = filterContext.HttpContext.Request.Headers;

            var actionDescriptor = filterContext.ActionDescriptor;
            _controllerName = actionDescriptor.ControllerDescriptor.ControllerName;
            _actionName = actionDescriptor.ActionName;

            _url = filterContext.RequestContext.HttpContext.Request.RawUrl;

            if (filterContext.RouteData.Values["id"] != null)
                _routeId = filterContext.RouteData.Values["id"].ToString();

            if (filterContext.HttpContext.Request.Form != null)
                _formCollection = filterContext.HttpContext.Request.Form;

            if (filterContext.HttpContext.Request.Form != null)
                _formCollection = filterContext.HttpContext.Request.Form;

            if (filterContext.HttpContext.Request.ContentType == "application/json")
                _jsonPayload = Json.GetPayload(filterContext.HttpContext.Request);
        }

       public bool IsHttpGet
        {
            get { return _httpMethod.ToLowerInvariant() == "get"; }
        }

        public override string ToString()
        {
            var logEvent = new StringBuilder();
            logEvent.AppendFormat("Url={0}| ", _url);
            logEvent.AppendFormat("Method={0}|", _httpMethod);
            logEvent.AppendFormat("Headers={0}|", _headers.Scrub().ToLogFormat());
            logEvent.AppendFormat("Controller={0}|", _controllerName);
            logEvent.AppendFormat("Action={0}|", _actionName);

            if (!string.IsNullOrEmpty(_routeId))
                logEvent.AppendFormat("RouteId={0}|", _routeId);

            if (_formCollection.HasKeys())
                logEvent.AppendFormat("FormData={0}|", _formCollection.Scrub().ToLogFormat());

            if (!string.IsNullOrWhiteSpace(_jsonPayload))
                logEvent.AppendFormat("JsonPayload={0}|", _jsonPayload.ScrubJson());

            return logEvent.ToString();
        }
    }
}