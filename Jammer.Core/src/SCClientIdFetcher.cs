using PuppeteerSharp;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Jammer
{
    public static class SCClientIdFetcher
    {
        public static async Task<string> MonitorNetwork(string url)
        {
            Message.Data("Starting Puppeteer...", "...", false, false);
            await new BrowserFetcher().DownloadAsync();
            Message.Data("Puppeteer started.", "...", false, false);
            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });

            using var page = await browser.NewPageAsync();
            var tcs = new TaskCompletionSource<string>();
            var clientIdFound = false;

            page.Request += async (sender, e) =>
            {
                Message.Data($">> Request: {e.Request.Method} {e.Request.Url}", "Soundcloud sending back..", false, false);

                if (!clientIdFound && e.Request.Url.Contains("client_id"))
                {
                    var clientIdMatch = Regex.Match(e.Request.Url, @"client_id=([^&]+)");
                    if (clientIdMatch.Success)
                    {
                        clientIdFound = true;
                        await browser.CloseAsync();
                        tcs.TrySetResult(clientIdMatch.Groups[1].Value);
                    }
                }
            };

            await page.GoToAsync(url, WaitUntilNavigation.DOMContentLoaded);

            var clientId = await tcs.Task;
            return clientId;
        }

        public static async Task<string> GetClientId()
        {
            const string targetUrl = "https://soundcloud.com/rick-astley-official/never-gonna-give-you-up-4";
            return await MonitorNetwork(targetUrl);
        }
    }
}