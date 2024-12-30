using System.Text;
using static DatTrans.TransChar;

namespace DatTrans.Dats;

public class ChainDatBaseItem
{
    public ushort Entry;
    public ushort TextEntry;

    // 0x6 bytes {持续时间,语音id,0x0,0x0}
    //B6 04 20 03 0A 04 00 00
    public byte[] Data = [];

    public string Text = "";
}

public class ChainDatData
{
    public ushort BaseItems_2_Entry;

    //78 01 00 00 00 00 00 00
    public ChainDatBaseItem[,] BaseItems_2 = new ChainDatBaseItem[20, 2];

    public ushort BaseItems_3_Entry;

    //AE 04
    public ChainDatBaseItem[] BaseItems_3 = new ChainDatBaseItem[20];
}

public static class ChainDat
{
    public static void Generate(string file, ChainDatData xseedData, ChainDatData yltData)
    {
        file = Path.GetFileName(file);
        using var fs = new FileStream(file, FileMode.OpenOrCreate);
        byte[] head =
        [
            0x08, 0x00, 0x20, 0x00, 0x38, 0x00, 0x11, 0x05, 0x00, 0x00,
            0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00,
            0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        ];
        fs.Write(head);
        //2
        var entriesPos = fs.Position;
        var entries = new byte[xseedData.BaseItems_2.GetLength(0) * 2 * 4 * 2];
        fs.Write(entries);
        for (int i = 0; i < xseedData.BaseItems_2.GetLength(0); i++)
        {
            var text = yltData.BaseItems_2[i, 0].Text;
            WriteBaseItem(fs, xseedData.BaseItems_2[i, 0], text);
        }
        for (int i = 0; i < xseedData.BaseItems_2.GetLength(0); i++)
        {
            var text = yltData.BaseItems_2[i, 1].Text;
            WriteBaseItem(fs, xseedData.BaseItems_2[i, 1], text);
        }
        var endPos = fs.Position;
        //write entries
        fs.Seek(entriesPos, SeekOrigin.Begin);
        for (int i = 0; i < xseedData.BaseItems_2.GetLength(0); i++)
        {
            fs.WriteUshort(xseedData.BaseItems_2[i, 0].Entry);
            fs.Write(new byte[6]);
            fs.WriteUshort(xseedData.BaseItems_2[i, 1].Entry);
            fs.Write(new byte[6]);
        }

        //3
        //entry
        fs.Seek(0x6, SeekOrigin.Begin);
        fs.WriteUshort((ushort)endPos);
        fs.Seek(endPos, SeekOrigin.Begin);
        entriesPos = fs.Position;
        fs.Write(new byte[xseedData.BaseItems_3.Length * 2]);
        for (int i = 0; i < xseedData.BaseItems_3.Length; i++)
        {
            var text = yltData.BaseItems_3[i].Text;
            WriteBaseItem(fs, xseedData.BaseItems_3[i], text);
        }
        //entries
        fs.Seek(endPos, SeekOrigin.Begin);
        for (int i = 0; i < xseedData.BaseItems_3.Length; i++)
        {
            fs.WriteUshort(xseedData.BaseItems_3[i].Entry);
        }
    }

    public static ChainDatData Parse(string file, Encoding encoding)
    {
        var data = new ChainDatData();
        using var fs = new FileStream(file, FileMode.Open);
        fs.Seek(0x4, SeekOrigin.Begin);
        data.BaseItems_2_Entry = fs.ReadUshort();
        data.BaseItems_3_Entry = fs.ReadUshort();
        fs.Seek(data.BaseItems_2_Entry, SeekOrigin.Begin);
        // 2
        for (int i = 0; i < data.BaseItems_2.GetLength(0); i++)
        {
            data.BaseItems_2[i, 0] = new ChainDatBaseItem
            {
                Entry = fs.ReadUshort(),
            };
            fs.Seek(0x6, SeekOrigin.Current);
            data.BaseItems_2[i, 1] = new ChainDatBaseItem
            {
                Entry = fs.ReadUshort(),
            };
            fs.Seek(0x6, SeekOrigin.Current);
            var pos = fs.Position;
            //parse item
            ReadBaseItem(fs, ref data.BaseItems_2[i, 0], encoding);
            ReadBaseItem(fs, ref data.BaseItems_2[i, 1], encoding);

            fs.Seek(pos, SeekOrigin.Begin);
        }
        // 3
        fs.Seek(data.BaseItems_3_Entry, SeekOrigin.Begin);
        for (int i = 0; i < data.BaseItems_3.Length; i++)
        {
            data.BaseItems_3[i] = new ChainDatBaseItem
            {
                Entry = fs.ReadUshort(),
            };

            var pos = fs.Position;
            //parse item
            ReadBaseItem(fs, ref data.BaseItems_3[i], encoding);

            fs.Seek(pos, SeekOrigin.Begin);
        }
        return data;
    }

    public static void WriteBaseItem(FileStream fs, ChainDatBaseItem item, string text)
    {
        text = text switch
        {
            //fix
            "Ｌｅｔ＇ｓ\u3000ｇｏ！" => "一起上！",
            "这样就收工啦！！" => "这样就结束了！！",
            _ => text
        };
        item.Entry = (ushort)fs.Position;
        item.TextEntry = (ushort)(fs.Position + 8);
        fs.WriteUshort(item.TextEntry);
        fs.Write(item.Data);
        var replacedText = TransChar.ReplaceClmChars(text);
        var textBytes = SjisEncoding.GetBytes(replacedText);
        fs.Write(textBytes);
        fs.WriteByte(0);
    }

    public static void ReadBaseItem(FileStream fs, ref ChainDatBaseItem item, Encoding encoding)
    {
        fs.Seek(item.Entry, SeekOrigin.Begin);
        item.TextEntry = fs.ReadUshort();
        item.Data = fs.ReadBytes(0x6);
        fs.Seek(item.TextEntry, SeekOrigin.Begin);
        var ascii = fs.ReadAsciiBytes();
        item.Text = encoding.GetString(ascii);
    }
}