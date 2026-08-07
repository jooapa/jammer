using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace Jammer
{
    public struct RootRssData
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public IndividualRssData[] Content { get; set; }
    }

    public struct IndividualRssData
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public string PubDate { get; set; }
        public string Author { get; set; }
    }

    public static class Rss
    {
        public static async Task<RootRssData> GetRssData(string url)
        {
            try
            {
                var rssContent = await GetRssFeed(url);
                var rssData = ExtractRssData(rssContent);
                return rssData;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get RSS data: {ex.Message}");
                return new RootRssData
                {
                    Title = "Unknown Title",
                    Author = "Unknown Author",
                    Link = url,
                    Description = "Failed to fetch RSS feed",
                    Content = Array.Empty<IndividualRssData>()
                };
            }
        }

        private static async Task<string> GetRssFeed(string url)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        private static RootRssData ExtractRssData(string rssContent)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(rssContent);

                // Create a namespace manager and add common namespaces
                var namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
                namespaceManager.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
                namespaceManager.AddNamespace("media", "http://search.yahoo.com/mrss/");
                namespaceManager.AddNamespace("itunes", "http://www.itunes.com/dtds/podcast-1.0.dtd");
                namespaceManager.AddNamespace("custom", "http://example.com/custom"); // Add custom namespaces if needed

                // Extract channel-level information
                var channelNode = xmlDoc.SelectSingleNode("//channel");
                if (channelNode == null)
                {
                    throw new Exception("RSS channel not found.");
                }

                var title = channelNode.SelectSingleNode("title")?.InnerText ?? Locale.UiMessages.UnknownTitle;
                var author = channelNode.SelectSingleNode("author")?.InnerText ??
                             channelNode.SelectSingleNode("managingEditor")?.InnerText ?? Locale.UiMessages.UnknownAuthor;
                var link = channelNode.SelectSingleNode("link")?.InnerText ?? Locale.UiMessages.UnknownLink;
                var description = channelNode.SelectSingleNode("description")?.InnerText ?? Locale.UiMessages.NoDescription;

                // Extract items
                var items = new List<IndividualRssData>();
                var itemNodes = channelNode.SelectNodes("item");
                if (itemNodes != null)
                {
                    foreach (XmlNode itemNode in itemNodes)
                    {
                        try
                        {
                            var itemTitle = itemNode.SelectSingleNode("title")?.InnerText ?? Locale.UiMessages.UnknownTitle;
                            var itemDescription = itemNode.SelectSingleNode("description")?.InnerText ?? Locale.UiMessages.NoDescription;
                            var itemPubDate = itemNode.SelectSingleNode("pubDate")?.InnerText ?? Locale.UiMessages.UnknownDate;
                            var itemAuthor = itemNode.SelectSingleNode("author")?.InnerText ??
                                             itemNode.SelectSingleNode("dc:creator", namespaceManager)?.InnerText ??
                                             itemNode.SelectSingleNode("media:credit", namespaceManager)?.InnerText ??
                                             itemNode.SelectSingleNode("custom:author", namespaceManager)?.InnerText ??
                                             itemNode.SelectSingleNode("itunes:author", namespaceManager)?.InnerText ??
                                             null;

                            // Look for the media file in multiple possible locations
                            var itemLink = itemNode.SelectSingleNode("enclosure")?.Attributes["url"]?.Value ?? // Standard RSS enclosure
                                           itemNode.SelectSingleNode("media:content", namespaceManager)?.Attributes["url"]?.Value ?? // Media RSS
                                           itemNode.SelectSingleNode("media:group/media:content", namespaceManager)?.Attributes["url"]?.Value ?? // Media RSS group
                                           itemNode.SelectSingleNode("link")?.InnerText ?? // Fallback to <link>
                                           itemNode.SelectSingleNode("guid")?.InnerText ?? // GUID as a fallback
                                           Locale.UiMessages.UnknownLink;


                            if (itemAuthor == null)
                            {
                                itemAuthor = author;
                            }

                            items.Add(new IndividualRssData
                            {
                                Title = itemTitle,
                                Link = itemLink,
                                Description = itemDescription,
                                PubDate = itemPubDate,
                                Author = itemAuthor
                            });
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Failed to extract item data: {ex.Message}");
                        }
                    }
                }

                return new RootRssData
                {
                    Title = title,
                    Author = author,
                    Link = link,
                    Description = description,
                    Content = items.ToArray()
                };
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to extract RSS data: {ex.Message}");
                return new RootRssData
                {
                    Title = Locale.UiMessages.UnknownTitle,
                    Author = Locale.UiMessages.UnknownAuthor,
                    Link = Locale.UiMessages.UnknownLink,
                    Description = Locale.UiMessages.RssParseFailed,
                    Content = Array.Empty<IndividualRssData>()
                };
            }
        }
    }
}
