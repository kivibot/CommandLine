# CommandLine

A very opinionated command line parser library for .NET

# Usage

```csharp
using Kivibot.CommandLine;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = new CliApplicationBuilder();

        builder.Services.AddCommandLine()
            .AddCommand<HelloCommand, HelloCommandOptions>("hello");

        await builder.BuildAndExecuteAsync(args);
    }
}

[Options]
public class HelloCommandOptions
{
    public required string Name { get; set; }
}

[Handler(NoHost = true)]
public class HelloCommand : ICommandHandler<HelloCommandOptions>
{
    public async Task ExecuteAsync(HelloCommandOptions options, CancellationToken cancellationToken)
    {
        await Console.Out.WriteLineAsync($"Hello, {options.Name}!");
    }
}
```

```
$ dotnet run -- hello --help
Description:

Usage:
  CliApp hello <NAME> [options]

Arguments:
  <NAME>

Options:
  -?, -h, --help  Show help and usage information
```

```
$ dotnet run -- hello World
Hello, World!
```

## Arguments

```c#
[Options]
public class HelloCommandOptions
{
    // Properties are arguments by default
    public required string Argument { get; set; }

    [Argument(Name = "CUSTOM-NAME", Description = "Argument with a custom name")]
    public required string ArgumentWithCustomName { get; set; }

    public string ArgumentWithDefaultValue { get; set; } = "foo";

    public string? ArgumentWithoutDefaultValue { get; set; }
}
```

```
$ app hello --help
Usage:
  CliApp hello <ARGUMENT> <CUSTOM-NAME> [<ARGUMENT-WITH-DEFAULT-VALUE> [<ARGUMENT-WITHOUT-DEFAULT-VALUE>]] [options]

Arguments:
  <ARGUMENT>
  <CUSTOM-NAME>                     Argument with a custom name
  <ARGUMENT-WITH-DEFAULT-VALUE>     [default: foo]
  <ARGUMENT-WITHOUT-DEFAULT-VALUE>  []
```

## Options

```csharp
[Options]
public class HelloCommandOptions
{
    [Option(Long = "my-option", Short = 'o', Description = "foo")]
    public required string RequiredOption { get; set; }
    [Option]
    public string OptionWithDefaultValue { get; set; } = "foo";
    [Option]
    public bool Flag { get; set; }
}
```

```
$ app hello --help
Usage:
  CliApp hello [options]

Options:
  -o, --my-option <my-option>                              foo
  --option-with-default-value <option-with-default-value>  [default: foo]
  --flag                                                   [default: False]
```

## Subcommands

```csharp
builder.Services.AddCommandLine()
    .AddCommand<HelloCommand, HelloCommandOptions>("hello-a")
    .AddCommand<HelloCommand, HelloCommandOptions>("hello-b")
    .AddGroup("hello-group", g => g
        .AddCommand<HelloCommand, HelloCommandOptions>("hello-c")
        .AddCommand<HelloCommand, HelloCommandOptions>("hello-d")
    );
```

```
$ app --help
Usage:
  CliApp [command] [options]

Options:
  --version       Show version information
  -?, -h, --help  Show help and usage information

Commands:
  hello-a
  hello-b
  hello-group
```

```
$ app hello-group --help
Usage:
  CliApp hello-group [command] [options]

Options:
  -?, -h, --help  Show help and usage information

Commands:
  hello-c
  hello-d
```

## Microsoft.Extensions.Hosting

Disable background services

```csharp
[Handler(NoHost = true)]
public class HelloCommand : ICommandHandler<HelloCommandOptions>
{
}
```
