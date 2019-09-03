using System.Net;
using System.Net.Http;

namespace Ovh.Test.Responses
{
    public static class Delete
    {
        public static readonly string nullAsJsonString = "null";
        public static readonly HttpResponseMessage nullAsHttpResponseMessage =
            HttpResponseMessageFactory.Create(nullAsJsonString, HttpStatusCode.OK);
    }
}