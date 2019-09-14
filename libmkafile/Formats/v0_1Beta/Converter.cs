using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace libmkafile.Formats.v0_1Beta
{
    internal class Converter
    {
        private static byte[] DefaultStaticFileHeader = { 0x07, 0x6D, 0x6B, 0x61, 0x66, 0x69, 0x6C, 0x65 };

        public static void CreateStreamed(FileClass fc, Stream s, bool CollectGC = true)
        {
            s.Position = 0;

            s.Write(DefaultStaticFileHeader, 0, DefaultStaticFileHeader.Length);
            s.Write(new byte[] { 0x01 }, 0, 1);

            byte[] appname_byte = Encoding.ASCII.GetBytes(fc.ApplicationName);
            byte[] desc_byte = Encoding.ASCII.GetBytes(fc.Description);

            byte[] appnamelen_byte = { Convert.ToByte((ushort)appname_byte.Length) };
            byte[] desclen_byte = BitConverter.GetBytes((ushort)desc_byte.Length);

            s.Write(appnamelen_byte, 0, 1);
            s.Write(appname_byte, 0, appname_byte.Length);
            s.Write(desclen_byte, 0, 2);
            s.Write(desc_byte, 0, desc_byte.Length);

            foreach (var d in fc.Objects)
            {
                byte[] vname_byte = Encoding.UTF8.GetBytes(d.Key);
                byte[] vnamelen_byte = BitConverter.GetBytes(vname_byte.Length);
                s.Write(vnamelen_byte, 0, 4);
                s.Write(vname_byte, 0, vname_byte.Length);

                var ii = d.Value;

                DataTypes writedt = ii.IsNull ? DataTypes.empty : ii.DataType;

                byte[] vtype_byte = { GetDataType(writedt) };
                s.Write(vtype_byte, 0, 1);
                switch (writedt)
                {
                    case DataTypes.empty:
                        byte[] avtype = { GetDataType(ii.DataType) };
                        s.Write(avtype, 0, 1);
                        break;

                    case DataTypes.boolean:
                        s.Write(ii.RawData, 0, 1);
                        break;

                    case DataTypes.int32:
                    case DataTypes.uint32:
                        s.Write(ii.RawData, 0, 4);
                        break;

                    case DataTypes.int64:
                    case DataTypes.uint64:
                        s.Write(ii.RawData, 0, 8);
                        break;

                    case DataTypes.binary:
                    case DataTypes.utf8:
                        byte[] olen_byte = BitConverter.GetBytes(ii.RawData.Length);
                        s.Write(olen_byte, 0, 4);
                        s.Write(ii.RawData, 0, ii.RawData.Length);
                        break;

                    default:
                        throw new Exception("Unknown Error");
                }
            }

            s.Flush();
            if (CollectGC)
                GC.Collect();
        }

        public static FileClass Parse(Stream mkad, bool CollectGC = true)
        {
            if (mkad.Length < 18)
                throw new Exception("Invalid format");

            mkad.Position = 0;

            #region ReadFileStaticHeader

            byte[] mkadfilestaticheader1 = new byte[DefaultStaticFileHeader.Length];
            mkad.Read(mkadfilestaticheader1, 0, DefaultStaticFileHeader.Length);
            if (!mkadfilestaticheader1.SequenceEqual(DefaultStaticFileHeader))
            {
                throw new Exception("Invalid format");
            }

            byte[] mkadfileversion = new byte[1];
            mkad.Read(mkadfileversion, 0, 1);
            if (mkadfileversion[0] != 0x01)
            {
                throw new Exception("Not mkafile v0.1Beta File");
            }

            #endregion ReadFileStaticHeader

            var fc = new FileClass();

            #region ReadAppNameHeader

            byte[] size_byte_raw = new byte[1];
            mkad.Read(size_byte_raw, 0, 1);

            ushort size = Convert.ToUInt16(size_byte_raw[0]);
            byte[] data_byte_raw = new byte[size];
            mkad.Read(data_byte_raw, 0, size);

            fc.ApplicationName = Encoding.ASCII.GetString(data_byte_raw);

            #endregion ReadAppNameHeader

            #region ReadDescriptionHeader

            size_byte_raw = new byte[2];
            mkad.Read(size_byte_raw, 0, 2);

            size = BitConverter.ToUInt16(size_byte_raw, 0);
            data_byte_raw = new byte[size];
            mkad.Read(data_byte_raw, 0, size);

            fc.Description = Encoding.ASCII.GetString(data_byte_raw);

            #endregion ReadDescriptionHeader

            int ObjectMinSize = 7;

            Dictionary<string, ObjectData> FoundData = new Dictionary<string, ObjectData>();
            while (mkad.Length - mkad.Position > ObjectMinSize)
            {
                #region GetObjectName

                byte[] oname_size_raw = new byte[4];
                mkad.Read(oname_size_raw, 0, 4);
                int oname_size = BitConverter.ToInt32(oname_size_raw, 0);
                if (oname_size < 1) throw new Exception("Invalid Object Size");

                byte[] oname_raw = new byte[oname_size];
                mkad.Read(oname_raw, 0, oname_size);
                string ObjectName = Encoding.UTF8.GetString(oname_raw);

                #endregion GetObjectName

                #region GetObject

                byte[] otype = new byte[1];
                mkad.Read(otype, 0, 1);

                var od = new ObjectData()
                {
                    DataType = GetDataType(otype[0]),
                    IsNull = false,
                };
                byte[] rawdata = null;

                switch (od.DataType)
                {
                    case DataTypes.empty:
                        byte[] aotype = new byte[1];
                        mkad.Read(aotype, 0, 1);
                        od.DataType = GetDataType(aotype[0]);
                        od.IsNull = true;
                        break;

                    case DataTypes.boolean:
                        rawdata = new byte[1];
                        mkad.Read(rawdata, 0, 1);
                        if (rawdata[0] != 0x00 && rawdata[1] != 0x01)
                            throw new Exception("Invalid Data Exception");
                        break;

                    case DataTypes.int32:
                    case DataTypes.uint32:
                        rawdata = new byte[4];
                        mkad.Read(rawdata, 0, 4);
                        break;

                    case DataTypes.int64:
                    case DataTypes.uint64:
                        rawdata = new byte[8];
                        mkad.Read(rawdata, 0, 8);
                        break;

                    case DataTypes.binary:
                    case DataTypes.utf8:
                        byte[] obj_len = new byte[4];
                        mkad.Read(obj_len, 0, 4);
                        int objsize = BitConverter.ToInt32(obj_len, 0);
                        if (objsize < 1) throw new Exception("Invalid Object Size");
                        rawdata = new byte[objsize];
                        mkad.Read(rawdata, 0, objsize);
                        break;

                    default: throw new Exception("Not supported data type");
                }

                od.RawData = rawdata;
                FoundData.Add(ObjectName, od);

                #endregion GetObject
            }
            fc.Objects = FoundData;

            if (CollectGC)
                GC.Collect();

            return fc;
        }

        private static DataTypes GetDataType(byte objectType)
        {
            switch (objectType)
            {
                case 0x00: return DataTypes.empty;
                case 0x01: return DataTypes.binary;
                case 0x02: return DataTypes.boolean;
                case 0x03: return DataTypes.int32;
                case 0x04: return DataTypes.uint32;
                case 0x05: return DataTypes.int64;
                case 0x06: return DataTypes.uint64;
                case 0x07: return DataTypes.utf8;
                default: throw new Exception("Not supported data type");
            }
        }

        private static byte GetDataType(DataTypes objectType)
        {
            switch (objectType)
            {
                case DataTypes.empty: return 0x00;
                case DataTypes.binary: return 0x01;
                case DataTypes.boolean: return 0x02;
                case DataTypes.int32: return 0x03;
                case DataTypes.uint32: return 0x04;
                case DataTypes.int64: return 0x05;
                case DataTypes.uint64: return 0x06;
                case DataTypes.utf8: return 0x07;
                default: throw new Exception("Not supported data type");
            }
        }
    }
}