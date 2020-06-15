using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FormulaEvaluator;

namespace FEUnitTest
{
    [TestClass]
    public class FEUnitTest
    {
        public static int emptyLookup(string s)
        {
            throw new ArgumentException("This function should not be used");
        }

        public static int sampleLookup(string s)
        {
            if (s.Equals("A1") || s.Equals("a1"))
                return 5;
            else if (s.Equals("A2") || s.Equals("a2"))
                return 6;
            else if (s.Equals("A3") || s.Equals("a3"))
                return 2;
            else if (s.Equals("B3") || s.Equals("b3"))
                return 27;
            else if (s.Equals("B4") || s.Equals("b4"))
                return 40;
            else if (s.Equals("C8") || s.Equals("c8"))
                return 19;
            else if (s.Equals("E1") || s.Equals("e1"))
                return 0;
            else
                throw new ArgumentException("Invalid Cell Lookup");
        }

        [TestMethod]
        public void TestBasicAddition()
        {
            Assert.AreEqual(3, Evaluator.Evaluate("1 + 2", emptyLookup));
        }

        [TestMethod]
        public void TestBasicSubtraction()
        {
            Assert.AreEqual(40, Evaluator.Evaluate("49 - 9", emptyLookup));
        }

        [TestMethod]
        public void TestBasicMultiplication()
        {
            Assert.AreEqual(81, Evaluator.Evaluate("9 * 9", emptyLookup));
        }

        [TestMethod]
        public void TestBasicDivision()
        {
            Assert.AreEqual(20, Evaluator.Evaluate("100 / 5", emptyLookup));
        }

        [TestMethod]
        public void TestComplexEq1()
        {
            Assert.AreEqual(28, Evaluator.Evaluate("(5+3)  * 7 / 2 - 2 + (4- 2)", emptyLookup));
        }

        [TestMethod]
        public void TestComplexEq2()
        {
            Assert.AreEqual(127, Evaluator.Evaluate(" (5+3-   3+2*7     /7*7-2)  * 14 / 2 / 1 + (4* 2)  ", emptyLookup));
        }

        [TestMethod]
        public void TestBasicLookup()
        {
            Assert.AreEqual(5, Evaluator.Evaluate("A1", sampleLookup));
        }

        [TestMethod]
        public void TestComplexLookup1()
        {
            Assert.AreEqual(196, Evaluator.Evaluate("A1 * B4 + (B3 - C8 + E1) - A2 * A3", sampleLookup));
        }

        [TestMethod]
        public void TestComplexLookup2()
        {
            Assert.AreEqual(1274, Evaluator.Evaluate("(A2) - 8 + 28 / 2 * (90) - (E1 + C8) + (B4) - (A1)   * 1", sampleLookup));
        }

        [TestMethod]
        public void TestComplexLookup3()
        {
            Assert.AreEqual(-5755, Evaluator.Evaluate("((((  (A2) - (8 + 28) * (     (2 * (90) - (E1 +      C8))) + (B4) - (A1)   * 1)  ) ) )", sampleLookup));
        }

        [TestMethod]
        public void TestLowercaseLookup()
        {
            Assert.AreEqual(-5755, Evaluator.Evaluate("((((  (a2) - (8 + 28) * (     (2 * (90) - (e1 +      c8))) + (b4) - (a1)   * 1)  ) ) )", sampleLookup));
        }

        [TestMethod]
        public void TestInvalidTokenException1()
        {
            try
            {
                Evaluator.Evaluate("A1 * B4 + 3 - 2 * (18 _ 9) + 8", sampleLookup);
            }
            catch(ArgumentException exception)
            {
                Assert.AreEqual(exception.Message, "Invalid token");
            }
        }

        [TestMethod]
        public void TestInvalidTokenException2()
        {
            try
            {
                Evaluator.Evaluate("(8  - 9   ) * 1BB - 234 + A3", sampleLookup);
            }
            catch (ArgumentException exception)
            {
                Assert.AreEqual(exception.Message, "Invalid token");
            }
        }

        [TestMethod]
        public void TestInvalidTokenException3()
        {
            try
            {
                Evaluator.Evaluate("(AA - BB) * 564 - 234 + A3", sampleLookup);
            }
            catch (ArgumentException exception)
            {
                Assert.AreEqual(exception.Message, "Invalid token");
            }
        }

        [TestMethod]
        public void TestInvalidTokenException4()
        {
            try
            {
                Evaluator.Evaluate("((B4 - 90 - 843) / 2 * 9 0", sampleLookup);
            }
            catch (ArgumentException exception)
            {
                Assert.AreEqual(exception.Message, "Invalid token");
            }
        }

        [TestMethod]
        public void TestDivideByZeroException()
        {
            try
            {
                Evaluator.Evaluate("A1 * B4 + (18 / 0) + 10", sampleLookup);
            }
            catch (ArgumentException exception)
            {
                Assert.AreEqual(exception.Message, "Divide by zero");
            }
        }

        [TestMethod]
        public void TestEmptyValueStackException1()
        {
            try
            {
                Evaluator.Evaluate("* 1 + 345 - 93 * 90", sampleLookup);
            }
            catch (ArgumentException exception)
            {
                Assert.AreEqual(exception.Message, "Value stack is empty");
            }
        }

        [TestMethod]
        public void TestEmptyValueStackException2()
        {
            try
            {
                Evaluator.Evaluate("/  / 93 *   90", sampleLookup);
            }
            catch (ArgumentException exception)
            {
                Assert.AreEqual(exception.Message, "Value stack is empty");
            }
        }

        [TestMethod]
        public void TestDelegateException()
        {
            try
            {
                Evaluator.Evaluate("67 - A1 * A2 * (F4 - E1)", sampleLookup);
            }
            catch (ArgumentException exception)
            {
                Assert.AreEqual(exception.Message, "Lookup delegate threw an exception");
            }
        }

        [TestMethod]
        public void TestAdditionException()
        {
            try
            {
                Evaluator.Evaluate("+ 43 + 29 +3", sampleLookup);
            }
            catch (ArgumentException exception)
            {
                Assert.AreEqual(exception.Message, "Addition Error: value stack contains fewer than 2 operands");
            }
        }

        [TestMethod]
        public void TestSubtractionException()
        {
            try
            {
                Evaluator.Evaluate("- 93 + 23 - 94 - 0", sampleLookup);
            }
            catch (ArgumentException exception)
            {
                Assert.AreEqual(exception.Message, "Subtraction Error: value stack contains fewer than 2 operands");
            }
        }

        [TestMethod]
        public void TestMultiplicationException()
        {
            try
            {
                Evaluator.Evaluate("* (33 - 3)", sampleLookup);
            }
            catch (ArgumentException exception)
            {
                Assert.AreEqual(exception.Message, "Multiplication Error: value stack contains fewer than 2 operands");
            }
        }

        [TestMethod]
        public void TestDivisionException()
        {
            try
            {
                Evaluator.Evaluate("/ (3 - E1)", sampleLookup);
            }
            catch (ArgumentException exception)
            {
                Assert.AreEqual(exception.Message, "Division Error: value stack contains fewer than 2 operands");
            }
        }

        [TestMethod]
        public void TestEndTokenScanException()
        {
            try
            {
                Evaluator.Evaluate("(3) - 3 + ** A3", sampleLookup);
            }
            catch (ArgumentException exception)
            {
                Assert.AreEqual(exception.Message, "End Token Scan: Invalid opStack/valStack count");
            }
        }
    }
}
