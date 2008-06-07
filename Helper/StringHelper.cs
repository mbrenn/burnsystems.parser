//-----------------------------------------------------------------------
// <copyright file="StringHelper.cs" company="Martin Brenn">
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
using System.Web;
using System.Globalization;

namespace BurnSystems.Parser.Helper
{
    /// <summary>
    /// Diese Hilfsklasse kümmert sich um die allgemeine Verarbeitung von Strings
    /// </summary>
    public class StringHelper : IParserObject
    {
        String _Content;

        public StringHelper(String strContent)
        {
            _Content = strContent;
        }

        #region IParserObject Member

        public object GetProperty(string strName)
        {
            switch (strName)
            {
                case "Length":
                    return _Content.Length;
                case "HtmlEncoded":
                    return HttpUtility.HtmlEncode(_Content);
                case "UrlEncoded":
                    return HttpUtility.UrlEncode(_Content);
                case "Nl2Br":
                    return StringManipulation.Nl2Br(_Content);
                default:
                    return null;
            }
        }

        public object ExecuteFunction(string strFunctionname, IList<object> aParameter)
        {
            switch (strFunctionname)
            {
                case "ToUpper":
                    return _Content.ToUpper(CultureInfo.CurrentUICulture);
                case "ToLower":
                    return _Content.ToLower(CultureInfo.CurrentUICulture);
                case "Trim":
                    return _Content.Trim();
                case "TrimEnd":
                    return _Content.TrimEnd();
                case "TrimStart":
                    return _Content.TrimStart();
                case "Substring":
                    return _Content.Substring((int)aParameter[0], (int)aParameter[1]);
                case "Contains":
                    return _Content.Contains(aParameter[0].ToString());
            }
            return null;            
        }

        #endregion
    }
}
