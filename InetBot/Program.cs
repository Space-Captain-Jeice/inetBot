using System.Threading;

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
