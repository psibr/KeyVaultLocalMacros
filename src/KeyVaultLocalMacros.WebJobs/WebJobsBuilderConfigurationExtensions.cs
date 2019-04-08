using System;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Psibr.Extensions.AspNetCore.KeyVaultLocalMacros.WebJobs
{
    public static class WebJobsBuilderConfigurationExtensions
    {
        internal static IWebJobsBuilder AddConfiguration(this IWebJobsBuilder webJobsBuilder, Action<IConfigurationBuilder> configBuilderFunc)
        {
            var configBuilder = new ConfigurationBuilder();

            var configurationRoot = webJobsBuilder.Services
                .FirstOrDefault(d => d.ServiceType == typeof(IConfiguration))?.ImplementationInstance as IConfigurationRoot;

            configBuilder.AddConfiguration(configurationRoot);

            configBuilderFunc(configBuilder);

            var newConfiguration = configBuilder.Build();

            webJobsBuilder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), newConfiguration));

            return webJobsBuilder;
        }

        /// <summary>
        /// Locates and adds local.settings.json to the configuration and scans
        /// all configuration values for Azure KeyVault macros
        /// and replaces them with actual values.
        /// <para />
        /// Uses IHostingEnvironment.IsDevelopment() implicitly.
        /// </summary>
        /// <typeparam name="TStartup">The web jobs / Azure Function startup class</typeparam>
        /// <param name="webJobsBuilder"></param>
        /// <returns></returns>
        public static IWebJobsBuilder TransformKeyVaultMacros<TStartup>(this IWebJobsBuilder webJobsBuilder)
            where TStartup : class
        {
            var hostingEnvironment = webJobsBuilder.Services
                .FirstOrDefault(d => d.ServiceType == typeof(IHostingEnvironment))?.ImplementationInstance as IHostingEnvironment;

            if (!hostingEnvironment.IsDevelopment()) return webJobsBuilder;

            webJobsBuilder.AddConfiguration(configurationBuilder => configurationBuilder
                .AddLocalSettings<TStartup>()
                .TransformKeyVaultMacrosAsync().Wait());

            return webJobsBuilder;
        }
    }
}
