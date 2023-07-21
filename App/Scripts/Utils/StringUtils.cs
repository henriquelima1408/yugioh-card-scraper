using System.Text.RegularExpressions;

namespace yugioh_card_scraper.Utils
{
    public static class StringUtils
    {
        public static string RemoveSpecialCharacters(this string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }

        public static IEnumerable<int> FindAllNumbers(this string str)
        {
            var pattern = @"\d+(\.\d+)?";
            var regex = new Regex(pattern);
            var matches = regex.Matches(str);
            return matches.Select(i => int.Parse(i.Value));
        }
    }
}
