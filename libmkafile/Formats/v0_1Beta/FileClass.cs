using System;
using System.Collections.Generic;
using System.Text;

namespace libmkafile.Formats.v0_1Beta
{
    public class FileClass
    {
        public string ApplicationName { get; set; }

        public string Description { get; set; }

        public Dictionary<string, ObjectData> Objects { get; set; }
    }
}
