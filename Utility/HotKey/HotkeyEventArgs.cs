using System;

namespace Utility.HotKey
{
    public class HotkeyEventArgs : EventArgs
    {
        internal HotkeyEventArgs(string name)
        {
            this.Name = name;
        }

        public string Name { get; }
    }
}