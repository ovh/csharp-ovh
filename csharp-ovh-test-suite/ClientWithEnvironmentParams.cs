// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ovh.Api;
using System;

namespace csharp_ovh_test_suite
{
    [TestClass]
    public class ClientWithEnvironmentParams
    {
        [TestInitialize]
        public void AddEnpointToEnv()
        {
            Environment.SetEnvironmentVariable("OVH_ENDPOINT", "ovh-eu", EnvironmentVariableTarget.Process);
        }

        [TestCleanup]
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

        [TestMethod]
        public void ValidEndpoint()
        {
            Client client = new Client();
            Assert.AreEqual(client.Endpoint, "https://eu.api.ovh.com/1.0/");
            long a = client.TimeDelta;
        }

        [TestMethod]
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
