using System;
using System.IO;

namespace SV.Maat.lib.FileStore
{
    public interface IFileStore
    {
        public string Save(byte[] data);

        public byte[] Get(string filaname);
    }
}
