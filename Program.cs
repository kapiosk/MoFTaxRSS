using System.Text;
using System.Text.RegularExpressions;
using MoFTaxRSS;
//https://docs.ntfy.sh/publish/#__tabbed_3_4
//https://mof.gov.cy/gr/%CF%84%CE%B5%CE%BB%CE%B5%CF%85%CF%84%CE%B1%CE%AF%CE%B1-%CE%BD%CE%AD%CE%B1
// using var reader = System.Xml.XmlReader.Create("https://www.mof.gov.cy/mof/tax/taxdep.nsf/rssfeed.xml");
// var feed = System.ServiceModel.Syndication.SyndicationFeed.Load(reader);

using var httpClient = new HttpClient();
var directory = "Data";
Directory.CreateDirectory(directory);
var NTFY_CHANNEL = Environment.GetEnvironmentVariable("NTFY_CHANNEL");
var NTFY_EMAIL = Environment.GetEnvironmentVariable("NTFY_EMAIL");
TimeZoneInfo cyprusTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Nicosia");

Dictionary<string, string> sites = new()
{
    ["https://www.mof.gov.cy/mof/tax/taxdep.nsf/rssfeed.xml"] = "feed_en.csv",
    ["https://www.mof.gov.cy/mof/tax/taxdep.nsf/rssfeedgr.xml"] = "feed_gr.csv",
};

foreach (var site in sites)
    try
    {
        var response = await httpClient.GetStringAsync(site.Key);
        var rss = response.Replace(" & ", " &amp; ")
                          .Replace("\n", "")
                          .DeserializeFromXML<Rss>() ?? throw new Exception("Failed to deserialize RSS feed");
        var filePath = Path.Combine(directory, site.Value);
        List<FeedItem> items;
        if (File.Exists(filePath))
            items = filePath.ReadFromCSV<FeedItem>();
        else
            items = [];
        var newItems = rss.Channel.Item.Select(i => new FeedItem(i.Title, i.Link, DateTime.Parse(i.PubDate).ToUniversalTime()))
                                       .Except(items);
        if (newItems.Any())
        {
            items.Concat(newItems).OrderByDescending(c => c.PublishDate).WriteCSV(filePath);
            if (!string.IsNullOrEmpty(NTFY_CHANNEL))
            {
                foreach (var newItem in newItems)
                {
                    var link = newItem.Link;
                    try
                    {
                        var linkData = await httpClient.GetStringAsync(link);
                        if (!string.IsNullOrEmpty(linkData))
                        {
                            Match match = RegexExtensions.PDFRegex().Match(linkData);
                            if (match.Success)
                                if (!match.Groups[1].Value.StartsWith("http"))
                                    link = link[..(link.LastIndexOf('/') + 1)] + match.Groups[1].Value;
                                else
                                    link = match.Groups[1].Value;
                        }
                    }
                    catch { }
                    var content = new StringContent(
                        string.Join(
                            "\n ",
                            newItem.Title,
                            link,
                            TimeZoneInfo.ConvertTimeFromUtc(newItem.PublishDate, cyprusTimeZone)
                        ),
                        Encoding.UTF8,
                        "application/x-www-form-urlencoded"
                    );
                    content.Headers.Add("Title", "MoFTaxRSS");
                    content.Headers.Add("Click", newItem.Link);
                    // content.Headers.Add("Priority", "4");
                    if (!string.IsNullOrEmpty(NTFY_EMAIL))
                        content.Headers.Add("Email", NTFY_EMAIL);
                    await httpClient.PostAsync(NTFY_CHANNEL, content);
                }
            }
        }
    }
    catch (Exception ex)
    {
        File.AppendAllLines(Path.Combine("Data", "Exceptions.log"), [ex.Message]);
    }
