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
using Ovh.Api.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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
    /// </summary>
    public partial class Client
    {

        public const string OVH_APP_HEADER = "X-Ovh-Application";
        public const string OVH_CONSUMER_HEADER = "X-Ovh-Consumer";
        public const string OVH_TIME_HEADER = "X-Ovh-Timestamp";
        public const string OVH_SIGNATURE_HEADER = "X-Ovh-Signature";
        public const string OVH_BATCH_HEADER = "X-Ovh-Batch";

        private readonly Dictionary<string, string> _endpoints =
            new Dictionary<string, string>();

        private TimeSpan _defaultTimeout = TimeSpan.FromSeconds(180);

        private static HttpClient _httpClient;

        /// <summary>
        /// Configuration manager used by this <c>Client</c>
        /// </summary>
        public ConfigurationManager ConfigurationManager { get; set; }
        /// <summary>
        /// API Endpoint that this <c>Client</c> targets
        /// </summary>
        public Uri Endpoint { get; set; }
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
                    _timeDelta = ComputeTimeDelta().GetAwaiter().GetResult();
                    _isTimeDeltaInitialized = true;
                }
                return _timeDelta;
            }
        }

        private ITimeProvider _timeProvider = new TimeProvider();

        private Client()
        {
            _endpoints.Add("ovh-eu", "https://eu.api.ovh.com/1.0/");
            _endpoints.Add("ovh-us", "https://api.us.ovhcloud.com/1.0");
            _endpoints.Add("ovh-ca", "https://ca.api.ovh.com/1.0/");
            _endpoints.Add("kimsufi-eu", "https://eu.api.kimsufi.com/1.0/");
            _endpoints.Add("kimsufi-ca", "https://ca.api.kimsufi.com/1.0/");
            _endpoints.Add("soyoustart-eu", "https://eu.api.soyoustart.com/1.0/");
            _endpoints.Add("soyoustart-ca", "https://ca.api.soyoustart.com/1.0/");
            _endpoints.Add("runabove-ca", "https://api.runabove.com/1.0/");
        }

        private void LoadConfiguration(string endpoint, string applicationKey,
            string applicationSecret, string consumerKey, char parameterSeparator, 
            string confFileName = ".ovh.conf")
        {
            ConfigurationManager = new ConfigurationManager(confFileName);

            try
            {
                endpoint = endpoint ?? ConfigurationManager.Get("default", "endpoint");
                if (endpoint is null)
                {
                    throw new InvalidRegionException("Endpoint cannot be null.");
                }
                Endpoint = new Uri(_endpoints[endpoint]);
            }
            catch (KeyNotFoundException)
            {
                throw new InvalidRegionException(
                    $"Unknown endpoint {endpoint}. Valid endpoints: {string.Join(",", _endpoints.Keys)}");
            }

            //ApplicationKey
            if (string.IsNullOrWhiteSpace(applicationKey))
            {
                ConfigurationManager.TryGet(endpoint, "application_key", out applicationKey);
            }
            ApplicationKey = applicationKey;

            //SecretKey
            if (string.IsNullOrWhiteSpace(applicationSecret))
            {
                ConfigurationManager.TryGet(endpoint, "application_secret", out applicationSecret);
            }
            ApplicationSecret = applicationSecret;

            //ConsumerKey
            if (string.IsNullOrWhiteSpace(consumerKey))
            {
                ConfigurationManager.TryGet(endpoint, "consumer_key", out consumerKey);
            }
            ConsumerKey = consumerKey;

            ParameterSeparator = parameterSeparator;
        }

        /// <summary>
        /// Creates a new Client. No credential check is done at this point.
        /// The "applicationKey" identifies your application while
        /// "applicationSecret" authenticates it. On the other hand, the
        /// "consumerKey" uniquely identifies your application's end user without
        /// requiring his personal password.
        /// If any of "endpoint", "applicationKey", "applicationSecret"
        /// or "consumerKey" is not provided, this client will attempt to locate
        /// them from environment, %USERPROFILE%/.ovh.cfg and finally current_dir/.ovh.cfg.
        /// </summary>
        /// <remarks><c>timeout</c> will be taken into account only at the first
        /// instantiation of an <see cref=Ovh.Api.Client/> because the <c>HttpClient</c> used
        /// by it is static</remarks>
        /// <param name="endpoint">API endpoint to use. Valid values in "Endpoints"</param>
        /// <param name="applicationKey">Application key as provided by OVH</param>
        /// <param name="applicationSecret">Application secret key as provided by OVH</param>
        /// <param name="consumerKey">User token as provided by OVH</param>
        /// <param name="defaultTimeout">Connection timeout for each request</param>
        /// <param name="parameterSeparator">Separator that should be used when sending Batch Requests</param>
        public Client(string endpoint = null, string applicationKey = null,
            string applicationSecret = null, string consumerKey = null,
            TimeSpan? defaultTimeout = null, char parameterSeparator = ',',
            HttpClient httpClient = null, string confFileName = ".ovh.conf") : this()
        {
            LoadConfiguration(endpoint, applicationKey, applicationSecret, consumerKey, parameterSeparator, confFileName);
            _defaultTimeout = defaultTimeout ?? _defaultTimeout;
            _httpClient = httpClient ?? _httpClient ?? new HttpClient();
        }

        /// <summary>
        /// Returns the same client with a modified TimeProvider to make it unit-testable
        /// This is not intended to be used in production code, if you find a valid usecase
        /// for it outside of unit testing, please open an issue so we can integrate it properly.
        /// As such, changes to this method will happen without any notice.
        /// </summary>
        /// <param name="customTimeProvider"></param>
        /// <returns>A client with a custom <see cref=ITimeProvider/></returns>
        internal Client AsTestable(ITimeProvider customTimeProvider)
        {
            _timeProvider = customTimeProvider;
            return this;
        }

        /// <summary>
        /// Generates an async <c>ConsumerKey</c> request
        /// </summary>
        /// <param name="credentialRequest">The exact request to issue</param>
        /// <returns>A result with the confirmation URL returned by the API</returns>
        public async Task<CredentialRequestResult> RequestConsumerKeyAsync(CredentialRequest credentialRequest)
        {
            return await PostAsync<CredentialRequestResult>("/auth/credential", credentialRequest, false);
        }

        /// <summary>
        /// Generates a signature for OVH's APÏ
        /// </summary>
        /// <remarks>
        /// While it can be useful for debugging or testing, There is no need to
        /// manually use that method, it's called behind the scenes by the Client
        /// when helper methods are called.
        /// </remarks>
        /// <param name="applicationSecret">Application's secret</param>
        /// <param name="consumerKey">ConsumerKey generated by OVH's API /atuh/credential</param>
        /// <param name="currentTimestamp">Current unix timestamp, requests are timed</param>
        /// <param name="method">Http method of the request</param>
        /// <param name="target">Target of the request, with query params, without API's url, starting with "/" (i.e: "/me")</param>
        /// <param name="data">HTTP Request's payload</param>
        /// <returns></returns>
        public static string GenerateSignature(string applicationSecret, string consumerKey,
            long currentTimestamp, string method, string target, string data = null)
        {
            SHA1Managed sha1Hasher = new SHA1Managed();
            string toSign =
                string.Join("+", applicationSecret, consumerKey, method,
                target, data, currentTimestamp);
            byte[] binaryHash = sha1Hasher.ComputeHash(Encoding.UTF8.GetBytes(toSign));
            string signature = string.Join("",
                binaryHash.Select(x => x.ToString("X2"))).ToLower();
            return $"$1${signature}";
        }

        private void SetHeaders(HttpRequestMessage msg, string method, string target, string data, bool needAuth, bool isBatch = false)
        {
            var headers = msg.Headers;
            headers.Add(OVH_APP_HEADER, ApplicationKey);

            if (isBatch)
            {
                headers.Add(OVH_BATCH_HEADER, ParameterSeparator.ToString());
            }

            if (!needAuth)
            {
                return;
            }

            if (ApplicationSecret == null)
            {
                throw new InvalidKeyException("Application secret is missing.");
            }
            if (ConsumerKey == null)
            {
                throw new InvalidKeyException("ConsumerKey is missing.");
            }

            long currentTimestamp = _timeProvider.UtcNow.ToUnixTimeSeconds() + TimeDelta;
            string signature = GenerateSignature(
                applicationSecret: ApplicationSecret,
                consumerKey: ConsumerKey,
                currentTimestamp: currentTimestamp,
                method: method,
                target: target,
                data: data);

            headers.Add(OVH_CONSUMER_HEADER, ConsumerKey);
            headers.Add(OVH_TIME_HEADER, currentTimestamp.ToString());
            headers.Add(OVH_SIGNATURE_HEADER, signature);
        }

        #region Call

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
        /// <param name="timeout">If specified, overrides default <see cref="Client"/>'s timeout with a custom one</param>
        /// <exception cref="HttpException">When underlying request failed for network reason</exception>
        /// <exception cref="InvalidResponseException">when API response could not be decoded</exception>
        private async Task<string> CallAsync(string method, string path, string data = null, bool needAuth = true, bool isBatch = false, TimeSpan? timeout = null)
        {
            if (path.StartsWith("/"))
            {
                path = path.Substring(1);
            }

            HttpResponseMessage response = null;

            try
            {
                var httpMethod = new HttpMethod(method);
                HttpRequestMessage msg = new HttpRequestMessage(httpMethod, new Uri(Endpoint + path));
                if (httpMethod != HttpMethod.Get && data != null)
                {
                    msg.Content = new StringContent(data ?? "", Encoding.UTF8, "application/json");
                }

                SetHeaders(msg, method, Endpoint + path, data, needAuth, isBatch);

                using (var cts = new CancellationTokenSource(timeout ?? _defaultTimeout))
                {
                    response = await _httpClient.SendAsync(msg, cts.Token).ConfigureAwait(false);
                }
            }
            catch (HttpRequestException e)
            {
                throw new HttpException("An exception occured while trying to issue the HTTP call", e);
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return await response.Content.ReadAsStringAsync();
            }

            throw await ExtractExceptionFromHttpResponse(response);
        }

        private async Task<T> CallAsync<T>(string method, string path, string data = null, bool needAuth = true, bool isBatch = false, TimeSpan? timeout = null)
        {
            var response = await CallAsync(method, path, data, needAuth, isBatch, timeout);
            return JsonConvert.DeserializeObject<T>(response);
        }

        private Task<T> CallAsync<T>(string method, string path, object data = null, bool needAuth = true)
        {
            return CallAsync<T>(method, path, JsonConvert.SerializeObject(data), needAuth);
        }

        #endregion

        private async Task<ApiException> ExtractExceptionFromHttpResponse(HttpResponseMessage response)
        {
            JObject responseObject = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());
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

            switch (response.StatusCode)
            {
                case HttpStatusCode.Forbidden:
                    if (errorCode == "NOT_GRANTED_CALL")
                    {
                        return new NotGrantedCallException(message);
                    }
                    else if (errorCode == "NOT_CREDENTIAL")
                    {
                        return new NotCredentialException(message);
                    }
                    else if (errorCode == "INVALID_KEY")
                    {
                        return new InvalidKeyException(message);
                    }
                    else if (errorCode == "INVALID_CREDENTIAL")
                    {
                        return new InvalidCredentialException(message);
                    }
                    else if (errorCode == "FORBIDDEN")
                    {
                        return new ForbiddenException(message);
                    }
                    else
                    {
                        return new ApiException(message);
                    }
                case HttpStatusCode.NotFound:
                    throw new ResourceNotFoundException(message);
                case HttpStatusCode.BadRequest:
                    if (errorCode == "QUERY_TIME_OUT")
                    {
                        return new StaleRequestException(message);
                    }
                    else
                    {
                        return new BadParametersException(message);
                    }
                case HttpStatusCode.Conflict:
                    return new ResourceConflictException(message);
                default:
                    return new ApiException(message);
            }
        }

        private async Task<long> ComputeTimeDelta()
        {
            long serverUnixTimestamp = await GetAsync<long>("/auth/time", null, false).ConfigureAwait(false);
            long currentUnixTimestamp = _timeProvider.UtcNow.ToUnixTimeSeconds();
            return serverUnixTimestamp - currentUnixTimestamp;
        }
    }
}
