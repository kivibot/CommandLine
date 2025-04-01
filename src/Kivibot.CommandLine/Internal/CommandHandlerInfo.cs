using System.Reflection;

namespace Kivibot.CommandLine.Internal;

public static class CommandHandlerInfo<T>
{
    // ReSharper disable once StaticMemberInGenericType
    private static bool? _noHost;

    public static bool NoHost
    {
        get => _noHost ??= typeof(T).GetCustomAttribute<HandlerAttribute>()?.NoHost ?? false;
        set => _noHost = value;
    }
}