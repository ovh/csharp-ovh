using System;
using System.Threading.Tasks;

namespace Ovh.Api
{
    public partial class Client
    {
        /// <summary>
        /// Issues a POST call
        /// </summary>
        /// <param name="target">API method to call</param>
        /// <param name="data">Json data to send as body</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>Raw API response</returns>
        [Obsolete("This method just calls the async version and applies 'GetAwaiter().GetResult()' to it. You should not rely on this as it can cause deadlocks. This method will be removed in 4.0.0, switch to the async one.")]
        public string Post(string target, string data, bool needAuth = true)
        {
            return Call("POST", target, data, needAuth);
        }

        /// <summary>
        /// Issues a POST call
        /// </summary>
        /// <typeparam name="T">Expected return type</typeparam>
        /// <param name="target">API method to call</param>
        /// <param name="data">Json data to send as body</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>API response deserialized to T by JSON.Net</returns>
        [Obsolete("This method just calls the async version and applies 'GetAwaiter().GetResult()' to it. You should not rely on this as it can cause deadlocks. This method will be removed in 4.0.0, switch to the async one.")]
        public T Post<T>(string target, string data, bool needAuth = true)
        {
            return Call<T>("POST", target, data, needAuth);
        }

        /// <summary>
        /// Issues a POST call
        /// </summary>
        /// <typeparam name="T">Expected return type</typeparam>
        /// <typeparam name="Y">Input type</typeparam>
        /// <param name="target">API method to call</param>
        /// <param name="data">Json data to send as body</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>API response deserialized to T by JSON.Net with Strongly typed object as input</returns>
        [Obsolete("This method just calls the async version and applies 'GetAwaiter().GetResult()' to it. You should not rely on this as it can cause deadlocks. This method will be removed in 4.0.0, switch to the async one.")]
        public T Post<T, Y>(string target, Y data, bool needAuth = true)
            where Y : class
        {
            return Call<T, Y>("POST", target, data, needAuth);
        }

        /// <summary>
        /// Issues an async POST call
        /// </summary>
        /// <param name="target">API method to call</param>
        /// <param name="data">Json data to send as body</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>Raw API response</returns>
        public Task<string> PostAsync(string target, string data, bool needAuth = true)
        {
            return CallAsync("POST", target, data, needAuth);
        }

        /// <summary>
        /// Issues an async POST call
        /// </summary>
        /// <typeparam name="T">Expected return type</typeparam>
        /// <param name="target">API method to call</param>
        /// <param name="data">Json data to send as body</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>API response deserialized to T by JSON.Net</returns>
        public Task<T> PostAsync<T>(string target, string data, bool needAuth = true)
        {
            return CallAsync<T>("POST", target, data, needAuth);
        }

        /// <summary>
        /// Issues an aync POST call
        /// </summary>
        /// <typeparam name="T">Expected return type</typeparam>
        /// <typeparam name="Y">Input type</typeparam>
        /// <param name="target">API method to call</param>
        /// <param name="data">Json data to send as body</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>API response deserialized to T by JSON.Net with Strongly typed object as input</returns>
        public Task<T> PostAsync<T, Y>(string target, Y data, bool needAuth = true)
            where Y : class
        {
            return CallAsync<T, Y>("POST", target, data, needAuth);
        }

    }
}