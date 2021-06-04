using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordPressPCL.Models;
using WordPressPCL.Utility;

namespace WordPressPCL.Client
{
    /// <summary>
    /// Client class for interaction with Comments endpoint WP REST API
    /// </summary>

    public class Comments : CRUDOperation<Comment, CommentsQueryBuilder>
    {
        #region Init

        private const string _methodPath = "comments";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="HttpHelper">reference to HttpHelper class for interaction with HTTP</param>
        /// <param name="defaultPath">path to site, EX. http://demo.com/wp-json/ </param>
        public Comments(ref HttpHelper HttpHelper, string defaultPath) : base(ref HttpHelper, defaultPath, _methodPath)
        {
        }

        #endregion Init

        #region Custom

        /// <summary>
        /// Get comments for Post
        /// </summary>
        /// <param name="PostID">Post id</param>
        /// <param name="embed">include embed info</param>
        /// <param name="useAuth">Send request with authentication header</param>
        /// <returns>List of comments for post</returns>
        public Task<IEnumerable<Comment>> GetCommentsForPost(int PostID, bool embed = false, bool useAuth = false)
        {
            return HttpHelper.GetRequestAsync<IEnumerable<Comment>>($"{DefaultPath}{_methodPath}?post={PostID}", embed, useAuth);
        }

        /// <summary>
        /// Get all comments for Post
        /// </summary>
        /// <param name="PostID">Post id</param>
        /// <param name="embed">include embed info</param>
        /// <param name="useAuth">Send request with authentication header</param>
        /// <returns>List of comments for post</returns>
        public async Task<IEnumerable<Comment>> GetAllCommentsForPostAsync(int PostID, bool embed = false, bool useAuth = false)
        {
            //100 - Max comments per page in WordPress REST API, so this is hack with multiple requests
            List<Comment> comments = (await HttpHelper.GetRequestAsync<IEnumerable<Comment>>($"{DefaultPath}{_methodPath}?post={PostID}&per_page=100&page=1", embed, useAuth).ConfigureAwait(false))?.ToList();
            if (HttpHelper.LastResponseHeaders.Contains("X-WP-TotalPages") &&
                int.TryParse(HttpHelper.LastResponseHeaders.GetValues("X-WP-TotalPages").FirstOrDefault(), out int totalPages) &&
                totalPages > 1)
            {
                for (int page = 2; page <= totalPages; page++)
                {
                    comments.AddRange((await HttpHelper.GetRequestAsync<IEnumerable<Comment>>($"{DefaultPath}{_methodPath}?post={PostID}&per_page=100&page={page}", embed, useAuth).ConfigureAwait(false))?.ToList());
                }
            }
            return comments;
        }

        /// <summary>
        /// Force deletion of comments
        /// </summary>
        /// <param name="ID">Comment Id</param>
        /// <param name="force">force deletion</param>
        /// <returns>Result of operation</returns>
        public Task<bool> Delete(int ID, bool force = false)
        {
            return HttpHelper.DeleteRequestAsync($"{DefaultPath}{_methodPath}/{ID}?force={force.ToString().ToLower()}");
        }

        #endregion Custom
    }
}