using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WordPressPCL.Tests.Selfhosted.Utility;
using System.Threading.Tasks;
using WordPressPCL.Models;
using System.Linq;

namespace WordPressPCL.Tests.Selfhosted
{
    [TestClass]
    public class Basic_Tests
    {
        private static WordPressClient _client;
        private static WordPressClient _clientAuth;

        [ClassInitialize]
        public static async Task Init(TestContext testContext)
        {
            _client = ClientHelper.GetWordPressClient();
            _clientAuth = await ClientHelper.GetAuthenticatedWordPressClient(testContext);
        }

        [TestMethod]
        public async Task BasicSetupTest()
        {
            // Initialize
            Assert.IsNotNull(_client);
            // Posts
            var posts = await _client.Posts.GetAllAsync();
            Assert.IsNotNull(posts);
        }

        [TestMethod]
        public async Task GetFirstPostTest()
        {
            // Initialize
            var posts = await _client.Posts.GetAllAsync();
            var post = await _client.Posts.GetByID(posts.First().Id);
            Assert.IsTrue(posts.First().Id == post.Id);
            Assert.IsTrue(!String.IsNullOrEmpty(posts.First().Content.Rendered));
        }

        [TestMethod]
        public async Task GetStickyPosts()
        {
            // Initialize
            var posts = await _client.Posts.GetStickyPosts();

            foreach (Post post in posts)
            {
                Assert.IsTrue(post.Sticky);
            }
        }

        [TestMethod]
        public async Task GetPostsByCategory()
        {
            // This CategoryID MUST exists at ApiCredentials.WordPressUri
            int category = 1;
            // Initialize
            var posts = await _client.Posts.GetPostsByCategory(category);

            foreach (Post post in posts)
            {
                Assert.IsTrue(post.Categories.ToList().Contains(category));
            }
        }

        [TestMethod]
        public async Task GetPostsByTag()
        {
            // This TagID MUST exists at ApiCredentials.WordPressUri
            int tag = 12;
            // Initialize
            var posts = await _client.Posts.GetPostsByTag(tag);

            foreach (Post post in posts)
            {
                Assert.IsTrue(post.Tags.ToList().Contains(tag));
            }
        }

        [TestMethod]
        public async Task GetPostsByAuthor()
        {
            // This AuthorID MUST exists at ApiCredentials.WordPressUri
            int author = 2;
            // Initialize
            var posts = await _client.Posts.GetPostsByAuthor(author);

            foreach (Post post in posts)
            {
                Assert.IsTrue(post.Author == author);
            }
        }

        [TestMethod]
        public async Task GetPostsBySearch()
        {
            // This search term MUST be used at least once
            string search = "hello";
            // Initialize
            var posts = await _client.Posts.GetPostsBySearch(search);

            foreach (Post post in posts)
            {
                bool containsOnContentOrTitle = false;

                if (post.Content.Rendered.ToUpper().Contains(search.ToUpper()) || post.Title.Rendered.ToUpper().Contains(search.ToUpper()))
                {
                    containsOnContentOrTitle = true;
                }

                Assert.IsTrue(containsOnContentOrTitle);
            }
        }

        [TestMethod]
        public async Task Authorize()
        {
            var validToken = await _clientAuth.IsValidJWTokenAsync();
            Assert.IsTrue(validToken);
        }
    }
}
