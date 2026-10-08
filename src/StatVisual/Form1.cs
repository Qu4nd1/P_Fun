using StatLib;

namespace StatVisual
{
    public partial class Form1 : Form
    {
        private ScottPlot.WinForms.FormsPlot statPlot = new ScottPlot.WinForms.FormsPlot();
        private Dictionary<string, List<Game>> seasonsList = SeasonProvider.ReturnSeasonContent();
        private ComboBox cmbSeason = new ComboBox();
        private ComboBox cmbStat = new ComboBox();
        private FlowLayoutPanel topBar = new FlowLayoutPanel();

        private static readonly string[] SeasonTypes = { "Regular Season", "Play-In", "Playoffs" };

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

        public Form1()
        {
            InitializeComponent();

            statPlot.Dock = DockStyle.Fill;
            Controls.Add(statPlot);

            topBar.Dock = DockStyle.Top;
            topBar.AutoSize = true;
            topBar.Controls.Add(cmbSeason);
            topBar.Controls.Add(cmbStat);
            Controls.Add(topBar);

            cmbSeason.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStat.DropDownStyle = ComboBoxStyle.DropDownList;

            cmbSeason.Items.AddRange(seasonsList.Keys.OrderBy(k => k).Cast<object>().ToArray());
            cmbStat.Items.AddRange(Stats.Keys.Cast<object>().ToArray());

            cmbSeason.SelectedIndexChanged += Redraw;
            cmbStat.SelectedIndexChanged += Redraw;

            cmbStat.SelectedIndex = 0;                          // Points
            cmbSeason.SelectedIndex = cmbSeason.Items.Count - 1; // latest season, triggers the first draw
        }

        private void Redraw(object? sender, EventArgs e)
        {
            if (cmbSeason.SelectedItem is string season && cmbStat.SelectedItem is string stat)
                DrawStat(season, stat);
        }

        private void DrawStat(string season, string statName)
        {
            var getStat = Stats[statName];
            statPlot.Plot.Clear();

            foreach (string type in SeasonTypes)
            {
                var games = seasonsList[season]
                    .Where(g => g.SeasonType == type)
                    .ToList();

                if (games.Count == 0)
                    continue;

                double[] xs = games.Select(g => (double)g.Id).ToArray();
                double[] ys = games
                    .Select(g => g.Status == "Played" && getStat(g) is int v ? (double)v : double.NaN)
                    .ToArray();

                var scatter = statPlot.Plot.Add.Scatter(xs, ys);
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
    }
}
