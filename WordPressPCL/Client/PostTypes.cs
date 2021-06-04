using System.Collections.Generic;
using System.Threading.Tasks;
using WordPressPCL.Interfaces;
using WordPressPCL.Models;
using WordPressPCL.Utility;

namespace WordPressPCL.Client
{
    /// <summary>
    /// Client class for interaction with Post Types endpoint WP REST API
    /// </summary>
    public class PostTypes : IReadOperation<PostType>
    {
        private HttpHelper _httpHelper;
        private string _defaultPath;
        private const string _methodPath = "types";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpHelper">reference to HttpHelper class for interaction with HTTP</param>
        /// <param name="defaultPath">path to site, EX. http://demo.com/wp-json/ </param>
        public PostTypes(ref HttpHelper httpHelper, string defaultPath)
        {
            _httpHelper = httpHelper;
            _defaultPath = defaultPath;
        }

        /// <summary>
        /// Get latest
        /// </summary>
        /// <param name="embed">Include embed info</param>
        /// <param name="useAuth">Send request with authentication header</param>
        /// <returns>List of latest PostTypes</returns>
        public async Task<IEnumerable<PostType>> GetAsync(bool embed = false, bool useAuth = false)
        {
            List<PostType> entities = new List<PostType>();
            Dictionary<string, PostType> entities_page = (await _httpHelper.GetRequestAsync<Dictionary<string, PostType>>($"{_defaultPath}{_methodPath}", embed, useAuth).ConfigureAwait(false));
            foreach (var ent in entities_page)
            {
                entities.Add(ent.Value);
            }
            return entities;
        }

        /// <summary>
        /// Get All
        /// </summary>
        /// <param name="embed">Include embed info</param>
        /// <param name="useAuth">Send request with authentication header</param>
        /// <returns>List of all PostTypes</returns>
        public async Task<IEnumerable<PostType>> GetAllAsync(bool embed = false, bool useAuth = false)
        {
            //100 - Max posts per page in WordPress REST API, so this is hack with multiple requests
            List<PostType> entities = new List<PostType>();
            Dictionary<string, PostType> entities_page = (await _httpHelper.GetRequestAsync<Dictionary<string, PostType>>($"{_defaultPath}{_methodPath}", embed, useAuth).ConfigureAwait(false));
            foreach (var ent in entities_page)
            {
                entities.Add(ent.Value);
            }
            return entities;
        }

        /// <summary>
        /// Get Entity by Id
        /// </summary>
        /// <param name="ID">ID</param>
        /// <param name="embed">include embed info</param>
        /// <param name="useAuth">Send request with authentication header</param>
        /// <returns>Entity by Id</returns>
        public Task<PostType> GetByID(object ID, bool embed = false, bool useAuth = false)
        {
            return _httpHelper.GetRequestAsync<PostType>($"{_defaultPath}{_methodPath}/{ID}", embed, useAuth);
        }
    }
}