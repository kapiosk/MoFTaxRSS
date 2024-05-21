using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CsvHelper;
using CsvHelper.Configuration;

namespace MoFTaxRSS;

public static class Utils
{
    public static List<T> ReadFromCSV<T>(this string path)
    {
        List<T> records = [];
        using var reader = new StreamReader(path, true);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.CurrentCulture));
        csv.Read();
        csv.ReadHeader();
        while (csv.Read())
        {
            records.Add(csv.GetRecord<T>());
        }
        return [.. records];
    }

    public static void WriteCSV<T>(this IEnumerable<T> records, string path)
    {
        using var stream = File.Open(path, FileMode.CreateNew);
        using var writer = new StreamWriter(stream);
        using var csv = new CsvWriter(writer, CultureInfo.CurrentCulture);
        csv.WriteRecords(records);
    }

    public static T? DeserializeFromURL<T>(this string value) where T : new()
    {
        using var reader = XmlReader.Create(value);
        var xmlSerializer = new XmlSerializer(typeof(T));
        return (T?)xmlSerializer.Deserialize(reader);
    }

    public static T? DeserializeFromXML<T>(this Stream stream) where T : new()
    {
        var xmlSerializer = new XmlSerializer(typeof(T));
        return (T?)xmlSerializer.Deserialize(stream);
    }

    public static T? DeserializeFromXML<T>(this string value) where T : new()
    {
        using var stringReader = new StringReader(value);
        var xmlSerializer = new XmlSerializer(typeof(T));
        return (T?)xmlSerializer.Deserialize(stringReader);
    }

    private static readonly XmlSerializerNamespaces ns = new([XmlQualifiedName.Empty]);

    private static readonly XmlWriterSettings settings = new()
    {
        Indent = true,
        Encoding = Encoding.UTF8,
        IndentChars = "\t",
    };

    public static byte[] SerializeToByteArray<T>(this T? toSerialize) where T : new()
    {
        toSerialize ??= new();
        XmlSerializer xmlSerializer = new(toSerialize.GetType());
        using MemoryStream stream = new();
        using XmlWriter writer = XmlWriter.Create(stream, settings);
        xmlSerializer.Serialize(writer, toSerialize, ns);
        return stream.ToArray();
    }

    public static string SerializeToXML<T>(this T? toSerialize) where T : new()
    {
        return Encoding.UTF8.GetString(toSerialize.SerializeToByteArray());
    }

    public static ByteArrayContent ToByteArrayContent<T>(this T? toSerialize) where T : new()
    {
        ByteArrayContent content = new(toSerialize.SerializeToByteArray());
        content.Headers.ContentType = new("application/xml");
        return content;
    }
}
