using System.IO;
using System.Web;

namespace CodeConverters.Mvc.Diagnostics
{
    public static class Json
    {
        public static string GetPayload(HttpRequestBase request)
        {
            string jsonString;
            request.InputStream.Position = 0;
            using (var inputStream = new StreamReader(request.InputStream))
            {
                jsonString = inputStream.ReadToEnd();
            }
            request.InputStream.Position = 0;
            return jsonString;
        }
    }
}