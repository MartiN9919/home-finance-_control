using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using Balansic.DB;
using Balansic.DB.Entities;
using Balansic.Pages.Controls.GridView;

namespace Balansic.Pages.Controls
{
    public abstract class MainControl
    {
        public static DataGridViewCellStyle CellTemplate { get; protected set; }
        public static DataGridViewCellStyle CellTemplateAlt { get; protected set; }
        private static MainWindow Window;
        private static AnalysisReport Report;
        private static bool UpdatingSFG = false;

        public static void InitComponents(MainWindow mainWindow)
        {
            Window = mainWindow;
            InitCellTemplates();
            ApplyCellTemplates();
            FillSpendFiltersTable();
            FillIncomeFiltersTable();
            FillSpendItemsTable();
            FillIncomeItemsTable();
            Report = new AnalysisReport(Window);
            Report.Init();

            Window.datePicker.Value = DateTime.Now.Date;

            CalculateMoneyLeft(null, null);
            Window.tabControl.Selecting += (o, e) =>
            {
                switch (e.TabPageIndex)
                {
                    case 0: 
                        UpdateSpendItemsTable(Window.spendGrid, Window.datePicker.Value.Date);
                        break;
                    case 1: 
                        UpdateIncomeItemsTable(Window.incomeGrid, Window.datePicker.Value.Date);
                        break;
                    case 2:
                        TreeViewControl.FillSpendFilters(Window.AnalysisFiltersTree);
                        break;
                }
            };
        }

        private static void InitCellTemplates()
        {
            CellTemplate = new DataGridViewCellStyle();
            CellTemplate.Alignment = DataGridViewContentAlignment.MiddleLeft;
            CellTemplate.WrapMode = DataGridViewTriState.True;
            CellTemplate.Font = new Font("Microsoft Sans Serif", 10);
            CellTemplate.BackColor = Color.White;
            CellTemplate.ForeColor = Color.Black;
            CellTemplate.SelectionBackColor = Color.FromArgb(192, 255, 192);
            CellTemplate.SelectionForeColor = Color.Black;

            CellTemplateAlt = new DataGridViewCellStyle(CellTemplate);
            CellTemplateAlt.BackColor = Color.FromArgb(224, 224, 224);
        }

        private static void ApplyCellTemplates()
        {
            Window.spendFiltersGrid.RowsDefaultCellStyle = MainControl.CellTemplate;
            Window.spendFiltersGrid.AlternatingRowsDefaultCellStyle = MainControl.CellTemplateAlt;

            Window.incomeFiltersGrid.RowsDefaultCellStyle = MainControl.CellTemplate;
            Window.incomeFiltersGrid.AlternatingRowsDefaultCellStyle = MainControl.CellTemplateAlt;

            Window.spendGrid.RowsDefaultCellStyle = MainControl.CellTemplate;
            Window.spendGrid.AlternatingRowsDefaultCellStyle = MainControl.CellTemplateAlt;

            Window.incomeGrid.RowsDefaultCellStyle = MainControl.CellTemplate;
            Window.incomeGrid.AlternatingRowsDefaultCellStyle = MainControl.CellTemplateAlt;
        }

        public static DataGridViewFilterTypeColumn GetFilterTypeColumn(SpendFilterType spendFilterType)
        {
            return spendFilterType == null ? null : new DataGridViewFilterTypeColumn(spendFilterType);
        }

        public static DataGridViewFilterTypeColumn GetFilterTypeColumn(string columnName)
        {
            DataGridViewFilterTypeColumn column = null;
            if (columnName != null && columnName != string.Empty)
            {
                // add in DB
                SpendFilterType filterType = new SpendFilterType(columnName);
                DBManager.CreateOrUpdateItem(filterType);
                DBManager.SpendFilters.Add(filterType, new List<SpendFilter>());

                column = GetFilterTypeColumn(filterType);
            }
            return column;
        }

        private static void FillSpendFiltersTable()
        {
            DataGridView SFG = Window.spendFiltersGrid;
            SFG.ColumnAdded += (o, e) =>
            {
                e.Column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                DataGridView table = o as DataGridView;
                if (table != null)
                {
                    foreach (DataGridViewRow row in table.Rows)
                    {
                        var cell = row.Cells[e.Column.Name] as DataGridViewFilterCell;
                        if (cell != null)
                        {
                            cell.ItemType = typeof(SpendFilter);
                            cell.ItemRemoved += (obj, args) => { UpdateSpendFiltersTable(SFG); };
                        }
                    }
                }
            };
            SFG.RowsAdded += (o, e) =>
            {
                for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
                {
                    foreach (DataGridViewFilterCell cell in SFG.Rows[i].Cells)
                    {
                        cell.ItemType = typeof(SpendFilter);
                        cell.ItemRemoved += (obj, args) => { UpdateSpendFiltersTable(SFG); };
                    }
                }
            };
            SFG.ColumnRemoved += (o, e) => { if (!UpdatingSFG) UpdateSpendFiltersTable(SFG); };
            UpdateSpendFiltersTable(SFG);
        }

        private static void UpdateSpendFiltersTable(DataGridView SFG)
        {
            UpdatingSFG = true;
            SFG.Rows.Clear();
            SFG.Columns.Clear();
            foreach (var filter in DBManager.SpendFilters)
            {
                var column = new DataGridViewFilterTypeColumn(filter.Key);
                SFG.Columns.Add(column);

                int iRow = filter.Value.Count - SFG.Rows.Count + 1;
                if (iRow > 0)
                {
                    SFG.Rows.Add(iRow);
                }
                for (iRow = 0; iRow < filter.Value.Count; iRow++)
                {
                    var row = SFG.Rows[iRow];
                    var cell = row.Cells[column.Name];
                    cell.Value = filter.Value[iRow];
                }
            }
            UpdatingSFG = false;
        }

        private static void FillIncomeFiltersTable()
        {
            DataGridView IFG = Window.incomeFiltersGrid;
            IFG.RowsAdded += (o, e) =>
            {
                for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
                {
                    foreach (DataGridViewFilterCell cell in IFG.Rows[i].Cells)
                    {
                        cell.ItemType = typeof(IncomeFilter);
                    }
                }
            };
            UpdateIncomeFiltersTable(IFG);
        }

        private static void UpdateIncomeFiltersTable(DataGridView IFG)
        {
            IFG.Columns.Clear();
            IFG.Rows.Clear();

            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column.MinimumWidth = 50;
            column.SortMode = DataGridViewColumnSortMode.NotSortable;
            column.MaxInputLength = 128;
            column.CellTemplate = new DataGridViewFilterCell();
            IFG.Columns.Add(column);

            for (int i = 0; i < DBManager.IncomeFilters.Count; i++)
            {
                IFG.Rows.Add();
                if (IFG.Rows[i].Cells.Count > 0)
                {
                    var cell = IFG.Rows[i].Cells[0].Value = DBManager.IncomeFilters[i];
                }
            }
        }

        private static void FillSpendItemsTable()
        {
            DataGridView SIT = Window.spendGrid;
            DateTimePicker dateTimePicker = Window.datePicker;

            var rowTemplate = new DataGridViewSpendRow();
            SIT.RowTemplate = rowTemplate;
            UpdateSpendItemsTable(SIT, dateTimePicker.Value.Date);

            dateTimePicker.ValueChanged += (o, e) => 
            {
                UpdateSpendItemsTable(SIT, dateTimePicker.Value.Date);
            };
            SIT.RowsAdded += (o, e) =>
            {
                for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
                {
                    var row = SIT.Rows[i] as DataGridViewSpendRow;
                    if (row != null)
                    {
                        row.Date = dateTimePicker.Value.Date;
                        row.FillCellsWithValues();
                        row.TotalSpendChanged += CalculateMoneyLeft;
                    }
                }
            };
            SIT.CellValueChanged += (o, e) =>
            {
                var table = o as DataGridView;
                if (table != null)
                {
                    var row = table.Rows[e.RowIndex] as DataGridViewSpendRow;
                    if (row != null && !row.RowIsEditing)
                        row.UpdateCell(e.ColumnIndex);
                }
            };
            SIT.UserDeletingRow += (o, e) =>
            {
                var row = e.Row as DataGridViewSpendRow;
                if (row != null && DB.DBManager.IncomeData.ContainsKey(row.Date.Date) && DB.DBManager.SpendData[row.Date.Date].Remove(row.Item))
                {
                    DB.DBManager.DeleteItem(row.Item);
                }
            };
        }

        private static void UpdateSpendItemsTable(DataGridView SIT, DateTime date)
        {
            SIT.Rows.Clear();
            List<SpendItem> spendItems;
            if (DBManager.SpendData.ContainsKey(date.Date))
                spendItems = DBManager.SpendData[date.Date];
            else
            {
                spendItems = new List<SpendItem>();
                DBManager.SpendData.Add(date.Date, spendItems);
            }

            foreach(SpendItem item in spendItems)
            {
                var row = SIT.Rows[SIT.Rows.Add()] as DataGridViewSpendRow;
                row.Item = item;
            }
        }

        private static void FillIncomeItemsTable()
        {
            DataGridView IIT = Window.incomeGrid;
            DateTimePicker datePicker = Window.datePicker;

            var rowTemplate = new DataGridViewIncomeRow();
            IIT.RowTemplate = rowTemplate;
            UpdateIncomeItemsTable(IIT, datePicker.Value.Date);

            datePicker.ValueChanged += (o, e) =>
            {
                UpdateIncomeItemsTable(IIT, datePicker.Value.Date);
            };
            IIT.RowsAdded += (o, e) =>
            {
                for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
                {
                    var row = IIT.Rows[i] as DataGridViewIncomeRow;
                    if (row != null)
                    {
                        row.Date = datePicker.Value.Date;
                        row.FillCellsWithValues();
                        row.IncomeSummaryChanged += CalculateMoneyLeft;
                    }
                }
            };
            IIT.CellValueChanged += (o, e) =>
            {
                var table = o as DataGridView;
                if (table != null)
                {
                    var row = table.Rows[e.RowIndex] as DataGridViewIncomeRow;
                    if (row != null && !row.RowIsEditing)
                        row.UpdateCell(e.ColumnIndex);
                }
            };
            IIT.UserDeletingRow += (o, e) =>
            {
                var row = e.Row as DataGridViewIncomeRow;
                if (row != null && DB.DBManager.IncomeData.ContainsKey(row.Date.Date) && DB.DBManager.IncomeData[row.Date.Date].Remove(row.Item))
                {
                    DB.DBManager.DeleteItem(row.Item);
                }
            };
        }

        private static void UpdateIncomeItemsTable(DataGridView IIT, DateTime date)
        {
            IIT.Rows.Clear();
            if (!DB.DBManager.IncomeData.ContainsKey(date.Date))
            {
                DB.DBManager.IncomeData.Add(date.Date, new List<IncomeItem>());
            }
            List<IncomeItem> incomeItems = DB.DBManager.IncomeData[date.Date];
            foreach (var item in incomeItems)
            {
                var row = IIT.Rows[IIT.Rows.Add()] as DataGridViewIncomeRow;
                if (row != null)
                {
                    row.Item = item;
                }
            }
        }

        private static void CalculateMoneyLeft(object sender, EventArgs eventArgs)
        {
            double income = DB.DBManager.IncomeData.Sum(date => date.Value.Sum(item => item.Summary));
            double spend = DB.DBManager.SpendData.Sum(date => date.Value.Sum(item => item.Price * item.Amount));
            Window.moneyLeftLabel.Text = (income - spend).ToString("C2");
        }
    }

    public delegate void MoneyChangedEventHandler(object source, EventArgs eventArgs);
}
