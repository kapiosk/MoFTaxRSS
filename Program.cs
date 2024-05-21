using System.Text;
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

Dictionary<string, string> sites = new()
{
    ["https://www.mof.gov.cy/mof/tax/taxdep.nsf/rssfeed.xml"] = "feed_en.csv",
    ["https://www.mof.gov.cy/mof/tax/taxdep.nsf/rssfeedgr.xml"] = "feed_gr.csv"
};

foreach (var site in sites)
    try
    {
        var response = await httpClient.GetStringAsync(site.Key);
        var rss = response.Replace(" & ", " &amp; ").Replace("\n", "")
                          .DeserializeFromXML<Rss>() ?? throw new Exception("Failed to deserialize RSS feed");
        var filePath = Path.Combine(directory, site.Value);
        List<FeedItem> items;
        if (File.Exists(filePath))
            items = filePath.ReadFromCSV<FeedItem>();
        else
            items = [];
        var oldCount = items.Count;
        var newItems = rss.Channel.Item.Select(i => new FeedItem(i.Title, i.Link, DateTime.Parse(i.PubDate).ToUniversalTime()));
        items = items.Concat(newItems).OrderByDescending(c => c.PublishDate).Distinct().ToList();
        if (oldCount < items.Count)
        {
            items.WriteCSV(filePath);
            if (!string.IsNullOrEmpty(NTFY_CHANNEL))
            {
                var content = new StringContent($"New Announcement from Tax Department {site.Value}", Encoding.UTF8, "application/x-www-form-urlencoded");
                if (!string.IsNullOrEmpty(NTFY_EMAIL))
                    content.Headers.Add("Email", NTFY_EMAIL);
                await httpClient.PostAsync(NTFY_CHANNEL, content);
            }
        }
    }
    catch (Exception ex)
    {
        File.AppendAllLines(Path.Combine("Data", "Exceptions.log"), [ex.Message]);
    }
