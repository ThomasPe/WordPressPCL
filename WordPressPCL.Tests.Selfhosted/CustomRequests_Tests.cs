using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordPressPCL.Tests.Selfhosted.Utility;

namespace WordPressPCL.Tests.Selfhosted
{
    [TestClass]
    public class CustomRequests_Tests
    {
        //Requires contact-form-7 plugin
        private class ContactFormItem
        {
            [JsonProperty("id")]
            public int? Id;
            [JsonProperty("title")]
            public string Title;
            [JsonProperty("slug")]
            public string Slug;
            [JsonProperty("locale")]
            public string Locale;
        }

        private static WordPressClient _clientAuth;

        [ClassInitialize]
        public static async Task Init(TestContext testContext)
        {
            _clientAuth = await ClientHelper.GetAuthenticatedWordPressClient(testContext);
        }

        [TestMethod]
        public async Task CustomRequests_Read()
        {
            var forms = await _clientAuth.CustomRequest.Get<IEnumerable<ContactFormItem>>("contact-form-7/v1/contact-forms", false, true);
            Assert.IsNotNull(forms);
            Assert.AreNotEqual(forms.Count(), 0);
        }

        // TODO: check why this isn't returning the form
        //[TestMethod]
        public async Task CustomRequests_Create()
        {
            var random = new Random();
            var r = random.Next(0, 10000);
            var title = $"Test Form {r}";
            var form = await _clientAuth.CustomRequest.CreateAsync<ContactFormItem, ContactFormItem>("contact-form-7/v1/contact-forms", new ContactFormItem() { Title = title, Locale = "en-US" });
            Assert.IsNotNull(form);
            Assert.IsNotNull(form.Id);
            Assert.AreEqual(form.Title, title);
        }

        // TODO: make test not depend on other tests
        //[TestMethod]
        public async Task CustomRequests_Update()
        {
            var random = new Random();
            var r = random.Next(0, 10000);
            var title = $"Test Form {r}";
            var form = await _clientAuth.CustomRequest.CreateAsync<ContactFormItem, ContactFormItem>("contact-form-7/v1/contact-forms", new ContactFormItem() { Title = title, Locale = "en-US" });

            var forms = await _clientAuth.CustomRequest.Get<IEnumerable<ContactFormItem>>("contact-form-7/v1/contact-forms", false, true);
            Assert.IsNotNull(forms);
            Assert.AreNotEqual(forms.Count(), 0);
            var editform = forms.First();
            editform.Title += "test";
            var form2 = await _clientAuth.CustomRequest.Update<ContactFormItem, ContactFormItem>($"contact-form-7/v1/contact-forms/{editform.Id.Value}", editform);
            Assert.IsNotNull(form2);
            Assert.AreEqual(form.Title, editform.Title);
        }

        // TODO: make test not depend on other tests
        //[TestMethod]
        public async Task CustomRequests_Delete()
        {
            var forms = await _clientAuth.CustomRequest.Get<IEnumerable<ContactFormItem>>("contact-form-7/v1/contact-forms", false, true);
            Assert.IsNotNull(forms);
            Assert.AreNotEqual(forms.Count(), 0);
            var deleteform = forms.First();
            var result = await _clientAuth.CustomRequest.Delete($"contact-form-7/v1/contact-forms/{deleteform.Id.Value}");
            Assert.IsTrue(result);
        }
    }
}