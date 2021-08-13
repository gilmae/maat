using System;
using System.Collections.Generic;
using System.Linq;
using mf;
using Microsoft.Extensions.DependencyInjection;
using RestSharp;
using StrangeVanilla.Blogging.Events;
using SV.Maat.ExternalNetworks;
using SV.Maat.lib;
using SV.Maat.Micropub;

namespace SV.Maat.Microblog
{
    public class Microblog : IRequiresBearerToken, ISyndicationNetwork
    {
        public Microblog()
        {
        }

        public string Name => "Microblog";

        public string Photo => "/images/networks/microblog.png";

        public string Url => "https://micro.blog";

        public bool IsUrlForNetwork(Credentials credentials, string url)
        {
            throw new NotImplementedException();
        }

        public string Syndicate(Credentials credentials, Post post, string inNetworkReplyingTo = null)
        {
            string[] propertiesToInclude = new[] { "name", "content", "category", "photo", "in-reply-to", "bookmark-of" };
            var properties = post.Data.Properties.DuplicateOnlyKeys(propertiesToInclude);
            var client = new RestClient($"https://micro.blog");

            //if (post.Data.Properties.ContainsKey("photo") && post.Data.Properties["photo"] != null) {
            //    for (int ii=0;ii< post.Data.Properties["photo"].Length; ii++)
            //    {
            //        var request = new RestRequest("/micropub")
            //    .AddFileBytes("photo", Downloader.Download(post.Data.Properties["photo"][1] as string).Result, "photo");
            //    }
            //}
            
           
            var request = new RestRequest("/micropub")
                .AddJsonBody(new
                {
                    type = new [] {"h-entry"},
                    properties
                });

            request.AddHeader("Authorization", $"Bearer {credentials.Secret}");

            var response = client.Post(request);

            if (response.IsSuccessful)
            {
                return response.Headers.FirstOrDefault(h => h.Name == "Location").Value.ToString();
            }

            return string.Empty;

        }
    }

    public static class ServicesExtensions
    {
        public static void AddMicroblog(this IServiceCollection services)
        {
            services.AddTransient<ISyndicationNetwork, Microblog>();
        }
    }
}
