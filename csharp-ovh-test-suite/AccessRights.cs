using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ovh.Api;
using System.Collections.Generic;
using System.Linq;

namespace csharp_ovh_test_suite
{
    [TestClass]
    public class AccessRights
    {
        [TestMethod]
        public void AddRulesWorksAsCtor()
        {
            var rights = new List<AccessRight>();
            rights.Add(new AccessRight("GET", "/me"));
            rights.Add(new AccessRight("GET", "/me/*"));
            rights.Add(new AccessRight("GET", "/domain"));
            rights.Add(new AccessRight("POST", "/domain"));
            rights.Add(new AccessRight("DELETE","/domain"));
            CredentialRequest request = new CredentialRequest(rights, "");

            CredentialRequest request2 = new CredentialRequest(new List<AccessRight>(), "");
            request2.AddRecursiveRules(new string[] { "GET" }, "/me");
            request2.AddRules(new string[] { "GET", "POST", "DELETE" }, "/domain");

            Assert.IsTrue(request.AccessRules.Count == request2.AccessRules.Count);

            foreach (var rule in request.AccessRules)
            {
                Assert.IsTrue(request2.AccessRules.Any(r => rule.Path == r.Path && rule.Method == r.Method));
            }

            CredentialRequest request3 = new CredentialRequest(new List<AccessRight>(), "");
            request3.AddRecursiveRules(new string[] { "GET" }, "/me/");
            request3.AddRules(new string[] { "GET", "POST", "DELETE" }, "/domain");

            CredentialRequest request4 = new CredentialRequest(new List<AccessRight>(), "");
            request4.AddRecursiveRules(new string[] { "GET" }, "/me/*");
            request4.AddRules(new string[] { "GET", "POST", "DELETE" }, "/domain");

            Assert.IsTrue(request3.AccessRules.Count == request4.AccessRules.Count);

            foreach (var rule in request3.AccessRules)
            {
                Assert.IsTrue(request4.AccessRules.Any(r => rule.Path == r.Path && rule.Method == r.Method));
            }
        }
    }
}
