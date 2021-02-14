namespace Dax.Formatter.Tests.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;

    internal class DictionaryLookupNamingPolicy : JsonNamingPolicyDecorator
    {
        private readonly Dictionary<string, string> _dictionary;

        public DictionaryLookupNamingPolicy(Dictionary<string, string> dictionary, JsonNamingPolicy policy) : base(policy) => _dictionary = dictionary ?? throw new ArgumentNullException();

        public override string ConvertName(string name) => _dictionary.TryGetValue(name, out var value) ? value : base.ConvertName(name);
    }
}
