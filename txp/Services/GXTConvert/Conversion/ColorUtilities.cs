using SixLabors.ImageSharp;

namespace GXTConvert.Conversion
{
    public static class ColorUtilities
    {
        // Conversion between System.Drawing.PixelFormat and ImageSharp pixel formats
        public enum PixelFormat
        {
            Format32bppArgb,
            Format32bppRgb,
            Format24bppRgb,
            Format16bppArgb1555,
            Format16bppRgb565,
            Format8bppIndexed,
            Format4bppIndexed
        }

        // Get byte size for each pixel format
        public static int GetPixelFormatSize(PixelFormat format)
        {
            return format switch
            {
                PixelFormat.Format32bppArgb => 32,
                PixelFormat.Format32bppRgb => 32,
                PixelFormat.Format24bppRgb => 24,
                PixelFormat.Format16bppArgb1555 => 16,
                PixelFormat.Format16bppRgb565 => 16,
                PixelFormat.Format8bppIndexed => 8,
                PixelFormat.Format4bppIndexed => 4,
                _ => 32
            };
        }

        // Create a Color from RGBA components
        public static Color FromArgb(byte a, byte r, byte g, byte b)
        {
            return Color.FromRgba(r, g, b, a);
        }

        // Create a Color with full alpha
        public static Color FromArgb(byte r, byte g, byte b)
        {
            return Color.FromRgb(r, g, b);
        }

        // Get bytes per pixel for a specific format
        public static int GetBytesPerPixel(PixelFormat format)
        {
            return GetPixelFormatSize(format) / 8;
        }
    }
}