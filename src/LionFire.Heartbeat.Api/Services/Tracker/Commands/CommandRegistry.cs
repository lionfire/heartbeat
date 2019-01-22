using System;
using System.Collections.Generic;

namespace LionFire.Heartbeat
{
    public class CommandRegistry
    {
        public Dictionary<int, Type> Dictionary { get; } = new Dictionary<int, Type>();

        public CommandRegistry()
        {
            Dictionary.Add(1, typeof(UndoAckMissing));
        }
    }
}
