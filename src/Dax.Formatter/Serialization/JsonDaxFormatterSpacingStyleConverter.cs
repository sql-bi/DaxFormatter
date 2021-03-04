namespace Dax.Formatter.Serialization
{
    using Dax.Formatter.Models;
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    internal class JsonDaxFormatterSpacingStyleConverter : JsonConverter<DaxFormatterSpacingStyle>
    {
        public override DaxFormatterSpacingStyle Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.True:
                    return DaxFormatterSpacingStyle.NoNpaceAfterFunction;
                case JsonTokenType.False:
                    return DaxFormatterSpacingStyle.SpaceAfterFunction;
            }

            throw new JsonException($"Unsupperted DaxFormatterSpacingStyle token type [{ reader.TokenType }]");
        }

        public override void Write(Utf8JsonWriter writer, DaxFormatterSpacingStyle value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case DaxFormatterSpacingStyle.SpaceAfterFunction:
                    writer.WriteBooleanValue(false);
                    break;
                case DaxFormatterSpacingStyle.NoNpaceAfterFunction:
                    writer.WriteBooleanValue(true);
                    break;
                default:
                    throw new JsonException($"Unsupperted DaxFormatterSpacingStyle [{ value }]");
            }            
        }
    }
}
