using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetGUI;

namespace PS6Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            UndoRedoStack s = new UndoRedoStack();

            s.CellContentsChange("a1", "1");

            string a, b;
            Assert.IsTrue(s.TryUndo(t => t, out a, out b));
        }
    }
}
