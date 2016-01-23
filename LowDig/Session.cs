using System;
using System.Net;
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
            mSem = new Semaphore(mSettings.requestLimit, mSettings.requestLimit);

            /*Go through the steam accounts*/
            for (int i = mSettings.startId; i < mSettings.endId; i++)
            {
                mSem.WaitOne();
                QueryProfile(i);
            }

            /*Done checking*/
            Console.WriteLine("\n\nFinished");
            mLog.FlushLog(true);
        }


        /// <summary>
        /// Queries a steam profile async
        /// Passes response to ParseResponse()
        /// </summary>
        /// <param name="steamDigit">Steam Digit to query (ex 6000)</param>
        private void QueryProfile(int steamDigit)
        {
            try
            {
                /*Set up local vars for request*/
                string steamId = string.Format("STEAM_0:0:{0}", steamDigit);
                string steamId64 = Functions.ConvertToSteam64(steamId);
                string communityUrl = string.Format("http://steamcommunity.com/profiles/{0}", steamId64);

                /*Download page async*/
                Console.WriteLine("Checking {0} ...", steamId);
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadStringCompleted += (sender, e) => { ParseResponse(sender, e, steamId); };
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                    wc.DownloadStringAsync(new Uri(communityUrl));
                }
            }
            catch(WebException ex)
            {
                Console.WriteLine("WebException: {0}", ex.Message);
            }
        }


        /// <summary>
        /// Parses the response from query
        /// </summary>
        /// <param name="o">Sender</param>
        /// <param name="e">EventArgs</param>
        private void ParseResponse(object o, DownloadStringCompletedEventArgs e, string steamId)
        {
            try
            {
                string pageSource = e.Result;
                if (pageSource.Contains("This user has not yet set up their Steam Community profile."))
                {
                    /*Get the account name from page title*/
                    string accountName = Functions.GetStringBetween(pageSource, "<title>", "</title>")
                        .Replace("Steam Community :: ", "");

                    if (!string.IsNullOrEmpty(accountName) && accountName.Length > 5)
                    {
                        /*Format email and check if it looks okay*/
                        string accountEmail = string.Format("{0}@hotmail.com", accountName);
                        if (Functions.IsProperEmail(accountEmail))
                        {
                            /*Set up account and log it*/
                            var account = new Config.Account()
                            {
                                steamid  = steamId,
                                username = accountName,
                                email    = accountEmail
                            };
                            mLog.Write(account);
                            mAccountsFound++;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex.Message);
            }

            /*Release queue position*/
            mSem.Release();
        }
    }
}
