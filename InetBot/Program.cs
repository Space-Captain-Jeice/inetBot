using System.Globalization;
using System.Threading;

[AttributeUsage(AttributeTargets.Assembly)]
internal class BuildDateAttribute : Attribute
{
    public BuildDateAttribute(string value)
    {
        DateTime = DateTime.ParseExact(
            value,
            "yyyyMMddHHmmss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None);
    }

    public DateTime DateTime { get; }
}

namespace InetBot
{
    class Program
    {
        public static void Main(string[] args)
        {
            Thread thread = new Thread(new ThreadStart(startBot));
            thread.Start();
        }

        private static void startBot()
        {
            var bot = new MainClass();
            bot.Run().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
