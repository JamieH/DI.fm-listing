using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace trackListingGrabber
{
    class Program
    {
        static void Main(string[] args)
        {
        
            WebClient client = new WebClient();

            client.Headers["User-Agent"] =
                "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/33.0.1750.117 Safari/537.36";
            
            while (true)
            {
                Console.WriteLine("Downloading");

                string[] stringSeparator1 = { "_AudioAddict_TrackHistory_WP(" };
                string[] stringSeparator2 = { ");" };

                var json = client.DownloadString(
                    "http://api.audioaddict.com/v1/di/track_history/channel/209.jsonp?callback=_AudioAddict_TrackHistory_WP");
                json = json.Split(stringSeparator1, StringSplitOptions.None)[1];
                json = json.Split(stringSeparator2, StringSplitOptions.None)[0];
                var tracks = JsonConvert.DeserializeObject<dynamic>(json);

                foreach (dynamic track in tracks)
                {
                    if (track["ad"] != null)
                    {
                        Console.WriteLine("Advert");
                    }
                    else
                    {
                        Console.WriteLine(track["title"]);
                    }
                }

                Thread.Sleep(60 * 1000); // One Minute
            }
        }
    }
}
