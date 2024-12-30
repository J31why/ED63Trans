using System.Text;
using static DatTrans.TransChar;

namespace DatTrans.Dats;

public class MemoDatItem
{
    public ushort Entry;
    public ushort Id;
    public byte[] Data = [];
    public ushort TextOffset;
    public string Text = "";
}

public static class MemoDat
{
    public static List<MemoDatItem> Parse(string file, Encoding encoding)
    {
        var itemList = new List<MemoDatItem>();
        using var fs = new FileStream(file, FileMode.Open);
        for (var i = 0; i < 0xffff; i++)
        {
            if (itemList.Count > 0)
                if (fs.Position == itemList[0].Entry)
                    break;
            var item = new MemoDatItem
            {
                Entry = fs.ReadUshort()
            };

            itemList.Add(item);
            var pos = fs.Position;
            fs.Seek(item.Entry, SeekOrigin.Begin);
            item.Id = fs.ReadUshort();
            item.Data = fs.ReadBytes(2);
            item.TextOffset = fs.ReadUshort();
            //name
            fs.Seek(item.TextOffset, SeekOrigin.Begin);
            var ascii = fs.ReadAsciiBytes();
            item.Text = encoding.GetString(ascii);
            fs.Seek(pos, SeekOrigin.Begin);
        }

        return itemList;
    }

    public static void Generate(string datName, List<MemoDatItem> xseedList, List<MemoDatItem> yltList)
    {
        datName = Path.GetFileName(datName);
        using var fs = new FileStream(datName, FileMode.OpenOrCreate);
        var buffer = new byte[xseedList.Count * 2]; //entrys
        fs.Write(buffer);

        for (var i = 0; i < xseedList.Count; i++)
        {
            var xseedItem = xseedList[i];
            var yltItem = yltList[i];
            xseedItem.Entry = (ushort)fs.Position;
            //id
            fs.WriteUshort(xseedItem.Id);
            //data
            fs.Write(xseedItem.Data);
            //text
            fs.WriteUshort((ushort)(fs.Position + 2));

            var replacedText = ReplaceClmChars(yltItem.Text);
            if (xseedItem.Id == 14)
            {
                replacedText = ReplaceClmChars("往事回忆");
            }
            var bytes = SjisEncoding.GetBytes(replacedText);
            fs.Write(bytes);
            fs.WriteByte(0);
        }

        //entry
        fs.Seek(0, SeekOrigin.Begin);
        foreach (var xseedChar in xseedList) fs.WriteUshort(xseedChar.Entry);

        fs.Flush();
    }
}