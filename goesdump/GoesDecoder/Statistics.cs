using System;
using System.Runtime.InteropServices;

namespace OpenSatelliteProject {
    
    //[StructLayout(LayoutKind.Explicit)] // , Size = 4160, Pack = 1
    [StructLayout(LayoutKind.Sequential, Size = 4160, Pack = 1)]
    public struct Statistics_st {
        
        //[FieldOffset(0)]
        public byte scid;

        //[FieldOffset(1)]
        public byte vcid;

        //[FieldOffset(2)]
        public UInt64 packetNumber;

        //[FieldOffset(10)]
        public UInt16 vitErrors;

        //[FieldOffset(12)]
        public UInt16 frameBits;

        //[FieldOffset(14)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public UInt32[] rsErrors;

        //[FieldOffset(30)]
        public byte signalQuality;

        //[FieldOffset(31)]
        public byte syncCorrelation;

        //[FieldOffset(32)]
        public byte phaseCorrection;

        //[FieldOffset(33)]
        public UInt64 lostPackets;

        //[FieldOffset(41)]
        public UInt16 averageVitCorrections;

        //[FieldOffset(43)]
        public byte averageRSCorrections;

        //[FieldOffset(44)]
        public UInt64 droppedPackets;

        //[FieldOffset(52)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public Int64[] receivedPacketsPerChannel;

        //[FieldOffset(2100)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public Int64[] lostPacketsPerChannel;


        //[FieldOffset(4148)]
        public UInt64 totalPackets;

        //[FieldOffset(4156)]
        public UInt32 startTime;

        public static Statistics_st fromByteArray(byte[] data) {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            Statistics_st stuff = (Statistics_st)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Statistics_st));
            handle.Free();
            return stuff;
        }
    }
}

