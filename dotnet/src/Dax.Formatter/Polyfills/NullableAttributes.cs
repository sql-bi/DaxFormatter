#if !NET
namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.ReturnValue)]
internal sealed class NotNullAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class NotNullWhenAttribute : Attribute
{
    public NotNullWhenAttribute(bool returnValue)
    {
        ReturnValue = returnValue;
    }

    public bool ReturnValue { get; }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
internal sealed class MemberNotNullWhenAttribute : Attribute
{
    public MemberNotNullWhenAttribute(bool returnValue, string member)
    {
        ReturnValue = returnValue;
        Member = member;
    }

    public bool ReturnValue { get; }

    public string Member { get; }
}
#endif
