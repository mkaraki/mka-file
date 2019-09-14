using System;
using System.Collections.Generic;
using System.Text;

namespace libmkafile
{
    public class MKAFileDataBuilder
    {
        private MKAFile d;

        public MKAFileDataBuilder(string ApplicationName, string Description = "NO INFORMATION")
        {
            d = new MKAFile()
            {
                ApplicationName = ApplicationName,
                Description = Description,
                Objects = new Dictionary<string, MKAObjectData>()
            };
        }

        public MKAFileDataBuilder(MKAFile mkafile)
        {
            d = mkafile;
        }

        public string ApplicationName { get => d.ApplicationName; set => d.ApplicationName = value; }

        public string Description { get => d.Description; set => d.Description = value; }

        public Dictionary<string, MKAObjectData> Objects { get => d.Objects; }

        public MKAObjectData GetObject(string Name)
        {
            if (!d.Objects.ContainsKey(Name))
                throw new Exception("No Object Holder");

            return d.Objects[Name];
        }

        public void RenameObjectHolder(string OldName, string NewName)
        {
            if (!d.Objects.ContainsKey(OldName))
                throw new Exception("No Object Holder");

            if (d.Objects.ContainsKey(NewName))
                throw new Exception("Same Object Holder Found");

            d.Objects.Add(NewName, d.Objects[OldName]);
            d.Objects.Remove(OldName);
        }

        public void AddObjectHolder(string Name, MKADataType Type)
        {
            if (d.Objects.ContainsKey(Name))
                throw new Exception("Same Object Holder Found");

            d.Objects.Add(Name, new MKAObjectData() {
                DataType = Type,
                IsNull = true
            });
        }

        public void RemoveObjectHolder(string Name)
        {
            if (!d.Objects.ContainsKey(Name))
                throw new Exception("No Object Holder");

            d.Objects.Remove(Name);
        }

        public void AddObjectHolderAndBytes(string Name, MKADataType Type, byte[] bytes)
        {
            d.Objects.Add(Name, new MKAObjectData()
            {
                DataType = Type,
                RawData = bytes,
                IsNull = false
            });
        }

        public void SetBytes(string Name, byte[] bytes)
        {
            if (!d.Objects.ContainsKey(Name))
                throw new Exception("No Object Holder");

            d.Objects[Name].RawData = bytes;
            d.Objects[Name].IsNull = false;
        }

        public void SetNull(string Name)
        {
            if (!d.Objects.ContainsKey(Name))
                throw new Exception("No Object Holder");

            d.Objects[Name].RawData = null;
            d.Objects[Name].IsNull = true;
        }

        public void ChangeDataType(string Name, MKADataType Type)
        {
            if (!d.Objects.ContainsKey(Name))
                throw new Exception("No Object Holder");

            d.Objects[Name].DataType = Type;
        }

        public void ChangeDataType(string Name, string Type)
        {
            if (!strDTConverter.ContainsKey(Type))
                throw new Exception("No Type");

            ChangeDataType(Name, strDTConverter[Type]);
        }

        public static Dictionary<string, MKADataType> strDTConverter = new Dictionary<string, MKADataType>
        {
            { "empty", MKADataType.empty },
            { "binary", MKADataType.binary },
            { "boolean", MKADataType.boolean },
            { "int32", MKADataType.int32 },
            { "uint32", MKADataType.uint32 },
            { "int64", MKADataType.int64 },
            { "uint64", MKADataType.uint64 },
            { "utf8", MKADataType.utf8 },
        };

        public MKAFile Build()
            => d;
    }
}
