#if !NET
namespace System.Runtime.CompilerServices;

// The record of the readme samples needs it for its init accessors, and .NET Framework doesn't have it.
internal static class IsExternalInit
{
}
#endif
