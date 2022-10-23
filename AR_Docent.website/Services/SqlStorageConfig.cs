using static System.Net.WebRequestMethods;

namespace AR_Docent.website.Services
{
    public class SqlConfig
    {
        public static readonly string connectionString = "Server = tcp:ar-docent-server.database.windows.net,1433;Initial Catalog = AR_Docent_Data; Persist Security Info=False;User ID = admin_; Password=1q2w3e4r!; MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout = 30;";
    }

    public class StorageConfig
    {
        public static readonly string connectionString = "DefaultEndpointsProtocol=https;AccountName=imageaudiostorageaccount;AccountKey=4LL/+c3HNyT8uOvNbVjg2X0eOb28K3f5VqNIAjhl6xiUeRZStnvVht2k8HjdFwCAbDxDWY+gVgLl+AStScyAFA==;EndpointSuffix=core.windows.net";
        public static readonly string imageContainerName = "arimage";
    }
}
