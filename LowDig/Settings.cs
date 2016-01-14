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
            string fileName = "Settings.json";
            if (File.Exists(fileName))
            {
                /*File exist, so we'll parse the information here*/
                string setJson = File.ReadAllText(fileName);
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
                        Console.WriteLine("Error parsing {0}\n{1}", fileName, jEx.ToString());
                        File.Delete(fileName);
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
                    requestLimit = 50
                };

                /*Write file*/
                string djson = JsonConvert.SerializeObject(settingsClass, Formatting.Indented);
                Console.WriteLine("{0} has been written. Edit the settings and launch the program again.", fileName);
                File.WriteAllText(fileName, djson);
                Thread.Sleep(1500);
            }
            
            return null;
        }
    }
}
