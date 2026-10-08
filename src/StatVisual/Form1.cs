using StatLib;

namespace StatVisual
{
    public partial class Form1 : Form
    {
        private ScottPlot.WinForms.FormsPlot statPlot = new ScottPlot.WinForms.FormsPlot();
        private Dictionary<string, List<Game>> seasonsList = SeasonProvider.ReturnSeasonContent();
        private ComboBox cmbSeason = new ComboBox();
        private static readonly string[] SeasonTypes = { "Regular Season", "Play-In", "Playoffs" };

        public Form1()
        {
            InitializeComponent();

            statPlot.Dock = DockStyle.Fill;
            Controls.Add(statPlot);

            cmbSeason.Dock = DockStyle.Top;
            cmbSeason.DropDownStyle = ComboBoxStyle.DropDownList;
            Controls.Add(cmbSeason);

            cmbSeason.Items.AddRange(seasonsList.Keys.OrderBy(k => k).Cast<object>().ToArray());
            cmbSeason.SelectedIndexChanged += CmbSeason_SelectedIndexChanged;
            cmbSeason.SelectedIndex = cmbSeason.Items.Count - 1;
        }
        private void CmbSeason_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbSeason.SelectedItem is string season)
                DrawPoints(season);
        }

        private void DrawPoints(string season)
        {
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
                    .Select(g => g.Status == "Played" ? (double)g.Points!.Value : double.NaN)
                    .ToArray();

                var scatter = statPlot.Plot.Add.Scatter(xs, ys);
                scatter.LegendText = type;                 // verify name
                scatter.Color = type switch                // verify name
                {
                    "Regular Season" => ScottPlot.Colors.Blue,
                    "Play-In" => ScottPlot.Colors.Orange,
                    _ => ScottPlot.Colors.Red,
                };
                // keep the NaN gap setting you used before on this scatter
            }

            statPlot.Plot.ShowLegend();                    // verify name
            statPlot.Plot.Title($"{season}  Points per game");
            statPlot.Plot.XLabel("Game #");
            statPlot.Plot.YLabel("Points");
            statPlot.Refresh();
        }
    }
}
