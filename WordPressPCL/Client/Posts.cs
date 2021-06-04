using System.Collections.Generic;
using System.Threading.Tasks;
using WordPressPCL.Models;
using WordPressPCL.Utility;

namespace WordPressPCL.Client
{
    /// <summary>
    /// Client class for interaction with Posts endpoint WP REST API
    /// </summary>
    public class Posts : CRUDOperation<Post, PostsQueryBuilder>
    {
        #region Init

        private const string _methodPath = "posts";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="HttpHelper">reference to HttpHelper class for interaction with HTTP</param>
        /// <param name="defaultPath">path to site, EX. http://demo.com/wp-json/ </param>
        public Posts(ref HttpHelper HttpHelper, string defaultPath) : base(ref HttpHelper, defaultPath, _methodPath)
        {
        }

        #endregion Init

        #region Custom

        /// <summary>
        /// Get sticky posts
        /// </summary>
        /// <param name="embed">include embed info</param>
        /// <param name="useAuth">Send request with authentication header</param>
        /// <returns>List of posts</returns>
        public Task<IEnumerable<Post>> GetStickyPosts(bool embed = false, bool useAuth = false)
        {
            // default values
            // int page = 1, int per_page = 10, int offset = 0, Post.OrderBy orderby = Post.OrderBy.date
            return HttpHelper.GetRequestAsync<IEnumerable<Post>>($"{DefaultPath}{_methodPath}?sticky=true", embed, useAuth);
        }

        /// <summary>
        /// Get posts by category
        /// </summary>
        /// <param name="categoryId">Category Id</param>
        /// <param name="embed">include embed info</param>
        /// <param name="useAuth">Send request with authentication header</param>
        /// <returns>List of posts</returns>
        public Task<IEnumerable<Post>> GetPostsByCategory(int categoryId, bool embed = false, bool useAuth = false)
        {
            // default values
            // int page = 1, int per_page = 10, int offset = 0, Post.OrderBy orderby = Post.OrderBy.date
            return HttpHelper.GetRequestAsync<IEnumerable<Post>>($"{DefaultPath}{_methodPath}?categories={categoryId}", embed, useAuth);
        }

        /// <summary>
        /// Get posts by tag
        /// </summary>
        /// <param name="tagId">Tag Id</param>
        /// <param name="embed">include embed info</param>
        /// <param name="useAuth">Send request with authentication header</param>
        /// <returns>List of posts</returns>
        public Task<IEnumerable<Post>> GetPostsByTag(int tagId, bool embed = false, bool useAuth = false)
        {
            // default values
            // int page = 1, int per_page = 10, int offset = 0, Post.OrderBy orderby = Post.OrderBy.date
            return HttpHelper.GetRequestAsync<IEnumerable<Post>>($"{DefaultPath}{_methodPath}?tags={tagId}", embed, useAuth);
        }

        /// <summary>
        /// Get posts by its author
        /// </summary>
        /// <param name="authorId">Author id</param>
        /// <param name="embed">include embed info</param>
        /// <param name="useAuth">Send request with authentication header</param>
        /// <returns>List of posts</returns>
        public Task<IEnumerable<Post>> GetPostsByAuthor(int authorId, bool embed = false, bool useAuth = false)
        {
            // default values
            // int page = 1, int per_page = 10, int offset = 0, Post.OrderBy orderby = Post.OrderBy.date
            return HttpHelper.GetRequestAsync<IEnumerable<Post>>($"{DefaultPath}{_methodPath}?author={authorId}", embed, useAuth);
        }

        /// <summary>
        /// Get posts by search term
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <param name="embed">include embed info</param>
        /// <param name="useAuth">Send request with authentication header</param>
        /// <returns>List of posts</returns>
        public Task<IEnumerable<Post>> GetPostsBySearch(string searchTerm, bool embed = false, bool useAuth = false)
        {
            // default values
            // int page = 1, int per_page = 10, int offset = 0, Post.OrderBy orderby = Post.OrderBy.date
            return HttpHelper.GetRequestAsync<IEnumerable<Post>>($"{DefaultPath}{_methodPath}?search={searchTerm}", embed, useAuth);
        }

        /// <summary>
        /// Delete post with force deletion
        /// </summary>
        /// <param name="ID">Post id</param>
        /// <param name="force">force deletion</param>
        /// <returns>Result of operation</returns>
        public Task<bool> Delete(int ID, bool force = false)
        {
            return HttpHelper.DeleteRequestAsync($"{DefaultPath}{_methodPath}/{ID}?force={force.ToString().ToLower()}");
        }

        /// <summary>
        /// Get instance ob object to manipulate with post revisions
        /// </summary>
        /// <param name="postId">ID of parent Post</param>
        /// <returns>Post revisions object</returns>
        public PostRevisions Revisions(int postId)
        {
            return new PostRevisions(ref _httpHelper, DefaultPath, postId);
        }

        #endregion Custom
    }
}