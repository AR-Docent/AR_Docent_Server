using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Threading.Tasks;
using System;
using AR_Docent_MVC.Config;

namespace AR_Docent_MVC.Service
{
    public class AzureKeyVaultService
    {
        private SecretClientOptions options;
        private SecretClient client;

        public string sqlConnectionString { get; private set; } = null;
        public string blobConnectionString { get; private set; } = null;
        public string speechConnectionString { get; private set; } = null;
        public string blobAccountKeyString { get; private set; } = null;
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

            sqlConnectionString = GetSecret(ServerConfig.connectionStringSql).Result;
            blobConnectionString = GetSecret(ServerConfig.connectionStringBlob).Result;
            speechConnectionString = GetSecret(ServerConfig.connectionStringAudio).Result;
            blobAccountKeyString = GetSecret(ServerConfig.accountKey).Result;
        }

        private async Task<string> GetSecret(string name)
        {
            var secret = await client.GetSecretAsync(name);
            return secret.Value.Value;
        }
    }
}
