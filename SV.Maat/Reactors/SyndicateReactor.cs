using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Events;
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

        public SyndicateEntry(ILogger<SyndicateEntry> logger,
            EventDelegate next,
            IEntryProjection entries,
            IEnumerable<ISyndicationNetwork> externalNetworks,
            ISyndicationStore syndicationStore,
            TokenSigning tokenSigning,
            CommandHandler commandHandler)
        {
            _logger = logger;
            _next = next;
            _entries = entries;
            _syndicationStore = syndicationStore;
            
            _tokenSigning = tokenSigning;
            _externalNetworks = externalNetworks; _externalNetworks = externalNetworks;
            _commandHandler = commandHandler;
        }

        public async Task InvokeAsync(Event e)
        {
            try
            {
                Syndicated syndicated = e as Syndicated;
                if (syndicated != null) // TODO - fix the SyndicationAccount detection
                {
                    var syndication = _syndicationStore.FindByAccountName(syndicated.SyndicationAccount);

                    if (syndication == null)
                    {
                        return;
                    }

                    var network = _externalNetworks.First(n => n.Name.ToLower() == syndication.Network.ToLower());

                    if (network == null)
                    {
                        return;
                    }

                    var credentials = _tokenSigning.Decrypt<Credentials>(syndication.Credentials);

                    _logger.LogTrace($"Syndicating {e.AggregateId} to {syndicated.SyndicationAccount}");
                    Entry entry = null;
                    int attempts = 0;

                    while ((entry == null || entry.Version != e.Version) && attempts < 10)
                    {
                        entry = _entries.Get(e.AggregateId);
                        attempts += 1;
                        Thread.Sleep(TimeSpan.FromSeconds(1)); // Wait for the projection to be eventually consistent
                    }

                    if (entry == null)
                    {
                        _logger.LogTrace($"Could not find {e.AggregateId} in projection");
                        return;
                    }

                    IList<string> replyingTo = GetSyndicationsOfReplyToParent(entry);

                    var syndicatedUrl = network.Syndicate(credentials, entry);
                    _logger.LogTrace($"Syndicated {e.AggregateId} as {syndicatedUrl}");
                    _commandHandler.Handle<Entry>(e.AggregateId, new PublishSyndication { SyndicationUrl = syndicatedUrl });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            await _next(e);
        }

        public IList<string> GetSyndicationsOfReplyToParent(Entry entry)
        {
            IList<string> replyingTo = new List<string>();
            if (!string.IsNullOrEmpty(entry.ReplyTo))
            {
                replyingTo.Add(entry.ReplyTo);

                // If ReplyTo is a Maat Entry, then GetEntryIdFromUrl will return a non-Empty Guid
                Guid? entryId = new Uri(entry.ReplyTo)?.GetEntryIdFromUrl();

                if (entryId.HasValue && entryId != Guid.Empty)
                {
                    var parentEntry = _entries.Get(entryId.Value);
                    if (parentEntry != null && parentEntry.Syndications != null)
                    {
                        replyingTo = replyingTo.Union(parentEntry.Syndications).ToList();
                    }
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
