using System.Xml.Serialization;
namespace MoFTaxRSS;

[XmlRoot(ElementName = "rss")]
public class Rss
{

    [XmlElement(ElementName = "channel")]
    public Channel Channel { get; set; } = new();

    [XmlAttribute(AttributeName = "version")]
    public double Version { get; set; }

    [XmlAttribute(AttributeName = "atom")]
    public string Atom { get; set; } = string.Empty;

    [XmlText]
    public string Text { get; set; } = string.Empty;
}
