using System;
using Dapper.Contrib.Extensions;

namespace SimpleRepo
{
    public class Model
    {
        [Key]
        public int id { get; set; }
    }
}
