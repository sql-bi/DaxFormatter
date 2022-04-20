namespace Dax.Formatter.Serialization
{
    using System.Text.Json;

    internal class JsonNamingPolicyDecorator : JsonNamingPolicy
    {
        readonly JsonNamingPolicy? _policy;

        public JsonNamingPolicyDecorator(JsonNamingPolicy? policy) => _policy = policy;

        public override string ConvertName(string name) => _policy == null ? name : _policy.ConvertName(name);
    }
}
