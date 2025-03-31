using System.CommandLine;

using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.IO;

namespace AzDevopsPool;

internal partial class Program
{
    private static async Task<int> Main(string[] args)
    {
        var rootCommand = CommandLineParser.Create();
        return await rootCommand.InvokeAsync(args);
    }

    static private AzureDevopsConnection LoadAliasConfiguration(string? alias = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        if (string.IsNullOrEmpty(alias)) alias = configuration["DefaultAlias"]
            ?? throw new ArgumentException("DefaultAlias not found in config file");

        var aliasesConfig = configuration.GetSection("Aliases")
            ?? throw new ArgumentException("Aliases not found inf config file");

        var aliasConfig = aliasesConfig.GetSection(alias);
        var url = aliasConfig["Url"]
            ?? throw new ArgumentException($"Url not found for alias {alias}");
        var token = aliasConfig["Token"]
            ?? throw new ArgumentException($"Url not found for alias {alias}");

        return new AzureDevopsConnection(url, token);
    }
}