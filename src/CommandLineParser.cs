using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AzDevopsPool;

static class CommandLineParser
{
    static public RootCommand Create()
    {
        // Command Line Arguments
        var aliasArgument = new Argument<string>(
            name: "alias",
            description: "Alias name");

        var urlArgument = new Argument<string>(
            name: "url",
            description: "Azure Devops URL");

        var tokenArgument = new Argument<string>(
            name: "token",
            description: "Azure Devops PAT Token");

        var poolArgument = new Argument<string>(
            name: "pool",
            description: "Name of agent poll");

        var agentArgument = new Argument<string>(
            name: "agent",
            description: "Name of agent");

        // Command Line Options
        var isDefaultAliasOption = new Option<bool>(
            name: "--default",
            description: "Set as default alias");

        var agentOption = new Option<string>(
            name: "--agent",
            description: "List only Agent Pools that have this agent");

        var aliasOption = new Option<string>(
            name: "--alias",
            description: "Use this alias select the Azure Devops Url and credentials.");

        // Command Line commands
        var rootCommand = new RootCommand("CLI to manage pools and agents in Azure Devops.");

        var aliasCommand = new Command("alias", "List or configure Azure Devops Url and credentials.");
        rootCommand.AddCommand(aliasCommand);

        var aliasListCommand = new Command("list", "List Azure Devops stored URLs and Credentials");
        aliasCommand.AddCommand(aliasListCommand);

        var aliasSetCommand = new Command("set", "List Azure Devops stored URLs and Credentials.")
        {
            aliasArgument,
            urlArgument,
            tokenArgument,
            isDefaultAliasOption
        };
        aliasCommand.AddCommand(aliasSetCommand);

        var listPoolsCommand = new Command("list", "List all Agent Pools.")
        {
            agentOption,
            aliasOption
        };
        rootCommand.AddCommand(listPoolsCommand);

        var agentCommand = new Command("agent", "Manage agents in a agent pool");
        rootCommand.AddCommand(agentCommand);

        var agentListCommand = new Command("list", "List all Agent of a Agent Pool.")
        {
            poolArgument
        };
        agentCommand.AddCommand(agentListCommand);

        var agentDisableCommand = new Command("disable", "Disable a agent.");
        agentCommand.AddCommand(agentDisableCommand);

        var agentEnableCommand = new Command("enable", "Disable a agent.");
        agentCommand.AddCommand(agentEnableCommand);

        var agentDisableAllPoolsCommand = new Command("allpools", "Disable a agent by name in all agent pools.")
        {
            agentArgument,
            aliasOption
        };
        agentDisableCommand.AddCommand(agentDisableAllPoolsCommand);

        var agentEnableAllPoolsCommand = new Command("allpools", "Enable a agent by name in all agent pools.")
        {
            agentArgument,
            aliasOption
        };
        agentEnableCommand.AddCommand(agentEnableAllPoolsCommand);

        // Command Line Handlers
        aliasSetCommand.SetHandler((alias, url, token, isDefaultAlias) =>
        {
            Program.SetAlias(alias, url, token, isDefaultAlias);
        },
            aliasArgument, urlArgument, tokenArgument, isDefaultAliasOption
        );

        listPoolsCommand.SetHandler(async (agent, alias) =>
        {
            await Program.ListPools(agent, alias);
        },
            agentOption, aliasOption
        );

        agentListCommand.SetHandler(async (pool) =>
        {
            await Program.ListAgents(pool);
        },
            poolArgument
        );

        agentDisableAllPoolsCommand.SetHandler(async (agent, alias) =>
        {
            await Program.AgentDisableAllPools(agent, alias);
        },
            agentArgument, aliasOption
        );

        agentEnableAllPoolsCommand.SetHandler(async (agent, alias) =>
        {
            await Program.AgentEnableAllPools(agent, alias);
        },
            agentArgument, aliasOption
        );


        return rootCommand;
    }
}
