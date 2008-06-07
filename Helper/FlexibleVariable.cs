//-----------------------------------------------------------------------
// <copyright file="FlexibleVariable.cs" company="Martin Brenn">
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
    /// Diese Klasse implementiert eine flexible Variable, die genutzt werden kann
    /// wenn man für spezielle Anwendungen keine eigene Klassenhierarchie aufbauen
    /// müchte. 
    /// </summary>
    public class FlexibleVariable : IParserObject, IEnumerable, IEnumerable<object>
    {
        /// <summary>
        /// Unterobjekte, die aufgezühlt werden künnen
        /// </summary>
        List<object> _SubItems;

        /// <summary>
        /// Die Eigenschaften
        /// </summary>
        Dictionary<String, object> _Properties;

        /// <summary>
        /// Erzeugt eine neue Instanz
        /// </summary>
        public FlexibleVariable()
        {
            _SubItems = new List<object>();
            _Properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// Setzt die Eigenschaften oder ruft diese ab
        /// </summary>
        /// <param name="strKey">Schlüssel</param>
        /// <returns>Eigenschaft</returns>
        public object this[String strKey]
        {
            get
            {
                object oReturn;
                if (_Properties.TryGetValue(strKey, out oReturn))
                {
                    return oReturn;
                }
                FlexibleVariable oVariable = new FlexibleVariable();
                _Properties[strKey] = oVariable;
                return oVariable;
            }
            set
            {
                _Properties[strKey] = value;
            }
        }

        /// <summary>
        /// Fügt ein neues Objekt hinzu
        /// </summary>
        /// <param name="oItem">Hinzuzufügendes Objekt</param>
        public void AddItem(object oItem)
        {
            _SubItems.Add(oItem);
        }

        #region IParserObject Member

        /// <summary>
        /// Gibt eine benannte Eigenschaft zurück
        /// </summary>
        /// <param name="strName">Name der Eigenschaft</param>
        /// <returns>Inhalt der Eigenschaft</returns>
        public object GetProperty(string strName)
        {
            object oReturn;
            if (_Properties.TryGetValue(strName, out oReturn))
            {
                return oReturn;
            }
            return null;
        }

        /// <summary>
        /// Führt eine benutzerdefinierte Funktion aus
        /// </summary>
        /// <param name="strFunctionname">Name der Funktion</param>
        /// <param name="aParameter">Parameter</param>
        /// <returns>null, da dieses Objekt keine Funktion implementiert</returns>
        public object ExecuteFunction(string strFunctionname, IList<object> aParameter)
        {
            return null;
        }

        #endregion

        #region IEnumerable Member

        /// <summary>
        /// Gibt die Aufzühlung für die Subitems zurück
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return _SubItems.GetEnumerator();
        }

        #endregion

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            foreach (object oObject in _SubItems)
            {
                yield return oObject;
            }
        }
    }
}
