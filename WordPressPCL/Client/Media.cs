using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WordPressPCL.Interfaces;
using WordPressPCL.Models;
using WordPressPCL.Utility;

namespace WordPressPCL.Client
{
    /// <summary>
    /// Client class for interaction with Media endpoint WP REST API
    /// </summary>
    public class Media : IUpdateOperation<MediaItem>, IReadOperation<MediaItem>, IDeleteOperation, IQueryOperation<MediaItem, MediaQueryBuilder>
    {
        #region Init

        private readonly string _defaultPath;
        private const string _methodPath = "media";
        private readonly HttpHelper _httpHelper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="HttpHelper">reference to HttpHelper class for interaction with HTTP</param>
        /// <param name="defaultPath">path to site, EX. http://demo.com/wp-json/ </param>
        public Media(ref HttpHelper HttpHelper, string defaultPath)
        {
            _httpHelper = HttpHelper;
            _defaultPath = defaultPath;
        }

        #endregion Init

        /// <summary>
        /// Create Media entity with attachment
        /// </summary>
        /// <param name="fileStream">stream with file content</param>
        /// <param name="filename">Name of file in WP Media Library</param>
        /// <param name="mimeType">Override for automatic mime type detection</param>
        /// <returns>Created media object</returns>
        public async Task<MediaItem> Create(Stream fileStream, string filename, string mimeType = null)
        {
            if (string.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            using (StreamContent content = new StreamContent(fileStream))
            {
                if (string.IsNullOrEmpty(mimeType))
                {
                    string extension = filename.Split('.').Last();
                    content.Headers.TryAddWithoutValidation("Content-Type", MimeTypeHelper.GetMIMETypeFromExtension(extension));
                }
                else
                {
                    content.Headers.TryAddWithoutValidation("Content-Type", mimeType);
                }
                content.Headers.TryAddWithoutValidation("Content-Disposition", $"attachment; filename={filename}");
                return (await _httpHelper.PostRequest<MediaItem>($"{_defaultPath}{_methodPath}", content).ConfigureAwait(false)).Item1;
            }
        }

#if NETSTANDARD2_0
        /// <summary>
        /// Create Media entity with attachment
        /// </summary>
        /// <param name="filePath">Local Path to file</param>
        /// <param name="filename">Name of file in WP Media Library</param>
        /// <returns>Created media object</returns>
        /// <param name="mimeType">Override for automatic mime type detection</param>
        public async Task<MediaItem> Create(string filePath, string filename, string mimeType = null)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (string.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            if (File.Exists(filePath))
            {
                using (StreamContent content = new StreamContent(File.OpenRead(filePath)))
                {
                    if (string.IsNullOrEmpty(mimeType))
                    {
                        string extension = filename.Split('.').Last();
                        content.Headers.TryAddWithoutValidation("Content-Type", MimeTypeHelper.GetMIMETypeFromExtension(extension));
                    }
                    else
                    {
                        content.Headers.TryAddWithoutValidation("Content-Type", mimeType);
                    }
                    content.Headers.TryAddWithoutValidation("Content-Disposition", $"attachment; filename={filename}");
                    return (await _httpHelper.PostRequest<MediaItem>($"{_defaultPath}{_methodPath}", content).ConfigureAwait(false)).Item1;
                }
            }
            else
            {
                throw new FileNotFoundException($"{filePath} was not found");
            }
        }
#endif

        /// <summary>
        /// Delete Entity
        /// </summary>
        /// <param name="ID">Entity Id</param>
        /// <returns>Result of operation</returns>
        public Task<bool> Delete(int ID)
        {
            return _httpHelper.DeleteRequest($"{_defaultPath}{_methodPath}/{ID}?force=true");
        }

        /// <summary>
        /// Get latest
        /// </summary>
        /// <param name="embed">include embed info</param>
        /// <param name="useAuth">Send request with authentication header</param>
        /// <returns>Latest media items</returns>
        public Task<IEnumerable<MediaItem>> Get(bool embed = false, bool useAuth = false)
        {
            return _httpHelper.GetRequest<IEnumerable<MediaItem>>($"{_defaultPath}{_methodPath}", embed, useAuth);
        }

        /// <summary>
        /// Get All
        /// </summary>
        /// <param name="embed">Include embed info</param>
        /// <param name="useAuth">Send request with authentication header</param>
        /// <returns>List of all result</returns>
        public async Task<IEnumerable<MediaItem>> GetAll(bool embed = false, bool useAuth = false)
        {
            //100 - Max posts per page in WordPress REST API, so this is hack with multiple requests
            List<MediaItem> entities = new List<MediaItem>();
            entities = (await _httpHelper.GetRequest<IEnumerable<MediaItem>>($"{_defaultPath}{_methodPath}?per_page=100&page=1", embed, useAuth).ConfigureAwait(false))?.ToList<MediaItem>();
            if (_httpHelper.LastResponseHeaders.Contains("X-WP-TotalPages") && Convert.ToInt32(_httpHelper.LastResponseHeaders.GetValues("X-WP-TotalPages").FirstOrDefault()) > 1)
            {
                int totalpages = Convert.ToInt32(_httpHelper.LastResponseHeaders.GetValues("X-WP-TotalPages").FirstOrDefault());
                for (int page = 2; page <= totalpages; page++)
                {
                    entities.AddRange((await _httpHelper.GetRequest<IEnumerable<MediaItem>>($"{_defaultPath}{_methodPath}?per_page=100&page={page}", embed, useAuth).ConfigureAwait(false))?.ToList<MediaItem>());
                }
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
        public Task<MediaItem> GetByID(object ID, bool embed = false, bool useAuth = false)
        {
            return _httpHelper.GetRequest<MediaItem>($"{_defaultPath}{_methodPath}/{ID}", embed, useAuth);
        }

        /// <summary>
        /// Create a parametrized query and get a result
        /// </summary>
        /// <param name="queryBuilder">Query builder with specific parameters</param>
        /// <param name="useAuth">Send request with authentication header</param>
        /// <returns>List of filtered result</returns>
        public async Task<QueryResult<MediaItem>> Query(MediaQueryBuilder queryBuilder, bool useAuth = false)
        {
            var results = await _httpHelper.GetRequest<IEnumerable<MediaItem>>($"{_defaultPath}{_methodPath}{queryBuilder.BuildQueryURL()}", false, useAuth);
            int total = System.Convert.ToInt32(_httpHelper.LastResponseHeaders.GetValues("X-WP-Total").FirstOrDefault());
            int totalpages = System.Convert.ToInt32(_httpHelper.LastResponseHeaders.GetValues("X-WP-TotalPages").FirstOrDefault());
            return new QueryResult<MediaItem>(results, total, totalpages);
        }

        /// <summary>
        /// Update Entity
        /// </summary>
        /// <param name="Entity">Entity object</param>
        /// <returns>Updated object</returns>
        public async Task<MediaItem> Update(MediaItem Entity)
        {
            var entity = _httpHelper.JsonSerializerSettings == null ? JsonConvert.SerializeObject(Entity) : JsonConvert.SerializeObject(Entity, _httpHelper.JsonSerializerSettings);
            var postBody = new StringContent(entity, Encoding.UTF8, "application/json");
            return (await _httpHelper.PostRequest<MediaItem>($"{_defaultPath}{_methodPath}/{(Entity as Base)?.Id}", postBody).ConfigureAwait(false)).Item1;
        }
    }
}