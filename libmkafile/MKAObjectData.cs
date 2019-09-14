using System;
using System.Collections.Generic;
using System.Text;

namespace libmkafile
{
    public class MKAObjectData
    {
        public MKADataType DataType { get; set; }

        public bool IsNull { get; set; }

        public byte[] RawData { get; set; }
    }
}
