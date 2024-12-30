#region

using System.Text;
using static DatTrans.TransChar;

#endregion

namespace DatTrans.Dats;

public class NameDtItem
{
    public ushort Entry;
    public ushort Id;
    public byte[] Data = [];
    public ushort NameOffSet;
    public string Name = "";
}

public static class NameDat
{
    public static void Generate(string file, List<NameDtItem> xseedList, List<NameDtItem> yltList)
    {
        file = Path.GetFileName(file);
        using var fs = new FileStream(file, FileMode.OpenOrCreate);
        fs.WriteUshort(0xe8); //head
        var buffer = new byte[xseedList.Count * 2]; //entrys
        fs.Write(buffer);
        foreach (var xseedItem in xseedList)
        {
            xseedItem.Entry = (ushort)fs.Position;
            //id
            fs.WriteUshort(xseedItem.Id);
            //unknown data
            fs.Write(xseedItem.Data);
            //name position
            fs.WriteUshort((ushort)(fs.Position + 2));
            //name
            var c = xseedItem;
            var yltChar = yltList.Find(x => x.Id == c.Id);
            if (yltChar == null) Console.WriteLine($"空ID: {xseedItem.Id}");
            var replacedText = ReplaceClmChars(yltChar.Name);
            var bytes = SjisEncoding.GetBytes(replacedText);
            fs.Write(bytes);
            fs.WriteByte(0);
        }

        //entry
        fs.Seek(2, SeekOrigin.Begin);
        foreach (var xseedChar in xseedList) fs.WriteUshort(xseedChar.Entry);

        fs.Flush();
    }

    public static List<NameDtItem> Parse(string file, Encoding encoding)
    {
        var itemList = new List<NameDtItem>();
        using var fs = new FileStream(file, FileMode.Open);
        fs.ReadUshort(); // head 0xE8 0x00 
        for (var i = 0; i < 0xffff; i++) //3rd character count = 309 //0x135
        {
            if (itemList.Count > 0)
                if (fs.Position == itemList[0].Entry)
                    break;

            var c = new NameDtItem
            {
                Entry = fs.ReadUshort()
            };
            itemList.Add(c);
            var pos = fs.Position;
            fs.Seek(c.Entry, SeekOrigin.Begin);

            c.Id = fs.ReadUshort();
            c.Data = fs.ReadBytes(0x1e);
            c.NameOffSet = fs.ReadUshort();
            fs.Seek(c.NameOffSet, SeekOrigin.Begin);
            var ascii = fs.ReadAsciiBytes();
            c.Name = encoding.GetString(ascii);

            fs.Seek(pos, SeekOrigin.Begin);
        }

        return itemList;
    }
}