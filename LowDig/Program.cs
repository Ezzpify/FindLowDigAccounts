using System.Threading;
using System;

namespace LowDig
{
    class Program
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private static Session mSession;
        private static Config.Settings mSettings;


        /// <summary>
        /// Main functions
        /// Set settings and start session
        /// </summary>
        /// <param name="args">No args</param>
        static void Main(string[] args)
        {
            /*Read settings*/
            mSettings = Settings.Read();
            if (mSettings == null)
                return;

            /*Start session*/
            mSession = new Session(mSettings);

            /*Keep us alive*/
            while (true)
            {
                /*Update title with number of account found from session*/
                Console.Title = string.Format("Zute | LowDig - fork by tarcseh#3400 | Accounts found: {0}", mSession.mAccountsFound);
                Thread.Sleep(100);
            }
        }
    }
}
