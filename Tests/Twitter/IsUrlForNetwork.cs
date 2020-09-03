using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Xunit;
namespace Tests.Twitter
{
    public class IsUrlForNetwork
    {

        public IConfiguration Config()
        {
            string config = @"
{
    ""ConnectionStrings"": {
        ""Twitter"": ""ConsumerKey=xxx;ConsumerKeySecret=yyy""
    }
}";

            using (Stream stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(config)))
            {

                return new ConfigurationBuilder().AddJsonStream(stream).Build();
            }
        }

        [Fact]
        public void IdentifiesTwitterUrls()
        {

            Assert.True(new SV.Maat.ExternalNetworks.Twitter(Config(), null).IsUrlForNetwork(null, "https://twitter.com/gilmae/statuses/1301381260552486912"));
        }
    }
}
