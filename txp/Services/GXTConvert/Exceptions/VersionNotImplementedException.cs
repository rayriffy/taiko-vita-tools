namespace GXTConvert.Exceptions
{
    public class VersionNotImplementedException : Exception
    {
        public uint Version { get; private set; }

        public VersionNotImplementedException(uint version) : base()
        {
            this.Version = version;
        }
    }
}