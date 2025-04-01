using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using Microsoft.Extensions.DependencyInjection;

namespace Kivibot.CommandLine.Internal;

internal class CommandHandler<THandler, TOptions> : ICommandHandler
    where THandler : ICommandHandler<TOptions>
{
    private readonly Func<ParseResult, TOptions> _optionsBinder;

    public CommandHandler(Func<ParseResult, TOptions> optionsBinder)
    {
        _optionsBinder = optionsBinder;
    }

    public int Invoke(InvocationContext context)
    {
        throw new NotImplementedException("Only InvokeAsync is implemented");
    }

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        var internalContext = context.BindingContext.GetRequiredService<InternalContext>();
        var cancellationToken = context.GetCancellationToken();

        var options = _optionsBinder.Invoke(context.ParseResult);

        var handler = internalContext.Host.Services.GetRequiredService<THandler>();

        if (CommandHandlerInfo<THandler>.NoHost)
        {
            await handler.ExecuteAsync(options, cancellationToken);
            return 0;
        }

        await internalContext.Host.StartAsync(cancellationToken);
        try
        {
            await handler.ExecuteAsync(options, cancellationToken);
        }
        finally
        {
            await internalContext.Host.StopAsync(cancellationToken);
        }

        return 0;
    }
}