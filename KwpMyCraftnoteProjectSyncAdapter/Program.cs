using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Net.Http;

namespace KwpMyCraftnoteProjectSyncAdapter
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName = "config.json";
            string host = "localhost";
            string username = "sa";
            string password = "kwpsarix";
            string database = "BNWINS";
            string instance = "kwp";
            string lastSync = "";
            int limit = 10;
            string current = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff");
            string apikey = "";
            List<Project> projects = new List<Project>();
            //check if configfile path is present
            if (args.Length != 0)
            {
                fileName = args[0].ToString();
            }

            //check if File exists and is not Empty
            if (File.Exists(fileName) && new FileInfo(fileName).Length != 0)
            {
                //read the File contents and parse it to Json
                using (StreamReader r = new StreamReader(fileName))
                {
                    //Parse the File content to a JSONObject
                    var json = r.ReadToEnd();
                    var filecontent = JObject.Parse(json);

                    // update the variables
                    host = (string)filecontent["host"];
                    username = (string)filecontent["username"];
                    password = (string)filecontent["password"];
                    database = (string)filecontent["database"];
                    database = (string)filecontent["database"];
                    instance = (string)filecontent["instance"];
                    lastSync = (string)filecontent["last_sync"];
                    limit = (int)filecontent["limit"];
                    apikey = (string)filecontent["apikey"];
                }
            }
            else
            {
                //create JSONObject wiht default Values
                JObject json = new JObject(new JProperty("host", "localhost"), new JProperty("username", "sa"), new JProperty("password", "kwpsarix"), new JProperty("database", "BNWINS"), new JProperty("instance", "kwp"), new JProperty("last_sync", ""), new JProperty("limit", 10), new JProperty("apikey", ""));
                //open Filestream to Write to the Configfile
                using (FileStream fs = File.Create(fileName))
                {
                    //convert JSONObject to a Byte Array 
                    Byte[] contents = new UTF8Encoding(true).GetBytes(json.ToString());

                    //write the Byte array to the File
                    fs.Write(contents, 0, contents.Length);
                }
            }

            //create SQLServer connection
            SqlConnection cnn;
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = host + "\\" + instance;
            builder.UserID = username;
            builder.Password = password;
            builder.InitialCatalog = database;
            //Console.WriteLine(builder.ConnectionString);                
            cnn = new SqlConnection(builder.ConnectionString);
            SqlCommand queryCommand;
            SqlDataReader queryReader;
            try
            {
                //try to open sql connection
                cnn.Open();
                Console.WriteLine("connection open");

                // Get All Projects from Database
                

                //prepare the sql query
                
                queryCommand = new SqlCommand(null, cnn);
                queryCommand.CommandText = "Select * From dbo.Projekt;";
                Console.WriteLine(lastSync);
                Console.WriteLine(current);
                if (lastSync != "")
                {
                    queryCommand.CommandText = "Select * From dbo.Projekt where ProjAnlage between @lastsync and @now ;";
                
                queryCommand.Parameters.Add("@lastsync", SqlDbType.VarChar,lastSync.Length).Value = lastSync;
                queryCommand.Parameters.Add("@now", SqlDbType.VarChar, current.Length).Value = current;
                }
                queryCommand.Prepare();
                queryReader = queryCommand.ExecuteReader();

                Console.WriteLine("Reading dbo.Projekt");
                //start reading response
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
                   
                    //prepare the sql query
                    
                    queryCommand = new SqlCommand(null, cnn);
                    queryCommand.CommandText = "Select * From dbo.adrAdressen Where AdrNrGes = @AdrNrGes;";
                    queryCommand.Parameters.Add("@AdrNrGes", SqlDbType.VarChar, projects[i].ProjAdr.Length).Value = projects[i].ProjAdr;
                    queryCommand.Prepare();

                    queryReader = queryCommand.ExecuteReader();
                    Console.WriteLine("Reading dbo.adrAdressen ProjAdr");
                    //start reading response
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

                    queryCommand.Parameters.Clear();
                    queryCommand.Parameters.Add("@AdrNrGes", SqlDbType.VarChar, projects[i].BauHrAdr.Length).Value = projects[i].BauHrAdr;
                    queryCommand.Prepare();
                    queryReader = queryCommand.ExecuteReader();

                    Console.WriteLine("Reading dbo.adrAdressen BauHrAdr");

                    //start reading response
                    while (queryReader.Read())
                    {
                        projects[i].BauHrAdrAnredeID = (int)queryReader["Anrede"];
                        projects[i].BauHrAdrName = queryReader["Name"].ToString();
                        projects[i].BauHrAdrVorname = queryReader["Vorname"].ToString();
                        projects[i].BauHrAdrStrasse = queryReader["Strasse"].ToString();
                        projects[i].BauHrAdrOrtOrtID = (int)queryReader["Ort"];
                    }
                    queryReader.Close();
                    //closing reader
                    
                    
                }

                //Get all Anrede Values for all Projects from the Database
                for (var i = 0; i < projects.Count; i++)
                {

                    //prepare the sql query                  
                    queryCommand = new SqlCommand(null, cnn);
                    queryCommand.CommandText = "Select * From dbo.adrAnreden Where AnredeID = @AnredeID;";
                    queryCommand.Parameters.Add("@AnredeID", SqlDbType.Int, projects[i].ProjAdrAnredeID.ToString().Length).Value = projects[i].ProjAdrAnredeID;
                    queryCommand.Prepare();
                    queryReader = queryCommand.ExecuteReader();
                    Console.WriteLine("Reading dbo.adrAnreden ProjAdrAnrede");
                    //start reading response
                    while (queryReader.Read())
                    {
                        projects[i].ProjAdrAnrede = queryReader["Anrede"].ToString();
                    }
                    //closing reader
                    queryReader.Close();


                    queryCommand.Parameters.Clear();
                    queryCommand.Parameters.Add("@AnredeID", SqlDbType.Int, projects[i].BauHrAdrAnredeID.ToString().Length).Value = projects[i].BauHrAdrAnredeID;
                    queryCommand.Prepare();
                    queryReader = queryCommand.ExecuteReader();

                    Console.WriteLine("Reading dbo.adrAnreden ProjAdrAnrede");
                    //start reading response
                    while (queryReader.Read())
                    {
                        projects[i].BauHrAdrAnrede = queryReader["Anrede"].ToString();
                    }
                    //closing reader
                    queryReader.Close();


                }

                //Get all Tel and Email Values for all Projects from the Database
                for (var i = 0; i < projects.Count; i++)
                {
                    
                    queryCommand = new SqlCommand(null, cnn);
                    queryCommand.CommandText = "Select * From dbo.adrKontakte Where AdrNrGes = @AdrNrGes;";
                    queryCommand.Parameters.Add("@AdrNrGes", SqlDbType.VarChar, projects[i].ProjAdr.Length).Value = projects[i].ProjAdr;
                    queryCommand.Prepare();
                    queryReader = queryCommand.ExecuteReader();
                    Console.WriteLine("Reading dbo.adrKontakte ProjAdrEmail and ProjAdrTel");
                    //start reading response
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
                    //closing reader
                    queryReader.Close();


                    queryCommand.Parameters.Clear();
                    queryCommand.Parameters.Add("@AdrNrGes", SqlDbType.VarChar, projects[i].ProjAdr.Length).Value = projects[i].ProjAdr;
                    queryCommand.Prepare();
                    queryReader = queryCommand.ExecuteReader();

                    Console.WriteLine("Reading dbo.adrKontakte BauHrAdrEmail and BauHrAdrTel");
                    //start reading response
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
                    //closing reader
                    queryReader.Close();

                }

                //Get all City Values for all Projects from the Database
                for (var i = 0; i < projects.Count; i++)
                {

                    
                    
                    queryCommand = new SqlCommand(null, cnn);
                    queryCommand.CommandText = "Select * From dbo.adrOrte Where OrtID = @OrtID;";

                    queryCommand.Parameters.Add("@OrtID", SqlDbType.Int, projects[i].ProjAdrOrtOrtID.ToString().Length).Value = projects[i].ProjAdrOrtOrtID;
                    queryCommand.Prepare();
                    queryReader = queryCommand.ExecuteReader();

                    Console.WriteLine("Reading dbo.adrOrte ProjAdrOrt");
                    //start reading response
                    while (queryReader.Read())
                    {
                        projects[i].ProjAdrOrtLand = queryReader["Land"].ToString();
                        projects[i].ProjAdrOrtPLZ = queryReader["PLZ"].ToString();
                        projects[i].ProjAdrOrtOrt = queryReader["Ort"].ToString();
                    }
                    //closing reader
                    queryReader.Close();

                    queryCommand.Parameters.Clear();
                    queryCommand.Parameters.Add("@OrtID", SqlDbType.Int, projects[i].BauHrAdrOrtOrtID.ToString().Length).Value = projects[i].BauHrAdrOrtOrtID;
                    queryCommand.Prepare();
                    queryReader = queryCommand.ExecuteReader();

                    Console.WriteLine("Reading dbo.adrOrte BauHrAdrOrt");
                    //start reading response
                    while (queryReader.Read())
                    {
                        projects[i].BauHrAdrOrtLand = queryReader["Land"].ToString();
                        projects[i].BauHrAdrOrtPLZ = queryReader["PLZ"].ToString();
                        projects[i].BauHrAdrOrtOrt = queryReader["Ort"].ToString();
                    }
                    //closing reader
                    queryReader.Close();

                }
                //closing sql connection
                cnn.Close();

                //Update config File
                //clear configFile
                System.IO.File.WriteAllText(fileName, string.Empty);
                //create JSONObject wihth config Values and updated last sync
                JObject json = new JObject(new JProperty("host", host), new JProperty("username", username), new JProperty("password", password), new JProperty("database", database), new JProperty("instance", instance), new JProperty("last_sync", current), new JProperty("limit", limit), new JProperty("apikey", apikey));
                //open Filestream to Write to the Configfile
                using (FileStream fs = File.Create(fileName))
                {
                    //convert JSONObject to a Byte Array 
                    Byte[] contents = new UTF8Encoding(true).GetBytes(json.ToString());

                    //write the Byte array to the File
                    fs.Write(contents, 0, contents.Length);
                }

                if (projects.Count > limit)
                {
                    Console.WriteLine("Synchronisation Gestoppt !!!");
                    Console.WriteLine("Zu viele neue Projekte seit der letzten Synchronisation. Sollte dies normal sein, ändern sie bitte den parameter limit in der Konfigurationsdatei");
                    Environment.Exit(1);
                }

                //Print all Project IDs
                for (var i = 0; i < projects.Count; i++)
                {
                    Console.WriteLine("Projekt: " + projects[i].ProjNr + " | " + projects[i].ProjBezeichnung);
                    SendProject(projects[i], apikey);

                }

            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                Environment.Exit(1);
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(e.ToString());
                Environment.Exit(1);
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e.ToString());
                Environment.Exit(1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Environment.Exit(1);
            }

        }

        private static void SendProject(Project project, string apikey)
        {

            if (project.BauHrAdrTelNr != null)
            {
                project.BauHrAdrTelNr = project.BauHrAdrTelNr.Replace("/", "");
            }
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
            //Console.WriteLine(json.ToString());
            /*using (var client = new HttpClient())
            {
                var response = await client.PostAsync(
                    "https://europe-west1-craftnote-live.cloudfunctions.net/v1/createproject?apikey"+apikey,
                     new StringContent(json.ToString(), Encoding.UTF8, "application/json"));
                Console.WriteLine(response.ToString());
            }*/
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