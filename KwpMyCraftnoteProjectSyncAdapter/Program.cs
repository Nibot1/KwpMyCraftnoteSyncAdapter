using System;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Timers;

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
        private static string keyword = "craftnote";
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
            if (args.Length != 0)
            {
                fileName = args[0].ToString();
            }
            LogfileHandler.Log("Configfile: " + fileName);

            try
            {
                //////////////////////////
                /// Reading Configfile ///
                //////////////////////////
                config = ConfigfileHandler.ReadConfigfile(fileName);
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

                /////////////////////////
                /// Run DB Processing ///
                /////////////////////////

                current = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff");
                LogfileHandler.Log("Current timestamp: " + current);
                List<Models.Project> projects = DatabaseHandler.GetProjects(cnn, config);
                List<Models.Project> serviceProjects = DatabaseHandler.GetServiceProjects(cnn, config);
                ApiHandler.SendProjects(projects, apikey);
                ApiHandler.SendProjects(serviceProjects, apikey);
                ConfigfileHandler.UpdateConfigfile(fileName, config, current);

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
            try
            {
                //////////////////////////
                /// Reading Configfile ///
                //////////////////////////
                config = ConfigfileHandler.ReadConfigfile(fileName);
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
                if (projects[i].ProjAdrAnsprechpartnerID != (long)-1) {
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
            ApiHandler.SendProjects(projects,apikey);
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
            ApiHandler.SendProjects(serviceProjects,apikey);
            ConfigfileHandler.UpdateConfigfile(fileName, config, current);
        }




    }
}