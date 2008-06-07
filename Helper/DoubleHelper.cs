//-----------------------------------------------------------------------
// <copyright file="DoubleHelper.cs" company="Martin Brenn">
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
    /// Hilfsklasse für Fließkommazahlen Zahlen
    /// </summary>
    public class DoubleHelper : IParserObject
    {
        double _Number;

        /// <summary>
        /// Erzeugt eine neue Instanz
        /// </summary>
        /// <param name="nNumber"></param>
        public DoubleHelper(double dNumber)
        {
            _Number = dNumber;
        }

        #region IParserObject Member

        public object GetProperty(string strName)
        {
            switch (strName)
            {
                case "NumberFormat":
                    return _Number.ToString("n0", CultureInfo.CurrentUICulture);
                case "Ceiling":
                    return Math.Ceiling(_Number);
                case "Floor":
                    return Math.Floor(_Number);
                case "InvariantCulture":
                    return _Number.ToString(CultureInfo.InvariantCulture);
                default:
                    return null;
            }
        }

        public object ExecuteFunction(string strFunctionname, IList<object> aParameter)
        {
            switch (strFunctionname)
            {                    
                case "Round":
                    if (aParameter.Count == 0)
                    {
                        return Math.Round(_Number);
                    }
                    if (aParameter.Count == 1)
                    {
                        double dNumber = 
                            Math.Round(_Number, Convert.ToInt32(aParameter[0], CultureInfo.InvariantCulture));
                        return dNumber;
                    }
                    break;
                default:
                    break;
            }
            return null;
        }

        #endregion
    }
}
