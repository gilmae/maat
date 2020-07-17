using System;
using System.Threading.Tasks;

namespace SV.Maat.ExternalNetworks
{
    public interface IRequiresFederatedInstance : ISyndicationNetwork
    {
        bool RegisterApp(string instance, string name, string url, string returnUrls);
        bool SetInstance(string instance);
    }
}
