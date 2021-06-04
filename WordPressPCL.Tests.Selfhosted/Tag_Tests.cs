using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WordPressPCL.Tests.Selfhosted.Utility;
using WordPressPCL;
using System.Threading.Tasks;
using WordPressPCL.Models;
using WordPressPCL.Utility;
using System.Linq;

namespace WordPressPCL.Tests.Selfhosted
{
    [TestClass]
    public class Tag_Tests
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
        public async Task Tags_Create()
        {
            var random = new Random();
            var tagname = $"Test {random.Next(0, 1000)}";
            var tag = await _clientAuth.Tags.CreateAsync(new Tag()
            {
                Name = tagname,
                Description = "Test Description"
            });
            Assert.IsNotNull(tag);
            Assert.AreEqual(tagname, tag.Name);
            Assert.AreEqual("Test Description", tag.Description);
        }

        [TestMethod]
        public async Task Tags_Read()
        {
            var tags = await _clientAuth.Tags.GetAllAsync();
            Assert.IsNotNull(tags);
            Assert.AreNotEqual(tags.Count(), 0);
            CollectionAssert.AllItemsAreUnique(tags.Select(tag => tag.Id).ToList());
        }

        [TestMethod]
        public async Task Tags_Get()
        {
            var tags = await _client.Tags.GetAsync();
            Assert.IsNotNull(tags);
            Assert.AreNotEqual(tags.Count(), 0);
            CollectionAssert.AllItemsAreUnique(tags.Select(tag => tag.Id).ToList());
        }

        [TestMethod]
        public async Task Tags_Update()
        {
            var tags = await _clientAuth.Tags.GetAllAsync();
            var tag = tags.FirstOrDefault();
            if(tag == null)
            {
                Assert.Inconclusive();
            }
            var random = new Random();
            var tagname = $"Testname {random.Next(0, 1000)}";
            var tagdesc = "Test Description";
            tag.Name = tagname;
            tag.Description = tagdesc;
            var tagUpdated = await _clientAuth.Tags.UpdateAsync(tag);
            Assert.AreEqual(tagname, tagUpdated.Name);
            Assert.AreEqual(tagdesc, tagUpdated.Description);
        }

        [TestMethod]
        public async Task Tags_Delete()
        {
            var tags = await _clientAuth.Tags.GetAllAsync();
            var tag = tags.FirstOrDefault();
            if (tag == null)
            {
                Assert.Inconclusive();
            }
            var tagId = tag.Id;
            var response = await _clientAuth.Tags.Delete(tagId);
            Assert.IsTrue(response);
            tags = await _clientAuth.Tags.GetAllAsync();
            var tagsWithId = tags.Where(x => x.Id == tagId).ToList();
            Assert.AreEqual(tagsWithId.Count, 0);
        }
        [TestMethod]
        public async Task Tags_Query()
        {
            var queryBuilder = new TagsQueryBuilder()
            {
                Page = 1,
                PerPage = 15,
                OrderBy = TermsOrderBy.Id,
                Order = Order.DESC,
            };
            var queryresult = await _clientAuth.Tags.QueryAsync(queryBuilder);
            Assert.AreEqual("?page=1&per_page=15&orderby=id&order=desc&context=view", queryBuilder.BuildQueryURL());
            Assert.IsNotNull(queryresult);
            Assert.AreNotSame(queryresult.Count(), 0);
        }

    }

}