namespace app
{
    public static partial class FileTransformer
    {
        public class BlobFileParameters
        {
            public string FileName { get; private set; }
            public string BlobEndpoint { get; private set; }
            public string BlobContainer { get; private set; }

            public BlobFileParameters(string fileName, string blobEndpoint, string blobContainer)
            {
                FileName = fileName;
                BlobEndpoint = blobEndpoint;
                BlobContainer = blobContainer;
            }
        }
    }
}