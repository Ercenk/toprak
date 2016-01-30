namespace ToprakWeb.ImageManager.AzureStorage
{
    using Microsoft.WindowsAzure.Storage;

    public interface IStorageConnectionFactory
    {
        bool TryGetAccount(out CloudStorageAccount account);
    }
}