using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using WordPressPCL;
using WordPressPCL.Tests.Selfhosted.Utility;

namespace WordPressPCL.Tests.Selfhosted
{
    [TestClass]
    public class PostStatuses_Tests
    {
        private static WordPressClient _clientAuth;

        [ClassInitialize]
        public static async Task Init(TestContext testContext)
        {
            _clientAuth = await ClientHelper.GetAuthenticatedWordPressClient(testContext);
        }

        [TestMethod]
        public async Task PostStatuses_Read()
        {
            var poststatuses = await _clientAuth.PostStatuses.GetAllAsync();
            Assert.IsNotNull(poststatuses);
            Assert.AreNotEqual(poststatuses.Count(), 0);
        }

        [TestMethod]
        public async Task PostStatuses_Get()
        {
            var poststatuses = await _clientAuth.PostStatuses.GetAsync();
            Assert.IsNotNull(poststatuses);
            Assert.AreNotEqual(poststatuses.Count(), 0);
        }
    }
}