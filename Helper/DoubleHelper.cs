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
        double item;

        /// <summary>
        /// Erzeugt eine neue Instanz
        /// </summary>
        /// <param name="number">Number to be parsed</param>
        public DoubleHelper(double number)
        {
            item = number;
        }

        #region IParserObject Member

        public object GetProperty(string strName)
        {
            switch (strName)
            {
                case "NumberFormat":
                    return item.ToString("n0", CultureInfo.CurrentUICulture);
                case "Ceiling":
                    return Math.Ceiling(item);
                case "Floor":
                    return Math.Floor(item);
                case "InvariantCulture":
                    return item.ToString(CultureInfo.InvariantCulture);
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
                        return Math.Round(item);
                    }
                    if (aParameter.Count == 1)
                    {
                        double dNumber = 
                            Math.Round(item, Convert.ToInt32(aParameter[0], CultureInfo.InvariantCulture));
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
