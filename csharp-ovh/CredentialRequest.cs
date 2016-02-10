using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ovh.Api
{
    public class CredentialRequest
    {
        [JsonProperty(PropertyName = "accessRules")]
        public List<AccessRight> AccessRules { get; set; }

        [JsonProperty(PropertyName = "redirection")]
        public string Redirection { get; set; }

        public CredentialRequest()
        {

        }

        public CredentialRequest(List<AccessRight> accessRules, string redirection)
        {
            AccessRules = accessRules;
            Redirection = redirection;
        }

        public CredentialRequest(List<Tuple<string, string>> accessRules, string redirection)
        {
            AccessRules = accessRules.Select(r => new AccessRight(r)).ToList();
            Redirection = redirection;
        }
    }
}