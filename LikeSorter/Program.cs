using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;


internal class Program
{
    public static void Main(string[] args)
    {
        Console.Write("Enter wall ID: ");
        int owner_id = Convert.ToInt32(Console.ReadLine()); 
        Console.Write("Enter max number of posts to analyze (0 is all wall posts): ");
        int max_number = Convert.ToInt32(Console.ReadLine()); 
        int count = 100;
        int offset = 0;
        int post_count = 1;
        LinkedList<Post> posts = new LinkedList<Post>();
        while (offset < post_count)
        {
            Console.WriteLine($"{offset}/{post_count}");
            WebRequest request = WebRequest.Create(
                $"https://api.vk.com/method/wall.get?" +
                $"owner_id={owner_id}&" +
                $"access_token=a3a57e9fa3a57e9fa3a57e9f6ca3face30aa3a5a3a57e9ff9b25bc3899baea2cf8024bc&" +
                $"v=5.92&" +
                $"fields=id&" +
                $"count={count}&" +
                $"offset={offset}");
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            VKResponse res = JsonConvert.DeserializeObject<VKResponse>(responseFromServer);
            //Console.WriteLine(responseFromServer);
            foreach (VKPost responseItem in res.response.items)
            {
                posts.AddLast(new Post(){id = responseItem.id, likes = responseItem.likes.count, owner_id = responseItem.owner_id});
            }
            reader.Close();
            response.Close();
            post_count = res.response.count;
            offset += count;
            if (max_number >= 0 && offset > max_number)
            {
                break;
            }
            System.Threading.Thread.Sleep(50);
        }

        IEnumerable sortedPost = posts.OrderBy(x => x.likes);
        using (StreamWriter outputFile = new StreamWriter($"WallPostsFor{owner_id}.txt"))
        {
            foreach (var o in sortedPost)
            {
                string line = $"{((Post) o).FullUrl} - {((Post) o).likes}";
                Console.WriteLine(line);
                outputFile.WriteLine(line);
            }
        }

        Console.ReadLine();
    }

    public class VKResponse
    {
        public VKResponseMessage response { get; set; }
    }

    public class VKResponseMessage
    {
        public int count { get; set; }
        public List<VKPost> items { get; set; }
    }

    public class VKPost
    {
        public int id { get; set; }
        public int owner_id { get; set; }
        public VKLikes likes { get; set; }
    }

    public class VKLikes
    {
        public int count { get; set; }
    }

    public class Post
    {
        public int id;
        public int owner_id;
        public int likes;

        public string FullUrl => $"https://vk.com/wall{owner_id}_{id}";
    }
}