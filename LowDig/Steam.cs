using System;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace LowDig
{
    static class Steam
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private static CookieContainer mCookies = new CookieContainer();


        /// <summary>
        /// Gets the steam session
        /// </summary>
        public static string GetSession(Config.Proxy proxy)
        {
            /*Start the request*/
            var webAdd = "https://help.steampowered.com";
            var webReq = (HttpWebRequest)WebRequest.Create(webAdd);

            /*Add nessesary information to the request so it will be accepted as a legit request*/
            webReq.ContentType = "application/json; charset=utf-8";
            webReq.Method = "GET";
            webReq.Host = "help.steampowered.com";
            webReq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            webReq.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36";
            webReq.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            webReq.KeepAlive = true;
            webReq.CookieContainer = mCookies;
            webReq.Proxy = new WebProxy(proxy.host, proxy.port);

            /*Read the response*/
            var httpResponse = (HttpWebResponse)webReq.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                /*Scrape all the session details we need to make dirty api requests*/
                var result = streamReader.ReadToEnd();
                return Functions.GetStringBetween(result, "g_sessionID = \"", "\";");
            }
        }


        /// <summary>
        /// If a steam account is available
        /// </summary>
        /// <param name="address">email to check</param>
        /// <returns>0 = unavailable, 1 = need captcha, 2 = success, 3 = timeout, 4 = error</returns>
        public static int IsAvailable(string address, Config.Proxy proxy)
        {
            try
            {
                /*Start the request*/
                var webAdd = string.Format("https://help.steampowered.com/wizard/AjaxLoginInfoSearch?nav=&search={0}&captchagid=&captcha_text=&sessionid={1}", address, proxy.steamSessionId);
                var webReq = (HttpWebRequest)WebRequest.Create(webAdd);

                /*Add nessesary information to the request so it will be accepted as a legit request*/
                webReq.ContentType = "application/json; charset=utf-8";
                webReq.Method = "GET";
                webReq.Host = "help.steampowered.com";
                webReq.Accept = "*/*";
                webReq.Referer = "https://help.steampowered.com/";
                webReq.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36";
                webReq.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                webReq.KeepAlive = true;
                webReq.CookieContainer = mCookies;
                webReq.Timeout = 10000;
                webReq.Proxy = new WebProxy(proxy.host, proxy.port);

                /*Parse the json response we receive here*/
                var httpResponse = (HttpWebResponse)webReq.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    /*Deserialize the response and return if the email is available or not*/
                    var result = streamReader.ReadToEnd();
                    var resp = JsonConvert.DeserializeObject<Config.SteamResponse>(result);

                    /*We need captcha, so we'll keep the account in queue and try with a different ip*/
                    if (resp.needCaptcha) { return 1; }

                    /*No error, that means the account is available*/
                    if (string.IsNullOrWhiteSpace(resp.errorMsg)) { return 2; }

                    /*Account not available*/
                    return 0;
                }
            }
            catch(WebException e)
            {
                if (e.Status == WebExceptionStatus.Timeout)
                {
                    return 3;
                }
                
                return 4;
            }
        }
    }
}
