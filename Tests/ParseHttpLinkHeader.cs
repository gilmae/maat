using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static SV.Maat.lib.HttpHeaders;

namespace Tests
{
    public class ParseHttpLinkHeader
    {
        [Fact]
        public void HeaderWithSingleLinkReturnsOneLink()
        {
            var result = SV.Maat.lib.HttpHeaders.ParseHttpLinkHeader(@"<https://example.com>; rel='preconnect'");
            var usableResult = new List<HttpLink>(result);

            Assert.Single(result);
            Assert.Equal("https://example.com", usableResult[0].Url);
            Assert.Single(usableResult[0].Params);
            Assert.True(usableResult[0].Params.ContainsKey("rel"));
            Assert.Equal("preconnect", usableResult[0].Params["rel"]);
        }

        [Fact]
        public void HeaderWithTwoLinkReturnsTwoLinks()
        {
            var result = SV.Maat.lib.HttpHeaders.ParseHttpLinkHeader(@"<https://example.com>; rel='preconnect',<https://two.example.com>; rel='unconnect'");
            var usableResult = new List<HttpLink>(result);
            Assert.Equal("https://example.com", usableResult[0].Url);
            Assert.Single(usableResult[0].Params);
            Assert.True(usableResult[0].Params.ContainsKey("rel"));
            Assert.Equal("preconnect", usableResult[0].Params["rel"]);

            Assert.Equal("https://two.example.com", usableResult[1].Url);
            Assert.Single(usableResult[1].Params);
            Assert.True(usableResult[1].Params.ContainsKey("rel"));
            Assert.Equal("unconnect", usableResult[1].Params["rel"]);
        }

        [Fact]
        public void HeaderWithMultipleParamsLinkReturnsOneLinkWithMultipleParams()
        {
            var result = SV.Maat.lib.HttpHeaders.ParseHttpLinkHeader(@"<https://example.com>; rel='preconnect'; media='print'");

            Assert.Single(result);

            var usableResult = new List<HttpLink> ( result );
            Assert.Equal("https://example.com", usableResult[0].Url);
            Assert.Equal(2, usableResult[0].Params.Count);
            Assert.True(usableResult[0].Params.ContainsKey("rel"));
            Assert.Equal("preconnect", usableResult[0].Params["rel"]);
            Assert.True(usableResult[0].Params.ContainsKey("media"));
            Assert.Equal("print", usableResult[0].Params["media"]);
        }

        [Fact]
        public void HeaderWithInvalidlyEscapedUrlReturnsEmpty()
        {
            var result = SV.Maat.lib.HttpHeaders.ParseHttpLinkHeader(@"https://example.com; rel='preconnect'; media='print'");

            Assert.Empty(result);
        }
    }
}
