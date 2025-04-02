namespace GXTConvert.Exceptions
{
    public class CommandLineArgsException : Exception
    {
        public string ExpectedArgs { get; private set; }

        public CommandLineArgsException(string expectedArgs) : base()
        {
            this.ExpectedArgs = expectedArgs;
        }
    }
}