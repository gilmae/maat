using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace SV.Maat.lib.FileStore
{
    public class FSStore : IFileStore
    {
        string _rootPath;

        public FSStore(IConfiguration configuration)
        {
            _rootPath = Path.GetFullPath(configuration.GetConnectionString("Media"));
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
