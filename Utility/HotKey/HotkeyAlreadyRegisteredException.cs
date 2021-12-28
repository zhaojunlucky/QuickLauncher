using System;
using System.Runtime.Serialization;

namespace Utility.HotKey
{
    [Serializable]
    public class HotkeyAlreadyRegisteredException : Exception
    {
        public HotkeyAlreadyRegisteredException()
        {
        }

        public HotkeyAlreadyRegisteredException(string message) : base(message)
        {
        }

        public HotkeyAlreadyRegisteredException(object name, Exception ex): base(name.ToString(), ex)
        {

        }

        public HotkeyAlreadyRegisteredException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected HotkeyAlreadyRegisteredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}