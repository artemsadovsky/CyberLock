using System;
using System.Windows;
using System.Windows.Threading;
using System.IO;

namespace SunRise.CyberLock.Common.Library.Logger
{
    public class AppLogger
    {
        private readonly String _logPath;
        private readonly int _logLevel; // None = 0, Error = 1, Info = 2, All = 3
        private readonly object _syncLock = new object();

        public AppLogger(LogLevel logLevel, String logDir, String logFile)
        {
            this._logPath = logDir + @"\" + logFile;
            this._logLevel = (int)logLevel;

            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            if (!File.Exists(this._logPath))
            {
                var fileSteam = File.Create(this._logPath);
                using (StreamWriter writer = new StreamWriter(fileSteam))
                {
                    writer.Close();
                }
            }
            Debug("Logger initialized");
        }

        private void Write(String message)
        {
            lock (_syncLock)
            {
                message.WriteLog(_logPath);
            }
        }

        #region ErrorLogger
        public void Error(String log)
        {
            if (_logLevel > 0)
                Write(String.Format("{0} [ERROR] : {1}", DateTime.Now, log));
        }
        public void Error(String method, String action)
        {
            if (_logLevel > 0)
                Write(String.Format("{0} [ERROR] : {1} - {2}", DateTime.Now, method, action));
        }
        #endregion

        #region InfoLogger
        public void Info(String log)
        {
            if (_logLevel > 1)
                Write(String.Format("{0} [INFO] : {1}", DateTime.Now, log));
        }
        public void Info(String method, String action)
        {
            if (_logLevel > 1)
                Write(String.Format("{0} [INFO] : {1} - {2}", DateTime.Now, method, action));
        }
        #endregion
        
        #region DebugLogger
        public void Debug(String log)
        {
            if (_logLevel > 2)
                Write(String.Format("{0} [DEBUG] : {1}", DateTime.Now, log));
        }
        public void Debug(String method, String action)
        {
            if (_logLevel > 2)
                Write(String.Format("{0} [DEBUG] : {1} - {2}", DateTime.Now, method, action));
        }
        #endregion
    }

    public static class Logger
    {
        public static Boolean WriteLog(this String str, String fileName)
        {
            try
            {
                var sw = File.AppendText(fileName);
                sw.WriteLine(str);
                sw.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
