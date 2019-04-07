using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace TimerFromKeyVaultFromMacroFunction
{
    public static class TimerFromKeyVaultFromMacroFunction
    {
        [FunctionName(nameof(TimerFromKeyVaultFromMacroFunction))]
        public static void Run([TimerTrigger("%CronExpression%")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
