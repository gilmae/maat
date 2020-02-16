using System;
using System.IO;

namespace StrangeVanilla.Maat.lib.FileStore
{
    public class FSStore : IFileStore
    {
        string _rootPath;

        public FSStore()
        {
            _rootPath = Path.GetFullPath(Environment.GetEnvironmentVariable("FSStoreRoot"));
        }

        public byte[] Get(string filename)
        {
            string filePath = Path.Join(_rootPath, filename);

            if (File.Exists(filePath))
            {
                return File.ReadAllBytes(filePath);
            }
            throw new FileNotFoundException();
        }

        public string Save(byte[] data)
        {
            string fileName = Guid.NewGuid().ToString();

            while (File.Exists(Path.Join(_rootPath, fileName)))
            {
                fileName = Guid.NewGuid().ToString();
            }

            File.WriteAllBytes(Path.Join(_rootPath, fileName), data);

            return fileName;
        }
    }
}
