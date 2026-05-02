using GitFlow.Commands;
using System.CommandLine;
using System.CommandLine.Parsing;

RootCommand rootCommand = new(@"GitFlow - Git workflow management tool

Manages git branches according to the GitFlow schema.


INITIALIZATION:

gitflow config init

Parameters are optional and contain default values.
");

rootCommand.Add(new ConfigCommand());
rootCommand.Add(new FeatureCommand());
rootCommand.Add(new BugfixCommand());
rootCommand.Add(new ReleaseCommand());
rootCommand.Add(new HotfixCommand());

return await CommandLineParser.Parse(rootCommand, args).InvokeAsync();
