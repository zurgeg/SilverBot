﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SilverBotDS.Utils
{
    internal class OnlyHitUtils
    {
        public static async Task<CurrentSong> SearchAsync(string url = "https://api.onlyhit.us/fingerprinting/onlyhit.json")
        {
            System.Net.Http.HttpClient httpClient = WebClient.Get();
            UriBuilder uri = new UriBuilder(url);
            HttpResponseMessage RM = await httpClient.GetAsync(uri.Uri);
            if (RM.StatusCode == HttpStatusCode.OK)
            {
                var cursong = JsonSerializer.Deserialize<CurrentSong>(await RM.Content.ReadAsStringAsync());
                if (cursong.Status.Msg == "Success")
                {
                    return cursong;
                }
                return await Task.FromException<CurrentSong>(new Exception($"Requested yielded a message that isnt Success it is {cursong.Status.Msg}"));
            }
            else
            {
                return await Task.FromException<CurrentSong>(new Exception($"Request yielded a statuscode that isnt OK it is {RM.StatusCode}"));
            }
        }

        public class CurrentSong
        {
            [JsonPropertyName("status")]
            public Status Status { get; set; }

            [JsonPropertyName("result_type")]
            public int Result_type { get; set; }

            [JsonPropertyName("metadata")]
            public Metadata Metadata { get; set; }
        }

        public class Status
        {
            [JsonPropertyName("msg")]
            public string Msg { get; set; }

            [JsonPropertyName("code")]
            public int Code { get; set; }

            [JsonPropertyName("version")]
            public string Version { get; set; }
        }

        public class Metadata
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("timestamp_utc")]
            public string Timestamp_utc { get; set; }

            [JsonPropertyName("music")]
            public Music[] Music { get; set; }
        }

        public class Music
        {
            [JsonPropertyName("album")]
            public Album Album { get; set; }

            [JsonPropertyName("play_offset_ms")]
            public int Play_offset_ms { get; set; }

            [JsonPropertyName("sample_begin_time_offset_ms")]
            public int Sample_begin_time_offset_ms { get; set; }

            [JsonPropertyName("contributors")]
            public Contributors Contributors { get; set; }

            [JsonPropertyName("title")]
            public string Title { get; set; }

            [JsonPropertyName("result_from")]
            public int Result_from { get; set; }

            [JsonPropertyName("release_date")]
            public string Release_date { get; set; }

            [JsonPropertyName("sample_end_time_offset_ms")]
            public int Sample_end_time_offset_ms { get; set; }

            [JsonPropertyName("genres")]
            public Genre[] Genres { get; set; }

            [JsonPropertyName("label")]
            public string Label { get; set; }

            [JsonPropertyName("db_end_time_offset_ms")]
            public int Db_end_time_offset_ms { get; set; }

            [JsonPropertyName("score")]
            public int Score { get; set; }

            [JsonPropertyName("db_begin_time_offset_ms")]
            public int Db_begin_time_offset_ms { get; set; }

            [JsonPropertyName("artists")]
            public Artist2[] Artists { get; set; }

            [JsonPropertyName("duration_ms")]
            public int Duration_ms { get; set; }

            [JsonPropertyName("external_ids")]
            public External_Ids External_ids { get; set; }

            [JsonPropertyName("acrid")]
            public string Acrid { get; set; }

            [JsonPropertyName("external_metadata")]
            public External_Metadata External_metadata { get; set; }
        }

        public class Album
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
        }

        public class Contributors
        {
            [JsonPropertyName("composers")]
            public string[] Composers { get; set; }

            [JsonPropertyName("lyricists")]
            public string[] Lyricists { get; set; }
        }

        public class External_Ids
        {
            [JsonPropertyName("isrc")]
            public string Isrc { get; set; }

            [JsonPropertyName("upc")]
            public string Upc { get; set; }
        }

        public class External_Metadata
        {
            [JsonPropertyName("spotify")]
            public Spotify Spotify { get; set; }

            [JsonPropertyName("deezer")]
            public Deezer Deezer { get; set; }

            [JsonPropertyName("youtube")]
            public Youtube Youtube { get; set; }
        }

        public class Youtube
        {
            [JsonPropertyName("vid")]
            public string Vid { get; set; }
        }

        public class Spotify
        {
            [JsonPropertyName("album")]
            public Album1 Album { get; set; }

            [JsonPropertyName("track")]
            public Track Track { get; set; }

            [JsonPropertyName("artists")]
            public Artist[] Artists { get; set; }
        }

        public class Album1
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
        }

        public class Track
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("id")]
            public string Id { get; set; }
        }

        public class Artist
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
        }

        public class Deezer
        {
            [JsonPropertyName("album")]
            public Album2 Album { get; set; }

            [JsonPropertyName("track")]
            public Track1 Track { get; set; }

            [JsonPropertyName("artists")]
            public Artist1[] Artists { get; set; }
        }

        public class Album2
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
        }

        public class Track1
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("id")]
            public string Id { get; set; }
        }

        public class Artist1
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
        }

        public class Genre
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
        }

        public class Artist2
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
        }
    }
}