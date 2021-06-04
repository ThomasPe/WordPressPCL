using Microsoft.VisualStudio.TestTools.UnitTesting;
using WordPressPCL.Tests.Selfhosted.Utility;
using WordPressPCL;
using System.Threading.Tasks;
using WordPressPCL.Utility;
using System.Linq;

namespace WordPressPCL.Tests.Selfhosted
{
    [TestClass]
    public class ListPosts_QueryBuilder_Tests
    {
        private static WordPressClient _client;

        [ClassInitialize]
        public static void Init(TestContext testContext)
        {
            _client = ClientHelper.GetWordPressClient();
        }

        // TODO: initialize more test posts
        //[TestMethod]
        public async Task List_Posts_QueryBuilder_Test_Pagination()
        {
            // Posts
            var postsA = await _client.Posts.QueryAsync(new PostsQueryBuilder()
            {
                Page = 1,
                PerPage = 2
            });
            var postsB = await _client.Posts.QueryAsync(new PostsQueryBuilder()
            {
                Page = 2,
                PerPage = 2
            });
            Assert.IsNotNull(postsA);
            Assert.IsNotNull(postsB);
            Assert.AreNotEqual(postsA.Count(), 0);
            Assert.AreNotEqual(postsB.Count(), 0);
            CollectionAssert.AreNotEqual(postsA.Select(post => post.Id).ToList(), postsB.Select(post => post.Id).ToList());
        }

        [TestMethod]
        [Description("Test that the ListPosts method with the QueryBuilder set to list posts published After a specific date works.")]
        public async Task List_Posts_QueryBuilder_After()
        {
            // Posts
            var posts = await _client.Posts.QueryAsync(new PostsQueryBuilder { After = System.DateTime.Parse("2017-05-22T13:41:09") });
            Assert.IsNotNull(posts);
        }
    }
}