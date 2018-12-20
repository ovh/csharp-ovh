using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Ovh.Api
{
    public class QueryStringParams : List<(string, string)>
    {
        private List<string> _existingKeys = new List<string>();

        public void Add(string key, string value)
        {
            if(_existingKeys.Contains(key))
            {
                throw new ArgumentException("Duplicate keyword. This is currently not supported by OVH API");
            }
            Add((key, value));
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("?");

            bool firstParam = true;

            foreach (var param in this)
            {
                if(!firstParam)
                {
                    sb.Append("&");
                }

                sb.Append(HttpUtility.UrlEncode(param.Item1));
                sb.Append("=");
                sb.Append(HttpUtility.UrlEncode(param.Item2));

                firstParam = false;
            }

            return sb.ToString();
        }
    }
}