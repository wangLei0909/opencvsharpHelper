using NLog;
using System;
using System.IO;

namespace ModuleCore.Services
{
    public class NLogService
    {
        private static readonly object o = new object();
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void Error(string msg)
        {
            WriteLog(msg, LogLevel.Error);
        }

        public static void Info(string msg)
        {
            WriteLog(msg, LogLevel.Info);
        }

        public static void Debug(string msg)
        {
            WriteLog(msg, LogLevel.Debug);
        }

        private static void WriteLog(string msg, LogLevel level)
        {
            lock (o)
                try
                {
                    if (LogLevel.Debug == level)
                    {
                        logger.Debug(msg);
                    }
                    else if (LogLevel.Error == level)
                    {
                        logger.Error(msg);
                    }
                    else if (LogLevel.Fatal == level)
                    {
                        logger.Fatal(msg);
                    }
                    else if (LogLevel.Info == level)
                    {
                        logger.Info(msg);
                    }
                    else if (LogLevel.Trace == level)
                    {
                        logger.Trace(msg);
                    }
                    else if (LogLevel.Warn == level)
                    {
                        logger.Warn(msg);
                    }
                    else
                    {
                        logger.Info(msg);
                    }
                }
                catch (System.Exception e)
                {
                    try
                    {
                        string strPath = Directory.GetCurrentDirectory();
                        using FileStream fs = File.Open(strPath + @"\Logs\fatal.txt", FileMode.OpenOrCreate);
                        using StreamWriter sw = new(fs);
                        sw.WriteLine(DateTime.Now.ToString() + ": Can not write the nlog, " + e.Message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
        }

        public static void Error(Exception ex)
        {
            Exception iex = GetInnerException(ex);
            Error(iex.ToString());
        }

        private static Exception GetInnerException(Exception ex)
        {
            if (ex.InnerException == null)
            {
                return ex;
            }
            return GetInnerException(ex.InnerException);
        }
    }
}