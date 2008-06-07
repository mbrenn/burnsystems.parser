//-----------------------------------------------------------------------
// <copyright file="TemplateParser.cs" company="Martin Brenn">
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
using System.IO;
using System.Collections;
using System.Globalization;

namespace BurnSystems.Parser
{
    /// <summary>
    /// Dieser Delegate wird genutzt um externe Kommandos einzubinden
    /// </summary>
    /// <param name="oParser">Der Parser selbst</param>
    /// <param name="strCommand">Das Kommando</param>
    /// <returns></returns>
    public delegate String ExternalCommand
        (TemplateParser oParser, String strCommand);

    /// <summary>
    /// This is the core
    /// </summary>
    public class TemplateParser
    {
        #region Classes for internal stack

        abstract class ScopeStack
        {
            /// <summary>
            /// End-Position of statement, which created this stack
            /// </summary>
            public int EndPosition;
        }

        class ScopeIfStack : ScopeStack
        {
            /// <summary>
            /// Flag, if this scope is currently active
            /// </summary>
            public bool IsActive;

            /// <summary>
            /// Flag, if this the if-statement is in an active scope
            /// </summary>
            public bool IsRelevant;
        }

        class ScopeWhileStack : ScopeStack
        {
            /// <summary>
            /// Expression of while
            /// </summary>
            public String Expression;

            /// <summary>
            /// Flag, if this while scope is active
            /// </summary>
            // public bool IsActive;
        }

        class ScopeForeachStack : ScopeStack
        {
            /// <summary>
            /// Name of the variable
            /// </summary>
            public String Variablename;

            /// <summary>
            /// Enumerator for this scope
            /// </summary>
            public IEnumerator Enumerator;

            /// <summary>
            /// Flag, if this foreach scope is active
            /// </summary>
            public bool IsActive;
        }

        #endregion

        #region Interne Variablen

        /// <summary>
        /// Flag, ob gerade geparst wird. Dieser Wert wird zur
        /// Laufzeitsicherheit benütigt.
        /// </summary>
        bool _IsParsing;

        /// <summary>
        /// Parser Variables
        /// </summary>
        Dictionary<String, object> _Variables;

        /// <summary>
        /// Stringbuilder, which contains the result
        /// </summary>
        private StringBuilder _Result;

        /// <summary>
        /// Mit diesem Stringbuilder wird das Ergebnis des Verarbeitungsvorgangs
        /// aufgebaut
        /// </summary>
        protected StringBuilder Result
        {
            get { return _Result; }
            set { _Result = value; }
        }
        
        /// <summary>
        /// Flag, if the current scope is active
        /// </summary>
        bool _IsActive;

        /// <summary>
        /// Stack
        /// </summary>
        Stack<ScopeStack> _Stack;

        /// <summary>
        /// Current position of parser
        /// </summary>
        int _CurrentPosition;

        /// <summary>
        /// Externes Kommando, das aufgerufen wird, wenn keiner der internen
        /// Kommandos zu einem angeforderten Kommando passt
        /// </summary>
        ExternalCommand _ExternalCommand;

        #endregion

        /// <summary>
        /// Parser Variables
        /// </summary>
        public Dictionary<String, object> Variables
        {
            get { return _Variables; }
        }

        /// <summary>
        /// Externes Kommando, das aufgerufen wird, wenn keiner der internen
        /// Kommandos zu einem angeforderten Kommando passt
        /// </summary>
        public ExternalCommand ExternalCommand
        {
            get { return _ExternalCommand; }
            set { _ExternalCommand = value; }
        }

        /// <summary>
        /// Creates new parser core instance
        /// </summary>
        public TemplateParser()
        {
            _Variables = new Dictionary<string, object>();
        }

        /// <summary>
        /// Creates new instance
        /// </summary>
        /// <param name="oVariables"></param>
        public TemplateParser(Dictionary<String, object> oVariables)
            : this()
        {
            foreach (KeyValuePair<String, Object> oPair in oVariables)
            {
                _Variables[oPair.Key] = oPair.Value;
            }
        }

        /// <summary>
        /// Adds variable
        /// </summary>
        /// <param name="szKey"></param>
        /// <param name="oValue"></param>
        public void AddVariable(String szKey, object oValue)
        {
            _Variables[szKey] = oValue;
        }

        /// <summary>
        /// Parses stream
        /// </summary>
        /// <param name="oStream">Stream to be parsed</param>
        /// <returns></returns>
        public String Parse(TextReader oReader)
        {
            if (oReader == null)
            {
                throw new ArgumentNullException("oReader");
            }
            return Parse(oReader.ReadToEnd());
        }

        /// <summary>
        /// Parses string
        /// </summary>
        /// <param name="strString">String to be parsed</param>
        /// <returns>String to be parsed</returns>
        public String Parse(String strContent)
        {
            if (String.IsNullOrEmpty(strContent))
            {
                // Sonderbehandlung 
                return String.Empty;
            }
        
            try
            {
                // Normales Parsing
                if (_IsParsing)
                {
                    throw new InvalidOperationException(
                        Localization_Parser.TemplateParser_AlreadyParsing);
                }
                _IsParsing = true;
                // Initialize Variables
                _CurrentPosition = 0;
                int nLength = strContent.Length;
                _Result = new StringBuilder();
                _IsActive = true;
                _Stack = new Stack<ScopeStack>();

                while (true)
                {
                    int nPosition = strContent.IndexOf('@', _CurrentPosition);

                    if ((nPosition == -1) || (nPosition == strContent.Length - 1))
                    {
                        nPosition = nLength;
                    }

                    if (_IsActive)
                    {
                        _Result.Append(strContent.Substring(_CurrentPosition, nPosition - _CurrentPosition));
                    }

                    // Check if end of file

                    if (nPosition >= nLength)
                    {
                        break;
                    }

                    // Search for end of command, the end of command is set by a closing bracket

                    if (strContent[nPosition + 1] == '[')
                    {
                        bool bQuote = false; // True, wenn sich der Parser gerade in den Anführungszeichen befindet
                        int nCursorEndPosition = nPosition + 2;
                        int nEndPosition;
                        int nOpenPosition;
                        int nOpened = 0;        // Anzahl der geüffneten Klammern
                        while (true)
                        {
                            nOpenPosition = strContent.IndexOf('[', nCursorEndPosition);
                            nEndPosition = strContent.IndexOf(']', nCursorEndPosition);

                            if (nEndPosition == -1)
                            {
                                break;
                            }

                            int nQuotePosition = strContent.IndexOf('"', nCursorEndPosition);
                            if (nQuotePosition == -1)
                            {
                                break;
                            }
                            else if (nEndPosition > nQuotePosition)
                            {
                                // Die Anführungszeichen sind vor der geschlossenen Klammer
                                bQuote = !bQuote;
                                nCursorEndPosition = nQuotePosition + 1;
                            }
                            else
                            {
                                // überprüfe, ob sich eine zu üffnende Klammer vor der
                                // zu schlieüenden befindet
                                if (nOpenPosition != -1 && nOpenPosition < nEndPosition)
                                {
                                    nOpened++;
                                    nCursorEndPosition = nOpenPosition + 1;
                                    continue;
                                }
                                else if (nOpened > 0)
                                {
                                    // Es existieren noch offene Klammern
                                    nOpened--;
                                    nCursorEndPosition = nEndPosition + 1;
                                }
                                else if (bQuote)
                                {
                                    // Es sind noch Anführungszeichen offen
                                    nCursorEndPosition = nEndPosition + 1;
                                }
                                else
                                {
                                    // Es passt alles
                                    break;
                                }
                            }
                        }

                        if (nEndPosition != -1)
                        {
                            String strCommand = strContent.Substring(nPosition + 2, nEndPosition - nPosition - 2);

                            if (!ExecuteCommand(strCommand, nEndPosition + 1))
                            {
                                _CurrentPosition = nEndPosition + 1;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (_IsActive)
                        {
                            _Result.Append('@');
                        }
                        _CurrentPosition = nPosition + 1;
                    }
                }

                return _Result.ToString();
            }
            catch (Exception exc)
            {
                throw new ParserException(
                    String.Format(
                        CultureInfo.CurrentUICulture,
                        Localization_Parser.TemplateParser_Exception, exc.Message, strContent), exc);
            }
            finally
            {
                _IsParsing = false;
            }
        }

        /// <summary>
        /// Executes command and stores the result into stringbuilder
        /// </summary>
        /// <param name="strCommand"></param>
        /// <param name="nEndPosition">EndPosition of the command</param>
        /// <returns>Flag, if cursor is set by the function</returns>
        virtual protected bool ExecuteCommand(string strCommand, int nEndPosition)
        {
            int nLength = strCommand.Length;
            if (String.IsNullOrEmpty (strCommand))
            {
                return false;
            }
            if (strCommand[0] == '=')
            {
                if (_IsActive)
                {
                    ExpressionParser oParser = new ExpressionParser(this);
                    _Result.Append(oParser.ParseForString(strCommand.Substring(1)));
                }
                return false;
            }

            if (nLength > 3 && strCommand.Substring(0, 3) == "IF:")
            {
                ExecuteIfCommand(strCommand, nEndPosition);
                return false;
            }

            if (strCommand == "ENDIF")
            {
                ScopeIfStack oStackEntry = (ScopeIfStack)_Stack.Pop();
                if (oStackEntry.IsRelevant)
                {
                    _IsActive = oStackEntry.IsActive;
                }
                return false;
            }

            if (strCommand == "ELSE")
            {
                ScopeIfStack oStackEntry = (ScopeIfStack)_Stack.Peek();
                if (oStackEntry.IsRelevant)
                {
                    _IsActive = !_IsActive;
                }
                return false;
            }

            if (nLength > 4 && strCommand.Substring(0, 4) == "SET:")
            {
                if (_IsActive)
                {
                    ExecuteSetStatement(strCommand);
                }
                return false;
            }

            if (nLength > 6 && strCommand.Substring(0, 6) == "WHILE:")
            {
                String strExpression = strCommand.Substring(6);

                ScopeWhileStack oStack = new ScopeWhileStack();
                ExpressionParser oParser = new ExpressionParser(this);

                oStack.EndPosition = nEndPosition;
                //oStack.IsActive = _IsActive;
                oStack.Expression = strExpression;
                _IsActive = oParser.ParseForBoolean(strExpression);

                _Stack.Push(oStack);
                return false;
            }

            if (strCommand == "ENDWHILE")
            {
                ScopeWhileStack oStack = (ScopeWhileStack)_Stack.Peek();

                ExpressionParser oParser = new ExpressionParser(this);
                bool bLoop = oParser.ParseForBoolean(oStack.Expression);
                if (bLoop)
                {
                    _CurrentPosition = oStack.EndPosition;
                    return true;
                }
                else
                {
                    _Stack.Pop();
                    return false;
                }
            }

            if (nLength > 8 && strCommand.Substring(0, 8) == "FOREACH:")
            {
                IEnumerable iEnumerable;
                int nInPosition = strCommand.IndexOf("IN", StringComparison.Ordinal);

                ScopeForeachStack oStack = new ScopeForeachStack();
                oStack.IsActive = _IsActive;
                oStack.EndPosition = nEndPosition;

                if (nInPosition == -1)
                {
                    _IsActive = false;
                }
                else
                {
                    String strVariable = strCommand.Substring(8, nInPosition - 9);
                    String strExpression = strCommand.Substring(nInPosition + 3);

                    ExpressionParser oParser = new ExpressionParser(this);
                    if (_IsActive)
                    {
                        // Nur im aktiven Fall wird die Variable wirklich geparst. Ansonsten ist
                        // dies nicht nütig
                        iEnumerable = oParser.Parse(strExpression) as IEnumerable;
                    }
                    else
                    {
                        iEnumerable = null;
                    }

                    if (iEnumerable == null)
                    {
                        _IsActive = false;
                        oStack.Enumerator = null;
                    }
                    else
                    {
                        oStack.Enumerator = iEnumerable.GetEnumerator();
                    }

                    oParser = new ExpressionParser(this);
                    oStack.Variablename = oParser.ParseForString(strVariable); 
                }

                if (_IsActive)
                {
                    if (oStack.Enumerator.MoveNext())
                    {
                        _Variables[oStack.Variablename] = oStack.Enumerator.Current;
                    }
                    else
                    {
                        _IsActive = false;
                    }
                }

                _Stack.Push(oStack);
                return false;
            }

            if (strCommand == "ENDFOREACH")
            {
                ScopeForeachStack oStack = (ScopeForeachStack)_Stack.Peek();

                bool bLoop = 
                    oStack.Enumerator != null
                        && oStack.Enumerator.MoveNext();

                if (bLoop)
                {
                    _CurrentPosition = oStack.EndPosition;
                    Variables[oStack.Variablename] = oStack.Enumerator.Current;
                    return true;
                }
                else
                {
                    oStack = (ScopeForeachStack) _Stack.Pop();
                    _IsActive = oStack.IsActive;
                    return false;
                }
            }

            if (strCommand == "LIST")
            {
                _Result.Append(
                    StringManipulation.Join(_Variables,
                    x => String.Format("{0}: {1}", x.Key, x.Value), ", "));

                return false;
            }

            if (_ExternalCommand != null)
            {
                if (_IsActive)
                {
                    _Result.Append(_ExternalCommand(this, strCommand));
                }
            }

            return false;
        }

        private void ExecuteSetStatement(string strCommand)
        {
            int nEqualPosition = strCommand.IndexOf('=');
            if (nEqualPosition == -1)
            {
                return;
            }

            String strVariable = strCommand.Substring(4, nEqualPosition - 4);
            String strExpression = strCommand.Substring(nEqualPosition + 1);

            // Parses variablename
            ExpressionParser oParser = new ExpressionParser(this);
            strVariable = oParser.ParseForString(strVariable);

            // Parses value
            oParser = new ExpressionParser(this);
            _Variables[strVariable] = oParser.Parse(strExpression);
            return;
        }

        /// <summary>
        /// Executes an if command
        /// </summary>
        /// <param name="strCommand"></param>
        /// <param name="nEndPosition"></param>
        private void ExecuteIfCommand(string strCommand, int nEndPosition)
        {
            // If clause

            String strExpression = strCommand.Substring(3);
            ExpressionParser oParser = new ExpressionParser(this);
            bool bIf;
            bool bRelevant = true;

            ScopeIfStack oStack = new ScopeIfStack();
            oStack.EndPosition = nEndPosition;
            oStack.IsActive = _IsActive;

            if (_IsActive)
            {
                bIf = oParser.ParseForBoolean(strExpression);
                _IsActive = bIf;
            }
            else
            {
                bIf = false;
                bRelevant = false;
            }
            oStack.IsRelevant = bRelevant;

            _Stack.Push(oStack);
        }

        /// <summary>
        /// Parses an exception
        /// </summary>
        /// <param name="strExpression">Expression to be parsed</param>
        /// <returns>Result of expression</returns>
        public object ParseExpression(String strExpression)
        {
            ExpressionParser oParser = new ExpressionParser(this);
            return oParser.Parse(strExpression);
        }

        /// <summary>
        /// Executes a global function
        /// </summary>
        /// <param name="strFunctionName">Name of the Function to be executed</param>
        /// <param name="aParameters">Parameters</param>
        static internal object ExecuteFunction(string strFunctionName, List<object> aParameters)
        {
            switch (strFunctionName)
            {
                case "globaltestfunction":
                    int nX = (int)aParameters[0];
                    String strY = (String)aParameters[1];
                    return (nX * nX) + strY.ToUpper(CultureInfo.CurrentUICulture);
            }

            return "";
        }

        /// <summary>
        /// Klont den aktuellen Parser.
        /// </summary>
        /// <returns>Der geklonte Parser</returns>
        public TemplateParser CloneParser()
        {
            TemplateParser oParser = new TemplateParser();
            foreach (KeyValuePair<String, object> oPair in Variables)
            {
                oParser.AddVariable(oPair.Key, oPair.Value);
            }
            oParser.ExternalCommand = ExternalCommand;

            return oParser;
        }
    }
}