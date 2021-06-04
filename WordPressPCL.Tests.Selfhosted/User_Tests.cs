using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using WordPressPCL.Models;
using WordPressPCL.Utility;
using WordPressPCL.Tests.Selfhosted.Utility;
using WordPressPCL.Models.Exceptions;
using Guid = System.Guid;

namespace WordPressPCL.Tests.Selfhosted
{
    [TestClass]
    public class User_Tests
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
        public async Task Users_Create()
        {
            var username = Guid.NewGuid().ToString();
            var nickname = $"Nickname{username}";
            var email = $"{username}@test.com";
            var firstname = $"Firstname{username}";
            var lastname = $"Lastname{username}";
            var password = $"testpassword{username}";
            var name = $"{firstname} {lastname}";

            var user = await _clientAuth.Users.CreateAsync(new User(username, email, password)
            {
                NickName = nickname,
                Name = name,
                FirstName = firstname,
                LastName = lastname
            });
            Assert.IsNotNull(user);
            Assert.AreEqual(nickname, user.NickName);
            Assert.AreEqual(name, user.Name);
            Assert.AreEqual(firstname, user.FirstName);
            Assert.AreEqual(lastname, user.LastName);
            Assert.AreEqual(username, user.UserName);
            Assert.AreEqual(email, user.Email);
        }

        [TestMethod]
        public async Task Users_Read()
        {
            var users = await _client.Users.GetAllAsync();
            Assert.IsNotNull(users);
            Assert.IsTrue(users.Count() >= 1);
            var user = await _client.Users.GetByID(users.First().Id);
            Assert.IsNotNull(user);
            Assert.AreEqual(user.Id, users.First().Id);
        }

        [TestMethod]
        public async Task Users_Get()
        {
            var users = await _client.Users.GetAsync();
            Assert.IsNotNull(users);
            Assert.IsTrue(users.Count() >= 1);
            var user = await _client.Users.GetByID(users.First().Id);
            Assert.IsNotNull(user);
            Assert.AreEqual(user.Id, users.First().Id);
        }

        [TestMethod]
        public async Task Users_Update()
        {
            var user = await CreateRandomUser();
            Assert.IsNotNull(user);

            var name = $"TestuserUpdate";
            user.Name = name;
            user.NickName = name;
            user.FirstName = name;
            user.LastName = name;

            var updatedUser = await _clientAuth.Users.UpdateAsync(user);
            Assert.IsNotNull(updatedUser);
            Assert.AreEqual(updatedUser.Name, name);
            Assert.AreEqual(updatedUser.NickName, name);
            Assert.AreEqual(updatedUser.FirstName, name);
            Assert.AreEqual(updatedUser.LastName, name);
        }

        [TestMethod]
        public async Task Users_Delete()
        {
            var user = await CreateRandomUser();
            Assert.IsNotNull(user);
            var me = await _clientAuth.Users.GetCurrentUser();
            var response = await _clientAuth.Users.Delete(user.Id, me.Id);
            Assert.IsTrue(response);
        }

        [TestMethod]
        public async Task Users_Delete_And_Reassign_Posts()
        {
            // Create new user
            var user1 = await CreateRandomUser();
            Assert.IsNotNull(user1);
            var user2 = await CreateRandomUser();
            Assert.IsNotNull(user2);

            // Create post for user
            var post = new Post()
            {
                Title = new Title("Title 1"),
                Content = new Content("Content PostCreate"),
                Author = user1.Id
            };
            var postCreated = await _clientAuth.Posts.CreateAsync(post);
            Assert.IsNotNull(post);
            Assert.AreEqual(postCreated.Author, user1.Id);

            // Delete User1 and reassign posts to user2
            var response = await _clientAuth.Users.Delete(user1.Id, user2.Id);
            Assert.IsTrue(response);

            // Get posts for user 2 and check if ID of postCreated is in there
            var postsOfUser2 = await _clientAuth.Posts.GetPostsByAuthor(user2.Id);
            var postsById = postsOfUser2.Where(x => x.Id == postCreated.Id).ToList();
            Assert.AreEqual(postsById.Count, 1);
        }

        [TestMethod]
        public async Task Users_Query()
        {
            var queryBuilder = new UsersQueryBuilder()
            {
                Page = 1,
                PerPage = 15,
                OrderBy = UsersOrderBy.RegisteredDate,
                Order = Order.DESC,
                Context = Context.Edit
            };
            Assert.AreEqual("?page=1&per_page=15&orderby=registered_date&order=desc&context=edit", queryBuilder.BuildQueryURL());

            var queryresult = await _clientAuth.Users.QueryAsync(queryBuilder, true);
            Assert.IsNotNull(queryresult);
            Assert.AreNotSame(queryresult.Count(), 0);
        }

        #region Utils

        private Task<User> CreateRandomUser()
        {
            var username = Guid.NewGuid().ToString();
            var email = $"{username}@test.com";
            var password = $"testpassword{username}";
            return _clientAuth.Users.CreateAsync(new User(username, email, password));
        }

        #endregion Utils
    }
}