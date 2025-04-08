using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Kivibot.CommandLine.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kivibot.CommandLine;

public class CliApplicationBuilder : IHostApplicationBuilder
{
    private readonly HostApplicationBuilder _inner;

    public CliApplicationBuilder()
    {
        _inner = new HostApplicationBuilder();
    }

    public void ConfigureContainer<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory,
        Action<TContainerBuilder>? configure = null) where TContainerBuilder : notnull
    {
        _inner.ConfigureContainer(factory, configure);
    }

    public IDictionary<object, object> Properties => ((IHostApplicationBuilder)_inner).Properties;

    public IConfigurationManager Configuration => ((IHostApplicationBuilder)_inner).Configuration;

    public IHostEnvironment Environment => _inner.Environment;

    public ILoggingBuilder Logging => _inner.Logging;

    public IMetricsBuilder Metrics => _inner.Metrics;

    public IServiceCollection Services => _inner.Services;

    public async Task<int> BuildAndExecuteAsync(string[] args)
    {
        using var host = _inner.Build();

        var rootCommand = host.Services.GetService<RootCommand>();
        if (rootCommand == null)
        {
            throw new InvalidOperationException("CLI not configured. Please call services.AddCommandLine()");
        }

        var parser = new CommandLineBuilder(rootCommand)
            .UseDefaults()
            .EnablePosixBundling()
            .AddMiddleware(ctx => ctx.BindingContext.AddService<InternalContext>(_ => new InternalContext
            {
                Host = host
            }))
            .Build();

        return await parser.InvokeAsync(args);
    }
}
