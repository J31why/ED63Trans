#region

using System.Text;
using static DatTrans.TransChar;

#endregion

namespace DatTrans.Dats;

public class QuestDatItem
{
    public ushort Entry;
    public ushort Id;
    public byte[] Data = [];
    public ushort[] TextEntries = new ushort[18];
    public string[] Texts = new string[18];
}

public static class QuestDat
{
    public static void Generate(string file, List<QuestDatItem> xseedList, List<QuestDatItem> yltList)
    {
        file = Path.GetFileName(file);
        using var fs = new FileStream(file, FileMode.OpenOrCreate);
        var buffer = new byte[xseedList.Count * 2]; //entrys
        fs.Write(buffer);
        for (var i = 0; i < xseedList.Count; i++)
        {
            var xseedItem = xseedList[i];
            var yltItem = yltList.Find(x => x.Id == xseedItem.Id);
            xseedItem.Entry = (ushort)fs.Position;
            //id
            fs.WriteUshort(xseedItem.Id);
            //unknown data
            fs.Write(xseedItem.Data);
            var entry = fs.Position;
            fs.Write(new byte[xseedItem.TextEntries.Length * 2]);

            if (yltItem != null)
            {
                for (int j = 0; j < 18; j++)
                {
                    xseedItem.TextEntries[j] = (ushort)fs.Position;
                    var replacedText = ReplaceClmChars(yltItem.Texts[j]);
                    var bytes = SjisEncoding.GetBytes(replacedText);
                    fs.Write(bytes);
                    fs.WriteByte(0);
                }
            }
            else
            {
                if (xseedItem.Id == 65)            // id: 65  xseed多一个
                {
                    for (int j = 0; j < 18; j++)
                    {
                        var text = xseedItem.Texts[j];
                        switch (j)
                        {
                            //翻译
                            case 0:
                                text = "太阳\u2464";
                                break;
                            case 2:
                                text = "轨迹之PONG／终极";
                                break;
                            case 3:
                                text = "黑色方舟\u3000前部第１层";
                                break;
                            case 4:
                                text = "\u3000\u0001 七曜之光，驱散黑暗……\u0001  其或将成为迷惘时\u0001 指示正确方向之路标。\u0001\u3000\u0001  蕴藏七耀光辉之神秘宝珠，\u0001  请将其全数展示于吾面前。\u0001\u3000如此，则『门』将开启……";
                                break;
                            case 5:
                                text = "全部５级结晶回路";
                                break;
                            case 6:
                                text = "关于『空之轨迹』的一切？\u0001问答小游戏，终极篇。";
                                break;
                            case 7:
                                text = "刻耀珠（结晶回路）";
                                break;
                            case 8:
                                text = "太阳之门\u2464";
                                break;
                        }
                        xseedItem.TextEntries[j] = (ushort)fs.Position;
                        var replacedText = ReplaceClmChars(text);
                        var bytes = SjisEncoding.GetBytes(replacedText);
                        fs.Write(bytes);
                        fs.WriteByte(0);
                    }

                    Console.WriteLine("Id:65 已处理");
                }

            }
            var endPos = fs.Position;
            fs.Seek(entry, SeekOrigin.Begin);
            for (int j = 0; j < 18; j++)
            {
                fs.WriteUshort(xseedItem.TextEntries[j]);
            }
            fs.Seek(endPos, SeekOrigin.Begin);
        }
        //entry
        fs.Seek(0, SeekOrigin.Begin);
        foreach (var xseedChar in xseedList) fs.WriteUshort(xseedChar.Entry);

        fs.Flush();
    }

    public static List<QuestDatItem> Parse(string file, Encoding encoding)
    {
        var itemList = new List<QuestDatItem>();
        using var fs = new FileStream(file, FileMode.Open);
        for (var i = 0; i < 0xffff; i++)
        {
            if (itemList.Count > 0)
                if (fs.Position == itemList[0].Entry)
                    break;
            var item = new QuestDatItem
            {
                Entry = fs.ReadUshort()
            };

            itemList.Add(item);
            var pos = fs.Position;
            fs.Seek(item.Entry, SeekOrigin.Begin);
            item.Id = fs.ReadUshort();
            item.Data = fs.ReadBytes(0x10);
            for (var j = 0; j < 18; j++)
                item.TextEntries[j] = fs.ReadUshort();

            for (var j = 0; j < 18; j++)
            {
                fs.Seek(item.TextEntries[j], SeekOrigin.Begin);
                var ascii = fs.ReadAsciiBytes();
                item.Texts[j] = encoding.GetString(ascii);
            }

            fs.Seek(pos, SeekOrigin.Begin);
        }

        return itemList;
    }
}