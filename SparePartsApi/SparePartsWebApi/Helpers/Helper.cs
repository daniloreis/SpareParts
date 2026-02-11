using System;
using System.IO;

namespace SparePartsWebApi.Helpers
{
    public class Helper
    {
        public static void Logger(string msg)
        {
            File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "log.txt", DateTime.Now + ": " + msg + Environment.NewLine);
        }
    }
}