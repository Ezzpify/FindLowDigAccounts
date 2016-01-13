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
            public int threadCount { get; set; }
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
    }
}
