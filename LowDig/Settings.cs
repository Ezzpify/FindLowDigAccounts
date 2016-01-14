using System;
using System.Threading;
using System.IO;
using Newtonsoft.Json;

namespace LowDig
{
    static class Settings
    {
        /// <summary>
        /// Read or write settings for application
        /// </summary>
        /// <returns>Returns null if settings was not read</returns>
        public static Config.Settings Read()
        {
            if (File.Exists("Settings.json"))
            {
                /*File exist, so we'll parse the information here*/
                string setJson = File.ReadAllText("Settings.json");
                if (!string.IsNullOrWhiteSpace(setJson))
                {
                    try
                    {
                        /*Return deserialized json string*/
                        return JsonConvert.DeserializeObject<Config.Settings>(setJson);
                    }
                    catch (JsonException jEx)
                    {
                        /*Incorrect format probably*/
                        Console.WriteLine("Error parsing Settings.json\n{0}", jEx.ToString());
                        Thread.Sleep(1500);
                    }
                }
            }
            else
            {
                /*File doesn't exist, we'll create it here*/
                var settingsClass = new Config.Settings()
                {
                    /*Temp values*/
                    startId = 5000,
                    endId = 10000,
                    threadCount = 5
                };

                /*Write file*/
                string djson = JsonConvert.SerializeObject(settingsClass, Formatting.Indented);
                Console.WriteLine("Settings.json has been written. Edit the settings and launch the program again.");
                File.WriteAllText("Settings.json", djson);
                Thread.Sleep(1500);
            }
            
            return null;
        }
    }
}
