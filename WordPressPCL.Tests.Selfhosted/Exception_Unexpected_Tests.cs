using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WordPressPCL.Models;
using WordPressPCL.Models.Exceptions;

namespace WordPressPCL.Tests.Selfhosted
{
    [TestClass]
    public class Exception_Unexpected_Tests
    {
        private WordPressClient _badConnectionClient;

        [TestInitialize]
        public void Initialize()
        {
            _badConnectionClient = new WordPressClient("https://microsoft.com");
        }

        [TestMethod]
        public async Task Tags_Get_UnexpectedException()
        {
            await CheckForUnexpectedException(async () => await _badConnectionClient.Tags.GetAsync());
        }

        [TestMethod]
        public async Task Tags_Post_UnexpectedException()
        {
            await CheckForUnexpectedException(async () => await _badConnectionClient.Tags.UpdateAsync(new Tag()));
        }

        [TestMethod]
        public async Task Tags_Delete_UnexpectedException()
        {
            await CheckForUnexpectedException(async () => await _badConnectionClient.Tags.Delete(1), HttpStatusCode.NotImplemented);
        }

        private async Task CheckForUnexpectedException(Func<Task> task, HttpStatusCode expectedStatus = HttpStatusCode.NotFound)
        {
            bool exceptionCaught = false;

            try
            {
                await task();
            }
            catch (Exception ex)
            {
                exceptionCaught = true;

                if (!(ex is WPUnexpectedException typedException))
                {
                    Assert.Fail($"Expected an exception of type WPUnexpectedException, but saw exception of type {ex.GetType()}");
                }
                else
                {
                    Assert.AreEqual($"Server returned HTTP status {expectedStatus}", typedException.Message);
                    Assert.IsNotNull(typedException.Response, "No HTTP response has been returned");
                    Assert.AreEqual(expectedStatus, typedException.Response.StatusCode);
                    Assert.IsFalse(string.IsNullOrEmpty(typedException.ResponseBody), "Expected a response body but didn't see one");
                }
            }

            Assert.IsTrue(exceptionCaught, "Exception was expected but none was seen");
        }
    }
}
