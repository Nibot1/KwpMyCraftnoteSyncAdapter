using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Timers;
using System.Windows.Forms;

namespace KwpMyCraftnoteProjectSyncAdapter
{
    class Program
    {
        private static Version version = Version.Parse("1.0.0.0");
        private static System.Timers.Timer aTimer;
        private static string host = "localhost";
        private static string username = "sa";
        private static string password = "kwpsarix";
        private static string database = "BNWINS";
        private static string instance = "kwp";
        private static string lastSync = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff");
        private static int limit = 10;
        private static double interval = 5.0;
        private static string current = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff");
        private static string apikey = "";
        private static string keyword = "craftnote";
        private static string configfilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KwpMyCraftnoteSyncAdapter\\config.json");
        private static JObject config = new JObject();
        private static SqlConnection cnn;
        static void Main(string[] args)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            version = assembly.GetName().Version;
            UpdateHandler.CheckUpdate(version);



            //////////////////////////////////////////////////////
            /// Searches commandline args for a Configfilepath ///
            //////////////////////////////////////////////////////
            LogfileHandler.Log("Configfile: " + configfilePath);

            if (!ConfigfileHandler.CheckIfConfigFileExists(configfilePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Dies ist der Erste start der Anwendung. Als ertses müssen wir noc ein paar sachen Wissen.");
                Console.Write("Bitte Geben sie die IPAdresse des Computers mit dem Installierten Sqlservers an (localhost): ");
                host = Console.ReadLine();
                if (host.Length == 0)
                {
                    host = "localhost";
                }
                Console.Write("Bitte Geben sie jetzt die Sqlserver Instanz ein (kwp): ");
                instance = Console.ReadLine();
                if (instance.Length == 0)
                {
                    instance = "kwp";
                }
                Console.Write("Bitte Geben sie Jetzt die Start Datenbank ein (BNWINS): ");
                database = Console.ReadLine();
                if (database.Length == 0)
                {
                    database = "BNWINS";
                }
                Console.Write("Bitte geben sie jetz den Benutzernamen der Sqlserverinstanz ein (sa): ");
                username = Console.ReadLine();
                if (username.Length == 0)
                {
                    username = "sa";
                }
                Console.Write("Bitte geben sie jetzt das Passwort der Sqlserverinstanz ein (kwpsarix): ");
                password = Console.ReadLine();
                if (password.Length == 0)
                {
                    password = "kwpsarix";
                }
                Console.Write("Bitte geben sie jetzt den MyCraftnote API Schlüssel ein: ");
                apikey = Console.ReadLine();
                while (apikey == "")
                {
                    Console.Write("Der Api Schlüssel darf nicht leer sein. Bitte tragen sie hier ihren API Schlüssel ein: ");
                    apikey = Console.ReadLine();
                }
                Console.Write("Bitte geben sie jetzt den Gewünschten aktualisierungs rythmus in minuten an (5): ");
                string intervalString = Console.ReadLine();
                if (intervalString.Length == 0)
                {
                    interval = 5.0;
                }
                else
                {
                    if (isDouble(intervalString))
                    {
                        interval = Double.Parse(intervalString);
                    }
                    else { 
                        while (!isDouble(intervalString))
                        {
                            Console.Write("Die eingabe muss eine Zahl sein. Bitte geben sie jetzt den Gewünschten aktualisierungs rythmus in minuten an (5): ");
                            intervalString = Console.ReadLine();
                        }
                        interval = Double.Parse(intervalString);
                    }
                }
                config = new JObject(new JProperty("host", host), new JProperty("username", username), new JProperty("password", password), new JProperty("database", database), new JProperty("instance", instance), new JProperty("last_sync", current), new JProperty("limit", limit), new JProperty("apikey", apikey), new JProperty("interval", interval), new JProperty("keyword", keyword));
                ConfigfileHandler.CreateConfigFile(configfilePath, config);
                Console.WriteLine("Configuration Abgeschlossen. Die Konfigurationsdatei befindet sich hier: " + configfilePath);
            }

            try
            {
                //////////////////////////
                /// Reading Configfile ///
                //////////////////////////
                config = ConfigfileHandler.ReadConfigfile(configfilePath);
                LogfileHandler.Log("Configfile content: " + config.ToString());

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
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while processing the config file. Using fallback values. Please check the Configfile for any issues");
                Console.WriteLine(e.ToString());
                LogfileHandler.Log("Error in Configfile: " + e.ToString());
            }

            cnn = DatabaseHandler.BuildConnecion(host, instance, username, password, database);
            try
            {
                ////////////////////////////////////////
                /// Try to Connect to the Sql Server ///
                ////////////////////////////////////////
                Console.WriteLine("Connecting to SQL Server");
                cnn.Open();
                Console.WriteLine("!!!Connection Open!!!");
                LogfileHandler.Log("Sql Server Connected");

                runDBProcessing();

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
                LogfileHandler.Log("Sql Server disonnected");

            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                MessageBox.Show("Bitte lassen sie Dieses Fenster Geöffnet und informieren sie ihren Systemadministrator \n\n\n" + e.ToString(), "KWP -> MyCraftnote Projekt Synchronisation             Schwerwiegender Fehler!!! ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cnn.Close();
                LogfileHandler.Log("Sql Error: " + e.ToString());
                System.Threading.Thread.Sleep(5000);
                Environment.Exit(1);
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(e.ToString());
                MessageBox.Show("Bitte lassen sie Dieses Fenster Geöffnet und informieren sie ihren Systemadministrator \n\n\n" + e.ToString(), "KWP -> MyCraftnote Projekt Synchronisation             Schwerwiegender Fehler!!! ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cnn.Close();
                LogfileHandler.Log("ArgumentNull Error: " + e.ToString());
                System.Threading.Thread.Sleep(5000);
                Environment.Exit(1);
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e.ToString());
                MessageBox.Show("Bitte lassen sie Dieses Fenster Geöffnet und informieren sie ihren Systemadministrator \n\n\n" + e.ToString(), "KWP -> MyCraftnote Projekt Synchronisation             Schwerwiegender Fehler!!! ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cnn.Close();
                LogfileHandler.Log("NullReference Error: " + e.ToString());
                System.Threading.Thread.Sleep(5000);
                Environment.Exit(1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                MessageBox.Show("Bitte lassen sie Dieses Fenster Geöffnet und informieren sie ihren Systemadministrator \n\n\n" + e.ToString(), "KWP -> MyCraftnote Projekt Synchronisation             Schwerwiegender Fehler!!! ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cnn.Close();
                LogfileHandler.Log("Error: " + e.ToString());
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
            LogfileHandler.Log("Timer run");
            runDBProcessing();
        }

        public static void runDBProcessing()
        {
            try
            {
                //////////////////////////
                /// Reading Configfile ///
                //////////////////////////
                config = ConfigfileHandler.ReadConfigfile(configfilePath);
                LogfileHandler.Log("Configfile content: " + config.ToString());


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
                keyword = (string)config["keyword"];
            }
            catch (Exception fileError)
            {
                Console.WriteLine("Error while processing the config file. Using fallback values. Please check the Configfile for any issues");
                Console.WriteLine(fileError.ToString());
                LogfileHandler.Log("Error in Configfile: " + fileError.ToString());
            }
            current = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff");
            LogfileHandler.Log("Current timestamp: " + current);
            List<Models.Project> projects = DatabaseHandler.GetProjects(cnn, config);
            List<Models.Project> serviceProjects = DatabaseHandler.GetServiceProjects(cnn, config);
            for (var i = 0; i < projects.Count; i++)
            {
                if (projects[i].ProjAdrAnsprechpartnerID != (long)-1)
                {
                    projects[i].ProjectAdrAnsprechpartner = DatabaseHandler.GetContactPersonById(cnn, projects[i].ProjAdrAnsprechpartnerID);
                }
                if (projects[i].BauHrAdrAnsprechpartnerID != (long)-1)
                {
                    projects[i].BauHrAdrAnsprechpartner = DatabaseHandler.GetContactPersonById(cnn, projects[i].BauHrAdrAnsprechpartnerID);
                }
                if (projects[i].RechAdrAnsprechpartnerID != (long)-1)
                {
                    projects[i].RechAdrAnsprechpartner = DatabaseHandler.GetContactPersonById(cnn, projects[i].ProjAdrAnsprechpartnerID);
                }
            }
            ApiHandler.SendProjects(projects, apikey);

            for (var i = 0; i < serviceProjects.Count; i++)
            {
                if (serviceProjects[i].ProjAdrAnsprechpartnerID != (long)-1)
                {
                    serviceProjects[i].ProjectAdrAnsprechpartner = DatabaseHandler.GetContactPersonById(cnn, serviceProjects[i].ProjAdrAnsprechpartnerID);
                }
                if (serviceProjects[i].BauHrAdrAnsprechpartnerID != (long)-1)
                {
                    serviceProjects[i].BauHrAdrAnsprechpartner = DatabaseHandler.GetContactPersonById(cnn, serviceProjects[i].BauHrAdrAnsprechpartnerID);
                }
                if (serviceProjects[i].RechAdrAnsprechpartnerID != (long)-1)
                {
                    serviceProjects[i].RechAdrAnsprechpartner = DatabaseHandler.GetContactPersonById(cnn, serviceProjects[i].ProjAdrAnsprechpartnerID);
                }
            }
            ApiHandler.SendProjects(serviceProjects, apikey);
            ConfigfileHandler.UpdateConfigfile(configfilePath, config, current);
        }

        public static Boolean isDouble(string input)
        {
            try
            {
                Double.Parse(input);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }
    }
}