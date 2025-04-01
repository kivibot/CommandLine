using System.CommandLine;
using Kivibot.CommandLine.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Kivibot.CommandLine.Builder;

public class CommandLineGroupBuilder
{
    private readonly Command _command;
    private readonly IServiceCollection _services;

    internal CommandLineGroupBuilder(Command command, IServiceCollection services)
    {
        _command = command;
        _services = services;
    }

    public CommandLineGroupBuilder AddCommand<THandler, TOptions>(string name, string? description = null)
        where THandler : class, ICommandHandler<TOptions>
    {
        _services.AddTransient<THandler>();
        AddCommandCommon<THandler, TOptions>(name, description);
        return this;
    }

    public CommandLineGroupBuilder AddCommand<THandler, TOptions>(string name, Func<IServiceProvider, THandler> factory, string? description = null)
        where THandler : class, ICommandHandler<TOptions>
    {
        _services.AddTransient(factory);
        AddCommandCommon<THandler, TOptions>(name, description);
        return this;
    }

    public CommandLineGroupBuilder AddCommand<THandler, TOptions>(string name, THandler handler, string? description = null)
        where THandler : class, ICommandHandler<TOptions>
    {
        _services.AddSingleton(handler);
        AddCommandCommon<THandler, TOptions>(name, description);
        return this;
    }

    private void AddCommandCommon<THandler, TOptions>(string name, string? description) 
        where THandler : ICommandHandler<TOptions>
    {
        var command = new Command(name, description);
        var createBinder = CommandOptionsInfo<TOptions>.CreateBinder;
        if (createBinder == null)
        {
            throw new InvalidOperationException("ICommandOptions<TOptions>.Binder not initialized");
        }

        var binder = createBinder(command);
        command.Handler = new CommandHandler<THandler, TOptions>(binder);

        _command.AddCommand(command);
    }

    public CommandLineGroupBuilder AddGroup(string name, Action<CommandLineGroupBuilder> callback)
    {
        var command = new Command(name);
        _command.AddCommand(command);

        callback(new CommandLineGroupBuilder(command, _services));

        return this;
    }

    public CommandLineGroupBuilder AddDescription(string description)
    {
        _command.Description = description;
        return this;
    }
}