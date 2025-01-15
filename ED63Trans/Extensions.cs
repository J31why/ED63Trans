using System.Collections.Generic;
using System.IO;

namespace ED63Trans;

public static class Extensions
{
    public static ushort ReadUshort(this FileStream fs)
    {
        var a = fs.ReadByte();
        a += fs.ReadByte() << 8;
        return (ushort)a;
    }

    public static void WriteUshort(this FileStream fs, ushort Ushort)
    {
        var bytes = new byte[2];
        bytes[0] = (byte)(Ushort & 0xff);
        bytes[1] = (byte)((Ushort >> 8) & 0xff);
        fs.Write(bytes);
    }

    public static uint ReadUint(this FileStream fs)
    {
        var a = fs.ReadByte();
        a += fs.ReadByte() << 8;
        a += fs.ReadByte() << 0x10;
        a += fs.ReadByte() << 0x18;
        return (uint)a;
    }

    public static void WriteUint(this FileStream fs, uint Uint)
    {
        var bytes = new byte[4];
        bytes[0] = (byte)(Uint & 0xff);
        bytes[1] = (byte)((Uint >> 8) & 0xff);
        bytes[2] = (byte)((Uint >> 0x10) & 0xff);
        bytes[3] = (byte)((Uint >> 0x18) & 0xff);
        fs.Write(bytes);
    }

    public static byte[] ReadBytes(this FileStream fs, int length)
    {
        var bytes = new byte[length];
        fs.ReadExactly(bytes);
        return bytes;
    }

    public static byte[] ReadAsciiBytes(this FileStream fs)
    {
        List<byte> bytes = [];
        int b;
        while ((b = fs.ReadByte()) != 0 && fs.Position < fs.Length) bytes.Add((byte)b);
        return bytes.ToArray();
    }
}