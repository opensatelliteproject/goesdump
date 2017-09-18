using System;

namespace OpenSatelliteProject.Flow {
    public static class GlobalStatManager {
       
        #region Thread Unsafe values
        static readonly object locker = new object();
        static ulong packetsReceived;
        static ulong crcErrors;
        static ulong lengthFails;
        #endregion

        #region Getters
        public static ulong PacketsReceived {
            get {
                return packetsReceived;
            }
        }
        public static ulong CRCErrors {
            get {
                return crcErrors;
            }
        }
        public static ulong LengthFails {
            get {
                return lengthFails;
            }
        }
        #endregion
        #region Thread Safe Manipulators
        public static void IncPacketsReceived() {
            lock (locker) {
                packetsReceived++;
            }
        }
        public static void IncCRCErrors() {
            lock (locker) {
                crcErrors++;
            }
        }
        public static void IncLengthFails() {
            lock (locker) {
                lengthFails++;
            }
        }
        #endregion

        static GlobalStatManager() {
            packetsReceived = 0;
            crcErrors = 0;
        }
    }
}

