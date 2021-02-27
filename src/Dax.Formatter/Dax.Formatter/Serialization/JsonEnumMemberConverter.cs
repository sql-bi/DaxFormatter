namespace Dax.Formatter.Serialization
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class JsonEnumMemberConverter : JsonConverterFactory
    {
        private readonly bool _allowIntegerValues;
        private readonly JsonNamingPolicy _namingPolicy;
        private readonly JsonStringEnumConverter _baseConverter;

        public JsonEnumMemberConverter() 
            : this(namingPolicy: null, allowIntegerValues: true) 
        { 
        }

        public JsonEnumMemberConverter(JsonNamingPolicy namingPolicy = null, bool allowIntegerValues = true)
        {
            _namingPolicy = namingPolicy;
            _allowIntegerValues = allowIntegerValues;
            _baseConverter = new JsonStringEnumConverter(namingPolicy, allowIntegerValues);
        }

        public override bool CanConvert(Type typeToConvert) => _baseConverter.CanConvert(typeToConvert);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var query = from field in typeToConvert.GetFields(BindingFlags.Public | BindingFlags.Static)
                        let attr = field.GetCustomAttribute<EnumMemberAttribute>()
                        where attr != null
                        select (field.Name, attr.Value);

            var dictionary = query.ToDictionary((p) => p.Name, (p) => p.Value);
            if (dictionary.Count > 0)
            {
                var namingPolicy = new DictionaryLookupNamingPolicy(dictionary, _namingPolicy);
                var converter = new JsonStringEnumConverter(namingPolicy, _allowIntegerValues).CreateConverter(typeToConvert, options);

                return converter;
            }

            return _baseConverter.CreateConverter(typeToConvert, options);            
        }
    }
}
