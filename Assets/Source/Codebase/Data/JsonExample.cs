using Newtonsoft.Json;

namespace Source.Codebase.Data
{
    public struct JsonExample
    {
        [JsonProperty(PropertyName = "link")]
        public string link;
    }
}