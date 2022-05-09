﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Neo4j.Driver;
using System.Configuration;
using System.Collections.Specialized;

namespace Script
{
    //https://wiki.archlinux.org/title/.NET
    //Config file: https://www.c-sharpcorner.com/article/four-ways-to-read-configuration-setting-in-c-sharp/ 
    //https://docs.microsoft.com/en-us/dotnet/api/system.configuration.configurationmanager?view=dotnet-plat-ext-6.0 
    //Segmentation Fault Problem Solution --> Use pacman to install .NET Core SDK and Runtime and not Snap since it is prone to this error.

    public class Program
    {
        private static IDriver _driver;
        static async Task Main(string[] args)   //Kommune = 0, Password = 1
        {
            await InitAsync(); 
        }

        //TODO: Fix the args spot to fix to database location. 
        private static async Task InitAsync()
        {
            try{    
                var searchSettings = ConfigurationManager.GetSection("SearchSettings") as NameValueCollection; 
                var connectionString = ConfigurationManager.GetSection("DatabaseSettings") as NameValueCollection;
                if(searchSettings.Count == 0)
                {
                    Console.WriteLine("SearchSettings was empty!"); 
                }
                else 
                {
                    foreach(var key in searchSettings.AllKeys)
                    {
                        //TODO: Get all the keys and place them in the right spot. 
                        Console.WriteLine(key + " = " + searchSettings[key]);
                    }
                }
                if(connectionString.Count == 0)
                {
                    Console.WriteLine("DatabaseSettings was empty!");
                } 
                else 
                {
                    foreach(var key in connectionString.AllKeys)
                    {
                        //TODO: Get all the keys and place them in the right spot. 
                        Console.WriteLine(key + " = " + connectionString[key]);
                    }
                }
            } catch(ConfigurationErrorsException)
            {
                Console.WriteLine("Error reading app settings..."); 
            }

            //TODO: Get the values and save them in these properties. 
            //_driver = GraphDatabase.Driver(_neo4J_Uri, AuthTokens.Basic(user, password));
            
            while(true)
            {
                await Search_Kulturarv_Async();
                await Search_Europeana_Database(); 
                //TODO: Break this loop if admin says so. 
            }
        }

        private async static Task Search_Kulturarv_Async(){ //TODO: Make a generic search method instead. 
            string Host = "https://www.kulturarv.dk/";      //TODO: Change to global variable. 
            string Path = "fundogfortidsminder/Lokalitet/"; 
            int Search_Point = 1;
            int Max_Search_Point = 3000; //TODO: Should be changed.
            HttpClient client = new HttpClient();  
           
                if(Search_Point <= Max_Search_Point){
                    string uri = Host + Path + Search_Point.ToString();
                    HttpResponseMessage response = await client.GetAsync(uri);
                    string contentString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(contentString);

                    await Strip_Text_From_HTML_RegEx_KulturarvAsync(contentString); 
                }   
                else {
                    Search_Point = 1; 
                }
        }

        private async static Task Search_Europeana_Database()
        {
            string Host = "";   //TODO: Mangler. EDM --> Undersøg. 
            string Path = ""; 
            int Search_Point = 1; 
            int Max_Search_Point = 3000; 
            HttpClient client = new HttpClient(); 
          
                if(Search_Point <= Max_Search_Point)
                {
                    string uri = Host + Path + ""; 
                    HttpResponseMessage response = await client.GetAsync(uri);
                    string contentString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(contentString);

                    await Strip_Text_From_HTML_RegEx_KulturarvAsync(contentString);
                }
            

        }

        private static async Task Cleanup_And_Prepare_Europeana_Data_For_DatabaseAsync(string data)
        {
            //TODO: Recognize data. 
            //Validate data. --> Language, content and maybe other missing parameters. 
            string result = ""; 
            Save_Result_To_Neo4J(result); 
        }

        private static async Task Strip_Text_From_HTML_RegEx_KulturarvAsync(string data){
            string RegEx = "(<!-- Start free text for location -->)"; 
            string Result = "";
            Regex re = new Regex(RegEx, RegexOptions.None);

            //TODO: Check HTML document with regex. 

            Result = re.ToString();  
            Save_Result_To_Neo4J(Result); 
        }

        private static void Save_Result_To_Neo4J(string document)
        {
            using (var session = _driver.AsyncSession())
            {
                var return_message = session.WriteTransactionAsync(async x =>
                {
                    var result = await x.RunAsync("CREATE (a:Greeting) " +
                                        "SET a.message = $message " +
                                        "RETURN a.message + ', from node ' + id(a)",
                        new { document });
                    return result;
                });
                Console.WriteLine("Return message: " + return_message);
            }
        }

        private static void Check_Database_Connection()
        {
            bool connected = false; 
            while(!connected)
            {
                //Console.WriteLine("Connection to " + _neo4J_Uri + " could not be completed!\n Waiting two seconds and trying again...");
                System.Threading.Thread.Sleep(1000);
                //TODO: Check database connection to Neo4J.
            }
        }

        private static void Pause_Program()
        {
            //TODO: Pause program command. 
            //Use while(true) until user presses enter. Then start the program over. Remember properties. 
        }   


        //TODO: Make able to call this. 
        static void AddUpdateAppSettings(string key, string value)  
        {  
            try  
            {  
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);  
                var settings = configFile.AppSettings.Settings;  
                if (settings[key] == null)  
                {  
                    settings.Add(key, value);  
                }  
                else  
                {  
                    settings[key].Value = value;  
                }  
                configFile.Save(ConfigurationSaveMode.Modified);  
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);  
            }  
            catch (ConfigurationErrorsException)  
            {  
                Console.WriteLine("Error writing app settings");  
            }  
        }  
    }
}
