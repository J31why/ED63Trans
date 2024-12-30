using System.Text;
namespace DatTrans.Dats;

public class MonsterDatItem
{
    public ushort Id;
    public List<long> TextsOffset = [];
    public List<string> Texts = [];
}

[Obsolete]
public static class MonsterDat
{
    public static MonsterDatItem GetTexts(string fileName, Encoding encoding)
    {
        using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        var pos = fs.Length - 1;
        var zeroCount = 0;
        var textData = new List<byte>(0xff);
        var ms = new MonsterDatItem();
        ms.Id = fs.ReadUshort();
        while (zeroCount < 3)
        {
            fs.Position = pos;
            var b = (byte)fs.ReadByte();
            if (b > 0)
            {
                textData.Add(b);
                pos--;
                continue;
            }

            if (textData.Count > 0 || ms.Texts.Count < 1)
            {
                switch (zeroCount)
                {
                    case 1:
                        textData.Reverse();
                        ms.TextsOffset.Add(fs.Position);
                        ms.Texts.Add(encoding.GetString(textData.ToArray()));
                        break;
                    case 2:
                        textData.Reverse();
                        ms.TextsOffset.Add(fs.Position);
                        ms.Texts.Add(encoding.GetString(textData.ToArray()));
                        break;
                }
            }
            zeroCount++;
            textData.Clear();
            pos--;
        }

        ms.Texts.Reverse();
        ms.TextsOffset.Reverse();
        return ms;
    }
}