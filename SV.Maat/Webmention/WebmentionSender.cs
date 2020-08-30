using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using Events;
using Microsoft.Extensions.Logging;
using RestSharp;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;
using SV.Maat.lib;
using SV.Maat.lib.Pipelines;
using SV.Maat.Projections;
using Users;

namespace SV.Maat.Webmention
{
    public class WebmentionSender
    {
        ILogger<WebmentionSender> _logger;
        EventDelegate _next;
        IEntryProjection _entries;
        IUserStore _userStore;

        public WebmentionSender(ILogger<WebmentionSender> logger,
            EventDelegate next,
            IEntryProjection entries,
            IUserStore userStore)
        {
            _logger = logger;
            _next = next;
            _entries = entries;
            _userStore = userStore;
        }

        public async Task InvokeAsync(Event e)
        {
            if (e is EntryPublished)
            {
                HandleEvent(e as EntryPublished);
            }
            await _next(e);
        }

        public void HandleEvent(EntryPublished e)
        {
            if (e == null)
            {
                return;
            }

            Entry entry = null;
            int attempts = 0;
            while ((entry == null || entry.Version != e.Version) && attempts < 10)
            {
                entry = _entries.Get(e.AggregateId);
                attempts += 1;
                if (entry == null)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1)); // Wait for the projection to be eventually consistent
                }
            }

            if (entry == null)
            {
                _logger.LogTrace($"Could not find {e.AggregateId} in projection");
                return;
            }

            string entryUrl = UrlHelper.EntryUrl(entry, _userStore.Find(e.Id));

            var links = entry.Body.DiscoverLinks();
            var receivers = DiscoverWebMentionReceivers(links);

            foreach(var receiver in receivers)
            {
                foreach (string link in receiver.Value)
                {
                    SendWebMentionToReceiver(entryUrl, link, receiver.Key);
                }
            }

        }

        void SendWebMentionToReceiver(string entryUrl, string link, string receiver)
        {
            var client = new RestClient(receiver);
            var request = new RestRequest()
                .AddParameter("source", entryUrl)
                .AddParameter("target", link);

            var response = client.Post(request);
            if (response.IsSuccessful)
            {
                _logger.LogInformation($"Sent WebMention from {entryUrl} to {link}");
            }
            else
            {
                _logger.LogInformation($"WebMention from {entryUrl} to {link} rejected with status {response.StatusCode}");
            }
        }

        Dictionary<string, IEnumerable<string>> DiscoverWebMentionReceivers(IEnumerable<string> links)
        {
            return links.Select(WebMention.FindReceiver)
                        .GroupBy(i => i.link)
                        .Select(g => new { receiver = g.Key, links = g.Select(i => i.receiver) })
                        .ToDictionary(i => i.receiver, i => i.links);

        }

        
    }
}
