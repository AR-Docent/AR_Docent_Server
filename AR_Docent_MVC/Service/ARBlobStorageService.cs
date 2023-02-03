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
using System.Security.AccessControl;
using System.Security.Cryptography;
using Azure.Identity;
using AR_Docent_MVC.Config;

namespace AR_Docent_MVC.Service
{
    public class ARBlobStorageService : StorageService
    {
        private BlobServiceClient _blobServiceClient;
        private AzureKeyVaultService _azureKey;

        private int _delayDay;
        private Dictionary <string, string> _sasToken;
        
        private Timer _sasTimer;

        //file length로 버전관리 생성일자.

        public ARBlobStorageService(AzureKeyVaultService azureKey)
        {
            _azureKey = azureKey;
            _sasToken = new Dictionary<string, string>();
            Task.Run(() => {
                while (_azureKey.blobConnectionString == null || _azureKey.sqlConnectionString == null)
                {
                    Thread.Sleep(100);
                }
                _blobServiceClient = new BlobServiceClient(_azureKey.blobConnectionString);

                //start timer;
                _delayDay = 1;
                _sasTimer = new Timer(GenerateSasBlob, null, 0, (int)new TimeSpan(24 * _delayDay, 0, 0).TotalMilliseconds);
                return Task.CompletedTask;
            });
        }

        //user 폴더 생성
        public string StringGenerator(int length)
        {
            int bits = (length * 6);
            int byte_size = ((bits + 7) / 8);
            byte[] bytes = new byte[byte_size];
            bytes = RandomNumberGenerator.GetBytes(byte_size);
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
            string uri = blob.Uri.AbsoluteUri;
            return uri;
        }

        public string GetItemDownloadUrl(string containerName, string name)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blob = containerClient.GetBlobClient(name);
            Monitor.Enter(_sasToken);
            string uri = blob.Uri.AbsoluteUri + "?" + _sasToken[containerName];
            Debug.WriteLine("bloburl" + uri);
            Monitor.Exit(_sasToken);
            return uri;
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

        private void GenerateSasBlob(Object state)
        {
            try
            {
                DateTime now, end;

                Debug.WriteLine("create sas token");
                Monitor.Enter(_sasToken);
                foreach (string containerName in ServerConfig.containers)
                {
                    now = DateTime.UtcNow;
                    end = now.AddDays(1);

                    BlobSasBuilder _blobSasBuilder = new BlobSasBuilder()
                    {
                        BlobContainerName = containerName,
                        Resource = "c",
                        StartsOn = now.AddMinutes(-1),
                        ExpiresOn = end,
                    };
                    _blobSasBuilder.SetPermissions(
                        BlobContainerSasPermissions.Read | BlobContainerSasPermissions.List
                        );
                    //use Default Azure Credential
                    BlobServiceClient _blobClient = new BlobServiceClient(
                        new BlobUriBuilder(new Uri($"https://{ServerConfig.accountName}.blob.core.windows.net")).ToUri(),
                        new DefaultAzureCredential());
                    //get user delegation key
                    UserDelegationKey _userDelegationKey = _blobClient.GetUserDelegationKey(now.AddMinutes(-1), end);
                    string token = _blobSasBuilder.ToSasQueryParameters(_userDelegationKey, _blobServiceClient.AccountName).ToString();

                    Debug.WriteLine("token:" + token);

                    if (_sasToken.ContainsKey(containerName) == false)
                    {
                        _sasToken.Add(containerName, token);
                    }
                    else
                    {
                        _sasToken[containerName] = token;
                    }
                }
                Monitor.Exit(_sasToken);
                Debug.WriteLine("complete sas token");
            }
            catch (Exception e)
            {
                Debug.Write(e.Message);
            }
        }

    }
}
