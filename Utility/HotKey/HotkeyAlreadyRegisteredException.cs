using System;
using System.Runtime.Serialization;

namespace Utility.HotKey
{
    [Serializable]
    internal class HotkeyAlreadyRegisteredException : Exception
    {
        private object name;
        private Exception ex;

        public HotkeyAlreadyRegisteredException()
        {
        }

        public HotkeyAlreadyRegisteredException(string message) : base(message)
        {
        }

        public HotkeyAlreadyRegisteredException(object name, Exception ex)
        {
            this.name = name;
            this.ex = ex;
        }

        public HotkeyAlreadyRegisteredException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected HotkeyAlreadyRegisteredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}