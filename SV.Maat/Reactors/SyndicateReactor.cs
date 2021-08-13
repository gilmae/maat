using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        //static ActivitySource s_source = new ActivitySource("Maat.SyndicateReactor");

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
            //using (Activity a = s_source.StartActivity("Maat.Syndication"))
            //{
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                try
                {
                    //a.AddTag("syndication.account", syndicated.SyndicationAccount);
                    var syndication = _syndicationStore.FindByAccountName(syndicated.SyndicationAccount);

                    if (syndication == null)
                    {
                        _logger.LogDebug($"No syndication account for {syndicated.SyndicationAccount}");
                        return;
                    }

                    //a.AddTag("syndication.network", syndication.Network);
                    var network = _externalNetworks.First(n => n.Name.ToLower() == syndication.Network.ToLower());

                    if (network == null)
                    {
                        _logger.LogDebug($"No network for {syndication.Network.ToLower()}");
                        return;
                    }

                    var credentials = _tokenSigning.Decrypt<Credentials>(syndication.Credentials);

                    _logger.LogDebug($"Syndicating {syndicated.AggregateId} to {syndicated.SyndicationAccount}");
                    //a.AddTag("syndication.aggregate.id", syndicated.AggregateId);
                    Post post = null;
                    Micropub.Models.Entry entry = null;
                    int attempts = 0;

                    while ((post == null || post.Version != syndicated.Version) && attempts < 10)
                    {
                        entry = _entries.Get(syndicated.AggregateId);
                        attempts += 1;
                        if (entry == null)
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(1)); // Wait for the projection to be eventually consistent
                        }
                    }

                    //a.AddTag("syndication.aggregate.load_attempts", attempts);

                    if (entry == null)
                    {
                        _logger.LogDebug($"Could not find {syndicated.AggregateId} in projection");
                        return;
                    }

                    IList<string> replyingTo = GetSyndicationsOfReplyToParent(entry);

                    string inNetworkReplyTo = replyingTo.FirstOrDefault(u => network.IsUrlForNetwork(credentials, u));

                    var syndicatedUrl = network.Syndicate(
                        credentials,
                        post,
                        inNetworkReplyTo
                    );
                    //a.AddTag("syndication.aggregate.success", !string.IsNullOrEmpty(syndicatedUrl));

                    _logger.LogDebug($"Syndicated {syndicated.AggregateId} as {syndicatedUrl}");
                    _commandHandler.Handle<Entry>(syndicated.AggregateId, new PublishSyndication { SyndicationUrl = syndicatedUrl, Network = network.Name });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            //}
        }

        public  IList<string> GetSyndicationsOfReplyToParent(Micropub.Models.Entry entry)
        {
            IList<string> replyingTo = new List<string>();
            if (!string.IsNullOrEmpty(entry.ReplyTo))
            {
                replyingTo.Add(entry.ReplyTo);

                // If ReplyTo is a Maat Entry, then GetEntryIdFromUrl will return a non-Empty Guid
                Micropub.Models.Entry parentEntry = _entries.Get(new Uri(entry.ReplyTo)?.AbsolutePath);

                if (parentEntry != null && parentEntry.Syndications != null)
                {
                    replyingTo = replyingTo.Union(parentEntry.Syndications.Select(s=>s.ToString())).ToList(); 
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
