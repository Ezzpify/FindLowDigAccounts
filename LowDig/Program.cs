using System.Threading;

namespace LowDig
{
    class Program
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private static Session session;
        private static Config.Settings settings;


        /// <summary>
        /// Main functions
        /// Set settings and start session
        /// </summary>
        /// <param name="args">No args</param>
        static void Main(string[] args)
        {
            /*Set settings - Change here*/
            settings = new Config.Settings()
            {
                startId     = 5000,
                endId       = 10000,
                threadCount = 5
            };

            /*Start session*/
            session = new Session(settings);

            /*Keep us alive*/
            while (true) { Thread.Sleep(500); }
        }
    }
}
