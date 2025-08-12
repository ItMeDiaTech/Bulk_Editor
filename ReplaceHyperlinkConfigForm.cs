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
        private readonly HyperlinkReplacementRules _rules;

        // Copy of the original rules for cancel functionality
        private readonly List<HyperlinkReplacementRule> _originalRules;

        public ReplaceHyperlinkConfigForm(HyperlinkReplacementRules rules)
        {
            InitializeComponent();
            _rules = rules;

            // Make a copy of the original rules for cancel functionality
            _originalRules = new List<HyperlinkReplacementRule>();
            foreach (var rule in _rules.Rules)
            {
                _originalRules.Add(new HyperlinkReplacementRule
                {
                    OldTitle = rule.OldTitle,
                    NewTitle = rule.NewTitle,
                    NewFullContentID = rule.NewFullContentID
                });
            }

            InitializeDataGridView();
        }

        // Binding source for better change notifications
        private BindingSource _bindingSource = default!;

        private void InitializeDataGridView()
        {
            // Set up the DataGridView columns
            dataGridViewRules.AutoGenerateColumns = false;

            // Create columns
            var oldTitleColumn = new DataGridViewTextBoxColumn
            {
                Name = "OldTitle",
                HeaderText = "Old Title",
                DataPropertyName = "OldTitle",
                FillWeight = 40 // 40% of the width
            };

            var newTitleColumn = new DataGridViewTextBoxColumn
            {
                Name = "NewTitle",
                HeaderText = "New Title",
                DataPropertyName = "NewTitle",
                FillWeight = 40 // 40% of the width
            };

            var newContentIdColumn = new DataGridViewTextBoxColumn
            {
                Name = "NewFullContentID",
                HeaderText = "New Full Content ID",
                DataPropertyName = "NewFullContentID",
                FillWeight = 20 // 20% of the width (half of what the titles get)
            };

            // Add columns to DataGridView
            dataGridViewRules.Columns.Add(oldTitleColumn);
            dataGridViewRules.Columns.Add(newTitleColumn);
            dataGridViewRules.Columns.Add(newContentIdColumn);

            // Create and configure BindingSource
            _bindingSource = new BindingSource();
            _bindingSource.DataSource = _rules.Rules;

            // Bind data source
            dataGridViewRules.DataSource = _bindingSource;

            // Remove any empty rows that might have been loaded
            RemoveEmptyRows();
        }

        /// <summary>
        /// Checks if a rule is empty (all properties are empty or null)
        /// </summary>
        /// <param name="rule">The rule to check</param>
        /// <returns>True if the rule is empty, false otherwise</returns>
        private static bool IsRuleEmpty(HyperlinkReplacementRule rule)
        {
            return string.IsNullOrWhiteSpace(rule.OldTitle) &&
                   string.IsNullOrWhiteSpace(rule.NewTitle) &&
                   string.IsNullOrWhiteSpace(rule.NewFullContentID);
        }

        /// <summary>
        /// Trims whitespace from all properties of a rule
        /// </summary>
        /// <param name="rule">The rule to trim</param>
        private static void TrimRule(HyperlinkReplacementRule rule)
        {
            if (rule.OldTitle != null)
                rule.OldTitle = rule.OldTitle.Trim();
            if (rule.NewTitle != null)
                rule.NewTitle = rule.NewTitle.Trim();
            if (rule.NewFullContentID != null)
                rule.NewFullContentID = rule.NewFullContentID.Trim();
        }

        /// <summary>
        /// Removes all empty rows from the data source and trims whitespace from non-empty rows
        /// </summary>
        private void RemoveEmptyRows()
        {
            for (int i = _bindingSource.Count - 1; i >= 0; i--)
            {
                var rule = (HyperlinkReplacementRule)_bindingSource[i];

                // Trim whitespace from all properties
                TrimRule(rule);

                // Remove if empty
                if (IsRuleEmpty(rule))
                {
                    _bindingSource.RemoveAt(i);
                }
            }
        }

        private void BtnAddRow_Click(object sender, EventArgs e)
        {
            // Add a new rule to the data source using the BindingSource
            var newRule = new HyperlinkReplacementRule();
            _bindingSource.Add(newRule);

            // Position the cursor on the new row for immediate editing
            dataGridViewRules.CurrentCell = dataGridViewRules.Rows[dataGridViewRules.Rows.Count - 1].Cells[0];
        }

        private void BtnDeleteRow_Click(object sender, EventArgs e)
        {
            if (dataGridViewRules.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridViewRules.SelectedRows)
                {
                    if (!row.IsNewRow)
                    {
                        // Remove from the data source using the BindingSource
                        if (row.DataBoundItem != null)
                        {
                            _bindingSource.Remove(row.DataBoundItem);
                        }
                    }
                }
            }
            else if (dataGridViewRules.CurrentRow != null && !dataGridViewRules.CurrentRow.IsNewRow)
            {
                // Remove from the data source using the BindingSource
                if (dataGridViewRules.CurrentRow.DataBoundItem != null)
                {
                    _bindingSource.Remove(dataGridViewRules.CurrentRow.DataBoundItem);
                }
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            // Remove any empty rows before saving
            RemoveEmptyRows();

            // Data is automatically updated in the bound collection
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            // Restore the original rules
            _rules.Rules.Clear();
            foreach (var rule in _originalRules)
            {
                _rules.Rules.Add(new HyperlinkReplacementRule
                {
                    OldTitle = rule.OldTitle,
                    NewTitle = rule.NewTitle,
                    NewFullContentID = rule.NewFullContentID
                });
            }

            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ReplaceHyperlinkConfigForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Remove any empty rows when the form is closing
            RemoveEmptyRows();
        }
    }
}