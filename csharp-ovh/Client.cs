using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ovh.Api.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    ///     APP_KEY= "<application key>"
    ///     APP_SECRET= "<application secret key>"
    ///     CONSUMER_KEY= "<consumer key>"
    ///     client = Client(REGION, APP_KEY, APP_SECRET, CONSUMER_KEY)
    ///     try:
    ///         print client.get('/me')
    ///     except APIError as e:
    ///         print "Ooops, failed to get my info:", e.msg
    /// </summary>
    public class Client
    {
        private readonly Dictionary<string, string> Endpoints = 
            new Dictionary<string, string>();

        private const int DefaultTimeout = 180;
        private WebClient WebClient;

        public ConfigurationManager ConfigurationManager { get; set; }
        public string Endpoint { get; set; }
        public string ApplicationKey { get; set; }
        public string ApplicationSecret { get; set; }
        public string ConsumerKey { get; set; }
        public int Timeout { get; set; }

        private long EpochInseconds = (long)TimeSpan.FromTicks(new DateTime(1970, 1, 1).Ticks).TotalSeconds;

        private bool isTimeDeltaInitialized = false;
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
                if (!isTimeDeltaInitialized)
                {
                    _timeDelta = ComputeTimeDelta();
                    isTimeDeltaInitialized = true;
                }
                return _timeDelta;
            }
        }

        private Client()
        {
            Endpoints.Add("ovh-eu", "https://eu.api.ovh.com/1.0/");
            Endpoints.Add("ovh-ca", "https://ca.api.ovh.com/1.0/");
            Endpoints.Add("kimsufi-eu", "https://eu.api.kimsufi.com/1.0/");
            Endpoints.Add("kimsufi-ca", "https://ca.api.kimsufi.com/1.0/");
            Endpoints.Add("soyoustart-eu", "https://eu.api.soyoustart.com/1.0/");
            Endpoints.Add("soyoustart-ca", "https://ca.api.soyoustart.com/1.0/");
            Endpoints.Add("runabove-ca", "https://api.runabove.com/1.0/");

            WebClient = new WebClient();
            WebClient.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
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
        /// <param name="readTimeout">Connection and read timeout for each request</param>
        /// <param name="writeTimeout"></param>
        public Client(string endpoint = null, string applicationKey = null,
            string applicationSecret = null, string consumerKey = null, 
            int timeout = DefaultTimeout) : this()
        {
            ConfigurationManager = new ConfigurationManager();

            //Endpoint
            if (String.IsNullOrWhiteSpace(endpoint))
            {
                endpoint = ConfigurationManager.Get("default", "endpoint");
            }

            try
            {
                Endpoint = Endpoints[endpoint];
                WebClient.BaseAddress = Endpoint;
            }
            catch (KeyNotFoundException)
            {
                throw new InvalidRegionException(String.Format("Unknow endpoint {0}. Valid endpoints: {1}",
                    endpoint, String.Join(",", Endpoints.Keys)));
            }

            //ApplicationKey
            if (String.IsNullOrWhiteSpace(applicationKey))
            {
                string tempApplicationKey = "";
                if(ConfigurationManager.TryGet(
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
            if (String.IsNullOrWhiteSpace(applicationSecret))
            {
                string tempAppSecret = "";
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
            if (String.IsNullOrWhiteSpace(consumerKey))
            {
                string tempConsumerKey = "";
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
        /// Append arguments to the target URL
        /// </summary>
        /// <param name="target">Target URL</param>
        /// <param name="kwargs">Key value arguments to append</param>
        /// <returns>Url suffixed with kwargs</returns>
        private string PrepareGetTarget(string target, NameValueCollection kwargs)
        {
            if (kwargs != null)
            {
                target += kwargs.ToString();
            }

            return target;
        }

        /// <summary>
        /// Issues a POST call
        /// </summary>
        /// <param name="target">API method to call</param>
        /// <param name="kwargs">Arguments to append to URL</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>Raw API response</returns>
        public string Get(string target, NameValueCollection kwargs = null, bool needAuth = true)
        {
            target = PrepareGetTarget(target, kwargs);
            return Call("GET", target, null, needAuth);
        }


        /// <summary>
        /// Issues a POST call with an expected return type
        /// </summary>
        /// <typeparam name="T">Expected return type</typeparam>
        /// <param name="target">API method to call</param>
        /// <param name="kwargs">Arguments to append to URL</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>API response deserialized to T by JSON.Net</returns>
        public T Get<T>(string target, NameValueCollection kwargs = null, bool needAuth = true)
        {
            target = PrepareGetTarget(target, kwargs);
            return Call<T>("GET", target, null, needAuth);
        }

        #endregion

        #region POST

        /// <summary>
        /// Issues a POST call
        /// </summary>
        /// <param name="target">API method to call</param>
        /// <param name="data">Object to serialize and send as body</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>Raw API response</returns>
        public string Post(string target, object data, bool needAuth = true)
        {
            return Call("POST", target, JsonConvert.SerializeObject(data), needAuth);
        }

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
        /// <param name="data">Object to serialize and send as body</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>API response deserialized to T by JSON.Net</returns>
        public T Post<T>(string target, object data, bool needAuth = true)
        {
            return Call<T>("POST", target, JsonConvert.SerializeObject(data), needAuth);
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
        #endregion

        #region PUT
        /// <summary>
        /// Issues a PUT call
        /// </summary>
        /// <param name="target">API method to call</param>
        /// <param name="data">Object to serialize and send as body</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>Raw API response</returns>
        public string Put(string target, object data, bool needAuth = true)
        {
            return Call("PUT", target, JsonConvert.SerializeObject(data), needAuth);
        }

        /// <summary>
        /// Issues a POST call
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
        /// Issues a POST call
        /// </summary>
        /// <typeparam name="T">Expected return type</typeparam>
        /// <param name="target">API method to call</param>
        /// <param name="data">Object to serialize and send as body</param>
        /// <param name="needAuth">If true, send authentication headers</param>
        /// <returns>API response deserialized to T by JSON.Net</returns>
        public T Put<T>(string target, object data, bool needAuth = true)
        {
            return Call<T>("PUT", target, JsonConvert.SerializeObject(data), needAuth);
        }

        /// <summary>
        /// Issues a POST call
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

        #endregion

        public CredentialRequestResult RequestConsumerKey(CredentialRequest request)
        {
            return Post<CredentialRequestResult>("/auth/credential", request, false);
        }

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
        /// <exception cref="HttpException">When underlying request failed for network reason</exception>
        /// <exception cref="InvalidResponseException">when API response could not be decoded</exception>
        private string Call(string method, string path, string data = null, bool needAuth = true)
        {
            method = method.ToUpper();
            if (path.StartsWith("/"))
            {
                path = path.Substring(1);
            }
            string target = Endpoint + path;
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
                    String.Join("+", ApplicationSecret, ConsumerKey, method,
                        target, data, currentServerTimestamp);
                byte[] binaryHash = sha1Hasher.ComputeHash(Encoding.UTF8.GetBytes(toSign));
                string signature = string.Join("",
                    binaryHash.Select(x => x.ToString("X2"))).ToLower();

                headers.Add("X-Ovh-Consumer", ConsumerKey);
                headers.Add("X-Ovh-Timestamp", currentServerTimestamp.ToString());
                headers.Add("X-Ovh-Signature", "$1$" + signature);
            }

            string response = "";
            try
            {
                //NOTE: would be better to reuse some headers
                WebClient.Headers = headers;
                if (method != "GET")
                {
                    response = WebClient.UploadString(path, method, data ?? "");
                }
                else
                {
                    response = WebClient.DownloadString(path);
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse httpResponse = (HttpWebResponse)ex.Response;
                if (httpResponse == null)
                {
                    throw new HttpException("Low HTTP request failed error", ex);
                }
                HandleHttpError(httpResponse, ex);
            }
            return response;
        }

        private T Call<T>(string method, string path, string data = null, bool needAuth = true)
        {
            return JsonConvert.DeserializeObject<T>(Call(method, path, data, needAuth));
        }

        private void HandleHttpError(HttpWebResponse httpResponse, Exception ex)
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
                        throw new NotGrantedCallException(message);
                    }
                    else if (errorCode == "NOT_CREDENTIAL")
                    {
                        throw new NotCredentialException(message);
                    }
                    else if (errorCode == "INVALID_KEY")
                    {
                        throw new InvalidKeyException(message);
                    }
                    else if (errorCode == "INVALID_CREDENTIAL")
                    {
                        throw new InvalidCredentialException(message);
                    }
                    else if (errorCode == "FORBIDDEN")
                    {
                        throw new ForbiddenException(message);
                    }
                    else
                    {
                        throw new ApiException(message);
                    }
                case HttpStatusCode.NotFound:
                    throw new ResourceNotFoundException(message);
                case HttpStatusCode.BadRequest:
                    if (errorCode == "QUERY_TIME_OUT")
                    {
                        throw new StaleRequestException(message);
                    }
                    else
                    {
                        throw new BadParametersException(message);
                    }
                case HttpStatusCode.Conflict:
                    throw new ResourceConflictException(message);
                default:
                    throw new ApiException(message);
            }
            throw new HttpException("Low Http request failed", ex);
        }

        private JObject ExtractResponseObject(HttpWebResponse httpResponse)
        {
            var dataStream = httpResponse.GetResponseStream();
            int dataLength = (int)dataStream.Length;
            byte[] dataBuffer = new byte[dataLength];
            dataStream.Read(dataBuffer, 0, dataLength);
            dataStream.Close();
            string dataString = Encoding.UTF8.GetString(dataBuffer);
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
