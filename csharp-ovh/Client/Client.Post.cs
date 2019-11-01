using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Ovh.Api
{
    public partial class Client
    {
        /// <summary>
        /// Issues an async POST call
        /// </summary>
        /// <param name="target">API method to call</param>
        /// <param name="json">Json string to send as body</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <param name="timeout">If specified, overrides default <see cref="Client"/>'s timeout with a custom one</param>
        /// <returns>Raw API response</returns>
        public Task<string> PostStringAsync(string target, string json, bool needAuth = true, TimeSpan? timeout = null)
        {
            return CallAsync("POST", target, json, needAuth, timeout: timeout);
        }

        /// <summary>
        /// Issues an async POST call
        /// </summary>
        /// <param name="target">API method to call</param>
        /// <param name="data">Object to be serialized and sent as a json body</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <param name="timeout">If specified, overrides default <see cref="Client"/>'s timeout with a custom one</param>
        /// <returns>Raw API response</returns>
        public Task<string> PostAsync(string target, object data = null, bool needAuth = true, TimeSpan? timeout = null)
        {
            string json = data is null ? null : JsonConvert.SerializeObject(data);
            return CallAsync("POST", target, json, needAuth, timeout: timeout);
        }

        /// <summary>
        /// Issues an async POST call.
        /// </summary>
        /// <typeparam name="T">Expected return type</typeparam>
        /// <param name="target">API method to call</param>
        /// <param name="json">Json string to send as body</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <param name="timeout">If specified, overrides default <see cref="Client"/>'s timeout with a custom one</param>
        /// <returns>API response deserialized to T by JSON.Net</returns>
        public Task<T> PostStringAsync<T>(string target, string json, bool needAuth = true, TimeSpan? timeout = null)
        {
            return CallAsync<T>("POST", target, json, needAuth, timeout: timeout);
        }

        /// <summary>
        /// Issues an aync POST call
        /// </summary>
        /// <typeparam name="T">Expected return type</typeparam>
        /// <param name="target">API method to call</param>
        /// <param name="data">Object to be serialized and sent as a json body</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <param name="timeout">If specified, overrides default <see cref="Client"/>'s timeout with a custom one</param>
        /// <returns>API response deserialized to T by JSON.Net with Strongly typed object as input</returns>
        public Task<T> PostAsync<T>(string target, object data = null, bool needAuth = true, TimeSpan? timeout = null)
        {
            string json = data is null ? null : JsonConvert.SerializeObject(data);
            return CallAsync<T>("POST", target, json, needAuth, timeout: timeout);
        }
    }
}