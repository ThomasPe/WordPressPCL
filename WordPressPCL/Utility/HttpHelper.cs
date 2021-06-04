using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WordPressPCL.Models;
using WordPressPCL.Models.Exceptions;

namespace WordPressPCL.Utility
{
    /// <summary>
    /// Helper class encapsulates common HTTP requests methods
    /// </summary>
    public class HttpHelper
    {
        private static readonly HttpClient _defaultHttpClient = new HttpClient();
        private readonly HttpClient _httpClient;
        private readonly string _WordpressURI;

        /// <summary>
        /// JSON Web Token
        /// </summary>
        public string JWToken { get; set; }

        /// <summary>
        /// The Application Password to be used for authentication
        /// </summary>
        public string ApplicationPassword { get; set; }

        /// <summary>
        /// Authentication Method
        /// </summary>
        public AuthMethod AuthMethod { get; set; }

        /// <summary>
        /// The username to be used with the Application Password
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Function called when a HttpRequest response is read
        /// Executed before trying to convert json content to a TClass object.
        /// </summary>
        public Func<string, string> HttpResponsePreProcessing { get; set; }

        /// <summary>
        /// Serialization/Deserialization settings for Json.NET library
        /// https://www.newtonsoft.com/json/help/html/SerializationSettings.htm
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; set; }
        /// <summary>
        /// Headers returns by WP and http server from last response
        /// </summary>
        public HttpResponseHeaders LastResponseHeaders { get; set; }

        /// <summary>
        /// Constructor
        /// <paramref name="wordpressURI"/>
        /// </summary>
        /// <param name="wordpressURI">base WP REST API endpoint EX. http://demo.com/wp-json/ </param>
        public HttpHelper(string wordpressURI)
        {
            _httpClient = _defaultHttpClient;
            _WordpressURI = wordpressURI;

            // by default don't crash on missing member
            JsonSerializerSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
        }

        /// <summary>
        /// Constructor
        /// <paramref name="httpClient"/>
        /// </summary>
        /// <param name="httpClient">Http client which would be used for sending requests to the WordPress API endpoint.</param>
        public HttpHelper(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _WordpressURI = httpClient.BaseAddress.ToString();

            // by default don't crash on missing member
            JsonSerializerSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
        }

        internal async Task<TClass> GetRequestAsync<TClass>(string route, bool embed, bool isAuthRequired = false)
            where TClass : class
        {
            string embedParam = "";
            if (embed)
            {
                if (route.Contains("?"))
                    embedParam = "&_embed";
                else
                    embedParam = "?_embed";
            }

            HttpResponseMessage response;
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{_WordpressURI}{route}{embedParam}"))
            {
                SetAuthHeader(isAuthRequired, requestMessage);
                response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            }
            LastResponseHeaders = response.Headers;
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                if (HttpResponsePreProcessing != null)
                    responseString = HttpResponsePreProcessing(responseString);
                if (JsonSerializerSettings != null)
                    return JsonConvert.DeserializeObject<TClass>(responseString, JsonSerializerSettings);
                return JsonConvert.DeserializeObject<TClass>(responseString);
            }
            else
            {
                throw CreateUnexpectedResponseException(response, responseString);
            }
        }

        internal async Task<(TClass, HttpResponseMessage)> PostRequestAsync<TClass>(string route, HttpContent postBody, bool isAuthRequired = true)
            where TClass : class
        {

            HttpResponseMessage response;
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_WordpressURI}{route}"))
            {
                SetAuthHeader(isAuthRequired, requestMessage);
                requestMessage.Content = postBody;
                response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            }

            LastResponseHeaders = response.Headers;
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                if (HttpResponsePreProcessing != null)
                    responseString = HttpResponsePreProcessing(responseString);
                if (JsonSerializerSettings != null)
                    return (JsonConvert.DeserializeObject<TClass>(responseString, JsonSerializerSettings), response);
                return (JsonConvert.DeserializeObject<TClass>(responseString), response);
            }
            else
            {
                throw CreateUnexpectedResponseException(response, responseString);
            }
        }

        internal async Task<bool> DeleteRequestAsync(string route, bool isAuthRequired = true)
        {
            HttpResponseMessage response;
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"{_WordpressURI}{route}"))
            {
                SetAuthHeader(isAuthRequired, requestMessage);
                response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            }

            LastResponseHeaders = response.Headers;
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                throw CreateUnexpectedResponseException(response, responseString);
            }
        }

        private void SetAuthHeader(bool isAuthRequired, HttpRequestMessage requestMessage)
        {
            if (isAuthRequired)
            {
                if (AuthMethod == AuthMethod.JWT || AuthMethod == AuthMethod.JWTAuth)
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", JWToken);
                }
                else if (AuthMethod == AuthMethod.ApplicationPassword)
                {
                    var authToken = Encoding.ASCII.GetBytes($"{UserName}:{ApplicationPassword}");
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
                }
                else
                {
                    throw new WPException("Unsupported Authentication Method");
                }
            }
        }

        private static Exception CreateUnexpectedResponseException(HttpResponseMessage response, string responseString)
        {
            BadRequest badrequest;
            try
            {
                badrequest = JsonConvert.DeserializeObject<BadRequest>(responseString);
            }
            catch (JsonReaderException)
            {
                // the response is not a well formed bad request
                return new WPUnexpectedException(response, responseString);
            }
            return new WPException(badrequest.Message, badrequest);
        }
    }
}