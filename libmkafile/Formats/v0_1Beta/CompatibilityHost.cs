using System;
using System.Collections.Generic;
using System.Text;

namespace libmkafile.Formats.v0_1Beta
{
    internal class CompatibilityHost
    {
        internal static DataTypes ConvertDataTypeI(MKADataType v)
        {
            switch (v)
            {
                case MKADataType.empty: return DataTypes.empty;
                case MKADataType.binary: return DataTypes.binary;
                case MKADataType.boolean: return DataTypes.boolean;
                case MKADataType.int32: return DataTypes.int32;
                case MKADataType.uint32: return DataTypes.uint32;
                case MKADataType.int64: return DataTypes.int64;
                case MKADataType.uint64: return DataTypes.uint64;
                case MKADataType.utf8: return DataTypes.utf8;
                default: throw new Exception("Not supported data type");
            }
        }

        internal static MKADataType ConvertDataTypeIShared(DataTypes v)
        {
            switch (v)
            {
                case DataTypes.empty: return MKADataType.empty;
                case DataTypes.binary: return MKADataType.binary;
                case DataTypes.boolean: return MKADataType.boolean;
                case DataTypes.int32: return MKADataType.int32;
                case DataTypes.uint32: return MKADataType.uint32;
                case DataTypes.int64: return MKADataType.int64;
                case DataTypes.uint64: return MKADataType.uint64;
                case DataTypes.utf8: return MKADataType.utf8;
                default: throw new Exception("Not supported data type");
            }
        }

        internal static MKAObjectData ConvertToSharedObjectData(ObjectData v)
        {
            return new MKAObjectData()
            {
                DataType = ConvertDataTypeIShared(v.DataType),
                IsNull = v.IsNull,
                RawData = v.RawData
            };
        }

        internal static ObjectData ConvertFromSharedObjectData(MKAObjectData v)
        {
            return new ObjectData()
            {
                DataType = ConvertDataTypeI(v.DataType),
                IsNull = v.IsNull,
                RawData = v.RawData
            };
        }

        internal static FileClass ConvertFromSharedClass(MKAFile v)
        {
            var toret =  new FileClass() {
                ApplicationName = v.ApplicationName,
                Description = v.Description,
                Objects = new Dictionary<string, ObjectData>()
            };

            foreach (var d in v.Objects)
                toret.Objects.Add(d.Key, ConvertFromSharedObjectData(d.Value));

            return toret;
        }

        internal static MKAFile ConvertToSharedClass(FileClass v)
        {
            var toret = new MKAFile()
            {
                ApplicationName = v.ApplicationName,
                Description = v.Description,
                Objects = new Dictionary<string, MKAObjectData>()
            };

            foreach (var d in v.Objects)
                toret.Objects.Add(d.Key, ConvertToSharedObjectData(d.Value));

            return toret;
        }
    }
}
