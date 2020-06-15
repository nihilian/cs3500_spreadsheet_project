using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FormulaEvaluator;

namespace EvaluatorTests
{
    /// <summary>
    /// This class provides code for testing a FormulaEvaluator
    /// </summary>
    class Tester
    {
        /// <summary>
        /// A simple method matching the signature for a Lookup delegate
        /// This delegate hard-codes the value of a single variable, B4.
        /// </summary>
        /// <param name="s">The name of the variable to lookup</param>
        /// <returns>The value of the variable if known, throws otherwise</returns>
        public static int simpleLookup(string s)
        {
            if (s == "B4")
                return 17;

            throw new ArgumentException("Unknown variable");
        }

        static void Main(string[] args)
        {
            // We can invoke the evaluator with a simple static Lookup method, like so
            Console.WriteLine(Evaluator.Evaluate("1 + 2 + B4", simpleLookup));

            // Or we can make an object to hold variable values
            // See the definition of FakeSpreadsheet below
            FakeSpreadsheet sheet = new FakeSpreadsheet();

            // This fake spreadsheet has values for two variables, A1, and A2
            sheet.SetCell("A1", 12);
            sheet.SetCell("A2", 100);

            // Then invoke the evaluator with sheet's GetCell method acting as the Lookup delegate
            Console.WriteLine(Evaluator.Evaluate("1 + 2 + A1", sheet.GetCell));

            // Pause the console
            Console.Read();
        }
    }



    /// <summary>
    /// A class for providing functionality similar to a basic spreadsheet.
    /// This class simply provides a mapping from variable names to their values.
    /// Use this class to provide a variable Lookup delegate for writing tests
    /// for your Evaluate method.
    /// </summary>
    class FakeSpreadsheet
    {
        // The dictionary holding the variables and their values
        // These are the fake spreadsheet "cells"
        private Dictionary<string, int> cells;

        /// <summary>
        /// A simple constructor. Just initialize the dictionary.
        /// </summary>
        public FakeSpreadsheet()
        {
            cells = new Dictionary<string, int>();
        }

        /// <summary>
        /// Sets the value of a certain variable
        /// </summary>
        /// <param name="cellName">The name of the variable (or "cell")</param>
        /// <param name="val">The value of the variable</param>
        public void SetCell(string cellName, int val)
        {
            cells[cellName] = val;
        }

        /// <summary>
        /// Gets the value of a certain variable
        /// </summary>
        /// <param name="cellName">The name of the variable (or "cell")</param>
        /// <returns>The value of the specified variable.</returns>
        public int GetCell(string cellName)
        {
            if (!cells.ContainsKey(cellName))
                throw new ArgumentException("unknown variable");

            return cells[cellName];
        }
    }
}




// Pseudocode for Evaluate
// Your real Evaluate method will go in another file, in your FormulaEvaluator project
/* 
public static int Evaluate(String exp, Lookup variableEvaluator)
{
    split exp in to tokens
    foreach token
      remove leading and trailing whitespace from token
      if(token is number)
        ... follow algorithm
      if(token is '+')
        ... follow algorithm     

      if(token is variable)
        val = variableEvaluator(token)
        ... follow algorithm
}
*/
