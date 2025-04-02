// See https://aka.ms/new-console-template for more information

using compression;
using l7c;

if (args.Length == 0)
{
    Console.WriteLine("usage:");
    Console.WriteLine("Extraction:");
    Console.WriteLine("\t{0} x input.l7z", AppDomain.CurrentDomain.FriendlyName);
    Console.WriteLine();
    Console.WriteLine("Creation:");
    Console.WriteLine("\t{0} c input_foldername output.l7z", AppDomain.CurrentDomain.FriendlyName);
    Console.WriteLine();
    Console.WriteLine("Decompress individual file:");
    Console.WriteLine("\t{0} d input.bin output.bin", AppDomain.CurrentDomain.FriendlyName);
    Console.WriteLine();
    Console.WriteLine("Compress individual file:");
    Console.WriteLine("\t{0} e input.bin output.bin", AppDomain.CurrentDomain.FriendlyName);
    Environment.Exit(1);
}

if (args[0] == "c")
{
    Utils.PackL7CA(args[1], args[2]);
}
else if (args[0] == "x")
{
    Utils.UnpackL7CA(args[1]);
}
else if (args[0] == "d")
{
    string input = args[1];
    string output = output = input + ".out";

    if (args.Length >= 2)
    {
        output = args[2];
    }

    byte[] data;
    using (BinaryReader reader = new BinaryReader(File.OpenRead(input)))
    {
        var expectedFilesize = reader.ReadUInt32();

        if (expectedFilesize == 0x19) // Blank
        {
            expectedFilesize = reader.ReadUInt32();
        }
        else
        {
            expectedFilesize = (expectedFilesize & 0xffffff00) >> 8;
        }

        data = TaikoCompression.Decompress(reader.ReadBytes((int)reader.BaseStream.Length - 4));
        if (data.Length != expectedFilesize)
        {
            Console.WriteLine(
                "Filesize didn't match expected output filesize. Maybe bad decompression? ({0:x8} != {1:x8})",
                data.Length, expectedFilesize);
        }
    }

    File.WriteAllBytes(output, data);
}
else if (args[0] == "e")
{
    string input = args[1];
    string output = output = input + ".out";

    if (args.Length >= 2)
    {
        output = args[2];
    }

    byte[] data = File.ReadAllBytes(input);
    using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(output)))
    {
        int expectedFilesize = data.Length;

        if (expectedFilesize > 0xffffff)
        {
            writer.Write(0x00000019);
            writer.Write(expectedFilesize);
        }
        else
        {
            writer.Write((expectedFilesize << 8) | 0x19);
        }

        writer.Write(TaikoCompression.Compress(data));
    }
}