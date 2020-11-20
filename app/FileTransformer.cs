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
            // Read BLOB file into lines
            var lines = await context.CallActivityAsync<List<InputFormat>>("ReadInputFileFromBlob", "notused-but-cannotbenull");
            var numberOfLines = lines.Count;
            
            var formatAs = lines.Select(line => context.CallActivityAsync<FormatAInstance>("ConvertCSVToFormatA", line));
            await Task.WhenAll(formatAs);

            var formatBs = formatAs.Select(formatA =>
                context.CallActivityAsync<FormatBInstance>("ConvertFormatAToFormatB", formatA.Result));
            await Task.WhenAll(formatBs);

            var dbWrites = formatBs.Select(formatB => context.CallActivityAsync<bool>("WriteToDatabase", formatB.Result));
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