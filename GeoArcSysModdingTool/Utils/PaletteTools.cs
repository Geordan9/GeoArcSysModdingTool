using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GeoArcSysModdingTool.Utils
{
    public static class PaletteTools
    {
        public static BitmapPalette ReadACTPalette(byte[] bytes, int colorRange)
        {
            try
            {
                using (var reader = new BinaryReader(new MemoryStream(bytes)))
                {
                    var colors = new Color[colorRange];
                    for (var i = 0; i < colorRange; i++)
                    {
                        var red = reader.ReadByte();
                        var green = reader.ReadByte();
                        var blue = reader.ReadByte();
                        colors[i] = Color.FromRgb(red, green, blue);
                    }

                    reader.Close();
                    return new BitmapPalette(colors);
                }
            }
            catch
            {
                return null;
            }
        }

        public static BitmapPalette ReadPALPalette(byte[] bytes, int colorRange)
        {
            try
            {
                using (var reader = new BinaryReader(new MemoryStream(bytes)))
                {
                    reader.BaseStream.Seek(22, SeekOrigin.Begin);
                    int palColorRange = reader.ReadInt16();
                    if ((uint) palColorRange > (uint) colorRange)
                        palColorRange = colorRange;
                    var colors = new Color[colorRange];
                    for (var _i = 0; _i < palColorRange; _i++)
                    {
                        var red = reader.ReadByte();
                        var green = reader.ReadByte();
                        var blue = reader.ReadByte();
                        var alpha = reader.ReadByte();
                        colors[colors.Length - 1 - _i] = Color.FromArgb(alpha, red, green, blue);
                    }

                    return new BitmapPalette(colors);
                }
            }
            catch
            {
                return null;
            }
        }

        public static BitmapPalette ReadACOPalette(byte[] bytes, int colorRange)
        {
            try
            {
                using (var reader = new BinaryReader(new MemoryStream(bytes)))
                {
                    reader.BaseStream.Seek(2, SeekOrigin.Current);
                    var colorrangebytes = reader.ReadBytes(2);
                    Array.Reverse(colorrangebytes);
                    var colorrange = (int) BitConverter.ToInt16(colorrangebytes, 0);
                    reader.BaseStream.Seek(2, SeekOrigin.Current);
                    if (colorrange > colorRange)
                        colorrange = colorRange;
                    var colors = new Color[colorrange];
                    for (var i = 0; i < colorrange; i++)
                    {
                        var red = reader.ReadByte();
                        reader.ReadByte();
                        var green = reader.ReadByte();
                        reader.ReadByte();
                        var blue = reader.ReadByte();
                        reader.ReadBytes(5);
                        colors[i] = Color.FromRgb(red, green, blue);
                    }

                    reader.Close();
                    return new BitmapPalette(colors);
                }
            }
            catch
            {
                return null;
            }
        }

        public static BitmapPalette ReadASEPalette(byte[] bytes, int colorRange)
        {
            try
            {
                using (var reader = new BinaryReader(new MemoryStream(bytes)))
                {
                    reader.BaseStream.Seek(8, SeekOrigin.Current);
                    var totalblocksbytes = reader.ReadBytes(4);
                    Array.Reverse(totalblocksbytes);
                    var totalblocks = BitConverter.ToInt32(totalblocksbytes, 0);
                    if (totalblocks > colorRange)
                        totalblocks = colorRange;
                    var colors = new Color[totalblocks];
                    for (var i = 0; i < totalblocks; i++)
                    {
                        reader.BaseStream.Seek(2, SeekOrigin.Current);
                        var blocklengthbytes = reader.ReadBytes(4);
                        Array.Reverse(blocklengthbytes);
                        var blocklength = BitConverter.ToInt32(blocklengthbytes, 0);
                        if (blocklength == 0)
                            break;
                        var blockstringlengthbytes = reader.ReadBytes(2);
                        Array.Reverse(blockstringlengthbytes);
                        var blockstringlength = BitConverter.ToInt16(blockstringlengthbytes, 0) * 2;
                        reader.ReadBytes(blockstringlength);
                        if (blocklength - blockstringlength == 20)
                        {
                            reader.BaseStream.Seek(6, SeekOrigin.Current);
                            var red = reader.ReadByte();
                            reader.BaseStream.Seek(3, SeekOrigin.Current);
                            var green = reader.ReadByte();
                            reader.BaseStream.Seek(3, SeekOrigin.Current);
                            var blue = reader.ReadByte();
                            reader.BaseStream.Seek(3, SeekOrigin.Current);
                            colors[i] = Color.FromRgb(red, green, blue);
                        }
                        else
                        {
                            --i;
                        }
                    }

                    reader.Close();
                    return new BitmapPalette(colors);
                }
            }
            catch
            {
                return null;
            }
        }

        public static byte[] CreateHPLByteArray(Color[] colors)
        {
            var length = colors.Length * 4 + 0x20;
            var bytes = new byte[length];
            using (var binaryWriter = new BinaryWriter(new MemoryStream(bytes)))
            {
                binaryWriter.Write(0x4C415048); // HPAL
                binaryWriter.Write(0x125);
                binaryWriter.Write(length);
                binaryWriter.Write(colors.Length);
                binaryWriter.Write(0);
                binaryWriter.Write(0);
                binaryWriter.Write(0x10000001);
                binaryWriter.Write(0);
                foreach (var color in colors)
                {
                    binaryWriter.Write(color.B);
                    binaryWriter.Write(color.G);
                    binaryWriter.Write(color.R);
                    binaryWriter.Write(color.A);
                }
            }

            return bytes;
        }

        public static byte[] CreateACTByteArray(Color[] colors)
        {
            var length = colors.Length * 3;
            var bytes = new byte[length];
            using (var binaryWriter = new BinaryWriter(new MemoryStream(bytes)))
            {
                for (var i = 0; i < colors.Length; i++)
                {
                    binaryWriter.Write(colors[i].R);
                    binaryWriter.Write(colors[i].G);
                    binaryWriter.Write(colors[i].B);
                }

                ;
            }

            return bytes;
        }
    }
}