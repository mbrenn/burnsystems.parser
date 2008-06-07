//-----------------------------------------------------------------------
// <copyright file="UserCommandTemplateParser.cs" company="Martin Brenn">
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

namespace BurnSystems.Parser
{
    /// <summary>
    /// Dieser Delegat wird genutzt um eine eigene Behandlung von Kommandos
    /// durchzuführen
    /// </summary>
    /// <param name="strCommand">Kommando</param>
    /// <returns>Ergebnis, welcher in die Vorlage eingefügt wird</returns>
    public delegate String ExecuteCommand (String strCommand, int nEndPosition);

    /// <summary>
    /// Dieser Parser wird genutzt, um eine eigene Behandlung der
    /// Kommandos zu erzielen
    /// </summary>
    public class UserCommandTemplateParser : TemplateParser
    {
        ExecuteCommand _ExecuteCommandDelegate;

        /// <summary>
        /// Erzeugt eine neue Instanz
        /// </summary>
        /// <param name="oDelegate"></param>
        public UserCommandTemplateParser(ExecuteCommand oDelegate)
        {
            if (oDelegate == null)
            {
                throw new ArgumentNullException("oDelegate");
            }
            _ExecuteCommandDelegate = oDelegate;
        }

        /// <summary>
        /// Lüsst ein Kommando vom Delegaten ausführen
        /// </summary>
        /// <param name="strCommand"></param>
        /// <param name="nEndPosition"></param>
        /// <returns></returns>
        protected override bool ExecuteCommand(string strCommand, int nEndPosition)
        {
            Result.Append(_ExecuteCommandDelegate(strCommand, nEndPosition));
            return false;
        }
    }
}
