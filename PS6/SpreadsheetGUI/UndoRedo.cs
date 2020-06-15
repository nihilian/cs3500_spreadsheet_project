using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpreadsheetGUI
{
    /// <summary>
    /// Class UndoRedoStack stores information about the undoing and redoing of changes to a Spreadsheet
    /// </summary>
    public class UndoRedoStack
    {
        /// <summary>
        /// Class to keep track of two strings.
        /// </summary>
        internal class CellChange
        {
            private string name, contents;
            public string Name { get { return name; } set { name = value; } }
            public string Contents { get { return contents; } set { contents = value; } }
            public CellChange(string n, string c)
            {
                name = n;
                contents = c;
            }
        }

        Stack<CellChange> undoStack; //stores the old contents of a cell before a changes
        Stack<CellChange> redoStack; //stores the contents of a cell before it was undone.

        public UndoRedoStack()
        {
            undoStack = new Stack<CellChange>();
            redoStack = new Stack<CellChange>();
        }

        public void CellContentsChange(string name, string contents)
        {
            CellChange var = new CellChange(name, contents);
            undoStack.Push(var);
            redoStack.Clear();
        }

        public bool TryUndo(Func<string, string> GetOldCellContents, out string name, out string newContents)
        {
            if (!CanUndo())
            {

                name = null;
                newContents = null;
                return false;
            }
            else
            {
                CellChange temp = undoStack.Pop();
                redoStack.Push(new CellChange(temp.Name, GetOldCellContents(temp.Name)));
                name = temp.Name;
                newContents = temp.Contents;
                return true;
            }
        }

        public bool TryRedo(Func<string, string> GetOldCellContents, out string name, out string newContents)
        {
            if (!CanRedo())
            {
                name = null;
                newContents = null;
                return false;
            }
            else
            {
                CellChange temp = redoStack.Pop();
                undoStack.Push(new CellChange(temp.Name, GetOldCellContents(temp.Name)));
                name = temp.Name;
                newContents = temp.Contents;
                return true;
            }
        }

        /// <summary>
        /// Checks if a redo can be performed.
        /// </summary>
        /// <returns></returns>
        public bool CanRedo()
        {
            return redoStack.Count != 0;
        }

        /// <summary>
        /// Checks if an undo can be performed.
        /// </summary>
        /// <returns></returns>
        public bool CanUndo()
        {
            return undoStack.Count != 0;
        }
    }
}
