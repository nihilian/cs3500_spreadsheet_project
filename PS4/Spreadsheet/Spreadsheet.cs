///<author>
///Benjamin Allred
///</author>
///<UID>
///u1090524
///</UID>

using System;
using System.IO;
using System.Collections.Generic;
using SpreadsheetUtilities;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Linq;
using System.Xml;

namespace SS
{
    /// <summary>
    /// A Spreadsheet object represents the state of a simple spreadsheet.  A 
    /// spreadsheet consists of an infinite number of named cells.
    /// 
    /// A string is a cell name if and only if it consists of one or more letters,
    /// followed by one or more digits AND it satisfies the predicate IsValid.
    /// For example, "A15", "a15", "XY032", and "BC7" are cell names so long as they
    /// satisfy IsValid.  On the other hand, "Z", "X_", and "hello" are not cell names,
    /// regardless of IsValid.
    /// 
    /// Any valid incoming cell name, whether passed as a parameter or embedded in a formula,
    /// must be normalized with the Normalize method before it is used by or saved in 
    /// this spreadsheet.  For example, if Normalize is s => s.ToUpper(), then
    /// the Formula "x3+a5" should be converted to "X3+A5" before use.
    /// 
    /// A spreadsheet contains a cell corresponding to every possible cell name.  
    /// In addition to a name, each cell has a contents and a value.  The distinction is
    /// important.
    /// 
    /// The contents of a cell can be (1) a string, (2) a double, or (3) a Formula.  If the
    /// contents is an empty string, we say that the cell is empty.  (By analogy, the contents
    /// of a cell in Excel is what is displayed on the editing line when the cell is selected.)
    /// 
    /// In a new spreadsheet, the contents of every cell is the empty string.
    ///  
    /// The value of a cell can be (1) a string, (2) a double, or (3) a FormulaError.  
    /// (By analogy, the value of an Excel cell is what is displayed in that cell's position
    /// in the grid.)
    /// 
    /// If a cell's contents is a string, its value is that string.
    /// 
    /// If a cell's contents is a double, its value is that double.
    /// 
    /// If a cell's contents is a Formula, its value is either a double or a FormulaError,
    /// as reported by the Evaluate method of the Formula class.  The value of a Formula,
    /// of course, can depend on the values of variables.  The value of a variable is the 
    /// value of the spreadsheet cell it names (if that cell's value is a double) or 
    /// is undefined (otherwise).
    /// 
    /// Spreadsheets are never allowed to contain a combination of Formulas that establish
    /// a circular dependency.  A circular dependency exists when a cell depends on itself.
    /// For example, suppose that A1 contains B1*2, B1 contains C1*2, and C1 contains A1*2.
    /// A1 depends on B1, which depends on C1, which depends on A1.  That's a circular
    /// dependency.
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {
        //keep track of all used cells
        private Dictionary<string, Cell> usedCells;
        //keep track of cell dependencies
        private DependencyGraph depGraph;

        // ADDED FOR PS5
        /// <summary>
        /// True if this spreadsheet has been modified since it was created or saved                  
        /// (whichever happened most recently); false otherwise.
        /// </summary>
        public override bool Changed { get; protected set; }

        // ADDED FOR PS5
        /// <summary>
        /// Constructs a spreadsheet by recording its variable validity test,
        /// its normalization method, and its version information.  The variable validity
        /// test is used throughout to determine whether a string that consists of one or
        /// more letters followed by one or more digits is a valid cell name.  The variable
        /// equality test should be used thoughout to determine whether two variables are
        /// equal.
        /// </summary>
        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version) :
            base(isValid, normalize, version)
        {
            usedCells = new Dictionary<string, Cell>();
            depGraph = new DependencyGraph();
        }

        // ADDED FOR PS5
        /// <summary>
        /// Loads a spreadsheet from a given filepath.
        /// </summary>
        public Spreadsheet(string filepath, Func<string, bool> isValid, Func<string, string> normalize, string version) :
            this(isValid, normalize, version)
        {
            LoadSpreadsheet(filepath, version);
        }

        /// <summary>
        /// Creates an empty Spreadsheet
        /// </summary>
        public Spreadsheet() :
            this(s => true, s => s, "default")
        {
        }


        /// <summary>
        /// Helper method that is called when a spreadsheet object is instantiated with a filepath parameter. Reads an xml
        /// document and sets up cell dictionary and dependency graph.
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="version"></param>
        private void LoadSpreadsheet(string filepath, string version)
        {
            //XML element to represent spreadsheet tag
            XElement spreadsheet;
            try
            {
                //try loading file
                spreadsheet = XElement.Load(filepath);
            }
            catch (Exception)
            {
                //throw error if it doesn't exist.
                throw new SpreadsheetReadWriteException("Error reading file");
            }

            //throw error if version doesn't match up.
                if (spreadsheet.Attribute("version").Value.ToString() != version)
                throw new SpreadsheetReadWriteException("Version Mismatch");

            //queries spreadsheet for all cell elements and stores them
            IEnumerable<XElement> spreadsheetQuery =
                from cell in spreadsheet.Elements("cell")
                select cell;

            //iterate through each cell
            foreach (XElement cell in spreadsheetQuery)
            {
                string name, contents;
                try
                {
                    //gets cell's name and contents.
                    if (!IsValid(cell.Element("name").Value.ToString()))
                        throw new SpreadsheetReadWriteException("Invalid cell name");
                    name = Normalize(cell.Element("name").Value.ToString());
                    contents = cell.Element("contents").Value.ToString();
                }
                catch(Exception)
                {
                    throw new SpreadsheetReadWriteException("Cannot read cell data");
                }

                //validates cell names
                if (!Regex.IsMatch(name, @"^[a-zA-Z]+[0-9]+$"))
                    throw new SpreadsheetReadWriteException("Invalid cell name in spreadsheet file");

                try
                {
                    //sets contents of cell unless a circular exception is thrown
                    SetContentsOfCell(name, contents);
                }
                catch (CircularException)
                {
                    throw new SpreadsheetReadWriteException("Circular dependency in spreadsheet file");
                }
                catch (Exception)
                {
                    throw new SpreadsheetReadWriteException("Invalid cell contents in spreadsheet file");
                }
            }
        }

        // ADDED FOR PS5
        /// <summary>
        /// Returns the version information of the spreadsheet saved in the named file.
        /// If there are any problems opening, reading, or closing the file, the method
        /// should throw a SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        public override string GetSavedVersion(String filename)
        {
            XElement spreadsheet;
            try
            {
                //load xml file
                spreadsheet = XElement.Load(filename);
            }
            catch (Exception)
            {
                //throw exception if file cannot be read/doesn't exist
                throw new SpreadsheetReadWriteException("Error reading file");
            }

            try
            {
                //get version
                return spreadsheet.Attribute("version").Value.ToString();
            }
            catch(Exception)
            {
                throw new SpreadsheetReadWriteException("Error reading data: Can't find version attribute");
            }
        }

        // ADDED FOR PS5
        /// <summary>
        /// Writes the contents of this spreadsheet to the named file using an XML format.
        /// The XML elements should be structured as follows:
        /// 
        /// <spreadsheet version="version information goes here">
        /// 
        /// <cell>
        /// <name>
        /// cell name goes here
        /// </name>
        /// <contents>
        /// cell contents goes here
        /// </contents>    
        /// </cell>
        /// 
        /// </spreadsheet>
        /// 
        /// There should be one cell element for each non-empty cell in the spreadsheet.  
        /// If the cell contains a string, it should be written as the contents.  
        /// If the cell contains a double d, d.ToString() should be written as the contents.  
        /// If the cell contains a Formula f, f.ToString() with "=" prepended should be written as the contents.
        /// 
        /// If there are any problems opening, writing, or closing the file, the method should throw a
        /// SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        public override void Save(String filename)
        {
            //settings to specify xml formatting
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";

            XmlWriter writer = null;
            try
            {
                //xml writer from filepath name.
                writer = XmlWriter.Create(filename, settings);
                //set start element with version attribute
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", Version);

                //iterate through each non empty cell
                foreach(string cellName in usedCells.Keys)
                {
                    Cell cell = usedCells[cellName];
                    //check if cell has been overriden to be empty
                    if (cell.Contents.ToString() != "")
                    {
                        //cell tag
                        writer.WriteStartElement("cell");
                        //contains name and contents values
                        writer.WriteElementString("name", cell.Name);

                        //prepend "=" if formula
                       if (cell.Contents is Formula)
                            writer.WriteElementString("contents", "=" + cell.Contents.ToString());

                       //otherwise just write contents as a string
                       else
                            writer.WriteElementString("contents", cell.Contents.ToString());

                       //closing cell tag
                        writer.WriteEndElement();
                    }
                }
                //closing spreadsheet tag
                writer.WriteEndElement();

                //save xml file to filepath
                XmlDocument doc = new XmlDocument();
                doc.Save(writer);
            }
            catch(Exception)
            {
                //throw error if saving fails
                throw new SpreadsheetReadWriteException("Error saving file");
            }
            finally
            {
                //close writer
                writer.Close();
            }
        }

        // ADDED FOR PS5
        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
        /// </summary>
        public override object GetCellValue(String name)
        {
            //check for valid name
            if (name == null || !Regex.IsMatch(name, @"^[a-zA-Z]+[0-9]+$") || !IsValid(name))
                throw new InvalidNameException();

            //ensure cell is non empty
            if(usedCells.TryGetValue(Normalize(name), out Cell cell))
                return cell.Value;

            //return empty string for all empty cells
            else
                return "";
        }


        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<String> GetNamesOfAllNonemptyCells()
        {
            foreach(string name in usedCells.Keys)
            {
                //return all used cells that don't have the empty string as their contents
                usedCells.TryGetValue(name, out Cell cell);
                if (!(cell.Contents.ToString() == ""))
                    yield return name;
            }
        }


        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        public override object GetCellContents(String name)
        {
            //check for valid cell name
            if (name == null || !Regex.IsMatch(name, @"^[a-zA-Z]+[0-9]+$") || !IsValid(name))
                throw new InvalidNameException();

            else
            {
                //return cell contents if there are any
                if (usedCells.TryGetValue(Normalize(name), out Cell cell))
                    return cell.Contents;
                //otherwise it is an empty cell
                else
                    return "";
            }
        }

        // ADDED FOR PS5
        /// <summary>
        /// If content is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        /// 
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor.  There are then three possibilities:
        /// 
        ///   (1) If the remainder of content cannot be parsed into a Formula, a 
        ///       SpreadsheetUtilities.FormulaFormatException is thrown.
        ///       
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown.
        ///       
        ///   (3) Otherwise, the contents of the named cell becomes f.
        /// 
        /// Otherwise, the contents of the named cell becomes content.
        /// 
        /// If an exception is not thrown, the method returns a set consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        public override ISet<String> SetContentsOfCell(String name, String content)
        {
            //checks for valid content
            if (content == null)
                throw new ArgumentNullException();

            //checks for valid name
            if (name == null || !Regex.IsMatch(name, @"^[a-zA-Z]+[0-9]+$") || !IsValid(name))
                throw new InvalidNameException();

            name = Normalize(name);
            //checks for double
            if (Double.TryParse(content, out double parsed))
            {
                Changed = true;
                return SetCellContents(name, parsed);
            }   

            //checks for formula
            else if (content != "" && content.ElementAt(0) == '=')
            {
                Formula formula = new Formula(content.Remove(0, 1), Normalize, IsValid);
                Changed = true;
                return SetCellContents(name, formula);
            }

            //else treat content as a string
            else
            {
                Changed = true;
                return SetCellContents(name, content);
            }   
        }


        // MODIFIED PROTECTION FOR PS5
        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<String> SetCellContents(String name, double number)
        {
            //set its contents and return all the dependencies
            SetContents(name, number);
            //recalculate potentially changed cells
            RecalculateCells(GetCellsToRecalculate(name));
            //remove dependees
            depGraph.ReplaceDependees(name, new HashSet<string>());
            return new HashSet<string>(GetCellsToRecalculate(name));
        }


        // MODIFIED PROTECTION FOR PS5
        /// <summary>
        /// If text is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<String> SetCellContents(String name, String text)
        {
            SetContents(name, text);
            //recalculate potentially changed cells
            RecalculateCells(GetCellsToRecalculate(name));
            //remove dependees
            depGraph.ReplaceDependees(name, new HashSet<string>());
            return new HashSet<string>(GetCellsToRecalculate(name));
        }

        // MODIFIED PROTECTION FOR PS5
        /// <summary>
        /// If formula parameter is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException.
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// Set consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<String> SetCellContents(String name, Formula formula)
        {
            //backup the part of depGraph that will be modified
            HashSet<string> backupDependees = new HashSet<string>(depGraph.GetDependees(name));
            HashSet<string> dependents;

            //modify depGraph
            depGraph.ReplaceDependees(name, formula.GetVariables());

            //check for circular dependencies
            try
            {
                dependents = new HashSet<string>(GetCellsToRecalculate(name));

            }
            catch (CircularException exception)
            {
                //restore the backup in the chance a circular exception is thrown
                depGraph.ReplaceDependees(name, backupDependees);
                throw exception;
            }

            SetContents(name, formula);
            //recalculate potentially changed cells
            RecalculateCells(GetCellsToRecalculate(name));
            return dependents;
        }


        /// <summary>
        /// Is passed to Cell's constructor to evaluate its value.
        /// </summary>
        /// <param name="variable">cell name</param>
        /// <returns></returns>
        private double Lookup(string variable)
        {
            if (Double.TryParse(GetCellValue(variable).ToString(), out double parsed))
                return parsed;
            else
                throw new ArgumentException();
        }

        /// <summary>
        /// Helper method for SetCellContents. If the cell has been used before
        /// modify its contents. Else create a new cell.
        /// </summary>
        /// <param name="name">Name of cell</param>
        /// <param name="contents">Contents of cell</param>
        private void SetContents(string name, object contents)
        {
            //remove cell if it exists
            if (usedCells.TryGetValue(name, out Cell cell))
                usedCells.Remove(name);

            //add cell to used cell storage
            usedCells.Add(name, new Cell(name, contents, IsValid, Normalize, Lookup));
        }


        /// <summary>
        /// Helper method to recalculate all cells that could potentially have been changed.
        /// </summary>
        /// <param name="cellsToRecalculate"></param>
        private void RecalculateCells(IEnumerable<string> cellsToRecalculate)
        {
            foreach(string cell in cellsToRecalculate)
            {
                usedCells[cell].CalculateValue();
            }
        }


        /// <summary>
        /// If name is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name isn't a valid cell name, throws an InvalidNameException.
        /// 
        /// Otherwise, returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        protected override IEnumerable<String> GetDirectDependents(String name)
        {
            //check for null
            if (name == null)
                throw new ArgumentNullException();

            //check for valid cell name
            if (!Regex.IsMatch(name, @"^[a-zA-Z]+[0-9]+$"))
                throw new InvalidNameException();

            //return its dependents
            return new HashSet<string>(depGraph.GetDependents(name));
        }

    }
}
