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
        public static void CheckUpdate(Version version)
        {
            try
            {
                Console.WriteLine("Prüfe auf aktualisierung");
                WebClient myWebClient = new WebClient();
                myWebClient.DownloadFile("https://server01.nibot.me/kwpcarftnote/update.json", "update.json");

                if (File.Exists("update.json") && new FileInfo("update.json").Length != 0)
                {
                    /////////////////////////////////////////////////////////////////////////////
                    /// Open a StreamReader to read the filecontent an parse it to a JObject ///
                    ///////////////////////////////////////////////////////////////////////////
                    using (StreamReader r = new StreamReader("update.json"))
                    {
                        string json = r.ReadToEnd();
                        JObject filecontent = JObject.Parse(json);
                        LogfileHandler.Log("update.json content: " + filecontent.ToString());
                        Version newVersion = Version.Parse(filecontent["version"].ToString());
                        if (version < newVersion)
                        {
                            Console.WriteLine("Update avilable");
                            myWebClient.DownloadFile("https://server01.nibot.me/kwpcarftnote/update.exe", "update.exe");
                            if (File.Exists("update.exe") && new FileInfo("update.exe").Length != 0)
                            {
                                Process.Start("update.exe");
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
