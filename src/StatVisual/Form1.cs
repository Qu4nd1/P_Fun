using StatLib;

namespace StatVisual
{
    /// <summary>
    /// Main window: lets the user pick a season and a stat, and draws it as a chart
    /// </summary>
    public partial class Form1 : Form
    {
        /// <summary>
        /// The chart control that fills the window
        /// </summary>
        private ScottPlot.WinForms.FormsPlot statPlot = new ScottPlot.WinForms.FormsPlot();

        /// <summary>
        /// Every season loaded once at startup, with the season name (ex: 2021-22) as key and its games as value
        /// </summary>
        private Dictionary<string, List<Game>> seasonsList = SeasonProvider.ReturnSeasonContent();

        private ComboBox cmbSeason = new ComboBox();
        private ComboBox cmbStat = new ComboBox();

        /// <summary>
        /// Holds the two comboboxes side by side at the top of the window
        /// </summary>
        private FlowLayoutPanel topBar = new FlowLayoutPanel();

        /// <summary>
        /// Links each line of the chart to the games it was drawn from, in the same order as its points
        /// </summary>
        private Dictionary<ScottPlot.Plottables.Scatter, List<Game>> scatterGames = new();

        /// <summary>
        /// The small box that follows the mouse and shows the details of a game
        /// </summary>
        private ToolTip gameToolTip = new ToolTip();

        /// <summary>
        /// The game currently shown in the tooltip, used to avoid redrawing it on every mouse move
        /// </summary>
        private Game? hoveredGame;

        /// <summary>
        /// The three kinds of games, each drawn as its own colored line
        /// </summary>
        private static readonly string[] SeasonTypes = { "Regular Season", "Play-In", "Playoffs" };

        /// <summary>
        /// Links the name shown in the stat combobox to the function that reads that stat from a game
        /// </summary>
        private static readonly Dictionary<string, Func<Game, int?>> Stats = new()
        {
            ["Points"] = g => g.Points,
            ["Rebounds"] = g => g.Rebounds,
            ["Assists"] = g => g.Assists,
            ["Steals"] = g => g.Steals,
            ["Blocks"] = g => g.Blocks,
            ["Turnovers"] = g => g.Turnovers,
            ["Minutes"] = g => g.TimePlayed,
        };

        /// <summary>
        /// Builds the window, fills the comboboxes and draws the first chart
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            // The plot fills all the space the top bar leaves free
            statPlot.Dock = DockStyle.Fill;
            Controls.Add(statPlot);

            // The top bar is added after the plot so it takes its strip first
            topBar.Dock = DockStyle.Top;
            topBar.AutoSize = true;
            topBar.Controls.Add(cmbSeason);
            topBar.Controls.Add(cmbStat);
            Controls.Add(topBar);

            // DropDownList means the user can only pick from the list, not type
            cmbSeason.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStat.DropDownStyle = ComboBoxStyle.DropDownList;

            // Dictionary keys have no guaranteed order, so the seasons are sorted
            cmbSeason.Items.AddRange(seasonsList.Keys.OrderBy(k => k).Cast<object>().ToArray());
            cmbStat.Items.AddRange(Stats.Keys.Cast<object>().ToArray());

            // Both comboboxes call the same method when their selection changes
            cmbSeason.SelectedIndexChanged += Redraw;
            cmbStat.SelectedIndexChanged += Redraw;
            statPlot.MouseMove += StatPlot_MouseMove;

            cmbStat.SelectedIndex = 0;                          // Points
            cmbSeason.SelectedIndex = cmbSeason.Items.Count - 1; // latest season, triggers the first draw
        }

        /// <summary>
        /// Redraws the chart with the season and stat currently selected
        /// </summary>
        /// <param name="sender">The combobox that changed.</param>
        /// <param name="e">The event data, not used here.</param>
        private void Redraw(object? sender, EventArgs e)
        {
            // Nothing is drawn until both comboboxes have a selection
            if (cmbSeason.SelectedItem is string season && cmbStat.SelectedItem is string stat)
                DrawStat(season, stat);
        }

        /// <summary>
        /// Draws one stat of one season, one colored line per season type
        /// </summary>
        /// <param name="season">The season to draw (ex: 2021-22).</param>
        /// <param name="statName">The name of the stat to draw, one of the keys of Stats.</param>
        private void DrawStat(string season, string statName)
        {
            var getStat = Stats[statName];

            statPlot.Plot.Clear();
            scatterGames.Clear();      // the old lines no longer exist
            hoveredGame = null;
            gameToolTip.Hide(statPlot);

            foreach (string type in SeasonTypes)
            {
                var games = seasonsList[season]
                    .Where(g => g.SeasonType == type)
                    .ToList();

                // Some seasons have no Play-In games, so there is nothing to draw
                if (games.Count == 0)
                    continue;

                // A game that was not played becomes NaN so the chart shows a gap instead of a 0
                double[] xs = games.Select(g => (double)g.Id).ToArray();
                double[] ys = games
                    .Select(g => g.Status == "Played" && getStat(g) is int v ? (double)v : double.NaN)
                    .ToArray();

                var scatter = statPlot.Plot.Add.Scatter(xs, ys);
                scatterGames[scatter] = games;   // remember which games this line was drawn from
                scatter.LegendText = type;
                scatter.Color = type switch
                {
                    "Regular Season" => ScottPlot.Colors.Blue,
                    "Play-In" => ScottPlot.Colors.Orange,
                    _ => ScottPlot.Colors.Red,
                };
                // keep your NaN gap setting here
            }

            statPlot.Plot.ShowLegend();
            statPlot.Plot.Title($"{season}  {statName} per game");
            statPlot.Plot.XLabel("Game #");
            statPlot.Plot.YLabel(statName);
            statPlot.Plot.Axes.AutoScale();   // verify name, so the y axis fits each stat
            statPlot.Refresh();
        }
        /// <summary>
        /// Shows the details of the game under the mouse when it is close to a point of the chart
        /// </summary>
        /// <param name="sender">The chart that raised the event.</param>
        /// <param name="e">The event data, contains the mouse position in pixels.</param>
        private void StatPlot_MouseMove(object? sender, MouseEventArgs e)
        {
            if (cmbStat.SelectedItem is not string statName)
                return;

            // Convert the mouse position from pixels to chart coordinates (game number, stat value)
            ScottPlot.Coordinates mouse = statPlot.Plot.GetCoordinates(new ScottPlot.Pixel(e.X, e.Y));

            foreach (var (scatter, games) in scatterGames)
            {
                // Closest point of this line to the mouse, only real if it is within a few pixels
                ScottPlot.DataPoint nearest = scatter.Data.GetNearest(mouse, statPlot.Plot.LastRender);
                if (!nearest.IsReal)
                    continue;

                // The point index is also the index of its game, because xs and ys were built in the same order as games
                Game game = games[nearest.Index];
                if (game.Status != "Played")
                    continue;

                // Same game as before, the tooltip is already up to date
                if (game == hoveredGame)
                    return;

                hoveredGame = game;
                string value = Stats[statName](game)?.ToString() ?? "-";
                string text = $"{game.Date:dd.MM.yyyy} vs {game.Opponent} ({game.GamePlace})\n" +
                              $"{game.Result} {game.TeamScore}-{game.OppScore}\n" +
                              $"{statName}: {value}";
                gameToolTip.Show(text, statPlot, e.X + 15, e.Y + 15);
                return;
            }

            // Mouse is not close to any point, hide the tooltip
            hoveredGame = null;
            gameToolTip.Hide(statPlot);
        }
    }
}
