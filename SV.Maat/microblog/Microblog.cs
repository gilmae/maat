using System;
using System.Linq;
using RestSharp;
using StrangeVanilla.Blogging.Events;
using SV.Maat.ExternalNetworks;
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

        public string Syndicate(Credentials credentials, Entry entry, string inNetworkReplyingTo = null)
        {
            var converter = new EntryToMicropubConverter();
            var properties = converter.ToDictionary(entry);

            properties.Remove("mp-syndicate-to");
            properties.Remove("syndication");
            if (!string.IsNullOrEmpty(inNetworkReplyingTo))
            {
                properties["in-reply-to"] = inNetworkReplyingTo;
            }

            var client = new RestClient($"https://micro.blog");
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
}
