// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ovh.Api;
using Ovh.Api.Exceptions;

namespace csharp_ovh_test_suite
{
    [TestClass]
    public class ClientWithtManualParams
    {
        [TestMethod]
        [ExpectedException(typeof(ConfigurationKeyMissingException))]
        public void NoParamsThrowsConfigurationKeyMissingException()
        {
            Client client = new Client();
        }

        [TestMethod]
        public void ValidEndpointParam()
        {
            Client client = new Client("ovh-eu");
            Assert.AreEqual(client.Endpoint, "https://eu.api.ovh.com/1.0/");
        }

        [TestMethod]
        public void ValidParams()
        {
            Client client =
                new Client("ovh-eu", "applicationKey", "secretKey",
                    "consumerKey", 120);
            Assert.AreEqual(client.Endpoint, "https://eu.api.ovh.com/1.0/");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidRegionException))]
        public void InvalidEndpointParamThrowsInvalidRegionException()
        {
            Client client = new Client("ovh-noWhere");
        }
    }
}
