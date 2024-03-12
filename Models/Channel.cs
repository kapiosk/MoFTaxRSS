using System.Xml.Serialization;
namespace MoFTaxRSS;

[XmlRoot(ElementName = "channel")]
public class Channel
{

    [XmlElement(ElementName = "link")]
    public List<Link> Link { get; set; } = [];

    [XmlElement(ElementName = "title")]
    public string Title { get; set; } = string.Empty;

    [XmlElement(ElementName = "description")]
    public string Description { get; set; } = string.Empty;

    [XmlElement(ElementName = "item")]
    public List<Item> Item { get; set; } = [];
}
