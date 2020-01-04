using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Nancy;
using Nancy.TinyIoc;
using Events;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Maat.lib.MessageBus;

namespace StrangeVanilla.Maat
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        readonly IServiceProvider _serviceProvider;

        public Bootstrapper(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            container.Register(_serviceProvider.GetService<ILogger<NancyModule>>());
            container.Register(_serviceProvider.GetService<IEventStore<Entry>>());
            container.Register(_serviceProvider.GetService<IEventStore<Media>>());
            container.Register(_serviceProvider.GetService<IMessageBus<Entry>>());
        }

        protected override byte[] FavIcon
        {
            get { return null; }
        }

    }
}
