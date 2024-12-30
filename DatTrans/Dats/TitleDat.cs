#region

using System.Text;
using static DatTrans.TransChar;

#endregion

namespace DatTrans.Dats;

public class TitleDatItem
{
    public ushort Entry;
    public List<ushort> TextEntries = [];
    public List<string> Texts = [];
}

public static class TitleDat
{
    public static void Generate(string file, List<TitleDatItem> xseedList, List<TitleDatItem> yltList)
    {
        file = Path.GetFileName(file);
        using var fs = new FileStream(file, FileMode.OpenOrCreate);
        var buffer = new byte[xseedList.Count * 2]; //entrys
        fs.Write(buffer);

        for (var i = 0; i < xseedList.Count; i++)
        {
            var xseedItem = xseedList[i];
            var yltItem = yltList[i];
            xseedItem.Entry = (ushort)fs.Position;
            buffer = new byte[xseedItem.TextEntries.Count * 2]; //entrys
            fs.Write(buffer);

            for (var j = 0; j < xseedItem.TextEntries.Count; j++)
            {
                var replacedText = ReplaceClmChars(yltItem.Texts[j].Trim());
                var contentBytes = SjisEncoding.GetBytes(replacedText);
                xseedItem.TextEntries[j] = (ushort)fs.Position;
                fs.Write(contentBytes);
                fs.WriteByte(0);
            }

            var itemEndPos = fs.Position;

            fs.Seek(xseedItem.Entry, SeekOrigin.Begin);
            foreach (var entry in xseedItem.TextEntries) fs.WriteUshort(entry);
            fs.Seek(itemEndPos, SeekOrigin.Begin);
        }

        fs.Seek(0, SeekOrigin.Begin);
        foreach (var xseedItem in xseedList) fs.WriteUshort(xseedItem.Entry);
        fs.Flush();
    }

    public static List<TitleDatItem> Parse(string file, Encoding encoding)
    {
        var itemList = new List<TitleDatItem>();
        using var fs = new FileStream(file, FileMode.Open);
        for (var i = 0; i < 0xffff; i++)
        {
            if (itemList.Count > 0)
                if (fs.Position == itemList[0].Entry)
                    break;

            var item = new TitleDatItem
            {
                Entry = fs.ReadUshort()
            };

            itemList.Add(item);
            var pos = fs.Position;
            fs.Seek(item.Entry, SeekOrigin.Begin);
            for (var j = 0; j < 0xff; j++)
            {
                if (item.TextEntries.Count > 0)
                    if (fs.Position == item.TextEntries[0])
                        break;
                var entry = fs.ReadUshort();
                item.TextEntries.Add(entry);
                var pos2 = fs.Position;
                fs.Seek(entry, SeekOrigin.Begin);
                var ascii = fs.ReadAsciiBytes();
                var text = encoding.GetString(ascii);
                item.Texts.Add(text);
                fs.Seek(pos2, SeekOrigin.Begin);
            }

            fs.Seek(pos, SeekOrigin.Begin);
        }

        return itemList;
    }
}