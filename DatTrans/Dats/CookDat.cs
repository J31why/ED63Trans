#region

using System.Text;
using static DatTrans.TransChar;

#endregion

namespace DatTrans.Dats;

public class CookDatItem
{
    public ushort Entry;
    public ushort Id;
    public byte[] Data = [];
    public ushort NameOffset;
    public string Name = "";
    public ushort DescOffset;
    public string Desc = "";
}

public static class CookDat
{
    public static void Generate(string file, List<CookDatItem> xseedList, List<CookDatItem> yltList)
    {
        file = Path.GetFileName(file);
        using var fs = new FileStream(file, FileMode.OpenOrCreate);
        var buffer = new byte[xseedList.Count * 2]; //entrys
        fs.Write(buffer);

        foreach (var xseedItem in xseedList)
        {
            //text convert
            byte[] nameBytes = [];
            byte[] descBytes = [];
            var yltItem = yltList.Find(x => x.Id == xseedItem.Id);
            if (yltItem == null)
            {
                Console.WriteLine($"空ID: {xseedItem.Id}");
            }
            else
            {
                if (xseedItem.Name is "-" or "--" or "－－－－－－－－－－" or " " or "\u25a0Unused"
                    || string.IsNullOrWhiteSpace(xseedItem.Name)) //空或无需替换的道具文本
                {
                    nameBytes = SjisEncoding.GetBytes(xseedItem.Name);
                    descBytes = SjisEncoding.GetBytes(xseedItem.Desc);
                }
                else
                {
                    if (yltItem.Name is "--" or " " or "-") Console.WriteLine("空ID:" + yltItem.Id);

                    switch (xseedItem.Id)
                    {   
                        case 2:
                            yltItem.Desc = "【大盘】全员 HP15000 CP40\\n如同燃烧中的绯红之火一般，\\n味道酸涩可口的极品汤水。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 7:
                            yltItem.Desc = "【大盘】全员 HP全回复 CP80\\n让所有美食家都为之吼叫赞叹，\\n至高无上的究极火锅料理。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 75:
                            yltItem.Desc = "【携带】HP24000\\n完全存活着的神秘料理。\\n具有解除『战斗不能』的功能。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 43:
                            yltItem.Desc = "【携带】HP16000 DEF+20%\\n连骨头也可吃下去的铁板料理，\\n富含充足的钙质。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                   
                    }
                    var replacedText = ReplaceClmChars(yltItem.Name);
                    nameBytes = SjisEncoding.GetBytes(replacedText);

                    replacedText = ReplaceClmChars(yltItem.Desc);
                    descBytes = SjisEncoding.GetBytes(replacedText);
                }
            }
            
            
            xseedItem.Entry = (ushort)fs.Position;
            //id
            fs.WriteUshort(xseedItem.Id);
            //data
            fs.Write(xseedItem.Data);
            //name pos
            fs.WriteUshort((ushort)(fs.Position + 4));
            //desc pos
            fs.WriteUshort((ushort)(fs.Position + 2 + nameBytes.Length + 1));
            fs.Write(nameBytes);
            fs.WriteByte(0);
            fs.Write(descBytes);
            fs.WriteByte(0);
        }

        fs.Seek(0, SeekOrigin.Begin);
        foreach (var xseedItem in xseedList) fs.WriteUshort(xseedItem.Entry);
        fs.Flush();
    }

    public static List<CookDatItem> Parse(string file, Encoding encoding)
    {
        var itemList = new List<CookDatItem>();
        using var fs = new FileStream(file, FileMode.Open);
        for (var i = 0; i < 0xffff; i++)
        {
            if (itemList.Count > 0)
                if (fs.Position == itemList[0].Entry)
                    break;
            var item = new CookDatItem
            {
                Entry = fs.ReadUshort()
            };

            itemList.Add(item);
            var pos = fs.Position;
            fs.Seek(item.Entry, SeekOrigin.Begin);
            item.Id = fs.ReadUshort();
            item.Data = fs.ReadBytes(0x28);
            item.NameOffset = fs.ReadUshort();
            item.DescOffset = fs.ReadUshort();
            //name
            fs.Seek(item.NameOffset, SeekOrigin.Begin);
            var ascii = fs.ReadAsciiBytes();
            item.Name = encoding.GetString(ascii);
            //desc
            fs.Seek(item.DescOffset, SeekOrigin.Begin);
            ascii = fs.ReadAsciiBytes();
            item.Desc = encoding.GetString(ascii);
            fs.Seek(pos, SeekOrigin.Begin);
        }

        return itemList;
    }
}