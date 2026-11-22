using System.Text.Json.Serialization;

namespace Dax.Formatter.Wire;

/// <summary>
/// The generated JSON serialization of the request and response bodies, which needs no reflection.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(FormatRequestBody))]
[JsonSerializable(typeof(List<FormatResponseItem?>))]
internal sealed partial class WireJsonContext : JsonSerializerContext
{
}
