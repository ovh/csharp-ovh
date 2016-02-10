using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharp_ovh_test_suite
{
    class OvhSSHKey
    {
        [JsonProperty(PropertyName = "default")]
        public bool Default { get; set; }

        [JsonProperty(PropertyName = "keyName")]
        public string KeyName { get; set; }

        [JsonProperty(PropertyName = "key")]
        public string Key { get; set; }
    }
}
