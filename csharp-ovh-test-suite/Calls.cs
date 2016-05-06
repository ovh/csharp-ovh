// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
    }
}
