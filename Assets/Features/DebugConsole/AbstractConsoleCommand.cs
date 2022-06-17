using System;
using System.Collections.Generic;

namespace Zekzek.DebugConsole
{
    public abstract class AbstractConsoleCommand
    {
        public abstract string[] Aliases { get; }
        public abstract string HelpText { get; }
        public abstract string Description { get; }

        public abstract bool Process(Action<string> write, List<string> args);
    }
}