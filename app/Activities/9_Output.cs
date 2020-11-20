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
            // https://docs.microsoft.com/en-us/azure/app-service/app-service-web-tutorial-connect-msi
            var connectionString = "Server=tcp:filetrnsfrm-dbsrv-dev.database.windows.net,1433;Database=filetrnsfrm-db-dev;";
            var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            connection.AccessToken = (new AzureServiceTokenProvider()).GetAccessTokenAsync("https://database.windows.net/").Result;
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