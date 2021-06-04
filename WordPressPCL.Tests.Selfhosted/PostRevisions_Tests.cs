using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using WordPressPCL;
using WordPressPCL.Models;
using WordPressPCL.Tests.Selfhosted.Utility;

namespace WordPressPCL.Tests.Selfhosted
{
    [TestClass]
    public class PostRevisions_Tests
    {
        private static WordPressClient _clientAuth;

        [ClassInitialize]
        public static async Task Init(TestContext testContext)
        {
            _clientAuth = await ClientHelper.GetAuthenticatedWordPressClient(testContext);
        }

        [TestMethod]
        public async Task PostRevisions_Read()
        {
            var id = await CreatePostWithRevision();
            var revisionsclient = _clientAuth.Posts.Revisions(id);
            var revisions = await revisionsclient.GetAllAsync();
            Assert.AreNotEqual(revisions.Count(), 0);
        }

        [TestMethod]
        public async Task PostRevisions_Get()
        {
            var id = await CreatePostWithRevision();
            var revisionsclient = _clientAuth.Posts.Revisions(id);
            var revisions = await revisionsclient.GetAsync();
            Assert.AreNotEqual(revisions.Count(), 0);
        }

        // TODO: check why revision can't be deleted
        //[TestMethod]
        public async Task PostRevisions_Delete()
        {
            var id = await CreatePostWithRevision();

            var revisionsclient = _clientAuth.Posts.Revisions(id);
            var revisions = await revisionsclient.GetAllAsync();
            Assert.AreNotEqual(revisions.Count(), 0);
            var res = await revisionsclient.Delete(revisions.First().Id);
            Assert.IsTrue(res);
        }

        private async Task<int> CreatePostWithRevision()
        {
            var post = new Post()
            {
                Title = new Title("Title 1"),
                Content = new Content("Content PostCreate"),
            };
            var createdPost = await _clientAuth.Posts.CreateAsync(post);
            createdPost.Content.Raw = "Updated Content";
            var updatedPost = await _clientAuth.Posts.UpdateAsync(createdPost);
            return updatedPost.Id;
        }
    }
}