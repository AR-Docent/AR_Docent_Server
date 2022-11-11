using Microsoft.AspNetCore.Http;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AR_Docent_MVC.Service
{
    public interface StorageService
    {
        public Task<IEnumerable<string>> GetItems(string containerName);
        public string GetItemUrl(string containerName, string name);
        public Task Upload(IFormFile file, string containerName, string upload_name);
        public Task Delete(string containerName, string delete_name);
    }
}
