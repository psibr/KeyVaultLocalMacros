# KeyVault Macros For Local Development

Azure KeyVault is a great way to secure your app, but configuring how to read from it takes a tad bit of learning or finding the right packages. Recently Azure exposed a simplified way of accessing secrets: using macros in the application settings, but there is just one catch: there is no local development experience! **This package bridges that gap by automatically expanding any KeyVault macros found in your configuration.**

## Example
As always when extending Azure Functions, we need to add a Startup, taking care to include `[assembly: WebJobsStartup(typeof(Startup))]`. In our `Configure`, we just call `builder.TransformKeyVaultMacros<Startup>();` and all configuration values that are KeyVault macros will be resolved.

```csharp
using ExampleFunction;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Psibr.Extensions.AzureFunctionsV2.KeyVaultLocalMacros;

[assembly: WebJobsStartup(typeof(Startup))]
namespace ExampleFunction
{

    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.TransformKeyVaultMacros<Startup>();
        }
    }
}

```

This simple example shows how you could replace any binding with a KeyVault secret, in this case the timer schedule `%CronExpression%` is our binding (not really secret, but useful for the demo).

```csharp
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
```

Take an example local.settings.json file:
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "CronExpression": "@Microsoft.KeyVault(SecretUri=https://your-keyvault-name.vault.azure.net/secrets/Values--CronExpression/fbe973a92f674ce09e9c03d044499243)"
  }
}
```

