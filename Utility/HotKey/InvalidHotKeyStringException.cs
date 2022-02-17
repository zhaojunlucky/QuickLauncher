using System;

namespace Utility.HotKey
{
    internal class InvalidHotKeyStringException : Exception
    {
        public InvalidHotKeyStringException(string v)
            : base(v)
        {

        }
    }
}