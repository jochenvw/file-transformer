namespace app.DTOs
{
    public class BlobFileParameters
    {
        public string FileName { get; set; }
        public string BlobEndpoint { get; set; }
        public string BlobContainer { get; set; }

        public BlobFileParameters()
        {
        }

        public BlobFileParameters(string fileName, string blobEndpoint, string blobContainer)
        {
            FileName = fileName;
            BlobEndpoint = blobEndpoint;
            BlobContainer = blobContainer;
        }
    }
}