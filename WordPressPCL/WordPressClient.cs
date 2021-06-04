using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WordPressPCL.Client;
using WordPressPCL.Models;
using WordPressPCL.Models.Exceptions;
using WordPressPCL.Utility;

namespace WordPressPCL
{
    /// <summary>
    /// Main class containing the wrapper client with all public API endpoints.
    /// </summary>
    public class WordPressClient
    {
        private readonly HttpHelper _httpHelper;
        private const string DEFAULT_PATH = "wp/v2/";
        private readonly string _defaultPath;
        private const string _jwtPath = "jwt-auth/v1/";

        /// <summary>
        /// WordPressUri holds the WordPress API endpoint, e.g. "http://demo.wp-api.org/wp-json/wp/v2/"
        /// </summary>
		public string WordPressUri { get; private set; }

        /// <summary>
        /// Function called when a HttpRequest response to WordPress APIs are read
        /// Executed before trying to convert json content to a TClass object.
        /// </summary>
        public Func<string, string> HttpResponsePreProcessing
        {
            set
            {
                _httpHelper.HttpResponsePreProcessing = value;
            }
        }

        /// <summary>
        /// Serialization/Deserialization settings for Json.NET library
        /// https://www.newtonsoft.com/json/help/html/SerializationSettings.htm
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings
        {
            set
            {
                _httpHelper.JsonSerializerSettings = value;
            }
            get
            {
                return _httpHelper.JsonSerializerSettings;
            }
        }

        /// <summary>
        /// Authentication method
        /// </summary>
        public AuthMethod AuthMethod 
        {
            get => _httpHelper.AuthMethod;
            set => _httpHelper.AuthMethod = value;
        }

        /// <summary>
        /// The username to be used with the Application Password
        /// </summary>
        public string UserName {
            get => _httpHelper.UserName;
            set => _httpHelper.UserName = value;
        }

        //public string JWToken;
        /// <summary>
        /// Posts client interaction object
        /// </summary>
        public Posts Posts { get; }

        /// <summary>
        /// Comments client interaction object
        /// </summary>
        public Comments Comments { get; }

        /// <summary>
        /// Tags client interaction object
        /// </summary>
        public Tags Tags { get; }

        /// <summary>
        /// Users client interaction object
        /// </summary>
        public Users Users { get; }

        /// <summary>
        /// Media client interaction object
        /// </summary>
        public Media Media { get; }

        /// <summary>
        /// Categories client interaction object
        /// </summary>
        public Categories Categories { get; }

        /// <summary>
        /// Pages client interaction object
        /// </summary>
        public Pages Pages { get; }

        /// <summary>
        /// Taxonomies client interaction object
        /// </summary>
        public Taxonomies Taxonomies { get; }

        /// <summary>
        /// Post Types client interaction object
        /// </summary>
        public PostTypes PostTypes { get; }

        /// <summary>
        /// Post Statuses client interaction object
        /// </summary>
        public PostStatuses PostStatuses { get; }

        /// <summary>
        /// Custom Request client interaction object
        /// </summary>
        public CustomRequest CustomRequest { get; }

        /// <summary>
        ///     The WordPressClient holds all connection infos and provides methods to call WordPress APIs.
        /// </summary>
        /// <param name="uri">URI for WordPress API endpoint, e.g. "http://demo.wp-api.org/wp-json/"</param>
        /// <param name="defaultPath">Relative path to standard API endpoints, defaults to "wp/v2/"</param>
        public WordPressClient(string uri, string defaultPath = DEFAULT_PATH)
        {
            if (string.IsNullOrWhiteSpace(uri))
            {
                throw new ArgumentNullException(nameof(uri));
            }
            if (!uri.EndsWith("/", StringComparison.Ordinal))
            {
                uri += "/";
            }
            WordPressUri = uri;
            _defaultPath = defaultPath;

            _httpHelper = new HttpHelper(WordPressUri);
            Posts = new Posts(ref _httpHelper, _defaultPath);
            Comments = new Comments(ref _httpHelper, _defaultPath);
            Tags = new Tags(ref _httpHelper, _defaultPath);
            Users = new Users(ref _httpHelper, _defaultPath);
            Media = new Media(ref _httpHelper, _defaultPath);
            Categories = new Categories(ref _httpHelper, _defaultPath);
            Pages = new Pages(ref _httpHelper, _defaultPath);
            Taxonomies = new Taxonomies(ref _httpHelper, _defaultPath);
            PostTypes = new PostTypes(ref _httpHelper, _defaultPath);
            PostStatuses = new PostStatuses(ref _httpHelper, _defaultPath);
            CustomRequest = new CustomRequest(ref _httpHelper);
        }

        /// <summary>
        /// The WordPressClient holds all connection infos and provides methods to call WordPress APIs.
        /// </summary>
        /// <param name="httpClient">HttpClient with BaseAddress set which will be used for sending requests to the WordPress API endpoint.</param>
        /// <param name="defaultPath">Relative path to standard API endpoints, defaults to "wp/v2/"</param>
        public WordPressClient(HttpClient httpClient, string defaultPath = DEFAULT_PATH)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }
            string uri = httpClient.BaseAddress.ToString();
            if (!uri.EndsWith("/", StringComparison.Ordinal))
            {
                uri += "/";
            }
            WordPressUri = uri;
            _defaultPath = defaultPath;

            _httpHelper = new HttpHelper(httpClient);
            Posts = new Posts(ref _httpHelper, _defaultPath);
            Comments = new Comments(ref _httpHelper, _defaultPath);
            Tags = new Tags(ref _httpHelper, _defaultPath);
            Users = new Users(ref _httpHelper, _defaultPath);
            Media = new Media(ref _httpHelper, _defaultPath);
            Categories = new Categories(ref _httpHelper, _defaultPath);
            Pages = new Pages(ref _httpHelper, _defaultPath);
            Taxonomies = new Taxonomies(ref _httpHelper, _defaultPath);
            PostTypes = new PostTypes(ref _httpHelper, _defaultPath);
            PostStatuses = new PostStatuses(ref _httpHelper, _defaultPath);
            CustomRequest = new CustomRequest(ref _httpHelper);
        }

        #region Settings methods

        /// <summary>
        /// Get site settings
        /// </summary>
        /// <returns>Site settings</returns>
        public Task<Settings> GetSettings()
        {
            return _httpHelper.GetRequestAsync<Settings>($"{_defaultPath}settings", false, true);
        }

        /// <summary>
        /// Update site settings
        /// </summary>
        /// <param name="settings">Settings object</param>
        /// <returns>Updated settings</returns>
        public async Task<Settings> UpdateSettingsAsync(Settings settings)
        {
            var postBody = new StringContent(JsonConvert.SerializeObject(settings), Encoding.UTF8, "application/json");
            (var setting, _) = await _httpHelper.PostRequestAsync<Settings>($"{_defaultPath}settings", postBody).ConfigureAwait(false);
            return setting;
        }

        #endregion Settings methods

        #region auth methods

        /// <summary>
        /// Perform authentication by JWToken
        /// </summary>
        /// <param name="Username">username</param>
        /// <param name="Password">password</param>
        public async Task RequestJWTokenAsync(string Username, string Password)
        {
            var route = $"{_jwtPath}token";
            var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("username", Username),
                    new KeyValuePair<string, string>("password", Password)
                });
            if (AuthMethod == AuthMethod.JWT)
            {
                (JWTUser jwtUser, _) = await _httpHelper.PostRequestAsync<JWTUser>(route, formContent, false).ConfigureAwait(false);
                _httpHelper.JWToken = jwtUser?.Token;
            }
            else if (AuthMethod == AuthMethod.JWTAuth)
            {
                HttpResponsePreProcessing = RemoveEmptyData;
                (JWTResponse jwtResponse, _) = await _httpHelper.PostRequestAsync<JWTResponse>(route, formContent, false).ConfigureAwait(false);
                HttpResponsePreProcessing = null;
                _httpHelper.JWToken = jwtResponse?.Data?.Token;
            }
            else
            {
                throw new ArgumentException($"Authentication methode {AuthMethod} is not supported");
            }
        }

        /// <summary>
        /// Forget the JWT Auth Token, won't invalidate it serverside though
        /// </summary>
        public void Logout()
        {
            _httpHelper.JWToken = default;
            _httpHelper.ApplicationPassword = default;
        }

        /// <summary>
        /// Check if token is valid
        /// </summary>
        /// <returns>Result of checking</returns>
        public async Task<bool> IsValidJWTokenAsync()
        {
            var route = $"{_jwtPath}token/validate";
            try
            {
                if (AuthMethod == AuthMethod.JWT)
                {
                    (JWTUser jwtUser, HttpResponseMessage repsonse) = await _httpHelper.PostRequestAsync<JWTUser>(route, null, true).ConfigureAwait(false);
                    return repsonse.IsSuccessStatusCode;
                }
                else if (AuthMethod == AuthMethod.JWTAuth)
                {
                    HttpResponsePreProcessing = RemoveEmptyData;
                    (JWTResponse jwtResponse, _) = await _httpHelper.PostRequestAsync<JWTResponse>(route, null, true).ConfigureAwait(false);
                    HttpResponsePreProcessing = null;
                    return jwtResponse.Success;
                }
                else
                {
                    throw new ArgumentException($"Authentication methode {AuthMethod} is not supported");
                }
            }
            catch (WPException)
            {
                return false;
            }
        }

        /// <summary>
        /// Removes an empty data field in a Json string, e.g. when checking validity of token
        /// </summary>
        /// <param name="response">Json response input string</param>
        /// <returns>Json response output string</returns>
        private string RemoveEmptyData(string response)
        {
            JObject jo = JObject.Parse(response);
            if (!jo.SelectToken("data").HasValues)
            {
                jo.Remove("data");
            }
            return jo.ToString();
        }

        /// <summary>
        /// Sets an existing JWToken
        /// </summary>
        /// <param name="token"></param>
        public void SetJWToken(string token)
        {
            _httpHelper.JWToken = token;
        }

        /// <summary>
        /// Gets the JWToken from the client
        /// </summary>
        /// <returns></returns>
        public string GetToken()
        {
            return _httpHelper.JWToken;
        }

        /// <summary>
        /// Store Application Password in the Client
        /// </summary>
        /// <param name="applictionPassword"></param>
        public void SetApplicationPassword(string applictionPassword)
        {
            _httpHelper.ApplicationPassword = applictionPassword;
        }

        #endregion auth methods
    }
}