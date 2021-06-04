using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WordPressPCL.Models;

namespace WordPressPCL.Tests.Hosted.Utility
{
    [TestClass]
    public class HttpHelper_Tests
    {

        [ClassInitialize]
        public static void Init(TestContext testContext)
        {
        }

        [TestMethod]
        public async Task Hosted_HttpHelper_InvalidPreProcessing()
        {
            // AUTHENTICATION DOES NOT YET WORK FOR HOSTED SITES
            var client = new WordPressClient(ApiCredentials.WordPressUri)
            {
                AuthMethod = AuthMethod.JWT
            };
            await client.RequestJWTokenAsync(ApiCredentials.Username, ApiCredentials.Password);

            // Create a random tag , must works:
            var random = new Random();
            var tagname = $"Test {random.Next(0, 1000)}";
            var tag = await client.Tags.CreateAsync(new Tag()
            {
                Name = tagname,
                Description = "Test Description"
            });
            Assert.IsTrue(tag.Id > 0);
            Assert.IsNotNull(tag);
            Assert.AreEqual(tagname, tag.Name);
            Assert.AreEqual("Test Description", tag.Description);

            // We call Get tag list without pre processing
            var tags = await client.Tags.GetAllAsync();
            Assert.IsNotNull(tags);
            Assert.AreNotEqual(tags.Count(), 0);
            CollectionAssert.AllItemsAreUnique(tags.Select(e => e.Id).ToList());

            // Now we add a PreProcessing task
            client.HttpResponsePreProcessing = (response) =>
            {
                throw new InvalidOperationException("PreProcessing must fail");
            };
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await client.Tags.GetAllAsync();
            });
        }

        [TestMethod]
        public async Task Hosted_HttpHelper_ValidPreProcessing()
        {
            var client = new WordPressClient(ApiCredentials.WordPressUri)
            {
                AuthMethod = AuthMethod.JWT
            };
            await client.RequestJWTokenAsync(ApiCredentials.Username, ApiCredentials.Password);

            // Create a random tag
            var random = new Random();
            var tagname = $"Test {random.Next(0, 1000)}";
            var tag = await client.Tags.CreateAsync(new Tag()
            {
                Name = tagname,
                Description = "Test Description"
            });
            Assert.IsTrue(tag.Id > 0);
            Assert.IsNotNull(tag);
            Assert.AreEqual(tagname, tag.Name);
            Assert.AreEqual("Test Description", tag.Description);

            // We call Get tag list without pre processing
            var tags = await client.Tags.GetAllAsync();
            Assert.IsNotNull(tags);
            Assert.AreNotEqual(tags.Count(), 0);
            CollectionAssert.AllItemsAreUnique(tags.Select(e => e.Id).ToList());

            // Now we add a PreProcessing task
            client.HttpResponsePreProcessing = (response) =>
            {
                return response;
            };

            tags = await client.Tags.GetAllAsync();
            Assert.IsNotNull(tags);
            Assert.AreNotEqual(tags.Count(), 0);
            CollectionAssert.AllItemsAreUnique(tags.Select(e => e.Id).ToList());
        }
    }
}
