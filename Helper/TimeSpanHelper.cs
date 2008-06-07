//-----------------------------------------------------------------------
// <copyright file="TimeSpanHelper.cs" company="Martin Brenn">
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
    /// Diese Hilfsfunktion kapselt den TimeSpan ab und stellt verschiedene Properties
    /// und Funktionen nach außen zur Verfügung
    /// </summary>
    public class TimeSpanHelper : IParserObject
    {
        TimeSpan _TimeSpan;

        public TimeSpanHelper(TimeSpan oTimeSpan)
        {
            _TimeSpan = oTimeSpan;
        }
        #region IParserObject Member

        public object GetProperty(string strName)
        {
            switch (strName)
            {
                case "Seconds":
                    return _TimeSpan.Seconds;
                case "Minutes":
                    return _TimeSpan.Minutes;
                case "Hours":
                    return _TimeSpan.Hours;
                case "Days":
                    return _TimeSpan.Days;
                case "Milliseconds":
                    return _TimeSpan.TotalMilliseconds;
                case "TotalSeconds":
                    return _TimeSpan.TotalSeconds;
                case "TotalMinutes":
                    return _TimeSpan.TotalMinutes;
                case "TotalHours":
                    return _TimeSpan.TotalHours;
                case "TotalMilliseconds":
                    return _TimeSpan.TotalMilliseconds;
                case "RoundForSeconds":
                    return TimeSpan.FromSeconds(Math.Round(_TimeSpan.TotalSeconds));
                case "Format":
                    return MathHelper.FormatTimeSpan(_TimeSpan);
            }
            return null;
        }

        public object ExecuteFunction(string strFunctionname, IList<object> aParameter)
        {
            return null;
        }

        #endregion
    }
}
