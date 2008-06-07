//-----------------------------------------------------------------------
// <copyright file="LongHelper.cs" company="Martin Brenn">
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
using System.Globalization;

namespace BurnSystems.Parser.Helper
{
    /// <summary>
    /// Hilfsklasse für lange Zahlen
    /// </summary>
    public class LongHelper : IParserObject
    {
        long _Number;

        /// <summary>
        /// Erzeugt eine neue Instanz
        /// </summary>
        /// <param name="nNumber"></param>
        public LongHelper(long nNumber)
        {
            _Number = nNumber;
        }

        #region IParserObject Member

        public object GetProperty(string strName)
        {
            switch (strName)
            {
                case "NumberFormat":
                    return _Number.ToString("n0", CultureInfo.CurrentUICulture);
                default:
                    return null;
            }
        }

        public object ExecuteFunction(string strFunctionname, IList<object> aParameter)
        {
            switch (strFunctionname)
            {
                default:
                    break;
            }
            return null;
        }

        #endregion
    }
}
