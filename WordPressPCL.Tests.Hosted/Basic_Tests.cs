using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WordPressPCL.Tests.Hosted.Utility;
using System.Threading.Tasks;
using WordPressPCL.Models;
using System.Linq;

namespace WordPressPCL.Hosted
{
    [TestClass]
    public class Basic_Tests
    {
        private static WordPressClient _client;

        [ClassInitialize]
        public static void Init(TestContext testContext)
        {
            _client = ClientHelper.GetWordPressClient();
        }

        [TestMethod]
        public async Task Hosted_BasicSetupTest()
        {
            // Initialize
            Assert.IsNotNull(_client);
            // Posts
            var posts = await _client.Posts.GetAllAsync();
            Assert.AreNotEqual(posts.Count(), 0);
            Assert.IsNotNull(posts);
        }

        [TestMethod]
        public async Task Hosted_GetFirstPostTest()
        {
            // Initialize
            var posts = await _client.Posts.GetAllAsync();
            var post = await _client.Posts.GetByID(posts.First().Id);
            Assert.IsTrue(posts.First().Id == post.Id);
            Assert.IsTrue(!String.IsNullOrEmpty(posts.First().Content.Rendered));
        }

        [TestMethod]
        public async Task Hosted_GetStickyPosts()
        {
            // Initialize
            var posts = await _client.Posts.GetStickyPosts();

            foreach (Post post in posts)
            {
                Assert.IsTrue(post.Sticky);
            }
        }

        [TestMethod]
        public async Task Hosted_GetPostsByCategory()
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
        public async Task Hosted_GetPostsByTag()
        {
            var tags = await _client.Tags.GetAsync();
            int tagId = tags.FirstOrDefault().Id;

            // Initialize
            var posts = await _client.Posts.GetPostsByTag(tagId);
            Assert.AreNotEqual(0, posts.Count());
            foreach (Post post in posts)
            {
                Assert.IsTrue(post.Tags.ToList().Contains(tagId));
            }
        }

        [TestMethod]
        public async Task Hosted_GetPostsByAuthor()
        {
            // This AuthorID MUST exists at ApiCredentials.WordPressUri
            int author = 3722200;
            // Initialize
            var posts = await _client.Posts.GetPostsByAuthor(author);
            Assert.AreNotEqual(0, posts.Count());
            foreach (Post post in posts)
            {
                Assert.IsTrue(post.Author == author);
            }
        }

        [TestMethod]
        public async Task Hosted_GetPostsBySearch()
        {
            // This search term MUST be used at least once
            string search = "hello";
            // Initialize
            var posts = await _client.Posts.GetPostsBySearch(search);
            Assert.AreNotEqual(0, posts.Count());
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
    }
}
