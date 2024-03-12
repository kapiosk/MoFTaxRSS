using System.Xml.Serialization;
namespace MoFTaxRSS;

[XmlRoot(ElementName = "item")]
public class Item
{

    [XmlElement(ElementName = "title")]
    public string Title { get; set; } = string.Empty;

    [XmlElement(ElementName = "guid")]
    public Guid Guid { get; set; } = new();

    [XmlElement(ElementName = "pubDate")]
    public string PubDate { get; set; } = string.Empty;

    [XmlElement(ElementName = "link")]
    public string Link { get; set; } = string.Empty;
}
