using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using AR_Docent_MVC.Config;
using Azure.Storage;

namespace AR_Docent_MVC.Service
{
    public class ARBlobStorageService : StorageService
    {
        private BlobServiceClient _blobServiceClient;
        private AzureKeyVaultService _azureKey;

        //file length로 버전관리 생성일자.

        public ARBlobStorageService(AzureKeyVaultService azureKey)
        {
            _azureKey = azureKey;
            Task.Run(() => {
                while (_azureKey.blobConnectionString == null || _azureKey.sqlConnectionString == null)
                {
                    Thread.Sleep(100);
                }
                _blobServiceClient = new BlobServiceClient(_azureKey.blobConnectionString);
            });
        }

        //user 폴더 생성
        public string StringGenerator(int length)
        {
            int bits = (length * 6);
            int byte_size = ((bits + 7) / 8);
            byte[] bytes = RandomNumberGenerator.GetBytes(byte_size);
            char[] name = Convert.ToBase64String(bytes).ToCharArray();
            for (int i = 0; i < name.Length; i++)
            {
                if (name[i] == '\\' || name[i] == '/' || name[i] == '.' || name[i] == '*')
                {
                    name[i] = '_';
                }
            }
            return new string(name);
        }

        public async Task<IEnumerable<string>> GetItems(string containerName)
        {
            List<string> items = new ();

            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            AsyncPageable<BlobItem> blobItems = containerClient.GetBlobsAsync();
            await foreach (var blobItem in blobItems)
            {
                items.Add(blobItem.Name);
            }
            return items;
        }

        public string GetItemUrl(string containerName, string name)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blob = containerClient.GetBlobClient(name);
            string uri = blob.Uri.AbsoluteUri;
            return uri;
        }

        public async Task Upload(IFormFile file, string containerName, string upload_name)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(upload_name);
            byte[] buffer;
            BinaryReader reader;
            MemoryStream target = new ();
            try
            {
                await file.CopyToAsync(target);
                
                target.Seek(0, SeekOrigin.Begin);
                
                reader = new BinaryReader(target);
                buffer = new byte[file.Length];

                reader.Read(buffer, 0, buffer.Length);
                BinaryData data = new (buffer);

                string file_extention = Path.GetExtension(blobClient.Uri.AbsoluteUri);

                BlobHttpHeaders blobHttpHeader = null;

                switch (file_extention)
                {
                    case ".png":
                        blobHttpHeader = new BlobHttpHeaders {ContentType = "image/png"};
                        break;
                    case ".jpg":
                        blobHttpHeader = new BlobHttpHeaders {ContentType = "image/jpeg"};
                        break;
                    default:
                        break;
                }
                if (blobHttpHeader == null)
                {
                    await blobClient.UploadAsync(data);
                }
                else
                {
                    await blobClient.UploadAsync(data, new BlobUploadOptions { HttpHeaders = blobHttpHeader });
                }

                reader.Close();
                target.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return;
            }
        }

        public async Task Upload(byte[] buffer, string containerName, string upload_name)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(upload_name);
            try
            {
                BinaryData data = new (buffer);
                string file_extention = Path.GetExtension(blobClient.Uri.AbsoluteUri);

                BlobHttpHeaders blobHttpHeader = null;

                switch (file_extention)
                {
                    case ".wav":
                        blobHttpHeader = new BlobHttpHeaders { ContentType = "audio/x-wav" };
                        break;
                    default:
                        break;
                }
                if (blobHttpHeader == null)
                {
                    await blobClient.UploadAsync(data);
                }
                else
                {
                    await blobClient.UploadAsync(data, new BlobUploadOptions { HttpHeaders = blobHttpHeader });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return;
            }
        }

        public async Task Delete(string containerName, string delete_name)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(delete_name);
            await blobClient.DeleteIfExistsAsync();
        }

        public string GenerateSasBlob(string containerName)
        {
            try
            {
                DateTime now, end;

                Debug.WriteLine("create sas token");
                now = DateTime.UtcNow;
                end = now.AddMinutes(5);

                BlobContainerClient _containerClient = new(
                    new Uri($"https://{ServerConfig.accountName}.blob.core.windows.net/{containerName}"),
                    new StorageSharedKeyCredential(ServerConfig.accountName, _azureKey.blobAccountKeyString)
                    );

                //get service sas token
                if (_containerClient.CanGenerateSasUri)
                {
                    BlobSasBuilder _blobSasBuilder = new ()
                    {
                        BlobContainerName = containerName,
                        Resource = "c",
                        StartsOn = now.AddMinutes(-1),
                        ExpiresOn = end,
                    };
                    _blobSasBuilder.SetPermissions(
                        BlobContainerSasPermissions.Read | BlobContainerSasPermissions.List
                        );


                    string token = _containerClient.GenerateSasUri(_blobSasBuilder).OriginalString;
                    string token_below = token.Split("?")[1];
                    
                    Debug.WriteLine("token:" + token_below);
                    return token_below;
                }
                else
                {
                    throw new Exception($"Can't Generate SasUri on {containerName}");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
                return null;
            }
        }

    }
}
