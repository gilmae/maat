using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using SV.Maat.lib;

namespace SV.Maat.IndieAuth
{
    public class MicroformatParser
    {
        public MicroformatParser()
        {
        }

        public async Task<IEnumerable<App>> GetApps(string site)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(site);

            var apps = document.QuerySelectorAll(".h-x-app, .h-app");


            return await Task.FromResult(apps.Select(ParseApp));
        }

        private App ParseApp(IElement node)
        {
            
            App app = new App();
            var nameProp = node.QuerySelectorAll(".p-name").FirstOrDefault();
            app.Name = (nameProp??node)?.TextContent;

            var urlProp = node.QuerySelectorAll(".u-url").FirstOrDefault();
            app.Url = (urlProp ?? node).Attributes["href"]?.Value;

            var logoProp = node.QuerySelectorAll(".u-logo").FirstOrDefault();
            app.Logo = (logoProp ?? node).Attributes["src"]?.Value;

            if (!app.Logo.IsAbsoluteUri())
            {
                app.Logo = node.BaseUrl.ToString().GetPathOfResourceRelativeToBase(app.Logo);
            }

            return app;
        }
    }

    public class App
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Logo { get; set; }
    }
}
