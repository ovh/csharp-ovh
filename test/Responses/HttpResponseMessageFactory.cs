using System.Net;
using System.Net.Http;
using System.Text;

namespace Ovh.Test
{
    public static class HttpResponseMessageFactory
    {
        public static HttpResponseMessage Create(string content, HttpStatusCode statusCode,
            string contentType = "application/json", Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            return new HttpResponseMessage
            {
                Content = new StringContent(content, encoding, contentType),
                StatusCode = statusCode
            };
        }
    }
}