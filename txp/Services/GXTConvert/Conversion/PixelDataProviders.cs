﻿using GXTConvert.FileFormat;

namespace GXTConvert.Conversion
{
    public static class PixelDataProviders
    {
        public static readonly Dictionary<SceGxmTextureFormat, ColorUtilities.PixelFormat> PixelFormatMap =
            new Dictionary<SceGxmTextureFormat, ColorUtilities.PixelFormat>()
            {
                /* L8       */ { SceGxmTextureFormat.U8_1RRR, ColorUtilities.PixelFormat.Format32bppArgb },
                /* A8       */ { SceGxmTextureFormat.U8_R000, ColorUtilities.PixelFormat.Format32bppArgb },
                /* LA88     */ { SceGxmTextureFormat.U8U8_RGGG, ColorUtilities.PixelFormat.Format32bppArgb },
                /* RG88     */ { SceGxmTextureFormat.U8U8_00GR, ColorUtilities.PixelFormat.Format32bppArgb },
                /* ARGB1555 */ { SceGxmTextureFormat.U1U5U5U5_ARGB, ColorUtilities.PixelFormat.Format16bppArgb1555 },
                /* ARGB4444 */ { SceGxmTextureFormat.U4U4U4U4_ARGB, ColorUtilities.PixelFormat.Format32bppArgb },
                /* RGB565   */ { SceGxmTextureFormat.U5U6U5_RGB, ColorUtilities.PixelFormat.Format16bppRgb565 },
                /* ARGB8888 */ { SceGxmTextureFormat.U8U8U8U8_ARGB, ColorUtilities.PixelFormat.Format32bppArgb },
                /* DXT1     */ { SceGxmTextureFormat.UBC1_ABGR, ColorUtilities.PixelFormat.Format32bppArgb },
                /* DXT3     */ { SceGxmTextureFormat.UBC2_ABGR, ColorUtilities.PixelFormat.Format32bppArgb },
                /* DXT5     */ { SceGxmTextureFormat.UBC3_ABGR, ColorUtilities.PixelFormat.Format32bppArgb },
                /* PVRT2    */ { SceGxmTextureFormat.PVRT2BPP_ABGR, ColorUtilities.PixelFormat.Format32bppArgb },
                /* PVRT4    */ { SceGxmTextureFormat.PVRT4BPP_ABGR, ColorUtilities.PixelFormat.Format32bppArgb },
                /* RGB888   */ { SceGxmTextureFormat.U8U8U8_RGB, ColorUtilities.PixelFormat.Format24bppRgb },
                /* RGB888X  */ { SceGxmTextureFormat.U8U8U8X8_RGB1, ColorUtilities.PixelFormat.Format32bppRgb },
                /* P4       */ { SceGxmTextureFormat.P4_ABGR, ColorUtilities.PixelFormat.Format4bppIndexed },
                /*          */ { SceGxmTextureFormat.P4_ARGB, ColorUtilities.PixelFormat.Format4bppIndexed },
                /*          */ { SceGxmTextureFormat.P4_RGBA, ColorUtilities.PixelFormat.Format4bppIndexed },
                /*          */ { SceGxmTextureFormat.P4_BGRA, ColorUtilities.PixelFormat.Format4bppIndexed },
                /*          */ { SceGxmTextureFormat.P4_1BGR, ColorUtilities.PixelFormat.Format4bppIndexed },
                /*          */ { SceGxmTextureFormat.P4_1RGB, ColorUtilities.PixelFormat.Format4bppIndexed },
                /*          */ { SceGxmTextureFormat.P4_RGB1, ColorUtilities.PixelFormat.Format4bppIndexed },
                /*          */ { SceGxmTextureFormat.P4_BGR1, ColorUtilities.PixelFormat.Format4bppIndexed },
                /* P8       */ { SceGxmTextureFormat.P8_ABGR, ColorUtilities.PixelFormat.Format8bppIndexed },
                /*          */ { SceGxmTextureFormat.P8_ARGB, ColorUtilities.PixelFormat.Format8bppIndexed },
                /*          */ { SceGxmTextureFormat.P8_RGBA, ColorUtilities.PixelFormat.Format8bppIndexed },
                /*          */ { SceGxmTextureFormat.P8_BGRA, ColorUtilities.PixelFormat.Format8bppIndexed },
                /*          */ { SceGxmTextureFormat.P8_1BGR, ColorUtilities.PixelFormat.Format8bppIndexed },
                /*          */ { SceGxmTextureFormat.P8_1RGB, ColorUtilities.PixelFormat.Format8bppIndexed },
                /*          */ { SceGxmTextureFormat.P8_RGB1, ColorUtilities.PixelFormat.Format8bppIndexed },
                /*          */ { SceGxmTextureFormat.P8_BGR1, ColorUtilities.PixelFormat.Format8bppIndexed },
            };

        public delegate byte[] ProviderFunctionDelegate(BinaryReader reader, SceGxtTextureInfo info);

        public static readonly Dictionary<SceGxmTextureFormat, ProviderFunctionDelegate> ProviderFunctions =
            new Dictionary<SceGxmTextureFormat, ProviderFunctionDelegate>()
            {
                { SceGxmTextureFormat.U8_1RRR, new ProviderFunctionDelegate(PixelProviderU8_1RRR) },
                { SceGxmTextureFormat.U8_R000, new ProviderFunctionDelegate(PixelProviderU8_R000) }, // TODO: verify me!
                {
                    SceGxmTextureFormat.U8U8_RGGG, new ProviderFunctionDelegate(PixelProviderU8U8_RGGG)
                }, // TODO: verify me!
                {
                    SceGxmTextureFormat.U8U8_00GR, new ProviderFunctionDelegate(PixelProviderU8U8_00GR)
                }, // TODO: verify me!
                { SceGxmTextureFormat.U1U5U5U5_ARGB, new ProviderFunctionDelegate(PixelProviderDirect) },
                {
                    SceGxmTextureFormat.U4U4U4U4_ARGB, new ProviderFunctionDelegate(PixelProviderU4U4U4U4_ARGB)
                }, // TODO: verify me!
                {
                    SceGxmTextureFormat.U5U6U5_RGB, new ProviderFunctionDelegate(PixelProviderDirect)
                }, // TODO: verify me!
                { SceGxmTextureFormat.U8U8U8U8_ARGB, new ProviderFunctionDelegate(PixelProviderDirect) },
                { SceGxmTextureFormat.UBC1_ABGR, new ProviderFunctionDelegate(PixelProviderDXTx) },
                { SceGxmTextureFormat.UBC2_ABGR, new ProviderFunctionDelegate(PixelProviderDXTx) },
                { SceGxmTextureFormat.UBC3_ABGR, new ProviderFunctionDelegate(PixelProviderDXTx) },
                { SceGxmTextureFormat.PVRT2BPP_ABGR, new ProviderFunctionDelegate(PixelProviderPVRTC) },
                //{ SceGxmTextureFormat.PVRT2BPP_1BGR, new ProviderFunctionDelegate(PixelProviderPVRTC) },
                { SceGxmTextureFormat.PVRT4BPP_ABGR, new ProviderFunctionDelegate(PixelProviderPVRTC) },
                //{ SceGxmTextureFormat.PVRT4BPP_1BGR, new ProviderFunctionDelegate(PixelProviderPVRTC) },
                //{ SceGxmTextureFormat.PVRTII2BPP_ABGR, new ProviderFunctionDelegate(PixelProviderPVRTC) },
                //{ SceGxmTextureFormat.PVRTII2BPP_1BGR, new ProviderFunctionDelegate(PixelProviderPVRTC) },
                //{ SceGxmTextureFormat.PVRTII4BPP_ABGR, new ProviderFunctionDelegate(PixelProviderPVRTC) },
                //{ SceGxmTextureFormat.PVRTII4BPP_1BGR, new ProviderFunctionDelegate(PixelProviderPVRTC) },
                { SceGxmTextureFormat.U8U8U8_RGB, new ProviderFunctionDelegate(PixelProviderDirect) },
                { SceGxmTextureFormat.U8U8U8X8_RGB1, new ProviderFunctionDelegate(PixelProviderDirect) },
                {
                    SceGxmTextureFormat.P4_ABGR, new ProviderFunctionDelegate(PixelProviderP4)
                }, // TODO: verify ALL these once files or bug reports come in!
                { SceGxmTextureFormat.P4_ARGB, new ProviderFunctionDelegate(PixelProviderP4) },
                { SceGxmTextureFormat.P4_RGBA, new ProviderFunctionDelegate(PixelProviderP4) },
                { SceGxmTextureFormat.P4_BGRA, new ProviderFunctionDelegate(PixelProviderP4) },
                { SceGxmTextureFormat.P4_1BGR, new ProviderFunctionDelegate(PixelProviderP4) },
                { SceGxmTextureFormat.P4_1RGB, new ProviderFunctionDelegate(PixelProviderP4) },
                { SceGxmTextureFormat.P4_RGB1, new ProviderFunctionDelegate(PixelProviderP4) },
                { SceGxmTextureFormat.P4_BGR1, new ProviderFunctionDelegate(PixelProviderP4) },
                { SceGxmTextureFormat.P8_ABGR, new ProviderFunctionDelegate(PixelProviderDirect) },
                { SceGxmTextureFormat.P8_ARGB, new ProviderFunctionDelegate(PixelProviderDirect) },
                { SceGxmTextureFormat.P8_RGBA, new ProviderFunctionDelegate(PixelProviderDirect) },
                { SceGxmTextureFormat.P8_BGRA, new ProviderFunctionDelegate(PixelProviderDirect) },
                { SceGxmTextureFormat.P8_1BGR, new ProviderFunctionDelegate(PixelProviderDirect) },
                { SceGxmTextureFormat.P8_1RGB, new ProviderFunctionDelegate(PixelProviderDirect) },
                { SceGxmTextureFormat.P8_RGB1, new ProviderFunctionDelegate(PixelProviderDirect) },
                { SceGxmTextureFormat.P8_BGR1, new ProviderFunctionDelegate(PixelProviderDirect) },
            };

        private static byte[] PixelProviderDirect(BinaryReader reader, SceGxtTextureInfo info)
        {
            return reader.ReadBytes((int)info.DataSize);
        }

        private static byte[] PixelProviderDXTx(BinaryReader reader, SceGxtTextureInfo info)
        {
            return Compression.DXTx.Decompress(reader, info);
        }

        private static byte[] PixelProviderPVRTC(BinaryReader reader, SceGxtTextureInfo info)
        {
            return Compression.PVRTC.Decompress(reader, info);
        }

        private static byte[] PixelProviderP4(BinaryReader reader, SceGxtTextureInfo info)
        {
            byte[] pixelData = new byte[info.DataSize];
            for (int i = 0; i < pixelData.Length; i++)
            {
                byte idx = reader.ReadByte();
                pixelData[i] = (byte)(idx >> 4 | idx << 4);
            }

            return pixelData;
        }

        private static byte[] PixelProviderU8_1RRR(BinaryReader reader, SceGxtTextureInfo info)
        {
            byte[] pixelData = new byte[info.DataSize * 4];
            for (int i = 0; i < pixelData.Length; i += 4)
            {
                pixelData[i + 0] = pixelData[i + 1] = pixelData[i + 2] = reader.ReadByte();
                pixelData[i + 3] = 0xFF;
            }

            ;
            return pixelData;
        }

        private static byte[] PixelProviderU8_R000(BinaryReader reader, SceGxtTextureInfo info)
        {
            byte[] pixelData = new byte[info.DataSize * 4];
            for (int i = 0; i < pixelData.Length; i += 4)
            {
                pixelData[i + 0] = pixelData[i + 1] = pixelData[i + 2] = 0x00;
                pixelData[i + 3] = reader.ReadByte();
            }

            ;
            return pixelData;
        }

        private static byte[] PixelProviderU8U8_RGGG(BinaryReader reader, SceGxtTextureInfo info)
        {
            byte[] pixelData = new byte[info.DataSize * 4];
            for (int i = 0; i < pixelData.Length; i += 4)
            {
                pixelData[i + 3] = reader.ReadByte();
                pixelData[i + 0] = pixelData[i + 1] = pixelData[i + 2] = reader.ReadByte();
            }

            ;
            return pixelData;
        }

        private static byte[] PixelProviderU8U8_00GR(BinaryReader reader, SceGxtTextureInfo info)
        {
            byte[] pixelData = new byte[info.DataSize * 4];
            for (int i = 0; i < pixelData.Length; i += 4)
            {
                pixelData[i + 1] = reader.ReadByte();
                pixelData[i + 2] = reader.ReadByte();
                pixelData[i + 0] = pixelData[i + 3] = 0x00;
            }

            ;
            return pixelData;
        }

        private static byte[] PixelProviderU4U4U4U4_ARGB(BinaryReader reader, SceGxtTextureInfo info)
        {
            byte[] pixelData = new byte[info.DataSize * 4];
            for (int i = 0; i < pixelData.Length; i += 4)
            {
                ushort rgba = reader.ReadUInt16();
                pixelData[i + 0] = (byte)(((rgba >> 4) << 4) & 0xFF);
                pixelData[i + 1] = (byte)(((rgba >> 8) << 4) & 0xFF);
                pixelData[i + 2] = (byte)(((rgba >> 12) << 4) & 0xFF);
                pixelData[i + 3] = (byte)((rgba << 4) & 0xFF);
            }

            ;
            return pixelData;
        }
    }
}