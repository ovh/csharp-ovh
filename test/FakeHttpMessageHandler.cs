using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ovh.Test
{
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        public FakeHttpMessageHandler(Dictionary<string, List<string>> expectedHeaders = null) {}

        public virtual HttpResponseMessage Send(HttpRequestMessage request)
        {
            throw new NotImplementedException("Now we can setup this method with our mocking framework");
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            return await Task.Factory.StartNew(() => Send(request), cancellationToken);
        }
    }
}