using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using WordPressPCL;
using WordPressPCL.Client;
using WordPressPCL.Models;
using WordPressPCL.Utility;

namespace Test
{
    class Program
    {
        public static string SiteUri = "mysite.wordpress.com";
        public static string WordPressUri = $"https://public-api.wordpress.com/wp/v2/sites/{SiteUri}/";

        static async Task Main(string[] args)
        {
            var client = new WordPressClient(WordPressUri, "");

             var queryBuilder = new PostsQueryBuilder()
            {
                Page = 1,
                PerPage = 15,
                OrderBy = PostsOrderBy.Title,
                Order = Order.ASC,
                Statuses = new Status[] { Status.Publish },
                Embed = true
            };
            var queryresult = await client.Posts.Query(queryBuilder);

            Console.WriteLine(queryresult.Total);
            Console.WriteLine(queryresult.TotalPages);
            Console.WriteLine(JsonSerializer.Serialize(queryresult.Count()));
        }
    }
}
