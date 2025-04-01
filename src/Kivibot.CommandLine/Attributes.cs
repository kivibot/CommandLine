namespace Kivibot.CommandLine;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class OptionAttribute : Attribute
{
    public string? Long { get; set; }
    public char Short { get; set; }
    public string? Description { get; set; }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class ArgumentAttribute : Attribute
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class OptionsAttribute : Attribute
{

}

[AttributeUsage(AttributeTargets.Class)]
public sealed class HandlerAttribute : Attribute
{
    public bool NoHost { get; set; } = false;
}