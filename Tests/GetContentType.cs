using System;
using Xunit;
using SV.Maat.lib;

namespace Tests
{
    public class GetContentType
    {
        [Fact]
        public void WebPageIsTextHtml()
        {
            Assert.Contains("text/html", HttpHeaders.GetContentType("https://google.com"));
        }

    }
}
