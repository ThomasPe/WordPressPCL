using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WordPressPCL.Models;

namespace WordPressPCL.Tests.Selfhosted.Utility
{
    [TestClass]
    public class HttpHelper_Tests
    {
        private static WordPressClient _clientAuth;

        [ClassInitialize]
        public static async Task Init(TestContext testContext)
        {
            _clientAuth = await ClientHelper.GetAuthenticatedWordPressClient(testContext);
        }


        [TestMethod]
        public async Task HttpHelper_InvalidPreProcessing()
        {
            // Create a random tag , must works:
            var random = new Random();
            var tagname = $"Test {random.Next(0, 1000)}";
            var tag = await _clientAuth.Tags.CreateAsync(new Tag()
            {
                Name = tagname,
                Description = "Test Description"
            });
            Assert.IsTrue(tag.Id > 0);
            Assert.IsNotNull(tag);
            Assert.AreEqual(tagname, tag.Name);
            Assert.AreEqual("Test Description", tag.Description);

            // We call Get tag list without pre processing
            var tags = await _clientAuth.Tags.GetAllAsync();
            Assert.IsNotNull(tags);
            Assert.AreNotEqual(tags.Count(), 0);
            CollectionAssert.AllItemsAreUnique(tags.Select(e => e.Id).ToList());

            // Now we add a PreProcessing task
            _clientAuth.HttpResponsePreProcessing = (response) =>
            {
                throw new InvalidOperationException("PreProcessing must fail");
            };
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await _clientAuth.Tags.GetAllAsync();
            });
            _clientAuth.HttpResponsePreProcessing = null;
        }

        [TestMethod]
        public async Task HttpHelper_ValidPreProcessing()
        {
            _clientAuth.HttpResponsePreProcessing = null;

            // Create a random tag
            var random = new Random();
            var tagname = $"Test {random.Next(0, 1000)}";
            var tag = await _clientAuth.Tags.CreateAsync(new Tag()
            {
                Name = tagname,
                Description = "Test Description"
            });
            Assert.IsTrue(tag.Id > 0);
            Assert.IsNotNull(tag);
            Assert.AreEqual(tagname, tag.Name);
            Assert.AreEqual("Test Description", tag.Description);

            // We call Get tag list without pre processing
            var tags = await _clientAuth.Tags.GetAllAsync();
            Assert.IsNotNull(tags);
            Assert.AreNotEqual(tags.Count(), 0);
            CollectionAssert.AllItemsAreUnique(tags.Select(e => e.Id).ToList());

            // Now we add a PreProcessing task
            _clientAuth.HttpResponsePreProcessing = (response) =>
            {
                return response;
            };

            tags = await _clientAuth.Tags.GetAllAsync();
            Assert.IsNotNull(tags);
            Assert.AreNotEqual(tags.Count(), 0);
            CollectionAssert.AllItemsAreUnique(tags.Select(e => e.Id).ToList());
            _clientAuth.HttpResponsePreProcessing = null;
        }
    }
}
