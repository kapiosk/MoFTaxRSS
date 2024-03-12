using System.Text;
using MoFTaxRSS;

using var httpClient = new HttpClient();
try
{
    using var reader = System.Xml.XmlReader.Create("https://www.mof.gov.cy/mof/tax/taxdep.nsf/rssfeed.xml");
    var feed = System.ServiceModel.Syndication.SyndicationFeed.Load(reader);
    var filePath = Path.Combine("Data", "feed_en.csv");
    List<FeedItem> items = filePath.ReadFromCSV<FeedItem>();
    var count = items.Count;
    var newItems = feed.Items.Select(i => new FeedItem(i.Title.Text, i.Links[0].Uri.AbsoluteUri, i.PublishDate.UtcDateTime)).ToList();
    if (newItems is not null)
        items.AddRange(newItems);
    items.OrderByDescending(c => c.PublishDate).Distinct().WriteCSV(filePath);
    var NTFY_CHANNEL = Environment.GetEnvironmentVariable("NTFY_CHANNEL");
    if (count < items.Count)
    {
        if (!string.IsNullOrEmpty(NTFY_CHANNEL))
            await httpClient.PostAsync(NTFY_CHANNEL, new StringContent("New Announcement from Tax Department English", Encoding.UTF8, "application/x-www-form-urlencoded"));
    }
}
catch (Exception ex)
{
    File.AppendAllLines(Path.Combine("Data", "Exceptions.log"), [ex.Message]);
}

try
{
    var response = await httpClient.GetStringAsync("https://www.mof.gov.cy/mof/tax/taxdep.nsf/rssfeedgr.xml");
    response = response.Replace(" & ", " &amp; ").Replace("\n", "");
    var rss = response.DeserializeFromXML<Rss>();
    var filePath = Path.Combine("Data", "feed_gr.csv");
    List<FeedItem> items = filePath.ReadFromCSV<FeedItem>();
    var count = items.Count;
    var newItems = rss?.Channel.Item.Select(i => new FeedItem(i.Title, i.Link, DateTime.Parse(i.PubDate).ToUniversalTime())).ToList();
    if (newItems is not null)
        items.AddRange(newItems);
    items.OrderByDescending(c => c.PublishDate).Distinct().WriteCSV(filePath);
    var NTFY_CHANNEL = Environment.GetEnvironmentVariable("NTFY_CHANNEL");
    if (count < items.Count)
    {
        if (!string.IsNullOrEmpty(NTFY_CHANNEL))
            await httpClient.PostAsync(NTFY_CHANNEL, new StringContent("New Announcement from Tax Department Greek", Encoding.UTF8, "application/x-www-form-urlencoded"));
    }
}
catch (Exception ex)
{
    File.AppendAllLines(Path.Combine("Data", "Exceptions.log"), [ex.Message]);
}
