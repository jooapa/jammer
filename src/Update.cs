using System.Net.Http;
using System.Threading.Tasks;

namespace jammer { 
    public class Update {
        public static async Task update(string version) {
            using (HttpClient client = new HttpClient()) {
                string downloadUrl = "https://github.com/jooapa/jammer/releases/download/X.X.X/jammer-Setup_V"+ version +".exe";
                string savePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Desktop\\jammer-Setup_V"+ version +".exe";
                await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead)
                    .ContinueWith(async (responseTask) => {
                        var response = await responseTask;
                        using (var contentStream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                        {
                            await contentStream.CopyToAsync(fileStream);
                        }
                    });
            }
        }

        public static string CheckForUpdate(string version) {
            string url = "https://raw.githubusercontent.com/jooapa/jammer/master/VERSION";
            using HttpClient client = new HttpClient();
            string latestVersion = client.GetStringAsync(url).Result;
            if (latestVersion != version) {
                return latestVersion;
            }
            return "";
        }
    }
}