using System.Collections.Generic;
using QuickLauncher.Model;

namespace QuickLauncher.Detector
{
    internal interface IDetector
    {
        string Category { get; }
        IList<QuickCommand> QuickCommands { get; }

        void Detect();
    }
}
