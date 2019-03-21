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
        public static void Log(string msg)
        {
            if (!File.Exists("log.txt"))
            {
                File.WriteAllText("log.txt", string.Empty);
            }
            if (new System.IO.FileInfo("log.txt").Length / 1000000 >= 1000)
            {
                File.WriteAllText("log.txt", string.Empty);
            }
            System.IO.StreamWriter sw = System.IO.File.AppendText(
                "log.txt");
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
