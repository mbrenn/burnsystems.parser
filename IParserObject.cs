//-----------------------------------------------------------------------
// <copyright file="IParserObject.cs" company="Martin Brenn">
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
using System.Collections.ObjectModel;

namespace BurnSystems.Parser
{
    /// <summary>
    /// This interface has to be implemented by all objects, which offer methods
    /// or properties to the parser
    /// </summary>
    public interface IParserObject
    {
        /// <summary>
        /// This function returns a specific property, which is accessed by name
        /// </summary>
        /// <param name="strName">Name of requested property</param>
        /// <returns>Property</returns>
        object GetProperty(String strName);

        /// <summary>
        /// This function has to execute a function and to return an object
        /// </summary>
        /// <param name="strFunctionname">Name of function</param>
        /// <param name="aParameter">Paramters</param>
        /// <returns>Return of function</returns>
        object ExecuteFunction(String strFunctionname, IList<object> aParameter);
    }
}
