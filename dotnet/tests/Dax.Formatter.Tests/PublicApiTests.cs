using System.Reflection;
using System.Text;
using Dax.Formatter.Tests.Fixtures;
using Xunit;

namespace Dax.Formatter.Tests;

/// <summary>
/// Compares the public surface of the library with <c>PublicApi.approved.txt</c>, on each target framework: a
/// change of the surface is a decision, so it must show up as a change of that file.
/// </summary>
public class PublicApiTests
{
    private const BindingFlags Declared =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    private static readonly Assembly s_library = typeof(DaxFormatterClient).Assembly;

    [Fact]
    public void PublicSurface_OfTheLibrary_IsTheApprovedOne()
    {
        var approved = FixtureFile.ReadText("PublicApi.approved.txt");

        var actual = Describe(s_library.GetExportedTypes());

        Assert.Equal(approved, actual);
    }

    [Fact]
    public void PublicTypes_OfTheLibrary_AreAllInItsRootNamespace()
    {
        var namespaces = s_library.GetExportedTypes().Select(type => type.Namespace).Distinct();

        Assert.Equal(["Dax.Formatter"], namespaces);
    }

    private static string Describe(IEnumerable<Type> types)
    {
        var text = new StringBuilder();
        foreach (var type in types.OrderBy(type => type.FullName, StringComparer.Ordinal))
        {
            text.Append(DescribeType(type)).Append('\n');
            foreach (var member in DescribeMembers(type).OrderBy(member => member, StringComparer.Ordinal))
            {
                text.Append("    ").Append(member).Append('\n');
            }
        }

        return text.ToString().TrimEnd();
    }

    private static string DescribeType(Type type)
    {
        if (type.IsEnum)
        {
            return "enum " + Name(type);
        }

        var modifiers = type.IsAbstract && type.IsSealed ? "static " : type.IsSealed ? "sealed " : string.Empty;
        var bases = new List<string>();
        if (type.BaseType is not null && type.BaseType != typeof(object))
        {
            bases.Add(Name(type.BaseType));
        }

        // Only the interfaces that the type adds: those of a framework base type differ between frameworks.
        var inherited = type.BaseType?.GetInterfaces() ?? [];
        bases.AddRange(type.GetInterfaces().Except(inherited).Select(Name).OrderBy(name => name, StringComparer.Ordinal));
        return modifiers + "class " + Name(type) + (bases.Count == 0 ? string.Empty : " : " + string.Join(", ", bases));
    }

    private static IEnumerable<string> DescribeMembers(Type type)
    {
        if (type.IsEnum)
        {
            return Enum.GetNames(type).Select(name => name + " = " + Convert.ToInt32(Enum.Parse(type, name), null));
        }

        var constructors = type.GetConstructors(Declared).Where(IsVisible).Select(
            constructor => Access(constructor) + "constructor(" + Parameters(constructor) + ")");
        var methods = type.GetMethods(Declared).Where(method => IsVisible(method) && !method.IsSpecialName).Select(
            method => Access(method) + Name(method.ReturnType) + " " + method.Name + GenericArguments(method)
                + "(" + Parameters(method) + ")");
        var properties = type.GetProperties(Declared).Where(property => Accessors(property).Length > 0).Select(
            property => Access(property.GetMethod ?? property.SetMethod!) + Name(property.PropertyType) + " "
                + property.Name + " { " + Accessors(property) + "}");
        var fields = type.GetFields(Declared).Where(field => field.IsPublic || field.IsFamily).Select(
            field => (field.IsLiteral ? "const " : string.Empty) + Name(field.FieldType) + " " + field.Name
                + (field.IsLiteral ? " = \"" + field.GetRawConstantValue() + "\"" : string.Empty));

        return constructors.Concat(methods).Concat(properties).Concat(fields);
    }

    private static bool IsVisible(MethodBase method)
    {
        return method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly;
    }

    private static string Access(MethodBase method)
    {
        var access = method.IsPublic ? string.Empty : "protected ";
        if (method.IsStatic)
        {
            return access + "static ";
        }

        var isOverride = method is MethodInfo info && info.GetBaseDefinition().DeclaringType != info.DeclaringType;
        if (isOverride)
        {
            return access + "override ";
        }

        return access + (method.IsVirtual && !method.IsFinal ? "virtual " : string.Empty);
    }

    private static string Accessors(PropertyInfo property)
    {
        var getter = property.GetMethod is not null && IsVisible(property.GetMethod) ? "get; " : string.Empty;
        var setter = property.SetMethod is not null && IsVisible(property.SetMethod) ? "set; " : string.Empty;
        return getter + setter;
    }

    private static string Parameters(MethodBase method)
    {
        return string.Join(", ", method.GetParameters().Select(
            parameter => Name(parameter.ParameterType) + " " + parameter.Name + (parameter.IsOptional ? " = default" : string.Empty)));
    }

    private static string GenericArguments(MethodInfo method)
    {
        return method.IsGenericMethodDefinition
            ? "<" + string.Join(", ", method.GetGenericArguments().Select(Name)) + ">"
            : string.Empty;
    }

    private static string Name(Type type)
    {
        if (type.IsGenericParameter)
        {
            return type.Name;
        }

        if (Nullable.GetUnderlyingType(type) is Type underlying)
        {
            return Name(underlying) + "?";
        }

        if (!type.IsGenericType)
        {
            return type.Namespace + "." + type.Name;
        }

        var name = type.Name.Split('`')[0];
        return type.Namespace + "." + name + "<" + string.Join(", ", type.GetGenericArguments().Select(Name)) + ">";
    }
}
