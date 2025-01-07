using System.Reflection.PortableExecutable;
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
    public Dictionary<uint, uint> FontAddrs = [];
    public uint Cmp48hAddr;
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
        ConvertPattern(Pattern, ref PatternArray);
    }
    private static void ConvertPattern(string[] p,ref  short[][] pArray)
    {
        for (int i = 0; i < p.Length; i++)
        {
            string pattern = p[i].Trim();
            var hex = pattern.Split(" ");
            pArray[i] = new short[hex.Length];
            for (int j = 0; j < hex.Length; j++)
            {
                pArray[i][j] = hex[j] == "??" ? (short)-1 : Convert.ToInt16(hex[j], 16);
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
    internal bool MatchAddr (short[] pattern,out int offset)
    {
        var matchLen = 0;
        while (Position < Length - 10) 
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
                offset = (int)Position;
                return true;
            }
            matchLen = 0;
        }

        offset = 0;
        return false;
    }
    internal void MatchCmp48hAddrAsm() 
    {
        string[] FontPattern =
        [
        "83 3C BD F8 4F 5F 00 48 7C",//192
        ];
        short[][] FontPatternArray = new short[FontPattern.Length][];
        ConvertPattern(FontPattern, ref FontPatternArray);
        var pos = PeEnd;
        base.Seek(pos, SeekOrigin.Begin);
        if (!MatchAddr(FontPatternArray[0], out int offset))
        {
            return;
        }
        offset--;
        Cmp48hAddr = Calc_vAddr(offset);
    }

    internal void MatchFontAddrAsm()
    {
        string[] FontPattern =
        [
        "81 FF C0 00 00 00 75 20 FF B4 24 20 01 00 00 A1",//192
        ];
        short[][] FontPatternArray = new short[FontPattern.Length][];
        ConvertPattern(FontPattern, ref FontPatternArray);
        if (!MatchAddr(FontPatternArray[0], out int offset))
        {
            return;
        }
        FontAddrs[192] = ReadUint();
        FontAddrs[160] = FontAddrs[192] - 4;
        FontAddrs[144] = FontAddrs[160] - 4;
        FontAddrs[128] = FontAddrs[144] - 4;
        FontAddrs[96] = FontAddrs[128] - 4;
        FontAddrs[80] = FontAddrs[96] - 4;
        FontAddrs[72] = FontAddrs[80] - 4;
        FontAddrs[64] = FontAddrs[72] - 4;
        FontAddrs[60] = FontAddrs[64] - 4;
        FontAddrs[54] = FontAddrs[60] - 4;
        FontAddrs[50] = FontAddrs[54] - 4;
        FontAddrs[48] = FontAddrs[50] - 4;
        FontAddrs[44] = FontAddrs[48] - 4;
        FontAddrs[40] = FontAddrs[44] - 4;
        FontAddrs[36] = FontAddrs[40] - 4;
        FontAddrs[30] = FontAddrs[36] - 4;  //
        FontAddrs[26] = FontAddrs[30] - 4;  //
        FontAddrs[18] = FontAddrs[26] - 4;  //
        FontAddrs[32] = FontAddrs[18] - 4;  //
        FontAddrs[24] = FontAddrs[32] - 4;  //
        FontAddrs[20] = FontAddrs[24] - 4;
        FontAddrs[16] = FontAddrs[20] - 4;  //
        FontAddrs[12] = FontAddrs[16] - 4;
        FontAddrs[8] = FontAddrs[12] - 4;
    }

    internal void MatchTextAddrAsm()
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

    public uint Calc_vAddr(long foa)
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