using System;
using System.Collections.Generic;
using System.Linq;
using OpenSatelliteProject.PacketData;
using OpenSatelliteProject.PacketData.Enums;
using OpenSatelliteProject.PacketData.Structs;
using System.IO;
using System.Text.RegularExpressions;

namespace OpenSatelliteProject.Tools {
    public static class FileParser {

        public static XRITHeader GetHeaderFromFile(string filename) {
            FileStream f = File.OpenRead(filename);
            var firstHeader = new byte[3];
            f.Read(firstHeader, 0, 3);
            if (firstHeader[0] == 0) {
                var tmp = firstHeader.Skip(1).Take(2).ToArray();
                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(tmp);
                }

                int size = BitConverter.ToUInt16(tmp, 0);
                firstHeader = new byte[size - 3];
                f.Seek(0, SeekOrigin.Begin);
                f.Read(firstHeader, 0, size - 3);

                PrimaryRecord fh = LLTools.ByteArrayToStruct<PrimaryRecord>(firstHeader);
                fh = LLTools.StructToSystemEndian(fh);

                f.Seek(0, SeekOrigin.Begin);
                tmp = new byte[fh.HeaderLength];
                f.Read(tmp, 0, (int)fh.HeaderLength);
                var header = FileParser.GetHeader(tmp);
                f.Close();
                return header;
            } else {
                f.Close();
                 throw new Exception("Invalid file");
            }
        }

        public static XRITHeader GetHeader(byte[] data) {
            List<XRITBaseHeader> headers = GetHeaderData(data);
            return new XRITHeader(headers);
        }

        public static List<XRITBaseHeader> GetHeaderData(byte[] data) {
            List<XRITBaseHeader> headers = new List<XRITBaseHeader>();
            int maxLength = data.Length; // Initial Guess
            int c = 0;

            // Parse Primary Header for size
            int type = data[0];

            if (type != (int)HeaderType.PrimaryHeader) {
                UIConsole.Error($"Expected PrimaryHeader({(int)HeaderType.PrimaryHeader} got {type}. File is corrupt.");
                return headers;
            }

            byte[] tmp = data.Skip(1).Take(2).ToArray();

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(tmp);
            }

            int size = BitConverter.ToUInt16(tmp, 0);
            tmp = data.Take(size).ToArray();

            PrimaryRecord fh = LLTools.ByteArrayToStruct<PrimaryRecord>(tmp);
            fh = LLTools.StructToSystemEndian(fh);
            maxLength = (int)fh.HeaderLength; // Set the correct size

            byte[] headerData = data.Take(maxLength).ToArray();
            data = data.Skip(maxLength).ToArray();

            // Parse Secondary Headers
            while (c < maxLength) {
                type = headerData[0];
                tmp = headerData.Skip(1).Take(2).ToArray();

                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(tmp);
                }

                size = BitConverter.ToUInt16(tmp, 0);
                tmp = headerData.Take(size).ToArray();

                if (tmp.Length < size) {
                    UIConsole.Warn($"Not enough data for unpack header: Expected {size} got {tmp.Length} - Header Type: {type} - File might be corrupted.");
                    if (c + size > maxLength) {
                        UIConsole.Debug($"c + size > maxLength: {c} + {size} > {maxLength}");
                        size = maxLength - c - 1;
                        c = maxLength;
                    }
                } else {
                    c += size;
                }

                headerData = headerData.Skip(size).ToArray();

                XRITBaseHeader h;
                switch (type) {
                    case (int)HeaderType.PrimaryHeader:
                        fh = LLTools.ByteArrayToStruct<PrimaryRecord>(tmp);
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
                    case (int)HeaderType.Head9:
                        Head9 h9 = new Head9();
                        h9.Data = tmp.Skip(3).ToArray();
                        h = new Head9Header(h9);
                        ((Head9Header)h).FileName = $"buggy_file_{LLTools.TimestampMS()}.lrit";
                        tmp.Skip(3).ToArray().Separate(new byte[] { 0x00 }).ToList().ForEach((barr) => {
                            if (barr.Length > 0 && barr[0] == 0x1F) {
                                ((Head9Header)h).FileName = System.Text.Encoding.UTF8.GetString(barr.Skip(1).ToArray());
                                ((Head9Header)h).FileName = LLTools.StripNonPrintable(((Head9Header)h).FileName);
                            }
                        });
                        UIConsole.Debug($"Got Head9 which may be a bug. Filename: {((Head9Header)h).FileName}");
                        break;
                    default:
                        h = new XRITBaseHeader();
                        h.Type = HeaderType.Unknown;
                        break;
                }

                h.RawData = tmp;
                headers.Add(h);
            }

            return headers;
        }

    }
}

