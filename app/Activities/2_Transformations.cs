using System;
using app.DTOs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace app.Activities
{
    public static class Transformations
    {
        [FunctionName("ConvertCSVToFormatA")]
        public static FormatAInstance ConvertCSVToFormatA([ActivityTrigger] InputFormat input, ILogger log)
        {
            var parts = input.StringValue.Split(";");
            if (parts.Length != 4)
            {
                throw new FormatException($"Invalid format - cannot convert string {input} to FormatA - expecting semi-colon delimited input string consisting of 4 parts. E.g. 'test13;4803;1835;1558'");
            }

            try
            {
                var first = Convert.ToInt32(parts[1]);
                var second = Convert.ToInt32(parts[2]);
                var third = Convert.ToInt32(parts[3]);
                var result = new FormatAInstance(input)
                    {Name = parts[0], First = first, Second = second, Third = third};
                log.LogInformation($"Input converted to FormatA - success - for id {input.Id}");
                return result;
            }
            catch (Exception e)
            {
                throw new FormatException($"Invalid format - cannot convert string {input} to FormatA - Numbers cannot be converted to integers. Expecting semi-colon delimited input string consising of 4 parts. E.g. 'test13;4803;1835;1558'", e);
            }
        }

        [FunctionName("ConvertFormatAToFormatB")]
        public static FormatBInstance ConvertFormatAToFormatB([ActivityTrigger] FormatAInstance input, ILogger log)
        {
            var result = new FormatBInstance(input);
            log.LogInformation($"FormatA converted to FormatB - success - for id {input.Id} : {result.ToString()}");
            return result;
        }
    }
}
