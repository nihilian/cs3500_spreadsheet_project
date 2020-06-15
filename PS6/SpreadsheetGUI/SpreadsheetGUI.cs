using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpreadsheetUtilities;
using System.Text.RegularExpressions;
using SS;

namespace SpreadsheetGUI
{
    public partial class SpreadsheetGUI : Form
    {
        Spreadsheet spreadsheet;
        string fileName;
        UndoRedoStack editStack; //information about changes to the spreadsheet and how to redo/ undo
        string contentsOfSelection; //the current contents of the selected cell before any changes are made.

        /// <summary>
        /// Constructor for GUI. Also instantiates a new Spreadsheet.
        /// </summary>
        public SpreadsheetGUI()
        {
            InitializeComponent();
            spreadsheet = new Spreadsheet(IsValidVariable, ToUpperCase, "ps6");
            fileName = "";
            editStack = new UndoRedoStack();
            contentsOfSelection = "";
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////// EVENT HANDLERS ///////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Displays initial cell selection (A1) on load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SpreadsheetPanel_Load(object sender, EventArgs e)
        {
            cellName.Text = "A1";
        }

        /// <summary>
        /// Tracks key presses to automatically update cell contents without putting it in focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void SpreadsheetPanel_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Back && cellContents.Text.Length != 0)
            {
                cellContents.Text = cellContents.Text.Substring(0, cellContents.Text.Length - 1);
            }
            if ((int)e.KeyChar >= 32 && (int)e.KeyChar <= 126)
            {
                cellContents.Text += e.KeyChar;
            }
        }

        /// <summary>
        /// Handles keydown event by checking for arrow key press. Arrow keys can navigate the GUI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SpreadsheetPanel_KeyDown(object sender, KeyEventArgs e)
        {
            int col, row;
            spreadsheetPanel.GetSelection(out col, out row);

            switch (e.KeyCode)
            {
                case Keys.Left:
                    spreadsheetPanel.SetSelection(col - 1, row);
                    HandleSelectionChange();
                    break;

                case Keys.Right:
                    spreadsheetPanel.SetSelection(col + 1, row);
                    HandleSelectionChange();
                    break;

                case Keys.Up:
                    spreadsheetPanel.SetSelection(col, row - 1);
                    HandleSelectionChange();
                    break;

                case Keys.Down:
                    spreadsheetPanel.SetSelection(col, row + 1);
                    HandleSelectionChange();
                    break;
            }
        }

        /// <summary>
        /// Sets arrow keys to input keys when pressed. This allows custom arrow key triggers.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SpreadsheetPanel_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Right:
                case Keys.Left:
                case Keys.Up:
                case Keys.Down:
                    e.IsInputKey = true; break;
            }
        }

        /// <summary>
        /// Sets focus to spreadsheet panel when enter is pressed in cell contents box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CellContents_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                spreadsheetPanel.Select();
        }

        /// <summary>
        /// Handles selection changed event. This selects the spreadsheetPanel element to allow navigation with arrow keys.
        /// </summary>
        /// <param name="sender"></param>
        private void SpreadsheetPanel_SelectionChanged(SS.SpreadsheetPanel sender)
        {
            HandleSelectionChange();
        }

        /// <summary>
        /// Tries to update cells whenver contents text is changed. Displays error label if invalid cell contents are entered.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CellContents_TextChanged(object sender, EventArgs e)
        {
            int col, row;
            spreadsheetPanel.GetSelection(out col, out row);
            char letter = (char)('A' + col);
            string name = (letter.ToString() + (row + 1).ToString());
            
            try
            {
                UpdateSingleCell(name, cellContents.Text);
                errorLabel.Visible = false;
            }
            catch (Exception)
            {
                errorLabel.Visible = true;
            }
        }

        /// <summary>
        /// Displays more info when hovered over the red exclamation mark.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ErrorLabel_MouseHover(object sender, EventArgs e)
        {
            if (errorLabel.Visible)
            {
                errorLabel.Text += " Invalid Formula Syntax";
            }
        }

        /// <summary>
        /// Removes the above info when mouse leaves.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ErrorLabel_MouseLeave(object sender, EventArgs e)
        {
            if (errorLabel.Visible)
            {
                errorLabel.Text = "!";
            }
        }

        /// <summary>
        /// Opens a new window with a blank spreadsheet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewButton_Click(object sender, EventArgs e)
        {
            FormCounterApplicationContext appContext = FormCounterApplicationContext.GetAppContext();
            appContext.RunForm(new SpreadsheetGUI());
        }

        /// <summary>
        /// Event handler for when the user clicks Save
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, EventArgs e)
        {
            //If fileName has not been specified open the Save Dialog
            if (fileName == "")
                ShowSaveDialog();
            //Else override the existing file
            else
                spreadsheet.Save(fileName);
        }

        /// <summary>
        /// Opens save dialog box no matter what.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void SaveAsButton_Click(object sender, EventArgs e)
        {
            ShowSaveDialog();
        }

        /// <summary>
        /// Checks if any changes have been saved. Prompts user to save them if not. Opens the open dialog box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void OpenButton_Click(object sender, EventArgs e)
        {
            //Unsaved Changes
            if (spreadsheet.Changed)
            {
                DialogResult saveChanges = SaveChangesBeforeClosingFile();
                if (saveChanges == DialogResult.Yes)
                {
                    if (fileName == "")
                        ShowSaveDialog();
                    else
                        spreadsheet.Save(fileName);
                }
                else if (saveChanges == DialogResult.No)
                {
                    // Do nothing and skip to the code below
                }
                else //If user selects "Cancel"
                {
                    return; //Skip the code below
                }
            }

            //The logic comes here if the user chose "Yes" or "No"

            //Open File Dialog
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.DefaultExt = "sprd";
            openFile.Filter = "sprd files (*.sprd)|*.sprd|All files (*.*)|*.*";
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ClearAllCells();
                    spreadsheet = new Spreadsheet(openFile.FileName, s => true, ToUpperCase, "ps6");
                }
                catch (Exception exception)
                //if there is a problem opening the file, tell the user and just open an empty spreadsheet.
                {
                    MessageBox.Show("There was a problem opening the file:\n" + exception.Message, "", MessageBoxButtons.OK);
                    spreadsheet = new Spreadsheet();
                }
                fileName = openFile.FileName;
                editStack = new UndoRedoStack();
                UpdateCells(spreadsheet.GetNamesOfAllNonemptyCells());
            }
        }

        /// <summary>
        /// Calls the form's close method.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SpreadsheetGUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Unsaved Changes
            if (spreadsheet.Changed)
            {
                DialogResult saveChanges = SaveChangesBeforeClosingFile();
                if (saveChanges == DialogResult.Yes)
                {
                    if (fileName == "")
                        ShowSaveDialog();
                    else
                        spreadsheet.Save(fileName);
                }
                else if (saveChanges == DialogResult.No)
                {
                    // Do nothing and skip to the code below
                }
                else //If user selects "Cancel"
                {
                    e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// Makes use of the the UndoRedo object to revert back to the last change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void UndoButton_Click(object sender, EventArgs e)
        {
            if (contentsOfSelection != cellContents.Text)
                editStack.CellContentsChange(cellName.Text, contentsOfSelection);

            string nameOfCellToChange;
            string newContentsOfCell;
            bool undoSuccessful = editStack.TryUndo(GetCurrentStringContentsOfCell, out nameOfCellToChange, out newContentsOfCell);
            if (undoSuccessful)
            {
                int col, row;
                col = nameOfCellToChange.ElementAt(0) - 'A';
                int.TryParse(nameOfCellToChange.Substring(1), out row);
                row -= 1;
                spreadsheetPanel.SetSelection(col, row);
                cellContents.Text = newContentsOfCell;
                UpdateSingleCell(nameOfCellToChange, newContentsOfCell);
                contentsOfSelection = cellContents.Text;
            }

        }

        /// <summary>
        /// Makes use of UndoRedo class to use redo functionality.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void RedoButton_Click(object sender, EventArgs e)
        {

            string nameOfCellToChange;
            string newContentsOfCell;
            bool redoSuccessful = editStack.TryRedo(GetCurrentStringContentsOfCell, out nameOfCellToChange, out newContentsOfCell);
            if (redoSuccessful)
            {
                int col, row;
                col = nameOfCellToChange.ElementAt(0) - 'A';
                int.TryParse(nameOfCellToChange.Substring(1), out row);
                row -= 1;
                spreadsheetPanel.SetSelection(col, row);
                cellContents.Text = newContentsOfCell;
                UpdateSingleCell(nameOfCellToChange, newContentsOfCell);
                contentsOfSelection = cellContents.Text;
            }

        }

        /// <summary>
        /// Event handled when the Edit menu is dropped down. Determines whether Undo and Redo should be enabled by checking
        /// the contents of each stack.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void EditMenu_DropDownOpened(object sender, EventArgs e)
        {
            redoToolStripMenuItem.Enabled = editStack.CanRedo();
            undoToolStripMenuItem.Enabled = editStack.CanUndo() || contentsOfSelection != cellContents.Text;
        }

        /// <summary>
        /// Opens help dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelpMenu();
        }

        private void SpreadsheetPanel_MouseClick(object sender, MouseEventArgs e)
        {
            IfCellChangedUpdateUndoStack();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////// HELPERS //////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Simple method to normalize cell names to upper case.
        /// Used as a delegate for spreadsheet cell name normalization.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string ToUpperCase(string name)
        {
            return name.ToUpper();
        }

        /// <summary>
        /// Defines the message box that will pop up if an open or close are attempted before saving changes.
        /// </summary>
        /// <returns></returns>
        private static DialogResult SaveChangesBeforeClosingFile()
        {
            return MessageBox.Show("Unsaved changes to file. Do you want " +
                            "to save your changes before closing this file?", "Unsaved Changes", MessageBoxButtons.YesNoCancel);
        }

        /// <summary>
		/// Defines the message box that will pop up when the help tool is clicked.
		/// </summary>
		/// <returns></returns>
        private static DialogResult HelpMenu()
        {
            return MessageBox.Show("Welcome to Gigasoft Excess! This editor allows basic spreadsheet functionality, as well as " +
                "some sick features. \n\nFirst feature: After loading a spreadsheet or saving a new one for the first time, the Save feature" +
                "will remember the filepath, allowing any future saves to work without popping up a dialog box. To specificy a new filepath," +
                "use the Save As option. \n\nSecond feature: Cell values will be updated constantly. No need to even select the contents text" +
                "box! Use the arrow keys to quickly navigate the spreadsheet. Strings beginning with a \"=\" will be treated as a formula. A" +
                "red exclamtion mark will dispay while the formula cannot be compiled. \n\nThird feature: Undo/Redo functionality. Use the edit tool" +
                "menu to select one of these options.", "Help", MessageBoxButtons.OK);
        }

        /// <summary>
        /// Opens Windows Save Dialog. Default filter and extension is for .sprd files.
        /// </summary>
        private void ShowSaveDialog()
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = "sprd";
            saveDialog.Filter = "sprd files (*.sprd)|*.sprd|All files (*.*)|*.*";
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = saveDialog.FileName;
                spreadsheet.Save(fileName);
            }
        }

        /// <summary>
        /// Finds the current contents of a cell in string form. If the contents are a Formula, 
        /// it will prefix the string with "="
        /// This is for use as a delegate in undoing and redoing. 
        /// </summary>
        /// <param name="cellName"></param>
        /// <returns></returns>
        private string GetCurrentStringContentsOfCell(string cellName)
        {
            object contents = spreadsheet.GetCellContents(cellName);
            if (contents is Formula)
            {
                return "=" + contents.ToString();
            }

            else return contents.ToString();
        }

        /// <summary>
        /// Checks to see if the Undo stack should be updated.
        /// </summary>
		private void IfCellChangedUpdateUndoStack()
        {
            string name = cellName.Text;
            string changedContents = cellContents.Text;
            if (changedContents.Length > 0 && changedContents.StartsWith("="))
            {
                try
                {
                    string oldContents = contentsOfSelection;
                    if (oldContents.StartsWith("="))
                    {
                        Formula oldFormula = new Formula(oldContents.Substring(1), ToUpperCase, IsValidVariable);
                        Formula newFormula = new Formula(changedContents.Substring(1), ToUpperCase, IsValidVariable);
                        if (oldFormula != newFormula)
                        {
                            editStack.CellContentsChange(name, contentsOfSelection);
                        }
                    }
                    else
                    {
                        editStack.CellContentsChange(name, contentsOfSelection);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error changing contents:\n" + e.Message, "", MessageBoxButtons.OK);
                }
            }
            else
            {

                if (changedContents != contentsOfSelection)
                {
                    editStack.CellContentsChange(name, contentsOfSelection);
                }
            }
        }
        /// <summary>
        /// Is called whenever a new cell is selected. This will update the current cell name and value text boxes.
        /// </summary>
        private void SelectionChange()
        {
            int col, row;
            string value;
            spreadsheetPanel.GetSelection(out col, out row);
            char letter = (char)('A' + col);
            cellName.Text = (letter.ToString() + (row + 1).ToString());
            spreadsheetPanel.GetValue(col, row, out value);
            cellValue.Text = value;
            object contents = spreadsheet.GetCellContents(cellName.Text);
            cellContents.Text = GetCurrentStringContentsOfCell(cellName.Text);
        }

        /// <summary>
        /// Called whenever the contents of a cell are modified. Uses a Spreadsheet to update cell information.
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        private void UpdateSingleCell(string name, string contents)
        {
            HashSet<string> cellDependents;
            cellDependents = new HashSet<string>(spreadsheet.SetContentsOfCell(name, contents));
            if (cellDependents.Count > 1)
            {
                UpdateCells(cellDependents);
            }
            else
            {
                object value = spreadsheet.GetCellValue(name);
                if (value is FormulaError)
                    value = "ERROR";

                int col, row;
                col = name.ElementAt(0) - 'A';
                int.TryParse(name.Substring(1), out row);
                row -= 1;
                spreadsheetPanel.SetValue(col, row, value.ToString());
            }
        }

        /// <summary>
        /// Updates each dependent in the spreadsheet and updates the spreadsheet panel.
        /// </summary>
        /// <param name="set"></param>
        private void UpdateCells(IEnumerable<string> set)
        {
            foreach (string name in set)
            {
                int col, row;
                col = name.ElementAt(0) - 'A';
                int.TryParse(name.Substring(1), out row);
                row -= 1;
                object value = spreadsheet.GetCellValue(name);
                if (value is FormulaError)
                    value = "ERROR";
                spreadsheetPanel.SetValue(col, row, value.ToString());
            }
        }

        /// <summary>
        /// Is called whenever a selection change occurs. 
        /// </summary>
		private void HandleSelectionChange()
        {
            IfCellChangedUpdateUndoStack();
            SelectionChange();
            contentsOfSelection = cellContents.Text;
            spreadsheetPanel.Select();
        }

        /// <summary>
        /// Returns true if and only if the string is a letter followed by a number 1-99
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private bool IsValidVariable(string s)
        {
            if (Double.TryParse(s, out double parsedToken) || Regex.IsMatch(s, @"[\+\-*/()]"))
                return true;
            return Regex.IsMatch(s, @"^[a-zA-Z](0?[1-9]|[1-9][0-9])$");
        }

        /// <summary>
        /// Called when opening a new file
        /// </summary>
        private void ClearAllCells()
        {
            foreach(string name in spreadsheet.GetNamesOfAllNonemptyCells())
            {
                int col, row;
                col = name.ElementAt(0) - 'A';
                int.TryParse(name.Substring(1), out row);
                row -= 1;
                spreadsheetPanel.SetValue(col, row, "");
            }
        }
    }
}
