using Newtonsoft.Json;

namespace FImonBot
{
    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefix")]
        public string Prefix { get; private set; }

        [JsonProperty("database_link")]
        public string DatabaseLink { get; private set; }

        [JsonProperty("admin_ids")]
        public ulong[] AdminIds { get; private set; }
    }
}
