using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Ovh.Test
{
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        public FakeHttpMessageHandler(Dictionary<string, List<string>> expectedHeaders = null) {}

        public virtual HttpResponseMessage Send(HttpRequestMessage request)
        {
            throw new NotImplementedException("Now we can setup this method with our mocking framework");
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            return Task.FromResult(Send(request));
        }
    }
}