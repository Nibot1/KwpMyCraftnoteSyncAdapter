using System;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics;

namespace KwpMyCraftnoteProjectSyncAdapter
{
    class UpdateHandler
    {
        public static string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KwpMyCraftnoteSyncAdapter\\update.json");
        public static string filePathExe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KwpMyCraftnoteSyncAdapter\\update.exe");
        public static void CheckUpdate(Version version)
        {
            try
            {
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KwpMyCraftnoteSyncAdapter\\"));

                Console.WriteLine("Prüfe auf aktualisierung");
                WebClient myWebClient = new WebClient();
                myWebClient.DownloadFile("https://server01.nibot.me/kwpcraftnote/update.json", filePath);

                if (File.Exists(filePath) && new FileInfo(filePath).Length != 0)
                {
                    /////////////////////////////////////////////////////////////////////////////
                    /// Open a StreamReader to read the filecontent an parse it to a JObject ///
                    ///////////////////////////////////////////////////////////////////////////
                    using (StreamReader r = new StreamReader(filePath))
                    {
                        string json = r.ReadToEnd();
                        JObject filecontent = JObject.Parse(json);
                        LogfileHandler.Log("update.json content: " + filecontent.ToString());
                        Version newVersion = Version.Parse(filecontent["version"].ToString());
                        if (version < newVersion)
                        {
                            Console.WriteLine("Update avilable");
                            myWebClient.DownloadFile("https://server01.nibot.me/kwpcraftnote/update.exe", filePathExe);
                            if (File.Exists(filePathExe) && new FileInfo(filePathExe).Length != 0)
                            {
                                Process.Start(Path.Combine(filePathExe+ " /SILENT"));
                                LogfileHandler.Log("Update started");
                                System.Environment.Exit(0);
                            }
                        }
                        else
                        {
                            LogfileHandler.Log("Version is not Higher");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                LogfileHandler.Log("Update Error: " + e.ToString());
                MessageBox.Show("Bitte lassen sie Dieses Fenster Geöffnet und informieren sie ihren Systemadministrator \n\n\n" + e.ToString(), "KWP -> MyCraftnote Projekt Synchronisation             Schwerwiegender Fehler!!! ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Threading.Thread.Sleep(5000);
                System.Environment.Exit(1);
            }


        }
    }
}
