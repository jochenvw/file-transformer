using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using app.DTOs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace app
{
    public static partial class FileTransformer
    {
        [FunctionName("FileTransformer")]
        public static async Task<string> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var fileParams = new BlobFileParameters("test.csv", "https://filetrnsfrmdataindev.blob.core.windows.net/", "inbox");

            // Read BLOB file into lines
            log.LogInformation("Reading BLOB input file into lines - Started ...");
            var lines = await context.CallActivityAsync<List<InputFormat>>("ReadInputFileFromBlob", fileParams);
            var numberOfLines = lines.Count;
            log.LogInformation($"Reading BLOB input file into lines - Done ! Found {numberOfLines} lines in the BLOB file");
            
            log.LogInformation("Converting BLOB lines to Format A - Started ...");
            var aTasks = new Task<FormatAInstance>[numberOfLines];
            for (var i = 0; i < numberOfLines; i++)
            {
                var line = lines[i];
                log.LogInformation($"Converting line {line.StringValue} with id {line.Id} to FormatA");
                aTasks[i] = context.CallActivityAsync<FormatAInstance>("ConvertCSVToFormatA", line);
            }
            await Task.WhenAll(aTasks);
            log.LogInformation("Converting BLOB lines to Format A - Done !");

            log.LogInformation("Converting Format A to Format B - Started ...");
            var bTasks = new Task<FormatBInstance>[numberOfLines];
            for (var i = 0; i < numberOfLines; i++)
            {
                var formatAInsstance = aTasks[i].Result;
                log.LogInformation($"Converting line {formatAInsstance.Name} with id {formatAInsstance.Id} to FormatB");
                bTasks[i] = context.CallActivityAsync<FormatBInstance>("ConvertFormatAToFormatB", formatAInsstance);
            }
            await Task.WhenAll(bTasks);
            log.LogInformation("Converting Format A to Format B - Done !");

            log.LogInformation("Writing to DB - Started ...");
            var dbWrites = new Task<bool>[numberOfLines];
            for (var i = 0; i < numberOfLines; i++)
            {
                var formatBInstance = bTasks[i].Result;
                log.LogInformation($"Writing Format B to database for id {formatBInstance}");
                dbWrites[i] = context.CallActivityAsync<bool>("WriteToDatabase", formatBInstance);
            }
            await Task.WhenAll(dbWrites);
            log.LogInformation("Writing to DB - Done !");            

            return "Done!";
        }


        /// <summary>
        /// Trigger function - starts the orchestration
        /// </summary>
        [FunctionName("FileTransformer_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            var instanceId = await starter.StartNewAsync("FileTransformer", null);
            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}