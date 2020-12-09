using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Balansic.DB.Entities;

namespace Balansic.Pages.Controls
{
    public class AnalysisReport
    {
        protected MainWindow Window { get; set; }
        protected List<SpendFilter> Selection;
        protected TextAnnotation NoDataAnnotation;
        
        public AnalysisReport(MainWindow window)
        {
            Window = window;
        }

        public void Init()
        {
            TreeViewControl.SetUpTree(Window.AnalysisFiltersTree);
            TreeViewControl.FillSpendFilters(Window.AnalysisFiltersTree);
            InitAnnotations();

            Window.buildButton.Click =  Window.buildButton.Click + StartBuildingReport;
            Window.ParamsDateFrom.ValueChanged =  Window.ParamsDateFrom.ValueChanged +(o, e) =>
            {
                Window.ParamsDateTo.MinDate = Window.ParamsDateFrom.Value;
            };
            Window.ParamsDateTo.ValueChanged += (o, e) =>
            {
                Window.ParamsDateFrom.MaxDate = Window.ParamsDateTo.Value;
            };
            Window.ParamsDateFrom.Value = DateTime.Now.AddMonths(-1);
            Window.ParamsDateTo.Value = DateTime.Now;
        }

        private void StartBuildingReport (object obj, EventArgs args)
        {
            BuildReport();
        }

        protected void BuildReport()
        {
            NoDataAnnotation.Visible = false;
            Selection = TreeViewControl.GetSelection(Window.AnalysisFiltersTree);
            var values = DB.DBManager.GetSpendValuesForPeriod(Selection, Window.ParamsDateFrom.Value, Window.ParamsDateTo.Value);
            var total = values.Values.Sum();
            Chart chart = Window.Chart;

            ChartArea spendChartArea = new ChartArea("Расходы");
            spendChartArea.BackColor = Color.White;
            spendChartArea.AlignmentOrientation = AreaAlignmentOrientations.Vertical;
            chart.ChartAreas.Clear();
            chart.ChartAreas.Add(spendChartArea);

            Legend spendLegend = InitLegend(spendChartArea, "spendLegend", "Фильтры:");
            chart.Legends.Clear();
            chart.Legends.Add(spendLegend);

            chart.Series.Clear();
            Series series = new Series();
            series.Legend = spendLegend.Name;
            series.ChartType = SeriesChartType.Pie;
            series.ChartArea = spendChartArea.Name;
            series.Palette = ChartColorPalette.BrightPastel;
            series.XValueType = ChartValueType.String;
            series.YValuesPerPoint = 1;
            series.YValueType = ChartValueType.Double;
            foreach (var value in values)
            {
                DataPoint point = series.Points.Add(new[] { value.Value });
                double pointLabelValue = value.Value / total;
                point.Label = value.Value > 0 ? pointLabelValue.ToString("P1") : string.Empty;
                point.LabelToolTip = point.LegendToolTip = value.Value.ToString("C2");
                point.LegendText = point.ToolTip = value.Key;
            }
            chart.Series.Add(series);

            if (values.Keys.Count == 0)
            {
                NoDataAnnotation.Visible = true;
            }
        }

        protected Legend InitLegend(ChartArea parent, string name, string title)
        {
            if (parent == null) throw new ArgumentNullException();
            Legend legend = new Legend(name);
            legend.Alignment = StringAlignment.Near;
            legend.AutoFitMinFontSize = 8;
            legend.BorderColor = Color.Black;
            legend.BorderDashStyle = ChartDashStyle.Dot;
            legend.BorderWidth = 1;
            legend.DockedToChartArea = parent.Name;
            legend.Docking = Docking.Right;
            legend.Enabled = true;
            legend.IsDockedInsideChartArea = false;
            legend.IsEquallySpacedItems = true;
            legend.IsTextAutoFit = true;
            legend.LegendStyle = LegendStyle.Table;
            legend.TableStyle = LegendTableStyle.Tall;
            legend.Title = title;
            legend.TitleAlignment = StringAlignment.Near;
            legend.TitleSeparator = LegendSeparatorStyle.GradientLine;
            return legend;
        }

        protected void InitAnnotations()
        {
            NoDataAnnotation = new TextAnnotation();
            NoDataAnnotation.Text = "Нет данных для отображения!";
            NoDataAnnotation.Alignment = ContentAlignment.MiddleCenter;
            NoDataAnnotation.Visible = false;
            NoDataAnnotation.Font = new Font(NoDataAnnotation.Font.FontFamily, 13, FontStyle.Regular);
            NoDataAnnotation.X = 0;
            NoDataAnnotation.Y = 0;
            Window.Chart.Annotations.Add(NoDataAnnotation);
        }
    }
}
