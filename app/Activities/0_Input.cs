using System;
using System.Collections.Generic;
using System.IO;
using app.DTOs;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace app.Activities
{
    public static class Input
    {
        [FunctionName("ReadInputFileFromBlob")]
        public static List<InputFormat> ReadInputFileFromBlob([ActivityTrigger] BlobFileParameters fileParameters, ILogger log)
        {
            // NOTE: Make sure the function app MSI has "Storage Blob Data Reader" (or more) rights
            //       on the storage container. Right now, the deployment script does *not* do this yet.
            // @TODO:   get params from environment variables (appSettings). But this cannot be done from within the orchestration
            //          function - because that violates deterministic principles:
            //          https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-code-constraints#using-deterministic-apis


            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var accessToken = azureServiceTokenProvider.GetAccessTokenAsync("https://storage.azure.com").Result;
            var containerEndpoint = string.Concat(fileParameters.BlobEndpoint, fileParameters.BlobContainer);
            var containerClient = new BlobContainerClient(new Uri(containerEndpoint), new ManagedIdentityCredential(accessToken));

            var blobClient = containerClient.GetBlobClient(fileParameters.FileName);
            var result = new List<InputFormat>();
            if (blobClient.Exists())
            {
                var response = blobClient.Download();
                using var streamReader = new StreamReader(response.Value.Content);
                while (!streamReader.EndOfStream)
                {
                    var line = streamReader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }
                    result.Add(new InputFormat(line));
                }
            } else
            {
                var msg = $"Could not find file {fileParameters.FileName} in container {fileParameters.BlobContainer}";
                log.LogError(msg);
                throw new Exception(msg);
            }
            log.LogInformation($"BLOB file successfully read - found {result.Count} lines");
            return result;
        }
    }
}