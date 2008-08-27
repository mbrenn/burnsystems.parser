//-----------------------------------------------------------------------
// <copyright file="DcssConverter.cs" company="Martin Brenn">
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
using System.Diagnostics;
using System.Globalization;

namespace BurnSystems.Parser.Dcss
{
    /// <summary>
    /// Kontext, in dem sich der Parser aktuell befindet. 
    /// Die Namen richten sich nach der Spezifikation gemäß
    /// CSS 2.1 Kapitel 4.1.1
    /// </summary>
    enum ParsingContext
    {
        Stylesheet,
        Statement,
        AtRule,
        Block,
        Ruleset,
        Selector,
        Declaration,
        Property,
        Value,
        Any
    }

    /// <summary>
    /// Dieser Konvertierer verarbeitet eine Dcss-Datei und gibt
    /// eine CSS 2.1 konforme CSS-Datei zurück.
    /// </summary>
    public class DcssConverter
    {
        /// <summary>
        /// Flag, ob eine Debug-Ausgabe auf die Konsole
        /// gebracht werden soll. 
        /// </summary>
        bool _Debug;

        /// <summary>
        /// Flag, ob eine Debug-Ausgabe auf die Konsole
        /// gebracht werden soll. 
        /// </summary>
        public bool DebugActive
        {
            get { return _Debug; }
            set { _Debug = value; }
        }

        /// <summary>
        /// Variablen
        /// </summary>
        Dictionary<String, String> _Variables =
            new Dictionary<string, string>();

        /// <summary>
        /// Speichert das Resultat des Konverters
        /// </summary>
        StringBuilder _Result = new StringBuilder();

        /// <summary>
        /// Dieser Textreader enthält den Quellstring
        /// </summary>
        String _Input;

        /// <summary>
        /// Aktuelle Position während des Parsevorgangs
        /// </summary>
        int _CurrentPosition;

        // <summary>
        // Dies ist der zu parsende Reststring. Diese Funktion
        // ist Hauptsächlich für Debugzwecke eingerichtet
        // </summary>
        /*String RestString
        {
            get
            {
                if (_Input != null &&
                    _CurrentPosition >= 0 && _CurrentPosition < _Input.Length)
                {
                    return _Input.Substring(_CurrentPosition);
                }
                return String.Empty;
            }
        }*/

        /// <summary>
        /// Konvertiert eine Datei
        /// </summary>
        /// <param name="strFile">Die zu lesende Datei</param>
        /// <returns>Gibt die resultierende Datei als String 
        /// zurück</returns>
        public String Convert(String strFile)
        {
            if (strFile == null)
            {
                throw new ArgumentNullException("strFile");
            }
            _Input = CommentParser.StripStarComments(strFile);

            ConvertStylesheet();

            return _Result.ToString();
        }

        /// <summary>
        /// Konvertiert den Inhalt eines Textreaders und gibt 
        /// einen neuen Textreader mit dem Inhalt der CSS-Datei zurück
        /// </summary>
        /// <param name="oReader">Textreader, der die DCSS-Datei
        /// gespeichert hat</param>
        /// <returns>Textreader, der die resultierende CSS-Datei 
        /// speichert.</returns>
        public TextReader Convert(TextReader oReader)
        {            
            return new StringReader(Convert(oReader.ReadToEnd()));
        }

        /// <summary>
        /// Parst den String als Stylesheet und ruft die jeweiligen
        /// Kontext-Funktionen auf. 
        /// </summary>
        private void ConvertStylesheet()
        {            
            var nLength = _Input.Length;
            _Result.Append(GetWhitespaces());
            while (_CurrentPosition < nLength)
            {
                var cCurrentCharacter = _Input[_CurrentPosition];

                if (cCurrentCharacter == '@')
                {
                    _Result.Append(ParseAtRule());
                }
                else if (IsAny(cCurrentCharacter))
                {
                    _Result.Append(ParseRuleset());
                }
                else
                {
                    _Result.Append(cCurrentCharacter);
                    _CurrentPosition++;
                }
                _Result.Append(GetWhitespaces());
            }
        }

        /// <summary>
        /// Liest eine AtRule ein
        /// </summary>
        private String ParseAtRule()
        {
            _CurrentPosition++;
            // Habe nun das Ende der At Rule
            var strAtRuleName = GetIdent();

            // Holt sich nun den Rest
            String strWhitespaces = GetWhitespaces();

            // Holt sich das 'Any'
            String strAny = GetAny();

            // Überspringt weitere Whitespaces
            String strWhitespaces2 = GetWhitespaces();

            // Überprüft, ob das aktuelle Symbol ein ';' oder ein 
            // Block ist

            String strRest;
            if (_Input[_CurrentPosition] == ';')
            {
                // Semikolon
                strRest = ";";
            }
            else
            {
                if (strAtRuleName.Trim() == "dcssdefine")
                {
                    ParseDCSSDefineBlock();
                    return String.Empty;
                }
                else
                {
                    // Block
                    strRest = GetBlock();
                }
            }

            WriteDebug("At-Rule: " + strAtRuleName);
            // Gibt die gesamte AtRule zurück
            return String.Format(
                CultureInfo.InvariantCulture,
                "@{0}{1}{2}{3}{4}",
                strAtRuleName, strWhitespaces,
                    strAny, strWhitespaces2, strRest);
        }

        /// <summary>
        /// Diese Funktion gibt einen kompletten Block zurück, 
        /// es wird sich nur auf die geschweiften Klammern, eventuellen
        /// Kommentaren und Anführungszeichen. Der Inhalt selbst wird
        /// nicht verstanden. 
        /// </summary>
        /// <returns></returns>
        private string GetBlock()
        {
            var oBlock = new StringBuilder();
            // Aktuelle Blocktiefe
            int nBlockDepth = 1;
            // Flag, ob sich der Parser gerade im Quote befindet
            bool bInQuote = false;
            // Flag, ob das letzte Zeichen ein Escape-Character war
            bool bEscaped = false;
            
            // Das erste Zeichen muss ein '{' sein. 
            if (_Input[_CurrentPosition] != '{')
            {
                Debug.Fail("_Input[_CurrentPosition ] != '{'");
            }
            _CurrentPosition++;
            oBlock.Append('{');

            while (_CurrentPosition < _Input.Length)
            {
                var cCurrentCharacter = _Input[_CurrentPosition];

                if (bInQuote)
                {
                    // Ist in quote. 
                    if (bEscaped)
                    {
                        bEscaped = false;
                    }
                    else if (cCurrentCharacter == '"')
                    {
                        bInQuote = false;
                    }
                    else if (cCurrentCharacter == '\\')
                    {
                        bEscaped = true;
                    }
                }
                else if (cCurrentCharacter == '}')
                {
                    nBlockDepth--;
                    if (nBlockDepth <= 0)
                    {
                        break;
                    }
                }
                else if (cCurrentCharacter == '{')
                {
                    nBlockDepth++;
                }

                oBlock.Append(cCurrentCharacter);
                _CurrentPosition++;
            }

            var strBlock = oBlock.ToString();
            WriteDebug("Block: " + strBlock);

            return strBlock;
        }

        delegate void RulesetPropertyValueHandler
            (String strProperty, String strValue);

        /// <summary>
        /// Dieses Objekt wird geworfen, wenn ein neues Ruleset-Objekt
        /// geparst wurde. 
        /// </summary>
        RulesetPropertyValueHandler RulesetPropertyValueParsed;

        /// <summary>
        /// Parst ein Regelwerk
        /// </summary>
        /// <returns></returns>
        private String ParseRuleset()
        {
            // Lädt den Selektor ein
            var strSelector = GetSelector();

            WriteDebug("Selector: " + strSelector);

            var oRules = ParseRulesetBlock();

            return String.Format(
                CultureInfo.InvariantCulture,
                "{0}{1}{2}", strSelector, '{', oRules.ToString());
        }

        /// <summary>
        /// Parst den Block des Regelwerks
        /// </summary>
        /// <returns>Der zu parsende Rulesetblock</returns>
        private StringBuilder ParseRulesetBlock()
        {
            // Nun folgt die OpenBracket
            Debug.Assert(_Input[_CurrentPosition] == '{');

            _CurrentPosition++;

            // Nun folgen die einzelnen Eigenschaften. 
            // Diese sind relativ simpel zu parsen: 
            // S* property S* ':' S* value;

            var oRules = new StringBuilder();

            while (_CurrentPosition < _Input.Length)
            {
                oRules.Append(GetWhitespaces());
                if (_CurrentPosition >= _Input.Length)
                {
                    // Ende
                    break;
                }

                if (_Input[_CurrentPosition] == '}')
                {
                    // Schließende Klammer.
                    oRules.Append('}');
                    _CurrentPosition++;
                    break;
                }
                else if (_Input[_CurrentPosition] == ';')
                {
                    oRules.Append(';');
                    _CurrentPosition++;
                }
                else
                {
                    // Suche nun den Doppelpunkt
                    int nColon = _Input.IndexOf(':', _CurrentPosition);
                    Debug.Assert(nColon != -1, "No Colon in property found");
                    if (nColon == -1)
                    {
                        // Abbruch, um Endlosschleife zu verhindern
                        _CurrentPosition = _Input.Length;
                        break;
                    }

                    // Suche nun Das Semikolon oder die schließende Klammer
                    int nSemikolon = _Input.IndexOf(';', nColon);
                    int nClosingBracket = _Input.IndexOf('}', nColon);

                    int nEndValue;
                    if (nSemikolon == -1)
                    {
                        nEndValue = nClosingBracket;
                    }
                    else
                    {
                        nEndValue = Math.Min(nClosingBracket, nSemikolon);
                    }
                    Debug.Assert(nEndValue != -1, "No end in Converter");
                    if (nEndValue == -1 || nColon == -1)
                    {
                        // Abbruch, um Endlosschleife zu verhindern
                        _CurrentPosition = _Input.Length;
                        break;
                    }

                    // Nun werden die Daten geholt. 

                    String strProperty =
                        _Input.Substring(
                            _CurrentPosition,
                            nColon - _CurrentPosition);
                    String strValue =
                        _Input.Substring(
                            nColon + 1,
                            nEndValue - nColon - 1);

                    if (RulesetPropertyValueParsed != null)
                    {
                        RulesetPropertyValueParsed(strProperty, strValue);
                    }

                    WriteDebug(strProperty + ": " + strValue);

                    // Füge nun die Variablen ein. 

                    if (strValue.IndexOf('[') != -1)
                    {
                        foreach (var oPair in _Variables)
                        {
                            strValue =
                                strValue.Replace(
                                    String.Format(CultureInfo.InvariantCulture, "[{0}]", oPair.Key),
                                    oPair.Value);
                        }
                    }

                    // Und baue den Spaß wieder zusammen
                    oRules.AppendFormat("{0}:{1}", strProperty, strValue);

                    _CurrentPosition = nEndValue;
                }

            }
            return oRules;
        }

        /// <summary>
        /// Parst einen Block und setzt die Variablen
        /// </summary>
        /// <param name="strRest"></param>
        private void ParseDCSSDefineBlock()
        {
            RulesetPropertyValueParsed =
                delegate(String strProperty, String strValue)
                {
                    WriteDebug("Set Variable: " + strProperty);
                    _Variables[strProperty.Trim()] = strValue.Trim();
                };

            ParseRulesetBlock();
        }

        /// <summary>
        /// Gibt den Selektor zurück
        /// </summary>
        /// <returns></returns>
        private string GetSelector()
        {
            int nOpenBracket =
                _Input.IndexOf('{', _CurrentPosition);

            if (nOpenBracket == -1)
            {
                Debug.Fail("No open bracket after Selector");
                return String.Empty;
            }

            var strSelector =
                _Input.Substring(_CurrentPosition,
                     nOpenBracket - _CurrentPosition);
            _CurrentPosition = nOpenBracket;
            return strSelector;
        }

        /// <summary>
        /// Gibt 'ANY' zurück
        /// </summary>
        /// <returns></returns>
        private string GetAny()
        {
            return GetByPredicate(
                x => IsAny(x));
        }

        /// <summary>
        /// Gibt einen Identifikationstring zurück
        /// </summary>
        /// <returns></returns>
        private string GetIdent()
        {
            return
                GetByPredicate(
                    x => Char.IsLetterOrDigit(x) || (int)x > 177);
        }

        private String GetWhitespaces()
        {
            return
               GetByPredicate(
                   x =>
                       x == ' '
                       || x == '\r'
                       || x == '\t'
                       || x == '\n'
                       || x == '\f');
        }

        private String GetByPredicate(Predicate<char> oPredicate)
        {
            StringBuilder oIdentification = new StringBuilder();
            while (_CurrentPosition < _Input.Length)
            {
                var cCharacter = _Input[_CurrentPosition];
                if (oPredicate(cCharacter))
                {
                    oIdentification.Append(cCharacter);
                }
                else
                {
                    break;
                }
                _CurrentPosition++;
            }
            return oIdentification.ToString();
        }

        /// <summary>
        /// Diese Funktion gibt true zurück, wenn der übergebene Character
        /// einen 'any'-Wert gemäß CSS 2.1-Spezifikation entspricht. 
        /// </summary>
        /// <param name="cCharacter">Zu prüfendes Zeichen</param>
        /// <remarks>
        /// Gemäß CSS-Spezifikation: 
        /// <code>
        /// any         
        /// : [ IDENT | NUMBER | PERCENTAGE | DIMENSION | STRING
        /// | DELIM | URI | HASH | UNICODE-RANGE | INCLUDES
        /// | DASHMATCH | FUNCTION S* any* ')' 
        /// | '(' S* any* ')' | '[' S* any* ']' ] S*;
        /// </code>
        /// </remarks>
        /// <returns>true. </returns>
        static private bool IsAny(char cCharacter)
        {
            // ANY ist alles, außer:
            // ATKEYWORD, \;, \{, \}, 
            // Einfach zu prüfen.

            return !(cCharacter == '@' || cCharacter == '{' ||
                cCharacter == '}' || cCharacter == ';');
        }

        /// <summary>
        /// Gibt bei Bedarf einen Debugstring auf die Konsole heraus
        /// </summary>
        /// <param name="p"></param>
        private void WriteDebug(string strMessage)
        {
            if (_Debug)
            {
                Console.WriteLine(strMessage);
            }
        }

    }
}
