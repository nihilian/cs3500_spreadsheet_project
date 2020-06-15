///<author>
/// Benjamin Allred
///</author>
///<UID>
/// u1090524
///</UID>

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;
using System.Collections.Generic;
using System.Linq;

namespace FormulaTester
{
    [TestClass]
    public class FormulaTester
    {
        public string DollarToLetterNormalizer(string token)
        {
            if (token.Equals("$"))
            {
                return "A";
            }
            else
            {
                return token;
            }
        }

        public string ToUpperCase(string token)
        {
            return token.ToUpper();
        }

        public bool SimpleValidator(string token)
        {
            if (token.Equals("YOUSHALLNOTPASS"))
            {
                return false;
            }

            return true;
        }

        public bool TwoCharVariables(string token)
        {
            if (token.Length != 2)
            {
                return false;
            }

            return true;
        }

        public double BasicLookup(string s)
        {
            if (s.Equals("A1") || s.Equals("a1"))
                return 5;
            else if (s.Equals("A2"))
                return 6;
            else if (s.Equals("A3"))
                return 2;
            else if (s.Equals("B3"))
                return 27;
            else if (s.Equals("B4"))
                return 40;
            else if (s.Equals("C8"))
                return 19;
            else if (s.Equals("E1"))
                return 0;
            else if (s.Equals("_AA"))
                return 5;
            else if (s.Equals("_234"))
                return 10;
            else if (s.Equals("P"))
                return 15;
            else
                throw new ArgumentException("Invalid Cell Lookup");
        }

        //------------------------PUBLIC TESTS---------------------//
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void PublicTestZeroToken()
        {
            Formula f = new Formula(" ");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void PublicTestInvalidStartToken1()
        {
            Formula f = new Formula(") + - 90");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void PublicTestInvalidStartToken2()
        {
            Formula f = new Formula(" / AB_90 - 49");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void PublicTestInvalidTokenAfterLeftParen()
        {
            Formula f = new Formula("80 - 37 + ( - 904 + 10) /2");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void PublicTestInvalidTokenAfterOperator()
        {
            Formula f = new Formula("80 - * 28 - 94 + (90 / 2) - 1");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void PublicTestMismatchedParentheses()
        {
            Formula f = new Formula("(80 - (28 - 94) * 52) + 90 / 2 - 1)");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void PublicTestExtraFollowingRule()
        {
            Formula f = new Formula("((80 - (28 - 94) * 52) + 80 )90 8 / 2 - 1)");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void PublicTestUnbalancedParentheses()
        {
            Formula f = new Formula("(((80 - (28 - 94) * 52) + 90 / 2 - 1)");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void PublicTestBogusSyntax()
        {
            Formula f = new Formula("5%5");
        }

        [TestMethod]
        public void PublicTestBasicFormula()
        {
            Formula f = new Formula("3 + 5 - 9");
        }

        [TestMethod]
        public void PublicTestNull()
        {
            Formula f = new Formula("3 + 5 - 9");
            Assert.IsFalse(f == null);
        }

        [TestMethod]
        public void PublicTestSimpleNormalizer()
        {
            Formula f = new Formula("$ + 3 - (9 * 7 + 7) + $", DollarToLetterNormalizer, s => true);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void PublicTestFailedNormalizer()
        {
            Formula f = new Formula("$ + 3 - (9 * % + 7) + &", DollarToLetterNormalizer, s => true);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void PublicTestBasicValidator()
        {
            Formula f = new Formula("$ + 3 - (9 * IMGONNPASS + 7) + YOUSHALLNOTPASS", DollarToLetterNormalizer, SimpleValidator);
        }

        [TestMethod]
        public void PublicTestComplexFormula()
        {
            Formula f = new Formula("((((  (A2) - (8 + 28) * (     (2 * (90) - (E1 +      C8))) + (B4) - (A1)   * 1)  ) ) )", DollarToLetterNormalizer, SimpleValidator);
        }

        [TestMethod]
        public void PublicTestGetVariablesEmpty()
        {
            Formula f = new Formula("80 - 2734 / 2 + 87");
            List<string> emptyList = new List<string>();
            List<string> variables = new List<string>(f.GetVariables());
            Assert.IsTrue(emptyList.Count == variables.Count);
        }

        [TestMethod]
        public void PublicTestGetVariables()
        {
            Formula f = new Formula("80 *(_90 + AB - UT234) - 2734 / B_234 + 87");
            List<string> varList = new List<string>() { "_90", "AB", "UT234", "B_234" };
            List<string> variables = new List<string>(f.GetVariables());
            Assert.IsTrue(varList.Count == variables.Count);
        }

        [TestMethod]
        public void PublicTestToString()
        {
            Formula formulaObject = new Formula("80 * (_90 + AB -   UT234) - 2734 / B_234 + 87");
            string formula = "80*(_90+AB-UT234)-2734/B_234+87";
            Assert.AreEqual(formulaObject.ToString(), formula);
        }

        [TestMethod]
        public void PublicTestToStringNormalized()
        {
            Formula formulaObject = new Formula("((((  (a2) - (8 + 28) * (     (2 * (90) - (e1 +      c8))) + (b4) - (a1)   * 1)  ) ) )", ToUpperCase, s => true);
            string formula = "(((((A2)-(8+28)*((2*(90)-(E1+C8)))+(B4)-(A1)*1))))";
            Assert.AreEqual(formulaObject.ToString(), formula);
        }

        [TestMethod]
        public void PublicTestBasicEquals()
        {
            Formula f1 = new Formula("9.89 + 38.11 - 9 / 2");
            Formula f2 = new Formula("9.89+38.11-9/2.0");
            Formula f3 = new Formula("9.89+38.11-9/2");
            Formula f4 = new Formula("9.829+38.11-9/2");
            string s1 = "9.89+38.11-9/2.0";
            Assert.IsTrue(f1.Equals(f2));
            Assert.IsTrue(f1.Equals(f3));
            Assert.IsFalse(f1.Equals(s1));
            Assert.IsFalse(f1.Equals(f4));
        }

        [TestMethod]
        public void PublicTestComplexEquals()
        {
            Formula f1 = new Formula("((((  (a2) - (8 + 28) * (     (2 * (90) - (e1 +      c8))) + (b4) - (a1)   * 1)  ) ) )", ToUpperCase, s => true);
            Formula f2 = new Formula("(((((A2)-(8+28.00)*((2.000*(90.000)-(E1+C8)))+(B4)-(A1)*1))))");
            Assert.IsTrue(f1.Equals(f2));
        }

        [TestMethod]
        public void PublicTestHashCode()
        {
            Formula f1 = new Formula("((((  (a2) - (8 + 28) * (     (2 * (90) - (e1 +      c8))) + (b4) - (a1)   * 1)  ) ) )", ToUpperCase, s => true);
            Formula f2 = new Formula("(((((A2)-(8+28.00)*((2.000*(90.000)-(E1+C8)))+(B4)-(A1)*1))))");
            Formula f3 = new Formula("((((  (a2) - (28 + 8) * (     (2 * (90) - (e1 +      c8))) + (b4) - (a1)   * 1)  ) ) )");
            Assert.AreEqual(f1.GetHashCode(), f2.GetHashCode());
            Assert.AreNotEqual(f1.GetHashCode(), f3.GetHashCode());
        }

        [TestMethod]
        public void PublicTestOverloadedOperators()
        {
            Formula f1 = new Formula("((((  (a2) - (8 + 28) * (     (2 * (90) - (e1 +      c8))) + (b4) - (a1)   * 1)  ) ) )", ToUpperCase, s => true);
            Formula f2 = new Formula("(((((A2)-(8+28.00)*((2.000*(90.000)-(E1+C8)))+(B4)-(A1)*1))))");
            Formula f3 = new Formula("(((((A2)-(28+8)*((2.000*(90.000)-(E1+C8)))+(B4)-(A1)*1))))");
            Formula f4 = null;
            Assert.IsTrue(f1 == f2);
            Assert.IsTrue(f1 != f3);
            Assert.IsTrue(f4 == f4);
            Assert.IsFalse(f4 != f4);
            Assert.IsFalse(f4 == f3);
            Assert.IsTrue(f4 != f3);
        }

        [TestMethod]
        public void PublicTestEvaluate1()
        {
            Formula f1 = new Formula("1 + 2");
            Formula f2 = new Formula("49 - 9");
            Formula f3 = new Formula("9 * 9");
            Formula f4 = new Formula("100 / 5");
            Assert.AreEqual((double)3, f1.Evaluate(BasicLookup));
            Assert.AreEqual((double)40, f2.Evaluate(BasicLookup));
            Assert.AreEqual((double)81, f3.Evaluate(BasicLookup));
            Assert.AreEqual((double)20, f4.Evaluate(BasicLookup));
        }

        [TestMethod]
        public void PublicTestEvaluate2()
        {
            Formula f1 = new Formula("(5+3)  * 7 / 2 - 2 + (4- 2)");
            Formula f2 = new Formula(" (5+3-   3+2*7     /7*7-2)  * 14 / 2 / 1 + (4* 2)  ");
            Formula f3 = new Formula(" (5+3-   3+2*7     /7*7-2)  * 14 / 2 / 1 / (4/ 2)  ");
            double expected = (5.0 + 3.0 - 3.0 + 2.0 * 7.0 / 7.0 * 7.0 - 2.0) * 14.0 / 2.0 / 1.0 / (4.0 / 2.0);
            Assert.AreEqual((double)28, f1.Evaluate(BasicLookup));
            Assert.AreEqual((double)127, f2.Evaluate(BasicLookup));
            Assert.AreEqual(expected, f3.Evaluate(BasicLookup));
        }

        [TestMethod]
        public void PublicTestEvaluate3()
        {
            Formula f1 = new Formula("A1");
            Formula f2 = new Formula("A1 * B4 + (B3 - C8 + E1) - A2 * A3");
            Formula f3 = new Formula("(A2) - 8 + 28 / 2 * (90) - (E1 + C8) + (B4) - (A1)   * 1");
            Formula f4 = new Formula("((((  (a2) - (8 + 28) * (     (2 * (90) - (e1 +      c8))) + (b4) - (a1)   * 1)  ) ) )", ToUpperCase, s => true);
            Assert.AreEqual((double)5, f1.Evaluate(BasicLookup));
            Assert.AreEqual((double)196, f2.Evaluate(BasicLookup));
            Assert.AreEqual((double)1274, f3.Evaluate(BasicLookup));
            Assert.AreEqual((double)-5755, f4.Evaluate(BasicLookup));
        }

        [TestMethod]
        public void PublicTestEvaluate4()
        {
            Formula f1 = new Formula("13332.80395 + 234.32894 - (90354.9 - 9345.23 * (345.0-111.33))");
            Formula f2 = new Formula("A1 * B4 + (B3 - C8 + E1) - A2 * A3 + (90.3822 - 0.9222)");
            double expected = 196 + (90.3822 - 0.9222);
            Assert.AreEqual((13332.80395 + 234.32894 - (90354.9 - 9345.23 * (345.0 - 111.33))), f1.Evaluate(BasicLookup));
            Assert.AreEqual(expected, f2.Evaluate(BasicLookup));
        }

        [TestMethod]
        public void PublicTestScientific()
        {
            Formula f1 = new Formula("5 + 4e2");
            Assert.AreEqual((double)405, f1.Evaluate(BasicLookup));
        }

        [TestMethod]
        public void PublicTestEvaluateExpectedError()
        {
            Formula f1 = new Formula("(A2) - 8 + 28 / 2 * (90) - (E90 + C8) + (B4) - (A1)   * 1");
            Formula f2 = new Formula("(0.002) - 8 + 28 / 0 * (90) - (0 + 2.7) + (150 - 8) - (90)   / 0");
            Formula f3 = new Formula("(0.002) - 8 + 28 / E1 * (90) - (0 + 2.7) + (150 - 8) - (90)   / 0");
            Formula f4 = new Formula("1 + (0.00002 / (9-9))");
            FormulaError fe1 = (FormulaError)f1.Evaluate(BasicLookup);
            FormulaError fe2 = (FormulaError)f2.Evaluate(BasicLookup);
            FormulaError fe3 = (FormulaError)f3.Evaluate(BasicLookup);
            FormulaError fe4 = (FormulaError)f4.Evaluate(BasicLookup);
            Assert.AreEqual("Lookup Error", fe1.Reason);
            Assert.AreEqual("Divide By Zero Error", fe2.Reason);
            Assert.AreEqual("Divide By Zero Error", fe3.Reason);
            Assert.AreEqual("Divide By Zero Error", fe4.Reason);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void PublicTestCusomIsValid()
        {
            Formula f1 = new Formula("(A2) - 8 + 28 / 2 * (90) - (E90 + C8) + (B4) - (A1)   * 1", ToUpperCase, TwoCharVariables);
            Assert.AreEqual((double)5, f1.Evaluate(BasicLookup));
            Formula f2 = new Formula("_234 - 8 + 28 / 2 * (90) - (P + C8) + (B4) - (A1)   * 1", ToUpperCase, TwoCharVariables);
        }

        /// <summary>
        /// Tests construction of a formula with a bunch of random variables
        /// </summary>
        [TestMethod]
        public void PublicTestRandomStringVariables()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ_";
            const string ops = "+-*/";
            string[] variables = new string[1000];
            char[] letters;
            for(int i = 0; i < variables.Length; i++)
            {
                letters = Enumerable.Repeat(chars, random.Next(10, 100)).Select(s => s[random.Next(s.Length)]).ToArray();
                variables[i] = new string(letters);
            }
            string formula = "";
            foreach(string variable in variables)
            {
                formula = formula + variable + ops[random.Next(0, 3)];
            }
            formula = formula + "1";
            Formula f1 = new Formula(formula);
        }


        //------------------------PRIVATE TESTS---------------------//
        [TestMethod]
        public void PrivateTestCheckParenFollow()
        {
            Formula f = new Formula("0");
            PrivateObject privateF = new PrivateObject(f);
            string s1 = "(";
            string s2 = ")";
            Assert.AreEqual(true, privateF.Invoke("CheckParenFollow", s1));
            Assert.AreEqual(false, privateF.Invoke("CheckParenFollow", s2));
        }

        [TestMethod]
        public void PrivateTestCheckExtraFollow()
        {
            Formula f = new Formula("0");
            PrivateObject privateF = new PrivateObject(f);
            string s1 = "-";
            string s2 = ")";
            string s3 = "A";
            Assert.AreEqual(true, privateF.Invoke("CheckExtraFollow", s1));
            Assert.AreEqual(true, privateF.Invoke("CheckExtraFollow", s2));
            Assert.AreEqual(false, privateF.Invoke("CheckExtraFollow", s3));
        }

        [TestMethod]
        public void PrivateTestParse()
        {
            Formula f = new Formula("0");
            PrivateObject privateF = new PrivateObject(f);
            string s1 = "-";
            string s2 = ")";
            string s3 = "A";
            string s4 = "$$";
            string s5 = "_AB23";
            string s6 = "23_A";
            Assert.AreEqual("operator", privateF.Invoke("Parse", s1));
            Assert.AreEqual(")", privateF.Invoke("Parse", s2));
            Assert.AreEqual("value", privateF.Invoke("Parse", s3));
            Assert.AreEqual("bogus", privateF.Invoke("Parse", s4));
            Assert.AreEqual("value", privateF.Invoke("Parse", s5));
            Assert.AreEqual("bogus", privateF.Invoke("Parse", s6));
        }

    }
}
