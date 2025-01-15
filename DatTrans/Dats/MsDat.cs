using System.Text;

namespace DatTrans.Dats;

public class CraftInfo
{
    public byte[] Data = new byte[0x1c];
    public string Name="";
    public string Desc="";
}
public class MsDatInfo
{
    public byte[] Meta = new byte[0xb6];
    public byte[][] Arts = [];
    public byte[][] Crafts = [];
    public byte[][] SCrafts = [];
    public CraftInfo[] CraftInfo = [];
    public byte[] Data = new byte[0x4];
    public string Name="";
    public string Desc="";
}


public static class MsDat
{
    public static void Generate(string file, MsDatInfo xseedInfo, MsDatInfo yltInfo)
    {
        var text = "";
        byte[] ascii = [];
        if(File.Exists(file))
            File.Delete(file);
        using var fs = new FileStream(file, FileMode.CreateNew);
        fs.Write(xseedInfo.Meta);
        
        fs.WriteByte((byte)xseedInfo.Arts.Length);
        foreach (var arts in xseedInfo.Arts)
            fs.Write(arts);
        
        fs.WriteByte((byte)xseedInfo.Crafts.Length);
        foreach (var craft in xseedInfo.Crafts)
            fs.Write(craft);
        
        fs.WriteByte((byte)xseedInfo.SCrafts.Length);
        foreach (var scraft in xseedInfo.SCrafts)
            fs.Write(scraft);
        
        
        fs.WriteByte((byte)xseedInfo.CraftInfo.Length);
        for (var i = 0; i < xseedInfo.CraftInfo.Length; i++)
        {
            var xseedCraftInfo = xseedInfo.CraftInfo[i];
            var yltCraftInfo = yltInfo.CraftInfo[i];
            fs.Write(xseedCraftInfo.Data);
             text = TransChar.ReplaceClmChars(yltCraftInfo.Name);
             ascii = TransChar.SjisEncoding.GetBytes(text);
            fs.Write(ascii);
            fs.WriteByte(0);
            text = TransChar.ReplaceClmChars(yltCraftInfo.Desc);
            ascii = TransChar.SjisEncoding.GetBytes(text);
            fs.Write(ascii);
            fs.WriteByte(0);
        }
        
        fs.Write(xseedInfo.Data);
        text = TransChar.ReplaceClmChars(yltInfo.Name);
        ascii = TransChar.SjisEncoding.GetBytes(text);
        fs.Write(ascii);
        fs.WriteByte(0);
        
        text = TransChar.ReplaceClmChars(yltInfo.Desc);
        ascii = TransChar.SjisEncoding.GetBytes(text);
        fs.Write(ascii);
        fs.WriteByte(0);
        fs.Flush();
    }
    
    public static MsDatInfo Parse(string file,Encoding encoding)
    {
        using var fs = new FileStream(file, FileMode.Open);
        var info = new MsDatInfo();
        fs.ReadExactly(info.Meta);
        var count = fs.ReadByte();
        info.Arts = new byte[count][];
        for (var i = 0; i < count; i++)
        {
            info.Arts[i] = new byte[0x18];
            fs.ReadExactly(info.Arts[i]);
        }
        count = fs.ReadByte();
        info.Crafts = new byte[count][];
        for (var i = 0; i < count; i++)
        {
            info.Crafts[i] = new byte[0x18];
            fs.ReadExactly(info.Crafts[i]);
        }
        count = fs.ReadByte();
        info.SCrafts = new byte[count][];
        for (var i = 0; i < count; i++)
        {
            info.SCrafts[i] = new byte[0x18];
            fs.ReadExactly(info.SCrafts[i]);
        }
        count = fs.ReadByte();
        info.CraftInfo = new CraftInfo[count];
        for (var i = 0; i < count; i++)
        {
            info.CraftInfo[i] = new CraftInfo();
            fs.ReadExactly(info.CraftInfo[i].Data);
            info.CraftInfo[i].Name = encoding.GetString(fs.ReadAsciiBytes());
            info.CraftInfo[i].Desc = encoding.GetString(fs.ReadAsciiBytes());
        }
        fs.ReadExactly(info.Data);
        info.Name = encoding.GetString(fs.ReadAsciiBytes());
        info.Desc = encoding.GetString(fs.ReadAsciiBytes());
        return info;
    }
}