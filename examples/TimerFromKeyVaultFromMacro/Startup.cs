using ExampleFunction;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Hosting;
using Psibr.Extensions.AspNetCore.KeyVaultLocalMacros.WebJobs;

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
