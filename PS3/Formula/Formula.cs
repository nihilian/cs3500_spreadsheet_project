///<author>
/// Benjamin Allred
///</author>
///<UID>
/// u1090524
///</UID>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpreadsheetUtilities
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  The allowed symbols are non-negative numbers written using double-precision 
    /// floating-point syntax; variables that consist of a letter or underscore followed by 
    /// zero or more letters, underscores, or digits; parentheses; and the four operator 
    /// symbols +, -, *, and /.  
    /// 
    /// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
    /// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable; 
    /// and "x 23" consists of a variable "x" and a number "23".
    /// 
    /// Associated with every formula are two delegates:  a normalizer and a validator.  The
    /// normalizer is used to convert variables into a canonical form, and the validator is used
    /// to add extra restrictions on the validity of a variable (beyond the standard requirement 
    /// that it consist of a letter or underscore followed by zero or more letters, underscores,
    /// or digits.)  Their use is described in detail in the constructor and method comments.
    /// </summary>
    public class Formula
    {
        private List<String> tokens;
        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically invalid,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer is the identity function, and the associated validator
        /// maps every string to true.  
        /// </summary>
        public Formula(String formula) :
            this(formula, s => s, s => true)
        {
        }

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically incorrect,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer and validator are the second and third parameters,
        /// respectively.  
        /// 
        /// If the formula contains a variable v such that normalize(v) is not a legal variable, 
        /// throws a FormulaFormatException with an explanatory message. 
        /// 
        /// If the formula contains a variable v such that isValid(normalize(v)) is false,
        /// throws a FormulaFormatException with an explanatory message.
        /// 
        /// Suppose that N is a method that converts all the letters in a string to upper case, and
        /// that V is a method that returns true only if a string consists of one letter followed
        /// by one digit.  Then:
        /// 
        /// new Formula("x2+y3", N, V) should succeed
        /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
        /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
        /// </summary>
        public Formula(String formula, Func<string, string> normalize, Func<string, bool> isValid)
        {
            //tokenize string
            try
            {
                tokens = new List<String>(GetTokens(formula, normalize, isValid));
            }
            catch(Exception exc)
            {
                throw new FormulaFormatException("Invalid token: " + exc.Message);
            }

            //ensure there is at least one token
            if(tokens.Count == 0)
                throw new FormulaFormatException("Formula cannot be empty");

            //check first token's syntax
            if(!CheckFirst())
                throw new FormulaFormatException("Invalid syntax starting token: " + tokens.First());
            //check last token's syntax
            if(!CheckLast())
                throw new FormulaFormatException("Invalid syntax ending token: " + tokens.Last());

            //keep track of last seen token and number of parentheses
            string lastSeenType = "N/A";
            int leftParenCount = 0;
            int rightParenCount = 0;
            
            //iterate through each token
            foreach (string token in tokens)
            {
                //check the parenthesis following rule
                if((lastSeenType.Equals("(") || lastSeenType.Equals("operator")) && !CheckParenFollow(token))
                    throw new FormulaFormatException("Invalid syntax after left paren: " + token);

                //check the extra following rule
                if((lastSeenType.Equals(")") || lastSeenType.Equals("value")) && !CheckExtraFollow(token))
                   throw new FormulaFormatException("Invalid syntax extra follow rule: " + token);

                lastSeenType = Parse(token);

                //keep track of parentheses
                if (lastSeenType.Equals("("))
                {
                    leftParenCount++;
                }
                    
                else if (lastSeenType.Equals(")"))
                {
                    rightParenCount++;

                    //check for mismatched parentheses
                    if (rightParenCount > leftParenCount)
                        throw new FormulaFormatException("Mismatched parentheses");
                }
                
                //check for invalid token
                else if (lastSeenType.Equals("bogus"))
                {
                    throw new FormulaFormatException("Invalid syntax: " + token);
                }
            }

            //check for balanced parentheses
            if (leftParenCount != rightParenCount)
                throw new FormulaFormatException("Unbalanced parentheses");
        }

        /// <summary>
        /// Parses tokens into simpler forms for further use. Returns "bogus" for an invalid token.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>token type</returns>
        private string Parse(string token)
        {
            if (Regex.IsMatch(token, @"^[a-zA-Z_]+[0-9a-zA-Z_]*$")
                     || Double.TryParse(token, out double parsedToken))
            {
                return "value";
            }

            else if (Regex.IsMatch(token, @"[\+\-*/]"))
            {
                return "operator";
            }
            
            else if (token.Equals("(") || token.Equals(")"))
            {
                return token;
            }

            else
            {
                return "bogus";
            }
        }

        /// <summary>
        /// Helper method for Formula constructor. Checks if the first token is valid (either an opening paren,
        /// a double value, or a variable.
        /// </summary>
        /// <returns>state of first token</returns>
        private bool CheckFirst()
        {
            if (!Regex.IsMatch(tokens.First(), pattern: "^[a-zA-Z_]+[0-9a-zA-Z_]*$")
                    && !tokens.First().Equals("(")
                    && !Double.TryParse(tokens.First(), out double parsedToken))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Helper method for Formula constructor. Checks if the last token is valid (either a closing paren,
        /// a double value, or a variable.
        /// </summary>
        /// <returns>state of last token</returns>
        private bool CheckLast()
        {
            if (!Regex.IsMatch(tokens.Last(), pattern: "^[a-zA-Z_]+[0-9a-zA-Z_]*$")
                    && !tokens.Last().Equals(")")
                    && !Double.TryParse(tokens.Last(), out double parsedToken))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Helper method for Formula constructor. Is called after a left paren or operator is seen.
        /// Checks to see if the next token is a valid following token.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>state of token following left paren/operator</returns>
        private bool CheckParenFollow(string token)
        {
            if (!Regex.IsMatch(token, pattern: "^[a-zA-Z_]+[0-9a-zA-Z_]*$")
                    && !token.Equals("(")
                    && !Double.TryParse(token, out double parsedToken))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Helper method for Formula constructor. Is called after a number, variable, or closing paren is seen.
        /// Checks to make sure the next token is valid.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>state if token following number, variable, closing paren</returns>
        private bool CheckExtraFollow(string token)
        {
            if (!Regex.IsMatch(token, @"[)\+\-*/]"))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Evaluates this Formula, using the lookup delegate to determine the values of
        /// variables.  When a variable symbol v needs to be determined, it should be looked up
        /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to 
        /// the constructor.)
        /// 
        /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters 
        /// in a string to upper case:
        /// 
        /// new Formula("x+7", N, s => true).Evaluate(L) is 11
        /// new Formula("x+7").Evaluate(L) is 9
        /// 
        /// Given a variable symbol as its parameter, lookup returns the variable's value 
        /// (if it has one) or throws an ArgumentException (otherwise).
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.  
        /// The Reason property of the FormulaError should have a meaningful explanation.
        ///
        /// This method should never throw an exception.
        /// </summary>
        public object Evaluate(Func<string, double> lookup)
        {
            Stack<string> opStack = new Stack<string>();
            Stack<double> valStack = new Stack<double>();
            double value;

            //loops through each token, using stacks to do arthimetic
            foreach (string token in tokens)
            {
                //check if token is an integer
                if (Double.TryParse(token, out value))
                {
                    try
                    {
                        if (opStack.Count > 0)
                            MultiplyOrDivide(value, opStack, valStack, opStack.Peek());
                        else
                            MultiplyOrDivide(value, opStack, valStack, "N/A");
                    }
                    catch(DivideByZeroException)
                    {
                        return new FormulaError("Divide By Zero Error");
                    }
                    
                }

                //check if token meets spreadsheet cell pattern
                else if (Regex.IsMatch(token, pattern: "^[a-zA-Z_]+[a-zA-Z0-9_]*$"))
                {
                    try
                    {
                        value = lookup(token);

                        if (opStack.Count > 0)
                            MultiplyOrDivide(value, opStack, valStack, opStack.Peek());
                        else
                            MultiplyOrDivide(value, opStack, valStack, "N/A");
                    }
                    catch (ArgumentException)
                    {
                        return new FormulaError("Lookup Error");
                    }
                    catch (DivideByZeroException)
                    {
                        return new FormulaError("Divide By Zero Error");
                    }
                }

                //check if token is addition or subtraction operators
                else if (token.Equals("+") || token.Equals("-"))
                {
                    if (opStack.Count > 0)
                        AdditionOrSubtraction(token, opStack, valStack, opStack.Peek());
                    else
                        AdditionOrSubtraction(token, opStack, valStack, "N/A");
                }

                //check if token is multiplication, division, or left paren operator
                else if (token.Equals("*") || token.Equals("/") || token.Equals("("))
                {
                    opStack.Push(token);
                }

                //check if token is a right paren
                else if (token.Equals(")"))
                {
                    //check for addition/subtraction operators on opStack
                    if (opStack.OnTop("+") || opStack.OnTop("-"))
                    {
                        AdditionOrSubtraction("N/A", opStack, valStack, opStack.Peek());
                    }

                    //pop left paren
                    opStack.Pop();

                    //now check for multiplication or division operators
                    if (opStack.OnTop("*") || opStack.OnTop("/"))
                    {
                        try
                        {
                            double rightVal = valStack.Pop();
                            double leftVal = valStack.Pop();
                            MultiplyOrDivide(leftVal, rightVal, opStack, valStack, opStack.Peek());
                        }
                        catch (DivideByZeroException)
                        {
                            return new FormulaError("Divide By Zero Error");
                        }
                    }
                }
            }

            //check if one last addition/subtraction needs to be executed
            if (opStack.Count == 1)
                AdditionOrSubtraction("N/A", opStack, valStack, opStack.Peek());

            //return value on stack
            return valStack.Pop();
        }

        /// <summary>
        /// Helper method for Evaluate. Handles all multiplication and division by directly modifying the stacks.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="opStack"></param>
        /// <param name="valStack"></param>
        /// <param name="op"></param>
        private void MultiplyOrDivide(double value, Stack<string> opStack, Stack<double> valStack, string op)
        {
            if(op.Equals("*"))
            {
                opStack.Pop();
                double val = valStack.Pop();
                valStack.Push(val * value);
            }
            
            else if(op.Equals("/"))
            {
                opStack.Pop();
                double val = valStack.Pop();

                //check if divide by zero occurs
                if (Double.IsInfinity(val / value))
                    throw new DivideByZeroException();

                valStack.Push(val / value);
            }

            else
            {
                valStack.Push(value);
            }
        }

        /// <summary>
        /// Special case of MultiplyOrDivide when two values need to be popped from valStack.
        /// </summary>
        /// <param name="leftVal"></param>
        /// <param name="rightVal"></param>
        /// <param name="opStack"></param>
        /// <param name="valStack"></param>
        /// <param name="op"></param>
        private void MultiplyOrDivide(double leftVal, double rightVal, Stack<string> opStack, Stack<double> valStack, string op)
        {
            opStack.Pop();

            if (op.Equals("*"))
                valStack.Push(leftVal * rightVal);

            else
            {
                //check if divide by zero occurs
                if (Double.IsInfinity(leftVal / rightVal))
                    throw new DivideByZeroException();

                valStack.Push(leftVal / rightVal);
            }
                
        }

        /// <summary>
        /// Helper method for Evaluate. Handles all addition and subtraction by directly modifying the stacks.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="opStack"></param>
        /// <param name="valStack"></param>
        /// <param name="op"></param>
        private void AdditionOrSubtraction(string token, Stack<string> opStack, Stack<double> valStack, string op)
        {
            if(op.Equals("+"))
            {
                opStack.Pop();
                double valRight = valStack.Pop();
                double valLeft = valStack.Pop();
                valStack.Push(valLeft + valRight);
            }
            
            else if(op.Equals("-"))
            {
                opStack.Pop();
                double valRight = valStack.Pop();
                double valLeft = valStack.Pop();
                valStack.Push(valLeft - valRight);
            }

            if(!token.Equals("N/A"))
                opStack.Push(token);
        }

        /// <summary>
        /// Enumerates the normalized versions of all of the variables that occur in this 
        /// formula.  No normalization may appear more than once in the enumeration, even 
        /// if it appears more than once in this Formula.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
        /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
        /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
        /// </summary>
        public IEnumerable<String> GetVariables()
        {
            foreach(string token in tokens)
            {
                if (Regex.IsMatch(token, @"^[a-zA-Z_]+[0-9a-zA-Z_]*$"))
                    yield return token;
            }
        }

        /// <summary>
        /// Returns a string containing no spaces which, if passed to the Formula
        /// constructor, will produce a Formula f such that this.Equals(f).  All of the
        /// variables in the string should be normalized.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
        /// new Formula("x + Y").ToString() should return "x+Y"
        /// </summary>
        public override string ToString()
        {
            string formula = "";
            foreach(string token in tokens)
            {
                if (Double.TryParse(token, out double parsed))
                    formula = formula + parsed.ToString();
                else
                    formula = formula + token;
            }
            return formula;
        }

        /// <summary>
        /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
        /// whether or not this Formula and obj are equal.
        /// 
        /// Two Formulae are considered equal if they consist of the same tokens in the
        /// same order.  To determine token equality, all tokens are compared as strings 
        /// except for numeric tokens and variable tokens.
        /// Numeric tokens are considered equal if they are equal after being "normalized" 
        /// by C#'s standard conversion from string to double, then back to string. This 
        /// eliminates any inconsistencies due to limited floating point precision.
        /// Variable tokens are considered equal if their normalized forms are equal, as 
        /// defined by the provided normalizer.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        ///  
        /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
        /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
        /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
        /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
        /// </summary>
        public override bool Equals(object obj)
        {
            if (!(obj.GetType() == typeof(Formula)))
                return false;

            if (ToString().Equals(obj.ToString()))
                return true;

            return false;
        }

        /// <summary>
        /// Reports whether f1 == f2, using the notion of equality from the Equals method.
        /// Note that if both f1 and f2 are null, this method should return true.  If one is
        /// null and one is not, this method should return false.
        /// </summary>
        public static bool operator ==(Formula f1, Formula f2)
        {
            if ((object)f1 == null)
            {
                if ((object)f2 == null)
                    return true;

                return false;
            }

            else if ((object)f2 == null)
                return false;

            return f1.ToString().Equals(f2.ToString());
        }

        /// <summary>
        /// Reports whether f1 != f2, using the notion of equality from the Equals method.
        /// Note that if both f1 and f2 are null, this method should return false.  If one is
        /// null and one is not, this method should return true.
        /// </summary>
        public static bool operator !=(Formula f1, Formula f2)
        {
            if ((object)f1 == null)
            {
                if ((object)f2 == null)
                    return false;

                return true;
            }

            else if ((object)f2 == null)
                return true;

            return !(f1.ToString().Equals(f2.ToString()));
        }

        /// <summary>
        /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
        /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two 
        /// randomly-generated unequal Formulae have the same hash code should be extremely small.
        /// </summary>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <summary>
        /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
        /// right paren; one of the four operator symbols; a string consisting of a letter or underscore
        /// followed by zero or more letters, digits, or underscores; a double literal; and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space. Normalize
        /// and isValid are both used during this method.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula, Func<string, string> normalize, Func<string, bool> isValid)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
            String spacePattern = @"\s+";

            String normalized;

            // Overall pattern
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    normalized = normalize(s);

                    if (!isValid(normalized))
                        throw new Exception(normalized);

                    yield return normalized;
                }
            }

        }
    }

    /// <summary>
    /// Used to report syntactic errors in the argument to the Formula constructor.
    /// </summary>
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Used as a possible return value of the Formula.Evaluate method.
    /// </summary>
    public struct FormulaError
    {
        /// <summary>
        /// Constructs a FormulaError containing the explanatory reason.
        /// </summary>
        /// <param name="reason"></param>
        public FormulaError(String reason)
            : this()
        {
            Reason = reason;
        }

        /// <summary>
        ///  The reason why this FormulaError was created.
        /// </summary>
        public string Reason { get; private set; }
    }

    /// <summary>
    /// This class contains extensions for the Stack class
    /// </summary>
    public static class StackExtension
    {
        /// <summary>
        /// Checks if desired value is on top of the stack. Returns true or false.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stack"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static bool OnTop<T>(this Stack<T> stack, T val)
        {
            if (stack.Count > 0 && stack.Peek().Equals(val))
            {
                return true;
            }
            return false;
        }
    }
}
