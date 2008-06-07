//-----------------------------------------------------------------------
// <copyright file="ExpressionParser.cs" company="Martin Brenn">
//     Alle Rechte vorbehalten. 
// 
//     Die Inhalte dieser Datei sind ebenfalls automatisch unter 
//     der AGPL lizenziert. 
//     http://www.fsf.org/licensing/licenses/agpl-3.0.html
//     Weitere Informationen: http://de.wikipedia.org/wiki/AGPL
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections;
using BurnSystems.Parser.Helper;
using System.Globalization;

namespace BurnSystems.Parser
{
    /// <summary>
    /// Expression parser
    /// </summary>
    public class ExpressionParser
    {
        #region Enumerations

        /// <summary>
        /// Operators
        /// </summary>
        enum Operator
        {
            Unknown,
            Addition,
            Subtraction,
            Multiplication,
            Division,
            Concatenation,
            LogicalAnd,
            LogicalOr,
            LogicalXor,
            LogicalNot,
            And,
            Or,
            Xor,
            Dereference,
            Equal,
            Greater,
            GreaterOrEqual,
            Less,
            LessOrEqual,
            Inequal
        }

        /// <summary>
        /// Type of the literal
        /// </summary>
        enum LiteralType
        {
            Unknown,
            Boolean,
            Integer,
            String,
            Object,
            Variable
        }

        /// <summary>
        /// Current parse mode
        /// </summary>
        enum ParseMode
        {
            /// <summary>
            /// Current text is an expression
            /// </summary>
            Expression,
            /// <summary>
            /// Current text is an operator
            /// </summary>
            Operator,
            /// <summary>
            /// Opens bracket
            /// </summary>
            BracketOpen,
            /// <summary>
            /// Closes bracket
            /// </summary>
            BracketClose
        }


        /// <summary>
        /// Bracketvalue
        /// </summary>
        const int BracketPriority = 20;

        #endregion

        #region Static variables and methods

        static Dictionary<String, Operator> _OperatorTable =
            new Dictionary<string, Operator>();

        /// <summary>
        /// Gets operator by string
        /// </summary>
        /// <param name="strOperator">Operator as string</param>
        /// <returns>Operator or Operator.Unknown of <c>strOperator</c> is invalid</returns>
        static Operator GetOperator(String strOperator)
        {
            Operator oReturn;

            if (_OperatorTable.TryGetValue(strOperator, out oReturn))
            {
                return oReturn;
            }
            return Operator.Unknown;
        }

        /// <summary>
        /// Gets operator priority of a certain operator. 
        /// </summary>
        /// <param name="eOperator">Operator</param>
        /// <returns>Priority</returns>
        static int GetOperatorPriority(Operator eOperator)
        {
            switch (eOperator)
            {
                case Operator.Equal:
                    return 0;
                case Operator.Less:
                    return 0;
                case Operator.LessOrEqual:
                    return 0;
                case Operator.Greater:
                    return 0;
                case Operator.GreaterOrEqual:
                    return 0;
                case Operator.Inequal:
                    return 0;
                case Operator.LogicalAnd:
                    return 1;
                case Operator.LogicalOr:
                    return 1;
                case Operator.LogicalXor:
                    return 1;
                case Operator.And:
                    return 2;
                case Operator.Or:
                    return 2;
                case Operator.Xor:
                    return 2;
                case Operator.LogicalNot:
                    return 3;
                case Operator.Concatenation:
                    return 4;
                case Operator.Addition:
                    return 5;
                case Operator.Subtraction:
                    return 5;
                case Operator.Multiplication:
                    return 6;
                case Operator.Division:
                    return 6;
                case Operator.Dereference:
                    return 7;
            }
            return 0;
        }

        static bool IsLeftPriority(Operator eOperator)
        {
            if (eOperator == Operator.LogicalNot)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Static constructor
        /// </summary>
        static ExpressionParser()
        {
            _OperatorTable["+"] = Operator.Addition;
            _OperatorTable["-"] = Operator.Subtraction;
            _OperatorTable["*"] = Operator.Multiplication;
            _OperatorTable["/"] = Operator.Division;
            _OperatorTable["."] = Operator.Concatenation;
            _OperatorTable["&"] = Operator.And;
            _OperatorTable["|"] = Operator.Or;
            _OperatorTable["^"] = Operator.Xor;
            _OperatorTable["&&"] = Operator.LogicalAnd;
            _OperatorTable["||"] = Operator.LogicalOr;
            _OperatorTable["^^"] = Operator.LogicalXor;
            _OperatorTable["!"] = Operator.LogicalNot;
            _OperatorTable["->"] = Operator.Dereference;
            _OperatorTable["=="] = Operator.Equal;
            _OperatorTable["!="] = Operator.Inequal;
            _OperatorTable[">"] = Operator.Greater;
            _OperatorTable[">="] = Operator.GreaterOrEqual;
            _OperatorTable["<"] = Operator.Less;
            _OperatorTable["<="] = Operator.LessOrEqual;
            _OperatorTable["!="] = Operator.Inequal;
            _OperatorTable["<>"] = Operator.Inequal;
        }

        /// <summary>
        /// Converts an object to a boolean
        /// </summary>
        /// <param name="oObject">Object to be converted</param>
        /// <returns>true, if oObject is true or != 0 and != "" and != null</returns>
        public static bool ConvertToBoolean(object oValue)
        {
            if (oValue == null)
            {
                return false;
            }
            if (oValue is bool)
            {
                return (bool)oValue;
            }
            if (oValue is int)
            {
                return ((int)oValue) != 0;
            }
            String strString = oValue as String;
            if (strString != null)
            {
                return !String.IsNullOrEmpty(strString)
                    && strString != Boolean.FalseString;
            }
            return true;
        }

        /// <summary>
        /// Compares two objects
        /// </summary>
        /// <param name="oA">First object</param>
        /// <param name="oB">Second object</param>
        /// <returns>-1 if oA is smaller than oB, 0 if they are equal, otherwise 1</returns>
        public static int CompareObjects(object oA, object oB)
        {
            if ((oA == null || (oA as String) == String.Empty) &&
                (oB == null || (oB as String) == String.Empty))
            {
                return 0;
            }
            if (oA == null)
            {
                return 1;
            }
            if (oB == null)
            {
                return -1;
            }

            // Konvertiere Int32 zu Int64
            if (oA is int || oA is short)
            {
                oA = Convert.ToInt64(oA, CultureInfo.CurrentUICulture);
            }
            if (oB is int || oB is short)
            {
                oB = Convert.ToInt64(oB, CultureInfo.CurrentUICulture);
            }
            if (oA is short || oA is double)
            {
                oA = Convert.ToDouble(oA, CultureInfo.CurrentUICulture);
                oB = Convert.ToDouble(oB, CultureInfo.CurrentUICulture);
            }
            if (oB is short || oB is double)
            {
                oA = Convert.ToDouble(oA, CultureInfo.CurrentUICulture);
                oB = Convert.ToDouble(oB, CultureInfo.CurrentUICulture);
            }

            String strA = oA as String;
            String strB = oB as String;

            if (strA != null)
            {
                if (strB != null)
                {
                    return String.Compare(strA, strB, StringComparison.CurrentCulture);
                }
                if (oB is int)
                {
                    return String.Compare(strA, oB.ToString(), StringComparison.CurrentCulture);
                }
                return String.Compare(strA, oB.ToString(), StringComparison.CurrentCulture);
            }
            if (strB != null)
            {
                if (strA != null)
                {
                    return String.Compare(strB, strA, StringComparison.CurrentCulture);
                }
                if (oA is int)
                {
                    return String.Compare(strB, oA.ToString(), StringComparison.CurrentCulture);
                }
                return String.Compare(strB, oA.ToString(), StringComparison.CurrentCulture);
            }
            if (oA is long)
            {
                long nA = (long)oA;
                if (strB != null)
                {
                    strA = nA.ToString(CultureInfo.CurrentUICulture);
                    return String.Compare(strA, strB, StringComparison.CurrentCulture);
                }
                if (oB is long)
                {
                    long nB = (long)oB;
                    return nA.CompareTo(nB);
                }                
                return nA.CompareTo(oB);
            }
            if (oB is long)
            {
                long nB = (long)oB;
                if (strA != null)
                {
                    strB = nB.ToString(CultureInfo.CurrentUICulture);
                    return String.Compare(strB, strA, StringComparison.CurrentCulture);
                }
                if (oA is long)
                {
                    long nA = (long)oA;
                    return nB.CompareTo(nA);
                }
                return nB.CompareTo(oA);
            }
            if (oA is double)
            {
                double dA = (double)oA;
                double dB = (double)oB;

                return dA.CompareTo(dB);
            }

            IComparable iCompA = oA as IComparable;
            if (iCompA != null)
            {
                return iCompA.CompareTo(oB);
            }
            return -1;
        }


        #endregion

        /// <summary>
        /// Core
        /// </summary>
        TemplateParser _Core;

        /// <summary>
        /// Current position
        /// </summary>
        int _CurrentPosition;

        /// <summary>
        /// Current operator priority
        /// </summary>
        int _CurrentOperatorPriority;

        /// <summary>
        /// Structure for expressions
        /// </summary>
        class ExpressionStructure
        {
            public object Literal;

            public LiteralType ExpressionType;

            /// <summary>
            /// Creates new expressiontype
            /// </summary>
            /// <param name="oLiteral">Literal</param>
            /// <param name="eType">Type</param>
            public ExpressionStructure(object oLiteral, LiteralType eType)
            {
                Literal = oLiteral;
                ExpressionType = eType;
            }
        }

        /// <summary>
        /// Operator structure
        /// </summary>
        class OperatorStructure
        {
            /// <summary>
            /// Operator
            /// </summary>
            public Operator Operator;

            /// <summary>
            /// Value
            /// </summary>
            public int Priority;

            /// <summary>
            /// Creates new operator structure
            /// </summary>
            /// <param name="oOperator">Operator</param>
            /// <param name="nPriority">Value</param>
            public OperatorStructure(Operator oOperator, int nPriority)
            {
                Operator = oOperator;
                Priority = nPriority;
            }
        }

        /// <summary>
        /// Operator stack
        /// </summary>
        Stack<OperatorStructure> _OperatorStack;

        /// <summary>
        /// Expression
        /// </summary>
        Stack<ExpressionStructure> _ExpressionStack;

        /// <summary>
        /// Flag, if debug is active
        /// </summary>
        bool _Debug;

        /// <summary>
        /// Flag, if debug information should be shown
        /// </summary>
        public bool Debug
        {
            get { return _Debug; }
            set { _Debug = value; }
        }

        /// <summary>
        /// Creates new expression parser
        /// </summary>
        public ExpressionParser(TemplateParser oCore)
        {
            _Core = oCore;
            _ExpressionStack = new Stack<ExpressionStructure>();
            _OperatorStack = new Stack<OperatorStructure>();
        }

        /// <summary>
        /// Parses an expression and returns a boolean value
        /// </summary>
        /// <param name="strExpression">Expression to be parsed</param>
        /// <returns>Flag</returns>
        public bool ParseForBoolean(string strExpression)
        {
            object oResult = Parse(strExpression);
            return ConvertToBoolean(oResult);
        }

        /// <summary>
        /// Parses an expression and returns a string
        /// </summary>
        /// <param name="strVariable">Variable</param>
        public String ParseForString(string strExpression)
        {
            object oResult = Parse(strExpression);
            String strString = oResult as String;
            if (strString != null)
            {
                return strString;
            }

            return String.Format(CultureInfo.CurrentUICulture, "{0}", oResult);
        }

        /// <summary>
        /// Parses an expression
        /// </summary>
        /// <param name="strExpression">Expression to be parsed</param>
        /// <returns>Result</returns>
        public object Parse(String strExpression)
        {
            try
            {
                // Check for empty string

                if (String.IsNullOrEmpty(strExpression))
                {
                    return String.Empty;
                }
                // Initialize
                int nLength = strExpression.Length;
                ParseMode eMode =
                    Char.IsLetterOrDigit(strExpression[0]) || strExpression[0] == '"'
                        || strExpression[0] == '(' ?
                        ParseMode.Expression : ParseMode.Operator;
                bool bOnlyDigits = true;
                bool bLoop = true;
                int nExpressionLength = -1;
                _ExpressionStack = new Stack<ExpressionStructure>();
                _OperatorStack = new Stack<OperatorStructure>();
                _CurrentOperatorPriority = 0;
                bool bQuote = false;
                _CurrentPosition = -1;

                // Start loop
                while (bLoop)
                {
                    // Parses current value till special character

                    _CurrentPosition++;
                    nExpressionLength++;
                    char cCurrentChar;

                    bool bEndExpression = false;

                    if (_CurrentPosition == nLength)
                    {
                        // End of string
                        bEndExpression = true;
                        bLoop = false;
                        cCurrentChar = ' ';
                    }
                    else
                    {
                        cCurrentChar = strExpression[_CurrentPosition];
                        if (cCurrentChar == ')')
                        {
                            bEndExpression = true;
                        }
                        else if (cCurrentChar == '(')
                        {
                            bEndExpression = true;
                        }
                        else
                        {
                            if (cCurrentChar == '"')
                            {
                                bQuote = !bQuote;
                            }
                            switch (eMode)
                            {
                                case ParseMode.Expression:
                                    if (!Char.IsDigit(cCurrentChar)
                                        && !Char.IsLetter(cCurrentChar)
                                        && !Char.IsWhiteSpace(cCurrentChar)
                                        && cCurrentChar != '_'
                                        && cCurrentChar != '"')
                                    {
                                        // No digit, no letter, no quotes, must be an operator
                                        if (!bQuote)
                                        {
                                            bEndExpression = true;
                                        }
                                    }
                                    else if (!Char.IsDigit(cCurrentChar))
                                    {
                                        bOnlyDigits = false;
                                    }
                                    break;
                                case ParseMode.Operator:
                                    if (Char.IsLetterOrDigit(cCurrentChar) || cCurrentChar == '"')
                                    {
                                        // Operator
                                        bEndExpression = true;

                                        bOnlyDigits = Char.IsDigit(cCurrentChar);
                                    }
                                    break;
                                case ParseMode.BracketOpen:
                                    // Ignore
                                    bEndExpression = true;
                                    break;
                                case ParseMode.BracketClose:
                                    // Ignore
                                    bEndExpression = true;
                                    break;
                            }
                        }
                    }

                    // If end of current expression/operator, evaluate it
                    if (bEndExpression)
                    {
                        if (nExpressionLength == 0)
                        {
                            eMode = ParseMode.Expression;
                        }
                        else
                        {
                            String strTemp = strExpression.Substring(_CurrentPosition - nExpressionLength, nExpressionLength);

                            // Executes mode
                            switch (eMode)
                            {
                                case ParseMode.Expression:
                                    EvaluateLiteral(strTemp, bOnlyDigits);
                                    if (bOnlyDigits)
                                    {
                                        WriteDebug("Number: " + strTemp);
                                    }
                                    else
                                    {
                                        WriteDebug("Expression: " + strTemp);
                                    }
                                    eMode = ParseMode.Operator;
                                    break;
                                case ParseMode.Operator:
                                    EvaluateOperator(strTemp);
                                    WriteDebug("Operator:" + strTemp);
                                    eMode = ParseMode.Expression;
                                    break;
                                case ParseMode.BracketClose:
                                    WriteDebug(")");
                                    eMode = ParseMode.Operator;
                                    break;
                                case ParseMode.BracketOpen:
                                    WriteDebug("(");
                                    eMode = ParseMode.Expression;
                                    break;
                            }
                        }

                        // Brackets increases priority of operator, so they are executed before
                        // the operators around the bracket
                        if (cCurrentChar == ')')
                        {
                            eMode = ParseMode.BracketClose;
                            _CurrentOperatorPriority -= BracketPriority;
                        }
                        if (cCurrentChar == '(')
                        {
                            if (eMode == ParseMode.Expression)
                            {
                                // This bracket is for setting priorities
                                eMode = ParseMode.BracketOpen;
                                _CurrentOperatorPriority += BracketPriority;
                            }
                            else
                            {
                                // This bracket is for a function

                                EvaluateFunction(strExpression);

                                // Checks, if function is last operator
                                if (_CurrentPosition >= nLength)
                                {
                                    break;
                                }

                                // Kleiner Sonderfall, der überprüft, ob sich die 
                                // Klammern schlieüen
                                if (strExpression[_CurrentPosition] == ')')
                                {
                                    _CurrentOperatorPriority -= BracketPriority;
                                    eMode = ParseMode.BracketClose;
                                }
                            }
                        }

                        nExpressionLength = 0;
                    }
                }

                while (_OperatorStack.Count > 0)
                {
                    OperatorStructure oOperator = _OperatorStack.Pop();
                    ExecuteOperator(oOperator.Operator);
                }

                return PopObject();
            }
            catch (Exception exc)
            {
                throw new ParserException(
                    String.Format(
                        CultureInfo.CurrentUICulture,
                        Localization_Parser.ExpressionParser_Exception,
                        strExpression,
                        exc.Message),
                    exc);
            }
        }

        /// <summary>
        /// Evaluates the function
        /// </summary>
        /// <param name="strExpression">Complete Expression string</param>
        private void EvaluateFunction(String strExpression)
        {
            char cCurrentChar;

            // Look for closing bracket and set all parameter
            List<object> aParameters = new List<object>();
            // Open brackets
            int nOpenBrackets = 1;
            // Flag, if currently in quote
            bool bQuote = false;
            bool bEscape = false;
            int nParameterLength = 0;
            _CurrentPosition++;

            while (true)
            {
                cCurrentChar = strExpression[_CurrentPosition];
                _CurrentPosition++;

                if (bEscape)
                {
                    bEscape = false;
                    continue;
                }
                else if (cCurrentChar == '\\')
                {
                    bEscape = true;
                }
                else if (cCurrentChar == '"')
                {
                    bQuote = !bQuote;
                    nParameterLength++;
                }
                else if (cCurrentChar == '(' && !bQuote)
                {
                    nOpenBrackets++;
                }
                else if (cCurrentChar == ')' && !bQuote)
                {
                    nOpenBrackets--;
                }
                else if (cCurrentChar == ',' && !bQuote)
                {
                    String strParameter =
                        strExpression.Substring(_CurrentPosition - nParameterLength - 1,
                        nParameterLength);

                    ExpressionParser oParser = new ExpressionParser(_Core);
                    aParameters.Add(oParser.Parse(strParameter));

                    nParameterLength = 0;
                }
                else
                {
                    nParameterLength++;
                }

                if (nOpenBrackets == 0)
                {
                    if (nParameterLength != 0)
                    {
                        String strParameter =
                            strExpression.Substring(_CurrentPosition - nParameterLength - 1,
                            nParameterLength);

                        ExpressionParser oParser = new ExpressionParser(_Core);
                        aParameters.Add(oParser.Parse(strParameter));
                    }

                    // Execute function

                    ExpressionStructure oStructure = _ExpressionStack.Pop();
                    if (oStructure.ExpressionType == LiteralType.Variable)
                    {
                        // Check, if last operator is a dereference pointer

                        String strFunctionName = (String)oStructure.Literal;

                        if (_OperatorStack.Count != 0 &&
                            _OperatorStack.Peek().Operator == Operator.Dereference)
                        {
                            // Ok, have fun, execute function on this object
                            _OperatorStack.Pop();

                            object oInstance = PopObject();

                            object oResult = ExecuteMethod(oInstance, strFunctionName, aParameters);

                            ExpressionStructure oNew = new ExpressionStructure(
                                oResult, LiteralType.Object);
                            _ExpressionStack.Push(oNew);
                        }
                        else
                        {
                            // Variable, execute function

                            object oResult = TemplateParser.ExecuteFunction(strFunctionName, aParameters);

                            ExpressionStructure oNew = new ExpressionStructure(
                                oResult, LiteralType.Object);
                            _ExpressionStack.Push(oNew);
                        }
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// Executes a method on an object
        /// </summary>
        /// <param name="oInstance">Instance</param>
        /// <param name="strFunctionName">Functionname</param>
        /// <param name="aParameters">Parameters</param>
        private static object ExecuteMethod(object oInstance, string strFunctionName, List<object> aParameters)
        {
            if (oInstance == null)
            {
                return null;
            }
            if (oInstance is TimeSpan)
            {
                oInstance = new TimeSpanHelper((TimeSpan)oInstance);
            }
            else if (oInstance is DateTime)
            {
                oInstance = new DateTimeHelper((DateTime)oInstance);
            }
            else if (oInstance is long)
            {
                oInstance = new LongHelper((long)oInstance);
            }
            else if (oInstance is double)
            {
                oInstance = new DoubleHelper((double)oInstance);
            }
            else
            {
                String strInstance = oInstance as String;
                if (strInstance != null)
                {
                    oInstance = new StringHelper(strInstance);
                }
            }

            IParserObject iParserInstance = oInstance as IParserObject;

            if (iParserInstance != null)
            {
                return iParserInstance.ExecuteFunction(strFunctionName, aParameters);
            }
            else
            {
                // Yeah, Reflection fun
                Type[] aoTypes = new Type[aParameters.Count];

                int nCounter = 0;
                foreach (object oParameter in aParameters)
                {
                    aoTypes[nCounter] = oParameter.GetType();
                    nCounter++;
                }

                MethodInfo oMethodInfo = oInstance.GetType().GetMethod(strFunctionName, aoTypes);

                if (oMethodInfo == null)
                {
                    return "";
                }
                else
                {
                    return oMethodInfo.Invoke(oInstance, aParameters.ToArray());
                }
            }
        }

        /// <summary>
        /// Writes a debug message
        /// </summary>
        /// <param name="strString"></param>
        private void WriteDebug(string strString)
        {
            if (_Debug)
            {
                Console.WriteLine(strString);
            }
        }

        /// <summary>
        /// Evaluates the expression and stores it on the stack if necessary
        /// </summary>
        /// <param name="strString">String to be converted to literal</param>
        /// <param name="bOnlyDigits">Flag, if string contains only digits</param>
        private void EvaluateLiteral(String strLiteral, bool bOnlyDigits)
        {
            object oLiteral;
            LiteralType eType = LiteralType.Unknown;

            if (String.IsNullOrEmpty(strLiteral))
            {
                oLiteral = "";
            }
            else if (bOnlyDigits)
            {
                oLiteral = Convert.ToInt32(strLiteral, CultureInfo.CurrentUICulture);
                eType = LiteralType.Integer;
            }
            else
            {
                if (strLiteral[0] == '"' && (strLiteral[strLiteral.Length - 1] == '"'))
                {
                    oLiteral = strLiteral.Substring(1, strLiteral.Length - 2);
                    eType = LiteralType.String;
                }
                else if (strLiteral == "false")
                {
                    oLiteral = false;
                    eType = LiteralType.Boolean;
                }
                else if (strLiteral == "true")
                {
                    oLiteral = true;
                    eType = LiteralType.Boolean;
                }
                else if (strLiteral == "null")
                {
                    oLiteral = null;
                    eType = LiteralType.Object;
                }
                else
                {
                    // Evaluate Variable

                    oLiteral = strLiteral;
                    eType = LiteralType.Variable;
                }
            }

            if (oLiteral != null)
            {
                WriteDebug(oLiteral.ToString() + " " + oLiteral.GetType().Name);
            }
            else
            {
                WriteDebug("null");
            }
            _ExpressionStack.Push(new ExpressionStructure(oLiteral, eType));
        }

        /// <summary>
        /// Pops an object
        /// </summary>
        /// <returns>Object</returns>
        private object PopObject()
        {
            ExpressionStructure oStructure = _ExpressionStack.Pop();
            object oObject = oStructure.Literal;

            if (oStructure.ExpressionType == LiteralType.Variable)
            {
                String strVariableName = (String)oObject;

                // Resolves variable

                object oResult;
                if (_Core.Variables.TryGetValue(strVariableName, out oResult))
                {
                    return oResult;
                }
                else
                {
                    return "";
                }
            }

            return oObject;
        }

        /// <summary>
        /// Pops an object without variable replacement
        /// </summary>
        /// <returns>Expression to be popped</returns>
        private ExpressionStructure PopExpressionStructure()
        {
            return _ExpressionStack.Pop();
        }

        /// <summary>
        /// Pops one integer from expressionstack
        /// </summary>
        /// <returns></returns>
        public int PopInteger()
        {
            object oObject = PopObject();
            if (oObject is int)
            {
                return (int)oObject;
            }
            if (oObject is IConvertible)
            {
                return Convert.ToInt32(oObject, CultureInfo.CurrentUICulture);
            }
            throw new InvalidCastException();
        }

        /// <summary>
        /// Pops one integer from expressionstack
        /// </summary>
        /// <returns></returns>
        public double PopDouble()
        {
            object oObject = PopObject();
            if (oObject is double)
            {
                return (double)oObject;
            }
            if (oObject is IConvertible)
            {
                return Convert.ToDouble(oObject, CultureInfo.CurrentUICulture);
            }
            throw new InvalidCastException();
        }

        /// <summary>
        /// Pops string from expressionstack
        /// </summary>
        /// <returns>String</returns>
        public String PopString()
        {
            object oObject = PopObject();
            String oString = oObject as String;
            if (oString != null)
            {
                return oString;
            }
            return oObject.ToString();
        }

        /// <summary>
        /// Pops a boolean
        /// </summary>
        /// <returns></returns>
        public bool PopBoolean()
        {
            object oObject = PopObject();
            return ConvertToBoolean(oObject);
        }


        /// <summary>
        /// Evaluates operator
        /// </summary>
        /// <param name="strOperator">Operator</param>
        private void EvaluateOperator(String strOperator)
        {
            Operator eOperator = GetOperator(strOperator);
            int nPriority = GetOperatorPriority(eOperator) + _CurrentOperatorPriority;
            bool bIsLeftPriority = IsLeftPriority(eOperator);

            // Checks, if current operator has higher priority than operator on stack

            while (_OperatorStack.Count > 0)
            {
                OperatorStructure oStructure = _OperatorStack.Peek();

                if ((nPriority == oStructure.Priority && bIsLeftPriority) ||
                    (nPriority < oStructure.Priority))
                {
                    // Execute current operator
                    _OperatorStack.Pop();

                    ExecuteOperator(oStructure.Operator);
                }
                else
                {
                    break;
                }
            }

            _OperatorStack.Push(new OperatorStructure(eOperator, nPriority));
        }

        /// <summary>
        /// Executes operator
        /// </summary>
        /// <param name="eOperator">Operator to be executed</param>
        private void ExecuteOperator(Operator eOperator)
        {
            WriteDebug("Execute Operator: " + eOperator.ToString());
            switch (eOperator)
            {
                case Operator.Addition:
                    ExecuteAddition();
                    break;
                case Operator.Subtraction:
                    ExecuteSubtraction();
                    break;
                case Operator.Multiplication:
                    ExecuteMultiplication();
                    break;
                case Operator.Division:
                    ExecuteDivision();
                    break;
                case Operator.Concatenation:
                    ExecuteConcatenation();
                    break;
                case Operator.LogicalAnd:
                    ExecuteLogicalAnd();
                    break;
                case Operator.LogicalOr:
                    ExecuteLogicalOr();
                    break;
                case Operator.LogicalXor:
                    ExecuteLogicalXor();
                    break;
                case Operator.LogicalNot:
                    ExecuteLogicalNot();
                    break;
                case Operator.And:
                    ExecuteAnd();
                    break;
                case Operator.Or:
                    ExecuteOr();
                    break;
                case Operator.Xor:
                    ExecuteXor();
                    break;
                case Operator.Dereference:
                    ExecuteDereference();
                    break;
                case Operator.Equal:
                    ExecuteEqual();
                    break;
                case Operator.Greater:
                    ExecuteGreater();
                    break;
                case Operator.GreaterOrEqual:
                    ExecuteGreaterOrEqual();
                    break;
                case Operator.Less:
                    ExecuteLess();
                    break;
                case Operator.LessOrEqual:
                    ExecuteLessOrEqual();
                    break;
                case Operator.Inequal:
                    ExecuteInequal();
                    break;
            }
        }

        private void ExecuteInequal()
        {

            object oA = PopObject();
            object oB = PopObject();

            bool bIsEqual = CompareObjects(oB, oA) != 0;
            ExpressionStructure oStructure = new ExpressionStructure(bIsEqual, LiteralType.Boolean);
            _ExpressionStack.Push(oStructure);
        }

        private void ExecuteLessOrEqual()
        {

            object oA = PopObject();
            object oB = PopObject();

            bool bIsEqual = CompareObjects(oB, oA) <= 0;
            ExpressionStructure oStructure = new ExpressionStructure(bIsEqual, LiteralType.Boolean);
            _ExpressionStack.Push(oStructure);
        }

        private void ExecuteLess()
        {

            object oA = PopObject();
            object oB = PopObject();

            bool bIsEqual = CompareObjects(oB, oA) < 0;
            ExpressionStructure oStructure = new ExpressionStructure(bIsEqual, LiteralType.Boolean);
            _ExpressionStack.Push(oStructure);
        }

        private void ExecuteGreaterOrEqual()
        {

            object oA = PopObject();
            object oB = PopObject();

            bool bIsEqual = CompareObjects(oB, oA) >= 0;
            ExpressionStructure oStructure = new ExpressionStructure(bIsEqual, LiteralType.Boolean);
            _ExpressionStack.Push(oStructure);
        }

        private void ExecuteGreater()
        {

            object oA = PopObject();
            object oB = PopObject();

            bool bIsEqual = CompareObjects(oB, oA) > 0;
            ExpressionStructure oStructure = new ExpressionStructure(bIsEqual, LiteralType.Boolean);
            _ExpressionStack.Push(oStructure);
        }

        private void ExecuteEqual()
        {
            object oA = PopObject();
            object oB = PopObject();

            bool bIsEqual = CompareObjects(oB, oA) == 0;
            ExpressionStructure oStructure = new ExpressionStructure(bIsEqual, LiteralType.Boolean);
            _ExpressionStack.Push(oStructure);
        }

        /// <summary>
        /// Makes a dereference
        /// </summary>
        private void ExecuteDereference()
        {
            ExpressionStructure oStructure = PopExpressionStructure();
            String strMethod = (String)oStructure.Literal;

            object oObject = PopObject();
            IList iList = oObject as IList;

            if (oObject == null)
            {
                _ExpressionStack.Push(
                    new ExpressionStructure(
                        null, LiteralType.Object));
                return;
            }
            else if (oObject is int)
            {
                // Creates int helper object
            }
            else if (oObject is double)
            {
                oObject = new DoubleHelper((double)oObject);
            }
            else if (oObject is long)
            {
                oObject = new LongHelper((long)oObject);
            }
            else if (oObject is TimeSpan)
            {
                oObject = new TimeSpanHelper((TimeSpan)oObject);
            }
            else if (oObject is DateTime)
            {
                oObject = new DateTimeHelper((DateTime)oObject);
            }
            else if (iList != null)
            {
                oObject = new IListHelper(iList);
            }
            else
            {
                String strObject = oObject as String;
                if (strObject != null)
                {
                    oObject = new StringHelper(strObject);
                }
            }

            IParserObject iParserObject = oObject as IParserObject;
            IDictionary iDictionary = oObject as IDictionary;
            IParserDictionary iParserDictionary = oObject as IParserDictionary;
            if (iParserObject != null)
            {
                _ExpressionStack.Push(
                    new ExpressionStructure(
                        iParserObject.GetProperty(strMethod), LiteralType.Object));
            }
            else if (iDictionary != null)
            {
                _ExpressionStack.Push(new ExpressionStructure(
                    iDictionary[strMethod],
                    LiteralType.Object
                ));
            }
            else if (iParserDictionary != null)
            {
                _ExpressionStack.Push(new ExpressionStructure(
                    iParserDictionary[strMethod],
                    LiteralType.Object
                ));
            }
            else
            {
                // Do reflections

                Type oType = oObject.GetType();
                PropertyInfo oProperty = oType.GetProperty(strMethod);
                object oResult = "";

                if (oProperty != null)
                {
                    oResult = oProperty.GetValue(oObject, null);
                }
                _ExpressionStack.Push(
                    new ExpressionStructure(
                        oResult, LiteralType.Object));
            }
        }

        private void ExecuteLogicalNot()
        {
            bool nA = PopBoolean();

            ExpressionStructure oStructure = new ExpressionStructure(!nA, LiteralType.Boolean);
            _ExpressionStack.Push(oStructure);
        }

        private void ExecuteXor()
        {
            int nA = PopInteger();
            int nB = PopInteger();

            ExpressionStructure oStructure = new ExpressionStructure(nA ^ nB, LiteralType.Integer);
            _ExpressionStack.Push(oStructure);
        }

        private void ExecuteOr()
        {
            int nA = PopInteger();
            int nB = PopInteger();

            ExpressionStructure oStructure = new ExpressionStructure(nA | nB, LiteralType.Integer);
            _ExpressionStack.Push(oStructure);
        }

        private void ExecuteAnd()
        {
            int nA = PopInteger();
            int nB = PopInteger();

            ExpressionStructure oStructure = new ExpressionStructure(nA & nB, LiteralType.Integer);
            _ExpressionStack.Push(oStructure);
        }

        private void ExecuteLogicalXor()
        {
            bool nA = PopBoolean();
            bool nB = PopBoolean();

            ExpressionStructure oStructure = new ExpressionStructure(nA ^ nB, LiteralType.Boolean);
            _ExpressionStack.Push(oStructure);
        }

        private void ExecuteLogicalOr()
        {
            bool nA = PopBoolean();
            bool nB = PopBoolean();

            ExpressionStructure oStructure = new ExpressionStructure(nA || nB, LiteralType.Boolean);
            _ExpressionStack.Push(oStructure);
        }

        private void ExecuteLogicalAnd()
        {
            bool nA = PopBoolean();
            bool nB = PopBoolean();

            ExpressionStructure oStructure = new ExpressionStructure(nA && nB, LiteralType.Boolean);
            _ExpressionStack.Push(oStructure);
        }

        /// <summary>
        /// Executes addition
        /// </summary>
        private void ExecuteAddition()
        {
            int nA = PopInteger();
            int nB = PopInteger();
            ExpressionStructure oStructure = new ExpressionStructure(nA + nB, LiteralType.Integer);
            _ExpressionStack.Push(oStructure);
        }

        /// <summary>
        /// Executes Subtraction
        /// </summary>
        private void ExecuteSubtraction()
        {
            int nA = PopInteger();
            int nB = PopInteger();
            ExpressionStructure oStructure = new ExpressionStructure(nB - nA, LiteralType.Integer);
            _ExpressionStack.Push(oStructure);
        }

        /// <summary>
        /// Executes multiplication
        /// </summary>
        private void ExecuteMultiplication()
        {
            object oA = PopObject();
            object oB = PopObject();

            if (oA is double || oB is double)
            {
                double dA = Convert.ToDouble(oA, CultureInfo.CurrentUICulture);
                double dB = Convert.ToDouble(oB, CultureInfo.CurrentUICulture);
                ExpressionStructure oStructure =
                    new ExpressionStructure(dA * dB, LiteralType.Integer);
                _ExpressionStack.Push(oStructure);
            }
            else
            {
                int nA = Convert.ToInt32(oA, CultureInfo.CurrentUICulture);
                int nB = Convert.ToInt32(oB, CultureInfo.CurrentUICulture);
                ExpressionStructure oStructure =
                    new ExpressionStructure(nA * nB, LiteralType.Integer);
                _ExpressionStack.Push(oStructure);
            }
        }

        /// <summary>
        /// Executes division
        /// </summary>
        private void ExecuteDivision()
        {
            int nA = PopInteger();
            int nB = PopInteger();
            ExpressionStructure oStructure = new ExpressionStructure(nB / nA, LiteralType.Integer);
            _ExpressionStack.Push(oStructure);
        }

        /// <summary>
        /// Executes concatenation
        /// </summary>
        private void ExecuteConcatenation()
        {
            String nA = PopString();
            String nB = PopString();

            ExpressionStructure oStructure = new ExpressionStructure(nB + nA, LiteralType.String);
            _ExpressionStack.Push(oStructure);
        }
    }
}