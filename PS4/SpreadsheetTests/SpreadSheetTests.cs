///<author>
///Benjamin Allred
///</author>
///<UID>
///u1090524
///</UID>

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS;
using SpreadsheetUtilities;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SpreadsheetTests
{
    [TestClass]
    public class SpreadsheetTests
    {
        [TestInitialize]
        public void SetUp()
        {
            File.WriteAllText("circular.xml", "<spreadsheetversion=\"default\"><cell><name>C1</name><contents>=5+A4</contents></cell><cell><name>A4</name><contents>=C2-9</contents></cell><cell><name>C2</name><contents>=C1</contents></cell></spreadsheet>");
            File.WriteAllText("invalid_content.xml", "<spreadsheet version=\"default\">  <cell>    <name>C1</name>    <contents>=8/9+E8-I9</contents>  </cell>  <cell>    <name>E8</name>    <contents>=A4+50</contents>  </cell>  <cell>    <name>A4</name>    <contents>=32+B5-7</contents>  </cell>  <cell>    <name>B5</name>    <contents>=10*C4</contents>  </cell>  <cell>    <name>C4</name>    <contents>=@30</contents>  </cell>  <cell>    <name>T9</name>    <contents>Hello there</contents>  </cell></spreadsheet>");
            File.WriteAllText("no_cell_data.xml", "<spreadsheet version=\"default\">  <cell>    <name>C1</name>    <contents>=8/9+E8-I9</contents>  </cell>  <cell>    <name>E8</name>    <contents>=A4+50</contents>  </cell>  <cell>    <name>A4</name>    <contents>=32+B5-7</contents>  </cell>  <cell>    <name>B5</name>    <contents>=10*C4</contents>  </cell>  <cell>    <name>C4</name>    <contents>=@30</contents>  </cell>  <cell>    <name>T9</name>    <contents>Hello there</contents>  </cell></spreadsheet>");
            File.WriteAllText("wrong_version.xml", "< spreadsheet version = \"notdefault\" >  < cell >    < name > C1 </ name >    < contents >= 8 / 9 + E8 - I9 </ contents >  </ cell >  < cell >    < name > E8 </ name >    < contents >= A4 + 50 </ contents >  </ cell >  < cell >    < name > A4 </ name >    < contents >= 32 + B5 - 7 </ contents >  </ cell >  < cell >    < name > B5 </ name >    < contents >= 10 * C4 </ contents >  </ cell >  < cell >    < name > C4 </ name >    < contents > 30 </ contents >  </ cell >  < cell >    < name > T9 </ name >    < contents > Hello there </ contents >  </ cell ></ spreadsheet >");
        }

        /// <summary>
        /// Simple isValid method
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        public bool Validator(string variable)
        {
            if (variable.Contains("Z"))
                return false;

            else
                return true;
        }

        /// <summary>
        /// Simple normalizer that converts all variables to uppercase
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        public string ToUpperCase(string variable)
        {
            return variable.ToUpper();
        }

        /// <summary>
        /// Dummy lookup for stress test
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        public double NullLookup(string variable)
        {
            return 0;
        }

        //------------------------------- PUBLIC TESTS ---------------------------------//

        [TestMethod]
        public void PublicTestGetNamesEmptySpreadsheet()
        {
            Spreadsheet s = new Spreadsheet();
            HashSet<string> h = new HashSet<string>(s.GetNamesOfAllNonemptyCells());
            Assert.AreEqual(0, h.Count);
        }

        [TestMethod]
        public void PublicTestGetEmptyCellContents()
        {
            Spreadsheet s = new Spreadsheet();
            Assert.AreEqual("", s.GetCellContents("A1"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void PublicTestGetCellContentsException1()
        {
            Spreadsheet s = new Spreadsheet();
            s.GetCellContents(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void PublicTestGetCellContentsException2()
        {
            Spreadsheet s = new Spreadsheet();
            s.GetCellContents("2_A");
        }

        [TestMethod]
        public void PublicTestBasicSetCellContentsToNumber()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "3.5");
            Assert.AreEqual((double)3.5, (double)s.GetCellContents("A1"));
        }

        [TestMethod]
        public void PublicTestSetCellContentsInt()
        {
            Spreadsheet s = new Spreadsheet();
            int integer = 4;
            s.SetContentsOfCell("A1", integer.ToString());
            Assert.AreEqual((double)4, s.GetCellContents("A1"));
        }

        [TestMethod]
        public void PublicTestBasicSetCellContentsToString()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "Hello World");
            Assert.IsTrue(s.GetCellContents("A1") is string);
            Assert.AreEqual("Hello World", s.GetCellContents("A1"));
        }

        [TestMethod]
        public void PublicTestBasicSetCellContentsToFormula()
        {
            Spreadsheet s = new Spreadsheet();
            string f = "=4 * (3 + 5)";
            HashSet<string> hs = new HashSet<string>(s.SetContentsOfCell("A1", f));
            Assert.IsTrue(hs.Contains("A1") && hs.Count == 1);
        }

        [TestMethod]
        public void PublicTestBasicSetCellContentsWithDependents()
        {
            Spreadsheet s = new Spreadsheet();
            string f1 = "=4 * (A1 + 3)";
            string f2 = "=50/2 - A1";
            s.SetContentsOfCell("B3", f1);
            s.SetContentsOfCell("C4", f2);
            HashSet<string> h = new HashSet<string>(s.SetContentsOfCell("A1", "10"));
            Assert.IsTrue(h.Contains("A1"));
            Assert.IsTrue(h.Contains("B3"));
            Assert.IsTrue(h.Contains("C4"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PublicTestSetCellContentsWithNullString()
        {
            Spreadsheet s = new Spreadsheet();
            string nullString = null;
            s.SetContentsOfCell("B2", nullString);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void PublicTestSetCellContentsInvalidName1()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("1OP", "9");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void PublicTestSetCellContentsInvalidName2()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("$H4", "Hello");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void PublicTestSetCellContentsInvalidName3()
        {
            Spreadsheet s = new Spreadsheet();
            string f = "=20";
            s.SetContentsOfCell("@gmail", f);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void PublicTestSetCellContentsInvalidName4()
        {
            Spreadsheet s = new Spreadsheet();
            string str = null;
            s.SetContentsOfCell(str, "9");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void PublicTestSetCellContentsInvalidName5()
        {
            Spreadsheet s = new Spreadsheet();
            string str = null;
            s.SetContentsOfCell(str, "Hey World");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void PublicTestSetCellContentsInvalidName6()
        {
            Spreadsheet s = new Spreadsheet();
            string f = "=20";
            string str = null;
            s.SetContentsOfCell(str, f);
        }

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void PublicTestBasicCircular()
        {
            Spreadsheet s = new Spreadsheet();
            string f1 = "=A4 + 50";
            string f2 = "=32 + B5 - 7";
            string f3 = "=10 * C4";
            s.SetContentsOfCell("A4", f2);
            s.SetContentsOfCell("C4", f1);
            s.SetContentsOfCell("B5", f3);
        }

        [TestMethod]
        public void PublicTestGetNonEmptyNames()
        {
            Spreadsheet s = new Spreadsheet();
            string f1 = "=A4 + 50";
            string f2 = "=32 + B5 - 7";
            string f3 = "=10 * C4";
            s.SetContentsOfCell("A4", f2);
            s.SetContentsOfCell("E8", f1);
            s.SetContentsOfCell("B5", f3);
            s.SetContentsOfCell("T9", "Hello there");
            s.SetContentsOfCell("Z18", "669");
            HashSet<string> nonEmpty = new HashSet<string>(s.GetNamesOfAllNonemptyCells());
            Assert.IsTrue(nonEmpty.Count == 5);
        }

        [TestMethod]
        public void PublicTestSetToEmptyCell()
        {
            Spreadsheet s = new Spreadsheet();
            string f1 = "=A4 + 50";
            string f2 = "=32 + B5 - 7";
            string f3 = "=10 * C4";
            s.SetContentsOfCell("A4", f2);
            s.SetContentsOfCell("E8", f1);
            s.SetContentsOfCell("B5", f3);
            HashSet<string> nonEmpty = new HashSet<string>(s.GetNamesOfAllNonemptyCells());
            Assert.IsTrue(nonEmpty.Count == 3);
            s.SetContentsOfCell("E8", "");
            nonEmpty = new HashSet<string>(s.GetNamesOfAllNonemptyCells());
            Assert.IsTrue(nonEmpty.Count == 2);
        }

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void PublicTestSetCellToItself()
        {
            Spreadsheet s = new Spreadsheet();
            string f1 = "=A1";
            s.SetContentsOfCell("A1", f1);
        }

        [TestMethod]
        public void PublicTestDeleteDependencies()
        {
            Spreadsheet s = new Spreadsheet();
            string f1 = "=A4 + C6 - E4";
            s.SetContentsOfCell("B1", f1);
            HashSet<string> A4 = new HashSet<string>(s.SetContentsOfCell("A4", "40"));
            HashSet<string> C6 = new HashSet<string>(s.SetContentsOfCell("C6", "String"));
            HashSet<string> E4 = new HashSet<string>(s.SetContentsOfCell("E4", "String"));
            Assert.IsTrue(A4.Count == 2 && C6.Count == 2 && E4.Count == 2);
            s.SetContentsOfCell("B1", "InterestingString");
            A4 = new HashSet<string>(s.SetContentsOfCell("A4", "40"));
            C6 = new HashSet<string>(s.SetContentsOfCell("C6", "String"));
            E4 = new HashSet<string>(s.SetContentsOfCell("E4", "String"));
            Assert.IsTrue(A4.Count == 1 && C6.Count == 1 && E4.Count == 1);
        }

        [TestMethod]
        public void PublicTestGetDeletedDependency()
        {
            Spreadsheet s = new Spreadsheet();
            string f1 = "=A4 + C6 - E4";
            s.SetContentsOfCell("B1", "");
            HashSet<string> A4 = new HashSet<string>(s.SetContentsOfCell("A4", "40"));
            HashSet<string> C6 = new HashSet<string>(s.SetContentsOfCell("C6", "String"));
            HashSet<string> E4 = new HashSet<string>(s.SetContentsOfCell("E4", "String"));
            HashSet<string> h = new HashSet<string>(s.GetNamesOfAllNonemptyCells());
            Assert.IsTrue(h.Count == 3);
            s.SetContentsOfCell("B1", "InterestingString");
            h = new HashSet<string>(s.GetNamesOfAllNonemptyCells());
            Assert.IsTrue(h.Count == 4);
        }

        [TestMethod]
        public void PublicTestGetNonEmptyAfterModification()
        {
            Spreadsheet s = new Spreadsheet();
            string f1 = "=A4 + 50";
            string f2 = "=32 + B5 - 7";
            string f3 = "=10 * C4";
            s.SetContentsOfCell("A4", f2);
            s.SetContentsOfCell("E8", f1);
            s.SetContentsOfCell("B5", f3);
            s.SetContentsOfCell("T9", "Hello there");
            HashSet<string> nonEmpty = new HashSet<string>(s.GetNamesOfAllNonemptyCells());
            Assert.IsTrue(nonEmpty.Count == 4);
            s.SetContentsOfCell("Z18", "669");
            nonEmpty = new HashSet<string>(s.GetNamesOfAllNonemptyCells());
            Assert.IsTrue(nonEmpty.Count == 5);
            s.SetContentsOfCell("E8", "");
            s.SetContentsOfCell("B5", "");
            s.SetContentsOfCell("T9", "");
            nonEmpty = new HashSet<string>(s.GetNamesOfAllNonemptyCells());
            Assert.IsTrue(nonEmpty.Count == 2);
        }

        [TestMethod]
        public void PublicTestDependenciesAfterModification()
        {
            Spreadsheet s = new Spreadsheet();
            //makes some formulas with variables
            string f1 = "=A4 + 50";
            string f2 = "=32 + B5 - 7";
            string f3 = "=10 * C4";
            string f4 = "=8 / 9 + E8 - I9";
            string f5 = "=T9";
            //set up some cells and store their dependencies in hashsets
            HashSet<string> C1 = new HashSet<string>(s.SetContentsOfCell("C1", f4));
            HashSet<string> E8 = new HashSet<string>(s.SetContentsOfCell("E8", f1));
            HashSet<string> A4 = new HashSet<string>(s.SetContentsOfCell("A4", f2));
            HashSet<string> B5 = new HashSet<string>(s.SetContentsOfCell("B5", f3));
            HashSet<string> C4 = new HashSet<string>(s.SetContentsOfCell("C4", "30"));
            HashSet<string> T9 = new HashSet<string>(s.SetContentsOfCell("T9", "Hello there"));
            //make hashsets of what they are expected to contain
            HashSet<string> A4_Contents = new HashSet<string>() { "A4", "E8", "C1"};
            HashSet<string> E8_Contents = new HashSet<string>() { "E8", "C1" };
            HashSet<string> C1_Contents = new HashSet<string>() { "C1" };
            HashSet<string> B5_Contents = new HashSet<string>() { "B5", "A4", "E8", "C1" };
            HashSet<string> T9_Contents = new HashSet<string>() { "T9" };
            HashSet<string> C4_Contents = new HashSet<string>() { "C4", "B5", "A4", "E8", "C1" };
            //test them
            Assert.IsTrue(A4.SetEquals(A4_Contents));
            Assert.IsTrue(E8.SetEquals(E8_Contents));
            Assert.IsTrue(C1.SetEquals(C1_Contents));
            Assert.IsTrue(B5.SetEquals(B5_Contents));
            Assert.IsTrue(T9.SetEquals(T9_Contents));
            Assert.IsTrue(C4.SetEquals(C4_Contents));
            //modify them and save new dependencies
            C1 = new HashSet<string>(s.SetContentsOfCell("C1", f4));
            E8 = new HashSet<string>(s.SetContentsOfCell("E8", f5));
            A4 = new HashSet<string>(s.SetContentsOfCell("A4", f5));
            T9 = new HashSet<string>(s.SetContentsOfCell("T9", "90"));
            B5 = new HashSet<string>(s.SetContentsOfCell("B5", f3));
            C4 = new HashSet<string>(s.SetContentsOfCell("C4", "30"));
            HashSet<string> I9 = new HashSet<string>(s.SetContentsOfCell("I9", "hey girl"));
            HashSet<string> I9_Contents = new HashSet<string>() { "I9", "C1" };
            //update expected dependencies
            T9_Contents = new HashSet<string>() { "T9", "A4", "E8", "C1" };
            A4_Contents = new HashSet<string>() { "A4" };
            E8_Contents = new HashSet<string>() { "E8", "C1" };
            C1_Contents = new HashSet<string>() { "C1" };
            B5_Contents = new HashSet<string>() { "B5" };
            C4_Contents = new HashSet<string>() { "C4", "B5" };
            //test the modifications
            Assert.IsTrue(A4.SetEquals(A4_Contents));
            Assert.IsTrue(E8.SetEquals(E8_Contents));
            Assert.IsTrue(C1.SetEquals(C1_Contents));
            Assert.IsTrue(B5.SetEquals(B5_Contents));
            Assert.IsTrue(T9.SetEquals(T9_Contents));
            Assert.IsTrue(C4.SetEquals(C4_Contents));
            Assert.IsTrue(I9.SetEquals(I9_Contents));
        }

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void PublicTestCircularAfterModification()
        {
            Spreadsheet s = new Spreadsheet();
            //makes some formulas with variables
            string f1 = "=A4 + 50";
            string f2 = "=32 + B5 - 7";
            string f3 = "=10 * C4";
            string f4 = "=8 / 9 + E8 - I9";
            string f5 = "=T9";
            //set up some cells and store their dependencies in hashsets
            HashSet<string> C1 = new HashSet<string>(s.SetContentsOfCell("C1", f4));
            HashSet<string> E8 = new HashSet<string>(s.SetContentsOfCell("E8", f1));
            HashSet<string> A4 = new HashSet<string>(s.SetContentsOfCell("A4", f2));
            HashSet<string> B5 = new HashSet<string>(s.SetContentsOfCell("B5", f3));
            HashSet<string> C4 = new HashSet<string>(s.SetContentsOfCell("C4", "30"));
            HashSet<string> T9 = new HashSet<string>(s.SetContentsOfCell("T9", "Hello there"));
            //make hashsets of what they are expected to contain
            HashSet<string> A4_Contents = new HashSet<string>() { "A4", "E8", "C1" };
            HashSet<string> E8_Contents = new HashSet<string>() { "E8", "C1" };
            HashSet<string> C1_Contents = new HashSet<string>() { "C1" };
            HashSet<string> B5_Contents = new HashSet<string>() { "B5", "A4", "E8", "C1" };
            HashSet<string> T9_Contents = new HashSet<string>() { "T9" };
            HashSet<string> C4_Contents = new HashSet<string>() { "C4", "B5", "A4", "E8", "C1" };
            //test them
            Assert.IsTrue(A4.SetEquals(A4_Contents));
            Assert.IsTrue(E8.SetEquals(E8_Contents));
            Assert.IsTrue(C1.SetEquals(C1_Contents));
            Assert.IsTrue(B5.SetEquals(B5_Contents));
            Assert.IsTrue(T9.SetEquals(T9_Contents));
            Assert.IsTrue(C4.SetEquals(C4_Contents));
            //modify them and save new dependencies
            string f6 = "=C1";
            C4 = new HashSet<string>(s.SetContentsOfCell("C4", f6));
        }

        [TestMethod]
        public void PublicTestSaveSpreadsheet()
        {
            Spreadsheet s = new Spreadsheet();
            //makes some formulas with variables
            string f1 = "=A4 + 50";
            string f2 = "=32 + B5 - 7";
            string f3 = "=10 * C4";
            string f4 = "=8 / 9 + E8 - I9";
            string f5 = "=T9";

            HashSet<string> C1 = new HashSet<string>(s.SetContentsOfCell("C1", f4));
            HashSet<string> E8 = new HashSet<string>(s.SetContentsOfCell("E8", f1));
            HashSet<string> A4 = new HashSet<string>(s.SetContentsOfCell("A4", f2));
            HashSet<string> B5 = new HashSet<string>(s.SetContentsOfCell("B5", f3));
            HashSet<string> C4 = new HashSet<string>(s.SetContentsOfCell("C4", "30"));
            HashSet<string> T9 = new HashSet<string>(s.SetContentsOfCell("T9", "Hello there"));

            s.Save(@"first.xml");
        }

        [TestMethod]
        public void PublicTestSaveAndLoadSpreadsheet()
        {
            Spreadsheet s1 = new Spreadsheet();
            //makes some formulas with variables
            string f1 = "=A4 + 50";
            string f2 = "=32 + B5 - 7";
            string f3 = "=10 * C4";
            string f4 = "=8 / 9 + E8 - I9";
            string f5 = "=T9";

            HashSet<string> C1 = new HashSet<string>(s1.SetContentsOfCell("C1", f4));
            HashSet<string> E8 = new HashSet<string>(s1.SetContentsOfCell("E8", f1));
            HashSet<string> A4 = new HashSet<string>(s1.SetContentsOfCell("A4", f2));
            HashSet<string> B5 = new HashSet<string>(s1.SetContentsOfCell("B5", f3));
            HashSet<string> C4 = new HashSet<string>(s1.SetContentsOfCell("C4", "30"));
            HashSet<string> T9 = new HashSet<string>(s1.SetContentsOfCell("T9", "Hello there"));

            s1.Save(@"second.xml");

            Spreadsheet s2 = new Spreadsheet(@"second.xml", s => true, s => s, "default");

            Formula fm1 = new Formula("A4 + 50");
            Formula fm2 = new Formula("32 + B5 - 7");
            Formula fm3 = new Formula("10 * C4");
            Formula fm4 = new Formula("8 / 9 + E8 - I9");
            double thirty = 30;

            Assert.AreEqual(s2.GetCellContents("C1"), fm4);
            Assert.AreEqual(s2.GetCellContents("E8"), fm1);
            Assert.AreEqual(s2.GetCellContents("A4"), fm2);
            Assert.AreEqual(s2.GetCellContents("B5"), fm3);
            Assert.AreEqual(s2.GetCellContents("C4"), thirty);
            Assert.AreEqual(s2.GetCellContents("T9"), "Hello there");
        }

        [TestMethod]
        public void PublicTestGetCellValue()
        {
            Spreadsheet s1 = new Spreadsheet();
            //makes some formulas with variables
            string f1 = "=A4 + 50";
            string f2 = "=32 + B5 - 7";
            string f3 = "=10 * C4";
            string f4 = "=8 / 9 + E8 - I9";
            double C1value = (8.0 / 9.0 + 375.0 - 5.0);
            HashSet<string> C1 = new HashSet<string>(s1.SetContentsOfCell("C1", f4));
            HashSet<string> E8 = new HashSet<string>(s1.SetContentsOfCell("E8", f1));
            HashSet<string> A4 = new HashSet<string>(s1.SetContentsOfCell("A4", f2));
            HashSet<string> B5 = new HashSet<string>(s1.SetContentsOfCell("B5", f3));
            HashSet<string> C4 = new HashSet<string>(s1.SetContentsOfCell("C4", "30"));
            HashSet<string> I9 = new HashSet<string>(s1.SetContentsOfCell("I9", "5"));
            HashSet<string> T9 = new HashSet<string>(s1.SetContentsOfCell("T9", "Hello there"));

            Assert.AreEqual(s1.GetCellValue("A4"), (double)325);
            Assert.AreEqual(s1.GetCellValue("T9"), "Hello there");
            Assert.AreEqual(s1.GetCellValue("B5"), (double)300);
            Assert.AreEqual(s1.GetCellValue("E8"), (double)375);
            Assert.AreEqual(s1.GetCellValue("I9"), (double)5);
            Assert.AreEqual(s1.GetCellValue("C1"), C1value);
            Assert.AreEqual(s1.GetCellValue("C4"), (double)30);
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void PublicTestMismatchedVersion()
        {
            Spreadsheet s1 = new Spreadsheet(@"wrong_version.xml", s => true, s => s, "default");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void PublicTestCircularDependency()
        {
            Spreadsheet s1 = new Spreadsheet(@"circular.xml", s => true, s => s, "default");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void PublicTestNonExistentFile()
        {
            Spreadsheet s1 = new Spreadsheet(@"imnotreal.xml", s => true, s => s, "default");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void PublicTestInvalidXMLFormat()
        {
            Spreadsheet s1 = new Spreadsheet(@"no_cell_data.xml", s => true, s => s, "default");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void PublicTestInvalidCellContent()
        {
            Spreadsheet s1 = new Spreadsheet(@"invalid_cell_content.xml", s => true, s => s, "default");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void PublicTestGetVersionError1()
        {
            Spreadsheet s1 = new Spreadsheet();
            s1.GetSavedVersion("iamnotreal.xml");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void PublicTestGetVersionError2()
        {
            Spreadsheet s1 = new Spreadsheet();
            File.WriteAllText("lame.xml", "<spreadsheet></spreadsheet>");
            s1.GetSavedVersion("lame.xml");
        }

        [TestMethod]
        public void PublicTestGetVersion()
        {
            Spreadsheet s1 = new Spreadsheet(s => true, s => s, "version");
            s1.SetContentsOfCell("A1", "1");
            s1.Save("lame.xml");
            Assert.AreEqual(s1.GetSavedVersion("lame.xml"), "version");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void PublicTestSaveError()
        {
            Spreadsheet s1 = new Spreadsheet(@"\\\234asdfcircular.xml", s => true, s => s, "default");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void PublicTestSetWithValidator()
        {
            Spreadsheet s1 = new Spreadsheet(Validator, s => s, "default");
            s1.SetContentsOfCell("Z0", "2");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void PublicTestGetValueWithValidator()
        {
            Spreadsheet s1 = new Spreadsheet(Validator, s => s, "default");
            s1.GetCellValue("Z0");
        }

        [TestMethod]
        public void PublicTestStressTest()
        {
            Random random = new Random();
            //setup variables
            //number of variables = SIZE * length of alphabet
            const int SIZE = 250;
            char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            char[] lwAlphabet = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
            string[] variables = new string[SIZE * alphabet.Length];
            for(int i = 0; i < SIZE; i++)
            {
                for(int j = 0; j < alphabet.Length; j++)
                {
                    if(random.Next(0, 2) == 1)
                        variables[i * alphabet.Length + j] = lwAlphabet[j].ToString() + i.ToString();
                    else
                        variables[i * alphabet.Length + j] = alphabet[j].ToString() + i.ToString();
                }
            }
            
            string[] formulae = new string[3000];
            string[] operators = new string[3] { "+", "-", "*" };
            double[] formulaeValues = new double[3000];
            for(int i = 0; i < formulae.Length; i++)
            {
                string f = "=";
                //formula length
                int j = random.Next(1, 15);
                //make it odd
                if (j % 2 == 0)
                    j--;
                //populate
                for(int k = 0; k < j; k++)
                {
                    f = f + random.Next(0, 100);
                    f = f + operators[random.Next(0, 3)];
                    if(k+1 == j)
                        f = f + random.Next(0, 100);
                }
                formulae[i] = f;
                Formula evaluator = new Formula(f.Remove(0, 1));
                formulaeValues[i] = (double) evaluator.Evaluate(NullLookup);
            }

            Spreadsheet s1 = new Spreadsheet(s => true, ToUpperCase, "stress");
            
            for(int i = 0; i < formulae.Length; i++)
            {
                s1.SetContentsOfCell(variables[i], formulae[i]);
            }

            double[] doubleValues = new double[1000];
            int counter = 0;
            for(int i = formulae.Length; i < (formulae.Length + 1000); i++)
            {
                doubleValues[counter] = random.Next(0, 50000);
                s1.SetContentsOfCell(variables[i], doubleValues[counter].ToString());
                counter++;
            }

            string[] stringValues = new string[2500];
            counter = 0;
            for(int i = (formulae.Length + 1000); i < SIZE*alphabet.Length; i++)
            {
                int strLength = random.Next(1, 12);
                string str = "";
                for(int j = 0; j < strLength; j++)
                {
                    str = str + alphabet[random.Next(0, alphabet.Length)];
                }
                s1.SetContentsOfCell(variables[i], str);
                stringValues[counter] = str;
                counter++;
            }


            //get cell values
            counter = 0;
            for(int i = 0; i < formulaeValues.Length; i++)
            {
                Assert.AreEqual(s1.GetCellValue(variables[i]), formulaeValues[i]);
                counter++;
            }

            for (int i = 0; i < doubleValues.Length; i++)
            {
                Assert.AreEqual(s1.GetCellValue(variables[counter]), doubleValues[i]);
                counter++;
            }

            for (int i = 0; i < stringValues.Length; i++)
            {
                Assert.AreEqual(s1.GetCellValue(variables[counter]), stringValues[i]);
                counter++;
            }



            //but wait! There's more!!
            //save and reload baby

            s1.Save("stressful.xml");
            Spreadsheet s2 = new Spreadsheet("stressful.xml", s => true, ToUpperCase, "stress");

            for(int i = 0; i < variables.Length; i++)
            {
                Assert.AreEqual(s1.GetCellValue(variables[i]), s2.GetCellValue(variables[i]));
            }
        }

        [TestMethod]
        public void PublicTestRecalculate()
        {
            Spreadsheet s1 = new Spreadsheet();
            s1.SetContentsOfCell("A1", "=B3");
            Assert.IsTrue(s1.GetCellValue("A1") is FormulaError);
            s1.SetContentsOfCell("B3", "=C4+1");
            Assert.IsTrue(s1.GetCellValue("B3") is FormulaError);
            s1.SetContentsOfCell("C4", "90");
            Assert.AreEqual((double)s1.GetCellValue("B3"), (double)91);
            Assert.AreEqual((double)s1.GetCellValue("A1"), (double)91);
        }

    }
}
