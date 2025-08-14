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
        public string? Duration { get; set; }
        public string? Description { get; set; }
        public string? PubDate { get; set; }
        /// <summary>
        /// Extracts song details from the Path property if it contains metadata.
        /// </summary>
        public void ExtractSongDetails()
        {
            if (URI != null && URI.Contains(Utils.JammerFileDelimeter))
            {
                string[] parts = URI.Split(Utils.JammerFileDelimeter);
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
                Duration = metadata.Duration;
                Description = metadata.Description;
                PubDate = metadata.PubDate;
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

            if (IsAlreadyInString(song))
            {
                // Message.Data(song.URI, "Already in string");
                return song.URI;
            }

            if (song == null || string.IsNullOrEmpty(song.URI))
            {
                return "";
            }

            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false,
                IgnoreReadOnlyProperties = true
            };

            // Create a new anonymous object with only defined properties
            var definedProperties = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(song.Title)) definedProperties.Add("Title", song.Title);
            if (!string.IsNullOrEmpty(song.Author)) definedProperties.Add("Author", song.Author);
            if (!string.IsNullOrEmpty(song.Album)) definedProperties.Add("Album", song.Album);
            if (!string.IsNullOrEmpty(song.Year)) definedProperties.Add("Year", song.Year);
            if (!string.IsNullOrEmpty(song.Genre)) definedProperties.Add("Genre", song.Genre);
            if (!string.IsNullOrEmpty(song.Duration)) definedProperties.Add("Duration", song.Duration);
            if (!string.IsNullOrEmpty(song.Description)) definedProperties.Add("Description", song.Description);
            if (!string.IsNullOrEmpty(song.PubDate)) definedProperties.Add("PubDate", song.PubDate);

            string songString = song.URI + Utils.JammerFileDelimeter;
            songString += JsonSerializer.Serialize(definedProperties, options);
            return songString;
        }

        public static Song ToSong(this string songString)
        {
            if (string.IsNullOrEmpty(songString))
            {
                return new Song();
            }

            if (songString.Contains(Utils.JammerFileDelimeter))
            {
                string[] parts = songString.Split(Utils.JammerFileDelimeter);
                string uri = parts[0];
                string json = parts[1];
                Song song = JsonSerializer.Deserialize<Song>(json) ?? new Song();
                song.URI = uri;
                return song;
            }

            return new Song() { URI = songString };
        }

        public static string ToSongM3UString(this Song song)
        {
            if (song == null || string.IsNullOrEmpty(song.URI))
            {
                return string.Empty; // or handle it as needed
            }

            string dur = song.Duration ?? "0";

            string songINF = "#EXTINF:" + dur + "," + song.Title;

            string songString = songINF
            + "\n" +
            song.URI;
            return songString;
        }

        public static bool IsAlreadyInString(this Song song)
        {
            if (song == null || string.IsNullOrEmpty(song.URI))
            {
                return false;
            }

            return song.URI.Contains(Utils.JammerFileDelimeter) && song.URI.Contains('{') && song.URI.Contains('}');
        }


        /// <summary>
        /// Get the title of the song
        /// </summary>
        /// <param name="title">title</param>
        /// <returns></returns>
        public static string Title(string song)
        {
            Song song1 = new Song() { URI = song };
            song1.ExtractSongDetails();
            if (song1.Title != null && song1.Title != "")
            {
                return song1.Title;
            }
            else
            {
                return song1.URI;
            }
        }

        /// <summary>
        /// Get the author of the song
        /// </summary>
        /// <param name="author">author</param>
        /// <returns></returns>
        public static string Author(string song)
        {
            Song song1 = new Song() { URI = song };
            song1.ExtractSongDetails();
            if (song1.Author != null && song1.Author != "")
            {
                return song1.Author;
            }
            else
            {
                return "";
            }
        }
    }
}