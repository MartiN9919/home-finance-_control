using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Balansiq.DB;
using Balansiq.DB.Entities;
using Balansiq.Pages.Controls;

namespace Balansiq
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
           
                DBConnector.OpenConnection();
                DBManager.GetAllItems();
                MainControl.InitComponents(this);
                
                // bind events
                this.FormClosing += OnClosing;
            
           
        }

        public void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DBConnector.CloseConnection();
        }

        private void addColumnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filterName = Microsoft.VisualBasic
                .Interaction.InputBox("ведите название нового типа фильтров:", "Добавить столбец", "Новый филтр");
            if (!string.IsNullOrWhiteSpace(filterName))
            {
                var dColumn = MainControl.GetFilterTypeColumn(filterName);
                this.spendFiltersGrid.Columns.Add(dColumn);
            }
        }
    }
}
