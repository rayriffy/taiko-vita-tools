using GXTConvert.FileFormat;

namespace GXTConvert.Exceptions
{
    public class TypeNotImplementedException : Exception
    {
        public SceGxmTextureType Type { get; private set; }

        public TypeNotImplementedException(SceGxmTextureType type) : base()
        {
            this.Type = type;
        }
    }
}