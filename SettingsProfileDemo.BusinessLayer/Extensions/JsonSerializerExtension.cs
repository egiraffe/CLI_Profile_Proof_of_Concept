using System.Text.Encodings.Web;
using System.Text.Json;

namespace SettingsProfileDemo.BusinessLayer.Extensions
{
    public static class JsonSerializerExtension
    {
        public static JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.Default,
            IgnoreNullValues = true,
            IgnoreReadOnlyProperties = false,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true
        };
    }
}