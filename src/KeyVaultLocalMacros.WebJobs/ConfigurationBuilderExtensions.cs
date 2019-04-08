using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace Psibr.Extensions.AspNetCore.KeyVaultLocalMacros.WebJobs
{
    public static class ConfigurationBuilderExtensions
    {
        /// <summary>
        /// Azure Functions v2 / WebJobs doesn't expose local.settings.json to IConfiguration, so this does it.
        /// </summary>
        internal static IConfigurationBuilder AddLocalSettings<TStartup>(this IConfigurationBuilder configurationBuilder)
            where TStartup : class
        {
            var assemblyLocation = new DirectoryInfo(
                Path.GetDirectoryName(typeof(TStartup).Assembly.Location)
                    ?? throw new InvalidOperationException("Unable to get directory from Startup assembly location"));

            var configDir = assemblyLocation.Parent;

            if (configDir != null)
            {
                configurationBuilder.AddJsonFile(
                    Path.Combine(
                        configDir.FullName,
                        "local.settings.json"), optional: true);
            }

            return configurationBuilder;
        }
    }
}
