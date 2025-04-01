using System.CommandLine;
using Kivibot.CommandLine.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Kivibot.CommandLine.Builder;

public class CommandLineBuilder
{
    private readonly RootCommand _command;
    private readonly IServiceCollection _services;
    private readonly CommandLineGroupBuilder _groupBuilder;

    internal CommandLineBuilder(RootCommand command, IServiceCollection services)
    {
        _command = command;
        _services = services;
        _groupBuilder = new CommandLineGroupBuilder(command, services);
    }

    public CommandLineBuilder AddCommand<THandler, TOptions>(string name, string? description = null)
        where THandler : class, ICommandHandler<TOptions>
    {
        _groupBuilder.AddCommand<THandler, TOptions>(name, description);
        return this;
    }

    public CommandLineBuilder AddCommand<THandler, TOptions>(string name, Func<IServiceProvider, THandler> factory, string? description = null)
        where THandler : class, ICommandHandler<TOptions>
    {
        _groupBuilder.AddCommand<THandler, TOptions>(name, factory, description);
        return this;
    }

    public CommandLineBuilder AddCommand<THandler, TOptions>(string name, THandler handler, string? description = null)
        where THandler : class, ICommandHandler<TOptions>
    {
        _groupBuilder.AddCommand<THandler, TOptions>(name, handler, description);
        return this;
    }

    public CommandLineBuilder AddGroup(string name, Action<CommandLineGroupBuilder> callback)
    {
        var command = new Command(name);
        _command.AddCommand(command);

        callback(new CommandLineGroupBuilder(command, _services));

        return this;
    }

    public CommandLineBuilder AddDescription(string description)
    {
        _command.Description = description;
        return this;
    }
}