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
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;
using SV.Maat.lib;
using SV.Maat.lib.Pipelines;
using SV.Maat.Projections;

namespace SV.Maat.Webmention
{
    public class WebmentionSender
    {
        ILogger<WebmentionSender> _logger;
        EventDelegate _next;
        IEntryProjection _entries;

        public WebmentionSender(ILogger<WebmentionSender> logger,
            EventDelegate next,
            IEntryProjection entries)
        {
            _logger = logger;
            _next = next;
            _entries = entries;
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

            var links = entry.Body.DiscoverLinks();
            var receivers = DiscoverWebMentionReceivers(links);

            foreach(var receiver in receivers)
            {
                foreach (string link in receiver.Value)
                {
                    SendWebMentionToReceiver(link, receiver.Key);
                }
            }

        }

        void SendWebMentionToReceiver(string link, string receiver)
        {
            throw new NotImplementedException();
        }

        Dictionary<string, IEnumerable<string>> DiscoverWebMentionReceivers(IEnumerable<string> links)
        {
            return links.Select(FindReceiver)
                        .GroupBy(i => i.link)
                        .Select(g => new { receiver = g.Key, links = g.Select(i => i.receiver) })
                        .ToDictionary(i => i.receiver, i => i.links);

        }

        (string link, string receiver) FindReceiver(string link)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(link);
            request.AllowAutoRedirect = true;

            using (var response = request.GetResponse())
            {
                //Find first Http Link with rel="webmention"
                var linkHeader = HttpHeaders.ParseHttpLinkHeader(response.Headers["link"]).FirstOrDefault(h=>h.Params.ContainsKey("rel") && h.Params["rel"] == "webmention");
                if (linkHeader != null)
                {
                    return (link, linkHeader.Url);
                }

                // Find first link in <head> with rel="webmention"
                var parser = new HtmlParser(new HtmlParserOptions
                {
                    IsNotConsumingCharacterReferences = true,
                });
                var doc = parser.ParseDocument(response.GetResponseStream());
                var headLink = doc
                    .QuerySelectorAll("head link[rel=webmention]")
                    .Select(l=>l.GetAttribute("href"))
                    .FirstOrDefault(h=>!string.IsNullOrEmpty(h));
                if (!string.IsNullOrEmpty(headLink))
                {
                    return (link, headLink);
                }

                // Find first <a rel="webmention">
                var bodyLink = doc
                    .QuerySelectorAll("body a[rel=webmention]")
                    .Select(l => l.GetAttribute("href"))
                    .FirstOrDefault(h => !string.IsNullOrEmpty(h));
                if (!string.IsNullOrEmpty(bodyLink))
                {
                    return (link, bodyLink);
                }

            }
            return (link, string.Empty);
        }
    }
}
