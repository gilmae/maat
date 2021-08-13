using System;
using System.Collections.Generic;
using SV.Maat.Micropub.Models;
using SV.Maat.Reactors;
using Tests.Mocks;
using Xunit;

namespace Tests
{
    public class GetSyndicationsOfReplyToParent
    {
        [Fact]
        public void If_ReplyTo_Not_An_Entry_Returns_ReplyTo()
        {
            string replyTo = "https://foo.bar";
            IEnumerable<string> expected = new[] { replyTo };

            Entry entry = new Entry { ReplyTo =  replyTo  };

            var reactor = new SyndicateEntry(null, null, new MockEntryProjection(new Entry[] { entry }), null, null, null, null);

            var actual = reactor.GetSyndicationsOfReplyToParent(entry);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void If_ReplyTo_An_Entry_With_No_Syndications_Returns_ReplyTo()
        {
            Guid id = Guid.NewGuid();
            string replyTo = $"https://foo.bar/entries/{id}";
            IEnumerable<string> expected = new[] { replyTo };
            Entry entry = new Entry { ReplyTo = replyTo, Id = id.ToString(), Syndications = new string[] { } };

            var reactor = new SyndicateEntry(null, null, new MockEntryProjection(new Entry[] { entry }), null, null, null, null);

            var actual = reactor.GetSyndicationsOfReplyToParent(entry);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void If_ReplyTo_An_Entry_With_Null_Syndications_Returns_ReplyTo()
        {
            Guid id = Guid.NewGuid();
            string replyTo = $"https://foo.bar/entries/{id}";
            IEnumerable<string> expected = new[] { replyTo };
            Entry entry = new Entry { ReplyTo = replyTo, Uid = id.ToString(), Syndications = null };

            var reactor = new SyndicateEntry(null, null, new MockEntryProjection(new Entry[] { entry }), null, null, null, null);

            var actual = reactor.GetSyndicationsOfReplyToParent(entry);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void If_ReplyTo_An_Entry_With_One_Syndication_Returns_ReplyTo_And_Syndications()
        {
            Guid id = Guid.NewGuid();
            string replyTo = $"https://foo.bar/entries/{id}";
            Entry entry = new Entry { ReplyTo = replyTo, Id = id.ToString(), Syndications = new string[] { "http://google.com"  }, Url = new[] { $"http://test.com/entries/{id}" } };

            IList<string> expected = new List<string> { replyTo, "http://google.com" };

            var reactor = new SyndicateEntry(null, null, new MockEntryProjection(new Entry[] { entry }), null, null, null, null);

            var actual = reactor.GetSyndicationsOfReplyToParent(entry);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void If_ReplyTo_An_Entry_With_Many_Syndications_Returns_ReplyTo_And_Syndications()
        {
            Guid id = Guid.NewGuid();
            string replyTo = $"https://foo.bar/entries/{id}";
            Entry entry = new Entry { ReplyTo = replyTo, Id = id.ToString(), Syndications = new string[] { "http://google.com" , "http://ebay.com"  }, Url = new[] { $"http://test.com/entries/{id}" } };

            IList<string> expected = new List<string> { replyTo, "http://google.com", "http://ebay.com" };

            var reactor = new SyndicateEntry(null, null, new MockEntryProjection(new Entry[] { entry }), null, null, null, null);

            var actual = reactor.GetSyndicationsOfReplyToParent(entry);

            Assert.Equal(expected, actual);
        }
    }
}