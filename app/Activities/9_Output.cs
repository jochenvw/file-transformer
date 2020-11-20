using System;
using System.Collections.Generic;
using app.DTOs;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace app.Activities
{
    public static class Output
    {
        [FunctionName("WriteToDatabase")]
        public static bool WriteToDatabase([ActivityTrigger] FormatBInstance line, ILogger log)
        {
            var sqlConnectionString = Environment.GetEnvironmentVariable("SQLDBConnectionString");
            var isRunningLocally = Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                .Contains("UseDevelopmentStorage=true");


            var connection = new Microsoft.Data.SqlClient.SqlConnection(sqlConnectionString);
            if (!isRunningLocally)
            {
                connection.AccessToken = (new AzureServiceTokenProvider())
                    .GetAccessTokenAsync("https://database.windows.net/").Result;
            }

            connection.Open();

            // YES - SUPER UNSAFE
            var sqlstatement = $"INSERT into [log] (Message) VALUES ( '{line.ToString()}' ) ";
            var cmd = new Microsoft.Data.SqlClient.SqlCommand(sqlstatement, connection);
            cmd.ExecuteNonQuery();
            log.LogInformation($"Executed query: '{sqlstatement}' for id {line.Id}");
            connection.Close();
            return true;
        }
    }
}