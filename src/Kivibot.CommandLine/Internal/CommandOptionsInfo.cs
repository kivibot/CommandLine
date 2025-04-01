using System.CommandLine;
using System.CommandLine.Parsing;

namespace Kivibot.CommandLine.Internal;

public static class CommandOptionsInfo<T>
{
    private static Func<Command, Func<ParseResult, T>>? _createBinder;

    public static Func<Command, Func<ParseResult, T>> CreateBinder
    {
        get => _createBinder ??= ReflectionOptionsBinder.CreateBinder<T>();
        set => _createBinder = value;
    }
}