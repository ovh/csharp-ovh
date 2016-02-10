using Newtonsoft.Json;
using System;

namespace Ovh.Api
{
    public class AccessRight
    {
        private string _method;
        [JsonProperty(PropertyName = "method")]
        public string Method
        {
            get
            {
                return _method;
            }
            set
            {
                _method = value.ToUpper();
            }
        }

        [JsonProperty(PropertyName = "path")]
        public string Path { get; set; }

        public AccessRight()
        {

        }

        public AccessRight(string method, string path)
        {
            Method = method;
            Path = path;
        }

        public AccessRight(Tuple<string, string> rule)
            : this(rule.Item1, rule.Item2)
        {
        }
    }
}