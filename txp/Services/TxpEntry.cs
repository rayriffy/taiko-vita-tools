using System.Text;
using System.Xml;
using System.Xml.Serialization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Runtime.InteropServices;
using GXTConvert.FileFormat;
using GXTConvert.Conversion;

namespace txp.Services;

[XmlRoot("Entry")]
public class TxpEntry
{
    [XmlElement("InternalFilePath")] public string InternalFilePath;
    [XmlElement("RealFilePath")] public string FilePath;

    [XmlElement("TextureSize")] [XmlIgnore]
    public uint TextureSize;

    [XmlElement("PaletteOffset")] [XmlIgnore]
    public uint PaletteOffset;

    [XmlElement("TextureOffset")] [XmlIgnore]
    public uint TextureOffset;

    [XmlElement("Format")] public uint Format;
    [XmlElement("Width")] public ushort Width;
    [XmlElement("Height")] public ushort Height;
    [XmlElement("MipLevel")] public byte MipLevel;
    [XmlElement("Type")] public byte Type; // 0 = Linear, 2 = Swizzled
    [XmlElement("Unknown")] public ushort Unknown;
    [XmlElement("Unknown2")] public uint Unknown2;

    [XmlIgnore] public uint FilenameHash;

    [XmlIgnore] public byte[] TextureRawData;

    [XmlIgnore] public uint[] PaletteData;

    [XmlElement("FileIndexA")] public int FileIndexA = -1;

    [XmlElement("FileIndexB")] public int FileIndexB = -1;

    [XmlElement("Raw")] public bool Raw = false;

    public void Write(BinaryWriter writer)
    {
        writer.Write(0); // Temporary. Will come back to write correct data later when filenames have been written into the data
        writer.Write(TextureRawData.Length);
        writer.Write(0); // No palette
        writer.Write(0); // Temporary
        writer.Write(Format);
        writer.Write(Width);
        writer.Write(Height);
        writer.Write(MipLevel);
        writer.Write(Type);
        writer.Write(Unknown);
        writer.Write(Unknown2); // What is this exactly?
    }

    public static TxpEntry Read(BinaryReader reader)
    {
        TxpEntry entry = new TxpEntry();

        var pathOffset = reader.ReadInt32();
        entry.TextureSize = reader.ReadUInt32();
        entry.PaletteOffset = reader.ReadUInt32();
        entry.TextureOffset = reader.ReadUInt32();
        entry.Format = reader.ReadUInt32();
        entry.Width = reader.ReadUInt16();
        entry.Height = reader.ReadUInt16();
        entry.MipLevel = reader.ReadByte();
        entry.Type = reader.ReadByte();
        entry.Unknown = reader.ReadUInt16();
        entry.Unknown2 = reader.ReadUInt32();

        if (entry.PaletteOffset != 0)
            entry.TextureSize -= 0x100 * 4;

        #region Read path string

        var curOffset = reader.BaseStream.Position;
        reader.BaseStream.Seek(pathOffset, SeekOrigin.Begin);

        List<byte> temp = new List<byte>();
        byte c = 0;
        while ((c = reader.ReadByte()) != 0)
        {
            temp.Add(c);
        }

        entry.InternalFilePath = Encoding.GetEncoding(932).GetString(temp.ToArray()); // 932 = Shift-JIS

        reader.BaseStream.Seek(curOffset, SeekOrigin.Begin);

        #endregion

        return entry;
    }

    public void ToFile(string outputFolder, bool isRawExtract = false)
    {
        string dir = Path.GetDirectoryName(InternalFilePath);
        string filename = Path.GetFileNameWithoutExtension(InternalFilePath);

        if (!String.IsNullOrWhiteSpace(outputFolder) && !Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        FilePath = Path.Combine(dir, filename); // Recombine using the OS's proper path formatting

        if (!String.IsNullOrWhiteSpace(dir))
        {
            if (!String.IsNullOrWhiteSpace(outputFolder))
            {
                dir = Path.Combine(outputFolder, dir);
                outputFolder = "";
            }

            filename = Path.Combine(dir, filename);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        if (isRawExtract)
        {
            FilePath += ".raw";
            Raw = true;
        }
        else
        {
            FilePath += ".png";
        }

        var outputPath = filename;
        if (!String.IsNullOrWhiteSpace(outputFolder))
            outputPath = Path.Combine(outputFolder, outputPath);

        using (TextWriter writer = new StreamWriter(outputPath + ".xml"))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TxpEntry));
            serializer.Serialize(writer, this);
        }

        if (isRawExtract)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(outputPath + ".raw", FileMode.Create)))
            {
                writer.Write(TextureRawData);
            }
        }
        else
        {
            outputPath += ".png";

            uint type = (uint)SceGxmTextureType.Linear;

            if (Type == 2)
            {
                type = (uint)SceGxmTextureType.Swizzled;
            }
            else if (Type != 0)
            {
                Console.WriteLine("Unknown type: {0}", Type);
                Environment.Exit(1);
            }

            SceGxtTextureInfo texinfo = new SceGxtTextureInfoRaw(type, Format, Width, Height, 0, TextureSize);
            GXTConvert.Conversion.TextureBundle tex =
                new GXTConvert.Conversion.TextureBundle(new BinaryReader(new MemoryStream(TextureRawData)), texinfo);

            using (var sourceImage = tex.CreateTexture(FetchPalette((SceGxmTextureFormat)Format, PaletteData)))
            {
                sourceImage.SaveAsPng(outputPath);
            }
        }
    }

    public static TxpEntry FromFile(string filename, string foldername)
    {
        if (!File.Exists(filename))
            return new TxpEntry();

        using (XmlTextReader reader = new XmlTextReader(filename))
        {
            reader.WhitespaceHandling = WhitespaceHandling.All;

            XmlSerializer serializer = new XmlSerializer(typeof(TxpEntry));

            TxpEntry entry = (TxpEntry)serializer.Deserialize(reader);

            Console.WriteLine("Reading {0}...", entry.FilePath);

            var path = entry.FilePath;

            if (!String.IsNullOrWhiteSpace(foldername))
                path = Path.Combine(foldername, path);

            if (entry.Raw)
            {
                using (BinaryReader bmp = new BinaryReader(File.Open(path, FileMode.Open)))
                {
                    entry.TextureRawData = bmp.ReadBytes((int)bmp.BaseStream.Length);
                }
            }
            else
            {
                // Import image file
                using (var image = Image.Load<Rgba32>(path))
                {
                    entry.Width = (ushort)image.Width;
                    entry.Height = (ushort)image.Height;

                    // Convert to ARGB format
                    entry.Format = (uint)SceGxmTextureFormat.U8U8U8U8_ARGB;
                    entry.Type = 0; // SceGxmTextureType.Linear

                    // Get raw pixel data
                    int stride = image.Width * 4; // 4 bytes per pixel (RGBA)
                    entry.TextureRawData = new byte[stride * image.Height];

                    // Copy pixel data in BGRA format (to match the expected ARGB format when byte-swapped)
                    image.ProcessPixelRows(accessor =>
                    {
                        for (int y = 0; y < image.Height; y++)
                        {
                            Span<Rgba32> pixelRow = accessor.GetRowSpan(y);
                            int rowOffset = y * stride;

                            for (int x = 0; x < image.Width; x++)
                            {
                                int pixelOffset = rowOffset + (x * 4);
                                Rgba32 pixel = pixelRow[x];

                                // Store in BGRA order to match the expected format
                                entry.TextureRawData[pixelOffset] = pixel.B; // B
                                entry.TextureRawData[pixelOffset + 1] = pixel.G; // G
                                entry.TextureRawData[pixelOffset + 2] = pixel.R; // R
                                entry.TextureRawData[pixelOffset + 3] = pixel.A; // A
                            }
                        }
                    });
                }
            }

            entry.TextureSize = (uint)entry.TextureRawData.Length;

            var filenameData = Encoding.GetEncoding(932).GetBytes(entry.InternalFilePath);
            entry.FilenameHash = Crc32.Calculate(filenameData, filenameData.Length);

            return entry;
        }
    }

    private static Color[] CreatePalette(uint[] inputPalette, Func<byte, byte, byte, byte, Color> arrangerAbgr)
    {
        Color[] outputPalette = new Color[inputPalette.Length];
        for (int i = 0; i < outputPalette.Length; i++)
            outputPalette[i] = arrangerAbgr((byte)(inputPalette[i] >> 24), (byte)(inputPalette[i] >> 0),
                (byte)(inputPalette[i] >> 8), (byte)(inputPalette[i] >> 16));
        return outputPalette;
    }

    public static Color[] FetchPalette(SceGxmTextureFormat textureFormat, uint[] paletteData)
    {
        if (paletteData == null)
            return null;

        Color[] palette;
        switch (textureFormat)
        {
            case SceGxmTextureFormat.P8_ABGR:
                palette = CreatePalette(paletteData, ((a, b, g, r) => { return Color.FromRgba(r, g, b, a); })); break;
            case SceGxmTextureFormat.P8_ARGB:
                palette = CreatePalette(paletteData, ((a, b, g, r) => { return Color.FromRgba(b, g, r, a); })); break;
            case SceGxmTextureFormat.P8_RGBA:
                palette = CreatePalette(paletteData, ((a, b, g, r) => { return Color.FromRgba(a, b, g, r); })); break;
            case SceGxmTextureFormat.P8_BGRA:
                palette = CreatePalette(paletteData, ((a, b, g, r) => { return Color.FromRgba(r, g, b, a); })); break;
            case SceGxmTextureFormat.P8_1BGR:
                palette = CreatePalette(paletteData,
                    ((a, b, g, r) => { return Color.FromRgba(r, g, b, 255); })); break;
            case SceGxmTextureFormat.P8_1RGB:
                palette = CreatePalette(paletteData,
                    ((a, b, g, r) => { return Color.FromRgba(b, g, r, 255); })); break;
            case SceGxmTextureFormat.P8_RGB1:
                palette = CreatePalette(paletteData,
                    ((a, b, g, r) => { return Color.FromRgba(a, b, g, 255); })); break;
            case SceGxmTextureFormat.P8_BGR1:
                palette = CreatePalette(paletteData,
                    ((a, b, g, r) => { return Color.FromRgba(r, g, b, 255); })); break;

            default: throw new GXTConvert.Exceptions.PaletteNotImplementedException(textureFormat);
        }

        return palette;
    }
}