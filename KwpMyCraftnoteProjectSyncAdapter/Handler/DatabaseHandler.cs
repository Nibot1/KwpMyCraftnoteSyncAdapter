using System;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Numerics;

namespace KwpMyCraftnoteProjectSyncAdapter
{
    class DatabaseHandler
    {

        public static SqlConnection BuildConnecion(String host, String instance, String username, String password, String database)
        {
            ///////////////////////////////
            /// Creating Sql connection ///
            ///////////////////////////////
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = host + "\\" + instance;
            builder.UserID = username;
            builder.Password = password;
            builder.InitialCatalog = database;
            SqlConnection cnn = new SqlConnection(builder.ConnectionString);
            LogfileHandler.Log("Connectionstring: " + builder.ConnectionString);
            return cnn;
        }

        public static List<Models.Project> GetProjects(SqlConnection cnn, JObject config)
        {
            string lastSync = "";
            int limit = 10;
            string current = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff");
            string keyword = "craftnote";
            List<Models.Project> projects = new List<Models.Project>();
            SqlCommand queryCommand;
            SqlDataReader queryReader;

            /////////////////////////////////
            ///Try to Update config Values///
            /////////////////////////////////
            try
            {
                lastSync = (string)config["last_sync"];
                limit = (int)config["limit"];
                keyword = (string)config["keyword"];
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while processing the config file. Using fallback values. Please check the Configfile for any issues");
                Console.WriteLine(e.ToString());
                LogfileHandler.Log("Error in Configfile: " + e.ToString());
            }

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
                Console.WriteLine("Letzte Synchronisation: " + lastSync);
            }
            else
            {
                Console.WriteLine("Letzte Synchronisation: nie");
            }
            Console.WriteLine("Aktuelle Zeit: " + current);

            //////////////////////////////////////////////
            /// Building and Executing the Sql String ///
            ////////////////////////////////////////////
            queryCommand.Prepare();
            queryReader = queryCommand.ExecuteReader();

            Console.WriteLine("Reading dbo.Projekt");
            LogfileHandler.Log("Reading dbo.Projekt");
            ///////////////////////////////////
            /// Reading Sql Query Response ///
            /////////////////////////////////
            while (queryReader.Read())
            {
                //add new Project to Projects List
                projects.Add(new Models.Project
                {
                    ProjAnlage = queryReader["ProjAnlage"].ToString(),
                    ProjNr = queryReader["ProjNr"].ToString(),
                    ProjBezeichnung = queryReader["ProjBezeichnung"].ToString(),
                    ProjAdr = queryReader["ProjAdr"].ToString(),
                    BauHrAdr = queryReader["BauHrAdr"].ToString(),
                    RechAdr = queryReader["RechAdr"].ToString(),
                    ProjAdrAnsprechpartnerID = (long)queryReader["fkAnsprechpartnerIDProjAdr"],
                    BauHrAdrAnsprechpartnerID = (long)queryReader["fkAnsprechpartnerIDBauhAdr"],
                    RechAdrAnsprechpartnerID = (long)queryReader["fkAnsprechpartnerIDRechAdr"]

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
                LogfileHandler.Log("Reading dbo.adrAdressen ProjAdr");

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
                LogfileHandler.Log("Reading dbo.adrAdressen BauHrAdr");

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

                //////////////////////////////////////////////
                /// Building and Executing the Sql String ///
                ////////////////////////////////////////////
                queryCommand.Parameters.Clear();
                queryCommand.Parameters.Add("@AdrNrGes", SqlDbType.VarChar, projects[i].RechAdr.Length).Value = projects[i].RechAdr;
                queryCommand.Prepare();
                queryReader = queryCommand.ExecuteReader();

                Console.WriteLine("Reading dbo.adrAdressen RechAdr");
                LogfileHandler.Log("Reading dbo.adrAdressen RechAdr");

                ///////////////////////////////////
                /// Reading Sql Query Response ///
                /////////////////////////////////
                while (queryReader.Read())
                {
                    projects[i].RechAdrAnredeID = (int)queryReader["Anrede"];
                    projects[i].RechAdrName = queryReader["Name"].ToString();
                    projects[i].RechAdrVorname = queryReader["Vorname"].ToString();
                    projects[i].RechAdrStrasse = queryReader["Strasse"].ToString();
                    projects[i].RechAdrOrtOrtID = (int)queryReader["Ort"];
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
                LogfileHandler.Log("Reading dbo.adrAnreden ProjAdrAnrede");
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
                LogfileHandler.Log("Reading dbo.adrAnreden ProjAdrAnrede");
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

                //////////////////////////////////////////////
                /// Building and Executing the Sql String ///
                ////////////////////////////////////////////
                queryCommand.Parameters.Clear();
                queryCommand.Parameters.Add("@AnredeID", SqlDbType.Int, projects[i].RechAdrAnredeID.ToString().Length).Value = projects[i].RechAdrAnredeID;
                queryCommand.Prepare();
                queryReader = queryCommand.ExecuteReader();

                Console.WriteLine("Reading dbo.adrAnreden RechAdrAnrede");
                LogfileHandler.Log("Reading dbo.adrAnreden RechAdrAnrede");
                ///////////////////////////////////
                /// Reading Sql Query Response ///
                /////////////////////////////////
                while (queryReader.Read())
                {
                    projects[i].RechAdrAnrede = queryReader["Anrede"].ToString();
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
                queryCommand.CommandText = "Select * From dbo.adrKontakte Where AdrNrGes = @AdrNrGes AND AnsprechpartnerID = -1;";
                queryCommand.Parameters.Add("@AdrNrGes", SqlDbType.VarChar, projects[i].ProjAdr.Length).Value = projects[i].ProjAdr;
                queryCommand.Prepare();
                queryReader = queryCommand.ExecuteReader();
                Console.WriteLine("Reading dbo.adrKontakte ProjAdrEmail and ProjAdrTel");
                LogfileHandler.Log("Reading dbo.adrKontakte ProjAdrEmail and ProjAdrTel");
                ///////////////////////////////////
                /// Reading Sql Query Response ///
                /////////////////////////////////
                while (queryReader.Read())
                {
                    switch ((int)queryReader["KontaktArt"])
                    {
                        case 0:
                            if (!DBNull.Value.Equals(queryReader["TelNrN"]) && queryReader["TelNrN"].ToString() != "-1")
                            {
                                projects[i].ProjAdrTelNr = queryReader["TelNrN"].ToString();
                            }
                            break;
                        case 2:
                            if (!DBNull.Value.Equals(queryReader["Kontakt"]) && queryReader["Kontakt"].ToString().Length >= 5)
                            {
                                projects[i].ProjAdrEmail = queryReader["Kontakt"].ToString();
                            }
                            break;
                        case 4:
                            if (String.IsNullOrEmpty(projects[i].ProjAdrTelNr) && queryReader["TelNrN"].ToString().Length != 0 && queryReader["TelNrN"].ToString() != "-1")
                            {
                                projects[i].ProjAdrTelNr = queryReader["TelNrN"].ToString();
                            }
                            break;

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
                queryCommand.Parameters.Add("@AdrNrGes", SqlDbType.VarChar, projects[i].BauHrAdr.Length).Value = projects[i].BauHrAdr;
                queryCommand.Prepare();
                queryReader = queryCommand.ExecuteReader();

                Console.WriteLine("Reading dbo.adrKontakte");
                LogfileHandler.Log("Reading dbo.adrKontakte");
                ///////////////////////////////////
                /// Reading Sql Query Response ///
                /////////////////////////////////
                while (queryReader.Read())
                {

                    switch ((int)queryReader["KontaktArt"])
                    {
                        case 0:
                            if (queryReader["TelNrN"].ToString() != "-1")
                            {
                                projects[i].BauHrAdrTelNr = queryReader["TelNrN"].ToString();
                            }
                            break;
                        case 2:
                            if (queryReader["Kontakt"].ToString().Length >= 5)
                            {
                                projects[i].BauHrAdrEmail = queryReader["Kontakt"].ToString();
                            }
                            break;
                        case 4:
                            if (!String.IsNullOrEmpty(projects[i].BauHrAdrTelNr) && queryReader["TelNrN"].ToString() != "-1")
                            {
                                projects[i].BauHrAdrTelNr = queryReader["TelNrN"].ToString();
                            }
                            break;

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
                queryCommand.Parameters.Add("@AdrNrGes", SqlDbType.VarChar, projects[i].RechAdr.Length).Value = projects[i].RechAdr;
                queryCommand.Prepare();
                queryReader = queryCommand.ExecuteReader();

                Console.WriteLine("Reading dbo.adrKontakte");
                LogfileHandler.Log("Reading dbo.adrKontakte");
                ///////////////////////////////////
                /// Reading Sql Query Response ///
                /////////////////////////////////
                while (queryReader.Read())
                {

                    switch ((int)queryReader["KontaktArt"])
                    {
                        case 0:
                            if (queryReader["TelNrN"].ToString() != "-1")
                            {
                                projects[i].RechAdrTelNr = queryReader["TelNrN"].ToString();
                            }
                            break;
                        case 2:
                            if (queryReader["Kontakt"].ToString().Length >= 5)
                            {
                                projects[i].RechAdrEmail = queryReader["Kontakt"].ToString();
                            }
                            break;
                        case 4:
                            if (!String.IsNullOrEmpty(projects[i].BauHrAdrTelNr) && queryReader["TelNrN"].ToString() != "-1")
                            {
                                projects[i].RechAdrTelNr = queryReader["TelNrN"].ToString();
                            }
                            break;

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
                LogfileHandler.Log("Reading dbo.adrOrte");
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
                LogfileHandler.Log("Reading dbo.adrOrte");
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

                //////////////////////////////////////////////
                /// Building and Executing the Sql String ///
                ////////////////////////////////////////////
                queryCommand.Parameters.Clear();
                queryCommand.Parameters.Add("@OrtID", SqlDbType.Int, projects[i].RechAdrOrtOrtID.ToString().Length).Value = projects[i].RechAdrOrtOrtID;
                queryCommand.Prepare();
                queryReader = queryCommand.ExecuteReader();

                Console.WriteLine("Reading dbo.adrOrte");
                LogfileHandler.Log("Reading dbo.adrOrte");
                ///////////////////////////////////
                /// Reading Sql Query Response ///
                /////////////////////////////////
                while (queryReader.Read())
                {
                    projects[i].RechAdrOrtLand = queryReader["Land"].ToString();
                    projects[i].RechAdrOrtPLZ = queryReader["PLZ"].ToString();
                    projects[i].RechAdrOrtOrt = queryReader["Ort"].ToString();
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
                LogfileHandler.Log("To many Projects since last Sync");
                System.Threading.Thread.Sleep(5000);
                Environment.Exit(1);
            }

            for (var i = 0; i < projects.Count; i++)
            {
                ///////////////////////////
                /// Print Project Infos ///
                ///////////////////////////
                Console.WriteLine("Projekt: " + projects[i].ProjNr + " | " + projects[i].ProjBezeichnung);
            }
            return projects;
        }

        public static List<Models.Project> GetServiceProjects(SqlConnection cnn, JObject config)
        {
            string lastSync = "";
            int limit = 10;
            string current = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff");
            string keyword = "craftnote";

            try
            {
                lastSync = (string)config["last_sync"];
                limit = (int)config["limit"];
                keyword = (string)config["keyword"];
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while processing the config file. Using fallback values. Please check the Configfile for any issues");
                Console.WriteLine(e.ToString());
                LogfileHandler.Log("Error in Configfile: " + e.ToString());
            }

            List<Models.Project> projects = new List<Models.Project>();
            SqlCommand queryCommand;
            SqlDataReader queryReader;

            //////////////////////////////////////////////////
            /// Try to Read all Projects from the Database ///
            //////////////////////////////////////////////////
            queryCommand = new SqlCommand(null, cnn);
            queryCommand.CommandText = "Select * From dbo.Regie Where Info1 Like @keyword;";
            queryCommand.Parameters.Add("@keyword", SqlDbType.VarChar, keyword.Length + 2).Value = "%" + keyword + "%";

            /////////////////////////////////////////////////////////////
            /// Check if There was already an synchronistation or Not ///
            /////////////////////////////////////////////////////////////
            if (lastSync != "")
            {
                /////////////////////////////////////////////////////////////////////
                /// Updating Sql String to Read Projects only in the Timerange    ///
                /// between lastsync and current Time to avoid duplicate Projects ///
                /////////////////////////////////////////////////////////////////////
                queryCommand.Parameters.Clear();
                queryCommand.CommandText = "Select * From dbo.Regie where ProjAnlage between @lastsync and @now AND Info1 Like @keyword;";
                queryCommand.Parameters.Add("@lastsync", SqlDbType.VarChar, lastSync.Length).Value = lastSync;
                queryCommand.Parameters.Add("@now", SqlDbType.VarChar, current.Length).Value = current;
                queryCommand.Parameters.Add("@keyword", SqlDbType.VarChar, keyword.Length + 2).Value = "%" + keyword + "%";
                Console.WriteLine("Letzte Synchronisation: " + lastSync);
            }
            else
            {
                Console.WriteLine("Letzte Synchronisation: nie");
            }
            Console.WriteLine("Aktuelle Zeit: " + current);

            //////////////////////////////////////////////
            /// Building and Executing the Sql String ///
            ////////////////////////////////////////////
            queryCommand.Prepare();
            queryReader = queryCommand.ExecuteReader();

            Console.WriteLine("Reading dbo.Regie");
            LogfileHandler.Log("Reading dbo.Regie");
            ///////////////////////////////////
            /// Reading Sql Query Response ///
            /////////////////////////////////
            while (queryReader.Read())
            {
                //add new Project to Projects List
                projects.Add(new Models.Project
                {
                    ProjAnlage = queryReader["ProjAnlage"].ToString(),
                    ProjNr = queryReader["ProjNr"].ToString(),
                    ProjBezeichnung = queryReader["ProjBezeichnung"].ToString(),
                    ProjAdr = queryReader["ProjAdr"].ToString(),
                    BauHrAdr = queryReader["BauHrAdr"].ToString(),
                    RechAdr = queryReader["RechAdr"].ToString(),
                    ProjAdrAnsprechpartnerID = (long)queryReader["fkAnsprechpartnerIDLiefAdr"],
                    BauHrAdrAnsprechpartnerID = (long)queryReader["fkAnsprechpartnerIDKommAdr"],
                    RechAdrAnsprechpartnerID = (long)queryReader["fkAnsprechpartnerIDRechAdr"]
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
                LogfileHandler.Log("Reading dbo.adrAdressen ProjAdr");

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
                LogfileHandler.Log("Reading dbo.adrAdressen BauHrAdr");

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

                //////////////////////////////////////////////
                /// Building and Executing the Sql String ///
                ////////////////////////////////////////////
                queryCommand.Parameters.Clear();
                queryCommand.Parameters.Add("@AdrNrGes", SqlDbType.VarChar, projects[i].RechAdr.Length).Value = projects[i].RechAdr;
                queryCommand.Prepare();
                queryReader = queryCommand.ExecuteReader();

                Console.WriteLine("Reading dbo.adrAdressen RechAdr");
                LogfileHandler.Log("Reading dbo.adrAdressen RechAdr");

                ///////////////////////////////////
                /// Reading Sql Query Response ///
                /////////////////////////////////
                while (queryReader.Read())
                {
                    projects[i].RechAdrAnredeID = (int)queryReader["Anrede"];
                    projects[i].RechAdrName = queryReader["Name"].ToString();
                    projects[i].RechAdrVorname = queryReader["Vorname"].ToString();
                    projects[i].RechAdrStrasse = queryReader["Strasse"].ToString();
                    projects[i].RechAdrOrtOrtID = (int)queryReader["Ort"];
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
                LogfileHandler.Log("Reading dbo.adrAnreden ProjAdrAnrede");
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

                Console.WriteLine("Reading dbo.adrAnreden BauHrAdrAnrede");
                LogfileHandler.Log("Reading dbo.adrAnreden BauHrAdrAnrede");
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

                //////////////////////////////////////////////
                /// Building and Executing the Sql String ///
                ////////////////////////////////////////////
                queryCommand.Parameters.Clear();
                queryCommand.Parameters.Add("@AnredeID", SqlDbType.Int, projects[i].RechAdrAnredeID.ToString().Length).Value = projects[i].RechAdrAnredeID;
                queryCommand.Prepare();
                queryReader = queryCommand.ExecuteReader();

                Console.WriteLine("Reading dbo.adrAnreden RechAdrAnrede");
                LogfileHandler.Log("Reading dbo.adrAnreden RechAdrAnrede");
                ///////////////////////////////////
                /// Reading Sql Query Response ///
                /////////////////////////////////
                while (queryReader.Read())
                {
                    projects[i].RechAdrAnrede = queryReader["Anrede"].ToString();
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
                queryCommand.CommandText = "Select * From dbo.adrKontakte Where AdrNrGes = @AdrNrGes AND AnsprechpartnerID = -1;";
                queryCommand.Parameters.Add("@AdrNrGes", SqlDbType.VarChar, projects[i].ProjAdr.Length).Value = projects[i].ProjAdr;
                queryCommand.Prepare();
                queryReader = queryCommand.ExecuteReader();
                Console.WriteLine("Reading dbo.adrKontakte ProjAdrEmail and ProjAdrTel");
                LogfileHandler.Log("Reading dbo.adrKontakte ProjAdrEmail and ProjAdrTel");
                ///////////////////////////////////
                /// Reading Sql Query Response ///
                /////////////////////////////////
                while (queryReader.Read())
                {
                    switch ((int)queryReader["KontaktArt"])
                    {
                        case 0:
                            if (!DBNull.Value.Equals(queryReader["TelNrN"]) && queryReader["TelNrN"].ToString() != "-1")
                            {
                                projects[i].ProjAdrTelNr = queryReader["TelNrN"].ToString();
                            }
                            break;
                        case 2:
                            if (!DBNull.Value.Equals(queryReader["Kontakt"]) && queryReader["Kontakt"].ToString().Length >= 5)
                            {
                                projects[i].ProjAdrEmail = queryReader["Kontakt"].ToString();
                            }
                            break;
                        case 4:
                            if (String.IsNullOrEmpty(projects[i].ProjAdrTelNr) && queryReader["TelNrN"].ToString().Length != 0 && queryReader["TelNrN"].ToString() != "-1")
                            {
                                projects[i].ProjAdrTelNr = queryReader["TelNrN"].ToString();
                            }
                            break;

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
                queryCommand.Parameters.Add("@AdrNrGes", SqlDbType.VarChar, projects[i].BauHrAdr.Length).Value = projects[i].BauHrAdr;
                queryCommand.Prepare();
                queryReader = queryCommand.ExecuteReader();

                Console.WriteLine("Reading dbo.adrKontakte");
                LogfileHandler.Log("Reading dbo.adrKontakte");
                ///////////////////////////////////
                /// Reading Sql Query Response ///
                /////////////////////////////////
                while (queryReader.Read())
                {

                    switch ((int)queryReader["KontaktArt"])
                    {
                        case 0:
                            if (queryReader["TelNrN"].ToString() != "-1")
                            {
                                projects[i].BauHrAdrTelNr = queryReader["TelNrN"].ToString();
                            }
                            break;
                        case 2:
                            if (queryReader["Kontakt"].ToString().Length >= 5)
                            {
                                projects[i].BauHrAdrEmail = queryReader["Kontakt"].ToString();
                            }
                            break;
                        case 4:
                            if (!String.IsNullOrEmpty(projects[i].BauHrAdrTelNr) && queryReader["TelNrN"].ToString() != "-1")
                            {
                                projects[i].BauHrAdrTelNr = queryReader["TelNrN"].ToString();
                            }
                            break;

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
                queryCommand.Parameters.Add("@AdrNrGes", SqlDbType.VarChar, projects[i].RechAdr.Length).Value = projects[i].RechAdr;
                queryCommand.Prepare();
                queryReader = queryCommand.ExecuteReader();

                Console.WriteLine("Reading dbo.adrKontakte");
                LogfileHandler.Log("Reading dbo.adrKontakte");
                ///////////////////////////////////
                /// Reading Sql Query Response ///
                /////////////////////////////////
                while (queryReader.Read())
                {

                    switch ((int)queryReader["KontaktArt"])
                    {
                        case 0:
                            if (queryReader["TelNrN"].ToString() != "-1")
                            {
                                projects[i].RechAdrTelNr = queryReader["TelNrN"].ToString();
                            }
                            break;
                        case 2:
                            if (queryReader["Kontakt"].ToString().Length >= 5)
                            {
                                projects[i].RechAdrEmail = queryReader["Kontakt"].ToString();
                            }
                            break;
                        case 4:
                            if (!String.IsNullOrEmpty(projects[i].BauHrAdrTelNr) && queryReader["TelNrN"].ToString() != "-1")
                            {
                                projects[i].RechAdrTelNr = queryReader["TelNrN"].ToString();
                            }
                            break;

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
                LogfileHandler.Log("Reading dbo.adrOrte");
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
                queryCommand.Parameters.Add("@OrtID", SqlDbType.Int, projects[i].RechAdrOrtOrtID.ToString().Length).Value = projects[i].RechAdrOrtOrtID;
                queryCommand.Prepare();
                queryReader = queryCommand.ExecuteReader();

                Console.WriteLine("Reading dbo.adrOrte");
                LogfileHandler.Log("Reading dbo.adrOrte");
                ///////////////////////////////////
                /// Reading Sql Query Response ///
                /////////////////////////////////
                while (queryReader.Read())
                {
                    projects[i].RechAdrOrtLand = queryReader["Land"].ToString();
                    projects[i].RechAdrOrtPLZ = queryReader["PLZ"].ToString();
                    projects[i].RechAdrOrtOrt = queryReader["Ort"].ToString();
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
                LogfileHandler.Log("To many Projects since last Sync");
                System.Threading.Thread.Sleep(5000);
                Environment.Exit(1);
            }

            for (var i = 0; i < projects.Count; i++)
            {
                ///////////////////////////
                /// Print Project Infos ///
                ///////////////////////////
                Console.WriteLine("Projekt: " + projects[i].ProjNr + " | " + projects[i].ProjBezeichnung);
            }
            return projects;
        }

        public static Models.ContactPerson GetContactPersonById(SqlConnection cnn, long ansprechpartnerID)
        {

            Models.ContactPerson contactPerson = new Models.ContactPerson();
            SqlCommand queryCommand;
            SqlDataReader queryReader;

            //////////////////////////////////////////////////
            /// Try to Read all Projects from the Database ///
            //////////////////////////////////////////////////
            queryCommand = new SqlCommand(null, cnn);
            queryCommand.CommandText = "Select * From dbo.adrAnsprechpartner Where AnsprechaprtnerID = @id;";
            queryCommand.Parameters.Add("@id", SqlDbType.BigInt, ansprechpartnerID.ToString().Length).Value = ansprechpartnerID;

            //////////////////////////////////////////////
            /// Building and Executing the Sql String ///
            ////////////////////////////////////////////
            queryCommand.Prepare();
            queryReader = queryCommand.ExecuteReader();

            Console.WriteLine("Reading dbo.adrAnsprechpartner");
            LogfileHandler.Log("Reading dbo.adrAnsprechpartner");
            ///////////////////////////////////
            /// Reading Sql Query Response ///
            /////////////////////////////////
            while (queryReader.Read())
            {
                contactPerson.Nachname = queryReader["Nachname"].ToString();
                contactPerson.Vorname = queryReader["Vorname"].ToString();
                contactPerson.AnredeID = (int)queryReader["Anrede"];
                contactPerson.Zusatz = queryReader["Zusatz"].ToString();
                contactPerson.Bemerkung = queryReader["Bemerkung"].ToString();
                contactPerson.AdrNrGes = queryReader["AdrNrGes"].ToString();
            }
            //closing reader
            queryReader.Close();

            //////////////////////////////////////////////////
            /// Try to Read all Projects from the Database ///
            //////////////////////////////////////////////////
            queryCommand = new SqlCommand(null, cnn);
            queryCommand.CommandText = "Select * From dbo.adrKontakte Where AnsprechaprtnerID = @id;";
            queryCommand.Parameters.Add("@id", SqlDbType.BigInt, ansprechpartnerID.ToString().Length).Value = ansprechpartnerID;

            //////////////////////////////////////////////
            /// Building and Executing the Sql String ///
            ////////////////////////////////////////////
            queryCommand.Prepare();
            queryReader = queryCommand.ExecuteReader();

            Console.WriteLine("Reading dbo.adrKontakte");
            LogfileHandler.Log("Reading dbo.adrKontakte");
            ///////////////////////////////////
            /// Reading Sql Query Response ///
            /////////////////////////////////
            while (queryReader.Read())
            {
                if ((int)queryReader["KontaktArtTyp"] == 1)
                {
                    switch ((int)queryReader["KontaktArt"])
                    {
                        case 0:
                            if (queryReader["TelNrN"].ToString() != "-1")
                            {
                                contactPerson.Telefonnummer = queryReader["TelNrN"].ToString();
                            }
                            break;
                        case 2:
                            if (queryReader["Kontakt"].ToString().Length >= 5)
                            {
                                contactPerson.Email = queryReader["Kontakt"].ToString();
                            }
                            break;
                        case 4:
                            if (!String.IsNullOrEmpty(contactPerson.Telefonnummer) && queryReader["TelNrN"].ToString() != "-1")
                            {
                                contactPerson.Telefonnummer = queryReader["TelNrN"].ToString();
                            }
                            break;

                    }
                }else if((int)queryReader["KontaktArtTyp"] == 1)
                {
                    switch ((int)queryReader["KontaktArt"])
                    {
                        case 0:
                            if (queryReader["TelNrN"].ToString() != "-1")
                            {
                                contactPerson.TelefonnummerPrivat = queryReader["TelNrN"].ToString();
                            }
                            break;
                        case 2:
                            if (queryReader["Kontakt"].ToString().Length >= 5)
                            {
                                contactPerson.EmailPrivat = queryReader["Kontakt"].ToString();
                            }
                            break;
                        case 4:
                            if (!String.IsNullOrEmpty(contactPerson.Telefonnummer) && queryReader["TelNrN"].ToString() != "-1")
                            {
                                contactPerson.TelefonnummerPrivat = queryReader["TelNrN"].ToString();
                            }
                            break;

                    }
                }
            }
            //closing reader
            queryReader.Close();


            //////////////////////////////////////////////
            /// Building and Executing the Sql String ///
            ////////////////////////////////////////////
            queryCommand = new SqlCommand(null, cnn);
            queryCommand.CommandText = "Select * From dbo.adrAnreden Where AnredeID = @AnredeID;";
            queryCommand.Parameters.Add("@AnredeID", SqlDbType.Int, contactPerson.AnredeID.ToString().Length).Value = contactPerson.AnredeID;
            queryCommand.Prepare();
            queryReader = queryCommand.ExecuteReader();

            Console.WriteLine("Reading dbo.adrAnreden");
            LogfileHandler.Log("Reading dbo.adrAnreden");
            ///////////////////////////////////
            /// Reading Sql Query Response ///
            /////////////////////////////////
            while (queryReader.Read())
            {
                contactPerson.Anrede = queryReader["Anrede"].ToString();
            }
            ////////////////////////////////////
            /// Closing Query Return Reader ///
            //////////////////////////////////
            queryReader.Close();
            return contactPerson;
        }
    }
}
