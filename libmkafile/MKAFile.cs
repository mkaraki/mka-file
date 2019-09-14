using System;
using System.Collections.Generic;
using System.Text;

namespace libmkafile
{
    public class MKAFile
    {
        public string ApplicationName { get; set; }

        public string Description { get; set; }

        public Dictionary<string, MKAObjectData> Objects { get; set; }
    }
}
