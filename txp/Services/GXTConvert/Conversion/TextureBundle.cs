using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using GXTConvert.Exceptions;
using GXTConvert.FileFormat;

namespace GXTConvert.Conversion
{
    // For convenience sake, to bundle (most of) the essential texture information
    public class TextureBundle
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int PaletteIndex { get; private set; }
        public int RawLineSize { get; private set; }
        public SceGxmTextureFormat TextureFormat { get; private set; }

        public ColorUtilities.PixelFormat PixelFormat { get; private set; }
        public byte[] PixelData { get; private set; }
        public int RoundedWidth { get; private set; }
        public int RoundedHeight { get; private set; }

        public TextureBundle(BinaryReader reader, SceGxtTextureInfo info)
        {
            reader.BaseStream.Seek(info.DataOffset, SeekOrigin.Begin);

            Width = info.GetWidth();
            Height = info.GetHeight();
            PaletteIndex = info.PaletteIndex;
            RawLineSize = (int)(info.DataSize / info.GetHeightRounded());
            TextureFormat = info.GetTextureFormat();

            RoundedWidth = Width; //info.GetWidthRounded();
            RoundedHeight = Height; //info.GetHeightRounded();

            if (!PixelDataProviders.PixelFormatMap.ContainsKey(TextureFormat) ||
                !PixelDataProviders.ProviderFunctions.ContainsKey(TextureFormat))
                throw new FormatNotImplementedException(TextureFormat);

            PixelFormat = PixelDataProviders.PixelFormatMap[TextureFormat];
            PixelData = PixelDataProviders.ProviderFunctions[TextureFormat](reader, info);

            SceGxmTextureBaseFormat textureBaseFormat = info.GetTextureBaseFormat();

            // TODO: is this right? PVRTC/PVRTC2 doesn't need this, but everything else does?
            if (textureBaseFormat != SceGxmTextureBaseFormat.PVRT2BPP &&
                textureBaseFormat != SceGxmTextureBaseFormat.PVRT4BPP &&
                textureBaseFormat != SceGxmTextureBaseFormat.PVRTII2BPP &&
                textureBaseFormat != SceGxmTextureBaseFormat.PVRTII4BPP)
            {
                SceGxmTextureType textureType = info.GetTextureType();
                switch (textureType)
                {
                    case SceGxmTextureType.Linear:
                        // Nothing to be done!
                        break;

                    case SceGxmTextureType.Tiled:
                        // TODO: verify me!
                        PixelData = PostProcessing.UntileTexture(PixelData, info.GetWidthRounded(),
                            info.GetHeightRounded(), PixelFormat);
                        break;

                    case SceGxmTextureType.Swizzled:
                    case SceGxmTextureType.Cube:
                        // TODO: is cube really the same as swizzled? seems that way from CS' *env* files...
                        PixelData = PostProcessing.UnswizzleTexture(PixelData, info.GetWidthRounded(),
                            info.GetHeightRounded(), PixelFormat);
                        break;

                    case (SceGxmTextureType)0xA0000000:
                        // TODO: mehhhhh
                        PixelData = PostProcessing.UnswizzleTexture(PixelData, info.GetWidthRounded(),
                            info.GetHeightRounded(), PixelFormat);
                        break;

                    default:
                        throw new TypeNotImplementedException(textureType);
                }
            }
        }

        public Image<Rgba32> CreateTexture(Color[] palette = null)
        {
            Image<Rgba32> texture = new Image<Rgba32>(RoundedWidth, RoundedHeight);

            int bytesPerPixel = ColorUtilities.GetBytesPerPixel(PixelFormat);

            // Copy pixel data to the image
            texture.ProcessPixelRows(pixelAccessor =>
            {
                for (int y = 0; y < texture.Height; y++)
                {
                    Span<Rgba32> row = pixelAccessor.GetRowSpan(y);

                    for (int x = 0; x < texture.Width; x++)
                    {
                        int srcOffset = (y * texture.Width + x) * bytesPerPixel;

                        if (srcOffset >= PixelData.Length)
                            continue;

                        // Extract color components depending on pixel format
                        byte r = 0, g = 0, b = 0, a = 255;

                        if (PixelFormat == ColorUtilities.PixelFormat.Format32bppArgb)
                        {
                            b = PixelData[srcOffset];
                            g = PixelData[srcOffset + 1];
                            r = PixelData[srcOffset + 2];
                            a = PixelData[srcOffset + 3];
                        }
                        else if (PixelFormat == ColorUtilities.PixelFormat.Format24bppRgb)
                        {
                            b = PixelData[srcOffset];
                            g = PixelData[srcOffset + 1];
                            r = PixelData[srcOffset + 2];
                        }
                        else if (PixelFormat == ColorUtilities.PixelFormat.Format8bppIndexed ||
                                 PixelFormat == ColorUtilities.PixelFormat.Format4bppIndexed)
                        {
                            // Use palette for indexed formats
                            if (palette != null)
                            {
                                // For 8bpp, the index is directly in the pixel data
                                // For 4bpp, we need to extract the index from the byte
                                int index;
                                if (PixelFormat == ColorUtilities.PixelFormat.Format8bppIndexed)
                                {
                                    index = PixelData[srcOffset];
                                }
                                else
                                {
                                    // For 4bpp formats, extract 4 bits based on x position
                                    int byteIndex = srcOffset / 2;
                                    if (byteIndex < PixelData.Length)
                                    {
                                        byte pixelByte = PixelData[byteIndex];
                                        index = (x % 2 == 0) ? (pixelByte >> 4) & 0xF : pixelByte & 0xF;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }

                                if (index < palette.Length)
                                {
                                    var color = palette[index];
                                    // For SixLabors.ImageSharp.Color, we need to extract components:
                                    r = color.ToPixel<Rgba32>().R;
                                    g = color.ToPixel<Rgba32>().G;
                                    b = color.ToPixel<Rgba32>().B;
                                    a = color.ToPixel<Rgba32>().A;
                                }
                            }
                        }

                        row[x] = new Rgba32(r, g, b, a);
                    }
                }
            });

            // Create a new image with the actual dimensions (not rounded)
            Image<Rgba32> realTexture = new Image<Rgba32>(Width, Height);

            // Copy pixels from texture to realTexture
            texture.ProcessPixelRows(realTexture, (sourceAccessor, targetAccessor) =>
            {
                for (int y = 0; y < Height; y++)
                {
                    Span<Rgba32> sourceRow = sourceAccessor.GetRowSpan(y);
                    Span<Rgba32> targetRow = targetAccessor.GetRowSpan(y);

                    for (int x = 0; x < Width; x++)
                    {
                        targetRow[x] = sourceRow[x];
                    }
                }
            });

            return realTexture;
        }
    }
}