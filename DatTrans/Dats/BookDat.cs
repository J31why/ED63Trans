#region

using System.Text;
using static DatTrans.TransChar;

#endregion

namespace DatTrans.Dats;

public class BookDatItem
{
    public ushort Entry;
    public string Content = "";
}

public static class BookDat
{
    public static void Generate(string file, List<BookDatItem> xseedList, List<BookDatItem> yltList)
    {
        file = Path.GetFileName(file);
        using var fs = new FileStream(file, FileMode.OpenOrCreate);
        var buffer = new byte[xseedList.Count * 2]; //entrys
        fs.Write(buffer);

        for (var i = 0; i < xseedList.Count; i++)
        {
            var xseedItem = xseedList[i];
            var yltItem = yltList[i];
            var replacedText = ReplaceClmChars(yltItem.Content);
            var contentBytes = SjisEncoding.GetBytes(replacedText);

            xseedItem.Entry = (ushort)fs.Position;
            //content
            fs.Write(contentBytes);
            fs.WriteByte(0);
        }

        fs.Seek(0, SeekOrigin.Begin);
        foreach (var xseedItem in xseedList) fs.WriteUshort(xseedItem.Entry);
        fs.Flush();
    }

    public static List<BookDatItem> Parse(string file, Encoding encoding)
    {
        var itemList = new List<BookDatItem>();
        using var fs = new FileStream(file, FileMode.Open);
        for (var i = 0; i < 0xffff; i++)
        {
            if (itemList.Count > 0)
                if (fs.Position == itemList[0].Entry)
                    break;
            var item = new BookDatItem
            {
                Entry = fs.ReadUshort()
            };

            itemList.Add(item);
            var pos = fs.Position;
            fs.Seek(item.Entry, SeekOrigin.Begin);

            var ascii = fs.ReadAsciiBytes();
            item.Content = encoding.GetString(ascii);

            fs.Seek(pos, SeekOrigin.Begin);
        }

        return itemList;
    }
}