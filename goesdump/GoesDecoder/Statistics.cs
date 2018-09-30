using System.Runtime.InteropServices;

namespace OpenSatelliteProject {
    
    [StructLayout(LayoutKind.Sequential, Size = 4167, Pack = 1)]
    public struct Statistics_st {
        
        public byte scid;

        public byte vcid;

        public ulong packetNumber;

        public ushort vitErrors;

        public ushort frameBits;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int[] rsErrors;

        public byte signalQuality;

        public byte syncCorrelation;

        public byte phaseCorrection;

        public ulong lostPackets;

        public ushort averageVitCorrections;

        public byte averageRSCorrections;

        public ulong droppedPackets;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public long[] receivedPacketsPerChannel;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public long[] lostPacketsPerChannel;

        public ulong totalPackets;

        public uint startTime;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] syncWord;

        public byte frameLock;

        public byte demodulatorFifoUsage;

        public byte decoderFifoUsage;

        public static Statistics_st fromByteArray(byte[] data) {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var stuff = (Statistics_st)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Statistics_st));
            handle.Free();
            return stuff;
        }
    }
}

