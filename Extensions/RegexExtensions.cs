using System.Text.RegularExpressions;

namespace MoFTaxRSS;

internal partial class RegexExtensions
{
    [GeneratedRegex(@"href=""(.*?\.pdf)""")]
    internal static partial Regex PDFRegex();
}
