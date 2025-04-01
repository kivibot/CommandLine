using System.CommandLine;
using Kivibot.CommandLine.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Kivibot.CommandLine;

public static class ServiceCollectionExtensions
{
    public static CommandLineBuilder AddCommandLine(this IServiceCollection services)
    {
        var command = new RootCommand();
        services.AddSingleton(command);
        return new CommandLineBuilder(command, services);
    }
}