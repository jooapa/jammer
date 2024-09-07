using System.Text.Json;
using System.Text.Json.Serialization;
using TagLib;

namespace Jammer
{
    public class Song
    {
        [JsonIgnore]
        public string? Path { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Album { get; set; }
        public string? Year { get; set; }
        public string? Genre { get; set; }

        /// <summary>
        /// Extracts song details from the Path property if it contains metadata.
        /// </summary>
        public void ExtractSongDetails()
        {
            if (Path != null && Path.Contains("###@@@###"))
            {
                string[] parts = Path.Split("###@@@###");
                Path = parts[0];
                string json = parts[1];

                Song metadata = JsonSerializer.Deserialize<Song>(json) ?? new Song();

                Title = metadata.Title;
                Author = metadata.Author;
                Album = metadata.Album;
                Year = metadata.Year;
                Genre = metadata.Genre;
            }
        }
    }

    public static class SongExtensions
    {
        /// <summary>
        /// Combines the path and serialized representation of a song into a single string.
        /// </summary>
        /// <param name="song">The song to combine.</param>
        /// <returns>The combined string.</returns>
        /// <remarks>
        /// The combined string is in the format of "path###@@@###{json}".
        /// </remarks>
        public static string ToSongString(this Song song)
        {
            string songString = song.Path + "###@@@###";
            songString += JsonSerializer.Serialize(song);
            return songString;
        }

        
        /// <summary>
        /// Get the title of the song
        /// </summary>
        /// <param name="title">title</param>
        /// <param name="getOrNot">get | not | getMeta</param>
        /// <returns></returns>
        public static string Title(string song)
        {
            Song song1 = new Song() { Path = song };
            song1.ExtractSongDetails();
            return song1.Title ?? song1.Path;
        }
    }
}