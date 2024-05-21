using System.Text;
using MoFTaxRSS;

using var httpClient = new HttpClient();
var directory = "Data";
Directory.CreateDirectory(directory);
var NTFY_CHANNEL = Environment.GetEnvironmentVariable("NTFY_CHANNEL");

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
                await httpClient.PostAsync(NTFY_CHANNEL, new StringContent($"New Announcement from Tax Department {site.Value}", Encoding.UTF8, "application/x-www-form-urlencoded"));
        }
    }
    catch (Exception ex)
    {
        File.AppendAllLines(Path.Combine("Data", "Exceptions.log"), [ex.Message]);
    }

// using var reader = System.Xml.XmlReader.Create("https://www.mof.gov.cy/mof/tax/taxdep.nsf/rssfeed.xml");
// var feed = System.ServiceModel.Syndication.SyndicationFeed.Load(reader);
// var filePath = Path.Combine("Data", "feed_en.csv");
// List<FeedItem> items = filePath.ReadFromCSV<FeedItem>();
// var count = items.Count;
// var newItems = feed.Items.Select(i => new FeedItem(i.Title.Text, i.Links[0].Uri.AbsoluteUri, i.PublishDate.UtcDateTime)).ToList();
// if (newItems is not null)
//     items.AddRange(newItems);
// items.OrderByDescending(c => c.PublishDate).Distinct().WriteCSV(filePath);
// var NTFY_CHANNEL = Environment.GetEnvironmentVariable("NTFY_CHANNEL");
// if (count < items.Count)
// {
//     if (!string.IsNullOrEmpty(NTFY_CHANNEL))
//         await httpClient.PostAsync(NTFY_CHANNEL, new StringContent("New Announcement from Tax Department English", Encoding.UTF8, "application/x-www-form-urlencoded"));
// }
