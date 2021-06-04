using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using WordPressPCL.Models;
using WordPressPCL.Tests.Selfhosted.Utility;
using System.Linq;
using WordPressPCL.Utility;
using WordPressPCL.Models.Exceptions;

namespace WordPressPCL.Tests.Selfhosted
{
    [TestClass]
    public class Posts_Tests
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
        public async Task Posts_Create()
        {
            var post = new Post()
            {
                Title = new Title("Title 1"),
                Content = new Content("Content PostCreate")
            };
            var createdPost = await _clientAuth.Posts.CreateAsync(post);


            Assert.AreEqual(post.Content.Raw, createdPost.Content.Raw);
            Assert.IsTrue(createdPost.Content.Rendered.Contains(post.Content.Rendered));
        }

        [TestMethod]
        public async Task Posts_Read()
        {
            var posts = await _clientAuth.Posts.QueryAsync(new PostsQueryBuilder());
            Assert.IsNotNull(posts);
            Assert.AreNotEqual(posts.Count(), 0);

            var postsEdit = await _clientAuth.Posts.QueryAsync(new PostsQueryBuilder()
            {
                Context = Context.Edit,
                PerPage = 1,
                Page = 1
            }, true);
            Assert.AreEqual(1, postsEdit.Count());
            Assert.IsNotNull(postsEdit.FirstOrDefault());
            Assert.IsNotNull(postsEdit.FirstOrDefault().Content.Raw);
        }

        [TestMethod]
        public async Task Posts_Get()
        {
            var posts = await _client.Posts.GetAsync();
            Assert.IsNotNull(posts);
            Assert.AreNotEqual(posts.Count(), 0);
        }

        [TestMethod]
        public async Task Posts_Read_Embedded()
        {
            var posts = await _client.Posts.QueryAsync(new PostsQueryBuilder()
            {
                PerPage = 10,
                Page = 1,
                Embed = true
            }, false);
            Assert.IsNotNull(posts);

        }

        [TestMethod]
        public async Task Posts_Update()
        {
            var testContent = "Test" + new Random().Next();
            var posts = await _clientAuth.Posts.GetAllAsync();
            Assert.IsTrue(posts.Count() > 0);

            // edit first post and update it
            var post = await _clientAuth.Posts.GetByID(posts.First().Id);
            post.Content.Raw = testContent;
            var updatedPost = await _clientAuth.Posts.UpdateAsync(post);
            Assert.AreEqual(updatedPost.Content.Raw, testContent);
            Assert.IsTrue(updatedPost.Content.Rendered.Contains(testContent));
        }

        [TestMethod]
        public async Task Posts_Delete()
        {
            var post = new Post()
            {
                Title = new Title("Title 1"),
                Content = new Content("Content PostCreate")
            };
            var createdPost = await _clientAuth.Posts.CreateAsync(post);
            Assert.IsNotNull(createdPost);

            var resonse = await _clientAuth.Posts.Delete(createdPost.Id);
            Assert.IsTrue(resonse);

            await Assert.ThrowsExceptionAsync<WPException>(async () =>
            {
                var postById = await _clientAuth.Posts.GetByID(createdPost.Id);
            });

            // Post should be available in trash
            var queryBuilder = new PostsQueryBuilder()
            {
                Statuses = new Status[] { Status.Trash }, 
                PerPage = 100
            };
            var posts = await _clientAuth.Posts.QueryAsync(queryBuilder, true);

            var deletedPost = posts.Where(x => x.Id == createdPost.Id).FirstOrDefault();
            Assert.IsNotNull(deletedPost);
        }

        [TestMethod]
        public async Task Posts_Query()
        {
            var queryBuilder = new PostsQueryBuilder()
            {
                Page = 1,
                PerPage = 15,
                OrderBy = PostsOrderBy.Title,
                Order = Order.ASC,
                Statuses = new Status[] { Status.Publish },
                Embed = true
            };
            var queryresult = await _clientAuth.Posts.QueryAsync(queryBuilder);
            Assert.AreEqual("?page=1&per_page=15&orderby=title&status=publish&order=asc&_embed=true&context=view", queryBuilder.BuildQueryURL());
            Assert.IsNotNull(queryresult);
            Assert.AreNotSame(queryresult.Count(), 0);
        }
    }
}
