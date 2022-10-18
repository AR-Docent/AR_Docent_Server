using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AR_Docent.website.Services
{
    public interface StorageService
    {
        public Task Initialize(string connectionStr, string containerName);
        public Task<IEnumerable<string>> GetItemNames();
        public Task Save(Stream fileStream, string name);
        public Task<Stream> Load(string name);
    }
}
