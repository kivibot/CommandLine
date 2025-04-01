using Microsoft.Extensions.Hosting;

namespace Kivibot.CommandLine.Internal;

internal class InternalContext
{
    public required IHost Host { get; set; }
}