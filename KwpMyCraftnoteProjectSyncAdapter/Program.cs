using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KwpMyCraftnoteProjectSyncAdapter
{
    class Program
    {
        static void Main(string[] args)
        {
            // Display the number of command line arguments:
            if (args.Length == 0)
            {
                System.Console.WriteLine("Please enter a numeric argument.");
                System.Console.Beep();
                System.Console.Beep();
                System.Console.Beep();
                System.Environment.Exit(1);
                return;
            }
            else if(args.Length == 1)
            {
                System.Console.Beep();
                //TODO: Open configfile if available

                //check if File exists and is not Empty
                if (File.Exists(args[0].ToString()) && new FileInfo(args[0].ToString()).Length != 0)
                {
                    
                    using (StreamReader r = new StreamReader(args[0].ToString()))
                    {
                        var json = r.ReadToEnd();
                        var filecontent = JObject.Parse(json);
                    }
                }
                else
                {
                    JObject json = new JObject(new JProperty("host","localhost"), new JProperty("username","sa"), new JProperty("password", "kwpsarix"), new JProperty("database", "BNWINS"), new JProperty("instance", "kwp"));
                    using (FileStream fs = File.Create(args[0].ToString()))
                    {
                        Byte[] contents = new UTF8Encoding(true).GetBytes(json.ToString());
                        // Add some information to the file.
                        fs.Write(contents, 0, contents.Length);
                    }
                }
            }
        }
    }
}
