using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace Jammer
{
    /// <summary>Finds the public SoundCloud client ID from the site's JavaScript assets.</summary>
    public sealed class SCClientIdFetcher
    {
        private static readonly Uri SoundCloudUri = new("https://soundcloud.com/");
        private static readonly Regex ScriptRegex = new(
            @"(?:src\s*=\s*[""'])(?<url>(?:https?:)?//a-v2\.sndcdn\.com/assets/[^""']+\.js(?:\?[^""']*)?|/assets/[^""']+\.js(?:\?[^""']*)?)[""']",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ClientIdRegex = new(
            @"client_id\s*[:=]\s*[""'](?<id>[A-Za-z0-9]{32})[""']",
            RegexOptions.Compiled);
        private static readonly Regex ClientIdFallbackRegex = new(
            @"[""']client_id[""']\s*:\s*[""'](?<id>[A-Za-z0-9]{32})[""']",
            RegexOptions.Compiled);
        private static readonly HttpClient SharedHttpClient = CreateHttpClient();

        private readonly HttpClient _httpClient;

        public SCClientIdFetcher(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? SharedHttpClient;
        }

        public static Task<string> GetClientId(CancellationToken cancellationToken = default) =>
            new SCClientIdFetcher().FetchAsync(cancellationToken);

        public async Task<string> FetchAsync(CancellationToken cancellationToken = default)
        {
            string html = await GetStringAsync(SoundCloudUri, cancellationToken);
            IReadOnlyList<Uri> scripts = ExtractScriptUrls(html, SoundCloudUri);
            if (scripts.Count == 0)
            {
                throw new InvalidOperationException("SoundCloud did not expose any JavaScript asset URLs.");
            }

            for (int index = scripts.Count - 1; index >= 0; index--)
            {
                string script = await GetStringAsync(scripts[index], cancellationToken);
                string? clientId = ExtractClientId(script);
                if (clientId != null)
                {
                    return clientId;
                }
            }

            throw new InvalidOperationException("No valid SoundCloud client ID was found in the site's JavaScript assets.");
        }

        public static IReadOnlyList<Uri> ExtractScriptUrls(string html, Uri baseUri)
        {
            var result = new List<Uri>();
            foreach (Match match in ScriptRegex.Matches(WebUtility.HtmlDecode(html)))
            {
                string value = match.Groups["url"].Value;
                if (value.StartsWith("//", StringComparison.Ordinal))
                {
                    value = "https:" + value;
                }

                if (!Uri.TryCreate(baseUri, value, out Uri? uri) ||
                    !uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Relative /assets URLs are accepted only after resolving them onto the CDN.
                if (uri.Host.Equals("soundcloud.com", StringComparison.OrdinalIgnoreCase) && value.StartsWith("/assets/"))
                {
                    uri = new Uri("https://a-v2.sndcdn.com" + value);
                }

                if (uri.Host.Equals("a-v2.sndcdn.com", StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(uri);
                }
            }

            return result;
        }

        public static string? ExtractClientId(string script)
        {
            Match match = ClientIdRegex.Match(script);
            if (!match.Success)
            {
                match = ClientIdFallbackRegex.Match(script);
            }

            return match.Success ? match.Groups["id"].Value : null;
        }

        private async Task<string> GetStringAsync(Uri uri, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            using HttpResponseMessage response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        private static HttpClient CreateHttpClient()
        {
            var handler = new SocketsHttpHandler
            {
                AllowAutoRedirect = true,
                AutomaticDecompression = DecompressionMethods.All,
                ConnectTimeout = TimeSpan.FromSeconds(10)
            };
            return new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
        }
    }
}
