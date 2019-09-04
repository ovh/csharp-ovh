using System.Net.Http;
using Ovh.Api;

namespace Ovh.Test
{
    public static class ClientFactory
    {


        public static Client GetClient(FakeHttpMessageHandler handler, bool withConsumerKey = true)
        {
            if(withConsumerKey)
            {
                return new Client(new HttpClient(handler), Constants.ENDPOINT, Constants.APPLICATION_KEY, Constants.APPLICATION_SECRET, Constants.CONSUMER_KEY);
            }
            return new Client(new HttpClient(handler), Constants.ENDPOINT, Constants.APPLICATION_KEY, Constants.APPLICATION_SECRET);
        }

    }

}