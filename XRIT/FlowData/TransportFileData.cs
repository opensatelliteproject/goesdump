using System;
using OpenSatelliteProject.PacketData;
using System.IO;
using OpenSatelliteProject.Tools;
using System.Linq;

namespace OpenSatelliteProject.FlowData {
    public class TransportFileData {

        public UInt16 FileCounter { get; private set; }
        public UInt64 Length { get; private set; }
        public string Filename { get; private set; }
        public string TemporaryFilename { get; private set; }
        public bool ReadyToSave { get; private set; }
        public bool Processed { get; private set; }
        public uint CorruptedPackets { get; private set; }

        XRITHeader header;
        UInt64 savedLength;
        readonly FileStream fileHandler;

        public TransportFileData () {
            ReadyToSave = false;
            Length = 0;
            FileCounter = 0;
            TemporaryFilename = Path.GetTempFileName ();
            Filename = "Undefined";
            savedLength = 0;
            fileHandler = File.OpenWrite (TemporaryFilename);
            CorruptedPackets = 0;
        }

        ~TransportFileData() {
            if (!Processed) {
                UIConsole.Warn ($"Erasing file {Filename} named as {TemporaryFilename} that wasn't processed!");
                try {
                    if (File.Exists (TemporaryFilename)) {
                        File.Delete (TemporaryFilename);
                    }
                } catch (Exception e) {
                    UIConsole.Warn ($"Cannot delete temporary file {TemporaryFilename}: {e}");
                }
            }
        }

        public void SetProcessed() {
            Processed = true;
        }

        public void PutMSDU(MSDU msdu) {
            byte[] dataToSave;

            if (msdu.Sequence == SequenceType.FIRST_SEGMENT || msdu.Sequence == SequenceType.SINGLE_DATA) {
                dataToSave = msdu.Data.Skip (10).ToArray (); // Skip Transport Header
                header = FileParser.GetHeader (dataToSave);
                byte[] fileCounter = msdu.Data.Take (2).ToArray ();
                byte[] fileLength = msdu.Data.Skip (2).Take (8).ToArray ();

                if (BitConverter.IsLittleEndian) {
                    Array.Reverse (fileCounter);
                    Array.Reverse (fileLength);
                }

                FileCounter = BitConverter.ToUInt16 (fileCounter, 0);
                Length = BitConverter.ToUInt64 (fileLength, 0);
                Filename = header.Filename;
            } else {
                dataToSave = msdu.Data;
            }

            if (!msdu.Full) {
                // Let's fill it for having the correct file size
                int bytesToFill = msdu.PacketLength - msdu.Data.Count();
                dataToSave = dataToSave.Concat (new byte[bytesToFill]).ToArray();
                UIConsole.Debug ("Filling data to save since msdu is not full but finished.");
            }

            if (!msdu.Valid) {
                CorruptedPackets++;
            }

            ReadyToSave |= msdu.Sequence == SequenceType.SINGLE_DATA;

            fileHandler.Write (dataToSave, 0, dataToSave.Length);
            savedLength += (ulong) dataToSave.Length;
            ReadyToSave |= savedLength == Length;

            if (ReadyToSave) {
                fileHandler.Close ();
            }
        }
    }
}

