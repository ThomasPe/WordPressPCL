using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WordPressPCL.Tests.Selfhosted.Utility;
using System.Threading.Tasks;
using WordPressPCL.Models;
using WordPressPCL.Utility;
using System.Linq;
using WordPressPCL.Models.Exceptions;

namespace WordPressPCL.Tests.Selfhosted
{
    [TestClass]
    public class Comments_Tests
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
        public async Task Comments_Create()
        {
            var posts = await _clientAuth.Posts.GetAllAsync();
            var postId = posts.First().Id;

            var me = await _clientAuth.Users.GetCurrentUser();

            // Create random content to prevent duplicate commment errors
            var random = new Random();
            var content = $"TestComment {random.Next(0, 10000)}";
            var comment = new Comment()
            {
                Content = new Content(content),
                PostId = postId,
                AuthorId = me.Id,
                AuthorEmail = "test@test.com",
                AuthorName = me.Name
            };
            var resultComment = await _clientAuth.Comments.CreateAsync(comment);
            Assert.IsNotNull(resultComment);

            // Posting same comment twice should fail
            await Assert.ThrowsExceptionAsync<WPException>(async () =>
            {
                var secondResultComment = await _clientAuth.Comments.CreateAsync(comment);
            });
        }
        [TestMethod]
        public async Task Comments_Read()
        {
            var comments = await _client.Comments.GetAllAsync();

            if (comments.Count() == 0)
            {
                Assert.Inconclusive("no comments to test");
            }

            foreach (var comment in comments)
            {
                // test Date parsing was successfull
                Assert.IsNotNull(comment.Date);
                Assert.AreNotEqual(DateTime.Now, comment.Date);
                Assert.AreNotEqual(DateTime.MaxValue, comment.Date);
                Assert.AreNotEqual(DateTime.MinValue, comment.Date);

                Assert.IsNotNull(comment.DateGmt);
                Assert.AreNotEqual(DateTime.Now, comment.DateGmt);
                Assert.AreNotEqual(DateTime.MaxValue, comment.DateGmt);
                Assert.AreNotEqual(DateTime.MinValue, comment.DateGmt);
            }
        }

        [TestMethod]
        public async Task Comments_Get()
        {
            var comments = await _client.Comments.GetAsync();

            if (comments.Count() == 0)
            {
                Assert.Inconclusive("no comments to test");
            }
            Assert.IsNotNull(comments);

        }

        [TestMethod]
        public async Task Comments_Update()
        {
            var me = await _clientAuth.Users.GetCurrentUser();
            var queryBuilder = new CommentsQueryBuilder()
            {
                Authors = new int[] { me.Id }
            };
            var comments = await _clientAuth.Comments.QueryAsync(queryBuilder, true);
            var comment = comments.FirstOrDefault();
            if (comment == null)
            {
                Assert.Inconclusive();
            }
            var random = new Random();
            var title = $"TestComment {random.Next(0, 10000)}";
            comment.Content.Raw = title;
            var commentUpdated = await _clientAuth.Comments.UpdateAsync(comment);
            Assert.AreEqual(commentUpdated.Content.Raw, title);
        }
        [TestMethod]
        public async Task Comments_Delete()
        {
            var posts = await _clientAuth.Posts.GetAllAsync();
            var postId = posts.First().Id;

            var me = await _clientAuth.Users.GetCurrentUser();

            // Create random content to prevent duplicate commment errors
            var random = new Random();
            var comment = new Comment()
            {
                Content = new Content($"Testcomment {random.Next(0, 10000)}"),
                PostId = postId,
                AuthorId = me.Id,
                AuthorEmail = "test@test.com",
                AuthorName = me.Name
            };
            var resultComment = await _clientAuth.Comments.CreateAsync(comment);

            var response = await _clientAuth.Comments.Delete(resultComment.Id);
            Assert.IsTrue(response);

        }
        [TestMethod]
        public async Task Comments_Query()
        {
            var queryBuilder = new CommentsQueryBuilder()
            {
                Page = 1,
                PerPage = 15,
                OrderBy = CommentsOrderBy.Id,
                Order = Order.DESC,
            };
            var queryresult = await _clientAuth.Comments.QueryAsync(queryBuilder);
            Assert.AreEqual("?page=1&per_page=15&orderby=id&order=desc&context=view", queryBuilder.BuildQueryURL());
            Assert.IsNotNull(queryresult);
            Assert.AreNotSame(queryresult.Count(), 0);
        }

        // TODO: Can't create pending comment from Admin Account
        //[TestMethod]
        public async Task Comments_Query_Pending()
        {
            // Create new pending comment
            var posts = await _clientAuth.Posts.GetAllAsync();
            var postId = posts.First().Id;
            var me = await _clientAuth.Users.GetCurrentUser();

            // Create random content to prevent duplicate commment errors
            var random = new Random();
            var content = $"TestComment {random.Next(0, 10000)}";
            var comment = new Comment()
            {
                Content = new Content(content),
                PostId = postId,
                AuthorId = me.Id,
                AuthorEmail = "test@test.com",
                AuthorName = me.Name,
                Status = CommentStatus.Pending
            };
            var resultComment = await _clientAuth.Comments.CreateAsync(comment);
            Assert.IsNotNull(resultComment);
            Assert.AreEqual(CommentStatus.Pending, resultComment.Status);

            // this test needs a pending comment added manually for now.
            var queryBuilder = new CommentsQueryBuilder()
            {
                Page = 1,
                PerPage = 15,
                OrderBy = CommentsOrderBy.Id,
                Order = Order.DESC,
                Statuses = new CommentStatus[] { CommentStatus.Pending }
            };
            var queryresult = await _clientAuth.Comments.QueryAsync(queryBuilder, true);
            var querystring = "?page=1&per_page=15&orderby=id&status=hold";
            Assert.AreEqual(querystring, queryBuilder.BuildQueryURL());
            Assert.IsNotNull(queryresult);
            Assert.AreNotEqual(queryresult.Count(), 0);

            // Delete Pending comment
            await _clientAuth.Comments.Delete(resultComment.Id);
        }

        [TestMethod]
        public async Task Comments_GetAllForPost()
        {
            var me = await _clientAuth.Users.GetCurrentUser();

            // create test post and add comments
            var post = new Post()
            {
                Title = new Title("Title 1"),
                Content = new Content("Content PostCreate")
            };
            var createdPost = await _clientAuth.Posts.CreateAsync(post);
            Assert.IsNotNull(createdPost);

            for (int i = 0; i < 30; i++)
            {
                // Create random content to prevent duplicate commment errors
                var content = $"TestComment {System.Guid.NewGuid()}";
                var comment = new Comment()
                {
                    Content = new Content(content),
                    PostId = createdPost.Id,
                    AuthorId = me.Id,
                    AuthorEmail = "test@test.com",
                    AuthorName = me.Name
                };
                var resultComment = await _clientAuth.Comments.CreateAsync(comment);
                Assert.IsNotNull(resultComment);
            }

            // shoud work without auth
            var nonauthclient = ClientHelper.GetWordPressClient();
            var comments = await nonauthclient.Comments.GetCommentsForPost(createdPost.Id);
            Assert.IsTrue(comments.Count() <= 10);

            var allComments = await nonauthclient.Comments.GetAllCommentsForPostAsync(createdPost.Id);
            Assert.IsTrue(allComments.Count() > 20);

            // cleanup
            var result = await _clientAuth.Posts.Delete(createdPost.Id);
            Assert.IsTrue(result);
        }

    }

}