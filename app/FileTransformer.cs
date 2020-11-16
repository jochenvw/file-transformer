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
            log.LogInformation($"Reading BLOB input file into lines - Done ! Found {lines.Count} lines in the BLOB file");
            
            log.LogInformation("Converting BLOB lines to Format A - Started ...");
            var ATasks = new List<Task<FormatAInstance>>();
            foreach (var inputFormat in lines)
            {
                log.LogInformation($"Converting line {inputFormat.StringValue} with id {inputFormat.Id} to FormatA");
                ATasks.Add(context.CallActivityAsync<FormatAInstance>("ConvertCSVToFormatA", inputFormat));
            }
            await Task.WhenAll(ATasks);
            log.LogInformation("Converting BLOB lines to Format A - Done !");

            log.LogInformation("Converting Format A to Format B - Started ...");

            var BTasks = new List<Task<FormatBInstance>>();
            foreach (var aTask in ATasks)
            {
                log.LogInformation($"Converting line {aTask.Result.Name} with id {aTask.Result.Id} to FormatB");
                BTasks.Add(context.CallActivityAsync<FormatBInstance>("ConvertFormatAToFormatB", aTask.Result));
            }
            await Task.WhenAll(BTasks);
            log.LogInformation("Converting Format A to Format B - Done !");

            log.LogInformation("Writing to DB - Started ...");
            var dbWrites = new List<Task<bool>>();
            foreach (var bTask in BTasks)
            {
                log.LogInformation($"Writing Format B to database for id {bTask.Id}");
                dbWrites.Add(context.CallActivityAsync<bool>("WriteToDatabase", bTask.Result));
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