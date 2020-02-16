using System;
using System.IO;

namespace StrangeVanilla.Maat.lib
{
    public interface IFileStore
    {
        public string Save(byte[] data);

        public byte[] Get(string filaname);
    }
}
