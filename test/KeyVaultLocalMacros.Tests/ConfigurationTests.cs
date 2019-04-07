using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Psibr.Extensions.AzureFunctionsV2.KeyVaultLocalMacros;
using Xunit;

namespace Psibr.Extensions.AzureFunctions.KeyVaultLocalMacros.Tests
{
    public class ConfigurationTests
    {
        [Fact]
        public void Matcher_can_find_root_level_matches()
        {
            // ARRANGE
            var builder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Root"] = "@Microsoft.KeyVault(SecretUri=https://foo.bar/baz)"
                });

            // ACT
            var found = ConfigurationBuilderExtensions.GatherTransformablePairs(builder);

            // ASSERT
            Assert.NotNull(found);
            Assert.NotEmpty(found);
            Assert.Single(found, match => match.Key == "Root" && match.Value == "https://foo.bar/baz");
        }

        [Fact]
        public void Matcher_can_find_nested_level_matches()
        {
            // ARRANGE
            var builder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Root:NotRoot:Setting"] = "@Microsoft.KeyVault(SecretUri=https://foo.bar/baz)"
                });

            // ACT
            var found = ConfigurationBuilderExtensions.GatherTransformablePairs(builder);

            // ASSERT
            Assert.NotNull(found);
            Assert.NotEmpty(found);
            Assert.Single(found, match => match.Key == "Root:NotRoot:Setting" && match.Value == "https://foo.bar/baz");
        }

        [Fact]
        public void Matcher_does_not_match_other_macros()
        {
            // ARRANGE
            var builder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Root:NotRoot:Setting"] = "@Microsoft.Other(SecretUri=https://foo.bar/baz)"
                });

            // ACT
            var found = ConfigurationBuilderExtensions.GatherTransformablePairs(builder);

            // ASSERT
            Assert.NotNull(found);
            Assert.Empty(found);
        }
    }
}
