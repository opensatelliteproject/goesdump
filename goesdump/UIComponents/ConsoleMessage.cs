using System;
using System.Collections.Generic;

#if !HEADLESS
using Microsoft.Xna.Framework;
#endif

namespace OpenSatelliteProject {
    public enum ConsoleMessagePriority {
        INFO,
        WARN,
        ERROR,
        DEBUG
    }

    public class ConsoleMessage: ICloneable {
        #if !HEADLESS
        public static readonly Dictionary<ConsoleMessagePriority, Color> CMP2COLOR = new Dictionary<ConsoleMessagePriority, Color>() {
            { ConsoleMessagePriority.INFO, Color.Blue },
            { ConsoleMessagePriority.WARN, Color.Yellow },
            { ConsoleMessagePriority.ERROR, Color.Red },
            { ConsoleMessagePriority.DEBUG, Color.Brown }
        };
        #endif

        public DateTime TimeStamp { get; set; }

        public string Message { get; set; }

        public ConsoleMessagePriority Priority { get; set; }

        public ConsoleMessage(ConsoleMessagePriority priority, string message) {
            TimeStamp = DateTime.Now;
            Message = message;
            Priority = priority;
        }

        public override string ToString() {
            return String.Format("{0}/{1,-5} {2}", TimeStamp.ToLongTimeString(), Priority.ToString(), Message);
        }

        #region ICloneable implementation

        public object Clone() {
            ConsoleMessage cm = new ConsoleMessage(this.Priority, this.Message);
            cm.TimeStamp = this.TimeStamp;
            return cm;
        }

        #endregion
    }
}

