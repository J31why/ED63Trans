#region

using System.Text;
using static DatTrans.TransChar;

#endregion

namespace DatTrans.Dats;

public class TownDtItem
{
    public ushort Entry;
    public string Name = "";
}

public static class TownDat
{
    public static void Generate(string file, List<TownDtItem> xseedList, List<TownDtItem> yltList)
    {
        file = Path.GetFileName(file);
        using var fs = new FileStream(file, FileMode.OpenOrCreate);
        fs.WriteUshort(0x1C2); //head
        var buffer = new byte[xseedList.Count * 2]; //entrys
        fs.Write(buffer);
        for (var i = 0; i < xseedList.Count; i++)
        {
            var xseedTown = xseedList[i];
            var yltTown = yltList[i];
            xseedTown.Entry = (ushort)fs.Position;
            //name
            var replacedText = yltTown.Name;
             switch (xseedTown.Name)
             {
                 case "Core Sector - Center":
                     replacedText = "根源区域\u3000中心";
                      break;
                 case "Core Sector-Altered Space":
                     replacedText = "根源区域\u3000异空间";
                     break;
                 case "Academy - Back Road":
                     replacedText = "王立学院\u3000小道";
                     break;
                 case "Academy - Boys' Dormitory":
                     replacedText = "王立学院\u3000男子宿舍";
                     break;
                 case "Academy - Girls' Dormitory":
                     replacedText = "王立学院\u3000女子宿舍";
                     break;
                 case "Humanities Classroom":
                     replacedText = "主楼\u3000人文系教室";
                     break;
                 case "Faculty Lounge":
                     replacedText = "主楼\u3000职员室";
                     break;
                 case "Student Council Room":
                     replacedText = "社团大楼\u3000学生会室";
                     break;
                 case "Material Archives":
                     replacedText = "社团大楼\u3000资料室";
                     break;
                 case "Boys' Locker Room":
                     replacedText = "社团大楼\u3000男子衣帽间";
                     break;
                 case "Social Studies Classroom":
                     replacedText = "主楼\u3000社会系教室";
                     break;
                 case "Natural Sciences Classroom":
                     replacedText = "主楼\u3000自然系教室";
                     break;
                 case "Girls' Locker Room":
                     replacedText = "社团大楼\u3000女子衣帽间";
                     break;
                 case "Monochrome Schoolhouse":
                     replacedText = "无色的学舍";
                     break;
                 case "Academy - Main Building":
                     replacedText = "王立学院\u3000主楼";
                     break;
                 case "Academy - Clubhouse":
                     replacedText = "王立学院\u3000社团大楼";
                     break;
                 case "Academy - Dean's Office":
                     replacedText = "王立学院\u3000校长室";
                     break;
                 case "Academy - Auditorium":
                     replacedText = "王立学院\u3000礼堂";
                     break;
                 case "Academy - Old Schoolhouse":
                     replacedText = "王立学院\u3000旧校舍";
                     break;
                 case "Moon Door":
                     replacedText = "月之门";
                     break;
                 case "Star Door":
                     replacedText = "星之门";
                     break;
                 case "Martial Arts Competition":
                     replacedText = "武术大会";
                     break;
             }
            replacedText = ReplaceClmChars(replacedText);
            var bytes = SjisEncoding.GetBytes(replacedText);
            if (bytes.Length > 0)
            {
                fs.Write(bytes);
                fs.WriteByte(0);
            }
            fs.WriteByte(0);
        }

        //entry
        fs.Seek(2, SeekOrigin.Begin);
        foreach (var xseedChar in xseedList) fs.WriteUshort(xseedChar.Entry);

        fs.Flush();
    }

    public static List<TownDtItem> Parse(string file, Encoding encoding)
    {
        var itemList = new List<TownDtItem>();
        using var fs = new FileStream(file, FileMode.Open);
        fs.ReadUshort(); // head 0xC2 01
        for (var i = 0; i < 0xffff; i++)
        {
            if (itemList.Count > 0)
                if (fs.Position == itemList[0].Entry)
                    break;
            var town = new TownDtItem
            {
                Entry = fs.ReadUshort()
            };
            itemList.Add(town);
            var pos = fs.Position;
            fs.Seek(town.Entry, SeekOrigin.Begin);
            var ascii = fs.ReadAsciiBytes();
            town.Name = encoding.GetString(ascii);
            fs.Seek(pos, SeekOrigin.Begin);
        }

        return itemList;
    }
}