using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using static StatLib.Helpers;

namespace StatLib
{
    public class GameParser
    {
        public List<Game> ParseCsv(string csv, string seasonYears)
        {
            List<Game> season = new List<Game>();
            using var reader = new StringReader(csv);
            string? row;
            reader.ReadLine();
            while ((row = reader.ReadLine()) != null)
            {
                Game game = ParseLine(row, seasonYears);
                season.Add(game);
            }
            return season;
        }
        public Game ParseLine(string line, string seasonYears)
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
