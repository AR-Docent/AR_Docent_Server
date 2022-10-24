using Microsoft.AspNetCore.Http;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AR_Docent.website.Services
{
    public interface StorageService
    {
        public Task Initialize();
        public Task<IEnumerable<string>> GetItemNames();
        public Task Upload(IFormFile file, string name);
        public Task Delete(string name);
        public Task<Stream> Download(string name);
    }
}
