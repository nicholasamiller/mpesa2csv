using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using System.Reflection;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

[assembly: FunctionsStartup(typeof(MPesa2Csv.Api.Startup))]

namespace MPesa2Csv.Api
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddHttpClient()
                // uses connection string in config
                .AddApplicationInsightsTelemetry();
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {

            var builtConfig = builder.ConfigurationBuilder.Build();
            var keyVaultEndpoint = builtConfig["KeyVaultName"];

            if (builder.GetContext().EnvironmentName == "Production")
            {
                var secretClient = new SecretClient(
                            new Uri($"https://{keyVaultEndpoint}.vault.azure.net/"),
                            // uses system assigned managed identity
                            new DefaultAzureCredential());
                builder.ConfigurationBuilder
                    .AddAzureKeyVault(secretClient, new KeyVaultSecretManager())
                    .Build(); 
            }
            else
            {
                builder.ConfigurationBuilder
                    .SetBasePath(Environment.CurrentDirectory)
                 //   .AddJsonFile("local.settings.json")
                    .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                    .Build();
            }
        }

    }
}
