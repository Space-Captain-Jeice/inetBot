using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InetBot.Data
{
    public class Cat
    {
        public string _id;

        public static Cat GetRandomCat()
        {
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            using (HttpResponseMessage response = client.GetAsync("https://cataas.com/cat?json=true").Result)
            {
                using (Stream stream = response.Content.ReadAsStream())
                {
                    StreamReader reader = new StreamReader(stream, true);
                    String responseString = reader.ReadToEnd();
                    Cat cat = JsonConvert.DeserializeObject<Cat>(responseString);

                    return cat;
                }
            }
        }
    }

    public class Dog
    {
        public string message;

        public static Dog GetRandomDog()
        {
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            using (HttpResponseMessage response = client.GetAsync("https://dog.ceo/api/breeds/image/random").Result)
            {
                using (Stream stream = response.Content.ReadAsStream())
                {
                    StreamReader reader = new StreamReader(stream, true);
                    String responseString = reader.ReadToEnd();
                    Dog dog = JsonConvert.DeserializeObject<Dog>(responseString);

                    return dog;
                }
            }
        }
    }

    public class Otter
    {

        public static string GetRandomOtter()
        {
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            Regex regex = new Regex("<a href=\".*\">(?<name>.*)</a>");
            using (HttpResponseMessage response = client.GetAsync("https://vendell.online/img/otter/").Result)
            {
                using (Stream stream = response.Content.ReadAsStream())
                {
                    StreamReader reader = new StreamReader(stream, true);
                    String responseString = reader.ReadToEnd();

                    MatchCollection matches = regex.Matches(responseString);
                    List<Match> matchList = new List<Match>();
                    foreach (Match match in matches)
                    {
                        if (!match.Success) { continue; }
                        if (match.Groups["name"].Value == "Size" || match.Groups["name"].Value == "Parent Directory") { continue; }
                        matchList.Add(match);
                    }

                    Random rn = new Random();
                    string filename = matchList.ElementAt(rn.Next(0, matchList.Count)).Groups["name"].Value;
                    
                    return filename;
                }
            }
        }
    }

    public class Bird
    {
        public string image;

        public static Bird GetRandomBird()
        {
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            using (HttpResponseMessage response = client.GetAsync("https://some-random-api.com/animal/bird").Result)
            {
                using (Stream stream = response.Content.ReadAsStream())
                {
                    StreamReader reader = new StreamReader(stream, true);
                    String responseString = reader.ReadToEnd();
                    Bird bird = JsonConvert.DeserializeObject<Bird>(responseString);

                    return bird;
                }
            }
        }
    }
}
