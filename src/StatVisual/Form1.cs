using StatLib;

namespace StatVisual
{
    /// <summary>
    /// Main window: lets the user check several seasons and pick a stat, and draws it as a chart
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

        /// <summary>
        /// The list of seasons, each one with a checkbox, so several can be shown at once
        /// </summary>
        private CheckedListBox cklSeasons = new CheckedListBox();

        /// <summary>
        /// The button that looks like a dropdown and opens the list of seasons
        /// </summary>
        private Button btnSeasons = new Button();

        /// <summary>
        /// The first game of the interval shown on the chart
        /// </summary>
        private NumericUpDown nudFrom = new NumericUpDown();

        /// <summary>
        /// The last game of the interval shown on the chart
        /// </summary>
        private NumericUpDown nudTo = new NumericUpDown();

        private Label lblFrom = new Label();
        private Label lblTo = new Label();

        /// <summary>
        /// The red message shown when the first game is higher than the last one
        /// </summary>
        private Label lblIntervalError = new Label();

        /// <summary>
        /// The popup that holds the list of seasons, closes when the user clicks outside it
        /// </summary>
        private ToolStripDropDown seasonsDropDown = new ToolStripDropDown();

        private ComboBox cmbStat = new ComboBox();

        /// <summary>
        /// Holds the stat combobox at the top of the window
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
        /// The ten base colors, one per season
        /// </summary>
        private static readonly ScottPlot.IPalette SeasonPalette = new ScottPlot.Palettes.Category10();

        /// <summary>
        /// The three kinds of games, each drawn as its own line so the chart shows a break between them
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
        /// Builds the window, fills the lists and draws the first chart
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            // The plot is added first so it fills the space the other controls leave free
            statPlot.Dock = DockStyle.Fill;
            Controls.Add(statPlot);

            // The list is not on the form, it lives in a popup opened by the button
            cklSeasons.CheckOnClick = true;   // one click checks the box, no need to select the line first
            cklSeasons.BorderStyle = BorderStyle.None;

            btnSeasons.Width = 140;
            btnSeasons.Click += (s, e) => seasonsDropDown.Show(btnSeasons, 0, btnSeasons.Height);

            // The top bar is added last so it takes its strip first
            topBar.Dock = DockStyle.Top;
            topBar.AutoSize = true;
            topBar.Controls.Add(btnSeasons);
            topBar.Controls.Add(cmbStat);
            // The highest game number of all the seasons, so the interval covers everything by default
            int lastGame = seasonsList.Values.SelectMany(games => games).Max(g => g.Id);

            // The labels show the order the limits have to be entered in: first the lowest, then the highest
            lblFrom.Text = "From game";
            lblFrom.AutoSize = true;
            lblFrom.Margin = new Padding(10, 6, 0, 0);

            lblTo.Text = "to game";
            lblTo.AutoSize = true;
            lblTo.Margin = new Padding(10, 6, 0, 0);

            // The maximum is set before the value, otherwise the value would be capped at the old maximum
            nudFrom.Minimum = 1;
            nudFrom.Maximum = lastGame;
            nudFrom.Value = 1;
            nudFrom.Width = 60;

            nudTo.Minimum = 1;
            nudTo.Maximum = lastGame;
            nudTo.Value = lastGame;
            nudTo.Width = 60;

            lblIntervalError.Text = "The first game must be lower than the last one";
            lblIntervalError.ForeColor = Color.Red;
            lblIntervalError.AutoSize = true;
            lblIntervalError.Margin = new Padding(10, 6, 0, 0);
            lblIntervalError.Visible = false;

            topBar.Controls.Add(lblFrom);
            topBar.Controls.Add(nudFrom);
            topBar.Controls.Add(lblTo);
            topBar.Controls.Add(nudTo);
            topBar.Controls.Add(lblIntervalError);
            Controls.Add(topBar);

            // DropDownList means the user can only pick from the list, not type
            cmbStat.DropDownStyle = ComboBoxStyle.DropDownList;

            // Dictionary keys have no guaranteed order, so the seasons are sorted
            cklSeasons.Items.AddRange(seasonsList.Keys.OrderBy(k => k).Cast<object>().ToArray());
            // The popup is as big as the list, so no scrollbar is needed
            cklSeasons.Width = 110;
            cklSeasons.Height = cklSeasons.Items.Count * cklSeasons.ItemHeight + 4;
            seasonsDropDown.Items.Add(new ToolStripControlHost(cklSeasons) { Padding = Padding.Empty, Margin = Padding.Empty });
            cmbStat.Items.AddRange(Stats.Keys.Cast<object>().ToArray());

            // The defaults are set before the handlers are attached, so they don't trigger a draw
            cmbStat.SelectedIndex = 0;                                    // Points
            cklSeasons.SetItemChecked(cklSeasons.Items.Count - 1, true);  // latest season

            cmbStat.SelectedIndexChanged += Redraw;
            cklSeasons.ItemCheck += CklSeasons_ItemCheck;
            statPlot.MouseMove += StatPlot_MouseMove;
            nudFrom.ValueChanged += Redraw;
            nudTo.ValueChanged += Redraw;

            // The first draw is done by hand because nothing was listening when the defaults were set
            Redraw(null, EventArgs.Empty);
        }

        /// <summary>
        /// Redraws the chart after a season is checked or unchecked
        /// </summary>
        /// <param name="sender">The list that raised the event.</param>
        /// <param name="e">The event data, not used here.</param>
        private void CklSeasons_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            // ItemCheck fires before the box changes state, so the redraw waits until the change is done
            BeginInvoke((Action)(() => Redraw(null, EventArgs.Empty)));
        }

        /// <summary>
        /// Redraws the chart with the seasons, the stat and the interval of games currently selected
        /// </summary>
        /// <param name="sender">The control that changed.</param>
        /// <param name="e">The event data, not used here.</param>
        private void Redraw(object? sender, EventArgs e)
        {
            List<string> seasons = cklSeasons.CheckedItems.Cast<string>().ToList();

            // The button shows the season, or how many are checked
            btnSeasons.Text = seasons.Count switch
            {
                0 => "Seasons \u25BE",
                1 => $"{seasons[0]} \u25BE",
                _ => $"{seasons.Count} seasons \u25BE",
            };

            int from = (int)nudFrom.Value;
            int to = (int)nudTo.Value;

            // With the limits in the wrong order the message is shown and the chart keeps its last drawing
            lblIntervalError.Visible = from > to;
            if (from > to)
                return;

            // Nothing is drawn until a stat is selected
            if (cmbStat.SelectedItem is string stat)
                DrawStat(seasons, stat, from, to);
        }

        /// <summary>
        /// Draws one stat for every checked season, one line per season and per season type, inside the chosen interval of games
        /// </summary>
        /// <param name="seasons">The seasons to draw (ex: 2021-22), can be empty.</param>
        /// <param name="statName">The name of the stat to draw, one of the keys of Stats.</param>
        /// <param name="from">The first game of the interval, included.</param>
        /// <param name="to">The last game of the interval, included.</param>
        private void DrawStat(List<string> seasons, string statName, int from, int to)
        {
            var getStat = Stats[statName];

            // Remove the previous lines so they don't pile up, and forget the games they were linked to
            statPlot.Plot.Clear();
            scatterGames.Clear();
            hoveredGame = null;
            gameToolTip.Hide(statPlot);

            foreach (string season in seasons)
            {
                // The color depends on the position of the season in the whole list, not in the checked ones,
                // so a season keeps the same color whatever else is checked
                ScottPlot.Color color = SeasonPalette.GetColor(cklSeasons.Items.IndexOf(season));

                // Only the first line of a season goes in the legend, the other types have the same color
                bool inLegend = false;

                foreach (string type in SeasonTypes)
                {
                    var games = seasonsList[season]
                        .Where(g => g.SeasonType == type && g.Id >= from && g.Id <= to)
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
                    scatter.Color = color;

                    if (!inLegend)
                    {
                        scatter.LegendText = season;
                        inLegend = true;
                    }

                    // Remember which games this line was drawn from, for the tooltip
                    scatterGames[scatter] = games;
                }
            }

            if (seasons.Count > 0)
                statPlot.Plot.ShowLegend();

            statPlot.Plot.Title($"{string.Join(", ", seasons)}  {statName} per game (games {from} to {to})");
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
                string text = $"{game.Season} {game.SeasonType}\n" +
                              $"{game.Date:dd.MM.yyyy} vs {game.Opponent} ({game.GamePlace})\n" +
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