using System;
using System.Runtime.InteropServices;

namespace OpenSatelliteProject.Tools {
    /// <summary>
    /// libaec Wrapper Class
    /// 
    /// Used for decompressing LRIT Rice
    /// </summary>
    public static class AEC {
        public static readonly int ALLOW_K13_OPTION_MASK = 1;
        public static readonly int CHIP_OPTION_MASK      = 2; 
        public static readonly int EC_OPTION_MASK        = 4;
        public static readonly int LSB_OPTION_MASK       = 8;
        public static readonly int MSB_OPTION_MASK       = 16;
        public static readonly int NN_OPTION_MASK        = 32;
        public static readonly int RAW_OPTION_MASK       = 128;

        [DllImport("satdecompress", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int Decompress(byte *input, byte *output, uint inputLength, uint outputLength, int bitsPerPixel, int pixelsPerBlock, int pixelsPerScanline, int mask);

        public static int LritRiceDecompress(ref byte[] dest, byte[] source, int bitsPerPixel, int pixelsPerBlock, int pixelsPerScanline, int mask) {
            int status = -100;
            unsafe {
                fixed (byte* destPtr = dest) {
                    fixed (byte *srcPtr = source) {
                        status = Decompress(srcPtr, destPtr, (uint) source.Length, (uint) dest.Length, bitsPerPixel, pixelsPerBlock, pixelsPerScanline, mask);
                    }
                }
            }

            if (status <= 0) {
                throw new AECException((AECStatus)status);
            }

            return status;
        }
    }

    public class AECException: Exception {
        public AECStatus status;

        public AECException(AECStatus error) {
            this.status = error;
        }
    }

    public enum AECStatus {
        INTERNAL_ERROR = -100,
        MEMORY_ERROR = -4,
        DATA_ERROR = -3,
        STREAM_ERROR = -2,
        CONFIG_ERROR = -1,
        OK = 0,
        OUTPUT_BUFFER_FULL = 2,
    }
}

