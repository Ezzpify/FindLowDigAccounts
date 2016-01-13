using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;

namespace LowDig
{
    class Log
    {
        /// <summary>
        /// Private log variables
        /// </summary>
        private string          logPath;
        private int             logQueueSize;
        private List<string>    logQueue;


        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="LogPath">Path to log file</param>
        public Log(string LogPath, int QueueSize)
        {
            /*Initialize log variables*/
            logQueue = new List<string>();
            logPath = LogPath;
            logQueueSize = QueueSize;

            /*Create log file if it doesn't exist already*/
            if (!File.Exists(logPath))
            {
                File.Create(logPath).Close();
                Console.WriteLine("Log file created");
                Thread.Sleep(250);
            }
        }


        /// <summary>
        /// Write an account to queue to be flushed and written to file later
        /// </summary>
        /// <param name="account">Account to log</param>
        public void Write(Config.Account account)
        {
            /*Write notice*/
            Console.WriteLine("Found account: {0}", account.username);

            /*Add account to queue*/
            string logMessage = string.Format("{0}\n{1}\n", account.username, account.email);
            logQueue.Add(logMessage);
            FlushLog();
        }


        /// <summary>
        /// Flushed the log queue to file
        /// </summary>
        /// <param name="ignoreCondition">If true, flush regardless of queue sie</param>
        public void FlushLog(bool ignoreCondition = false)
        {
            /*If we don't have enough entries to flush, return*/
            if (ignoreCondition || logQueue.Count < logQueueSize)
                return;

            /*Log entries*/
            lock (logQueue)
            {
                using (FileStream fs = File.Open(logPath, FileMode.Append, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        foreach (var str in logQueue)
                        {
                            sw.WriteLine(str);
                        }

                        logQueue.Clear();
                    }
                }
            }
        }
    }
}
