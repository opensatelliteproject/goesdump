using System;
using System.Collections.Generic;
using System.Linq;
using OpenSatelliteProject.PacketData;
using OpenSatelliteProject.PacketData.Enums;
using OpenSatelliteProject.PacketData.Structs;

namespace OpenSatelliteProject.Tools {
    public static class FileParser {
        public static XRITHeader GetHeader(byte[] data) {
            List<XRITBaseHeader> headers = GetHeaderData(data);
            return new XRITHeader(headers);
        }

        public static List<XRITBaseHeader> GetHeaderData(byte[] data) {
            List<XRITBaseHeader> headers = new List<XRITBaseHeader>();
            int maxLength = data.Length; // Initial Guess
            int c = 0;

            while (c < maxLength) {
                int type = data[0];
                byte[] tmp = data.Skip(1).Take(2).ToArray();

                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(tmp);
                }

                int size = BitConverter.ToUInt16(tmp, 0);
                tmp = data.Take(size).ToArray();

                if (tmp.Length < size) {
                    Console.WriteLine("Not enough data for unpack header: Expected {0} got {1}", size, tmp.Length);
                    break;
                }

                XRITBaseHeader h;
                switch (type) {
                    case (int)HeaderType.PrimaryHeader:
                        PrimaryRecord fh = LLTools.ByteArrayToStruct<PrimaryRecord>(tmp);
                        fh = LLTools.StructToSystemEndian(fh);
                        h = new PrimaryHeader(fh);
                        maxLength = (int)fh.HeaderLength; // Set the correct size
                        break;
                    case (int)HeaderType.ImageStructureRecord:
                        ImageStructureRecord isr = LLTools.ByteArrayToStruct<ImageStructureRecord>(tmp);
                        isr = LLTools.StructToSystemEndian(isr);
                        h = new ImageStructureHeader(isr);
                        break;
                    case (int)HeaderType.ImageNavigationRecord:
                        ImageNavigationRecord inr = LLTools.ByteArrayToStruct<ImageNavigationRecord>(tmp);
                        inr = LLTools.StructToSystemEndian(inr);
                        h = new ImageNavigationHeader(inr);
                        break;
                    case (int)HeaderType.ImageDataFunctionRecord:
                        // Cannot marshable due variable length
                        //ImageDataFunctionRecord idfr = LLTools.ByteArrayToStruct<ImageDataFunctionRecord>(tmp);
                        //idfr = LLTools.StructToSystemEndian(idfr);
                        ImageDataFunctionRecord idfr = new ImageDataFunctionRecord();
                        idfr.Data = System.Text.Encoding.UTF8.GetString(tmp.Skip(3).ToArray());
                        h = new ImageDataFunctionHeader(idfr);
                        break;
                    case (int)HeaderType.AnnotationRecord:
                        // Cannot be marshalled due variable length
                        //AnnotationRecord ar = LLTools.ByteArrayToStruct<AnnotationRecord>(tmp);
                        //ar = LLTools.StructToSystemEndian(ar);
                        AnnotationRecord ar = new AnnotationRecord();
                        ar.Filename = System.Text.Encoding.UTF8.GetString(tmp.Skip(3).ToArray());
                        h = new AnnotationHeader(ar);
                        break;
                    case (int)HeaderType.TimestampRecord:
                        TimestampRecord tr = LLTools.ByteArrayToStruct<TimestampRecord>(tmp);
                        tr = LLTools.StructToSystemEndian(tr);
                        h = new TimestampHeader(tr);
                        break;
                    case (int)HeaderType.AncillaryTextRecord:
                        // Cannot be marshalled due variable length.
                        // AncillaryText at = LLTools.ByteArrayToStruct<AncillaryText>(tmp);
                        //at = LLTools.StructToSystemEndian(at);
                        AncillaryText at = new AncillaryText();
                        at.Data = System.Text.Encoding.UTF8.GetString(tmp.Skip(3).ToArray());
                        h = new AncillaryHeader(at);
                        break;
                    case (int)HeaderType.KeyRecord:
                        h = new XRITBaseHeader(HeaderType.KeyRecord, tmp);
                        break;
                    case (int)HeaderType.SegmentIdentificationRecord:
                        SegmentIdentificationRecord sir = LLTools.ByteArrayToStruct<SegmentIdentificationRecord>(tmp);
                        sir = LLTools.StructToSystemEndian(sir);
                        h = new SegmentIdentificationHeader(sir);
                        break;
                    case (int)HeaderType.NOAASpecificHeader:
                        NOAASpecificRecord nsr = LLTools.ByteArrayToStruct<NOAASpecificRecord>(tmp);
                        nsr = LLTools.StructToSystemEndian(nsr);
                        h = new NOAASpecificHeader(nsr);
                        break;
                    case (int)HeaderType.HeaderStructuredRecord:
                        // Cannot be marshalled due variable length
                        //HeaderStructuredRecord hsr = LLTools.ByteArrayToStruct<HeaderStructuredRecord>(tmp);
                        //hsr = LLTools.StructToSystemEndian(hsr); // Header Structured Record doesn't have endianess dependant fields
                        HeaderStructuredRecord hsr = new HeaderStructuredRecord();
                        hsr.Data = System.Text.Encoding.UTF8.GetString(tmp.Skip(3).ToArray());
                        h = new HeaderStructuredHeader(hsr);
                        break;
                    case (int)HeaderType.RiceCompressionRecord:
                        RiceCompressionRecord rcr = LLTools.ByteArrayToStruct<RiceCompressionRecord>(tmp);
                        rcr = LLTools.StructToSystemEndian(rcr);
                        h = new RiceCompressionHeader(rcr);
                        break;
                    case (int)HeaderType.DCSFileNameRecord:
                        // Cannot be marshalled due variable length
                        //DCSFilenameRecord dfr = LLTools.ByteArrayToStruct<DCSFilenameRecord>(tmp);
                        //dfr = LLTools.StructToSystemEndian(dfr); // DCS Filename Record doesn't have endianess dependant fields
                        DCSFilenameRecord dfr = new DCSFilenameRecord();
                        dfr.Filename = System.Text.Encoding.UTF8.GetString(tmp.Skip(3).ToArray());
                        h = new DCSFilenameHeader(dfr);
                        break;
                    default:
                        h = new XRITBaseHeader();
                        h.Type = HeaderType.Unknown;
                        break;
                }

                h.RawData = tmp;
                headers.Add(h);
                c += size;
                data = data.Skip(size).ToArray();
            }

            return headers;
        }

    }
}

