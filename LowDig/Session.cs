using System;
using System.Threading;
using System.Threading.Tasks;

namespace LowDig
{
    class Session
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private Config.Settings mSettings;
        private Thread mSearchThread;
        private Semaphore mSem;
        private Log mLog;


        /// <summary>
        /// Public variables
        /// </summary>
        public int mAccountsFound;


        /// <summary>
        /// Class constructor
        /// </summary>
        public Session(Config.Settings settings)
        {
            /*Initialize globals*/
            mSettings = settings;
            mLog = new Log("accounts.txt", 3);

            /*Start our main thread*/
            mSearchThread = new Thread(Work);
            mSearchThread.Start();
        }


        /// <summary>
        /// Search thread function
        /// This will search for accounts using three seperate threads
        /// </summary>
        private void Work()
        {
            /*Initialize our semapore with the settings that was passed*/
            mSem = new Semaphore(mSettings.threadCount, mSettings.threadCount);

            /*Go through the steam accounts*/
            for (int i = mSettings.startId; i < mSettings.endId; i++)
            {
                mSem.WaitOne();
                Search(i);
            }

            /*Done checking*/
            Console.WriteLine("\n\nFinished");
            mLog.FlushLog(true);
        }


        /// <summary>
        /// Main search event
        /// Searches steam api and checks web response
        /// </summary>
        private void Search(int iDigit)
        {
            Task.Run(() =>
            {
                /*Account holder*/
                var account = new Config.Account();

                /*Get steamId64 and request the community page*/
                string steamId = string.Format("STEAM_0:0:{0}", iDigit);
                Console.WriteLine(string.Format("Checking {0} ...", steamId));

                /*Download community page*/
                string communityString = Website.DownloadString(
                    string.Format("http://steamcommunity.com/profiles/{0}",
                    Functions.ConvertToSteam64(steamId)));

                /*If this account has not been set up*/
                if (communityString.Contains("This user has not yet set up their Steam Community profile."))
                {
                    /*Get account name from page*/
                    string accountName = Functions.GetStringBetween(communityString, "<title>", "</title>");
                    accountName = accountName.Replace("Steam Community :: ", "");

                    /*String get guessed hotmail*/
                    if (!string.IsNullOrEmpty(accountName))
                    {
                        /*Check if email looks okay*/
                        string email = string.Format("{0}@hotmail.com", accountName);
                        if (Functions.IsProperEmail(email))
                        {
                            /*Set up account class*/
                            account.steamid = steamId;
                            account.username = accountName;
                            account.email = email;

                            /*Log account*/
                            mLog.Write(account);
                            mAccountsFound++;
                        }
                    }
                }

                /*Release our queue*/
                mSem.Release();
            });
        }
    }
}
