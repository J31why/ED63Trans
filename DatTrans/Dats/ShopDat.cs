#region

using System.Text;
using static DatTrans.TransChar;

#endregion

namespace DatTrans.Dats;

public class ShopDatItem
{
    public ushort Entry;
    public ushort Id;
    public byte[] Data = [];
    public ushort SaleItemsOffset;
    public ushort NameOffset;
    public string Name = "";
    public byte[] SaleItems = [];
    public ushort SaleItemsCount;

}

public static class ShopDat
{
    public static List<ShopDatItem> Parse(string file, Encoding encoding)
    {
        var itemList = new List<ShopDatItem>();
        using var fs = new FileStream(file, FileMode.Open);


        for (var i = 0; i < 0xffff; i++)
        {
            if (itemList.Count > 0)
                if (fs.Position == itemList[0].Entry)
                    break;
            var item = new ShopDatItem
            {
                Entry = fs.ReadUshort()
            };

            itemList.Add(item);
            if (item.Entry == 0)
                continue;

            var pos = fs.Position;
            fs.Seek(item.Entry, SeekOrigin.Begin);
            item.Id = fs.ReadUshort();
            item.Data = fs.ReadBytes(0xc);
            item.SaleItemsOffset = fs.ReadUshort();
            item.NameOffset = fs.ReadUshort();
            //name
            fs.Seek(item.NameOffset, SeekOrigin.Begin);
            var ascii = fs.ReadAsciiBytes();
            item.Name = encoding.GetString(ascii);
            //sale items
            fs.Seek(item.SaleItemsOffset, SeekOrigin.Begin);
            item.SaleItemsCount = (ushort)(item.Data[2] + (item.Data[3] << 8));
            item.SaleItems = fs.ReadBytes(item.SaleItemsCount * 2);
            fs.Seek(pos, SeekOrigin.Begin);
        }
        return itemList;
    }

    public static void Generate(string file, List<ShopDatItem> xseedList)
    {
        file = Path.GetFileName(file);
        using var fs = new FileStream(file, FileMode.OpenOrCreate);
        var buffer = new byte[xseedList.Count * 2]; //entrys
        fs.Write(buffer);
        foreach (var xseedItem in xseedList)
        {
            if (xseedItem.Entry == 0)
                continue;
            xseedItem.Entry = (ushort)fs.Position;
            //name
            byte[] nameBytes = [];
            switch (xseedItem.Name)
            {
                case "Base - Monument":
                    xseedItem.Name = "\u3000据点·石碑\u3000";
                    break;
                case "Monument":
                    xseedItem.Name = "\u3000石碑\u3000";
                    break;
                case "Shadow Monument":
                    xseedItem.Name = "\u3000影之石碑\u3000";
                    break;
                case "Base - Tree":
                    xseedItem.Name = "\u3000据点·大树\u3000";
                    break;
                default:
                    break;
            }
            var replacedText = ReplaceClmChars(xseedItem.Name);
            nameBytes = SjisEncoding.GetBytes(replacedText);
            //id
            fs.WriteUshort(xseedItem.Id);
            //data
            fs.Write(xseedItem.Data);
            //items offset
            fs.WriteUshort((ushort)(fs.Position + 4 + nameBytes.Length + 1));
            //name offset
            fs.WriteUshort((ushort)(fs.Position + 2));
            fs.Write(nameBytes);
            fs.WriteByte(0);
            fs.Write(xseedItem.SaleItems);
        }
        fs.Seek(0, SeekOrigin.Begin);
        foreach (var xseedItem in xseedList) fs.WriteUshort(xseedItem.Entry);
        fs.Flush();
    }
}