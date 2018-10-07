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
        public Task<string> DeleteAsync(string target, bool needAuth = true)
        {
            return CallAsync("DELETE", target, null, needAuth);
        }

        /// <summary>
        /// Issues an async DELETE call
        /// </summary>
        /// <typeparam name="T">Expected return type</typeparam>
        /// <param name="target">API method to call</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>API response deserialized to T by JSON.Net</returns>
        public Task<T> DeleteAsync<T>(string target, bool needAuth = true)
        {
            return CallAsync<T>("DELETE", target, null, needAuth);
        }
    }
}