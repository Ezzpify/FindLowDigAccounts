using System.Threading;
using Newtonsoft.Json;
using System.IO;
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
        /// Reads the settings
        /// </summary>
        /// <returns>Returns false if no file found</returns>
        private static bool ReadSettings()
        {
            string settingsPath = "Settings.json";
            if (!File.Exists(settingsPath))
            {
                /*File doesn't exist, we'll create it here*/
                var settingsClass = new Config.Settings()
                {
                    startId = 5000,
                    endId = 10000,
                    threadCount = 3
                };

                /*Write file*/
                string djson = JsonConvert.SerializeObject(settingsClass, Formatting.Indented);
                File.WriteAllText("Settings.json", djson);
                Console.WriteLine("Settings.json has been written. Go edit it.");
                Thread.Sleep(1500);
            }
            else
            {
                /*File exist, so we'll parse the information here*/
                string setJson = File.ReadAllText("Settings.json");
                if (!string.IsNullOrWhiteSpace(setJson))
                {
                    try
                    {
                        mSettings = JsonConvert.DeserializeObject<Config.Settings>(setJson);
                        return true;
                    }
                    catch (JsonException jEx)
                    {
                        /*Incorrect format probably*/
                        Console.WriteLine("Error parsing Settings.json");
                        Console.WriteLine(jEx.ToString());
                        Thread.Sleep(5000);
                    }
                }
            }


            return false;
        }


        /// <summary>
        /// Main functions
        /// Set settings and start session
        /// </summary>
        /// <param name="args">No args</param>
        static void Main(string[] args)
        {
            /*Read settings*/
            if (!ReadSettings())
                return;

            /*Start session*/
            mSession = new Session(mSettings);

            /*Keep us alive*/
            while (true)
            {
                Console.Title = string.Format("Zute | LogDig | Accounts found: {0}", mSession.mAccountsFound);
                Thread.Sleep(1000);
            }
        }
    }
}
