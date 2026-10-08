using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace StatLib
{
    public static class SeasonProvider
    {
        public static string GetResourceText(string fileName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(fileName);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        public static Dictionary<string, List<Game>> ReturnSeasonContent()
        {
            Dictionary<string, List<Game>> seasonsList = new Dictionary<string, List<Game>>();
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = assembly.GetManifestResourceNames();

            foreach (string resource in resourceNames)
            {
                if (!resource.EndsWith(".csv"))
                    continue;

                string[] parts = resource.Split(".");
                string season = parts[2];
                string csvText = GetResourceText(resource);
                seasonsList[season] = GameParser.ParseCsv(csvText, season);
            }

            return seasonsList;
        }
    }
}
