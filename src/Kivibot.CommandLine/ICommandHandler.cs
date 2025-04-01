using Kivibot.CommandLine.Internal;

namespace Kivibot.CommandLine;

public interface ICommandHandler<TOptions>
{
    Task ExecuteAsync(TOptions options, CancellationToken cancellationToken);
}