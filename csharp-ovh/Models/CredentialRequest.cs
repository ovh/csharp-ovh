//Copyright(c) 2013-2016, OVH SAS.
//All rights reserved.

//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:

//  * Redistributions of source code must retain the above copyright
//   notice, this list of conditions and the following disclaimer.

// * Redistributions in binary form must reproduce the above copyright
//   notice, this list of conditions and the following disclaimer in the
//   documentation and/or other materials provided with the distribution.

// * Neither the name of OVH SAS nor the
//   names of its contributors may be used to endorse or promote products
//   derived from this software without specific prior written permission.

//THIS SOFTWARE IS PROVIDED BY OVH SAS AND CONTRIBUTORS ``AS IS'' AND ANY
//EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED.IN NO EVENT SHALL OVH SAS AND CONTRIBUTORS BE LIABLE FOR ANY
//DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ovh.Api.Models
{
    /// <summary>
    /// Class that represents the data to send to API /auth
    /// </summary>
    public class CredentialRequest
    {
        /// <summary>
        /// The actual rights asked, composed of a set of paths and authorized methods
        /// </summary>
        [JsonProperty(PropertyName = "accessRules")]
        public List<AccessRight> AccessRules { get; set; }

        /// <summary>
        /// The URL on which to redirect the client when he confirms his credentials
        /// </summary>
        [JsonProperty(PropertyName = "redirection")]
        public string Redirection { get; set; }

        public CredentialRequest()
        {
            AccessRules = new List<AccessRight>();
        }

        /// <summary>
        /// Initializes a <c>CredentialRequest</c> with a list of <c>AccessRight</c>
        /// </summary>
        /// <param name="accessRules">Requested access rights</param>
        /// <param name="redirection">The URL on which to redirect the client when he confirms his credentials</param>
        public CredentialRequest(List<AccessRight> accessRules, string redirection) : base()
        {
            AccessRules = accessRules;
            Redirection = redirection;
        }

        /// <summary>
        /// Initializes a <c>CredentialRequest</c> without using <c>AccessRight</c>
        /// </summary>
        /// <param name="accessRules">Requested access rights</param>
        /// <param name="redirection">The URL on which to redirect the client when he confirms his credentials</param>
        public CredentialRequest(List<Tuple<string, string>> accessRules, string redirection) : base ()
        {
            AccessRules = accessRules.Select(r => new AccessRight(r)).ToList();
            Redirection = redirection;
        }

        /// <summary>
        /// Add a new rule to the request
        /// </summary>
        /// <param name="rule">The rule to add to the request</param>
        public void AddRule(AccessRight rule)
        {
            AccessRules.Add(rule);
        }

        /// <summary>
        /// Add a new rule to the request
        /// </summary>
        /// <param name="method">HTTP Method to authorize</param>
        /// <param name="path">API resource to authorize access to</param>
        public void AddRule(string method, string path)
        {
            AddRule(new AccessRight(method, path));
        }

        /// <summary>
        /// Add rules for <c>path</c> pattern, for each methods in <c>methods</c>. This is
        /// </summary>
        /// <param name="methods">HTTP Methods to authorize</param>
        /// <param name="path">API resource to authorize access to</param>
        public void AddRules(IEnumerable<string> methods, string path)
        {
            foreach (var method in methods)
            {
                AddRule(method, path);
            }
        }

        /// <summary>
        /// Use this method to grant access on a full API tree. This is the
        /// recommended way to grant access in the API. It will take care of granting
        /// the root call *AND* sub-calls for you.
        /// </summary>
        /// <param name="methods">HTTP Methods to authorize</param>
        /// <param name="path">API resource to authorize access to</param>
        public void AddRecursiveRules(IEnumerable<string> methods, string path)
        {
            path = Regex.Replace(path, @"/\*{0,1}$", ""); //Strips ending '/' or '/*'
            AddRules(methods, path);
            AddRules(methods, path + "/*");
        }
    }
}