using System;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LowDig
{
    class Session
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private List<Config.Account> mAccountQueue = new List<Config.Account>();
        private List<Config.Proxy> mProxies = new List<Config.Proxy>();
        private static Random mRandom = new Random();
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
            mLog = new Log("accounts.txt", 1);
            //LoadProxies(); not needed for hotmail only

            /*Start our main thread*/
            mSearchThread = new Thread(Work);
            mSearchThread.Start();
        }


        /// <summary>
        /// Loads all proxies from proxies.txt
        /// </summary>
        private void LoadProxies()
        {
            string content = File.ReadAllText("proxies.txt");
            using (StringReader reader = new StringReader(content))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Length < 4 || !line.Contains(":"))
                        continue;

                    string[] split = line.Split(':');
                    mProxies.Add(new Config.Proxy()
                    {
                        host = split[0],
                        port = Convert.ToInt32(split[1]),
                        uses = 2 /*We get 2 tries per proxy*/
                    });
                }
            }
        }


        /// <summary>
        /// Search thread function
        /// This will search for accounts using three seperate threads
        /// </summary>
        private void Work()
        {
            /*Initialize our semapore with the settings that was passed*/
            mSem = new Semaphore(mSettings.requestLimit, mSettings.requestLimit);
            Hotmail.GetSession();

            /*Go through the steam accounts*/
            for (int i = mSettings.startId; i < mSettings.endId; i++)
            {
                mSem.WaitOne();
                QueryProfile(0, i);
                QueryProfile(1, i);
            }

            /*Done checking*/
            Console.WriteLine("\n\nFinished");
            mLog.FlushLog(true);
        }


        /// <summary>
        /// Queries a steam profile async
        /// Passes response to ParseResponse()
        /// </summary>
        /// <param name="server">Steam account server (0 or 1)</param>
        /// <param name="steamDigit">Steam Digit to query (ex 6000)</param>
        private void QueryProfile(int server, int steamDigit)
        {
            try
            {
                /*Set up local vars for request*/
                string steamId = string.Format("STEAM_0:{0}:{1}", server, steamDigit);
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
        /// Checks if an e-mail address is okay
        /// </summary>
        /// <param name="address">address to check</param>
        /// <returns></returns>
        private bool IsEmailGood(string address)
        {
            if (!Functions.IsProperEmail(address))
                return false;

            if (mSettings.checkHotmail && Hotmail.IsAvailable(address))
                return true;

            return false;
        }


        /// <summary>
        /// Parses the response from query
        /// </summary>
        /// <param name="o">Sender</param>
        /// <param name="e">EventArgs - we get page source from result</param>
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

                        /*If the email is valid and not taken*/
                        if (IsEmailGood(accountEmail))
                        {
                            /*Set up the account*/
                            var account = new Config.Account()
                            {
                                steamid = steamId,
                                username = accountName,
                                email = accountEmail
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
