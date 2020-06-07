using System;
using System.Collections.Generic;

namespace SV.Maat.Projections
{
    public interface IRepliesProjection
    {
        IEnumerable<Guid> GetReplyIds(string url);
    }
}
