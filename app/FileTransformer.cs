using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Services.AppAuthentication;
using Azure.Storage.Blobs;
using Azure.Identity;
using System;
using System.IO;

namespace app
{
    public static partial class FileTransformer
    {
        [FunctionName("FileTransformer")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            // NOTE: Make sure the function app MSI has "Storage Blob Data Reader" (or more) rights
            //       on the storage container. Right now, the deployment script does *not* do this yet.
            
            var outputs = new List<string>();
            // @TODO:   get params from environment variables (appSettings). But this cannot be done from within the orchestration
            //          function - because that violates deterministic principles: https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-code-constraints#using-deterministic-apis
            var fileParams = new BlobFileParameters("test.csv", "https://filetrnsfrmdataindev.blob.core.windows.net/", "inbox");

            var lines = await context.CallActivityAsync<List<string>>("ReadInputFileFromBlob", fileParams);

            lines.ForEach(line => outputs.Add(line));
            return outputs;
        }


        [FunctionName("ReadInputFileFromBlob")]
        public static List<string> ReadInputFileFromBlob([ActivityTrigger] BlobFileParameters fileParameters, ILogger log)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var accessToken = azureServiceTokenProvider.GetAccessTokenAsync("https://storage.azure.com").Result;
            var containerEndpoint = string.Concat(fileParameters.BlobEndpoint, fileParameters.BlobContainer);
            var containerClient = new BlobContainerClient(new Uri(containerEndpoint), new ManagedIdentityCredential(accessToken));

            var blobClient = containerClient.GetBlobClient(fileParameters.FileName);
            var result = new List<string>();
            if (blobClient.Exists())
            {
                var response = blobClient.Download();
                using (var streamReader = new StreamReader(response.Value.Content))
                {
                    while (!streamReader.EndOfStream)
                    {
                        var line = streamReader.ReadLine();
                        result.Add(line);
                    }
                }
            } else
            {
                var msg = $"Could not find file {fileParameters.FileName} in container {fileParameters.BlobContainer}";
                log.LogError(msg);
                throw new Exception(msg);
            }
            log.LogInformation($"BLOB file succesfully read - found {result.Count} lines");
            return result;
        }

        [FunctionName("FileTransformer_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("FileTransformer", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}