#region

using System.Text;
using static DatTrans.TransChar;

#endregion

namespace DatTrans.Dats;

public class ItTextDatItem
{
    public ushort Entry;
    public uint Id;
    public ushort NameOffset;
    public string Name = "";
    public ushort DescOffset;
    public string Desc = "";
}

public static class ItTxtDat
{
    public static void Generate(string file, List<ItTextDatItem> xseedList, List<ItTextDatItem> yltList)
    {
        file = Path.GetFileName(file);
        using var fs = new FileStream(file, FileMode.OpenOrCreate);
        var buffer = new byte[xseedList.Count * 2]; //entries
        fs.Write(buffer);
        foreach (var xseedItem in xseedList)
        {
            //text convert
            byte[] nameBytes = [];
            byte[] descBytes = [];
            var item = xseedItem;
            var yltItem = yltList.Find(x => x.Id == item.Id);
            if (yltItem == null)
            {
                switch (xseedItem.Id)
                {
                    //entry: 37634    id: 750         text: Time Gem          text2: STR-20%, DEF-20%, ATS-15% (#93ICast 3)
                    case 750:
                        var replacedText = ReplaceClmChars("刻耀珠");
                        nameBytes = SjisEncoding.GetBytes(replacedText);
                        replacedText = ReplaceClmChars("STR-20%, DEF-20%, ATS-15% (#93I驱动３)");
                        descBytes = SjisEncoding.GetBytes(replacedText);
                        Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                        break;
                }
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
                    if (yltItem.Name is "--" or " " or "-")
                        //xseed版新物品 id: 527,658,659
                        Console.WriteLine("空ID:" + yltItem.Id);
                    switch (xseedItem.Id)
                    {
                        //bug fix
                        case 527:
                            //Monster Guide
                            yltItem.Name = "魔兽手册";
                            yltItem.Desc = "记录收集到的魔兽情报的手册。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;

                        case 658:
                            //Mirror
                            yltItem.Name = "镜子";
                            yltItem.Desc = "50%机率反射物理伤害。最大HP-20%";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;

                        case 659:
                            //Reflect
                            yltItem.Name = "反射";
                            yltItem.Desc = "50%机率反射魔法伤害。最大EP-20%";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;

                        case 709:
                            //木耀珠 娱乐通汉化BUG
                            yltItem.Desc = yltItem.Desc.Replace("妨害", "妨碍");
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;

                        case 1210:
                            yltItem.Name = "花剑+";
                            yltItem.Desc = "【STR+100】\\n\u3000专为突刺而设计的细剑。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;

                        case 1211:
                            yltItem.Name = "尖剑+";
                            yltItem.Desc = "【STR+200】\\n\u3000高级者用的细剑，以柔制刚的武器。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;

                        case 1420:
                            //Fencer+1
                            yltItem.Name = "击剑+1";
                            yltItem.Desc = "【STR+100】\\n\u3000一把对学习基础剑术至关重要的训练用剑。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;

                        case 1421:
                            yltItem.Name = "击剑+2";
                            yltItem.Desc = "【STR+200】\\n\u3000一把对学习基础剑术至关重要的训练用剑。";
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
            fs.WriteUint(xseedItem.Id);
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

    public static List<ItTextDatItem> Parse(string file, Encoding encoding)
    {
        var itemList = new List<ItTextDatItem>();
        using var fs = new FileStream(file, FileMode.Open);
        for (var i = 0; i < 0xffff; i++)
        {
            if (itemList.Count > 0)
                if (fs.Position == itemList[0].Entry)
                    break;
            var item = new ItTextDatItem
            {
                Entry = fs.ReadUshort()
            };

            itemList.Add(item);
            var pos = fs.Position;
            fs.Seek(item.Entry, SeekOrigin.Begin);

            item.Id = fs.ReadUint();
            item.NameOffset = fs.ReadUshort();
            item.DescOffset = fs.ReadUshort();
            fs.Seek(item.NameOffset, SeekOrigin.Begin);
            var ascii = fs.ReadAsciiBytes();
            item.Name = encoding.GetString(ascii);
            fs.Seek(item.DescOffset, SeekOrigin.Begin);
            ascii = fs.ReadAsciiBytes();
            item.Desc = encoding.GetString(ascii);

            fs.Seek(pos, SeekOrigin.Begin);
        }

        return itemList;
    }
}