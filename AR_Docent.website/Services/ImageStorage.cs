using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure;
using Azure.Identity;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using AR_Docent.website.Models;
using System.ComponentModel;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace AR_Docent.website.Services
{
    public class ImageStorage : StorageService
    {
        private BlobServiceClient _blobServiceClient;
        private BlobContainerClient _containerClient;
        
        public Task Initialize()
        {
            /*
            _blobServiceClient = new BlobServiceClient(
                new Uri("https://imageaudiostorageaccount.blob.core.windows.net"),
                new DefaultAzureCredential());
            */
            _blobServiceClient = new BlobServiceClient(StorageConfig.connectionString);

            _containerClient = _blobServiceClient.GetBlobContainerClient(StorageConfig.imageContainerName);
            //_containerClient = _blobServiceClient.CreateBlobContainer(StorageConfig.imageContainerName);
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

        public async Task Upload(IFormFile file, string name)
        {
            BlobClient blobClient = _containerClient.GetBlobClient(name);
            byte[] buffer;
            BinaryReader reader;
            MemoryStream target = new MemoryStream();

            try
            {
                await file.CopyToAsync(target);
                reader = new BinaryReader(target);
                buffer = new byte[file.Length];

                reader.Read(buffer, 0, buffer.Length);
                BinaryData data = new BinaryData(buffer);
                await blobClient.UploadAsync(data);
                target.Dispose();
            }
            catch (ObjectDisposedException ex)
            {
                Debug.WriteLine(ex.ObjectName);
                return;
            }
        }

        public Task<Stream> Download(string name)
        {
            BlobClient blobClient = _containerClient.GetBlobClient(name);
            return blobClient.OpenReadAsync();
        }
    }
}
