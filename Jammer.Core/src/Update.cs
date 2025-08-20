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
                using (var httpClient = new HttpClient())
                {
                    using (var response = httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead).Result)
                    {
                        response.EnsureSuccessStatusCode();
                        
                        using (var contentStream = response.Content.ReadAsStreamAsync().Result)
                        using (var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            var totalBytes = response.Content.Headers.ContentLength ?? 0;
                            var buffer = new byte[8192];
                            var totalBytesRead = 0L;
                            int bytesRead;
                            
                            while ((bytesRead = contentStream.ReadAsync(buffer, 0, buffer.Length).Result) != 0)
                            {
                                fileStream.WriteAsync(buffer, 0, bytesRead).Wait();
                                totalBytesRead += bytesRead;
                                
                                if (totalBytes > 0)
                                {
                                    var progressPercentage = (int)((totalBytesRead * 100) / totalBytes);
                                    Console.WriteLine($"{Locale.OutsideItems.Downloaded} {totalBytesRead} {Locale.OutsideItems.Of} {totalBytes} {Locale.OutsideItems.Bytes} ({progressPercentage}%).");
                                }
                            }
                        }
                    }
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
