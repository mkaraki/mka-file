using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace libmkafile
{
    public static class MKAFileConverter
    {
        public static void SerializeToStream(MKAFile MKAData, Stream stream, MKAFileVersion version = MKAFileVersion.Latest)
        {
            switch (version)
            {
                case MKAFileVersion.Latest:
                case MKAFileVersion.V0_1Beta:
                    var fc = Formats.v0_1Beta.CompatibilityHost.ConvertFromSharedClass(MKAData);
                    Formats.v0_1Beta.Converter.CreateStreamed(fc, stream);
                    break;

                default:
                    throw new Exception("Unknown Error");
            }
        }

        public static MKAFile DeserializeFromStream(Stream stream, MKAFileVersion version = MKAFileVersion.Latest)
        {
            switch (version)
            {
                case MKAFileVersion.Latest:
                case MKAFileVersion.V0_1Beta:
                    var parsed = Formats.v0_1Beta.Converter.Parse(stream);
                    return Formats.v0_1Beta.CompatibilityHost.ConvertToSharedClass(parsed);

                default:
                    throw new Exception("Unknown Error");
            }
        }
    }
}
