using System.IO;
using System.Security.Cryptography.X509Certificates;
using JsonFx.Json;
using MySql.Data.MySqlClient;
using MySQLJSONConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace trackListingGrabber
{
    class Program
    {
        static void Main(string[] args)
        {
            string cs = @"server=localhost;userid=localhost;
            password=pw;database=music";

            MySqlConnection conn = null;
            conn = new MySqlConnection(cs);
            conn.Open();
            Console.WriteLine("MySQL version : {0}", conn.ServerVersion);

            Scraper(conn, 209);
        }

        private static void Scraper(MySqlConnection conn, int channelID)
        {
            WebClient client = new WebClient();
            client.Headers["User-Agent"] =
                "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/33.0.1750.117 Safari/537.36";

            int failCount = 0;

            while (true)
            {
                Int32 unixTimestamp = (Int32) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                Console.WriteLine("Downloading");

                string[] stringSeparator1 = {"_AudioAddict_TrackHistory_WP("};
                string[] stringSeparator2 = {");"};
                string json = null;

                try
                {
                    json = client.DownloadString(
                        "http://api.audioaddict.com/v1/di/track_history/channel/" + channelID +
                        ".jsonp?callback=_AudioAddict_TrackHistory_WP");
                }
                catch
                {
                    failCount = failCount + 1;
                    if (failCount == 3)
                    {
                        Console.WriteLine("Failed 3 times, closing!");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Got an error on the audio addict API, sleeping for 1 minute");
                        Thread.Sleep(60 * 1000); // One Minute
                    }
                }
                if (json != null)
                {
                    json = json.Split(stringSeparator1, StringSplitOptions.None)[1];
                    json = json.Split(stringSeparator2, StringSplitOptions.None)[0];

                    var reader = new JsonReader();

                    dynamic tracks = reader.Read(json);
                    int lastTrackI = 0;

                    dynamic track = tracks[0];

                    if (track.type == "advertisement")
                    {
                        Console.WriteLine("Advert - sleeping for 10");
                        Thread.Sleep(10*1000); // 10 seconds
                    }
                    else
                    {
                        if (track.track_id != lastTrackI)
                        {
                            lastTrackI = track.track_id;

                            MySqlCommand cmd = new MySqlCommand();
                            cmd.Connection = conn;
                            cmd.CommandText =
                                "INSERT INTO tracks(channel_id, title, track, track_id, artist, art_url, started) VALUES(@channel_id,@title,@track,@track_id,@artist,@art_url,@started)";
                            cmd.Prepare();
                            cmd.Parameters.AddWithValue("@channel_id", channelID);
                            cmd.Parameters.AddWithValue("@title", track.title);
                            cmd.Parameters.AddWithValue("@track", track.track);
                            cmd.Parameters.AddWithValue("@track_id", track.track_id);
                            cmd.Parameters.AddWithValue("@artist", track.artist);
                            cmd.Parameters.AddWithValue("@art_url", track.art_url);
                            cmd.Parameters.AddWithValue("@started", track.started);
                            cmd.ExecuteNonQuery();

                            //channel_id
                            Console.WriteLine(track.title);
                        }
                        Thread.Sleep(60*1000); // One Minute
                    }
                }
            }
        }
    }
}
