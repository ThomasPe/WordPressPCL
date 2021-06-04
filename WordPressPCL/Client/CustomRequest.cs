using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WordPressPCL.Utility;

namespace WordPressPCL.Client
{
    /// <summary>
    /// Class to create custom requests
    /// </summary>
    public class CustomRequest
    {
        private readonly HttpHelper _httpHelper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpHelper">HttpHelper class to operate with Http methods</param>
        public CustomRequest(ref HttpHelper httpHelper)
        {
            _httpHelper = httpHelper;
        }

        /// <summary>
        /// Create object
        /// </summary>
        /// <typeparam name="TInput">type of input object</typeparam>
        /// <typeparam name="TOutput">type of result object</typeparam>
        /// <param name="route">path to exec request</param>
        /// <param name="Entity">object for creation</param>
        /// <returns>Created object</returns>
        public async Task<TOutput> CreateAsync<TInput, TOutput>(string route, TInput Entity) where TOutput : class
        {
            var entity = _httpHelper.JsonSerializerSettings == null ? JsonConvert.SerializeObject(Entity) : JsonConvert.SerializeObject(Entity, _httpHelper.JsonSerializerSettings);
            StringContent sc = new StringContent(entity, Encoding.UTF8, "application/json");
            return (await _httpHelper.PostRequestAsync<TOutput>(route, sc).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Delete object
        /// </summary>
        /// <param name="route">path to exec delete request</param>
        /// <returns>Result of deletion</returns>
        public Task<bool> Delete(string route)
        {
            return _httpHelper.DeleteRequestAsync(route, true);
        }

        /// <summary>
        /// Get object/s
        /// </summary>
        /// <typeparam name="TClass">type of object</typeparam>
        /// <param name="route">path to exec request</param>
        /// <param name="embed">is get embed params</param>
        /// <param name="useAuth">i use auth</param>
        /// <returns>List of objects</returns>
        public Task<TClass> Get<TClass>(string route, bool embed = false, bool useAuth = false) where TClass : class
        {
            return _httpHelper.GetRequestAsync<TClass>(route, embed, useAuth);
        }

        /// <summary>
        /// Update object
        /// </summary>
        /// <typeparam name="TInput">type of input object</typeparam>
        /// <typeparam name="TOutput">type of result object</typeparam>
        /// <param name="route">path to exec request</param>
        /// <param name="Entity">object for update</param>
        /// <returns>Updated object</returns>
        public Task<TOutput> Update<TInput, TOutput>(string route, TInput Entity) where TOutput : class
        {
            return this.CreateAsync<TInput, TOutput>(route, Entity);
        }
    }
}