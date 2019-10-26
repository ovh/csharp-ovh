using System;
using System.Threading.Tasks;

namespace Ovh.Api
{
    public partial class Client
    {
        /// <summary>
        /// Issues a DELETE call
        /// </summary>
        /// <param name="target">API method to call</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>Raw API response</returns>
        [Obsolete("This method just calls the async version and applies 'GetAwaiter().GetResult()' to it. You should not rely on this as it can cause deadlocks. This method will be removed in 4.0.0, switch to the async one.")]
        public string Delete(string target, bool needAuth = true)
        {
            return Call("DELETE", target, null, needAuth);
        }

        /// <summary>
        /// Issues a DELETE call
        /// </summary>
        /// <typeparam name="T">Expected return type</typeparam>
        /// <param name="target">API method to call</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>API response deserialized to T by JSON.Net</returns>
        [Obsolete("This method just calls the async version and applies 'GetAwaiter().GetResult()' to it. You should not rely on this as it can cause deadlocks. This method will be removed in 4.0.0, switch to the async one.")]
        public T Delete<T>(string target, bool needAuth = true)
        {
            return Call<T>("DELETE", target, null, needAuth);
        }

        /// <summary>
        /// Issues an async DELETE call
        /// </summary>
        /// <param name="target">API method to call</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>Raw API response</returns>
        public Task<string> DeleteAsync(string target, bool needAuth = true, TimeSpan? timeout = null)
        {
            return CallAsync("DELETE", target, null, needAuth, timeout: timeout);
        }

        /// <summary>
        /// Issues an async DELETE call
        /// </summary>
        /// <typeparam name="T">Expected return type</typeparam>
        /// <param name="target">API method to call</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>API response deserialized to T by JSON.Net</returns>
        public Task<T> DeleteAsync<T>(string target, bool needAuth = true, TimeSpan? timeout = null)
        {
            return CallAsync<T>("DELETE", target, null, needAuth, timeout: timeout);
        }
    }
}