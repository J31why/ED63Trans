
using System.Text;
using System.Text.RegularExpressions;

namespace DatTrans.Dats;
public class FishDatData
{
    public const ushort FishesEntry = 0xf66;
    public byte[] HeadData =[];
    public string[] Fishes = new string[0x1A];
    public ushort DialogEntry;
    public ushort[] DialogTextEntries = new ushort[0x50];
    public string[] DialogTexts = new string[0x50];
}
internal static class FishDat
{
    public static FishDatData Parse(string file, Encoding encoding)
    {
        var data = new FishDatData();
        using var fs = new FileStream(file, FileMode.Open);
      
        fs.Seek(0x8, SeekOrigin.Begin);
        data.DialogEntry = fs.ReadUshort();
        fs.Seek(0, SeekOrigin.Begin);
        data.HeadData = new byte[FishDatData.FishesEntry];
        fs.ReadExactly(data.HeadData);
        var index = 0;
        while (index < data.Fishes.Length) 
        {
            var ascii = fs.ReadAsciiBytes();
            if (ascii.Length == 0) continue;
            var text = encoding.GetString(ascii);
            data.Fishes[index] = text;
            index++;
        }

        fs.Seek(data.DialogEntry, SeekOrigin.Begin);
        for (int i = 0; i < data.DialogTextEntries.Length; i++)
        {
            data.DialogTextEntries[i] = fs.ReadUshort();
            var pos = fs.Position;
            fs.Seek(data.DialogTextEntries[i], SeekOrigin.Begin);
            var ascii = fs.ReadAsciiBytes();
            data.DialogTexts[i] = Escape(encoding.GetString(ascii));
            fs.Seek(pos, SeekOrigin.Begin);
        }

        return data;
    }
    public static void Generate(string file, FishDatData xseedDat, FishDatData xseedVoiceDat, FishDatData yltDat)
    {
        file = Path.GetFileName(file);
        using var fs = new FileStream(file, FileMode.OpenOrCreate);
        fs.Write(xseedDat.HeadData);
        //fish
        foreach (var fish in yltDat.Fishes)
        {
            var text = TransChar.ReplaceClmChars(fish);
            var ascii =TransChar.SjisEncoding.GetBytes(text);
            fs.Write(ascii);
            fs.WriteByte(0);
        }
        //script text
        xseedDat.DialogEntry = (ushort)fs.Position;
        //entries
        fs.Write(new byte[xseedDat.DialogTextEntries.Length * 2]);
        for (int i = 0; i < xseedDat.DialogTextEntries.Length; i++)
        {
            xseedDat.DialogTextEntries[i] = (ushort)fs.Position;
            if (xseedDat.DialogTexts[i] == " \\0x02")
            {
                WriteEscapeText(fs, xseedDat.DialogTexts[i]);
            }
            else
            {
                var text = yltDat.DialogTexts[i];
                if (Regex.IsMatch(xseedVoiceDat.DialogTexts[i], "#\\d+v"))
                {
                    var voice = Regex.Match(xseedVoiceDat.DialogTexts[i], "#\\d+v").Value;
                    text = voice + text;
                }
                WriteEscapeText(fs, text);
            }
        }

        fs.Seek(xseedDat.DialogEntry, SeekOrigin.Begin);
        foreach (var entry in xseedDat.DialogTextEntries)
        {
            fs.WriteUshort(entry);
        }
        fs.Seek(0x8, SeekOrigin.Begin); //text entry
        fs.WriteUshort(xseedDat.DialogEntry);
    }
    public static string Escape(string text)
    {
        return text
            .Replace("\u0001", "\\0x01")
            .Replace("\u0002", "\\0x02")
            .Replace("\u0003", "\\0x03")
            .Replace("\u0004", "\\0x04")
            .Replace("\u0005", "\\0x05");
    }

    public static void WriteEscapeText(FileStream fs, string text)
    {
        text = text.Replace("\\0x01", "\u0001");
        var endCode = -1;
        if (text.EndsWith("\\0x02\\0x03"))
        {
            text = text.Replace("\\0x02\\0x03", "");
            endCode = 0;
        }
        else if (text.EndsWith("\\0x02"))
        {
            text = text.Replace("\\0x02", "");
            endCode = 1;
        }
        text = TransChar.ReplaceClmChars(text);
        var ascii = TransChar.SjisEncoding.GetBytes(text);
        fs.Write(ascii);
        switch (endCode)
        {
            case 0:
                fs.WriteByte(0x02);
                fs.WriteByte(0x03);
                break;
            case 1:
                fs.WriteByte(0x02);
                fs.WriteByte(0x0);
                break;
            default:
                throw new Exception("error end code");
        }
        fs.WriteByte(0);
        fs.WriteByte(0);
        fs.WriteByte(0);
    }
    
  
}
