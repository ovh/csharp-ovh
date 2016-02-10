using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ovh.Api;
using Ovh.Api.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharp_ovh_test_suite
{
    [TestClass]
    public class Calls
    {
        private const string TestKeyName = "testKey";
        private const string TestKeyValue = "ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAAAYQC1MpRocPluAYqZCLN7/6gRQwWsimrJtQkAAecTKk8MSQrxEJNmvvdfndTtYVhCNxRhbWTevoIKq/FzeTbtDTHucoCxu0kzXAJSr8yPxAlqTql9sKl4pWFyeARj6wrVLP8= null@null.org";

        [TestInitialize]
        public void CreateConfigFile()
        {
            File.WriteAllText(".ovh.conf",
                "[default]" + Environment.NewLine +
                "endpoint=ovh-eu" + Environment.NewLine +

                "[ovh-eu]" + Environment.NewLine +
                "application_key=l1EuFokn8T9DPVm4" + Environment.NewLine +
                "application_secret=wyzOh5jVPfxsvl1YJHhYCYs0JVDUe3kk" + Environment.NewLine +
                "consumer_key=o2QvqEDmORJ4ywBcK3V3qhZ3Y7yYrtmt" + Environment.NewLine +
                "");
        }

        [TestCleanup]
        public void DeleteConfigFile()
        {
            if (File.Exists(".ovh.conf"))
            {
                File.Delete(".ovh.conf");
            }
        }

        [TestMethod]
        public void GetWithResultAsString()
        {
            Client client = new Client();
            client.Get("/me/sshKey/testKey");
        }

        [TestMethod]
        public void GetWithResultAsT()
        {
            Client client = new Client();
            OvhSSHKey key = client.Get<OvhSSHKey>("/me/sshKey/" + TestKeyName);
        }

        [TestMethod]
        public void PostWithResultAsString()
        {
            Client client = new Client();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("key", TestKeyValue);
            parameters.Add("keyName", TestKeyName);
            string result = client.Post("/me/sshKey", parameters);
        }

        [TestMethod]
        public void PostWithResultAsT()
        {
            Client client = new Client();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("key", TestKeyValue);
            parameters.Add("keyName", TestKeyName);
            string result = client.Post<string>("/me/sshKey", parameters);
        }

        [TestMethod]
        public void PutWithResultAsT()
        {
            Client client = new Client();
            Dictionary<string, bool> parameters = new Dictionary<string, bool>();
            parameters.Add("default", true);
            object result = client.Put<object>("/me/sshKey/testKey", parameters);
        }

        [TestMethod]
        public void PutWithResultAsString()
        {
            Client client = new Client();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("default", "false");
            string result = client.Put("/me/sshKey/testKey", parameters);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceConflictException))]
        public void PostThatConflicts()
        {
            Client client = new Client();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("key", TestKeyValue);
            parameters.Add("keyName", TestKeyName);
            string result = client.Post("/me/sshKey", parameters);
        }

        [TestMethod]
        public void DeleteWithResultAsString()
        {
            Client client = new Client();
            string result = client.Delete("/me/sshKey/testKey");
        }

        [TestMethod]
        public void DeleteWithResultAsT()
        {
            Client client = new Client();
            object result = client.Delete<object>("/me/sshKey/testKey");
        }
    }
}
