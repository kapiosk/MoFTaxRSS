using System.Xml.Serialization;
namespace MoFTaxRSS;

[XmlRoot(ElementName = "link")]
public class Link
{

    [XmlAttribute(AttributeName = "href")]
    public string Href { get; set; } = string.Empty;

    [XmlAttribute(AttributeName = "rel")]
    public string Rel { get; set; } = string.Empty;

    [XmlAttribute(AttributeName = "type")]
    public string Type { get; set; } = string.Empty;
}
