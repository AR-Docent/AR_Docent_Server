using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Threading.Tasks;
using System;
using AR_Docent_MVC.Config;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using System.Diagnostics;
using Azure.Storage.Blobs.Specialized;

namespace AR_Docent_MVC.Service
{
    public class AzureKeyVaultService
    {
        private SecretClientOptions options;
        private SecretClient client;

        public string sqlConnectionString { get; private set; } = null;
        public string blobConnectionString { get; private set; } = null;
        public string speechConnectionString { get; private set; } = null;
        public Uri sasUri { get; set; } = null;
        public AzureKeyVaultService()
        {
            options = new SecretClientOptions()
            {
                Retry =
                {
                    Delay= TimeSpan.FromSeconds(2),
                    MaxDelay= TimeSpan.FromSeconds(16),
                    MaxRetries= 5,
                    Mode= RetryMode.Exponential
                }
            };

            client = new SecretClient(new Uri(ServerConfig.keyVaultUrl),
                new DefaultAzureCredential(), options);

            sqlConnectionString = GetSqlConnectionString().Result;
            blobConnectionString = GetBlobStorageConnectionString().Result;
            speechConnectionString = GetSpeechConnectionString().Result;
        }

        private async Task<string> GetSqlConnectionString()
        {
            var secret = await client.GetSecretAsync(ServerConfig.connectionStringSql);
            sqlConnectionString = secret.Value.Value;
            return secret.Value.Value;
        }

        private async Task<string> GetBlobStorageConnectionString()
        {
            var secret = await client.GetSecretAsync(ServerConfig.connectionStringBlob);
            blobConnectionString = secret.Value.Value;
            return secret.Value.Value;
        }

        private async Task<string> GetSpeechConnectionString()
        {
            var secret = await client.GetSecretAsync(ServerConfig.connectionStringAudio);
            speechConnectionString = secret.Value.Value;
            return secret.Value.Value;
        }
    }
}
