using System;
using Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StrangeVanilla.Blogging.Events;
using SV.Maat.lib.FileStore;
using SV.Maat.lib.MessageBus;

namespace SV.Maat.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MicropubController : ControllerBase
    {

        private readonly ILogger<MicropubController> _logger;
        IEventStore<Entry> _entryRepository;
        IEventStore<Media> _mediaRepository;
        IMessageBus<Entry> _entryBus;
        IFileStore _fileStore;

        public MicropubController(ILogger<MicropubController> logger,
            IEventStore<Entry> entryRepository,
            IEventStore<Media> mediaRepository,
            IMessageBus<Entry> entryBus,
            IFileStore fileStore)
        {
            _logger = logger;
            _entryRepository = entryRepository;
            _mediaRepository = mediaRepository;
            _entryBus = entryBus;
            _fileStore = fileStore;
        }

        [HttpGet]
        [Route("/")]
        public void Create()
        {
            return;
        }


    }
}
