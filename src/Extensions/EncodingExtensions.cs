using System.Text;
using System.Text.RegularExpressions;

namespace FlowStorage;

public static partial class EncodingExtensions
{
    [GeneratedRegex(@"charset=(.+)", RegexOptions.IgnoreCase, "es-ES")]
    private static partial Regex MyRegex();

    public static Encoding GetFromContentType(string contentType)
    {
        if (string.IsNullOrEmpty(contentType))
        {
            return Encoding.UTF8;
        }

        var match = MyRegex().Match(contentType);

        if (match.Success)
        {
            try
            {
                return Encoding.GetEncoding(match.Groups[1].Value.Trim());
            }
            catch (ArgumentException)
            {
                return Encoding.UTF8;
            }
        }

        return Encoding.UTF8;
    }
}