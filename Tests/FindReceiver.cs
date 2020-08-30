using System;
using SV.Maat.lib;
using Xunit;

namespace Tests
{
    public class FindReceiver
    {
        [Fact]
        public void HttpLink_UnquoteRel_Relative_Url()
        {
            var (_, receiver) = "https://webmention.rocks/test/1".FindReceiver();

            string expected = "https://webmention.rocks/test/1/webmention";

            Assert.Equal(expected, receiver);
        }

        [Fact]
        public void HttpLink_UnquotedRel_AbsoluteUrl()
        {
            var (_, receiver) = "https://webmention.rocks/test/2".FindReceiver();

            string expected = "https://webmention.rocks/test/2/webmention";

            Assert.Equal(expected, receiver);
        }

        [Fact]
        public void HtmlLinkTag_Relative_Url()
        {
            var (_, receiver) = "https://webmention.rocks/test/3".FindReceiver();

            string expected = "https://webmention.rocks/test/3/webmention";

            Assert.Equal(expected, receiver);
        }

        [Fact]
        public void HtmlLinkTag_Absolute_Url()
        {
            var (_, receiver) = "https://webmention.rocks/test/4".FindReceiver();

            string expected = "https://webmention.rocks/test/4/webmention";

            Assert.Equal(expected, receiver);
        }

        [Fact]
        public void HtmlAnchorTag_Relative_Url()
        {
            var (_, receiver) = "https://webmention.rocks/test/5".FindReceiver();

            string expected = "https://webmention.rocks/test/5/webmention";

            Assert.Equal(expected, receiver);
        }

        [Fact]
        public void HtmlAnchorTag_Absolute_Url()
        {
            var (_, receiver) = "https://webmention.rocks/test/6".FindReceiver();

            string expected = "https://webmention.rocks/test/6/webmention";

            Assert.Equal(expected, receiver);
        }

        [Fact]
        public void HttpLink_StrangeCasing()
        {
            var (_, receiver) = "https://webmention.rocks/test/7".FindReceiver();

            string expected = "https://webmention.rocks/test/7/webmention";

            Assert.Equal(expected, receiver);
        }

        [Fact]
        public void HttpLink_QuotedRel()
        {
            var (_, receiver) = "https://webmention.rocks/test/8".FindReceiver();

            string expected = "https://webmention.rocks/test/8/webmention";

            Assert.Equal(expected, receiver);
        }

        [Fact]
        public void HtmlLink_MultipleRels()
        {
            var (_, receiver) = "https://webmention.rocks/test/9".FindReceiver();

            string expected = "https://webmention.rocks/test/9/webmention";

            Assert.Equal(expected, receiver);
        }

        [Fact]
        public void HttpLink_MultipleRels()
        {
            var (_, receiver) = "https://webmention.rocks/test/10".FindReceiver();

            string expected = "https://webmention.rocks/test/10/webmention";

            Assert.Equal(expected, receiver);
        }

        [Fact]
        public void HttpLink_And_HtmlLink_And_HtmlAnchor_MustGetHttpLink()
        {
            var (_, receiver) = "https://webmention.rocks/test/11".FindReceiver();

            string expected = "https://webmention.rocks/test/11/webmention";

            Assert.Equal(expected, receiver);
        }

        [Fact]
        public void HttpLink_With_RelEqualsNotWebMention()
        {
            var (_, receiver) = "https://webmention.rocks/test/12".FindReceiver();

            string expected = "https://webmention.rocks/test/12/webmention";

            Assert.Equal(expected, receiver);
        }

        [Fact]
        public void FalseEndpoint_As_HttpAnchor_Inside_Comment()
        {
            var (_, receiver) = "https://webmention.rocks/test/13".FindReceiver();

            string expected = "https://webmention.rocks/test/13/webmention";

            Assert.Equal(expected, receiver);
        }

        [Fact]
        public void FalseEndpoint_As_HttpAnchor_Inside_EscapedHtml()
        {
            var (_, receiver) = "https://webmention.rocks/test/14".FindReceiver();

            string expected = "https://webmention.rocks/test/14/webmention";

            Assert.Equal(expected, receiver);
        }

        [Fact]
        public void WebMention_Href_Is_EmptyString()
        {
            var (_, receiver) = "https://webmention.rocks/test/15".FindReceiver();

            string expected = "";

            Assert.Equal(expected, receiver);
        }

        [Fact]
        public void Multiple_Endpoints_HtmlAnchor_FollowedBy_HtmlLink()
        {
            var (_, receiver) = "https://webmention.rocks/test/16".FindReceiver();

            string expected = "https://webmention.rocks/test/16/webmention";

            Assert.Equal(expected, receiver);
        }

        [Fact]
        public void Multiple_Endpoints_HtmlLink_FollowedBy_HtmlAnchor()
        {
            var (_, receiver) = "https://webmention.rocks/test/17".FindReceiver();

            string expected = "https://webmention.rocks/test/17/webmention";

            Assert.Equal(expected, receiver);
        }

        [Fact]
        public void MultipleHttpLinks()
        {
            var (_, receiver) = "https://webmention.rocks/test/18".FindReceiver();

            string expected = "https://webmention.rocks/test/18/webmention";

            Assert.Equal(expected, receiver);
        }

        [Fact]
        public void SingleHttpLink_With_MultipleValues()
        {
            var (_, receiver) = "https://webmention.rocks/test/19".FindReceiver();

            string expected = "https://webmention.rocks/test/19/webmention";

            Assert.Equal(expected, receiver);
        }

        [Fact]
        public void HttpLink_With_EmptyHref()
        {
            var (_, receiver) = "https://webmention.rocks/test/20".FindReceiver();

            string expected = "https://webmention.rocks/test/20/webmention";

            Assert.Equal(expected, receiver);
        }

        [Fact]
        public void WebMentionEndpoint_With_QueryParams()
        {
            var (_, receiver) = "https://webmention.rocks/test/21".FindReceiver();

            string expected = "https://webmention.rocks/test/21/webmention?query=yes";

            Assert.Equal(expected, receiver);
        }

        [Fact]
        public void WebMentionEndpoint_Relative_To_Path()
        {
            var (_, receiver) = "https://webmention.rocks/test/22".FindReceiver();

            string expected = "https://webmention.rocks/test/22/webmention";

            Assert.Equal(expected, receiver);
        }

        [Fact]
        public void WebMentionEndpoint_Is_Redirect_And_Endpoint_Is_Relative()
        {
            var (_, receiver) = "https://webmention.rocks/test/23/page".FindReceiver();

            string expected = "https://webmention.rocks/test/23/webmention-endpoint";

            // There's a random string path being added
            Assert.StartsWith(expected, receiver);
        }
    }
}
