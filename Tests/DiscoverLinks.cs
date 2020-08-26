using System;
using System.Linq;
using StrangeVanilla.Blogging.Events;
using SV.Maat.lib;
using Xunit;
namespace Tests
{
    public class DiscoverLinks
    {
        [Fact]
        public void IfNoLinksReturnsEmptySet()
        {
            var testString = "lorum ipsum";

            var result = testString.DiscoverLinks();

            Assert.Empty(result);
        }

        [Fact]
        public void IfOneLinksReturnsLink()
        {
            var testString = "lorum ipsum https://google.com";

            var result = testString.DiscoverLinks();

            Assert.Single(result);
            Assert.Equal("https://google.com", result.First());
        }

        [Fact]
        public void IfManyLinksReturnsAllLinks()
        {
            var testString = "lorum ipsum https://google.com lorum ipsum http://example.com";

            var result = testString.DiscoverLinks();

            Assert.Equal(2, result.Count());
            Assert.Equal("https://google.com", result.First());
            Assert.Equal("http://example.com", result.Last());
        }

        [Fact]
        public void IfDuplicatedLinksReturnsDedupedLinks()
        {
            var testString = "lorum ipsum https://google.com lorum ipsum https://google.com";

            var result = testString.DiscoverLinks();

            Assert.Single(result);
            Assert.Equal("https://google.com", result.First());
        }

        [Fact]
        public void IfContentSearchesMarkupAndPlainTextForLinksAndDedupes()
        {
            var testContent = new Content
            {
                Value = "lorum ipsum https://google.com",
                Markup = "<p><a href='https://google.com'>lorum ipsum</a></p>"
            };


            var result = testContent.DiscoverLinks();

            Assert.Single(result);
            Assert.Contains("https://google.com", result);
        }

        [Fact]
        public void IfContentSearchesMarkupAndPlainTextForLinks()
        {
            var testContent = new Content
            {
                Value = "lorum ipsum https://example.com",
                Markup = "<p><a href='https://google.com'>lorum ipsum</a></p>"
            };
            

            var result = testContent.DiscoverLinks();

            Assert.Equal(2, result.Count());
            Assert.Contains("https://google.com", result);
            Assert.Contains("https://example.com", result);
        }


    }
}
