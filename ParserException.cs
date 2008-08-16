//-----------------------------------------------------------------------
// <copyright file="ParserException.cs" company="Martin Brenn">
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

    /// <summary>
    /// This exception is thrown, when an exception is thrown.
    /// </summary>
    [global::System.Serializable]
    public class ParserException : Exception
    {
        /// <summary>
        /// Creates a new instance
        /// </summary>
        public ParserException() 
        { 
        }

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="message">Message to be set</param>
        public ParserException(string message) : base(message) 
        { 
        }

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="message">Message to be set</param>
        /// <param name="inner">Inner exception</param>
        public ParserException(string message, Exception inner) : base(message, inner) 
        { 
        }

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Serialization context</param>
        protected ParserException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) 
        {         
        }
    }
}
