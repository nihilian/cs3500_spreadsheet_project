using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FormulaEvaluator
{
    /// <summary>
    /// This class provides the delegate template and the code for Evaluate function
    /// </summary>
    public static class Evaluator
    {
        public delegate int Lookup(String v);

        /// <summary>
        /// Takes an arithmetic expression represented as a string and splits
        /// it into tokens. The accepted tokens are '(', ')', '+', '-', '*', '/',
        /// integers, and spreadhsheet cell values. Uses two stacks to evaulate
        /// the expression and returns an integer value. A delegate can be passed
        /// to evaluate the integer value of spreadsheet cells.
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="variableEvaluator"></param>
        /// <returns></returns>
        public static int Evaluate(String exp, Lookup variableEvaluator)
        {
            //start by splitting exp into tokens and setting up stacks
            string[] substrings = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");
            Stack<string> opStack = new Stack<string>();
            Stack<int> valStack = new Stack<int>();

            //loops through each token, using stacks to do arthimetic
            foreach(string tokenW in substrings)
            {
                //remove any leading/trailing whitespace
                string token = tokenW.Trim();
                
                //ignore empty tokens
                if(token.Equals(""))
                {
                    
                }

                //check if token is an integer
                else if(Int32.TryParse(token, out int integer))
                {
                    //check stack for multiply or divide operators
                    if(opStack.OnTop("*"))
                    {
                        opStack.Pop();
                        //multiply current integer with previous value on valStack
                        try
                        {
                            int val = valStack.Pop();
                            valStack.Push(val * integer);
                        }
                        //throw exception if no previous value exists
                        catch(InvalidOperationException)
                        {
                            throw new ArgumentException("Value stack is empty");
                        }  
                    }
                    else if (opStack.OnTop("/"))
                    {
                        opStack.Pop();
                        //divide previous value on stack by current integer
                        try
                        {
                            int val = valStack.Pop();
                            valStack.Push(val / integer);
                        }
                        //throw exceptions for no previous value and divide by zero
                        catch(InvalidOperationException)
                        {
                            throw new ArgumentException("Value stack is empty");
                        }
                        catch(DivideByZeroException)
                        {
                            throw new ArgumentException("Divide by zero");
                        }
                    }
                    //if no operators are found then push the integer onto valStack
                    else
                    {
                        valStack.Push(integer);
                    }
                }

                //check if token meets spreadsheet cell pattern
                else if (Regex.IsMatch(token, pattern: "^[a-zA-Z]+[0-9]+$"))
                {
                    int variable;
                    //get integer value from delegate
                    try
                    {
                        variable = variableEvaluator(token);
                    }
                    //catch and rethrow delegate exception
                    catch(Exception)
                    {
                        throw new ArgumentException("Lookup delegate threw an exception");
                    }

                    //check stack for multiply or divide operators
                    if (opStack.OnTop("*"))
                    {
                        opStack.Pop();
                        //multiply current integer with previous value on valStack
                        try
                        {
                            int val = valStack.Pop();
                            valStack.Push(val * variable);
                        }
                        //throw exception for no previous value
                        catch (InvalidOperationException)
                        {
                            throw new ArgumentException("Value stack is empty");
                        }
                    }
                    else if (opStack.OnTop("/"))
                    {
                        opStack.Pop();
                        //divide previous value on stack by current integer
                        try
                        {
                            int val = valStack.Pop();
                            valStack.Push(val / variable);
                        }
                        //throw exceptions for no previous value and divide by zero
                        catch (InvalidOperationException)
                        {
                            throw new ArgumentException("Value stack is empty");
                        }
                        catch (DivideByZeroException)
                        {
                            throw new ArgumentException("Divide by zero");
                        }
                    }
                    //if no operators are found then push the integer onto valStack
                    else
                    {
                        valStack.Push(variable);
                    }
                }

                //check if token is addition or subtraction operators
                else if(token.Equals("+") || token.Equals("-"))
                {
                    //check for addition/subtraction operators already on opStack
                    if(opStack.OnTop("+"))
                    {
                        opStack.Pop();
                        //get top two values on valStack and add them
                        try
                        {
                            int valRight = valStack.Pop();
                            int valLeft = valStack.Pop();
                            valStack.Push(valLeft + valRight);
                        }
                        //throw exception if there aren't two values on valStack
                        catch(InvalidOperationException)
                        {
                            throw new ArgumentException("Addition Error: value stack contains fewer than 2 operands");
                        }
                    }
                    else if(opStack.OnTop("-"))
                    {
                        opStack.Pop();
                        //get top two values on valStack and subtract them
                        try
                        {
                            int valRight = valStack.Pop();
                            int valLeft = valStack.Pop();
                            valStack.Push(valLeft - valRight);
                        }
                        //throw exception if there aren't two values on valStack
                        catch (InvalidOperationException)
                        {
                            throw new ArgumentException("Subtraction Error: value stack contains fewer than 2 operands");
                        }
                    }
                    //push new add/sub operator onto opStack
                    opStack.Push(token);
                }

                //check if token is multiplication, division, or left paren operator
                else if(token.Equals("*") || token.Equals("/") || token.Equals("("))
                {
                    //simply push it onto the stack
                    opStack.Push(token);
                }

                //check if token is a right paren
                else if(token.Equals(")"))
                {
                    //check for addition/subtraction operators on opStack
                    if (opStack.OnTop("+"))
                    {
                        opStack.Pop();
                        //add two top values in valStack together
                        try
                        {
                            int valRight = valStack.Pop();
                            int valLeft = valStack.Pop();
                            valStack.Push(valLeft + valRight);
                        }
                        //throw exception if two values do not exist
                        catch(InvalidOperationException)
                        {
                            throw new ArgumentException("Addition Error: value stack contains fewer than 2 operands");
                        }
                    }
                    else if (opStack.OnTop("-"))
                    {
                        opStack.Pop();
                        //subtract two values in valStack
                        try
                        {
                            int valRight = valStack.Pop();
                            int valLeft = valStack.Pop();
                            valStack.Push(valLeft - valRight);
                        }
                        //throw exception if two values do not exist
                        catch(InvalidOperationException)
                        {
                            throw new ArgumentException("Subtraction Error: value stack contains fewer than 2 operands");
                        }
                    }

                    //now check for a matching left paren and throw exception if not found
                    if(opStack.OnTop("("))
                    {
                        opStack.Pop();
                    }
                    else
                    {
                        throw new ArgumentException("Mismatched parentheses");
                    }

                    //now check for multiplication or division operators
                    if (opStack.OnTop("*"))
                    {
                        opStack.Pop();
                        //multiply top two values on valStack
                        try
                        {
                            int valRight = valStack.Pop();
                            int valLeft = valStack.Pop();
                            valStack.Push(valLeft * valRight);
                        }
                        //throw exception if two values cannot be found
                        catch(InvalidOperationException)
                        {
                            throw new ArgumentException("Multiplication Error: value stack contains fewer than 2 operands");
                        }
                        
                    }
                    else if (opStack.OnTop("/"))
                    {
                        opStack.Pop();
                        //divide top two values on valStack
                        try
                        {
                            int valRight = valStack.Pop();
                            int valLeft = valStack.Pop();
                            valStack.Push(valLeft / valRight);
                        }
                        //throw execption if two values cannot be found or if divide by zero occurs
                        catch (InvalidOperationException)
                        {
                            throw new ArgumentException("Division Error: value stack contains fewer than 2 operands");
                        }
                        catch(DivideByZeroException)
                        {
                            throw new ArgumentException("Divide by zero");
                        }
                    }
                }
                
                //if the token is invalid throw an exception
                else
                {
                    throw new ArgumentException("Invalid token");
                }
            } //end token scan
            
            //check final state of opStack and valStack
            if(opStack.Count == 0)
            {
                //if there are no operators left then there should only be one value, the return value
                if(valStack.Count == 1)
                {
                    return valStack.Pop();
                }
                //else throw an exception
                else
                {
                    throw new ArgumentException("End Token Scan: Not exactly 1 value on value stack");
                }
            }
            else if(opStack.Count == 1 && valStack.Count == 2)
            {
                //if there is one operator left it should be addition or subtraction
                if(opStack.OnTop("+"))
                {
                    opStack.Pop();
                    int valRight = valStack.Pop();
                    int valLeft = valStack.Pop();
                    return (valLeft + valRight);
                }
                else if (opStack.OnTop("-"))
                {
                    opStack.Pop();
                    int valRight = valStack.Pop();
                    int valLeft = valStack.Pop();
                    return (valLeft - valRight);
                }
                //else throw an exception
                else
                {
                    throw new ArgumentException("End Token Scan: Invalid operator");
                }
            }
            else
            {
                //for all other final states of opStack and valStack throw an exception
                throw new ArgumentException("End Token Scan: Invalid opStack/valStack count");
            }
        }
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
            if(stack.Count > 0 && stack.Peek().Equals(val))
            {
                return true;
            }
            return false;
        }
    }
}
