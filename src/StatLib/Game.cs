using System;
using System.Linq;
using System.Text;

namespace StatLib
{
    public class Game
    {
        public string Season { get; set; }            // Ex: 2016-17
        public int Id { get; set; }               // Ex: 1-100
        public DateOnly Date { get; set; }            // Ex: 01-01-2001
        public string SeasonType { get; set; }        // Ex: Regular Season or Playoffs
        public string Opponent { get; set; }          // Ex: Lakers
        public string GamePlace { get; set; }         // Ex: Home or Away
        public char Result { get; set; }              // Ex: W(win) or L(loss)
        public int TeamScore { get; set; }            // Ex: 100
        public int OppScore { get; set; }             // Ex: 98
        public string? Ot { get; set; }                // Ex: OT or 2OT or null
        public string Status { get; set; }            // Ex: Played or DNP(did not play, injury) or DND(did not dress, no play time)
        public int? TimePlayed { get; set; }          // Ex: 30
        public int? Points { get; set; }              // Ex: 26
        public int? OffensiveRebound { get; set; }    // Ex: 1
        public int? DefensiveRebound { get; set; }   // Ex: 3
        public int? Rebounds { get; set; }            // Ex: ORb + DRb
        public int? Assists { get; set; }             // Ex: 7
        public int? Steals { get; set; }              // Ex: 3
        public int? Blocks { get; set; }              // Ex: 1
        public int? Turnovers { get; set; }           // Ex: 4
        public int? Fouls { get; set; }               // Ex: 3
        public int? FieldGoalsMade { get; set; }      // Ex: 9
        public int? FieldGoalsAttempted { get; set; } // Ex: 18
        public int? ThreePointsMade { get; set; }     // Ex: 3
        public int? ThreePointsAttempted { get; set; }// Ex: 10
        public int? FreeThrowsMade { get; set; }      // Ex: 5
        public int? FreeThrowsAttempted { get; set; } // Ex: 6

        
    }
}
