using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzDevopsPool;

internal partial class Program
{
    static public void SetAlias(string alias, string url, string token, bool isDefaultAlias)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        AliasConfig config;
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath) ??
                throw new Exception("Invalid config file");

            config = JsonSerializer.Deserialize<AliasConfig>(json) ??
                throw new Exception("Invalid config file");
        }
        else
        {
            config = new();
            config.DefaultAlias = alias;
        }

        if (isDefaultAlias) config.DefaultAlias = alias;

        config.Aliases[alias] = new(url, token);

        string jsonString = JsonSerializer.Serialize<AliasConfig>(config);
        File.WriteAllText(filePath, jsonString);
    }

    static public async Task ListPools(string? agentNameFilter, string? alias)
    {
        var config = LoadAliasConfiguration(alias);

        // Connect to Azure DevOps Services
        VssConnection connection = new VssConnection(config.AzureDevopsUri, new VssBasicCredential(string.Empty, config.AzureDevopsPATToken));

        var client = connection.GetClient<TaskAgentHttpClient>();
        var pools = await client.GetAgentPoolsAsync();

        foreach (var pool in pools)
        {
            var agents = await client.GetAgentsAsync(pool.Id, includeAssignedRequest: true);
            if (!string.IsNullOrEmpty(agentNameFilter))
            {
                agents = agents.FindAll(a => a.Name.ToLower() == agentNameFilter.ToLower());
                
            }
            if (agents != null && agents.Count > 0)
            {
                Console.WriteLine($"Pool: {pool.Name}.");
                foreach (var agent in agents)
                {
                    
                    Console.WriteLine($"    Agent: {agent.Name}. ");                
                    if (agent.Status is TaskAgentStatus.Offline)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("        Status is Offline");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("        Status is Online");
                        Console.ResetColor();
                    }                     
                    if (agent.AssignedRequest != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("        Running a Job");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("        Not Running a Job");
                    }
                    
                    Console.ResetColor();
                }
                
            }
            else if (string.IsNullOrEmpty(agentNameFilter))
            {
                Console.WriteLine($"Pool: {pool.Name}.");
            }
        }
    }

    static public async Task ListAgents(string poolName)
    {
        var config = LoadAliasConfiguration();

        VssConnection connection = new VssConnection(config.AzureDevopsUri, new VssBasicCredential(string.Empty, config.AzureDevopsPATToken));

        var client = connection.GetClient<TaskAgentHttpClient>();
        var pools = await client.GetAgentPoolsAsync(poolName);

        if (pools == null || pools.Count != 1)
        {
            Console.WriteLine($"Pool {poolName} not found");
            return;
        }

        var poolId = pools[0].Id;

        var agents = await client.GetAgentsAsync(poolId, includeAssignedRequest: true);

        foreach (var agent in agents)
        {
            Console.WriteLine(agent.Name);
            Console.WriteLine(agent.AssignedRequest);
            if (agent.AssignedRequest != null)
            {
                Console.WriteLine(agent.AssignedRequest.JobName);
            }
            else
            {
                Console.WriteLine("none");
            }
            Console.WriteLine(agent.Status);
            // agent.AssignedRequest.JobName
        }
    }

    static public async Task AgentDisableAllPools(string agentNameFilter, string? alias)
    {
        if (string.IsNullOrEmpty(agentNameFilter)) throw new ArgumentException("Agent name must be set.");

        var config = LoadAliasConfiguration(alias);

        // Connect to Azure DevOps Services
        VssConnection connection = new VssConnection(config.AzureDevopsUri, new VssBasicCredential(string.Empty, config.AzureDevopsPATToken));

        var client = connection.GetClient<TaskAgentHttpClient>();
        var pools = await client.GetAgentPoolsAsync();

        foreach (var pool in pools)
        {
            var agents = await client.GetAgentsAsync(pool.Id, includeAssignedRequest: true);
            if (!string.IsNullOrEmpty(agentNameFilter))
            {
                agents = agents.FindAll(a => a.Name.ToLower() == agentNameFilter.ToLower());

            }
            if (agents != null && agents.Count > 0)
            {
                Console.WriteLine($"Pool: {pool.Name}.");
                foreach (var agent in agents)
                {
                    if (agent.Status is TaskAgentStatus.Online)
                    {
                        agent.Status = TaskAgentStatus.Offline;
                        await client.UpdateAgentAsync(pool.Id, agent.Id, agent);
                        Console.WriteLine($"Agent {agent.Name} was disabled at pool {pool.Name}.");
                    }
                }
            }
        }
    }

    static public async Task AgentEnableAllPools(string agentNameFilter, string? alias)
    {
        if (string.IsNullOrEmpty(agentNameFilter)) throw new ArgumentException("Agent name must be set.");

        var config = LoadAliasConfiguration(alias);

        // Connect to Azure DevOps Services
        VssConnection connection = new VssConnection(config.AzureDevopsUri, new VssBasicCredential(string.Empty, config.AzureDevopsPATToken));

        var client = connection.GetClient<TaskAgentHttpClient>();
        var pools = await client.GetAgentPoolsAsync();

        foreach (var pool in pools)
        {
            var agents = await client.GetAgentsAsync(pool.Id, includeAssignedRequest: true);
            if (!string.IsNullOrEmpty(agentNameFilter))
            {
                agents = agents.FindAll(a => a.Name.ToLower() == agentNameFilter.ToLower());

            }
            if (agents != null && agents.Count > 0)
            {
                Console.WriteLine($"Pool: {pool.Name}.");
                foreach (var agent in agents)
                {
                    if (agent.Status is TaskAgentStatus.Offline)
                    {
                        agent.Status = TaskAgentStatus.Online;
                        await client.UpdateAgentAsync(pool.Id, agent.Id, agent);
                        Console.WriteLine($"Agent {agent.Name} was enabled at pool {pool.Name}.");
                    }
                }
            }
        }
    }
}
