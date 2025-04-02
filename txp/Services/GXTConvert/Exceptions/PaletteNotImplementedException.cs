using GXTConvert.FileFormat;

namespace GXTConvert.Exceptions
{
    public class PaletteNotImplementedException : Exception
    {
        public SceGxmTextureFormat Format { get; private set; }

        public PaletteNotImplementedException(SceGxmTextureFormat format) : base() { this.Format = format; }
    }
}
