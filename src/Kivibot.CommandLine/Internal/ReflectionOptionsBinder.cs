using System.CommandLine;
using System.CommandLine.Parsing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Kivibot.CommandLine.Internal;

internal class ReflectionOptionsBinder
{
    public static Func<Command, Func<ParseResult, T>> CreateBinder<T>()
    {
        var type = typeof(T);
        if (type.GetConstructor([]) == null)
        {
            throw new InvalidOperationException($"{type.Name} should have a parameterless constructor");
        }

        if (type.GetCustomAttribute<OptionsAttribute>() == null)
        {
            throw new InvalidOperationException($"{type.Name} should have OptionsAttribute");
        }

        var properties = new List<PropInfo>();
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanWrite)
            {
                continue;
            }

            if (!property.CanRead)
            {
                throw new NotSupportedException("Write-only properties are not supported");
            }

            properties.Add(GetPropInfo<T>(property));
        }
        
        foreach (var prop in properties)
        {
            if (prop is { OptionAttribute: not null, ArgumentAttribute: not null })
            {
                throw new InvalidOperationException(
                    "Properties can't have both OptionAttribute and ArgumentAttribute defined");
            }

            if (prop.OptionAttribute != null)
            {
                HandleOption<T>(prop, prop.OptionAttribute);
            }
            else
            {
                HandleArgument<T>(prop);
            }
        }

        return command =>
        {
            foreach (var prop in properties)
            {
                if (prop.Option != null)
                {
                    command.Add(prop.Option);
                }
                else if (prop.Argument != null)
                {
                    command.Add(prop.Argument);
                }
            }

            return parseResult =>
            {
                var instance = Activator.CreateInstance<T>();
                foreach (var prop in properties)
                {
                    if (prop.Option != null)
                    {
                        prop.Property.SetValue(instance, parseResult.GetValueForOption(prop.Option));
                    }
                    else if (prop.Argument != null)
                    {
                        prop.Property.SetValue(instance, parseResult.GetValueForArgument(prop.Argument));
                    }
                }

                return instance;
            };

        };
    }

    private static void HandleOption<T>(PropInfo prop, OptionAttribute optionAttribute)
    {
        var aliases = new List<string>();
        if (optionAttribute.Long != null)
        {
            aliases.Add($"--{optionAttribute.Long.TrimStart('-')}");
        }

        if (optionAttribute.Short != default(char))
        {
            aliases.Add($"-{optionAttribute.Short}");
        }

        if (aliases.Count == 0)
        {
            aliases.Add($"--{JsonNamingPolicy.KebabCaseLower.ConvertName(prop.Property.Name)}");
        }

        var option = (Option)Activator.CreateInstance(
            typeof(Option<>).MakeGenericType(prop.Property.PropertyType),
            aliases.ToArray(),
            optionAttribute.Description
        )!;

        if (prop.IsRequired)
        {
            option.IsRequired = true;
        }
        else
        {
            option.SetDefaultValueFactory(prop.GetDefaultValue);
        }

        prop.Option = option;
    }

    private static void HandleArgument<T>(PropInfo prop)
    {
        var name = prop.ArgumentAttribute?.Name?.ToUpperInvariant() ?? JsonNamingPolicy.KebabCaseUpper.ConvertName(prop.Property.Name);

        var argument = (Argument)Activator.CreateInstance(
            typeof(Argument<>).MakeGenericType(prop.Property.PropertyType),
            name,
            prop.ArgumentAttribute?.Description
        )!;

        if (!prop.IsRequired)
        {
            argument.SetDefaultValueFactory(prop.GetDefaultValue);
        }

        prop.Argument = argument;
    }

    private static PropInfo GetPropInfo<T>(PropertyInfo propertyInfo)
    {
        return new PropInfo()
        {
            Property = propertyInfo,
            OptionAttribute = propertyInfo.GetCustomAttribute<OptionAttribute>(),
            ArgumentAttribute = propertyInfo.GetCustomAttribute<ArgumentAttribute>(),
            IsRequired = propertyInfo.GetCustomAttribute<RequiredMemberAttribute>() != null,
            GetDefaultValue = () => propertyInfo.GetValue(Activator.CreateInstance<T>()),
        };
    }

    private class PropInfo
    {
        public required PropertyInfo Property { get; init; }

        public required OptionAttribute? OptionAttribute { get; init; }
        public required ArgumentAttribute? ArgumentAttribute { get; init; }

        public required bool IsRequired { get; init; }

        public required Func<object?> GetDefaultValue { get; init; }


        public Option? Option { get; set; }
        public Argument? Argument { get; set; }
    }
}