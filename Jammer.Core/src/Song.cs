using System.Text.Json;
using System.Text.Json.Serialization;
using TagLib;

namespace Jammer
{
    public class Song
    {
        [JsonIgnore]
        public string? URI { get; set; }
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
            if (URI != null && URI.Contains(Utils.jammerFileDelimeter))
            {
                string[] parts = URI.Split(Utils.jammerFileDelimeter);
                URI = parts[0];
                string json = parts[1];
                
                if (string.IsNullOrEmpty(json))
                {
                    return;
                }

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
        /// The combined string is in the format of "path?|{json}".
        /// </remarks>
        public static string ToSongString(this Song song)
        {
            // Message.Data(song.URI + " " + song.Title + " " + song.Author + " " + song.Album + " " + song.Year + " " + song.Genre, "ToSongString");
            if (IsAlreadyInString(song))
            {
                return song.URI;
            }
            
            if (song == null || string.IsNullOrEmpty(song.URI))
            {
                return string.Empty; // or handle it as needed
            }

            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            string songString = song.URI + Utils.jammerFileDelimeter;
            songString += JsonSerializer.Serialize(song, options);
            return songString;
        }

        public static bool IsAlreadyInString(this Song song)
        {
            if (song == null || string.IsNullOrEmpty(song.URI))
            {
                return false;
            }

            return song.URI.Contains(Utils.jammerFileDelimeter) && song.URI.Contains('{') && song.URI.Contains('}');
        }

        
        /// <summary>
        /// Get the title of the song
        /// </summary>
        /// <param name="title">title</param>
        /// <param name="getOrNot">get | not | getMeta</param>
        /// <returns></returns>
        public static string Title(string song)
        {
            Song song1 = new Song() { URI = song };
            song1.ExtractSongDetails();
            return song1.Title ?? song1.URI;
        }
    }
}