using System;
namespace SV.Maat.Projections
{
    public interface ISyndicationsProjection
    {
        public Guid? GetEntryForSyndication(string url);
    }
}
