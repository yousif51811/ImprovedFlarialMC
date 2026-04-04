using System;
using System.Diagnostics;

namespace Flarial.Services
{
    public static class Logging
    {
        /// <summary>
        /// Path of current log file. Logs are stored in "logs" folder with a timestamped name.
        /// Each log entry is prefixed with a timestamp.
        /// </summary>
        private readonly static string logFilePath = System.IO.Path.Combine("logs", $"log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");

        /// <summary>
        /// Static constructor to ensure the logs directory exists before any logging occurs 
        /// and log essential information.
        /// </summary>
        static Logging()
        {
            // Ensure the logs directory exists and create a new log file for the session
            if (!System.IO.Directory.Exists("logs"))
                System.IO.Directory.CreateDirectory("logs");
            if (!System.IO.File.Exists(logFilePath))
                System.IO.File.Create(logFilePath).Dispose();

            // Log the start of a new session
            Log("New Session Started", "START");
            string Version = FileVersionInfo
                            .GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location)
                            .FileVersion;
            // Log version
            Log($"Running FlarialReskin Version: {Version}", "INFO");
        }

        public static void Log(string message, string level)
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level.ToUpper()}] {message}";
            System.IO.File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
        }
    }
}
