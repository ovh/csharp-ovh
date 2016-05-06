// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using IniParser.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ovh.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharp_ovh_test_suite
{
    [TestClass]
    public class ClientWithConfigFile
    {
        public const string OvhConfigFile = ".ovh.conf";

        [TestCleanup]
        public void RemoveConfigFile()
        {
            if (File.Exists(OvhConfigFile))
            {
                File.Delete(OvhConfigFile);
            }
        }

        public void CreateInvalidConfigFile()
        {
            File.WriteAllText(".ovh.conf",
                "Wrong ini" + Environment.NewLine +
                "    file!");
        }

        public void CreateConfigFileWithEndpointOnly()
        {
            File.WriteAllText(".ovh.conf",
                "[default]" + Environment.NewLine +
                "endpoint=ovh-eu");
        }

        public void CreateConfigFileWithAllValues()
        {
            File.WriteAllText(".ovh.conf",
                "[default]" + Environment.NewLine +
                "endpoint=ovh-eu" + Environment.NewLine +

                "[ovh-eu]" + Environment.NewLine +
                "application_key=my_app_key" + Environment.NewLine +
                "application_secret=my_application_secret" + Environment.NewLine +
                "consumer_key=my_consumer_key" + Environment.NewLine +
                "");
        }

        [TestMethod]
        [ExpectedException(typeof(ParsingException))]
        public void InvalidConfigFile()
        {
            CreateInvalidConfigFile();
            Client client = new Client();
        }

        [TestMethod]
        public void ValidConfigFileWithEndpointOnly()
        {
            CreateConfigFileWithEndpointOnly();
            Client client = new Client();
            Assert.AreEqual(client.Endpoint, "https://eu.api.ovh.com/1.0/");
            long a = client.TimeDelta;
        }

        [TestMethod]
        public void ValidConfigFileWithAllValues()
        {
            CreateConfigFileWithAllValues();
            Client client = new Client();
            Assert.AreEqual(client.Endpoint, "https://eu.api.ovh.com/1.0/");
            Assert.AreEqual(client.ApplicationKey, "my_app_key");
            Assert.AreEqual(client.ApplicationSecret, "my_application_secret");
            Assert.AreEqual(client.ConsumerKey, "my_consumer_key");
            long a = client.TimeDelta;
        }
    }
}
