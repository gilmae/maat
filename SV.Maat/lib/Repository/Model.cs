using System;
using Dapper.Contrib.Extensions;

namespace SV.Maat.lib.Repository
{
    public class Model
    {
        [Key]
        public int id { get; set; }
    }
}
