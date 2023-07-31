using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WpfRasp.Models;

namespace WpfRasp
{
    public static class ConfigManager
    {
        public static Config config { get; private set; }
        public const string CONFIGPATH = "config.json";

        public static int ReadConf() 
        {
            try 
            {
                using (StreamReader sr = new StreamReader(CONFIGPATH))
                {
                    string data = sr.ReadToEnd();
                    if (data == null || data == "")
                        return -1;

                    Config cfg = JsonSerializer.Deserialize<Config>(data);

                    config = cfg;

                    return 0;
                }
            }catch(IOException e) 
            {
                return -1;
            }
        }

        public static int EditFacultiesConf(string newFaculty) 
        {
            config.FacultiesNames.Add(newFaculty, "Новый факультет");
            return 0;
        }
    }
}
