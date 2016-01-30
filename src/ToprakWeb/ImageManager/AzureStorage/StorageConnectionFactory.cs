namespace ToprakWeb.ImageManager.AzureStorage
{
    using Microsoft.WindowsAzure.Storage;

    public class StorageConnectionFactory : IStorageConnectionFactory
    {
        private readonly string connectionString;

        private readonly bool useDevStore;

        public StorageConnectionFactory(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public StorageConnectionFactory()
        {
            this.useDevStore = true;
        }

        public bool TryGetAccount(out CloudStorageAccount account)
        {
            if (!this.useDevStore)
            {
                return CloudStorageAccount.TryParse(this.connectionString, out account);
            }

            account = CloudStorageAccount.DevelopmentStorageAccount;
            return true;
        }
    }
}