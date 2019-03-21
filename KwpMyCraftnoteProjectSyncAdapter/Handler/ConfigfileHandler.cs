using System;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;

namespace KwpMyCraftnoteProjectSyncAdapter
{
    class ConfigfileHandler
    {
        private static string host = "localhost";
        private static string username = "sa";
        private static string password = "kwpsarix";
        private static string database = "BNWINS";
        private static string instance = "kwp";
        private static string lastSync = "";
        private static int limit = 10;
        private static double interval = 10.0;
        private static string current = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff");
        private static string apikey = "";
        private static string keyword = "craftnote";
        public static void UpdateConfigfile(String fileName, JObject config, String currentTime)
        {
            try
            {
                ////////////////////////////////////
                /// Read all values from JObject ///
                /// and update all values        ///
                ////////////////////////////////////
                host = (string)config["host"];
                username = (string)config["username"];
                password = (string)config["password"];
                database = (string)config["database"];
                database = (string)config["database"];
                instance = (string)config["instance"];
                lastSync = (string)config["last_sync"];
                limit = (int)config["limit"];
                interval = (double)config["interval"];
                apikey = (string)config["apikey"];
                keyword = (string)config["keyword"];
                current = currentTime;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while processing the config file. Using fallback values. Please check the Configfile for any issues");
                Console.WriteLine(e.ToString());
                LogfileHandler.Log("Error in Configfile: " + e.ToString());
            }
            ////////////////////////////////////////////////////////////////////////////////
            /// Rewriting Configfile to avoid malconfig and to update lastsync timestamp ///
            ////////////////////////////////////////////////////////////////////////////////
            File.WriteAllText(fileName, string.Empty);
            ///////////////////////////////////////////////////
            /// New JObject with updated Value for lastsync ///
            ///////////////////////////////////////////////////
            JObject json = new JObject(new JProperty("host", host), new JProperty("username", username), new JProperty("password", password), new JProperty("database", database), new JProperty("instance", instance), new JProperty("last_sync", current), new JProperty("limit", limit), new JProperty("apikey", apikey), new JProperty("interval", interval), new JProperty("keyword", keyword));
            ////////////////////////////////////////////////////
            /// Opening Filestream and write JObject to File ///
            ////////////////////////////////////////////////////
            using (FileStream fs = File.Create(fileName))
            {
                //////////////////////////////
                /// JObject to Byte array ///
                ////////////////////////////
                Byte[] contents = new UTF8Encoding(true).GetBytes(json.ToString());

                ///////////////////////////////////
                /// Writing Byte array to File ///
                /////////////////////////////////
                fs.Write(contents, 0, contents.Length);
                Console.WriteLine("Configfile successfuly updated");
                LogfileHandler.Log("Configfile updated");
            }
        }

        ///<summary>Read Configfile or create on if not Exists</summary>
        /// <returns>Returns Configfile content as JObject</returns>  
        public static JObject ReadConfigfile(string path)
        {
            /////////////////////////////////////////////////
            /// Checking if file exists and is not empty ///
            ///////////////////////////////////////////////
            if (File.Exists(path) && new FileInfo(path).Length != 0)
            {
                /////////////////////////////////////////////////////////////////////////////
                /// Open a StreamReader to read the filecontent an parse it to a JObject ///
                ///////////////////////////////////////////////////////////////////////////
                using (StreamReader r = new StreamReader(path))
                {
                    string json = r.ReadToEnd();
                    JObject filecontent = JObject.Parse(json);
                    return filecontent;
                }
            }
            else
            {
                /////////////////////////////////////////////////
                /// Creating JObject with the default values ///
                ///////////////////////////////////////////////
                JObject json = new JObject(new JProperty("host", "localhost"), new JProperty("username", "sa"), new JProperty("password", "kwpsarix"), new JProperty("database", "BNWINS"), new JProperty("instance", "kwp"), new JProperty("last_sync", ""), new JProperty("limit", 10), new JProperty("apikey", ""), new JProperty("interval", 10.0), new JProperty("keyword", "craftnote"));
                ////////////////////////////////////////////////////
                /// Opening Filestream and write JObject to File ///
                ////////////////////////////////////////////////////
                using (FileStream fs = File.Create(path))
                {
                    //////////////////////////////
                    /// JObject to Byte array ///
                    ////////////////////////////
                    Byte[] contents = new UTF8Encoding(true).GetBytes(json.ToString());

                    ///////////////////////////////////
                    /// Writing Byte array to File ///
                    /////////////////////////////////
                    fs.Write(contents, 0, contents.Length);
                    Console.WriteLine("Configfile successfuly created");
                    LogfileHandler.Log("Configfile created");
                }
                return json;
            }
        }
    }
}
