using NUnit.Framework;
using Ovh.Api;
using Ovh.Api.Exceptions;

namespace Ovh.Test
{
    [TestFixture]
    public class ClientWithtManualParams
    {
        [Test]
        public void NoParamsThrowsConfigurationKeyMissingException()
        {
            Assert.Throws<ConfigurationKeyMissingException>(() => new Client());
        }

        [Test]
        public void ValidEndpointParam()
        {
            Client client = new Client("ovh-eu");
            Assert.AreEqual(client.Endpoint, "https://eu.api.ovh.com/1.0/");
        }

        [Test]
        public void ValidParams()
        {
            Client client =
                new Client("ovh-eu", "applicationKey", "secretKey",
                    "consumerKey", timeout: 120);
            Assert.AreEqual(client.Endpoint, "https://eu.api.ovh.com/1.0/");
        }

        [Test]
        public void InvalidEndpointParamThrowsInvalidRegionException()
        {
            Assert.Throws<InvalidRegionException>(() => new Client("ovh-noWhere"));
        }
    }
}

