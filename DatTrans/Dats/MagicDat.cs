#region

using System.Text;
using static DatTrans.TransChar;

#endregion


namespace DatTrans.Dats;

public class MagicDatItem
{
    public ushort Entry;
    public ushort Id;
    public byte[] Data = [];
    public ushort NameOffset;
    public string Name = "";
    public ushort DescOffset;
    public string Desc = "";
}

public static class MagicDat
{
    public static List<MagicDatItem> Parse(string file, Encoding encoding)
    {
        var itemList = new List<MagicDatItem>();
        using var fs = new FileStream(file, FileMode.Open);
        for (var i = 0; i < 0xffff; i++)
        {
            if (itemList.Count > 0)
                if (fs.Position == itemList[0].Entry)
                    break;
            var item = new MagicDatItem
            {
                Entry = fs.ReadUshort()
            };

            itemList.Add(item);
            var pos = fs.Position;
            fs.Seek(item.Entry, SeekOrigin.Begin);
            item.Id = fs.ReadUshort();
            item.Data = fs.ReadBytes(0x1A);
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

    public static void Generate(string file, List<MagicDatItem> xseedList, List<MagicDatItem> yltList)
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
                    //bug fix
                    if (yltItem.Name.Contains("预约预约预约预约"))
                    {
                        yltItem.Name = "预留";
                        yltItem.Desc = "预留";
                    }

                    switch (xseedItem.Id)
                    {
                        case 350:
                            yltItem.Desc = "物理战技：单体·攻击、50%气绝\\n释放可使人失神的剑气，将远处的敌人强行拉到近前。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 362:
                            yltItem.Desc = "辅助战技：全体·STR+20%、SPD+10%\\n提高己方队友的士气。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 260:
                            yltItem.Name = "炮射冲击Ｆ";
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
}