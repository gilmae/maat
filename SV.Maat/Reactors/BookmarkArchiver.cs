using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SV.Maat.lib.Pipelines;
using SV.Maat.Projections;
using StrangeVanilla.Blogging.Events.Entries.Events;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using Events;
using SV.Maat.lib.FileStore;
using SV.Maat.Commands;
using StrangeVanilla.Blogging.Events;
using System.IO;


namespace SV.Maat.Reactors
{
    public class BookmarkArchiver
    {
        private readonly ILogger<BookmarkArchiver> _logger;
        private readonly EventDelegate _next;
        private readonly IEntryProjection _entries;
        private CommandHandler _commandHandler;
        private IFileStore _fileStore;
        string _chromeLocation;

        public BookmarkArchiver(ILogger<BookmarkArchiver> logger,
            IConfiguration configuration,
            EventDelegate next,
            IEntryProjection entries,
            CommandHandler commandHandler,
            IFileStore fileStore)
        {
            _logger = logger;
            _next = next;
            _entries = entries;
            _commandHandler = commandHandler;
            _chromeLocation = configuration.GetConnectionString("Chrome");
            _fileStore = fileStore;
        }

        public async Task InvokeAsync(Event e)
        {
            if (e is ContentSet2)
            {
                HandleEvent(e as ContentSet2);
            }
            else if (e is ContentAdded2)
            {
                HandleEvent(e as ContentAdded2);
            }
            await _next(e);
        }

        private void HandleEvent(ContentSet2 e)
        {
            if (e == null || string.IsNullOrEmpty(e.BookmarkOf))
            {
                return;
            }
            AttachPdf(DownloadUrl(e.BookmarkOf), e.AggregateId);
            
        }

        private void HandleEvent(ContentAdded2 e)
        {
            if (e == null || string.IsNullOrEmpty(e.BookmarkOf))
            {
                return;
            }
            AttachPdf(DownloadUrl(e.BookmarkOf), e.AggregateId);

        }

        private void AttachPdf(string archivedVersion, Guid entryId)
        {
            if (string.IsNullOrEmpty(archivedVersion))
            {
                return;
            }

            string filePath = _fileStore.Save(File.ReadAllBytes(archivedVersion));
            Guid id = Guid.NewGuid();
            var mediaUpload = new CreateMedia()
            {
                Name = "Archived copy",
                MimeType = "application/pdf",
                SavePath = filePath
            };

            _commandHandler.Handle<Media>(id, mediaUpload);

            string attachmentUrl = $"/media/{id}";

            var attachMediaToEntry = new AttachMediaToEntry() { Type = MediaType.archivedCopy.ToString() , Url = attachmentUrl, Description = "Archived PDF of bookmarked url" };
            _commandHandler.Handle<Entry>(entryId, attachMediaToEntry);
        }

        private string DownloadUrl(string url)
        {
            string tempLocation = Path.Join(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
            string arguments = $"{url} {tempLocation}";

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "wkhtmltopdf",
                    Arguments = $"{arguments}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            try
            {
                process.Start();
                string result = process.StandardOutput.ReadToEnd();

                if (process.WaitForExit((int)TimeSpan.FromSeconds(30).TotalMilliseconds))
                {
                    return tempLocation;
                }
                else
                {
                    _logger.LogWarning("Download of url to pdf did not complete in time");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in executing headless chrome");
                return string.Empty;
            }
        }

    }

    public static class BookmarkArchiverUtils
    {
        public static PipelineBuilder UseBookmarkArchival(this PipelineBuilder pipeline)
        {
            return pipeline.UseReactor<BookmarkArchiver>();
        }
    }
}
