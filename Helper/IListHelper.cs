//-----------------------------------------------------------------------
// <copyright file="IListHelper.cs" company="Martin Brenn">
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
using System.Collections;

namespace BurnSystems.Parser.Helper
{
    /// <summary>
    /// Listenhilfsklasse
    /// </summary>
    public class IListHelper : IParserObject
    {
        IList _List;

        /// <summary>
        /// Erstellt die Listenklasse
        /// </summary>
        /// <param name="iList"></param>
        public IListHelper(IList iList)
        {
            _List = iList;
        }

        #region IParserObject Members

        public object GetProperty(string strName)
        {
            switch (strName)
            {
                case "Count":
                    return _List.Count;
                default:
                    return null;
            }
        }

        public object ExecuteFunction(string strFunctionname, IList<object> aParameter)
        {
            return null;
        }

        #endregion
    }
}
