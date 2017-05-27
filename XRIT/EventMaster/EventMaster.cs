using System;
using System.Collections.Generic;

namespace OpenSatelliteProject {
    public class EventMaster {

        public static EventMaster Master { get; private set; }

        static EventMaster() {
            Master = new EventMaster ();
        }

        private readonly Dictionary<string, EMEventHandler> handlers;

        private EventMaster () {
            handlers = new Dictionary<string, EMEventHandler> ();
        }

        public static void On(string type, EMEventHandler handler) {
            EventMaster.Master._On (type, handler);
        }

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

