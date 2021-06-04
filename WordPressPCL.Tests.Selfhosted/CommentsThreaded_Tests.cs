using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Threading.Tasks;
using WordPressPCL.Models;
using WordPressPCL.Utility;
using WordPressPCL.Tests.Selfhosted.Utility;
using System.Linq;

namespace WordPressPCL.Tests.Selfhosted
{
    [TestClass]
    public class CommentsThreaded_Tests
    {
        private static int postid;
        private static int comment0id;
        private static int comment00id;
        private static int comment1id;
        private static int comment2id;
        private static int comment3id;
        private static int comment4id;
        private static WordPressClient _clientAuth;

        [ClassInitialize]
        public static async Task CommentsThreaded_SetupAsync(TestContext testContext)
        {
            _clientAuth = await ClientHelper.GetAuthenticatedWordPressClient(testContext);
            var IsValidToken = await _clientAuth.IsValidJWTokenAsync();
            Assert.IsTrue(IsValidToken);

            var post = await _clientAuth.Posts.CreateAsync(new Post()
            {
                Title = new Title("Title 1"),
                Content = new Content("Content PostCreate")
            });
            await Task.Delay(1000);
            var comment0 = await _clientAuth.Comments.CreateAsync(new Comment()
            {
                PostId = post.Id,
                Content = new Content("orem ipsum dolor sit amet")
            });

            var comment00 = await _clientAuth.Comments.CreateAsync(new Comment()
            {
                PostId = post.Id,
                Content = new Content("r sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam non")
            });

            var comment1 = await _clientAuth.Comments.CreateAsync(new Comment()
            {
                PostId = post.Id,
                ParentId = comment0.Id,
                Content = new Content("onsetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna ali")
            });
            var comment2 = await _clientAuth.Comments.CreateAsync(new Comment()
            {
                PostId = post.Id,
                ParentId = comment1.Id,
                Content = new Content("ro eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem i")
            });
            var comment3 = await _clientAuth.Comments.CreateAsync(new Comment()
            {
                PostId = post.Id,
                ParentId = comment2.Id,
                Content = new Content("tetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam e")
            });
            var comment4 = await _clientAuth.Comments.CreateAsync(new Comment()
            {
                PostId = post.Id,
                ParentId = comment1.Id,
                Content = new Content("t ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum do")
            });
            postid = post.Id;
            comment0id = comment0.Id;
            comment00id = comment00.Id;
            comment1id = comment1.Id;
            comment2id = comment2.Id;
            comment3id = comment3.Id;
            comment4id = comment4.Id;
        }

        [TestMethod]
        public async Task CommentsThreaded_Sort()
        {
            var allComments = await _clientAuth.Comments.GetAllCommentsForPostAsync(postid);

            var threaded = ThreadedCommentsHelper.GetThreadedComments(allComments);
            Assert.IsNotNull(threaded);
            var ct0 = threaded.Find(x => x.Id == comment0id);
            Assert.AreEqual(ct0.Depth, 0);
            var ct1 = threaded.Find(x => x.Id == comment1id);
            Assert.AreEqual(ct1.Depth, 1);
            var ct2 = threaded.Find(x => x.Id == comment2id);
            Assert.AreEqual(ct2.Depth, 2);
            var ct3 = threaded.Find(x => x.Id == comment3id);
            Assert.AreEqual(ct3.Depth, 3);
            var ct4 = threaded.Find(x => x.Id == comment4id);
            Assert.AreEqual(ct4.Depth, 2);

            var ct00 = threaded.Find(x => x.Id == comment00id);
            Assert.AreEqual(ct00.Depth, 0);

            for (int i = 0; i < threaded.Count - 1; i++)
            {
                // The following comment depth has to be the lower, equal or +1
                var ni = i + 1;
                var id = threaded[i].Depth;
                var nid = threaded[ni].Depth;
                var validDepth = (id >= nid || id + 1 == nid);
                Assert.IsTrue(validDepth);

                var idate = threaded[i].Date;
                var nidate = threaded[ni].Date;

                // The following comment date has to be
                var validDate = (
                    // newer
                    idate <= nidate
                    // or older and a child comment
                    || (idate > nidate && id > nid));
                Assert.IsTrue(validDate);
            }
        }

        [TestMethod]
        public async Task CommentsThreaded_MaxDepth()
        {
            var allComments = await _clientAuth.Comments.GetAllCommentsForPostAsync(postid);

            var threaded = ThreadedCommentsHelper.GetThreadedComments(allComments, 1);
            Assert.IsNotNull(threaded);
            var ct0 = threaded.Find(x => x.Id == comment0id);
            Assert.AreEqual(ct0.Depth, 0);
            var ct1 = threaded.Find(x => x.Id == comment1id);
            Assert.AreEqual(ct1.Depth, 1);
            var ct2 = threaded.Find(x => x.Id == comment2id);
            Assert.AreEqual(ct2.Depth, 1);
            var ct3 = threaded.Find(x => x.Id == comment3id);
            Assert.AreEqual(ct3.Depth, 1);
            var ct4 = threaded.Find(x => x.Id == comment4id);
            Assert.AreEqual(ct4.Depth, 1);

            var ct00 = threaded.Find(x => x.Id == comment00id);
            Assert.AreEqual(ct00.Depth, 0);
        }

        [TestMethod]
        public async Task CommentsThreaded_Sort_Extension()
        {
            var allComments = await _clientAuth.Comments.GetAllCommentsForPostAsync(postid);

            Assert.IsTrue(allComments.Count() > 0);
            //ExtensionMethod
            var threaded = ThreadedCommentsHelper.ToThreaded(allComments);
            Assert.IsNotNull(threaded);
            var ct0 = threaded.Find(x => x.Id == comment0id);
            Assert.AreEqual(ct0.Depth, 0);
            var ct1 = threaded.Find(x => x.Id == comment1id);
            Assert.AreEqual(ct1.Depth, 1);
            var ct2 = threaded.Find(x => x.Id == comment2id);
            Assert.AreEqual(ct2.Depth, 2);
            var ct3 = threaded.Find(x => x.Id == comment3id);
            Assert.AreEqual(ct3.Depth, 3);
            var ct4 = threaded.Find(x => x.Id == comment4id);
            Assert.AreEqual(ct4.Depth, 2);

            var ct00 = threaded.Find(x => x.Id == comment00id);
            Assert.AreEqual(ct00.Depth, 0);
            //Assert.AreEqual(threaded.Count, threaded.IndexOf(ct00) + 1);

            for (int i = 0; i < threaded.Count - 1; i++)
            {
                // The following comment depth has to be the lower, equal or +1
                var ni = i + 1;
                var id = threaded[i].Depth;
                var nid = threaded[ni].Depth;
                var validDepth = (id >= nid || id + 1 == nid);
                Assert.IsTrue(validDepth);

                var idate = threaded[i].Date;
                var nidate = threaded[ni].Date;

                // The following comment date has to be
                var validDate = (
                    // newer
                    idate <= nidate
                    // or older and a child comment
                    || (idate > nidate && id > nid));
                Assert.IsTrue(validDate);
            }
        }

        [TestMethod]
        public async Task CommentsThreaded_Sort_Extension_Desc()
        {
            var allComments = await _clientAuth.Comments.GetAllCommentsForPostAsync(postid);
            Assert.IsTrue(allComments.Count() > 0);

            var threaded = ThreadedCommentsHelper.ToThreaded(allComments, true);

            // Depth should be the same regardless of desc or asc
            Assert.IsNotNull(threaded);
            var ct0 = threaded.Find(x => x.Id == comment0id);
            Assert.AreEqual(ct0.Depth, 0);
            var ct1 = threaded.Find(x => x.Id == comment1id);
            Assert.AreEqual(ct1.Depth, 1);
            var ct2 = threaded.Find(x => x.Id == comment2id);
            Assert.AreEqual(ct2.Depth, 2);
            var ct3 = threaded.Find(x => x.Id == comment3id);
            Assert.AreEqual(ct3.Depth, 3);
            var ct4 = threaded.Find(x => x.Id == comment4id);
            Assert.AreEqual(ct4.Depth, 2);

            var ct00 = threaded.Find(x => x.Id == comment00id);
            Assert.AreEqual(ct00.Depth, 0);

            for (int i = 0; i < threaded.Count - 1; i++)
            {
                // The following comment depth has to be the lower, equal or +1 at most
                var ni = i + 1;
                var idepth = threaded[i].Depth;
                var nidepth = threaded[ni].Depth;
                var niparent = threaded[ni].ParentId;
                var validDepth = (idepth >= nidepth || idepth + 1 == nidepth);
                Assert.IsTrue(validDepth);

                var idate = threaded[i].Date;
                var nidate = threaded[ni].Date;

                // The following comment date has to be
                var validDate = (
                        // older
                        idate >= nidate
                    // or newer, if it's a direct child comment
                    || (idate < nidate && threaded[ni].ParentId == threaded[i].Id)
                    // or newer, if the comments share the same parent
                    || (idate < nidate && nidepth != 0 && idepth >= nidepth));

                Assert.IsTrue(validDate);
            }

            // Comments with depth 0 must be ordered desc
            var firstLvl = threaded.FindAll(x => x.Depth == 0);
            for (int i = 0; i < firstLvl.Count - 1; i++)
            {
                // The following comment depth has to be the lower, equal or +1
                var ni = i + 1;
                var idate = threaded[i].Date;
                var nidate = threaded[ni].Date;

                // The following comment date has to be older
                Assert.IsTrue(threaded[i].Id > threaded[ni].Id);
                Assert.IsTrue(idate <= nidate);
            }
        }

        [ClassCleanup]
        public async static Task CommentsThreaded_Cleanup()
        {
            await _clientAuth.Posts.Delete(postid);
        }
    }
}