using System.Text.RegularExpressions;

namespace wowa;

public class MarkupSanitizer {
    public static string SanitizeMarkdown(string markdown) {
        var output = markdown;

        output = Regex.Replace(output, @"<[^>]*>", ""); // Remove HTML tags
        output = Regex.Replace(output, @"^[=-]{2,}\s*$", "", RegexOptions.Multiline); // Remove setext-style headers
        output = Regex.Replace(output, @"\\^.+?\?", ""); // Remove footnotes
        output = Regex.Replace(output, @"\s{0,2}\[.*?\]: .*?$", ""); // Remove reference-style links
        output = Regex.Replace(output, @"!\[(.*?)\][[(].*?[\])]", ""); // Remove images
        output = Regex.Replace(output, @"\[(.*?)\][[(].*?[\])]", "$1"); // Remove inline links
        output = Regex.Replace(output, @"^\s{0,3}>\s?", "", RegexOptions.Multiline); // Remove blockquotes
        output = Regex.Replace(output, @"^\s{1,2}\[(.*?)\]: (\S+)( "".*?"")?\s*$", "",
            RegexOptions.Multiline); // Remove reference-style links
        output = Regex.Replace(output, @"^(\n)?\s{0,}#{1,6}\s+| {0,}(\n)?\s{0,}#{0,} {0,}(\n)?\s{0,}$", "$1$2$3",
            RegexOptions.Multiline); // Remove atx-style headers
        output = Regex.Replace(output, @"([*_]{1,3})(\S.*?\S{0,1})\1", "$2"); // Remove emphasis
        output = Regex.Replace(output, @"(`{3,})(.*?)\1", "$2", RegexOptions.Singleline); // Remove code blocks
        output = Regex.Replace(output, @"`(.+?)`", "$1"); // Remove inline code
        output = Regex.Replace(output, @"\n{2,}", "\n\n"); // Replace two or more newlines with exactly two

        return output;
    }

    public static string SanitizeBbCode(string bbcode) {
        // Define patterns for BBCode elements to remove
        string[] patterns = {
            @"\[img.*?\].*?\[\/img\]", // Remove images
            @"\[url.*?\].*?\[\/url\]", // Remove links
            @"\[b\].*?\[\/b\]", // Remove bold text
            @"\[i\].*?\[\/i\]", // Remove italic text
            @"\[code\].*?\[\/code\]" // Remove code blocks
        };

        // Remove BBCode elements using regular expressions
        foreach (var pattern in patterns) {
            bbcode = Regex.Replace(bbcode, pattern, "");
        }

        return bbcode;
    }
}