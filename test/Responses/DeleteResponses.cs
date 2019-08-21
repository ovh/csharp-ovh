using System.Net;
using System.Net.Http;
using System.Text;

namespace Ovh.Test.Responses
{
    public static class Delete
    {
        public static string nullAsJsonString = "null";
        public static HttpResponseMessage nullAsHttpResponseMessage = new HttpResponseMessage{
            Content = new StringContent(nullAsJsonString, Encoding.UTF8, "application/json"),
            StatusCode = HttpStatusCode.OK
        };
    }
}