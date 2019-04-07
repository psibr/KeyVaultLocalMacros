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
