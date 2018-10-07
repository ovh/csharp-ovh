//Copyright(c) 2013-2016, OVH SAS.
//All rights reserved.

//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:

//  * Redistributions of source code must retain the above copyright
//   notice, this list of conditions and the following disclaimer.

// * Redistributions in binary form must reproduce the above copyright
//   notice, this list of conditions and the following disclaimer in the
//   documentation and/or other materials provided with the distribution.

// * Neither the name of OVH SAS nor the
//   names of its contributors may be used to endorse or promote products
//   derived from this software without specific prior written permission.

//THIS SOFTWARE IS PROVIDED BY OVH SAS AND CONTRIBUTORS ``AS IS'' AND ANY
//EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED.IN NO EVENT SHALL OVH SAS AND CONTRIBUTORS BE LIABLE FOR ANY
//DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ovh.Api.Exceptions;
using Ovh.Api.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ovh.Api
{
    //This module provides a simple C# wrapper over the OVH REST API.
    //It handles requesting credential, signing queries...
    // - To get your API keys: https://eu.api.ovh.com/createApp/
    // - To get started with API: https://api.ovh.com/g934.first_step_with_api

    /// <summary>
    /// Low level OVH Client. It abstracts all the authentication and request
    /// signing logic along with some nice tools helping with key generation.
    /// All low level request logic including signing and error handling takes place
    /// in "Client.Call" function. Convenient wrappers
    /// "Client.Get" "Client.Post", "Client.Put",
    /// "Client.Delete" should be used instead. "Client.Post",
    /// "Client.Put" both accept arbitrary list of keyword arguments
    /// mapped to "data" param of "Client.Call".
    /// Example usage:
    ///     from ovh import Client, APIError
    ///     REGION = 'ovh-eu'
    ///     APP_KEY= "&lt;application key&gt;"
    ///     APP_SECRET= "&lt;application secret key&gt;"
    ///     CONSUMER_KEY= "&lt;consumer key&gt;>"
    ///     client = Client(REGION, APP_KEY, APP_SECRET, CONSUMER_KEY)
    ///     try:
    ///         print client.get('/me')
    ///     except APIError as e:
    ///         print "Ooops, failed to get my info:", e.msg
    /// </summary>
    public class Client
    {
        private readonly Dictionary<string, string> _endpoints =
            new Dictionary<string, string>();

        private const int _defaultTimeout = 180;
        private readonly WebClient _webClient;

        /// <summary>
        /// Configuration manager used by this <c>Client</c>
        /// </summary>
        public ConfigurationManager ConfigurationManager { get; set; }
        /// <summary>
        /// API Endpoint that this <c>Client</c> targets
        /// </summary>
        public string Endpoint { get; set; }
        /// <summary>
        /// API application Key
        /// </summary>
        public string ApplicationKey { get; set; }
        /// <summary>
        /// API application secret
        /// </summary>
        public string ApplicationSecret { get; set; }
        /// <summary>
        /// Consumer key that can be either <see cref="RequestConsumerKey">generated</see> or passed to the <see cref="ConfigurationManager">configuration manager</see>>
        /// </summary>
        public string ConsumerKey { get; set; }
        /// <summary>
        /// HTTP operations timeout
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// Character that will be considered as a value separator
        /// in the query URL (i.e https://api/resource/1,2).
        /// Default is the comma (',')
        public char ParameterSeparator { get; set; } = ',';

        private bool _isTimeDeltaInitialized;
        private long _timeDelta;

        /// <summary>
        /// Request signatures are valid only for a short amount of time to mitigate
        /// risk of attack replay scenarii which requires to use a common time
        /// reference.This function queries endpoint's time and computes the delta.
        /// This entrypoint does not require authentication.
        /// This method is *lazy*. It will only load it once even though it is used
        /// for each request.
        /// </summary>
        public long TimeDelta
        {
            get
            {
                if (!_isTimeDeltaInitialized)
                {
                    _timeDelta = ComputeTimeDelta();
                    _isTimeDeltaInitialized = true;
                }
                return _timeDelta;
            }
        }

        private Client()
        {
            _endpoints.Add("ovh-eu", "https://eu.api.ovh.com/1.0/");
            _endpoints.Add("ovh-us", "https://api.ovhcloud.com/1.0");
            _endpoints.Add("ovh-ca", "https://ca.api.ovh.com/1.0/");
            _endpoints.Add("kimsufi-eu", "https://eu.api.kimsufi.com/1.0/");
            _endpoints.Add("kimsufi-ca", "https://ca.api.kimsufi.com/1.0/");
            _endpoints.Add("soyoustart-eu", "https://eu.api.soyoustart.com/1.0/");
            _endpoints.Add("soyoustart-ca", "https://ca.api.soyoustart.com/1.0/");
            _endpoints.Add("runabove-ca", "https://api.runabove.com/1.0/");

            _webClient = new WebClient();
            _webClient.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
        }

        /// <summary>
        /// Creates a new Client. No credential check is done at this point.
        /// The "application_key" identifies your application while
        /// "application_secret" authenticates it. On the other hand, the
        /// "consumer_key" uniquely identifies your application's end user without
        /// requiring his personal password.
        /// If any of "endpoint", "application_key", "application_secret"
        /// or "consumer_key" is not provided, this client will attempt to locate
        /// from them from environment, %USERPROFILE%/.ovh.cfg or current_dir/.ovh.cfg.
        /// </summary>
        /// <param name="endpoint">API endpoint to use. Valid values in "Endpoints"</param>
        /// <param name="applicationKey">Application key as provided by OVH</param>
        /// <param name="applicationSecret">Application secret key as provided by OVH</param>
        /// <param name="consumerKey">User token as provided by OVH</param>
        /// <param name="timeout">Connection timeout for each request</param>
        public Client(string endpoint = null, string applicationKey = null,
            string applicationSecret = null, string consumerKey = null,
            int timeout = _defaultTimeout, char parameterSeparator = ',') : this()
        {
            ConfigurationManager = new ConfigurationManager();

            //Endpoint
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                endpoint = ConfigurationManager.Get("default", "endpoint");
            }

            try
            {
                Endpoint = _endpoints[endpoint];
                _webClient.BaseAddress = Endpoint;
            }
            catch (KeyNotFoundException)
            {
                throw new InvalidRegionException(
                    $"Unknown endpoint {endpoint}. Valid endpoints: {string.Join(",", _endpoints.Keys)}");
            }

            //ApplicationKey
            if (string.IsNullOrWhiteSpace(applicationKey))
            {
                string tempApplicationKey;
                if (ConfigurationManager.TryGet(
                    endpoint, "application_key", out tempApplicationKey))
                {
                    ApplicationKey = tempApplicationKey;
                }
            }
            else
            {
                ApplicationKey = applicationKey;
            }

            //SecretKey
            if (string.IsNullOrWhiteSpace(applicationSecret))
            {
                string tempAppSecret;
                if (ConfigurationManager.TryGet(
                    endpoint, "application_secret", out tempAppSecret))
                {
                    ApplicationSecret = tempAppSecret;
                }
            }
            else
            {
                ApplicationSecret = applicationSecret;
            }

            //ConsumerKey
            if (string.IsNullOrWhiteSpace(consumerKey))
            {
                string tempConsumerKey;
                if (ConfigurationManager.TryGet(
                    endpoint, "consumer_key", out tempConsumerKey))
                {
                    ConsumerKey = tempConsumerKey;
                }
            }
            else
            {
                ConsumerKey = consumerKey;
            }

            //Timeout
            Timeout = timeout;
        }

        #region GET
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

        #endregion

        #region POST

        /// <summary>
        /// Issues a POST call
        /// </summary>
        /// <param name="target">API method to call</param>
        /// <param name="data">Json data to send as body</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>Raw API response</returns>
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
        #endregion

        #region PUT
        /// <summary>
        /// Issues a PUT call
        /// </summary>
        /// <param name="target">API method to call</param>
        /// <param name="data">Json data to send as body</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>Raw API response</returns>
        public string Put(string target, string data, bool needAuth = true)
        {
            return Call("PUT", target, data, needAuth);
        }

        /// <summary>
        /// Issues a PUT call
        /// </summary>
        /// <typeparam name="T">Expected return type</typeparam>
        /// <param name="target">API method to call</param>
        /// <param name="data">Json data to send as body</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>API response deserialized to T by JSON.Net</returns>
        public T Put<T>(string target, string data, bool needAuth = true)
        {
            return Call<T>("PUT", target, data, needAuth);
        }

        /// <summary>
        /// Issues a PUT call
        /// </summary>
        /// <typeparam name="T">Expected return type</typeparam>
        /// <typeparam name="Y">Input type</typeparam>
        /// <param name="target">API method to call</param>
        /// <param name="data">Json data to send as body</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>API response deserialized to T by JSON.Net with Strongly typed object as input</returns>
        public T Put<T, Y>(string target, Y data, bool needAuth = true)
            where Y : class
        {
            return Call<T, Y>("PUT", target, data, needAuth);
        }

        /// <summary>
        /// Issues an async PUT call
        /// </summary>
        /// <param name="target">API method to call</param>
        /// <param name="data">Json data to send as body</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>Raw API response</returns>
        public Task<string> PutAsync(string target, string data, bool needAuth = true)
        {
            return CallAsync("PUT", target, data, needAuth);
        }

        /// <summary>
        /// Issues an async PUT call
        /// </summary>
        /// <typeparam name="T">Expected return type</typeparam>
        /// <param name="target">API method to call</param>
        /// <param name="data">Json data to send as body</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>API response deserialized to T by JSON.Net</returns>
        public Task<T> PutAsync<T>(string target, string data, bool needAuth = true)
        {
            return CallAsync<T>("PUT", target, data, needAuth);
        }

        /// <summary>
        /// Issues an async PUT call
        /// </summary>
        /// <typeparam name="T">Expected return type</typeparam>
        /// <typeparam name="Y">Input type</typeparam>
        /// <param name="target">API method to call</param>
        /// <param name="data">Json data to send as body</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>API response deserialized to T by JSON.Net with Strongly typed object as input</returns>
        public Task<T> PutAsync<T, Y>(string target, Y data, bool needAuth = true)
            where Y : class
        {
            return CallAsync<T, Y>("PUT", target, data, needAuth);
        }
        #endregion PUT

        #region DELETE
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

        #endregion


        /// <summary>
        /// Generates a <c>ConsumerKey</c> request
        /// </summary>
        /// <param name="credentialRequest">The exact request to issue</param>
        /// <returns>A result with the confirmation URL returned by the API</returns>
        public CredentialRequestResult RequestConsumerKey(CredentialRequest credentialRequest)
        {
            return Post<CredentialRequestResult, CredentialRequest>("/auth/credential", credentialRequest, false);
        }

        private WebHeaderCollection GetHeaders(string method, string data, bool needAuth, string target, bool isBatch = false)
        {
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add("X-Ovh-Application", ApplicationKey);

            if (data != null)
            {
                headers.Add("Content-type", "application/json");
            }

            if (needAuth)
            {
                if (ApplicationSecret == null)
                {
                    throw new InvalidKeyException("Application secret is missing.");
                }
                if (ConsumerKey == null)
                {
                    throw new InvalidKeyException("ConsumerKey is missing.");
                }

                long currentServerTimestamp = GetCurrentUnixTimestamp() + TimeDelta;

                SHA1Managed sha1Hasher = new SHA1Managed();
                string toSign =
                    string.Join("+", ApplicationSecret, ConsumerKey, method,
                        target, data, currentServerTimestamp);
                byte[] binaryHash = sha1Hasher.ComputeHash(Encoding.UTF8.GetBytes(toSign));
                string signature = string.Join("",
                    binaryHash.Select(x => x.ToString("X2"))).ToLower();

                headers.Add("X-Ovh-Consumer", ConsumerKey);
                headers.Add("X-Ovh-Timestamp", currentServerTimestamp.ToString());
                headers.Add("X-Ovh-Signature", "$1$" + signature);
            }

            if(isBatch)
            {
                headers.Add("X-Ovh-Batch", ParameterSeparator.ToString());
            }

            return headers;
        }

        #region Call

        /// <summary>
        /// Lowest level call helper. If "consumerKey" is not "null", inject
        /// authentication headers and sign the request.
        /// Request signature is a sha1 hash on following fields, joined by '+'
        ///  - application_secret
        ///  - consumer_key
        ///  - METHOD
        ///  - full request url
        ///  - body
        ///  - server current time (takes time delta into account)
        /// </summary>
        /// <param name="method">HTTP verb. Usualy one of GET, POST, PUT, DELETE</param>
        /// <param name="path">api entrypoint to call, relative to endpoint base path</param>
        /// <param name="data">any json serializable data to send as request's body</param>
        /// <param name="needAuth">if False, bypass signature</param>
        /// <param name="isBatch">If true, this call will query multiple resources at the same time</param>
        /// <exception cref="HttpException">When underlying request failed for network reason</exception>
        /// <exception cref="InvalidResponseException">when API response could not be decoded</exception>
        private string Call(string method, string path, string data = null, bool needAuth = true, bool isBatch = false)
        {
            PrepareCall(ref method, ref path, data, needAuth, isBatch);

            try
            {
                if (method != "GET")
                {
                    return _webClient.UploadString(path, method, data ?? "");
                }
                else
                {
                    return _webClient.DownloadString(path);
                }
            }
            catch (WebException ex)
            {
                throw HandleWebException(ex);
            }
        }

        /// <summary>
        /// Lowest level async call helper. If "consumerKey" is not "null", inject
        /// authentication headers and sign the request.
        /// Request signature is a sha1 hash on following fields, joined by '+'
        ///  - application_secret
        ///  - consumer_key
        ///  - METHOD
        ///  - full request url
        ///  - body
        ///  - server current time (takes time delta into account)
        /// </summary>
        /// <param name="method">HTTP verb. Usualy one of GET, POST, PUT, DELETE</param>
        /// <param name="path">api entrypoint to call, relative to endpoint base path</param>
        /// <param name="data">any json serializable data to send as request's body</param>
        /// <param name="needAuth">if False, bypass signature</param>
        /// <param name="isBatch">If true, this will query multiple resources in one call</param>
        /// <exception cref="HttpException">When underlying request failed for network reason</exception>
        /// <exception cref="InvalidResponseException">when API response could not be decoded</exception>
        private Task<string> CallAsync(string method, string path, string data = null, bool needAuth = true, bool isBatch = false)
        {
            PrepareCall(ref method, ref path, data, needAuth, isBatch);

            Task<string> responseTask = null;

            if (method != "GET")
            {
                responseTask = _webClient.UploadStringTaskAsync(path, method, data ?? "");
            }
            else
            {
                responseTask = _webClient.DownloadStringTaskAsync(path);
            }

            return responseTask.ContinueWith((r) => CallAsyncError(r));

            // Wrapper used so that we catch the exception ourselves
            // and return something more useful to the user if we can
            string CallAsyncError(Task<string> task)
            {
                try
                {
                    return task.Result;
                }
                catch (WebException ex)
                {
                    throw HandleWebException(ex);
                }
            }
        }

        private Exception HandleWebException(WebException ex)
        {
            using (HttpWebResponse httpResponse = (HttpWebResponse)ex.Response)
            {
                if (httpResponse == null)
                {
                    return new HttpException("Low HTTP request failed error", ex);
                }
                return ExtractExceptionFromHttpError(httpResponse, ex);
            }
        }

        private void PrepareCall(ref string method, ref string path, string data, bool needAuth, bool isBatch = false)
        {
            method = method.ToUpper();
            if (path.StartsWith("/"))
            {
                path = path.Substring(1);
            }
            string target = Endpoint + path;
            //NOTE: would be better to reuse some headers
            _webClient.Headers = GetHeaders(method, data, needAuth, target, isBatch);
        }

        private T Call<T>(string method, string path, string data = null, bool needAuth = true, bool isBatch = false)
        {
            return JsonConvert.DeserializeObject<T>(Call(method, path, data, needAuth, isBatch: isBatch));
        }

        private Task<T> CallAsync<T>(string method, string path, string data = null, bool needAuth = true, bool isBatch = false)
        {
            return CallAsync(method, path, data, needAuth, isBatch).ContinueWith((r) => JsonConvert.DeserializeObject<T>(r.Result));
        }

        private T Call<T, Y>(string method, string path, Y data = null, bool needAuth = true)
            where Y : class
        {
            return Call<T>(method, path, JsonConvert.SerializeObject(data), needAuth);
        }

        private Task<T> CallAsync<T, Y>(string method, string path, Y data = null, bool needAuth = true)
            where Y : class
        {
            return CallAsync<T>(method, path, JsonConvert.SerializeObject(data), needAuth);
        }

        #endregion

        private Exception ExtractExceptionFromHttpError(HttpWebResponse httpResponse, Exception ex)
        {
            JObject responseObject = ExtractResponseObject(httpResponse);
            string message = "";
            string errorCode = "";

            if (responseObject["message"] != null)
            {
                try
                {
                    message = responseObject["message"].Value<string>();
                }
                catch (Exception) { }
            }

            if (responseObject["errorCode"] != null)
            {
                try
                {
                    errorCode = responseObject["errorCode"].Value<string>();
                }
                catch (Exception) { }
            }

            switch (httpResponse.StatusCode)
            {
                case HttpStatusCode.Forbidden:
                    if (errorCode == "NOT_GRANTED_CALL")
                    {
                        return new NotGrantedCallException(message, ex);
                    }
                    else if (errorCode == "NOT_CREDENTIAL")
                    {
                       return new NotCredentialException(message, ex);
                    }
                    else if (errorCode == "INVALID_KEY")
                    {
                       return new InvalidKeyException(message, ex);
                    }
                    else if (errorCode == "INVALID_CREDENTIAL")
                    {
                       return new InvalidCredentialException(message, ex);
                    }
                    else if (errorCode == "FORBIDDEN")
                    {
                       return new ForbiddenException(message, ex);
                    }
                    else
                    {
                       return new ApiException(message, ex);
                    }
                case HttpStatusCode.NotFound:
                    throw new ResourceNotFoundException(message, ex);
                case HttpStatusCode.BadRequest:
                    if (errorCode == "QUERY_TIME_OUT")
                    {
                       return new StaleRequestException(message, ex);
                    }
                    else
                    {
                       return new BadParametersException(message, ex);
                    }
                case HttpStatusCode.Conflict:
                   return new ResourceConflictException(message, ex);
                default:
                    return new ApiException(message, ex);
            }
        }

        private JObject ExtractResponseObject(HttpWebResponse httpResponse)
        {
            var dataStream = new StreamReader(httpResponse.GetResponseStream());
            string dataString = dataStream.ReadToEnd();
            JObject responseObject = JsonConvert.DeserializeObject<JObject>(dataString);
            return responseObject;
        }

        private long ComputeTimeDelta()
        {
            long serverUnixTimestamp = Get<long>("/auth/time", null, false);
            long currentUnixTimestamp = GetCurrentUnixTimestamp();
            return serverUnixTimestamp - currentUnixTimestamp;
        }

        private long GetCurrentUnixTimestamp()
        {
            return (long)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}
