// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace csharp_ovh_test_suite
{
    [TestClass]
    public class Calls
    {
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
