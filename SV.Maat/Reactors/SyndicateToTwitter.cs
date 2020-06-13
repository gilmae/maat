using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Events;
using Microsoft.Extensions.Logging;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;
using SV.Maat.lib;
using SV.Maat.lib.Pipelines;
using SV.Maat.Projections;
using Tweetinvi;
using Tweetinvi.Parameters;

namespace SV.Maat.Reactors
{
    public class SyndicateEntry
    {
        private readonly ILogger<SyndicateEntry> _logger;
        private readonly EventDelegate _next;
        private readonly IEntryProjection _entries;
        public SyndicateEntry(ILogger<SyndicateEntry> logger, EventDelegate next, IEntryProjection entries)
        {
            _logger = logger;
            _next = next;
            _entries = entries;
        }

        public async Task InvokeAsync(Event e)
        {
            Syndicated syndicated = e as Syndicated;
            if (syndicated != null && syndicated.SyndicationAccount == "Twitter") // TODO - fix the SyndicationAccount detection
            {
                _logger.LogTrace($"Syndicating {e.AggregateId} to {syndicated.SyndicationAccount}");
                Entry entry = null;
                int attempts = 0;

                while (entry == null && entry.Version != e.Version && attempts < 10)
                {
                    entry = _entries.Get(e.AggregateId);
                    attempts += 1;
                    Thread.Sleep(TimeSpan.FromSeconds(1)); // Wait for the projection to be eventually consistent
                }

                if (entry == null)
                {
                    return;
                }

               await TweetIt(entry);
            }

            await _next(e);
        }

        public async Task TweetIt(Entry entry)
        {
            var client = new Tweetinvi.TwitterClient("consumerKey", "consumerSecret", "accessToken", "accessSecret");

            var media_upload_ids = new List<long>();
            foreach (var m in entry.AssociatedMedia)
            {
                var data = await Downloader.Download(m.Url);
                if (data != null)
                {
                    var media_upload = await client.Upload.UploadBinaryAsync(data);
                    if (media_upload.Id.HasValue)
                    {
                        await client.Upload.AddMediaMetadataAsync(new AddMediaMetadataParameters(media_upload.Id) { AltText = m.Description });
                        media_upload_ids.Add(media_upload.Id.Value);
                    }
                }
            }
            await client.Tweets.PublishTweetAsync(new PublishTweetParameters { Text = entry.Body, MediaIds = media_upload_ids });
        }
    }

    public static class SyndicateEntryUtils
    {
        public static PipelineBuilder UseSyndicateEntry(this PipelineBuilder pipeline)
        {
            return pipeline.UseReactor<SyndicateEntry>();
        }
    }
}
