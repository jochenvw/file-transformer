using System.Collections.Generic;
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
            // Read BLOB file into lines
            var lines = await context.CallActivityAsync<List<InputFormat>>("ReadInputFileFromBlob", "unused-butshouldnotbenull");
            var numberOfLines = lines.Count;

            var aTasks = new Task<FormatAInstance>[numberOfLines];
            for (var i = 0; i < numberOfLines; i++)
            {
                var line = lines[i];
                aTasks[i] = context.CallActivityAsync<FormatAInstance>("ConvertCSVToFormatA", line);
            }
            await Task.WhenAll(aTasks);

            var bTasks = new Task<FormatBInstance>[numberOfLines];
            for (var i = 0; i < numberOfLines; i++)
            {
                var formatAInsstance = aTasks[i].Result;
                bTasks[i] = context.CallActivityAsync<FormatBInstance>("ConvertFormatAToFormatB", formatAInsstance);
            }
            await Task.WhenAll(bTasks);

            var dbWrites = new Task<bool>[numberOfLines];
            for (var i = 0; i < numberOfLines; i++)
            {
                var formatBInstance = bTasks[i].Result;
                dbWrites[i] = context.CallActivityAsync<bool>("WriteToDatabase", formatBInstance);
            }
            await Task.WhenAll(dbWrites);

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