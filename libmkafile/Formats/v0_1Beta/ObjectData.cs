using System;
using System.Collections.Generic;
using System.Text;

namespace libmkafile.Formats.v0_1Beta
{
    public class ObjectData
    {
        public DataTypes DataType { get; set; }

        public bool IsNull { get; set; }

        public byte[] RawData { get; set; }
    }
}
