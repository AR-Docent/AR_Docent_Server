using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Threading;
using System.Security.Cryptography;
using Azure.Storage.Sas;
using System.Security.Policy;
using Azure.Storage.Blobs.Specialized;
using Microsoft.CognitiveServices.Speech;

namespace AR_Docent_MVC.Service
{
    public class ARBlobStorageService : StorageService
    {
        private BlobServiceClient _blobServiceClient;
        private AzureKeyVaultService _azureKey;
        
        private UserDelegationKey _userDelegationKey;
        private Dictionary<string, Uri> _sasUri;
        
        //file length로 버전관리 생성일자.
        
        public ARBlobStorageService(AzureKeyVaultService azureKey)
        {
            _azureKey = azureKey;
            _userDelegationKey = null;
            _sasUri = new Dictionary<string, Uri>();
            
            Task.Run(async () => {
                while (_azureKey.blobConnectionString == null || _azureKey.sqlConnectionString == null)
                {
                    Thread.Sleep(100);
                }
                _blobServiceClient = new BlobServiceClient(_azureKey.blobConnectionString);
                }
            );
        }
        //user 폴더 생성
        public string StringGenerator(int length)
        {
            using (var crypto = new RNGCryptoServiceProvider())
            {
                int bits = (length * 6);
                int byte_size = ((bits + 7) / 8);
                byte[] bytes = new byte[byte_size];
                crypto.GetBytes(bytes);
                return Convert.ToBase64String(bytes);
            }
        }

        public async Task<IEnumerable<string>> GetItems(string containerName)
        {
            List<string> items = new List<string>();

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
            return blob.Uri.ToString();
            /*
            await GetUserDelegationSasBlob(blob);
            BlobClient outBlob = new BlobClient(_sasUri[blob.Name], null);
            return outBlob.Uri.ToString();
            */
        }

        public async Task Upload(IFormFile file, string containerName, string upload_name)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(upload_name);
            byte[] buffer;
            BinaryReader reader;
            MemoryStream target = new MemoryStream();
            try
            {
                await file.CopyToAsync(target);
                
                target.Seek(0, SeekOrigin.Begin);
                
                reader = new BinaryReader(target);
                buffer = new byte[file.Length];

                reader.Read(buffer, 0, buffer.Length);
                BinaryData data = new BinaryData(buffer);

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
                BinaryData data = new BinaryData(buffer);
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
        
        /*
        private async Task GetUserDelegationSasBlob(BlobClient blob)
        {
            try
            {
                DateTime now = DateTime.UtcNow;
                TimeSpan ts = TimeSpan.Zero;
                if (_userDelegationKey != null && _sasUri.ContainsKey(blob.Name))
                {
                    ts = _userDelegationKey.SignedExpiresOn - now;
                    if (ts.Days + ts.Hours + ts.Minutes + ts.Seconds < 0)
                    {
                        return;
                    }
                }
                Debug.WriteLine("create sas token");
                BlobServiceClient serviceClient = blob.GetParentBlobContainerClient().GetParentBlobServiceClient();
                Debug.WriteLine("get serviceClient");
                _userDelegationKey = await serviceClient.GetUserDelegationKeyAsync(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddDays(7));
                Debug.WriteLine("create sas builder");
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = blob.BlobContainerName,
                    Resource = "b",
                    BlobName = blob.Name,
                    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                    ExpiresOn = DateTimeOffset.UtcNow.AddDays(7)
                };
                Debug.WriteLine("create uriBuilder");
                sasBuilder.SetPermissions(
                    BlobSasPermissions.Read
                    );
                BlobUriBuilder blobUriBuilder = new BlobUriBuilder(blob.Uri)
                {
                    Sas = sasBuilder.ToSasQueryParameters(_userDelegationKey, serviceClient.AccountName)
                };
                if (_sasUri.ContainsKey(blob.Name))
                    _sasUri[blob.Name] = blobUriBuilder.ToUri();
                else
                    _sasUri.Add(blob.Name, blobUriBuilder.ToUri());
                Debug.WriteLine("out:" + _sasUri[blob.Name].ToString());
            }
            catch (Exception e)
            {
                Debug.Write(e.Message);
            }
        }
        */
    }
}
