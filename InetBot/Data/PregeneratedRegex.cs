using System.Text.RegularExpressions;

namespace InetBot.Data
{
    public partial class PregeneratedRegex
    {
        [GeneratedRegex("[\\s]", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline)]
        private static partial Regex _Whitespaces();
        public static Regex Whitespaces { get { return _Whitespaces(); } }

        [GeneratedRegex("([0-9a-f]{8})", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline)]
        private static partial Regex _BootromShortCodeParser();
        public static Regex BootromShortCodeParser { get { return _BootromShortCodeParser(); } }

        [GeneratedRegex("([0-9a-f]{8})([0-9a-f]{8})([0-9a-f]{8})([0-9a-f]{8})([0-9a-f]{8})", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline)]
        private static partial Regex _BootromFullCodeParser();
        public static Regex BootromFullCodeParser { get { return _BootromFullCodeParser(); } }
    }
}
