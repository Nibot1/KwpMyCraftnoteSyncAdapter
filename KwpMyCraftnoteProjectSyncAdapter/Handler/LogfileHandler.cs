using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KwpMyCraftnoteProjectSyncAdapter
{
    class LogfileHandler
    {
        public static string logfilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KwpMyCraftnoteSyncAdapter\\log.txt");
        public static void Log(string msg)
        {
            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KwpMyCraftnoteSyncAdapter\\"));
            if (!File.Exists(logfilePath))
            {
                File.WriteAllText(logfilePath, string.Empty);
            }
            if (new FileInfo(logfilePath).Length / 1000000 >= 1000)
            {
                File.WriteAllText(logfilePath, string.Empty);
            }
            StreamWriter sw = File.AppendText(
                logfilePath);
            try
            {
                string logLine = System.String.Format(
                    "{0:G}: {1}.", System.DateTime.Now, msg);
                sw.WriteLine(logLine);
            }
            finally
            {
                sw.Close();
            }

        }
    }
}
