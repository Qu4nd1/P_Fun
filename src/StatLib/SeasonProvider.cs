using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace StatLib
{
    /// <summary>
    /// Gives access to every season's games, read from the CSV files embedded in StatLib.dll
    /// </summary>
    public static class SeasonProvider
    {
        /// <summary>
        /// Reads one embedded file
        /// </summary>
        /// <param name="fileName">The name of the file the method will read.</param>
        /// <returns>The whole content of the file as one string.</returns>
        public static string GetResourceText(string fileName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(fileName);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Reads every embedded CSV file and turns each one into a list of games
        /// </summary>
        /// <returns>A dictionary with the season name (ex: 2021-22) as key and the games of that season as value.</returns>
        public static Dictionary<string, List<Game>> ReturnSeasonContent()
        {
            Dictionary<string, List<Game>> seasonsList = new Dictionary<string, List<Game>>();
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = assembly.GetManifestResourceNames();

            foreach (string resource in resourceNames)
            {
                // Ignore anything embedded in the dll that is not a csv file
                if (!resource.EndsWith(".csv"))
                    continue;

                // StatLib.Stat.2021-22.csv becomes ["StatLib", "Stat", "2021-22", "csv"], the season is at index 2
                string[] parts = resource.Split(".");
                string season = parts[2];
                string csvText = GetResourceText(resource);
                seasonsList[season] = GameParser.ParseCsv(csvText, season);
            }

            return seasonsList;
        }
    }
}
