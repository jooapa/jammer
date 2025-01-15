using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Spectre.Console;
using System.IO;
namespace Jammer
{
    public class Update
    {
        public static string UpdateJammer(string version)
        {
            string downloadUrl = "https://github.com/jooapa/Jammer/releases/download/" + version + "/Jammer-Setup_V" + version + ".exe";
            string downloadPath = Path.Combine(Utils.JammerPath, "Jammer-Setup_V" + version + ".exe");
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.DownloadProgressChanged += (sender, e) =>
                    {
                        Console.WriteLine($"{Locale.OutsideItems.Downloaded} {e.BytesReceived} {Locale.OutsideItems.Of} {e.TotalBytesToReceive} {Locale.OutsideItems.Bytes} ({e.ProgressPercentage}%).");


                    };

                    webClient.DownloadFile(downloadUrl, downloadPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Locale.OutsideItems.ErrorDownload} " + ex.Message);
            }

            return Path.GetFullPath(downloadPath);
        }

        public static string CheckForUpdate(string version)
        {
            string url = "https://raw.githubusercontent.com/jooapa/Jammer/master/VERSION";
            string latestVersion = "";
            using (HttpClient client = new HttpClient())
            {
                latestVersion = client.GetStringAsync(url).Result;
            }
            AnsiConsole.MarkupLine($"{Locale.OutsideItems.LatestVersion}: [green]" + latestVersion + "[/]");
            if (latestVersion != version)
            {
                return latestVersion;
            }
            return "";
        }
    }
}
