using System.Text;
using System.Web;

namespace CodeConverters.Mvc.Diagnostics
{
    public static class Json
    {
        public static string GetPayload(HttpRequestBase request)
        {
            request.InputStream.Position = 0;
            var bytes = new byte[request.InputStream.Length];
            request.InputStream.Read(bytes, 0, bytes.Length);
            request.InputStream.Position = 0;
            return Encoding.ASCII.GetString(bytes);
        }
    }
}