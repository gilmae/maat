using System;
using SV.Maat.lib;
using Xunit;

namespace Tests
{
    public class GetUsernameFromUrl
    {

        [Fact]
        public void ReturnsUsernameForValidUserProfileUrl()
        {
            string expected = "baz";
            string actual = new Uri($"http://host/user/{expected}").GetUserNameFromUrl();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IgnoresQueryString()
        {
            string expected = "baz";
            string actual = new Uri($"http://host/user/{expected}?foo=bar").GetUserNameFromUrl();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReturnsEmptyStringForInvalidEntryUrl()
        {
            string expected = string.Empty;
            string actual = new Uri("http://google/com").GetUserNameFromUrl();

            Assert.Equal(expected, actual);
        }

    }
}
