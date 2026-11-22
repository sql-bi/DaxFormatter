namespace Dax.Formatter.Configuration;

/// <summary>
/// How much one request to the service carries.
/// </summary>
internal sealed class BatchLimits
{
    public static readonly BatchLimits Default = new(maxTexts: 250, maxCharacters: 250_000);

    public BatchLimits(int maxTexts, int maxCharacters)
    {
        MaxTexts = maxTexts;
        MaxCharacters = maxCharacters;
    }

    public int MaxTexts { get; }

    public int MaxCharacters { get; }
}
