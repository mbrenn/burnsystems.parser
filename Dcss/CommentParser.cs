//-----------------------------------------------------------------------
// <copyright file="CommentParser.cs" company="Martin Brenn">
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

namespace BurnSystems.Parser.Dcss
{
    /// <summary>
    /// Mit Hilfe dieses Parser können Kommentare aus
    /// verschiedenen Quelldateien herausgenommen werden.
    /// </summary>
    public static class CommentParser
    {
        /// <summary>
        /// Verarbeitet den übergebenen Eingangsstring so, 
        /// dass alle Kommentare in der Syntax /* comment */ 
        /// entfernt werden. 
        /// </summary>
        /// <param name="strInput">Eingangsstring</param>
        /// <returns>Ergebnis ohne Kommentare</returns>
        public static String StripStarComments(String strInput)
        {
            var bInComment = false;
            var cLastCharacter = ' ';
            var nCurrentPosition = 0;
            var nLength = strInput.Length;
            var oResult = new StringBuilder();
            var bFirst = true;

            while (nCurrentPosition < nLength)
            {
                var cCurrentCharacter = strInput[nCurrentPosition];

                if (bInComment)
                {
                    if (cLastCharacter == '*' &&
                        cCurrentCharacter == '/')
                    {
                        bInComment = false;
                        bFirst = true;
                    }
                }
                else
                {
                    if (cLastCharacter == '/' &&
                        cCurrentCharacter == '*')
                    {
                        bInComment = true;
                    }
                    else if (!bFirst)
                    {
                        oResult.Append(cLastCharacter);
                    }
                    bFirst = false;                  
                }

                cLastCharacter = cCurrentCharacter;
                nCurrentPosition++;
            }
            if (!bInComment && !bFirst)
            {
                oResult.Append(cLastCharacter);
            }

            return oResult.ToString();
        }
    }
}
