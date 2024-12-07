using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ConsoleApp_HighLander
{
    internal class Logger
    {
        private static string logFile = "log.txt";

        public static void Log(string message)
        {
            File.AppendAllText(logFile, message + Environment.NewLine);
        }
    }
}
