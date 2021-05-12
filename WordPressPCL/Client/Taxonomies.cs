using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordPressPCL.Interfaces;
using WordPressPCL.Models;
using WordPressPCL.Utility;

namespace WordPressPCL.Client
{
    /// <summary>
    /// Client class for interaction with Taxonomies endpoint WP REST API
    /// </summary>
    public class Taxonomies : IReadOperation<Taxonomy>, IQueryOperation<Taxonomy, TaxonomiesQueryBuilder>
    {
        private HttpHelper _httpHelper;
        private string _defaultPath;
        private const string _methodPath = "taxonomies";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpHelper">reference to HttpHelper class for interaction with HTTP</param>
        /// <param name="defaultPath">path to site, EX. http://demo.com/wp-json/ </param>
        public Taxonomies(ref HttpHelper httpHelper, string defaultPath)
        {
            _httpHelper = httpHelper;
            _defaultPath = defaultPath;
        }
        /// <summary>
        /// Get latest
        /// </summary>
        /// <param name="embed">include embed info</param>
        /// <param name="useAuth">Send request with authentication header</param>
        /// <returns>Get latest taxonomies</returns>
        public async Task<IEnumerable<Taxonomy>> Get(bool embed = false, bool useAuth = false)
        {
            List<Taxonomy> entities = new List<Taxonomy>();
            Dictionary<string, Taxonomy> entities_page = (await _httpHelper.GetRequest<Dictionary<string, Taxonomy>>($"{_defaultPath}{_methodPath}", embed, useAuth).ConfigureAwait(false));
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
        /// <returns>List of all result</returns>
        public async Task<IEnumerable<Taxonomy>> GetAll(bool embed = false, bool useAuth = false)
        {
            //100 - Max posts per page in WordPress REST API, so this is hack with multiple requests
            List<Taxonomy> entities = new List<Taxonomy>();
            Dictionary<string, Taxonomy> entities_page = (await _httpHelper.GetRequest<Dictionary<string, Taxonomy>>($"{_defaultPath}{_methodPath}", embed, useAuth).ConfigureAwait(false));
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
        public Task<Taxonomy> GetByID(object ID, bool embed = false, bool useAuth = false)
        {
            return _httpHelper.GetRequest<Taxonomy>($"{_defaultPath}{_methodPath}/{ID}", embed, useAuth);
        }

        /// <summary>
        /// Create a parametrized query and get a result
        /// </summary>
        /// <param name="queryBuilder">Query builder with specific parameters</param>
        /// <param name="useAuth">Send request with authentication header</param>
        /// <returns>List of filtered result</returns>
        public async Task<QueryResult<Taxonomy>> Query(TaxonomiesQueryBuilder queryBuilder, bool useAuth = false)
        {
            List<Taxonomy> entities = new List<Taxonomy>();
            var entities_dict = await _httpHelper.GetRequest<Dictionary<string, Taxonomy>>($"{_defaultPath}{_methodPath}{queryBuilder.BuildQueryURL()}", false, useAuth).ConfigureAwait(false);
            foreach (var ent in entities_dict)
            {
                entities.Add(ent.Value);
            }
            int total = System.Convert.ToInt32(_httpHelper.LastResponseHeaders.GetValues("X-WP-Total").FirstOrDefault());
            int totalpages = System.Convert.ToInt32(_httpHelper.LastResponseHeaders.GetValues("X-WP-TotalPages").FirstOrDefault());
            return new QueryResult<Taxonomy>(entities, total, totalpages);
        }
    }
}