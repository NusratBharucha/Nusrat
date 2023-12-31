﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nusrat
{
    public partial class Customdatagridview : DataGridView
    {
        private bool inGotoNextControl = false;
        public Customdatagridview()
        {
            InitializeComponent();
            this.AllowUserToAddRows = false;
            this.ColumnHeadersVisible = false;
            this.RowHeadersVisible = false;            
            this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.CellBorderStyle = DataGridViewCellBorderStyle.None;
            this.ScrollBars = ScrollBars.None;
            this.AllowUserToResizeColumns = false;
            this.AllowUserToResizeRows = false;           
            this.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;        
            this.MultiSelect = false;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }
     
        private const int
   VK_TAB = 0x09,
   VK_SHIFT = 0x10;

        private const short
            KEY_PRESSED = 0x80;

        [DllImport("USER32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        private static bool IsKeyPressed(int nVirtKey)
        {
            return (GetKeyState(nVirtKey) & KEY_PRESSED) == KEY_PRESSED;
        }
        protected override void OnEnter(EventArgs e)
        {
            // if no rows present on entering in control , add new row and set the first cell as the active cell 
            if (RowCount == 0)
            {
                TryAddRow();
            }
            if (!inGotoNextControl)
            {
                if (FirstRowIndex >= 0 && FirstColumnIndex >= 0)
                {
                    bool tab = IsKeyPressed(VK_TAB);
                    bool shift = IsKeyPressed(VK_SHIFT);
                    if (tab && !shift)   // --> add
                    {
                        BeginInvoke((Action)(() => {
                            CurrentCell = Rows[FirstRowIndex].Cells[FirstColumnIndex];
                        }));
                    }
                    else if (tab && shift)
                    {
                        BeginInvoke((Action)(() => {
                            CurrentCell = Rows[LastRowIndex].Cells[FirstColumnIndex];
                        }));
                    }
                }
            }
            base.OnEnter(e);
        }
        protected override void OnCellEnter(DataGridViewCellEventArgs e)
        {
            DataGridViewColumn column = Columns[e.ColumnIndex];
            if (!column.ReadOnly && column is DataGridViewTextBoxColumn)
            {
                BeginEdit(false); // false means don't select if if has some text 
                ((TextBox)this.EditingControl).SelectionStart = 0; // always start with index 0 
            }
            base.OnCellEnter(e);
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            int currentrowposition = CurrentCell.RowIndex;
            if (keyData == Keys.Enter || keyData == Keys.Tab)
            {
                var current = CurrentCell;
                if (current != null)
                {
                    if (IsCurrentCellDirty) // fires if the cell state is changed by typing 
                    {
                        CommitEdit(DataGridViewDataErrorContexts.Commit);
                        EndEdit(DataGridViewDataErrorContexts.Commit);
                    }
                    if (IsCurrentCellInEditMode)
                    {
                        EndEdit(DataGridViewDataErrorContexts.Commit);
                    }
                    int row = current.RowIndex;
                    int col = current.ColumnIndex;
                    if (row == LastRowIndex)
                    {

                        if (col == FirstVisibleColumn.Index)
                        {
                            if (Rows[row].IsNewRow || string.IsNullOrEmpty(CurrentCell.Value?.ToString()) || CurrentCell.Value.ToString() == "End of list")
                            {
                                this.Rows.RemoveAt(currentrowposition);// remove the last committed row
                                this.CurrentCell = null; // deselect the current cell 
                                return GotoNextControl();
                            }
                        }
                    }
                    if (col == LastVisibleColumn.Index) // if reached last cell of current row 
                    {
                        if (row == LastRowIndex)
                        {
                            if (TryAddRow())
                            {
                                CurrentCell = this[FirstVisibleColumn.Index, RowCount - 1];
                                return true;
                            }
                        }
                        else
                        {
                            CurrentCell = this[FirstVisibleColumn.Index, row + 1];
                            return true;
                        }

                    }
                }
                return ProcessTabKey(Keys.Tab);
            }
            else if (keyData == Keys.Back || keyData == (Keys.Tab | Keys.Shift))// && (string.IsNullOrEmpty(CurrentCell.Value?.ToString()) && string.IsNullOrEmpty(this.CurrentCell.EditedFormattedValue.ToString())))
            {
                var current = CurrentCell;
                if (current != null)
                {
                    // if (((TextBox)this.EditingControl).SelectionStart == 0)
                    if (this.EditingControl is DataGridViewTextBoxEditingControl textBox)
                    {
                        if (textBox.SelectionStart == 0)
                        {
                            int row = current.RowIndex;
                            int previousrow = current.RowIndex - 1;
                            int col = current.ColumnIndex;
                            if (row == FirstRowIndex) //if current row is first row 
                            {
                                if (col == FirstVisibleColumn.Index) // and if current column is first column 
                                {
                                    if (string.IsNullOrEmpty(CurrentCell.Value?.ToString()) || CurrentCell.Value.ToString() == "End of list")
                                    {
                                        this.Rows.RemoveAt(currentrowposition);// remove the last committed row
                                        this.CurrentCell = null; // deselect the current cell 
                                        return Gotopreviouscontrol();
                                    }
                                    this.CurrentCell = null; // deselect the current cell 
                                    return Gotopreviouscontrol();
                                }
                            }
                            if (col == FirstVisibleColumn.Index) // if reached first cell of current row 
                            {
                                if (!IsCurrentCellDirty) // fires if the cell state is changed by typing 
                                {
                                    if (string.IsNullOrEmpty(CurrentCell.Value?.ToString()))
                                    {
                                        this.Rows.RemoveAt(currentrowposition);// remove the last committed row
                                        CurrentCell = this[LastVisibleColumn.Index, previousrow];
                                        return true;
                                    }
                                    CurrentCell = this[LastVisibleColumn.Index, previousrow];
                                    return true;
                                }
                                else if (IsCurrentCellDirty)
                                {
                                    if (string.IsNullOrEmpty(this.CurrentCell.EditedFormattedValue.ToString()))
                                    {
                                        CommitEdit(DataGridViewDataErrorContexts.Commit);
                                        EndEdit(DataGridViewDataErrorContexts.Commit);
                                        this.Rows.RemoveAt(currentrowposition);// remove the last committed row
                                        CurrentCell = this[LastVisibleColumn.Index, previousrow];
                                        return true;
                                    }
                                    return false;
                                }

                            }
                        }
                        //else if (((TextBox)this.EditingControl).SelectionStart > 0)
                        else if (textBox.SelectionStart > 0)
                        {
                            //Normal deletion logic, call the ProcessCmdKey method of the base class and return its result
                            return base.ProcessCmdKey(ref msg, keyData);
                        }
                    }
                }
                return ProcessTabKey(Keys.Shift | Keys.Tab);
            }
            return base.ProcessCmdKey(ref msg, keyData); // If it is not the desired shortcut key, call the method of the base class for default processing

        }
        protected bool TryAddRow()
        {
            try
            {
                EndEdit();
                Rows.Add();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        protected int FirstRowIndex
        {
            get
            {
                return Rows.GetFirstRow(DataGridViewElementStates.Visible);
            }
        }
        protected int LastRowIndex
        {
            get
            {
                return Rows.GetLastRow(DataGridViewElementStates.Visible);
            }
        }
        protected int FirstColumnIndex
        {
            get
            {
                if (FirstVisibleColumn != null)
                {
                    return FirstVisibleColumn.Index;
                }
                return -1;
            }
        }

        #region Function to return first visible column in datagridview 
        protected DataGridViewColumn FirstVisibleColumn
        {
            get
            {
                return Columns.GetFirstColumn(
                    DataGridViewElementStates.Visible,
                    DataGridViewElementStates.None);
            }
        }
        #endregion

        #region Function to return Last visible column in datagridview
        protected DataGridViewColumn LastVisibleColumn
        {
            get
            {
                return Columns.GetLastColumn(
                    DataGridViewElementStates.Visible,
                    DataGridViewElementStates.None);
            }
        }
        #endregion

        #region Function to go to next control 
        protected bool GotoNextControl()
        {
            inGotoNextControl = true;
            bool result = Parent.SelectNextControl(this, true, true, true, true);
            inGotoNextControl = false;
            return result;
        }
        #endregion


        protected bool Gotopreviouscontrol()
        {
            bool result = Parent.SelectNextControl(this, false, true, true, true);
            return result;
        }

    }
}
