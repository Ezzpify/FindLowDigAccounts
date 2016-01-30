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
            Console.Title = "Steam Account Finder";
            Console.WriteLine("Steam Account Finder | https://github.com/matthewwc/SteamAccountFinder/");
            Console.WriteLine("Fork of https://github.com/Ezzpify/FindLowDigAccounts/");
            Console.WriteLine("");

            ///*Read settings*/
            mSettings = Settings.Read();

            /*Settings was not read, return*/
            if (mSettings == null)
                return;

            /*Start session*/
            mSession = new Session(mSettings);

            /*Keep us alive*/
            while (true)
            {
                /*Update title with information from current session*/
                if (mSession.mAccountsChecked > 0)
                {
                    double finishPercent = 100 * ((mSession.mCurrentId - mSettings.startId) / (double)(mSettings.endId - mSettings.startId));
                    double successPercent = 100 * (mSession.mAccountsFound / (double)mSession.mAccountsChecked);
                    finishPercent = Math.Round(finishPercent, 2);
                    successPercent = Math.Round(successPercent, 2);
                    Console.Title = string.Format("{1}/{2} ({3}%) finished, with a {0}/{1} ({4}%) success rate | Steam Account Finder", mSession.mAccountsFound, mSession.mAccountsChecked, mSettings.endId - mSettings.startId, finishPercent, successPercent);
                }
                Thread.Sleep(100);
            }
        }
    }
}
