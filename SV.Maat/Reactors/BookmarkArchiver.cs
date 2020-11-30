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
using SV.Maat.lib;

namespace SV.Maat.Reactors
{
    public class BookmarkArchiver
    {
        private readonly ILogger<BookmarkArchiver> _logger;
        private readonly EventDelegate _next;
        private readonly IEntryProjection _entries;
        private CommandHandler _commandHandler;
        private IFileStore _fileStore;

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
            _fileStore = fileStore;
        }

        public async Task InvokeAsync(Event e)
        {
            if (e is ContentSet2)
            {
                HandleBookmark(((ContentSet2)e)?.BookmarkOf, e.AggregateId);
            }
            else if (e is ContentAdded2)
            {
                HandleBookmark(((ContentAdded2)e)?.BookmarkOf, e.AggregateId);
            }
            await _next(e);
        }

        private void HandleBookmark(string url, Guid aggregateId)
        {
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            string savePath;
            string contentType;
            (savePath, contentType) = DownloadUrl(url);

            AttachPdf(savePath, contentType, aggregateId);
            
        }

        private void AttachPdf(string archivedVersion, string contentType, Guid entryId)
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
                MimeType = contentType,
                SavePath = filePath
            };

            _commandHandler.Handle<Media>(id, mediaUpload);

            string attachmentUrl = $"/media/{id}";

            var attachMediaToEntry = new AttachMediaToEntry() { Type = MediaType.archivedCopy.ToString() , Url = attachmentUrl, Description = "Archived PDF of bookmarked url" };
            _commandHandler.Handle<Entry>(entryId, attachMediaToEntry);
        }

        private (string, string) DownloadUrl(string url)
        {
            string tempLocation = Path.Join(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");

            string contentType = HttpHeaders.GetContentType(url);
            Func<string, string, bool> downloader = DownloadAsOriginal;
            if (contentType == "text/html") { 
                downloader = DownloadWebPageAsPdf;
            }

            if (downloader.Invoke(url, tempLocation))
            {
                return (tempLocation,contentType);
            }
            return (string.Empty, string.Empty);

            
        }

        private bool DownloadAsOriginal(string url, string savePath)
        {
            byte[] content = Downloader.Download(url).Result;
            File.WriteAllBytes(savePath, content);
            return true;
        }

        private bool DownloadWebPageAsPdf(string url, string savePath)
        {
            string cmd = $"wkhtmltopdf {url} {savePath}";

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "xvfb-run",
                    Arguments = $"--server-args=\"-screen 0 640x480x16\" {cmd}",
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
                    return true;
                }
                else
                {
                    _logger.LogWarning("Download of url to pdf did not complete in time");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in executing headless chrome");
                return false;
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
