# KeyVault Macros For Local Development

Azure KeyVault is a great way to secure your app, but configuring how to read from it takes a tad bit of learning or finding the right packages. Recently Azure exposed a simplified way of accessing secrets: using macros in the application settings, but there is just one catch: there is no local development experience! This package bridges that gap by automatically expanding any KeyVault macros found in your configuration.
