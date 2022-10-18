using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace AR_Docent.website.Services
{
    public class ImageStorage : StorageService
    {
        private BlobServiceClient _blobServiceClient;
        private BlobContainerClient _containerClient;
        
        public Task Initialize(string connectionStr, string containerName)
        {
            _blobServiceClient = new BlobServiceClient(connectionStr);
            _containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            return _containerClient.CreateIfNotExistsAsync();
        }

        public async Task<IEnumerable<string>> GetItemNames()
        {
            List<string> items = new List<string>();
            
            AsyncPageable<BlobItem> blobItems = _containerClient.GetBlobsAsync();
            await foreach (var blobItem in blobItems)
            {
                items.Add(blobItem.Name);
            }
            return items;
        }

        public Task Save(Stream fileStream, string name)
        {
            BlobClient blobClient = _containerClient.GetBlobClient(name);
            return blobClient.UploadAsync(fileStream);
        }

        public Task<Stream> Load(string name)
        {
            BlobClient blobClient = _containerClient.GetBlobClient(name);
            return blobClient.OpenReadAsync();
        }
    }
}
