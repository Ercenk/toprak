namespace Tests.ImageManagerTests
{
    using ImageManager;
    using ImageManager.AzureStorage;
    using Machine.Specifications;
    using Moq;
    using ToprakWeb.ImageManager;
    using ToprakWeb.ImageManager.AzureStorage;

    [Subject(typeof(ImageManagerService))]
    public class ImageManagerTests
    {
        Establish context = () =>
        {
            QueueRepositoryMock = new Mock<IQueueRepository>();
            //Subject = new ImageManagerService(QueueRepositoryMock.Object, ImageRepositoryMock.Object);
        };

        static ImageManagerService Subject;
        private static Mock<IQueueRepository> QueueRepositoryMock;

        private static Mock<IImageRepository> ImageRepositoryMock;
    }
}
