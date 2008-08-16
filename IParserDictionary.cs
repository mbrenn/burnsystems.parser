//-----------------------------------------------------------------------
// <copyright file="IParserDictionary.cs" company="Martin Brenn">
//     Alle Rechte vorbehalten. 
// 
//     Die Inhalte dieser Datei sind ebenfalls automatisch unter 
//     der AGPL lizenziert. 
//     http://www.fsf.org/licensing/licenses/agpl-3.0.html
//     Weitere Informationen: http://de.wikipedia.org/wiki/AGPL
// </copyright>
//-----------------------------------------------------------------------

namespace BurnSystems.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Dieses Interface kann von Objekten genutzt werden, die für den Parser
    /// eine Zugriffsfunktionalität über Felder ermöglichen sollte. 
    /// </summary>
    public interface IParserDictionary
    {
        /// <summary>
        /// Diese Methode wird aufgerufen, wenn ein Feld dieser Variable aufgelöst 
        /// werden soll.
        /// </summary>
        /// <param name="name">Name of object</param>
        /// <returns>Returned object</returns>
        object this[string name]
        {
            get;
        }
    }
}
