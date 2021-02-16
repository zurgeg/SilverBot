﻿using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebClient = SilverBotDS.Objects.WebClient;

namespace SilverBotDS.Utils
{
    internal class NuGetUtils
    {
        /// <summary>
        /// Searches for a query on the nuget
        /// </summary>
        /// <param name="query">The query to search</param>
        /// <returns>A list of packages</returns>
        /// <exception cref="Exception">given when the webserver didnt return a OK</exception>
        public static async Task<Datum[]> SearchAsync(string query)
        {
            var httpClient = WebClient.Get();
            var uri = new UriBuilder("https://azuresearch-usnc.nuget.org/query")
            {
                Query = $"?q={query}"
            };
            var rm = await httpClient.GetAsync(uri.Uri);
            if (rm.StatusCode == HttpStatusCode.OK)
            {
                return JsonSerializer.Deserialize<Rootobject>(await rm.Content.ReadAsStringAsync())?.Data;
            }
            else
            {
                return await Task.FromException<Datum[]>(new Exception($"Request yielded a status code that isn't OK it is {rm.StatusCode}"));
            }
        }

        private class Rootobject
        {
            [JsonPropertyName("context")]
            public Context Context { get; set; }

            [JsonPropertyName("totalHits")]
            public int TotalHits { get; set; }

            [JsonPropertyName("data")]
            public Datum[] Data { get; set; }
        }

        public class Context
        {
            [JsonPropertyName("vocab")]
            public string Vocab { get; set; }

            [JsonPropertyName("_base")]
            public string Base { get; set; }
        }

        public class Datum
        {
            [JsonPropertyName("@id")]
            public string Atid { get; set; }

            [JsonPropertyName("@type")]
            public string Type { get;  }

            [JsonPropertyName("registration")]
            public string Registration { get; set; }

            [JsonPropertyName("id")]
            public string Id { get;  }

            [JsonPropertyName("version")]
            public string Version { get;  }

            [JsonPropertyName("description")]
            public string Description { get;  }

            [JsonPropertyName("summary")]
            public string Summary { get; set; }

            [JsonPropertyName("title")]
            public string Title { get;  }

            [JsonPropertyName("iconUrl")]
            public string IconUrl { get;  }

            [JsonPropertyName("licenseUrl")]
            public string LicenseUrl { get; set; }

            [JsonPropertyName("projectUrl")]
            public string ProjectUrl { get;  }

            [JsonPropertyName("tags")]
            public string[] Tags { get; set; }

            [JsonPropertyName("authors")]
            public string[] Authors { get;  }

            [JsonPropertyName("totalDownloads")]
            public int TotalDownloads { get;  }

            [JsonPropertyName("verified")]
            public bool Verified { get;  }

            [JsonPropertyName("packageTypes")]
            public Packagetype[] PackageTypes { get; set; }

            [JsonPropertyName("versions")]
            public Version[] Versions { get; set; }
        }

        public class Packagetype
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
        }

        public class Version
        {
            [JsonPropertyName("version")]
            public string StrVersion { get; set; }

            [JsonPropertyName("downloads")]
            public int Downloads { get; set; }

            [JsonPropertyName("id")]
            public string Id { get; set; }
        }
    }
}