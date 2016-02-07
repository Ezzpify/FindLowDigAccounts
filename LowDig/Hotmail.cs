using System;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace LowDig
{
    static class Hotmail
    {
        /// <summary>
        /// Request class
        /// This holds all the information nessesary to talk to hotmail
        /// </summary>
        private class request
        {
            public string signInName { get; set; }
            public string uaid { get; set; }
            public bool performDisambigCheck { get; set; } = true;
            public bool includeSuggestions { get; set; } = true;
            public int uiflvr { get; set; }
            public int scid { get; set; }
            public int hpgid { get; set; }
        }


        /// <summary>
        /// Private variables
        /// </summary>
        private static CookieContainer mCookies = new CookieContainer();
        private static request mRequest = new request();
        private static string mApiCanary;


        /// <summary>
        /// Gets the session from hotmail
        /// </summary>
        public static void GetSession()
        {
            /*Start the request*/
            var webAdd = "https://signup.live.com/signup.aspx?lic=1";
            var webReq = (HttpWebRequest)WebRequest.Create(webAdd);

            /*Add nessesary information to the request so it will be accepted as a legit request*/
            webReq.ContentType = "application/json; charset=utf-8";
            webReq.Method = "GET";
            webReq.Host = "signup.live.com";
            webReq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            webReq.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36";
            webReq.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            webReq.KeepAlive = true;
            webReq.CookieContainer = mCookies;

            /*Read the response*/
            var httpResponse = (HttpWebResponse)webReq.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                /*Scrape all the session details we need to make dirty api requests*/
                var result = streamReader.ReadToEnd();
                mRequest.uaid = Functions.GetStringBetween(result, "\"uaid\":\"", "\",");
                mRequest.uiflvr = Convert.ToInt32(Functions.GetStringBetween(result, "\"uiflvr\":", ","));
                mRequest.scid = Convert.ToInt32(Functions.GetStringBetween(result, "\"scid\":", ","));
                mRequest.hpgid = Convert.ToInt32(Functions.GetStringBetween(result, "\"hpgid\":", ","));
                mApiCanary = Regex.Unescape(Functions.GetStringBetween(result, "\"apiCanary\":\"", "\","));
                Console.WriteLine("Got Hotmail session");
            }
        }


        /// <summary>
        /// Searches if the 
        /// </summary>
        /// <param name="address">hotmail address to check</param>
        /// <returns>Returns true if available</returns>
        public static bool IsAvailable(string address)
        {
            try
            {
                /*Start the request*/
                Config.HotmailResponse resp = null;
                int attempts = 0;

                do
                {
                    var webAdd = string.Format("https://signup.live.com/API/CheckAvailableSigninNames?uaid={0}&lic=1", mRequest.uaid);
                    var webReq = (HttpWebRequest)WebRequest.Create(webAdd);

                    /*Add nessesary information to the request so it will be accepted as a legit request*/
                    webReq.ContentType = "application/json; charset=utf-8";
                    webReq.Method = "POST";
                    webReq.Host = "signup.live.com";
                    webReq.Accept = "application/json";
                    webReq.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36";
                    webReq.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    webReq.KeepAlive = true;
                    webReq.CookieContainer = mCookies;
                    webReq.Headers["canary"] = mApiCanary;
                    webReq.Timeout = 10000;

                    /*We'll write the deserialized request class here*/
                    using (var streamWriter = new StreamWriter(webReq.GetRequestStream()))
                    {
                        /*Set up new request class with email*/
                        var req = new request();
                        req = mRequest;
                        req.signInName = address;

                        /*Write the values*/
                        string json = JsonConvert.SerializeObject(req);
                        streamWriter.Write(json);
                        streamWriter.Flush();
                    }

                    /*Parse the json response we receive here*/
                    var httpResponse = (HttpWebResponse)webReq.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        /*Deserialize the response and return if the email is available or not*/
                        var result = streamReader.ReadToEnd();
                        resp = JsonConvert.DeserializeObject<Config.HotmailResponse>(result);
                        if (resp.error != null)
                        {
                            /*Need to refresh our session*/
                            GetSession();
                            attempts++;
                            resp = null;
                        }
                        else
                        {
                            /*Set the new api string and return if account is free*/
                            mApiCanary = resp.apiCanary;
                        }
                    }
                }
                while (resp == null && attempts <= 3);
                return resp.isAvailable;
            }
            catch
            {
                return false;
            }
        }
    }
}
