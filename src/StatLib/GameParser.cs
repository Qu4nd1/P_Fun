using System.Globalization;
using static StatLib.Helpers;

namespace StatLib
{
    public static class GameParser
    {
        /// <summary>
        /// Converts the text from a csv to a list of `Game` objects. 
        /// </summary>
        /// <param name="csv">The entire text of the csv.</param>
        /// <param name="seasonYears">The years in which the season took place in a yyyy-yy format.</param>
        /// <returns></returns>
        public static List<Game> ParseCsv(string csv, string seasonYears)
        {
            List<Game> season = new List<Game>();
            using var reader = new StringReader(csv); // Reads line (one by one) of the string csv as if it was a file
            string? row;
            reader.ReadLine(); // First line read but not used beacause its the header line which is not useful to the program
            // Goes on as long as reader.Readline() dosen't read a line with nothing on it (end of csv)
            // Adds each line as a `Game` object to the list season
            while ((row = reader.ReadLine()) != null) // Assigns the line read to `row` and checks if its null or not
            {
                Game game = ParseLine(row, seasonYears);
                season.Add(game);
            }
            return season;
        }
        /// <summary>
        /// Parses the line's data and assigns it to the different fields knowing that the data is always in the same order
        /// </summary>
        /// <param name="line">The string of a specific line</param>
        /// <param name="seasonYears">The years in which the season took place in a yyyy-yy format.</param>
        /// <returns></returns>
        public static Game ParseLine(string line, string seasonYears)
        {
            string[] fields = line.Split(";");
            Game game = new Game();
            game.Season = seasonYears;
            game.Id = Convert.ToInt32(fields[0]);
            game.Date = DateOnly.ParseExact(fields[1], "dd.MM.yyyy", CultureInfo.InvariantCulture);
            game.SeasonType = fields[2];
            game.Opponent = fields[3];
            game.GamePlace = fields[4];
            game.Result = Convert.ToChar(fields[5]);
            game.TeamScore = Convert.ToInt32(fields[6]);
            game.OppScore = Convert.ToInt32(fields[7]);
            game.Ot = ((fields[8] is not "" ? fields[8] : null));
            game.Status = fields[9];
            game.TimePlayed = ParseNullableInt(fields[10]);
            game.Points = ParseNullableInt(fields[11]);
            game.OffensiveRebound = ParseNullableInt(fields[12]);
            game.DefensiveRebound = ParseNullableInt(fields[13]);
            game.Rebounds = ParseNullableInt(fields[14]);
            game.Assists = ParseNullableInt(fields[15]);
            game.Steals = ParseNullableInt(fields[16]);
            game.Blocks = ParseNullableInt(fields[17]);
            game.Turnovers = ParseNullableInt(fields[18]);
            game.Fouls = ParseNullableInt(fields[19]);
            game.FieldGoalsMade = ParseNullableInt(fields[20]);
            game.FieldGoalsAttempted = ParseNullableInt(fields[21]);
            game.ThreePointsMade = ParseNullableInt(fields[22]);
            game.ThreePointsAttempted = ParseNullableInt(fields[23]);
            game.FreeThrowsMade = ParseNullableInt(fields[24]);
            game.FreeThrowsAttempted = ParseNullableInt(fields[25]);

            return game;
        }
    }
}
