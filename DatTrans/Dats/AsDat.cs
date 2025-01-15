using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;

namespace DatTrans.Dats;

public class AsDatData
{
    public ushort Pos;
    public string Name = "";
    public string Loc = "";
}

public class AsDatBytes
{
    public ushort Pos;
    public byte[] Bytes = [];
}

public class AsDatAsm
{
    public byte OpCode = 0;
    public ushort Pos;
    public string LocName="";
    public string OpName="";
    public string Para="";
}

public static class AsDat
{
    static JsonSerializerOptions options = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };
    static byte GetAsmOpCodeByName(string name)
    {
        var txt =
            """
            @End [00]
            @Goto [01]
            @SubChip [02]
            @Sleep [06]
            @Update [07]
            @Teleport [08]
            @Turn [0B]
            @Jump [0D]
            @DropDown [0E]
            @JumpToTarget [0F]
            @JumpBack [10]
            @Move [11]
            @AddEff [12]
            @ReleaseEff [13]
            @WaitEff [15]
            @FinishEff [16]
            @CancelEff [17]
            @ShowEff [18]
            @Show3DEff [19]
            @SelectChip [1B]
            @Damage [1C]
            @DamageAnime [1D]
            @BeginThread [22]
            @WaitThread [23]
            @SetChipModeFlag [24]
            @ClearChipModeFlag [25]
            @Say [28]
            @TipText [2A]
            @ShadowBegin [2C]
            @ShadowEnd [2D]
            @ShakeChar [2E]
            @SuspendThread [2F]
            @KeepAngle [35]
            @RotationAngle [37]
            @RotationAngleVert [38]
            @SetAngle [39]
            @TiltAngle [3A]
            @RotationAngleHorz [3B]
            @ShakeScreen [3D]
            @LockAngle [41]
            @SetBkColor [43]
            @ZoomAngle [44]
            @Random [4B]
            @LoopTargetBegin [4C]
            @ResetLoopTarget [4D]
            @LoopTargetEnd [4E]
            @Call [50]
            @Ret [51]
            @MagicCastBegin [55]
            @MagicCastEnd [56]
            @BeatBack [58]
            @Show [5C]
            @Hide [5D]
            @SetBattleSpeed [61]
            @SE [64]
            @SeEx [65]
            @ScraftCutIn [67]
            @Nop [68]
            @ReleaseTexture [69]
            @LoadSChip [6A]
            @ResetSCraftChip [6B]
            @Die [6C]
            @CraftEnd [7A]
            @CraftEndFlag [7B]
            @Blur [7F]
            @SortTarget [83]
            @RotateChar [84]
            @Voice [88]
            @SaveCurPos [89]
            @Clone [8A]
            @UseItemBegin [8B]
            @UseItemEnd [8C]
            @LoadXFile [8E]
            @SetAngleTarget [96]
            @MoveAngle [97]
            @ResetChipStatus [9C]
            @SetBattleMode [9F]
            @BattleEffEnd [A7]
            @DamageVoice [A8]
            @OP_0 [00]
            @OP_1 [01]
            @OP_2 [02]
            @OP_3 [03]
            @OP_4 [04]
            @OP_5 [05]
            @OP_6 [06]
            @OP_7 [07]
            @OP_8 [08]
            @OP_9 [09]
            @OP_A [0A]
            @OP_B [0B]
            @OP_C [0C]
            @OP_D [0D]
            @OP_E [0E]
            @OP_F [0F]
            @OP_10 [10]
            @OP_11 [11]
            @OP_12 [12]
            @OP_13 [13]
            @OP_14 [14]
            @OP_15 [15]
            @OP_16 [16]
            @OP_17 [17]
            @OP_18 [18]
            @OP_19 [19]
            @OP_1A [1A]
            @OP_1B [1B]
            @OP_1C [1C]
            @OP_1D [1D]
            @OP_1E [1E]
            @OP_1F [1F]
            @OP_20 [20]
            @OP_21 [21]
            @OP_22 [22]
            @OP_23 [23]
            @OP_24 [24]
            @OP_25 [25]
            @OP_26 [26]
            @OP_27 [27]
            @OP_28 [28]
            @OP_29 [29]
            @OP_2A [2A]
            @OP_2B [2B]
            @OP_2C [2C]
            @OP_2D [2D]
            @OP_2E [2E]
            @OP_2F [2F]
            @OP_30 [30]
            @OP_31 [31]
            @OP_32 [32]
            @OP_33 [33]
            @OP_34 [34]
            @OP_35 [35]
            @OP_36 [36]
            @OP_37 [37]
            @OP_38 [38]
            @OP_39 [39]
            @OP_3A [3A]
            @OP_3B [3B]
            @OP_3C [3C]
            @OP_3D [3D]
            @OP_3E [3E]
            @OP_3F [3F]
            @OP_40 [40]
            @OP_41 [41]
            @OP_42 [42]
            @OP_43 [43]
            @OP_44 [44]
            @OP_45 [45]
            @OP_46 [46]
            @OP_47 [47]
            @OP_48 [48]
            @OP_49 [49]
            @OP_4A [4A]
            @OP_4B [4B]
            @OP_4C [4C]
            @OP_4D [4D]
            @OP_4E [4E]
            @OP_4F [4F]
            @OP_50 [50]
            @OP_51 [51]
            @OP_52 [52]
            @OP_53 [53]
            @OP_54 [54]
            @OP_55 [55]
            @OP_56 [56]
            @OP_57 [57]
            @OP_58 [58]
            @OP_59 [59]
            @OP_5A [5A]
            @OP_5B [5B]
            @OP_5C [5C]
            @OP_5D [5D]
            @OP_5E [5E]
            @OP_5F [5F]
            @OP_60 [60]
            @OP_61 [61]
            @OP_62 [62]
            @OP_63 [63]
            @OP_64 [64]
            @OP_65 [65]
            @OP_66 [66]
            @OP_67 [67]
            @OP_68 [68]
            @OP_69 [69]
            @OP_6A [6A]
            @OP_6B [6B]
            @OP_6C [6C]
            @OP_6D [6D]
            @OP_6E [6E]
            @OP_6F [6F]
            @OP_70 [70]
            @OP_71 [71]
            @OP_72 [72]
            @OP_73 [73]
            @OP_74 [74]
            @OP_75 [75]
            @OP_76 [76]
            @OP_77 [77]
            @OP_78 [78]
            @OP_79 [79]
            @OP_7A [7A]
            @OP_7B [7B]
            @OP_7C [7C]
            @OP_7D [7D]
            @OP_7E [7E]
            @OP_7F [7F]
            @OP_80 [80]
            @OP_81 [81]
            @OP_82 [82]
            @OP_83 [83]
            @OP_84 [84]
            @OP_85 [85]
            @OP_86 [86]
            @OP_87 [87]
            @OP_88 [88]
            @OP_89 [89]
            @OP_8A [8A]
            @OP_8B [8B]
            @OP_8C [8C]
            @OP_8D [8D]
            @OP_8E [8E]
            @OP_8F [8F]
            @OP_90 [90]
            @OP_91 [91]
            @OP_92 [92]
            @OP_93 [93]
            @OP_94 [94]
            @OP_95 [95]
            @OP_96 [96]
            @OP_97 [97]
            @OP_98 [98]
            @OP_99 [99]
            @OP_9A [9A]
            @OP_9B [9B]
            @OP_9C [9C]
            @OP_9D [9D]
            @OP_9E [9E]
            @OP_9F [9F]
            @OP_A0 [A0]
            @OP_A1 [A1]
            @OP_A2 [A2]
            @OP_A3 [A3]
            @OP_A4 [A4]
            @OP_A5 [A5]
            @OP_A6 [A6]
            @OP_A7 [A7]
            @OP_A8 [A8]
            @OP_A9 [A9]
            @OP_AA [AA]
            @OP_AB [AB]
            @OP_AC [AC]
            @OP_AD [AD]
            @OP_AE [AE]
            @OP_AF [AF]
            @OP_B0 [B0]
            @OP_B1 [B1]
            @OP_B2 [B2]
            """;
        return Convert.ToByte(Regex.Match(txt, $"{name} \\[(.*?)\\]").Groups[1].Value,16);
    }
    static AsDatData ParseData(string line)
    {
        var prop =  line.Split("=");
        return new AsDatData
        {
            Name=prop[0],
            Loc=prop[1],
        };
    }
    static AsDatBytes ParseBytes(string line)
    {
        var bytes = new AsDatBytes();
        var hex = Regex.Match(line, "bytes\\(\"(.*?)\"\\)").Groups[1].Value.Replace(" ","");
        bytes.Bytes = Convert.FromHexString(hex);
        return bytes;
    }

    static AsDatAsm? ParseAsm(string line)
    {
        if (string.IsNullOrEmpty(line.Trim()))
            return null;
        var asm = new AsDatAsm();
        var m = Regex.Match(line, "##(.*?)\t(.*?)\\((.*?)\\);");
        asm.LocName = m.Groups[1].Value;
        asm.OpName = m.Groups[2].Value;
        asm.Para = m.Groups[3].Value;
        asm.OpCode = GetAsmOpCodeByName(asm.OpName);
        return asm;
    }

    public static void Generate(string file,string asScript)
    {

        var lines = asScript.Split('\n');
        var asDat = new ArrayList(lines.Length);
        var asDatRedirect = new ArrayList();
        var encoding = Encoding.GetEncoding(int.Parse(lines[1].Replace("#CP=", "")));
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        var locDir = new Dictionary<string, ushort>();
        //head
        asDat.Add(ParseData(lines[3]));//CraftOffsetTable
        asDat.Add(ParseData(lines[4]));//CraftOffsetTableEnd
        asDat.Add(ParseBytes(lines[5]));
        var currentLine = 7;
        for (; currentLine < lines.Length; currentLine++)
        {
          var line = lines[currentLine];
          if (line is "#CraftOffsetTableEnd")
              break;
          asDat.Add(ParseData(line));
        }
        currentLine++;
        asDat.Add(ParseBytes(lines[currentLine]));
        currentLine++;
        //asm
        for (; currentLine < lines.Length; currentLine++)
        {
            var line = lines[currentLine];
            var asm = ParseAsm(line);
            if (asm != null)
                asDat.Add(asm);
        }
        //write temp
        foreach (var dat in asDat)
        {
            if (dat is AsDatData data)
            {
                data.Pos = (ushort)ms.Position;
                writer.Write(Convert.ToUInt16(data.Loc.Replace("Loc_",""),16));
                asDatRedirect.Add(data);
            }
            else if (dat is AsDatBytes bytes)
            {
                bytes.Pos = (ushort)ms.Position;
                writer.Write(bytes.Bytes);
            }
            else if (dat is AsDatAsm asm)
            {
                asm.Pos = (ushort)ms.Position;
                if (locDir.ContainsKey(asm.LocName))
                {

                }
                locDir[asm.LocName] = asm.Pos;
                writer.Write(asm.OpCode);
                if (string.IsNullOrEmpty(asm.Para))
                    continue;
                var paras = asm.Para.Split(",");
                foreach (var para in paras)
                {
                    if (para.EndsWith(":b"))
                    {
                        var b = Convert.ToByte(para.Replace(":b", ""), 16);
                        writer.Write(b);
                    }
                    else if (para.EndsWith(":s"))
                    {
                        var s = Convert.ToInt16(para.Replace(":s", ""), 16);
                        writer.Write(s);
                    }
                    else if (para.EndsWith(":i"))
                    {
                        var i = Convert.ToInt32(para.Replace(":i", ""), 16);
                        writer.Write(i);
                    }
                    else if (para.EndsWith(":sp"))
                    {
                        var sp = Convert.ToInt16(para.Replace(":sp", "").Replace("Loc_",""), 16);
                        writer.Write(sp);
                        asDatRedirect.Add(asm);
                    }
                    else if (para.StartsWith('\"')&& para.EndsWith('\"'))
                    {
                        var str = JsonSerializer.Deserialize<string>(para,options)!;
                        str = TransChar.ReplaceClmChars(str);
                        var ascii = encoding.GetBytes(str);
                        writer.Write(ascii);
                        writer.Write((byte)0);
                    }
                }
            }
        }
        
        //redirect
         foreach (var dat in asDatRedirect)
        {
            if (dat is AsDatData data)
            {
                if (data.Name is "CraftOffsetTable" or "CraftOffsetTableEnd")
                    continue;
                ms.Position = data.Pos;
                writer.Write(locDir[data.Loc]);
            }
            else if (dat is AsDatAsm asm)
            {
                ms.Position = asm.Pos;
                writer.Write(asm.OpCode);
                if (string.IsNullOrEmpty(asm.Para))
                    continue;
                var paras = asm.Para.Split(",");
                foreach (var para in paras)
                {
                    if (para.EndsWith(":b"))
                    {
                        ms.Seek(1, SeekOrigin.Current);
                    }
                    else if (para.EndsWith(":s"))
                    {
                        ms.Seek(2, SeekOrigin.Current);
                    }
                    else if (para.EndsWith(":i"))
                    {
                        ms.Seek(4, SeekOrigin.Current);
                    }
                    else if (para.EndsWith(":sp"))
                    {
                        var loc = para.Replace(":sp", "");
                        if (locDir.TryGetValue(loc,out var locValue))
                        {
                            writer.Write(locValue);
                        }
                        else if(loc.Replace("Loc_","").Length<=1)
                        {
                            writer.Write(Convert.ToUInt16(loc.Replace("Loc_", ""), 16));
                        }
                    }
                    else if (para.StartsWith('\"')&& para.EndsWith('\"'))
                    {
                        var str = JsonSerializer.Deserialize<string>(para,options)!;
                        str = TransChar.ReplaceClmChars(str);
                        var ascii = encoding.GetBytes(str);
                        ms.Seek(ascii.Length + 1, SeekOrigin.Current);
                    }
                }
            }
        }
        writer.Flush();
        File.WriteAllBytes(file, ms.ToArray());
    }
    public static string ParseDt(string file, Encoding encoding)
    {
        var text = $"#FileName={Path.GetFileName(file)}\n#CP={encoding.CodePage}\n\n";
        using var fs = new FileStream(file, FileMode.Open);
        using var reader = new BinaryReader(fs, encoding);
        var bytes = new List<byte>(0xffff);
        //head
        text += ParseHead(fs, reader)+$"\n";
        byte b = 0;
        byte[] ascii = [];
        while (fs.Position < fs.Length)
        {
            text += $"##Loc_{fs.Position:X}\t";
            b = reader.ReadByte();
            switch (b)
            {
                case 0x0:
                    text += $"End({ParseParas(reader, "", encoding)});\n\n";
                    break;
                case 0x1:
                    text += $"Goto({ParseParas(reader, "sp", encoding)});\n";
                    break;
                case 0x2:
                    text += $"SubChip({ParseParas(reader, "b,b", encoding)});\n";
                    break;
                case 0x3:
                    text += $"OP_3({ParseParas(reader, "b,s", encoding)});\n";
                    break;
                case 0x4:
                    text += $"OP_4({ParseParas(reader, "b,b,s", encoding)});\n";
                    break;
                case 0x5:
                    text += $"OP_5({ParseParas(reader, "b,b,i", encoding)});\n";
                    break;
                case 0x6:
                    text += $"Sleep({ParseParas(reader, "i", encoding)});\n";
                    break;
                case 0x7:
                    text += $"Update({ParseParas(reader, "", encoding)});\n";
                    break;
                case 0x8:
                    text += $"Teleport({ParseParas(reader, "b,b,i,i,i", encoding)});\n";
                    break;
                case 0x9:
                     b = reader.ReadByte();
                     b = reader.ReadByte();
                     fs.Position -= 2;
                     if (b == 0)
                     {
                         text += $"OP_9({ParseParas(reader, "b,b,i,i", encoding)});\n";
                     }
                     else
                     {
                         text += $"OP_9({ParseParas(reader, "b,b,i,i,i", encoding)});\n";
                     }
                    break;
                case 0xA:
                    text += $"OP_A({ParseParas(reader, "b,b,b,i", encoding)});\n";
                    break;
                case 0xB:
                    text += $"Turn({ParseParas(reader, "b,b,s", encoding)});\n";
                    break;
                case 0xC:
                    text += $"OP_C({ParseParas(reader, "b,b,s,s,b", encoding)});\n";
                    break;
                case 0xD:
                    text += $"Jump({ParseParas(reader, "b,b,i,i,i,s,s", encoding)});\n";
                    break;
                case 0xE:
                    text += $"DropDown({ParseParas(reader, "b,i,i,i,s,s", encoding)});\n";
                    break;
                case 0xF:
                    text += $"JumpToTarget({ParseParas(reader, "s,s", encoding)});\n";
                    break;
                case 0x10:
                    text += $"JumpBack({ParseParas(reader, "s,s", encoding)});\n";
                    break;
                case 0x11:
                    text += $"Move({ParseParas(reader, "b,b,i,i,i,i,b", encoding)});\n";
                    break;
                case 0x12:
                    text += $"AddEff({ParseParas(reader, "s,str", encoding)});\n";
                    break;
                case 0x13:
                    text += $"ReleaseEff({ParseParas(reader, "s", encoding)});\n";
                    break;
                case 0x14:
                    text += $"OP_14({ParseParas(reader, "s", encoding)});\n";
                    break;
                case 0x15:
                    text += $"WaitEff({ParseParas(reader, "b,b", encoding)});\n";
                    break;
                case 0x16:
                    text += $"FinishEff({ParseParas(reader, "b,b", encoding)});\n";
                    break;
                case 0x17:
                    text += $"CancelEff({ParseParas(reader, "b,b", encoding)});\n";
                    break;
                case 0x18:
                    text += $"ShowEff({ParseParas(reader, "b,b,b,s,i,i,i,s,s,s,s,s,s,b", encoding)});\n";
                    break;
                case 0x19:
                    text += $"Show3DEff({ParseParas(reader, "b,b,str,s,i,i,i,s,s,s,s,s,s,b", encoding)});\n";
                    break;
                case 0x1A:
                    text += $"OP_1A({ParseParas(reader, "b,s", encoding)});\n";
                    break;
                case 0x1b: 
                    text += $"SelectChip({ParseParas(reader, "b,b", encoding)});\n";
                    break;
                case 0x1c: 
                    text += $"Damage({ParseParas(reader, "b", encoding)});\n";
                    break;
                case 0x1d: 
                    text += $"DamageAnime({ParseParas(reader, "b,b,i", encoding)});\n";
                    break;
                case 0x1e: 
                    text += $"OP_1E({ParseParas(reader, "i", encoding)});\n";
                    break;
                case 0x1f: 
                    text += $"OP_1F({ParseParas(reader, "i", encoding)});\n";
                    break;
                case 0x20: 
                    text += $"OP_20({ParseParas(reader, "b,b,b,b,i,i", encoding)});\n";
                    break;
                case 0x21: 
                    text += $"OP_21({ParseParas(reader, "b,b,i,i", encoding)});\n";
                    break;
                case 0x22: 
                    text += $"BeginThread({ParseParas(reader, "b,b,sp,b", encoding)});\n";
                    break;
                case 0x23: 
                    text += $"WaitThread({ParseParas(reader, "b,b", encoding)});\n";
                    break;
                case 0x24: 
                    text += $"SetChipModeFlag({ParseParas(reader, "b,b,s", encoding)});\n";
                    break;
                case 0x25: 
                    text += $"ClearChipModeFlag({ParseParas(reader, "b,b,s", encoding)});\n";
                    break;
                case 0x26: 
                    text += $"OP_26({ParseParas(reader, "b,b,s", encoding)});\n";
                    break;
                case 0x27: 
                    text += $"OP_27({ParseParas(reader, "b,b,s", encoding)});\n";
                    break;
                case 0x28: 
                    text += $"Say({ParseParas(reader, "b,str,i", encoding)});\n";
                    break;
                case 0x29: 
                    text += $"OP_29({ParseParas(reader, "b", encoding)});\n";
                    break;
                case 0x2a: 
                    text += $"TipText({ParseParas(reader, "str,i", encoding)});\n";
                    break;
                case 0x2b: 
                    text += $"OP_2B({ParseParas(reader, "", encoding)});\n";
                    break;
                case 0x2c: 
                    text += $"ShadowBegin({ParseParas(reader, "b,s,s", encoding)});\n";
                    break;
                case 0x2d: 
                    text += $"ShadowEnd({ParseParas(reader, "b", encoding)});\n";
                    break;
                case 0x2e: 
                    text += $"ShakeChar({ParseParas(reader, "b,i,i,i", encoding)});\n";
                    break;
                case 0x2f: 
                    text += $"SuspendThread({ParseParas(reader, "b,b", encoding)});\n";
                    break;
                case 0x30:
                    b = reader.ReadByte();
                    text += $"OP_30(0x{b:X}:b";
                    for (int i = 0; i < b; i++)
                    {
                        ascii = fs.ReadAsciiBytes();
                        text  += $",{JsonSerializer.Serialize(encoding.GetString(ascii),options)}";
                        if (ascii.Length == 0)
                        {
                            break;
                        }
                    }
                    text += ");\n";
                    break;
                case 0x31: 
                    text += $"OP_31({ParseParas(reader, "b,i", encoding)});\n";
                    break;
                case 0x32: 
                    text += $"OP_32({ParseParas(reader, "b,b", encoding)});\n";
                    break;
                case 0x33: 
                    text += $"OP_33({ParseParas(reader, "b,b", encoding)});\n";
                    break;
                case 0x34: 
                    text += $"OP_34({ParseParas(reader, "", encoding)});\n";
                    break;
                case 0x35: 
                    text += $"KeepAngle({ParseParas(reader, "b,i,i,i,i", encoding)});\n";
                    break;
                case 0x36: 
                    text += $"OP_36({ParseParas(reader, "i,i,i,i", encoding)});\n";
                    break;
                case 0x37: 
                    text += $"RotationAngle({ParseParas(reader, "i,i,i,i", encoding)});\n";
                    break;
                case 0x38: 
                    text += $"RotationAngleVert({ParseParas(reader, "i,i,i,i", encoding)});\n";
                    break;
                case 0x39: 
                    text += $"SetAngle({ParseParas(reader, "i,i", encoding)});\n";
                    break;
                case 0x3a: 
                    text += $"TiltAngle({ParseParas(reader, "i,i", encoding)});\n";
                    break;
                case 0x3b: 
                    text += $"RotationAngleHorz({ParseParas(reader, "i,i", encoding)});\n";
                    break;
                case 0x3c: 
                    text += $"OP_3C({ParseParas(reader, "s,i", encoding)});\n";
                    break;
                case 0x3d: 
                    text += $"ShakeScreen({ParseParas(reader, "i,i,i,i", encoding)});\n";
                    break;
                case 0x3e: 
                    text += $"OP_3E({ParseParas(reader, "i,i", encoding)});\n";
                    break;
                case 0x3f: 
                    text += $"OP_3F({ParseParas(reader, "b", encoding)});\n";
                    break;
                case 0x40: 
                    text += $"OP_40({ParseParas(reader, "b", encoding)});\n";
                    break;
                case 0x41: 
                    text += $"LockAngle({ParseParas(reader, "b", encoding)});\n";
                    break;
                case 0x42: 
                    text += $"OP_42({ParseParas(reader, "b,i,b", encoding)});\n";
                    break;
                case 0x43: 
                    text += $"SetBkColor({ParseParas(reader, "b,i,i", encoding)});\n";
                    break;
                case 0x44: 
                    text += $"ZoomAngle({ParseParas(reader, "b,i,i", encoding)});\n";
                    break;
                case 0x45: 
                    text += $"OP_45({ParseParas(reader, "b,i", encoding)});\n";
                    break;
                case 0x46: 
                    text += $"OP_46({ParseParas(reader, "b,i,i", encoding)});\n";
                    break;
                case 0x47: 
                    text += $"OP_47({ParseParas(reader, "b", encoding)});\n";
                    break;
                case 0x48: 
                    text += $"OP_48({ParseParas(reader, "b,i", encoding)});\n";
                    break;
                case 0x49: 
                    text += $"OP_49({ParseParas(reader, "b,s", encoding)});\n";
                    break;
                case 0x4b: 
                    text += $"Random({ParseParas(reader, "b,b,i,sp", encoding)});\n";
                    break;
                case 0x4c:
                    text += $"LoopTargetBegin({ParseParas(reader, "sp", encoding)});\n";
                    break;
                case 0x4d:
                    text += $"ResetLoopTarget({ParseParas(reader, "", encoding)});\n";
                    break;
                case 0x4e:
                    text += $"LoopTargetEnd({ParseParas(reader, "", encoding)});\n";
                    break;
                case 0x4f: 
                    text += $"OP_4F({ParseParas(reader, "b,b", encoding)});\n";
                    break;
                case 0x50:
                    text += $"Call({ParseParas(reader, "sp", encoding)});\n";
                    break;
                case 0x51:
                    text += $"Ret({ParseParas(reader, "", encoding)});\n";
                    break;
                case 0x52:
                    text += $"OP_52({ParseParas(reader, "b", encoding)});\n";
                    break;
                case 0x53:
                    text += $"OP_53({ParseParas(reader, "b", encoding)});\n";
                    break;
                case 0x54:
                    text += $"OP_54({ParseParas(reader, "b", encoding)});\n";
                    break;
                case 0x55:
                    text += $"MagicCastBegin({ParseParas(reader, "s", encoding)});\n";
                    break;
                case 0x56:
                    text += $"MagicCastEnd({ParseParas(reader, "", encoding)});\n";
                    break;
                case 0x57:
                    text += $"OP_57({ParseParas(reader, "b,b", encoding)});\n";
                    break;
                case 0x58:
                    text += $"BeatBack({ParseParas(reader, "b", encoding)});\n";
                    break;
                case 0x5A:
                    text += $"OP_5A({ParseParas(reader, "b,b,i", encoding)});\n";
                    break;
                case 0x5B:
                    text += $"OP_5B({ParseParas(reader, "i", encoding)});\n";
                    break;
                case 0x5c:
                    text += $"Show({ParseParas(reader, "b,i", encoding)});\n";
                    break;
                case 0x5D:
                    text += $"Hide({ParseParas(reader, "b,i", encoding)});\n";
                    break;
                case 0x5e:
                    text += $"OP_5E({ParseParas(reader, "b", encoding)});\n";
                    break;
                case 0x5f:
                    text += $"OP_5F({ParseParas(reader, "b,b", encoding)});\n";
                    break;
                case 0x60:
                    text += $"OP_60({ParseParas(reader, "b", encoding)});\n";
                    break;
                case 0x61:
                    text += $"SetBattleSpeed({ParseParas(reader, "i", encoding)});\n";
                    break;
                case 0x62:
                    text += $"OP_62({ParseParas(reader, "b,b,b,b,s", encoding)});\n";
                    break;
                case 0x63:
                    text += $"OP_63({ParseParas(reader, "b,i", encoding)});\n";
                    break;
                case 0x64:
                    text += $"SE({ParseParas(reader, "s", encoding)});\n";
                    break;
                case 0x65:
                    text += $"SeEx({ParseParas(reader, "s,b", encoding)});\n";
                    break;
                case 0x66:
                    text += $"OP_66({ParseParas(reader, "s", encoding)});\n";
                    break;
                case 0x67:
                    text += $"ScraftCutIn({ParseParas(reader, "str", encoding)});\n";
                    break;
                case 0x69:
                    text += $"ReleaseTexture({ParseParas(reader, "", encoding)});\n";
                    break;
                case 0x6A:
                    text += $"LoadSChip({ParseParas(reader, "b,i,i", encoding)});\n";
                    break;
                case 0x6B:
                    text += $"ResetSCraftChip({ParseParas(reader, "", encoding)});\n";
                    break;
                case 0x6C:
                    text += $"Die({ParseParas(reader, "", encoding)});\n";
                    break;
                case 0x6D:
                    text += $"OP_6D({ParseParas(reader, "i", encoding)});\n";
                    break;
                case 0x6E:
                    text += $"OP_6E({ParseParas(reader, "i", encoding)});\n";
                    break;
                case 0x78:
                    text += $"OP_78({ParseParas(reader, "b", encoding)});\n";
                    break;
                case 0x79:
                    text += $"OP_79({ParseParas(reader, "b", encoding)});\n";
                    break;
                case 0x7a:
                    text += $"CraftEnd({ParseParas(reader, "b", encoding)});\n";
                    break;
                case 0x7b:
                    text += $"CraftEndFlag({ParseParas(reader, "s", encoding)});\n";
                    break;
                case 0x7c:
                    text += $"OP_7C({ParseParas(reader, "b,b", encoding)});\n";
                    break;
                case 0x7e:
                    text += $"OP_7E({ParseParas(reader, "i", encoding)});\n";
                    break;
                case 0x7f:
                    text += $"Blur({ParseParas(reader, "i,i,i,b,i", encoding)});\n";
                    break;
                case 0x80:
                    text += $"OP_80({ParseParas(reader, "i", encoding)});\n";
                    break;
                case 0x81:
                    text += $"OP_81({ParseParas(reader, "b,b,s", encoding)});\n";
                    break;
                case 0x82:
                    text += $"OP_82({ParseParas(reader, "", encoding)});\n";
                    break;
                case 0x83:
                    text += $"SortTarget({ParseParas(reader, "b", encoding)});\n";
                    break;
                case 0x84:
                    text += $"RotateChar({ParseParas(reader, "b,s,s,s,i,b", encoding)});\n";
                    break;
                case 0x85:
                    text += $"OP_85({ParseParas(reader, "b,b,i", encoding)});\n";
                    break;
                case 0x86:
                    text += $"OP_86({ParseParas(reader, "s,s,s,b,i", encoding)});\n";
                    break;
                case 0x88:
                    text += $"Voice({ParseParas(reader, "s", encoding)});\n";
                    break;
                case 0x89:
                    text += $"SaveCurPos({ParseParas(reader, "b", encoding)});\n";
                    break;
                case 0x8a:
                    text += $"Clone({ParseParas(reader, "b,b", encoding)});\n";
                    break;
                case 0x8b:
                    text += $"UseItemBegin({ParseParas(reader, "", encoding)});\n";
                    break;
                case 0x8c:
                    text += $"UseItemEnd({ParseParas(reader, "", encoding)});\n";
                    break;
                case 0x8d:
                    text += $"OP_8D({ParseParas(reader, "b,i,i,i,i", encoding)});\n";
                    break;
                case 0x8e:
                    b = reader.ReadByte();
                    fs.Position--;
                    text += b switch
                    {
                        1 => $"LoadXFile({ParseParas(reader, "b,b,str", encoding)});\n",
                        0xd=>$"LoadXFile({ParseParas(reader, "b,b,i,i,i,i,i", encoding)});\n",
                        _ => $"LoadXFile({ParseParas(reader, "b,b,i,i,i,i", encoding)});\n"
                    };
                    break;
                case 0x8f:
                    text += $"OP_8F({ParseParas(reader, "b", encoding)});\n";
                    break;
                case 0x90:
                    text += $"OP_90({ParseParas(reader, "b", encoding)});\n";
                    break;
                case 0x92:
                    text += $"OP_92({ParseParas(reader, "b,b,i,i,i,s,i", encoding)});\n";
                    break;
                case 0x93:
                    text += $"OP_93({ParseParas(reader, "b,b,str", encoding)});\n";
                    break;
                case 0x94:
                    text += $"OP_94({ParseParas(reader, "b,str,i", encoding)});\n";
                    break;
                case 0x95:
                    text += $"OP_95({ParseParas(reader, "", encoding)});\n";
                    break;
                case 0x96:
                    text += $"SetAngleTarget({ParseParas(reader, "b,str,s", encoding)});\n";
                    break;
                case 0x97:
                    text += $"MoveAngle({ParseParas(reader, "i,s,s", encoding)});\n";
                    break;
                case 0x98:
                    text += $"OP_98({ParseParas(reader, "b,b,i,i", encoding)});\n";
                    break;
                case 0x99:
                    text += $"OP_99({ParseParas(reader, "b", encoding)});\n";
                    break;
                case 0x9B:
                    text += $"OP_9B({ParseParas(reader, "b", encoding)});\n";
                    break;
                case 0x9C:
                    text += $"ResetChipStatus({ParseParas(reader, "b", encoding)});\n";
                    break;
                case 0x9D:
                    text += $"OP_9D({ParseParas(reader, "b", encoding)});\n";
                    break;
                case 0x9F:
                    text += $"SetBattleMode({ParseParas(reader, "b,i", encoding)});\n";
                    break;
                case 0xa0:
                    text += $"OP_A0({ParseParas(reader, "b,i,s,str", encoding)});\n";
                    break;
                case 0xa1:
                    text += $"OP_A1({ParseParas(reader, "b,i", encoding)});\n";
                    break;
                case 0xa2:
                    text += $"OP_A2({ParseParas(reader, "b", encoding)});\n";
                    break;
                case 0xa3:
                    text += $"OP_A3({ParseParas(reader, "b,b", encoding)});\n";
                    break;
                case 0xa4:
                    b = reader.ReadByte();
                    fs.Position--;
                    if (b == 2)
                        text += $"OP_A4({ParseParas(reader, "b,s", encoding)});\n";
                    else
                        text += $"OP_A4({ParseParas(reader, "b", encoding)});\n";
                    break;
                case 0xa5:
                    text += $"OP_A5({ParseParas(reader, "b,b,i,i,b", encoding)});\n";
                    break;
                case 0xa6:
                    text += $"OP_A6({ParseParas(reader, "b,b,b,i,i,i,i", encoding)});\n";
                    break;
                case 0xa7:
                    text += $"BattleEffEnd({ParseParas(reader, "b,s", encoding)});\n";
                    break;
                case 0xa8:
                    text += $"DamageVoice({ParseParas(reader, "b,b", encoding)});\n";
                    break;
                case 0xa9:
                    text += $"OP_A9({ParseParas(reader, "i", encoding)});\n";
                    break;
                case 0xaa:
                    text += $"OP_AA({ParseParas(reader, "i,i", encoding)});\n";
                    break;
                case 0xab:
                    text += $"OP_AB({ParseParas(reader, "b,b,b", encoding)});\n";
                    break;
                case 0xaC:
                    text += $"OP_AC({ParseParas(reader, "b,b,i,i,b", encoding)});\n";
                    break;
                case 0xaE:
                    text += $"OP_AE({ParseParas(reader, "s,i", encoding)});\n";
                    break;
                case 0xaf:
                    text += $"OP_AF({ParseParas(reader, "b,b,i,i,i", encoding)});\n";
                    break;
                case 0xb1:
                    text += $"OP_B1({ParseParas(reader, "b,s", encoding)});\n";
                    break;
                default:
                    throw new Exception($"unknown opcode: {b:X}");
            }
        }
        BytesFunc(ref text, ref bytes);
        return text;
    }

    private static string ParseParas(BinaryReader reader,string format, Encoding? encoding=null)
    {
        if (string.IsNullOrEmpty(format))
            return "";
        var paras = new string[1];
        if (format.Contains(','))
            paras = format.Split(',');
        else paras[0] = format;
        var text = "";
        for (var i = 0; i < paras.Length; i++)
        {
            var para = paras[i];
            switch (para)
            {
                case "b":
                    text += $"0x{reader.ReadByte():X}:b";
                    break;
                case "s":
                    text  += $"0x{reader.ReadInt16():X}:s";
                    break;
                case "sp":
                    text  += $"Loc_{reader.ReadInt16():X}:sp";
                    break;
                case "i":
                    text  += $"0x{reader.ReadInt32():X}:i";
                    break;
                case "str":
                    var fs = reader.BaseStream as FileStream;
                    ArgumentNullException.ThrowIfNull(encoding);
                    text  += $"{JsonSerializer.Serialize(encoding.GetString(fs!.ReadAsciiBytes()),options)}";
                    break;
                default:
                    throw new Exception($"unknown para: {para}");
            }
            if (i < paras.Length - 1)
                text += ",";
        }

        return text;
    }
    private static void BytesFunc(ref string text, ref List<byte> bytes)
    {
        if (bytes.Count == 0)
            return;
        BytesFunc(ref text,bytes.ToArray());
        bytes.Clear();
    }
    private static void BytesFunc(ref string text, byte[] bytes)
    {
        if (bytes.Length == 0)
            return;
        text += $"bytes(\"{BitConverter.ToString(bytes).Replace("-"," ")}\");\n";
    }
    private static string ParseHead(FileStream fs ,BinaryReader reader)
    {
        var text = "";
        
        var CraftOffsetTable = reader.ReadUInt16();
        var CraftOffsetTableEnd = reader.ReadUInt16();
        text += $"CraftOffsetTable=Loc_{CraftOffsetTable:X}\n";
        text += $"CraftOffsetTableEnd=Loc_{CraftOffsetTableEnd:X}\n";

        var buffer = reader.ReadBytes(CraftOffsetTable - 4);
        BytesFunc(ref text, buffer);
        int index=0;
        text += "#CraftOffsetTable\n";
        while (fs.Position< CraftOffsetTableEnd)
        {
            text += $"craft{index}=Loc_{reader.ReadUInt16():X}\n";
            index++;
        }
        text += "#CraftOffsetTableEnd\n";
        // var bytes = new List<byte>(0xff);
        // byte b = 0;
        // do {
        //     b = reader.ReadByte();
        //     bytes.Add(b);
        // } while (b != 0);
        
        BytesFunc(ref text, reader.ReadBytes(0x10));
        return text;
    }
    

}