// compile with: -doc:documentation.xml 
using System;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows.Forms;
using System.Timers;
using System.Net;
using System.Diagnostics;

namespace KwpMyCraftnoteProjectSyncAdapter
{
    class Program
    {
        private static Version version = Version.Parse("1.0.0.0");
        private static System.Timers.Timer aTimer;
        private static string fileName = "config.json";
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
        private static SqlConnection cnn;
        private static SqlConnectionStringBuilder builder;
        static void Main(string[] args)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            version = assembly.GetName().Version;
            CheckUpdate(version);



            //////////////////////////////////////////////////////
            /// Searches commandline args for a Configfilepath ///
            //////////////////////////////////////////////////////
            if (args.Length != 0)
            {
                fileName = args[0].ToString();
            }
            Log("Configfile: " + fileName);

            try
            {
                //////////////////////////
                /// Reading Configfile ///
                //////////////////////////
                JObject config = ReadConfigfile(fileName);
                Log("Configfile content: " + config.ToString());

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
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while processing the config file. Using fallback values. Please check the Configfile for any issues");
                Console.WriteLine(e.ToString());
                Log("Error in Configfile: " + e.ToString());
            }


            ///////////////////////////////
            /// Creating Sql connection ///
            ///////////////////////////////
            builder = new SqlConnectionStringBuilder();
            builder.DataSource = host + "\\" + instance;
            builder.UserID = username;
            builder.Password = password;
            builder.InitialCatalog = database;
            cnn = new SqlConnection(builder.ConnectionString);
            Log("Connectionstring: " + builder.ConnectionString);

            try
            {
                ////////////////////////////////////////
                /// Try to Connect to the Sql Server ///
                ////////////////////////////////////////
                Console.WriteLine("Connecting to SQL Server");
                cnn.Open();
                Console.WriteLine("!!!Connection Open!!!");
                Log("Sql Server Connected");

                /////////////////////////
                /// Run DB Processing ///
                /////////////////////////
                try
                {
                    //////////////////////////
                    /// Reading Configfile ///
                    //////////////////////////
                    JObject config = ReadConfigfile(fileName);
                    Log("Configfile content: " + config.ToString());


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
                    interval = (int)config["interval"];
                    apikey = (string)config["apikey"];
                }
                catch (Exception fileError)
                {
                    Console.WriteLine("Error while processing the config file. Using fallback values. Please check the Configfile for any issues");
                    Console.WriteLine(fileError.ToString());
                    Log("Error in Configfile: " + fileError.ToString());
                }
                current = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff");
                Log("Current timestamp: " + current);
                ProcessDatabase(lastSync, current, limit, apikey, cnn);
                UpdateConfigfile(fileName, host, username, password, database, instance, current, limit, apikey);

                ///////////////////////////////////////////////////////////
                /// Creating a Timer to run this Operations Periodicaly ///
                //////////////////////////////////////////////////////////
                SetTimer(interval * 60.0 * 1000.0);

                Console.Write("Press any key to exit... ");
                Console.ReadKey();

                ////////////////////////////////////
                /// Closing Sqlserver Connection ///
                ////////////////////////////////////
                cnn.Close();
                Console.WriteLine("!!!Connection Closed!!!");
                Log("Sql Server disonnected");

            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                MessageBox.Show("Bitte lassen sie Dieses Fenster Geöffnet und informieren sie ihren Systemadministrator \n\n\n" + e.ToString(), "KWP -> MyCraftnote Projekt Synchronisation             Schwerwiegender Fehler!!! ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cnn.Close();
                Log("Sql Error: " + e.ToString());
                System.Threading.Thread.Sleep(5000);
                Environment.Exit(1);
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(e.ToString());
                MessageBox.Show("Bitte lassen sie Dieses Fenster Geöffnet und informieren sie ihren Systemadministrator \n\n\n" + e.ToString(), "KWP -> MyCraftnote Projekt Synchronisation             Schwerwiegender Fehler!!! ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cnn.Close();
                Log("ArgumentNull Error: " + e.ToString());
                System.Threading.Thread.Sleep(5000);
                Environment.Exit(1);
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e.ToString());
                MessageBox.Show("Bitte lassen sie Dieses Fenster Geöffnet und informieren sie ihren Systemadministrator \n\n\n" + e.ToString(), "KWP -> MyCraftnote Projekt Synchronisation             Schwerwiegender Fehler!!! ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cnn.Close();
                Log("NullReference Error: " + e.ToString());
                System.Threading.Thread.Sleep(5000);
                Environment.Exit(1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                MessageBox.Show("Bitte lassen sie Dieses Fenster Geöffnet und informieren sie ihren Systemadministrator \n\n\n" + e.ToString(), "KWP -> MyCraftnote Projekt Synchronisation             Schwerwiegender Fehler!!! ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cnn.Close();
                Log("Error: " + e.ToString());
                System.Threading.Thread.Sleep(5000);
                Environment.Exit(1);
            }

        }

        private static void SetTimer(double timerInterval)
        {
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(timerInterval);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Log("Timer run");
            try
            {
                //////////////////////////
                /// Reading Configfile ///
                //////////////////////////
                JObject config = ReadConfigfile(fileName);
                Log("Configfile content: " + config.ToString());


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
                interval = (int)config["interval"];
                apikey = (string)config["apikey"];
            }
            catch (Exception fileError)
            {
                Console.WriteLine("Error while processing the config file. Using fallback values. Please check the Configfile for any issues");
                Console.WriteLine(fileError.ToString());
                Log("Error in Configfile: " + fileError.ToString());
            }
            current = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff");
            Log("Current timestamp: " + current);
            ProcessDatabase(lastSync, current, limit, apikey, cnn);
            UpdateConfigfile(fileName, host, username, password, database, instance, current, limit, apikey);
        }

        private static void ProcessDatabase(string lastSync, string current, int limit, string apikey, SqlConnection cnn)
        {
            List<Project> projects = new List<Project>();
            SqlCommand queryCommand;
            SqlDataReader queryReader;
            //////////////////////////////////////////////////
            /// Try to Read all Projects from the Database ///
            //////////////////////////////////////////////////
            queryCommand = new SqlCommand(null, cnn);
            queryCommand.CommandText = "Select * From dbo.Projekt;";

            /////////////////////////////////////////////////////////////
            /// Check if There was already an synchronistation or Not ///
            /////////////////////////////////////////////////////////////
            if (lastSync != "")
            {
                /////////////////////////////////////////////////////////////////////
                /// Updating Sql String to Read Projects only in the Timerange    ///
                /// between lastsync and current Time to avoid duplicate Projects ///
                /////////////////////////////////////////////////////////////////////
                queryCommand.CommandText = "Select * From dbo.Projekt where ProjAnlage between @lastsync and @now ;";
                queryCommand.Parameters.Add("@lastsync", SqlDbType.VarChar, lastSync.Length).Value = lastSync;
                queryCommand.Parameters.Add("@now", SqlDbType.VarChar, current.Length).Value = current;
                Console.WriteLine("Letzte Synchronisation: nie");
            }
            else
            {
                Console.WriteLine("Letzte Synchronisation: " + lastSync);
            }
            Console.WriteLine("Aktuelle Zeit: " + current);

            //////////////////////////////////////////////
            /// Building and Executing the Sql String ///
            ////////////////////////////////////////////
            queryCommand.Prepare();
            queryReader = queryCommand.ExecuteReader();

            Console.WriteLine("Reading dbo.Projekt");
            Log("Reading dbo.Projekt");
            ///////////////////////////////////
            /// Reading Sql Query Response ///
            /////////////////////////////////
            while (queryReader.Read())
            {
                //add new Project to Projects List
                projects.Add(new Project
                {
                    ProjAnlage = queryReader["ProjAnlage"].ToString(),
                    ProjNr = queryReader["ProjNr"].ToString(),
                    ProjBezeichnung = queryReader["ProjBezeichnung"].ToString(),
                    ProjAdr = queryReader["ProjAdr"].ToString(),
                    BauHrAdr = queryReader["BauHrAdr"].ToString()
                });
            }
            //closing reader
            queryReader.Close();



            //Get all Adress Values for all Projects from the Database
            for (var i = 0; i < projects.Count; i++)
            {
                //////////////////////////////////////////////
                /// Building and Executing the Sql String ///
                ////////////////////////////////////////////
                queryCommand = new SqlCommand(null, cnn);
                queryCommand.CommandText = "Select * From dbo.adrAdressen Where AdrNrGes = @AdrNrGes;";
                queryCommand.Parameters.Add("@AdrNrGes", SqlDbType.VarChar, projects[i].ProjAdr.Length).Value = projects[i].ProjAdr;
                queryCommand.Prepare();
                queryReader = queryCommand.ExecuteReader();
                Console.WriteLine("Reading dbo.adrAdressen ProjAdr");
                Log("Reading dbo.adrAdressen ProjAdr");

                ///////////////////////////////////
                /// Reading Sql Query Response ///
                /////////////////////////////////
                while (queryReader.Read())
                {
                    projects[i].ProjAdrAnredeID = (int)queryReader["Anrede"];
                    projects[i].ProjAdrName = queryReader["Name"].ToString();
                    projects[i].ProjAdrVorname = queryReader["Vorname"].ToString();
                    projects[i].ProjAdrStrasse = queryReader["Strasse"].ToString();
                    projects[i].ProjAdrOrtOrtID = (int)queryReader["Ort"];
                }
                //closing reader
                queryReader.Close();

                //////////////////////////////////////////////
                /// Building and Executing the Sql String ///
                ////////////////////////////////////////////
                queryCommand.Parameters.Clear();
                queryCommand.Parameters.Add("@AdrNrGes", SqlDbType.VarChar, projects[i].BauHrAdr.Length).Value = projects[i].BauHrAdr;
                queryCommand.Prepare();
                queryReader = queryCommand.ExecuteReader();

                Console.WriteLine("Reading dbo.adrAdressen BauHrAdr");
                Log("Reading dbo.adrAdressen BauHrAdr");

                ///////////////////////////////////
                /// Reading Sql Query Response ///
                /////////////////////////////////
                while (queryReader.Read())
                {
                    projects[i].BauHrAdrAnredeID = (int)queryReader["Anrede"];
                    projects[i].BauHrAdrName = queryReader["Name"].ToString();
                    projects[i].BauHrAdrVorname = queryReader["Vorname"].ToString();
                    projects[i].BauHrAdrStrasse = queryReader["Strasse"].ToString();
                    projects[i].BauHrAdrOrtOrtID = (int)queryReader["Ort"];
                }
                ////////////////////////////////////
                /// Closing Query Return Reader ///
                //////////////////////////////////
                queryReader.Close();



            }

            //Get all Anrede Values for all Projects from the Database
            for (var i = 0; i < projects.Count; i++)
            {

                //////////////////////////////////////////////
                /// Building and Executing the Sql String ///
                ////////////////////////////////////////////              
                queryCommand = new SqlCommand(null, cnn);
                queryCommand.CommandText = "Select * From dbo.adrAnreden Where AnredeID = @AnredeID;";
                queryCommand.Parameters.Add("@AnredeID", SqlDbType.Int, projects[i].ProjAdrAnredeID.ToString().Length).Value = projects[i].ProjAdrAnredeID;
                queryCommand.Prepare();
                queryReader = queryCommand.ExecuteReader();
                Console.WriteLine("Reading dbo.adrAnreden ProjAdrAnrede");
                Log("Reading dbo.adrAnreden ProjAdrAnrede");
                ///////////////////////////////////
                /// Reading Sql Query Response ///
                /////////////////////////////////
                while (queryReader.Read())
                {
                    projects[i].ProjAdrAnrede = queryReader["Anrede"].ToString();
                }
                ////////////////////////////////////
                /// Closing Query Return Reader ///
                //////////////////////////////////
                queryReader.Close();

                //////////////////////////////////////////////
                /// Building and Executing the Sql String ///
                ////////////////////////////////////////////
                queryCommand.Parameters.Clear();
                queryCommand.Parameters.Add("@AnredeID", SqlDbType.Int, projects[i].BauHrAdrAnredeID.ToString().Length).Value = projects[i].BauHrAdrAnredeID;
                queryCommand.Prepare();
                queryReader = queryCommand.ExecuteReader();

                Console.WriteLine("Reading dbo.adrAnreden ProjAdrAnrede");
                Log("Reading dbo.adrAnreden ProjAdrAnrede");
                ///////////////////////////////////
                /// Reading Sql Query Response ///
                /////////////////////////////////
                while (queryReader.Read())
                {
                    projects[i].BauHrAdrAnrede = queryReader["Anrede"].ToString();
                }
                ////////////////////////////////////
                /// Closing Query Return Reader ///
                //////////////////////////////////
                queryReader.Close();


            }

            //Get all Tel and Email Values for all Projects from the Database
            for (var i = 0; i < projects.Count; i++)
            {
                //////////////////////////////////////////////
                /// Building and Executing the Sql String ///
                ////////////////////////////////////////////
                queryCommand = new SqlCommand(null, cnn);
                queryCommand.CommandText = "Select * From dbo.adrKontakte Where AdrNrGes = @AdrNrGes;";
                queryCommand.Parameters.Add("@AdrNrGes", SqlDbType.VarChar, projects[i].ProjAdr.Length).Value = projects[i].ProjAdr;
                queryCommand.Prepare();
                queryReader = queryCommand.ExecuteReader();
                Console.WriteLine("Reading dbo.adrKontakte ProjAdrEmail and ProjAdrTel");
                Log("Reading dbo.adrKontakte ProjAdrEmail and ProjAdrTel");
                ///////////////////////////////////
                /// Reading Sql Query Response ///
                /////////////////////////////////
                while (queryReader.Read())
                {
                    projects[i].ProjAdrTelNr = queryReader["TelNrN"].ToString();
                    if (queryReader["Kontakt"].ToString().Contains("@"))
                    {
                        projects[i].ProjAdrEmail = queryReader["Kontakt"].ToString();
                    }
                    else
                    {
                        projects[i].ProjAdrEmail = "";
                    }
                }
                ////////////////////////////////////
                /// Closing Query Return Reader ///
                //////////////////////////////////
                queryReader.Close();

                //////////////////////////////////////////////
                /// Building and Executing the Sql String ///
                ////////////////////////////////////////////
                queryCommand.Parameters.Clear();
                queryCommand.Parameters.Add("@AdrNrGes", SqlDbType.VarChar, projects[i].ProjAdr.Length).Value = projects[i].ProjAdr;
                queryCommand.Prepare();
                queryReader = queryCommand.ExecuteReader();

                Console.WriteLine("Reading dbo.adrKontakte");
                Log("Reading dbo.adrKontakte");
                ///////////////////////////////////
                /// Reading Sql Query Response ///
                /////////////////////////////////
                while (queryReader.Read())
                {
                    projects[i].BauHrAdrTelNr = queryReader["TelNrN"].ToString();
                    if (queryReader["Kontakt"].ToString().Contains("@"))
                    {
                        projects[i].BauHrAdrEmail = queryReader["Kontakt"].ToString();
                    }
                    else
                    {
                        projects[i].BauHrAdrEmail = "";
                    }
                }
                ////////////////////////////////////
                /// Closing Query Return Reader ///
                //////////////////////////////////
                queryReader.Close();

            }

            //Get all City Values for all Projects from the Database
            for (var i = 0; i < projects.Count; i++)
            {


                //////////////////////////////////////////////
                /// Building and Executing the Sql String ///
                ////////////////////////////////////////////
                queryCommand = new SqlCommand(null, cnn);
                queryCommand.CommandText = "Select * From dbo.adrOrte Where OrtID = @OrtID;";

                queryCommand.Parameters.Add("@OrtID", SqlDbType.Int, projects[i].ProjAdrOrtOrtID.ToString().Length).Value = projects[i].ProjAdrOrtOrtID;
                queryCommand.Prepare();
                queryReader = queryCommand.ExecuteReader();

                Console.WriteLine("Reading dbo.adrOrte");
                Log("Reading dbo.adrOrte");
                ///////////////////////////////////
                /// Reading Sql Query Response ///
                /////////////////////////////////
                while (queryReader.Read())
                {
                    projects[i].ProjAdrOrtLand = queryReader["Land"].ToString();
                    projects[i].ProjAdrOrtPLZ = queryReader["PLZ"].ToString();
                    projects[i].ProjAdrOrtOrt = queryReader["Ort"].ToString();
                }
                ////////////////////////////////////
                /// Closing Query Return Reader ///
                //////////////////////////////////
                queryReader.Close();

                //////////////////////////////////////////////
                /// Building and Executing the Sql String ///
                ////////////////////////////////////////////
                queryCommand.Parameters.Clear();
                queryCommand.Parameters.Add("@OrtID", SqlDbType.Int, projects[i].BauHrAdrOrtOrtID.ToString().Length).Value = projects[i].BauHrAdrOrtOrtID;
                queryCommand.Prepare();
                queryReader = queryCommand.ExecuteReader();

                Console.WriteLine("Reading dbo.adrOrte");
                Log("Reading dbo.adrOrte");
                ///////////////////////////////////
                /// Reading Sql Query Response ///
                /////////////////////////////////
                while (queryReader.Read())
                {
                    projects[i].BauHrAdrOrtLand = queryReader["Land"].ToString();
                    projects[i].BauHrAdrOrtPLZ = queryReader["PLZ"].ToString();
                    projects[i].BauHrAdrOrtOrt = queryReader["Ort"].ToString();
                }
                ////////////////////////////////////
                /// Closing Query Return Reader ///
                //////////////////////////////////
                queryReader.Close();

            }
            //////////////////////////////////////////////////////////////
            /// Checking the max amount of Projects since the last     ///
            /// sync is not to high to avoid incase of malconfigurated ///
            /// confiogfile many duplicate Projects                    ///
            //////////////////////////////////////////////////////////////
            if (projects.Count > limit)
            {
                Console.WriteLine("Synchronisation Gestoppt !!!");
                Console.WriteLine("Zu viele neue Projekte seit der letzten Synchronisation. Sollte dies normal sein, ändern sie bitte den parameter limit in der Konfigurationsdatei");
                MessageBox.Show("Zu viele neue Projekte seit der letzten Synchronisation.Sollte dies normal sein, ändern sie bitte den parameter limit in der Konfigurationsdatei", "KWP -> MyCraftnote Projekt Synchronisation\nSynchronisation Gestoppt !!!");
                Log("To many Projects since last Sync");
                System.Threading.Thread.Sleep(5000);
                Environment.Exit(1);
            }

            for (var i = 0; i < projects.Count; i++)
            {
                ///////////////////////////
                /// Print Project Infos ///
                ///////////////////////////
                Console.WriteLine("Projekt: " + projects[i].ProjNr + " | " + projects[i].ProjBezeichnung);

                //////////////////////////////////////////////////////////////
                /// Call method to Send the Project to the MyCraftnote Api ///
                //////////////////////////////////////////////////////////////
                SendProject(projects[i], apikey);

            }
        }

        public static void UpdateConfigfile(string fileName, string host, string username, string password, string database, string instance, string current, int limit, string apikey)
        {
            ////////////////////////////////////////////////////////////////////////////////
            /// Rewriting Configfile to avoid malconfig and to update lastsync timestamp ///
            ////////////////////////////////////////////////////////////////////////////////
            File.WriteAllText(fileName, string.Empty);
            ///////////////////////////////////////////////////
            /// New JObject with updated Value for lastsync ///
            ///////////////////////////////////////////////////
            JObject json = new JObject(new JProperty("host", host), new JProperty("username", username), new JProperty("password", password), new JProperty("database", database), new JProperty("instance", instance), new JProperty("last_sync", current), new JProperty("limit", limit), new JProperty("apikey", apikey), new JProperty("interval",interval));
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
                Log("Configfile updated");
            }
        }

        ///<summary>Read Configfile or create on if not Exists</summary>
        /// <returns>Returns Configfile content as JObject</returns>  
        private static JObject ReadConfigfile(string path)
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
                JObject json = new JObject(new JProperty("host", "localhost"), new JProperty("username", "sa"), new JProperty("password", "kwpsarix"), new JProperty("database", "BNWINS"), new JProperty("instance", "kwp"), new JProperty("last_sync", ""), new JProperty("limit", 10), new JProperty("apikey", ""), new JProperty("interval", 10.0));
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
                    Log("Configfile created");
                }
                return json;
            }
        }

        ///<summary>Creates JObject for a Project and sends a Post request to the Carftnote Api</summary>
        private async static void SendProject(Project project, string apikey)
        {
            ////////////////////////////////////////////////
            /// Deleting evry slash from the Phonenumber ///
            ////////////////////////////////////////////////
            if (project.BauHrAdrTelNr != null)
            {
                project.BauHrAdrTelNr = project.BauHrAdrTelNr.Replace("/", "");
            }

            /////////////////////////////////////////////////
            /// Creating JObject with the Project values ///
            ///////////////////////////////////////////////
            if (project.ProjBezeichnung.Length != 0)
            {
                JObject json = new JObject(
                 new JProperty("name", project.ProjBezeichnung),
                 new JProperty("orderNumber", project.ProjNr),
                 new JProperty("street", project.ProjAdrStrasse),
                 new JProperty("zipcode", project.ProjAdrOrtPLZ),
                 new JProperty("city", project.ProjAdrOrtOrt),
                 new JProperty("contact",
                  new JArray(
                   new JObject(
                    new JProperty("name", project.BauHrAdrAnrede + " " + project.BauHrAdrVorname + " " + project.BauHrAdrName),
                    new JProperty("email", project.BauHrAdrEmail),
                    new JProperty("phone", project.BauHrAdrTelNr)
                   )
                  )
                 )
                );
                Log("Json Body: " + json.ToString());

                if (apikey.Length != 0)
                {
                    ///////////////////////////////////////////////
                    /// Sending JObject to the MyCraftnote Api ///
                    /////////////////////////////////////////////
                    using (var client = new HttpClient())
                    {
                        var response = await client.PostAsync(
                         "https://europe-west1-craftnote-live.cloudfunctions.net/v1/createproject?apikey" + apikey,
                         new StringContent(json.ToString(), Encoding.UTF8, "application/json"));

                        ////////////////////////////
                        /// Printing Api Result ///
                        //////////////////////////
                        Log("Http respinse: " + response.ToString());
                        JObject responseJson = JObject.Parse(response.ToString());
                        Console.WriteLine("Project Urls: " + responseJson["webLink"] + " || " + responseJson["appDeepLink"]);
                    }
                }
                else
                {
                    Console.WriteLine("kein ApiKey vorhanden");
                    Log("No ApiKey available");
                }
            }
            else
            {
                Console.WriteLine("Projekt bezeichnung nicht verfügbar");
                Log("No Project Name available");
            }
        }
        private static void Log(string msg)
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

        private static void CheckUpdate(Version version)
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
                        Log("update.json content: " + filecontent.ToString());
                        Version newVersion = Version.Parse(filecontent["version"].ToString());
                        if (version < newVersion)
                        {
                            Console.WriteLine("Update avilable");
                            myWebClient.DownloadFile("https://server01.nibot.me/kwpcarftnote/update.exe", "update.exe");
                            if (File.Exists("update.exe") && new FileInfo("update.exe").Length != 0)
                            {
                                Process.Start("update.exe");
                                Log("Update started");
                                System.Environment.Exit(0);
                            }
                        }
                        else
                        {
                            Log("Version is not Higher");
                        }
                    }
                }
            }catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                Log("Update Error: " + e.ToString());
                MessageBox.Show("Bitte lassen sie Dieses Fenster Geöffnet und informieren sie ihren Systemadministrator \n\n\n" + e.ToString(), "KWP -> MyCraftnote Projekt Synchronisation             Schwerwiegender Fehler!!! ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Threading.Thread.Sleep(5000);
                System.Environment.Exit(1);
            }


        }
    }

    

    public class Project
    {
        public String ProjAnlage
        {
            get;
            set;
        }
        public String ProjNr
        {
            get;
            set;
        }
        public String ProjBezeichnung
        {
            get;
            set;
        }
        public String ProjAdr
        {
            get;
            set;
        }
        public String BauHrAdr
        {
            get;
            set;
        }

        public int ProjAdrAnredeID
        {
            get;
            set;
        }
        public String ProjAdrAnrede
        {
            get;
            set;
        }
        public String ProjAdrName
        {
            get;
            set;
        }
        public String ProjAdrVorname
        {
            get;
            set;
        }
        public String ProjAdrStrasse
        {
            get;
            set;
        }
        public int ProjAdrOrtOrtID
        {
            get;
            set;
        }
        public String ProjAdrOrtLand
        {
            get;
            set;
        }
        public String ProjAdrOrtPLZ
        {
            get;
            set;
        }
        public String ProjAdrOrtOrt
        {
            get;
            set;
        }
        public String ProjAdrTelNr
        {
            get;
            set;
        }
        public String ProjAdrEmail
        {
            get;
            set;
        }

        public int BauHrAdrAnredeID
        {
            get;
            set;
        }
        public String BauHrAdrAnrede
        {
            get;
            set;
        }
        public String BauHrAdrName
        {
            get;
            set;
        }
        public String BauHrAdrVorname
        {
            get;
            set;
        }
        public String BauHrAdrStrasse
        {
            get;
            set;
        }
        public int BauHrAdrOrtOrtID
        {
            get;
            set;
        }
        public String BauHrAdrOrtLand
        {
            get;
            set;
        }
        public String BauHrAdrOrtPLZ
        {
            get;
            set;
        }
        public String BauHrAdrOrtOrt
        {
            get;
            set;
        }
        public String BauHrAdrTelNr
        {
            get;
            set;
        }
        public String BauHrAdrEmail
        {
            get;
            set;
        }
    }

}