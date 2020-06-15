///<author>
///Benjamin Allred
///</author>
///<UID>
///u1090524
///</UID>

using SpreadsheetUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS
{
    internal class Cell
    {
        private string name;
        private object contents, value;
        private Func<string, double> lookup;
        private Func<string, bool> isValid;
        private Func<string, string> normalize;

        /// <summary>
        /// Constructs a Cell with name and contents values. Store delegates for Formula. Calculates the value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="contents"></param>
        /// <param name="isValid"></param>
        /// <param name="normalize"></param>
        /// <param name="lookup"></param>
        public Cell(string name, object contents, Func<string, bool> isValid, Func<string, string> normalize, Func<string, double> lookup)
        {
            this.name = name;
            this.contents = contents;
            this.lookup = lookup;
            this.isValid = isValid;
            this.normalize = normalize;
            CalculateValue();
        }

        /// <summary>
        /// Returns cell name
        /// </summary>
        public string Name
        {
            get { return name; }
            private set { }
        }

        /// <summary>
        /// Returns cell contents
        /// </summary>
        public object Contents
        {
            get { return contents; }
            private set { }
        }

        /// <summary>
        /// Returns cell value
        /// </summary>
        public object Value
        {
            get { return value; }
            private set { }
        }

        /// <summary>
        /// Called upon Cell instantiation and whenever there are potential depencies/contents changes.
        /// </summary>
        public void CalculateValue()
        {
            if (contents is double)
                value = contents;

            else if (contents is Formula)
            {
                Formula formula = new Formula(contents.ToString(), normalize, isValid);
                value = formula.Evaluate(lookup);
            }  

            else
            {
                value = contents;
            }
        }

        /// <summary>
        /// Returns hashcode of Cell name.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
    }
}

