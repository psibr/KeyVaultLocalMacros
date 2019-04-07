using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;

namespace Psibr.Extensions.AzureFunctionsV2.KeyVaultLocalMacros
{
    public static class ConfigurationBuilderExtensions
    {
        internal static readonly Regex ExtractionRegex = new Regex("(?<=\\@Microsoft\\.KeyVault\\(SecretUri=)[^)]*");

        /// <summary>
        /// Azure Functions v2 doesn't expose local.settings.json to IConfiguration, so this does it.
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

        /// <summary>
        /// Transforms Azure KeyVault macros into their respective values.
        /// </summary>
        /// <param name="configurationBuilder"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<IConfigurationBuilder> TransformKeyVaultMacrosAsync(this IConfigurationBuilder configurationBuilder, CancellationToken cancellationToken = default)
        {
            var needsTransformation = GatherTransformablePairs(configurationBuilder);

            // Lookup & Transform
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var callback = new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback);
            var keyVaultClient = new KeyVaultClient(callback, new HttpClient());

            var transformedMacros = new Dictionary<string, string>();

            var lookupTasks = new Task<SecretBundle>[needsTransformation.Count];

            for (var index = 0; index < needsTransformation.Count; index++)
            {

                lookupTasks[index] = keyVaultClient.GetSecretAsync(needsTransformation[index].Value, cancellationToken);
            }

            await Task.WhenAll(lookupTasks).ConfigureAwait(false);

            for (var index = 0; index < needsTransformation.Count; index++)
            {
                transformedMacros.Add(needsTransformation[index].Key, lookupTasks[index].Result.Value);
            }

            configurationBuilder.AddInMemoryCollection(transformedMacros);

            return configurationBuilder;
        }

        internal static List<KeyValuePair<string, string>> GatherTransformablePairs(IConfigurationBuilder configurationBuilder)
        {
            var configuration = configurationBuilder.Build();

            var needsTransformation = new List<KeyValuePair<string, string>>();

            // Use stack to walk the configuration without recursion.
            var sections = new Stack<IConfigurationSection>(configuration.GetChildren());

            while (sections.Count > 0)
            {
                var section = sections.Pop();

                foreach (var subSection in section.GetChildren())
                {
                    sections.Push(subSection);
                }

                if (section.Value is null) continue;

                var matches = ExtractionRegex.Matches(section.Value);

                if (matches.Count == 1)
                {
                    needsTransformation.Add(new KeyValuePair<string, string>(section.Path, matches[0].Value));
                }
            }

            return needsTransformation;
        }
    }
}
