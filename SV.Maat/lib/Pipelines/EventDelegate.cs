﻿using System.Threading.Tasks;
using Events;

namespace SV.Maat.lib.Pipelines
{
    public delegate Task EventDelegate(Event entry);
}
