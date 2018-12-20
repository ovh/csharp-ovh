using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Ovh.Api
{
    public partial class Client
    {
        /// <summary>
        /// Issues a cascading GET call
        /// </summary>
        /// <param name="target">API method to call</param>
        /// <param name="rootCallKwargs">Arguments to append to the parent URL</param>
        /// <param name="childrenCallsKwargs">Arguments to append to the child URL</param>
        /// <param name="childUrlFormat">Format of the url for subsequent calls. Use * as the placeholder. Defaults to "&lt;target&gt;/*</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>An enumerator over children API Calls</returns>
        public IEnumerable<string> Enumerate(string target, QueryStringParams rootCallKwargs = null,
                                                 QueryStringParams childrenCallsKwargs = null,
                                                 string childUrlFormat = null, bool needAuth = true)
        {
            if (childUrlFormat == null)
            {
                childUrlFormat = target + "/*";
            }

            int placeholderCount = childUrlFormat.Count(c => c == '*');
            if (placeholderCount == 0)
            {
                throw new ArgumentException("Missing placeholder for childUrlFormat", "childUrlFormat");
            }
            else if (placeholderCount > 1)
            {
                throw new ArgumentException("Too many placeholders for childUrlFormat, only one is allowed", "childUrlFormat");
            }

            target += rootCallKwargs?.ToString();
            string childKwargs = childrenCallsKwargs?.ToString();

            foreach (string item in Call<IEnumerable<string>>("GET", target, null, needAuth))
            {
                string childUrl = getChildUrl(childUrlFormat, item, childKwargs);
                yield return Call("GET", childUrl, null, needAuth, isBatch: false);
            }
        }


        /// <summary>
        /// Issues a cascading GET call with an expected return type
        /// </summary>
        /// <typeparam name="T">Expected return type</typeparam>
        /// <param name="target">API method to call</param>
        /// <param name="rootCallKwargs">Arguments to append to the parent URL</param>
        /// <param name="childrenCallsKwargs">Arguments to append to the child URL</param>
        /// <param name="childUrlFormat">Format of the url for subsequent calls. Use * as the placeholder. Defaults to "&lt;target&gt;/*</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>An enumerator over children API Calls, deserialized to T by JSON.Net</returns>
        public IEnumerable<T> Enumerate<T>(string target, QueryStringParams rootCallKwargs = null,
                                                 QueryStringParams childrenCallsKwargs = null,
                                                 string childUrlFormat = null, bool needAuth = true)
        {
            foreach (string item in Enumerate(target, rootCallKwargs, childrenCallsKwargs, childUrlFormat, needAuth))
            {
                yield return JsonConvert.DeserializeObject<T>(item);
            }
        }

        private string getChildUrl(string childUrlFormat, string item, string childKwargs)
        {
            var sb = new StringBuilder(childUrlFormat.Length - 1 + item.Length + childKwargs?.Length ?? 0);
            sb.Append(childUrlFormat);
            sb.Replace("*", item);
            sb.Append(childKwargs);
            return sb.ToString();
        }
    }
}