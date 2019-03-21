using System;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Windows.Forms;
using System.Collections.Generic;

namespace KwpMyCraftnoteProjectSyncAdapter
{
    class ApiHandler
    {
        ///<summary>Creates JObject for a Project and sends a Post request to the Carftnote Api</summary>
        public async static void SendProjects(List<Models.Project> projects, string apikey)
        {
            for (var i = 0; i < projects.Count; i++)
            {

                ////////////////////////////////////////////////
                /// Deleting evry slash from the Phonenumber ///
                ////////////////////////////////////////////////
                if (projects[i].BauHrAdrTelNr != null)
                {
                    projects[i].BauHrAdrTelNr = projects[i].BauHrAdrTelNr.Replace("/", "");
                }

                /////////////////////////////////////////////////
                /// Creating JObject with the Project values ///
                ///////////////////////////////////////////////
                if (projects[i].ProjBezeichnung.Length != 0)
                {
                    JObject json = new JObject(
                     new JProperty("name", projects[i].ProjBezeichnung),
                     new JProperty("orderNumber", projects[i].ProjNr),
                     new JProperty("street", projects[i].ProjAdrStrasse),
                     new JProperty("zipcode", projects[i].ProjAdrOrtPLZ),
                     new JProperty("city", projects[i].ProjAdrOrtOrt),
                     new JProperty("contact",
                      new JArray(
                       new JObject(
                        new JProperty("name", projects[i].BauHrAdrAnrede + " " + projects[i].BauHrAdrVorname + " " + projects[i].BauHrAdrName),
                        new JProperty("email", projects[i].BauHrAdrEmail),
                        new JProperty("phone", projects[i].BauHrAdrTelNr)
                       )
                      )
                     )
                    );
                    LogfileHandler.Log("Json Body: " + json.ToString());

                    if (apikey.Length != 0)
                    {
                        ///////////////////////////////////////////////
                        /// Sending JObject to the MyCraftnote Api ///
                        /////////////////////////////////////////////
                        using (var client = new HttpClient())
                        {
                            String jsonString = "";
                            var response = await client.PostAsync(
                             "https://europe-west1-craftnote-live.cloudfunctions.net/v1/createproject?apikey=" + apikey,
                             new StringContent(json.ToString(), Encoding.UTF8, "application/json"));
                            if (response.IsSuccessStatusCode)
                            {
                                jsonString = await response.Content.ReadAsStringAsync();

                            }
                            ////////////////////////////
                            /// Printing Api Result ///
                            //////////////////////////
                            LogfileHandler.Log("Http respinse: " + response);
                            LogfileHandler.Log("Json Body" + jsonString);
                            try
                            {
                                JObject responseJson = JObject.Parse(jsonString);
                                Console.WriteLine("\nProject Urls: " + responseJson["weblink"] + " || " + responseJson["appDeepLink"]);
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
                    else
                    {
                        Console.WriteLine("kein ApiKey vorhanden");
                        LogfileHandler.Log("No ApiKey available");
                    }
                }
                else
                {
                    Console.WriteLine("Projekt bezeichnung nicht verfügbar");
                    LogfileHandler.Log("No Project Name available");
                }
            }
        }
    }
}
