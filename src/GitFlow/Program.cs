using GitFlow.Commands;
using System.CommandLine;
using System.CommandLine.Parsing;

RootCommand rootCommand = new(@"GitFlow - Git workflow management tool

Zarządza gałęziami git zgodnie ze schematem GitFlow.

Konfiguracja przechowywana jest w pliku konfiguracji gita (lokalnie lub globalnie).

INICJALIZACJA:

gitflow init --production main --development develop

Parametry są opcjonalne i zawierają domyślne wartości.
");

rootCommand.Add(new InitCommand());

return await CommandLineParser.Parse(rootCommand, args).InvokeAsync();
