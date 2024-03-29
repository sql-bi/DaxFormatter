﻿namespace Dax.Formatter.Serialization
{
    using Dax.Formatter.Models;
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    internal class JsonDaxFormatterSpacingStyleConverter : JsonConverter<DaxFormatterSpacingStyle>
    {
        public override DaxFormatterSpacingStyle Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.True => DaxFormatterSpacingStyle.NoSpaceAfterFunction,
                JsonTokenType.False => DaxFormatterSpacingStyle.SpaceAfterFunction,
                _ => throw new JsonException($"Unsupperted { nameof(DaxFormatterSpacingStyle) } token type [{ reader.TokenType }]"),
            };
        }

        public override void Write(Utf8JsonWriter writer, DaxFormatterSpacingStyle value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case DaxFormatterSpacingStyle.SpaceAfterFunction:
                    writer.WriteBooleanValue(false);
                    break;
                case DaxFormatterSpacingStyle.NoSpaceAfterFunction:
                    writer.WriteBooleanValue(true);
                    break;
                default:
                    throw new JsonException($"Unsupperted DaxFormatterSpacingStyle [{ value }]");
            }
        }
    }
}
