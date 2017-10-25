using Ovh.Api;
using System;
using NUnit.Framework;

namespace Ovh.Test
{
    [TestFixture]
    public class ClientWithEnvironmentParams
    {
        [SetUp]
        public void AddEnpointToEnv()
        {
            Environment.SetEnvironmentVariable("OVH_ENDPOINT", "ovh-eu", EnvironmentVariableTarget.Process);
        }

        [TearDown]
        public void RemoveEnvVariables()
        {
            Environment.SetEnvironmentVariable("OVH_ENDPOINT", null, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("OVH_APPLICATION_KEY", null, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("OVH_APPLICATION_SECRET", null, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("OVH_CONSUMER_KEY", null, EnvironmentVariableTarget.Process);
        }

        public void AddOtherParamsToEnv()
        {
            Environment.SetEnvironmentVariable("OVH_APPLICATION_KEY", "my_app_key", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("OVH_APPLICATION_SECRET", "my_application_secret", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("OVH_CONSUMER_KEY", "my_consumer_key", EnvironmentVariableTarget.Process);
        }

        [Test]
        public void ValidEndpoint()
        {
            Client client = new Client();
            Assert.AreEqual(client.Endpoint, "https://eu.api.ovh.com/1.0/");
            long a = client.TimeDelta;
        }

        [Test]
        public void AllParams()
        {
            AddOtherParamsToEnv();
            Client client = new Client();
            Assert.AreEqual(client.Endpoint, "https://eu.api.ovh.com/1.0/");
            Assert.AreEqual(client.ApplicationKey, "my_app_key");
            Assert.AreEqual(client.ApplicationSecret, "my_application_secret");
            Assert.AreEqual(client.ConsumerKey, "my_consumer_key");
            long a = client.TimeDelta;
        }
    }
}

