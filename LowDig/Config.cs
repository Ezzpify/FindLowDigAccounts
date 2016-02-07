namespace LowDig
{
    public class Config
    {
        /// <summary>
        /// Holds settings for application
        /// </summary>
        public class Settings
        {
            public int startId { get; set; }
            public int endId { get; set; }
            public int requestLimit { get; set; }
            public bool checkHotmail { get; set; }
        }


        /// <summary>
        /// Holds information about a steam account
        /// </summary>
        public class Account
        {
            public string username { get; set; }
            public string email { get; set; }
            public string steamid { get; set; }
        }


        /// <summary>
        /// Hotmail error class
        /// </summary>
        public class Error
        {
            public string code { get; set; }
            public string data { get; set; }
            public bool showError { get; set; }
            public string stackTrace { get; set; }
        }


        /// <summary>
        /// Hotmail reponse class
        /// </summary>
        public class HotmailResponse
        {
            public Error error { get; set; }
            public string apiCanary { get; set; }
            public bool isAvailable { get; set; }
            public string type { get; set; }
        }


        /// <summary>
        /// Steam response class
        /// </summary>
        public class SteamResponse
        {
            public bool needCaptcha { get; set; } = true;
            public string hash { get; set; }
            public string errorMsg { get; set; }
        }


        /// <summary>
        /// Class for holding proxies and how many uses they've had
        /// </summary>
        public class Proxy
        {
            public string host { get; set; }
            public int port { get; set; }
            public int uses { get; set; }
            public string steamSessionId { get; set; }
        }
    }
}
