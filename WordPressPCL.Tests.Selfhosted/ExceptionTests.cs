using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using WordPressPCL.Models;
using WordPressPCL.Models.Exceptions;
using WordPressPCL.Tests.Selfhosted.Utility;

namespace WordPressPCL.Tests.Selfhosted
{
    [TestClass]
    public class ExceptionTests
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
        public async Task Exception_JWTAuthExceptionTest()
        {
            // Initialize
            Assert.IsNotNull(_client);
            // Get settings without auth

            await Assert.ThrowsExceptionAsync<WPException>(async () =>
            {
                var settings = await _client.GetSettings();
            });
        }

        [TestMethod]
        public async Task Exception_PostCreateExceptionTest()
        {
            // Initialize
            Assert.IsNotNull(_clientAuth);
            // Create empty post
            try
            {
                var post = await _clientAuth.Posts.CreateAsync(new Post());
            }
            catch (WPException wpex)
            {
                Assert.IsNotNull(wpex.RequestData);
                Assert.AreEqual(wpex.RequestData.Name, "empty_content");
            }
        }

        [TestMethod]
        public async Task Exception_DeleteExceptionTest()
        {
            // Initialize
            Assert.IsNotNull(_clientAuth);
            // Delete nonexisted post
            try
            {
                var result = await _clientAuth.Posts.Delete(int.MaxValue);
            }
            catch (WPException wpex)
            {
                Assert.IsNotNull(wpex.RequestData);
                Assert.AreEqual(wpex.RequestData.Name, "rest_post_invalid_id");
            }
        }
    }
}