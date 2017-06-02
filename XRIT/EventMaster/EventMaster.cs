using System;
using System.Collections.Generic;

namespace OpenSatelliteProject {
    /// <summary>
    /// An Global Event master to mimic Node.JS EventEmitter.
    /// </summary>
    public class EventMaster {

        public static EventMaster Master { get; private set; }

        static EventMaster() {
            Master = new EventMaster ();
        }

        readonly Dictionary<string, EMEventHandler> handlers;

        EventMaster () {
            handlers = new Dictionary<string, EMEventHandler> ();
        }

        /// <summary>
        /// Attaches a listener for the event described by parameter type.
        /// </summary>
        /// <param name="type">Event Type to listen</param>
        /// <param name="handler">Event Handler</param>
        public static void On(string type, EMEventHandler handler) {
            EventMaster.Master._On (type, handler);
        }

        /// <summary>
        /// Post a event to everyone that is listening to.
        /// </summary>
        /// <param name="type">Event Type to listen</param>
        /// <param name="data">Event Data</param>
        public static void Post(string type, object data) {
            EventMaster.Master._Post (new EventMasterData(type, data));
        }

        public static void Detach(string type, EMEventHandler handler) {
            EventMaster.Master._Detach (type, handler);
        }

        public void _On(string type, EMEventHandler handler) {
            lock (handlers) {
                if (!handlers.ContainsKey (type)) {
                    handlers.Add (type, handler);
                } else {
                    handlers [type] += handler;
                }
            }
        }

        public void _Detach(string type, EMEventHandler handler) {
            lock (handlers) {
                if (handlers.ContainsKey (type)) {
                    Delegate.Remove(handlers [type], handler);
                }
            }
        }

        public void _Post(EventMasterData data) {
            lock (handlers) {
                if (handlers.ContainsKey (data.Type)) {
                    handlers [data.Type] (data);
                }
            }
        }

    }
    public delegate void EMEventHandler(EventMasterData data);
}

