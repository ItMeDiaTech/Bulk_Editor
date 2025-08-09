using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bulk_Editor
{
    public partial class ReplaceHyperlinkConfigForm : Form
    {
        private HyperlinkReplacementRules _rules;

        public ReplaceHyperlinkConfigForm(HyperlinkReplacementRules rules)
        {
            InitializeComponent();
            _rules = rules;
            InitializeDataGridView();
        }

        private void InitializeDataGridView()
        {
            // Set up the DataGridView columns
            dataGridViewRules.AutoGenerateColumns = false;

            // Create columns
            var oldTitleColumn = new DataGridViewTextBoxColumn
            {
                Name = "OldTitle",
                HeaderText = "Old Title",
                DataPropertyName = "OldTitle"
            };

            var newTitleColumn = new DataGridViewTextBoxColumn
            {
                Name = "NewTitle",
                HeaderText = "New Title",
                DataPropertyName = "NewTitle"
            };

            var newContentIdColumn = new DataGridViewTextBoxColumn
            {
                Name = "NewFullContentID",
                HeaderText = "New Full Content ID",
                DataPropertyName = "NewFullContentID"
            };

            // Add columns to DataGridView
            dataGridViewRules.Columns.Add(oldTitleColumn);
            dataGridViewRules.Columns.Add(newTitleColumn);
            dataGridViewRules.Columns.Add(newContentIdColumn);

            // Bind data source
            dataGridViewRules.DataSource = _rules.Rules;
        }

        private void BtnAddRow_Click(object sender, EventArgs e)
        {
            // The DataGridView allows adding rows directly, so we don't need to do anything here
            // unless we want custom behavior
        }

        private void BtnDeleteRow_Click(object sender, EventArgs e)
        {
            if (dataGridViewRules.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridViewRules.SelectedRows)
                {
                    if (!row.IsNewRow)
                    {
                        dataGridViewRules.Rows.RemoveAt(row.Index);
                    }
                }
            }
            else if (dataGridViewRules.CurrentRow != null && !dataGridViewRules.CurrentRow.IsNewRow)
            {
                dataGridViewRules.Rows.RemoveAt(dataGridViewRules.CurrentRow.Index);
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            // Data is automatically updated in the bound collection
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}