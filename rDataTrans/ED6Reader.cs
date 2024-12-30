using System.Text;
using System.Text.RegularExpressions;
using static rDataTrans.Mem;

namespace rDataTrans;

internal class AsmMatch
{
    public int Offset;
    public uint TextAddr;
}

internal class Section
{
    public string Name = "";
    public uint vSize = 0, vAddr = 0, rSize = 0, rAddr = 0;

    public override string ToString()
    {
        return $"Name: {Name}\nVirtual Address: {vAddr:X}\nVirtual Size: {vSize:X}\nRawData Address: {rAddr:X}\nRawData Size: {rSize:X}";
    }
}

internal class ED6Reader : MemoryStream
{
    public List<AsmMatch> AsmMatches = [];
    public Section[] Sections = [];
    public int PeEnd = 0;

    public static readonly string[] Pattern =
        [
        "c7 04 24 ?? ?? ?? 00",
        "c7 44 24 ?? ?? ?? ?? 00",
        "00 ?? ?? ?? 00",
        "ba ?? ?? ?? 00",
        "be ?? ?? ?? 00",
        "bf ?? ?? ?? 00",
        "b8 ?? ?? ?? 00",
        "b9 ?? ?? ?? 00",
        "a1 ?? ?? ?? 00",
        "68 ?? ?? ?? 00",
        "0f 10 05 ?? ?? ?? 00",
        ];

    public static short[][] PatternArray = new short[Pattern.Length][];

    static ED6Reader()
    {
        for (int i = 0; i < Pattern.Length; i++)
        {
            string pattern = Pattern[i].Trim();
            var hex = pattern.Split(" ");
            PatternArray[i] = new short[hex.Length];
            for (int j = 0; j < hex.Length; j++)
            {
                PatternArray[i][j] = hex[j] == "??" ? (short)-1 : Convert.ToInt16(hex[j], 16);
            }
        }
    }

    public ED6Reader(byte[] data) : base(data)
    {
    }

    internal bool MatchPattern(out int offset)
    {
        var pos = Position;
        var matchLen = 0;
        foreach (var pattern in PatternArray)
        {
            for (int i = 0; i < pattern.Length; i++)
            {
                var b = ReadByte();
                if (b == pattern[i] || pattern[i] == -1)
                {
                    matchLen++;
                    continue;
                }
                break;
            }
            if (matchLen == pattern.Length)
            {
                offset = (int)Position - 4;
                return true;
            }
            matchLen = 0;
            Position = pos;
        }
        offset = 0;
        return false;
    }

    internal void MatchAsm()
    {
        AsmMatches.Clear();
        var pos = PeEnd;
        while (pos < Length - 10)
        {
            base.Seek(pos, SeekOrigin.Begin);

            if (!MatchPattern(out var offset))
            {
                pos++;
                continue;
            }
            Position = offset;
            var addr = ReadUint();
            if (addr < 0x400)
            {
                pos++;
                continue;
            }
            var match = new AsmMatch
            {
                Offset = offset,
                TextAddr = addr
            };
            AsmMatches.Add(match);
            pos++;
        }
    }

    internal void ParsePE()
    {
        Seek(0x3c, SeekOrigin.Begin);
        var pe_entry = ReadUint();
        Seek(pe_entry + 0x6, SeekOrigin.Begin);
        var numberOfSections = ReadUshort();
        Sections = new Section[numberOfSections];
        //optional header
        Seek(0xc, SeekOrigin.Current);
        var sizeOfOptionalHeader = ReadUshort();
        Seek(0x2, SeekOrigin.Current);//Characteristics
        Seek(sizeOfOptionalHeader, SeekOrigin.Current);

        for (int i = 0; i < numberOfSections; i++)
        {
            var buffer = new byte[8];
            ReadExactly(buffer);
            Sections[i] = new Section
            {
                Name = Encoding.ASCII.GetString(buffer).Trim('\0'),
                vSize = ReadUint(),
                vAddr = ReadUint(),
                rSize = ReadUint(),
                rAddr = ReadUint()
            };
            Seek(0x10, SeekOrigin.Current); // other
        }
        PeEnd = (int)Position;
    }

    internal Dictionary<string, rdataString> ParseString()
    {
        var dic = new Dictionary<string, rdataString>();
        var rdata = Sections.First(x => x.Name == ".rdata");
        base.Seek(rdata.rAddr, SeekOrigin.Begin);
        while (Position < Length)
        {
            var arr = new List<byte>(0xff);
            var pos = Position;
            byte b;
            while ((b = (byte)ReadByte()) != 0)
            {
                //过滤某种垃圾数据
                if (arr.Count == 2 && arr[1] == 0xCB && b == 0x3F)
                {
                    break;
                }
                arr.Add(b);
            }

            if (arr.Count <= 1)
            {
                continue;
            }
            var text = ReplaceFactory.SjisEncoding.GetString(arr.ToArray());
            if (string.IsNullOrWhiteSpace(text) || Regex.IsMatch(text, "\\?\\?\\?\\?")) continue;
            var dataString = new rdataString
            {
                offset = pos,
                length = arr.Count,
                str = text
            };
            dic[text] = dataString;
        }

        return dic;
    }

    public uint Clac_vAddr(long foa)
    {
        foreach (Section sec in Sections)
        {
            if (foa >= sec.rAddr + sec.rSize)
                continue;
            return (uint)(foa + 0x400000 + sec.vAddr - sec.rAddr);
        }
        return 0;
    }

    public ushort ReadUshort()
    {
        var a = ReadByte();
        a += ReadByte() << 8;
        return (ushort)a;
    }

    public uint ReadUint()
    {
        var a = ReadByte();
        a += ReadByte() << 8;
        a += ReadByte() << 0x10;
        a += ReadByte() << 0x18;
        return (uint)a;
    }
}