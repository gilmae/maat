//using System;
//using SV.Maat.lib;
//using Xunit;

//namespace Tests
//{
//    public class GetEntryIdFromUrl
//    {

//        [Fact]
//        public void ReturnsIdForValidEntryUrl()
//        {
//            Guid expected = Guid.NewGuid();
//            Guid actual = new Uri($"http://host/entries/{expected}?foo=bar").GetEntryIdFromUrl();
//            Assert.Equal(expected, actual);
//        }

//        [Fact]
//        public void IgnoresQueryString()
//        {
//            Guid expected = Guid.NewGuid();
//            Guid actual = new Uri($"http://host/entries/{expected}?foo=bar").GetEntryIdFromUrl();
//            Assert.Equal(expected, actual);
//        }

//        [Fact]
//        public void ReturnsEmptyGuidForInvalidEntryUrl()
//        {
//            Guid expected = Guid.Empty;
//            Guid actual = new Uri("http://google/com").GetEntryIdFromUrl();

//            Assert.Equal(expected, actual);
//        }

//    }
//}
