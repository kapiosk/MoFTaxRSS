﻿using System.Text;
using MoFTaxRSS;

// using var reader = System.Xml.XmlReader.Create("https://www.mof.gov.cy/mof/tax/taxdep.nsf/rssfeed.xml");
// var feed = System.ServiceModel.Syndication.SyndicationFeed.Load(reader);
// var items = feed.Items.Select(i => new FeedItem(i.Title.Text, i.Links[0].Uri.AbsoluteUri, i.PublishDate.UtcDateTime)).ToList();
try
{
    using var httpClient = new HttpClient();
    var response = await httpClient.GetStringAsync("https://www.mof.gov.cy/mof/tax/taxdep.nsf/rssfeedgr.xml");
    response = response.Replace(" & ", " &amp; ").Replace("\n", "");
    var rss = response.DeserializeFromXML<Rss>();
    List<FeedItem> items = "Data\\feed.csv".ReadFromCSV<FeedItem>();
    var count = items.Count;
    var newItems = rss?.Channel.Item.Select(i => new FeedItem(i.Title, i.Link, DateTime.Parse(i.PubDate).ToUniversalTime())).ToList();
    if (newItems is not null)
        items.AddRange(newItems);
    items.OrderByDescending(c => c.PublishDate).Distinct().WriteCSV("Data\\feed.csv");
    var NTFY_CHANNEL = Environment.GetEnvironmentVariable("NTFY_CHANNEL");
    if (count < items.Count)
    {
        if (!string.IsNullOrEmpty(NTFY_CHANNEL))
            await httpClient.PostAsync(NTFY_CHANNEL, new StringContent("New Announcement from Tax Department", Encoding.UTF8, "application/x-www-form-urlencoded"));
    }
}
catch (Exception ex)
{
    File.AppendAllLines($"Data\\Exceptions.log", [ex.Message]);
}
