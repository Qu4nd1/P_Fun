using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatLib
{
    internal class Model
    {
        static string season;           // Ex: 2016-17
        static string gameId;           // Ex: 1-100
        static DateOnly date;           // Ex: 01-01-2001
        static string seasonType;       // Ex: Regular Season or Playoffs
        static string opponent;         // Ex: Lakers
        static string gamePlace;        // Ex: Home or Away
        static char result;             // Ex: W(win) or L(loss)
        static int teamScore;           // Ex: 100
        static int oppScore;            // Ex: 98
        static string ot;               // Ex: OT or 2OT or null
        static string status;           // Ex: Played or DNP(did not play, injury) or DND(did not dress, no play time)
        static int timePlayed;          // Ex: 30
        static int points;              // Ex: 26
        static int offensiveRebound;    // Ex: 1
        static int deffensiveRebound;   // Ex: 3
        static int rebounds;            // Ex: ORb + DRb
        static int assists;             // Ex: 7
        static int steals;              // Ex: 3
        static int blocks;              // Ex: 1
        static int turnovers;           // Ex: 4
        static int fouls;               // Ex: 3
        static int fieldGoalsMade;      // Ex: 9
        static int fieldGoalsAttempted; // Ex: 18
        static int threePointsMade;     // Ex: 3
        static int threePointsAttempted;// Ex: 10
        static int freeThrowsMade;      // Ex: 5
        static int freeThrowsAttempted; // Ex: 6

    }
}
