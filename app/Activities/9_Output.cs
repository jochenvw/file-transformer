using System;
using app.DTOs;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace app.Activities
{
    public static class Output
    {
        [FunctionName("WriteToDatabase")]
        public static bool WriteToDatabase([ActivityTrigger] FormatBInstance[] line, ILogger log)
        {
            var sqlConnectionString = Environment.GetEnvironmentVariable("SQLDBConnectionString");
            var isRunningLocally = Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                .Contains("UseDevelopmentStorage=true");

            var connection = new SqlConnection(sqlConnectionString);
            if (!isRunningLocally)
            {
                connection.AccessToken = (new AzureServiceTokenProvider())
                    .GetAccessTokenAsync("https://database.windows.net/").Result;
            }
            connection.Open();

            // Don't do this ! Super unsafe

            var sqlstatement = string.Empty;
            for (int i = 0; i < line.Length; i++)
            {
                sqlstatement += $"INSERT into [log] (Message) VALUES ( '{line[i]}' ); ";
            }
            
            var cmd = new SqlCommand(sqlstatement, connection);
            cmd.ExecuteNonQuery();
            connection.Close();
            log.LogInformation($"Written {line.Length} FormatB lines to database");
            log.LogMetric("DBWrites", line.Length);
            return true;
        }
    }
}