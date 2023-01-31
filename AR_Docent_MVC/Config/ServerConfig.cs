using System.Data.Common;

namespace AR_Docent_MVC.Config
{
    public class ServerConfig
    {
        //azure keyvault url
        public static readonly string keyVaultUrl = "https://arstorageaccess.vault.azure.net/";
        //sql config
        public static readonly string connectionStringSql = "ConnectionStrings--Sql";
        //blob storage config
        public static readonly string connectionStringBlob = "ConnectionString--Blob";
        //blob storage audio
        public static readonly string imgContainerName = "arimage";
        //blob storage audio config
        public static readonly string connectionStringAudio = "ConnectionString--Speech";
        public static readonly string audioContainerName = "araudio";
        public static readonly string region = "koreacentral";
        //storage list
        public static readonly string [] containers = { imgContainerName, audioContainerName };
    }
}
