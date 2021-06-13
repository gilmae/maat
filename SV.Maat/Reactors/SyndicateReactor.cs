using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Events;
using Honeycomb.AspNetCore;
using Microsoft.Extensions.Logging;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;
using SV.Maat.Commands;
using SV.Maat.ExternalNetworks;
using SV.Maat.IndieAuth;
using SV.Maat.lib;
using SV.Maat.lib.Pipelines;
using SV.Maat.Projections;
using SV.Maat.Syndications;

[assembly: InternalsVisibleTo("Tests")]
namespace SV.Maat.Reactors
{
    public class SyndicateEntry
    {
        private readonly ILogger<SyndicateEntry> _logger;
        private readonly EventDelegate _next;
        private readonly IEntryProjection _entries;
        private readonly IEnumerable<ISyndicationNetwork> _externalNetworks;
        readonly ISyndicationStore _syndicationStore;
        private readonly TokenSigning _tokenSigning;
        private CommandHandler _commandHandler;
        private IHoneycombEventManager _eventManager;

        public SyndicateEntry(ILogger<SyndicateEntry> logger,
            EventDelegate next,
            IEntryProjection entries,
            IEnumerable<ISyndicationNetwork> externalNetworks,
            ISyndicationStore syndicationStore,
            TokenSigning tokenSigning,
            CommandHandler commandHandler,
            IHoneycombEventManager eventManager)
        {
            _logger = logger;
            _next = next;
            _entries = entries;
            _syndicationStore = syndicationStore;
            
            _tokenSigning = tokenSigning;
            _externalNetworks = externalNetworks; _externalNetworks = externalNetworks;
            _commandHandler = commandHandler;
            _eventManager = eventManager;
        }

        public async Task InvokeAsync(Event e)
        {
            if (e is Syndicated)
            {
                HandleEvent(e as Syndicated);
            }
            await _next(e);
        }

        public void HandleEvent(Syndicated syndicated)
        {
            if (syndicated == null)
            {
                return;
            }
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                //_eventManager.AddData("syndication.data", syndicated);
                var syndication = _syndicationStore.FindByAccountName(syndicated.SyndicationAccount);

                if (syndication == null)
                {
                    _logger.LogTrace($"No syndication account for {syndicated.SyndicationAccount}");
                    return;
                }

                //_eventManager.AddData("syndication.account", syndication);

                var network = _externalNetworks.First(n => n.Name.ToLower() == syndication.Network.ToLower());

                if (network == null)
                {
                    _logger.LogTrace($"No network for {syndication.Network.ToLower()}");
                    return;
                }

                var credentials = _tokenSigning.Decrypt<Credentials>(syndication.Credentials);

                _logger.LogTrace($"Syndicating {syndicated.AggregateId} to {syndicated.SyndicationAccount}");
                Entry entry = null;
                int attempts = 0;

                while ((entry == null || entry.Version != syndicated.Version) && attempts < 10)
                {
                    entry = _entries.Get(syndicated.AggregateId);
                    attempts += 1;
                    if (entry == null)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(1)); // Wait for the projection to be eventually consistent
                    }
                }

                //_eventManager.AddData("syndication.waiting_for_consistency_attempts", attempts);

                if (entry == null)
                {
                    _logger.LogTrace($"Could not find {syndicated.AggregateId} in projection");
                    return;
                }

                IList<string> replyingTo = GetSyndicationsOfReplyToParent(entry);

                string inNetworkReplyTo = replyingTo.FirstOrDefault(u => network.IsUrlForNetwork(credentials, u));

                var syndicatedUrl = network.Syndicate(
                    credentials,
                    entry,
                    inNetworkReplyTo
                );
                _logger.LogTrace($"Syndicated {syndicated.AggregateId} as {syndicatedUrl}");
                _commandHandler.Handle<Entry>(syndicated.AggregateId, new PublishSyndication { SyndicationUrl = syndicatedUrl, Network = network.Name });
                //_eventManager.AddData("syndication.duration", stopwatch.ElapsedMilliseconds);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        public  IList<string> GetSyndicationsOfReplyToParent(Entry entry)
        {
            IList<string> replyingTo = new List<string>();
            if (!string.IsNullOrEmpty(entry.ReplyTo))
            {
                replyingTo.Add(entry.ReplyTo);

                // If ReplyTo is a Maat Entry, then GetEntryIdFromUrl will return a non-Empty Guid
                Entry parentEntry = _entries.Get(new Uri(entry.ReplyTo)?.AbsolutePath);

                if (parentEntry != null && parentEntry.Syndications != null)
                {
                    replyingTo = replyingTo.Union(parentEntry.Syndications.Select(s=>s.Url)).ToList();
                }
            }

            return replyingTo;
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
