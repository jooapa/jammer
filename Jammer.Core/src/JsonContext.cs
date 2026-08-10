using System.Text.Json.Serialization;

namespace Jammer
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Themes.Theme))]
    [JsonSerializable(typeof(SpotifyAuthState))]
    public partial class JsonContext : JsonSerializerContext
    {
    }
}
