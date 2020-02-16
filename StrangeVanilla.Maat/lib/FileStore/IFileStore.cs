using System;
using System.IO;

namespace StrangeVanilla.Maat.lib
{
    public interface IFileStore
    {
        public string Save(string filename, FileStream file);

        public FileStream Get(string filaname);
    }
}
