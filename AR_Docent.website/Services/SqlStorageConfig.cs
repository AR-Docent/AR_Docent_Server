namespace AR_Docent.website.Services
{
    public class SqlConfig
    {
        public static readonly string connectionString = "Server = tcp:ar-docent-server.database.windows.net,1433;Initial Catalog = AR_Docent_Data; Persist Security Info=False;User ID = admin_; Password=1q2w3e4r!; MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout = 30;";
    }

    public class StorageConfig
    {
        public static readonly string imageContainerName = "arimage";
    }
}
