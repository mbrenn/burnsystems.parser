//-----------------------------------------------------------------------
// <copyright file="DateTimeHelper.cs" company="Martin Brenn">
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

namespace BurnSystems.Parser.Helper
{
    /// <summary>
    /// Hilfsklasse für das Objekt DateTime
    /// </summary>
    public class DateTimeHelper : IParserObject
    {
        /// <summary>
        /// Zeitpunkt
        /// </summary>
        DateTime _DateTime;

        public DateTimeHelper(DateTime oDateTime)
        {
            _DateTime = oDateTime;
        }

        #region IParserObject Members

        public object GetProperty(string strName)
        {
            switch (strName)
            {
                case "Day":
                    return _DateTime.Day;
                case "Date":
                    return _DateTime.Date;
                case "DayOfWeek":
                    return _DateTime.DayOfWeek;
                case "DayOfYear":
                    return _DateTime.DayOfYear;
                case "Hour":
                    return _DateTime.Hour;
                case "Millisecond":
                    return _DateTime.Millisecond;
                case "Minute":
                    return _DateTime.Minute;
                case "Month":
                    return _DateTime.Month;
                case "Second":
                    return _DateTime.Second;
                case "Ticks":
                    return _DateTime.Ticks;
                case "TimeOfDay":
                    return _DateTime.TimeOfDay;
                case "Year":
                    return _DateTime.Year;
                case "LongTime":
                    return _DateTime.ToLongTimeString();
                case "ShortTime":
                    return _DateTime.ToShortTimeString();
                case "LongDate":
                    return _DateTime.ToLongDateString();
                case "ShortDate":
                    return _DateTime.ToShortDateString();
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
