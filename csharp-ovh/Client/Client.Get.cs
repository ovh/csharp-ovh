using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Ovh.Api
{
    public partial class Client
    {
        /// <summary>
        /// Issues a GET call
        /// </summary>
        /// <param name="target">API method to call</param>
        /// <param name="kwargs">Arguments to append to URL</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>Raw API response</returns>
        public string Get(string target, NameValueCollection kwargs = null, bool needAuth = true)
        {
            target += kwargs?.ToString();
            return Call("GET", target, null, needAuth);
        }

        /// <summary>
        /// Issues a batch GET call
        /// </summary>
        /// <param name="target">API method to call</param>
        /// <param name="kwargs">Arguments to append to URL</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>Raw API response</returns>
        public string GetBatch(string target, NameValueCollection kwargs = null, bool needAuth = true)
        {
            target += kwargs?.ToString();
            return Call("GET", target, null, needAuth, isBatch: true);
        }


        /// <summary>
        /// Issues a batch GET call with an expected return type
        /// </summary>
        /// <typeparam name="T">Expected return type</typeparam>
        /// <param name="target">API method to call</param>
        /// <param name="kwargs">Arguments to append to URL</param>
        /// <param name="isBatch">If true, this will query multiple resources in one call</param>
        /// <returns>API response deserialized to List<T> by JSON.Net</returns>
        public List<T> GetBatch<T>(string target, NameValueCollection kwargs = null, bool needAuth = true)
        {
            target += kwargs?.ToString();
            return Call<List<T>>("GET", target, null, needAuth, isBatch: true);
        }

        /// <summary>
        /// Issues a GET call with an expected return type
        /// </summary>
        /// <typeparam name="T">Expected return type</typeparam>
        /// <param name="target">API method to call</param>
        /// <param name="kwargs">Arguments to append to URL</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>API response deserialized to T by JSON.Net</returns>
        public T Get<T>(string target, NameValueCollection kwargs = null, bool needAuth = true)
        {
            target += kwargs?.ToString();
            return Call<T>("GET", target, null, needAuth);
        }

        /// <summary>
        /// Issues an async GET call
        /// </summary>
        /// <param name="target">API method to call</param>
        /// <param name="kwargs">Arguments to append to URL</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>Raw API response</returns>
        public Task<string> GetAsync(string target, NameValueCollection kwargs = null, bool needAuth = true)
        {
            target += kwargs?.ToString();
            return CallAsync("GET", target, null, needAuth);
        }

        /// <summary>
        /// Issues an async batch GET call
        /// </summary>
        /// <param name="target">API method to call</param>
        /// <param name="kwargs">Arguments to append to URL</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>Raw API response</returns>
        public Task<string> GetBatchAsync(string target, NameValueCollection kwargs = null, bool needAuth = true)
        {
            target += kwargs?.ToString();
            return CallAsync("GET", target, null, needAuth, isBatch: true);
        }

        /// <summary>
        /// Issues an async GET call with an expected return type
        /// </summary>
        /// <typeparam name="T">Expected return type</typeparam>
        /// <param name="target">API method to call</param>
        /// <param name="kwargs">Arguments to append to URL</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>API response deserialized to T by JSON.Net</returns>
        public Task<T> GetAsync<T>(string target, NameValueCollection kwargs = null, bool needAuth = true)
        {
            target += kwargs?.ToString();
            return CallAsync<T>("GET", target, null, needAuth);
        }

        /// <summary>
        /// Issues an async batch GET call with an expected return type
        /// </summary>
        /// <typeparam name="T">Expected return type</typeparam>
        /// <param name="target">API method to call</param>
        /// <param name="kwargs">Arguments to append to URL</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>API response deserialized to List<T> by JSON.Net</returns>
        public Task<List<T>> GetBatchAsync<T>(string target, NameValueCollection kwargs = null, bool needAuth = true)
        {
            target += kwargs?.ToString();
            return CallAsync<List<T>>("GET", target, null, needAuth, isBatch: true);
        }
    }
}