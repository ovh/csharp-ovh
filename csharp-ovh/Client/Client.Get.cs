using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ovh.Api
{
    public partial class Client
    {
        /// <summary>
        /// Issues an async GET call
        /// </summary>
        /// <param name="target">API method to call</param>
        /// <param name="kwargs">Arguments to append to URL</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <param name="timeout">If specified, overrides default <see cref="Client"/>'s timeout with a custom one</param>
        /// <returns>Raw API response</returns>
        public Task<string> GetAsync(string target, QueryStringParams kwargs = null, bool needAuth = true, TimeSpan? timeout = null)
        {
            target += kwargs?.ToString();
            return CallAsync("GET", target, null, needAuth, timeout: timeout);
        }

        /// <summary>
        /// Issues an async batch GET call
        /// </summary>
        /// <param name="target">API method to call</param>
        /// <param name="kwargs">Arguments to append to URL</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <param name="timeout">If specified, overrides default <see cref="Client"/>'s timeout with a custom one</param>
        /// <returns>Raw API response</returns>
        public Task<string> GetBatchAsync(string target, QueryStringParams kwargs = null, bool needAuth = true, TimeSpan? timeout = null)
        {
            target += kwargs?.ToString();
            return CallAsync("GET", target, null, needAuth, isBatch: true, timeout: timeout);
        }

        /// <summary>
        /// Issues an async GET call with an expected return type
        /// </summary>
        /// <typeparam name="T">Expected return type</typeparam>
        /// <param name="target">API method to call</param>
        /// <param name="kwargs">Arguments to append to URL</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <param name="timeout">If specified, overrides default <see cref="Client"/>'s timeout with a custom one</param>
        /// <returns>API response deserialized to T by JSON.Net</returns>
        public Task<T> GetAsync<T>(string target, QueryStringParams kwargs = null, bool needAuth = true, TimeSpan? timeout = null)
        {
            target += kwargs?.ToString();
            return CallAsync<T>("GET", target, null, needAuth, timeout: timeout);
        }

        /// <summary>
        /// Issues an async batch GET call with an expected return type
        /// </summary>
        /// <typeparam name="T">Expected return type</typeparam>
        /// <param name="target">API method to call</param>
        /// <param name="kwargs">Arguments to append to URL</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <param name="timeout">If specified, overrides default <see cref="Client"/>'s timeout with a custom one</param>
        /// <returns>API response deserialized to List<T> by JSON.Net</returns>
        public Task<List<T>> GetBatchAsync<T>(string target, QueryStringParams kwargs = null, bool needAuth = true, TimeSpan? timeout = null)
        {
            target += kwargs?.ToString();
            return CallAsync<List<T>>("GET", target, null, needAuth, isBatch: true, timeout: timeout);
        }
    }
}