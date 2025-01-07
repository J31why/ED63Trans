#region

using DatTrans;
using DatTrans.Dats;
using System.Text;
using System.Text.RegularExpressions;
using static DatTrans.TransChar;

#endregion

internal class Program
{
    private const string xseedDT22Dir = @"E:\SteamLibrary\steamapps\common\Trails in the Sky the 3rd\ED6_DT22";

    private const string yltDT22Dir = @"E:\Games\ED63RD\ED_SORA3\ED6_DT22";

    private const string xseedDT30Dir = @"E:\SteamLibrary\steamapps\common\Trails in the Sky the 3rd\ED6_DT30 - 副本";

    private const string yltDT30Dir = @"E:\Games\ED63RD\ED_SORA3\ED6_DT30";

    public static void Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        ReplaceListFileName = @"F:\源码\C#\ED63Trans\ED63Trans\replace.txt";
        //TransMnsnote();
        
        // TransMagic();
        // TransItTxt();
        // TransBook();
        // TransName();
        // TransCook();
        // TransTitle();
        // TransTown();
        // TransQuest();
        // TransMemo();
        // TransShop();
        // TransChain();
        // TransQuiz();
        // TransFish();
        // TransPoker();
        //查询语音版修改文件
        // var a = "E:\\SteamLibrary\\steamapps\\common\\Trails in the Sky the 3rd\\ED6_DT30 战斗语音版\\ED6_DT30";
        // var b = "E:\\SteamLibrary\\steamapps\\common\\Trails in the Sky the 3rd\\ED6_DT30 - 副本";
        // var fs = Directory.EnumerateFiles(a);
        // foreach (var f in fs)
        // {
        //     /*  语音版文件修改如下
        //         as04090._dt
        //         as14320._dt
        //         ms04520._dt //只是游戏的文本更新 pass
        //         ms04550._dt //只是游戏的文本更新 pass
        //         ms14322._dt //只是游戏的文本更新 pass
        //         ms14390._dt //只是游戏的文本更新 pass
        //         ms14391._dt //只是游戏的文本更新 pass
        //      */
        //     using var c1 = new FileStream(f, FileMode.Open);
        //     using var c2 = new FileStream(Path.Combine(b, Path.GetFileName(f)), FileMode.Open);
        //     for (int i = 0; i < c1.Length; i++)
        //     {
        //         var cc = c1.ReadByte();
        //         var ccc = c2.ReadByte();
        //         if (cc != ccc)
        //         {
        //             Console.WriteLine(f+ $":{c1.Position}");
        //             break;
        //         }
        //     }
        // }
        // 插入魔兽文件
        // var files = Directory.EnumerateFiles("E:\\SteamLibrary\\steamapps\\common\\Trails in the Sky the 3rd\\data\\ED6_DT30");
        // var factoria = "C:\\Users\\Jelly\\Downloads\\Programs\\factoria.exe";
        // var dat = "E:\\SteamLibrary\\steamapps\\common\\Trails in the Sky the 3rd\\ED6_DT30.dir";
        // foreach (var file in files)
        // {
        //     Process.Start(factoria, $"add \"{dat}\" \"{file}\"");
        // }

        Console.WriteLine("\nDatTrans结束\n");
    }

    public static void TransPoker()
    {
        var pokerEn = File.ReadAllText("E:\\SteamLibrary\\steamapps\\common\\Trails in the Sky the 3rd\\voice\\scena\\POKER9.v").Replace("\r\n", "\n");
        var pokerCh = File.ReadAllText("E:\\Games\\ED63RD\\ED_SORA3\\scena\\Z_POKERC._DT").Replace("\r\n", "\n");
        Console.WriteLine("====================================================开始搬运Poker");
        var matches = Regex.Matches(pokerEn, "(#[\\s\\S]+?%c)\\n\\n");
        List<string> en = [];
        foreach (Match match in matches)
        {
            var text = match.Groups[1].Value.Replace("\n", "");
            if (Regex.IsMatch(text, "！|。|？|、"))
            {
                continue;
            }
            en.Add(text);
        }
        matches = Regex.Matches(pokerCh, "(#[\\s\\S]+?%c)\\n\\n");
        List<string> cn = [];
        foreach (Match match in matches)
        {
            var text = match.Groups[1].Value.Replace("\n", "");
            cn.Add(text);
        }

        int i = 0;
        string content = "";
        foreach (var text in en)
        {
            var vid = Regex.Match(text, "#\\d+v").Value;
            var cnText = cn.Find(x => x.Contains(vid));
            var raw = Regex.Replace(text, "#\\d+v", "");
            content += $$"""
                         "{{raw}}": {
                           "Text": "{{cnText}}"
                         },

                         """;
            i++;
        }
        var contentTxt = "Poker_chars.txt";
        File.WriteAllText(contentTxt, content);
        Console.WriteLine(contentTxt + " 已生成");
        Console.WriteLine("====================================================搬运Poker结束");
    }

    public static void TransFish()
    {
        var datName = "t_fish._dt";
        Console.WriteLine(new string('-', 100) + datName + " 开始");
        var xseedDatFile = Path.Combine(xseedDT22Dir, datName);
        var yltDatFile = Path.Combine(yltDT22Dir, datName);
        var xseedDat = FishDat.Parse(xseedDatFile, SjisEncoding);
        var xseedVoiceDat = FishDat.Parse(xseedDatFile + ".v", SjisEncoding);
        var yltDat = FishDat.Parse(yltDatFile, GbkEncoding);

        var text = "";
        var content = "";

        for (int i = 0; i < xseedDat.Fishes.Length; i++)
        {
            text += yltDat.Fishes[i];
            content += $"|{xseedDat.Fishes[i]}|{yltDat.Fishes[i]}\n";
        }

        for (int i = 0; i < xseedDat.DialogTexts.Length; i++)
        {
            text += yltDat.DialogTexts[i];
            content += $"{xseedDat.DialogTextEntries[i]}|{xseedDat.DialogTexts[i]}|{yltDat.DialogTexts[i]}|{xseedVoiceDat.DialogTexts[i]}\n";
        }
        var charsTxt = Path.GetFileNameWithoutExtension(datName) + "_chars.txt";
        File.WriteAllText(charsTxt, text);
        Console.WriteLine(charsTxt + " 已生成");
        var contentTxt = Path.GetFileNameWithoutExtension(datName) + ".txt";
        File.WriteAllText(contentTxt, content);
        Console.WriteLine(contentTxt + " 已生成");
        FishDat.Generate(datName, xseedDat, xseedVoiceDat, yltDat);
        Console.WriteLine(datName + " 已生成");
        Console.WriteLine(new string('-', 100) + datName + " 结束");
    }

    public static void TransQuiz()
    {
        string[] quizFiles =
            [
            "t_quiz01._dt",
            "t_quiz02._dt",
            "t_quiz03._dt",
            "t_quiz04._dt"
            ];

        foreach (var datName in quizFiles)
        {
            var xseedDatFile = Path.Combine(xseedDT22Dir, datName);
            var yltDatFile = Path.Combine(yltDT22Dir, datName);
            var xseedList = QuizDat.Parse(xseedDatFile, SjisEncoding);
            var xseedVoiceList = QuizDat.Parse(xseedDatFile + ".v", SjisEncoding);
            var yltList = QuizDat.Parse(yltDatFile, GbkEncoding);
            Console.WriteLine(new string('-', 100) + datName + " 开始");
            Console.WriteLine($"XSEED \t{datName} item Count: {xseedList!.Count}");
            Console.WriteLine($"XSEED Voice \t{datName} item Count: {xseedVoiceList!.Count}");
            Console.WriteLine($"娱乐通 \t{datName} item Count: {yltList?.Count}");
            if (yltList?.Count != xseedList.Count || xseedList.Count != xseedVoiceList.Count) Console.WriteLine("数量不匹配");
            var text = "";
            var content = "";

            for (int i = 0; i < xseedList.Count; i++)
            {
                var xseedItem = xseedList[i];
                var xseedVoiceItem = xseedVoiceList.Find(x => x.Id == xseedItem.Id);
                var yltItem = yltList?.Find(x => x.Id == xseedItem.Id);
                if (xseedVoiceItem == null)
                {
                    Console.WriteLine($"语音 空ID: {xseedItem.Id}");
                }
                if (yltItem == null)
                {
                    Console.WriteLine($"娱乐通 空ID: {xseedItem.Id}");
                }
                content += $"{xseedItem.Entry}|{xseedItem.Id}";

                for (int j = 0; j < xseedItem.Texts.Length; j++)
                {
                    if (yltItem != null)
                    {
                        text += yltItem.Texts[j];
                    }
                    content += $"|{xseedItem.Texts[j]}|{xseedVoiceItem?.Texts[j]}|{yltItem?.Texts[j]}";
                }
                content += "\n";
            }

            var charsTxt = Path.GetFileNameWithoutExtension(datName) + "_chars.txt";
            File.WriteAllText(charsTxt, text);
            Console.WriteLine(charsTxt + " 已生成");
            var contentTxt = Path.GetFileNameWithoutExtension(datName) + ".txt";
            File.WriteAllText(contentTxt, content);
            Console.WriteLine(contentTxt + " 已生成");
            QuizDat.Generate(datName, xseedList, xseedVoiceList, yltList);
            Console.WriteLine(datName + " 已生成");
            Console.WriteLine(new string('-', 100) + datName + " 结束");
        }
    }

    public static void TransChain()
    {
        var datName = "t_chain._dt";
        Console.WriteLine(new string('-', 100) + datName + " 开始");
        var xseedDatFile = Path.Combine(xseedDT22Dir, datName);
        var yltDatFile = Path.Combine(yltDT22Dir, datName);
        var xseedDat = ChainDat.Parse(xseedDatFile, SjisEncoding);
        var yltDat = ChainDat.Parse(yltDatFile, GbkEncoding);
        var text = "";
        var content = "";

        for (int i = 0; i < xseedDat.BaseItems_2.GetLength(0); i++)
        {
            text += $"{yltDat.BaseItems_2[i, 0].Text}{yltDat.BaseItems_2[i, 1].Text}";

            content +=
                $"{xseedDat.BaseItems_2[i, 0].Entry}|{xseedDat.BaseItems_2[i, 0].Text}|{yltDat.BaseItems_2[i, 0].Text}\n";
            content +=
                $"{xseedDat.BaseItems_2[i, 1].Entry}|{xseedDat.BaseItems_2[i, 1].Text}|{yltDat.BaseItems_2[i, 1].Text}\n";
        }

        for (int i = 0; i < xseedDat.BaseItems_3.Length; i++)
        {
            text += yltDat.BaseItems_3[i].Text;
            content +=
                $"{xseedDat.BaseItems_3[i].Entry}|{xseedDat.BaseItems_3[i].Text}|{yltDat.BaseItems_3[i].Text}\n";
        }

        var charsTxt = Path.GetFileNameWithoutExtension(datName) + "_chars.txt";
        File.WriteAllText(charsTxt, text);
        Console.WriteLine(charsTxt + " 已生成");
        var contentTxt = Path.GetFileNameWithoutExtension(datName) + ".txt";
        File.WriteAllText(contentTxt, content);
        Console.WriteLine(contentTxt + " 已生成");
        ChainDat.Generate(datName, xseedDat, yltDat);
        Console.WriteLine(datName + " 已生成");
        Console.WriteLine(new string('-', 100) + datName + " 结束");
    }

    public static void TransShop()
    {
        var datName = "t_shop._dt";
        var xseedDatFile = Path.Combine(xseedDT22Dir, datName);
        var xseedList = ShopDat.Parse(xseedDatFile, SjisEncoding);
        Console.WriteLine(new string('-', 100) + datName + " 开始");
        Console.WriteLine($"XSEED \t{datName} item Count: {xseedList.Count}");
        var content = "";
        foreach (var xseedItem in xseedList)
        {
            content += $"{xseedItem.Entry}|{xseedItem.Id}|{xseedItem.Name}\n";
        }

        var contentTxt = Path.GetFileNameWithoutExtension(datName) + ".txt";
        File.WriteAllText(contentTxt, content);
        Console.WriteLine(contentTxt + " 已生成");
        ShopDat.Generate(datName, xseedList);
        Console.WriteLine(datName + " 已生成");
        Console.WriteLine(new string('-', 100) + datName + " 结束");
    }

    public static void TransMemo()
    {
        var datName = "t_memo._dt";
        var xseedDatFile = Path.Combine(xseedDT22Dir, datName);
        var yltDatFile = Path.Combine(yltDT22Dir, datName);
        var xseedList = MemoDat.Parse(xseedDatFile, SjisEncoding);
        var yltList = MemoDat.Parse(yltDatFile, GbkEncoding);
        Console.WriteLine(new string('-', 100) + datName + " 开始");
        Console.WriteLine($"XSEED \t{datName} item Count: {xseedList.Count}");
        Console.WriteLine($"娱乐通 \t{datName} item Count: {yltList.Count}");
        if (yltList.Count != xseedList.Count) Console.WriteLine("数量不匹配");
        var text = "";
        var content = "";

        for (int i = 0; i < xseedList.Count; i++)
        {
            var xseedItem = xseedList[i];
            var yltItem = yltList[i];
            text += yltItem.Text;
            content += $"{xseedItem.Entry}|{xseedItem.Id}|{xseedItem.Text}|{yltItem.Text}\n";
        }

        var charsTxt = Path.GetFileNameWithoutExtension(datName) + "_chars.txt";
        File.WriteAllText(charsTxt, text);
        Console.WriteLine(charsTxt + " 已生成");
        var contentTxt = Path.GetFileNameWithoutExtension(datName) + ".txt";
        File.WriteAllText(contentTxt, content);
        Console.WriteLine(contentTxt + " 已生成");
        MemoDat.Generate(datName, xseedList, yltList);
        Console.WriteLine(datName + " 已生成");
        Console.WriteLine(new string('-', 100) + datName + " 结束");
    }
    
    public static void TransMnsnote()
    {
        Console.WriteLine(new string('-', 100)  + "mnsnote2 开始");
        var datName = "E:\\SteamLibrary\\steamapps\\common\\Trails in the Sky the 3rd\\ED6_DT3C\\mnsnote2._dt";
        var translatedMsDir = "E:\\SteamLibrary\\steamapps\\common\\Trails in the Sky the 3rd\\ED6_DT30";
        MnsnoteDat.Generate(datName, translatedMsDir);
        Console.WriteLine("mnsnote2 已生成");
        Console.WriteLine(new string('-', 100)  + "mnsnote2 结束");
    }

    public static void TransQuest()
    {
        var datName = "t_quest._dt";
        var xseedDatFile = Path.Combine(xseedDT22Dir, datName);
        var yltDatFile = Path.Combine(yltDT22Dir, datName);
        var xseedList = QuestDat.Parse(xseedDatFile, SjisEncoding);
        var yltList = QuestDat.Parse(yltDatFile, GbkEncoding);
        Console.WriteLine(new string('-', 100) + datName + " 开始");
        Console.WriteLine($"XSEED \t{datName} item Count: {xseedList.Count}");
        Console.WriteLine($"娱乐通 \t{datName} item Count: {yltList.Count}");
        if (yltList.Count != xseedList.Count) Console.WriteLine("数量不匹配");
        var text = "";
        var content = "";

        for (var i = 0; i < xseedList.Count; i++)
        {
            var xseedItem = xseedList[i];
            var yltItem = yltList.Find(x => x.Id == xseedItem.Id);
            if (yltItem == null) Console.WriteLine($"空ID: {xseedItem.Id}");
            content += $"{xseedItem.Entry}|{xseedItem.Id}";
            for (var j = 0; j < 18; j++)
                if (yltItem != null)
                {
                    text += yltItem.Texts[j];
                    content += $"|{xseedItem.Texts[j]}|{yltItem.Texts[j]}";
                }
                else
                {
                    content += $"|{xseedItem.Texts[j]}|";
                }

            content += "\n";
        }

        var charsTxt = Path.GetFileNameWithoutExtension(datName) + "_chars.txt";
        File.WriteAllText(charsTxt, text);
        Console.WriteLine(charsTxt + " 已生成");
        var contentTxt = Path.GetFileNameWithoutExtension(datName) + ".txt";
        File.WriteAllText(contentTxt, content);
        Console.WriteLine(contentTxt + " 已生成");
        QuestDat.Generate(datName, xseedList, yltList);
        Console.WriteLine(datName + " 已生成");
        Console.WriteLine(new string('-', 100) + datName + " 结束");
    }

    private static void TransTown()
    {
        var datName = "t_town._dt";
        var xseedDatFile = Path.Combine(xseedDT22Dir, datName);
        var yltDatFile = Path.Combine(yltDT22Dir, datName);
        var xseedList = TownDat.Parse(xseedDatFile, SjisEncoding);
        var yltList = TownDat.Parse(yltDatFile, GbkEncoding);
        Console.WriteLine(new string('-', 100) + datName + " 开始");
        Console.WriteLine($"XSEED \t{datName} item Count: {xseedList.Count}");
        Console.WriteLine($"娱乐通 \t{datName} item Count: {yltList.Count}");
        if (yltList.Count != xseedList.Count) Console.WriteLine("数量不匹配");

        var text = "";
        var content = "";

        for (var i = 0; i < xseedList.Count; i++)
        {
            var xseedItem = xseedList[i];
            var yltItem = yltList[i];
            text += yltItem.Name;
            content += $"{xseedItem.Entry}|{xseedItem.Name}|{yltItem.Name}\n";
        }

        var charsTxt = Path.GetFileNameWithoutExtension(datName) + "_chars.txt";
        File.WriteAllText(charsTxt, text);
        Console.WriteLine(charsTxt + " 已生成");
        var contentTxt = Path.GetFileNameWithoutExtension(datName) + ".txt";
        File.WriteAllText(contentTxt, content);
        Console.WriteLine(contentTxt + " 已生成");
        TownDat.Generate(datName, xseedList, yltList);
        Console.WriteLine(datName + " 已生成");
        Console.WriteLine(new string('-', 100) + datName + " 结束");
    }

    private static void TransTitle()
    {
        var datName = "t_title._dt";
        var xseedDatFile = Path.Combine(xseedDT22Dir, datName);
        var yltDatFile = Path.Combine(yltDT22Dir, datName);
        var xseedList = TitleDat.Parse(xseedDatFile, SjisEncoding);
        var yltList = TitleDat.Parse(yltDatFile, GbkEncoding);
        Console.WriteLine(new string('-', 100) + datName + " 开始");
        var count1 = 0;
        var count2 = 0;
        foreach (var item in xseedList) count1 += item.TextEntries.Count;
        foreach (var item in yltList) count2 += item.TextEntries.Count;
        Console.WriteLine($"XSEED \t{datName} item Count: {count1}");
        Console.WriteLine($"娱乐通 \t{datName} item Count: {count2}");
        if (count1 != count2) Console.WriteLine("数量不匹配1");
        var text = "";
        var content = "";

        for (var i = 0; i < xseedList.Count; i++)
        {
            var xseedItem = xseedList[i];
            var yltItem = yltList[i];
            if (xseedItem.TextEntries.Count != yltItem.TextEntries.Count)
                throw new Exception("数量不匹配2");
            for (var j = 0; j < xseedItem.TextEntries.Count; j++)
            {
                text += yltItem.Texts[j];
                content += $"{xseedItem.Entry}|{j}|{xseedItem.TextEntries[j]}|{xseedItem.Texts[j]}|{yltItem.Texts[j]}\n";
            }
        }

        var charsTxt = Path.GetFileNameWithoutExtension(datName) + "_chars.txt";
        File.WriteAllText(charsTxt, text);
        Console.WriteLine(charsTxt + " 已生成");
        var contentTxt = Path.GetFileNameWithoutExtension(datName) + ".txt";
        File.WriteAllText(contentTxt, content);
        Console.WriteLine(contentTxt + " 已生成");
        TitleDat.Generate(datName, xseedList, yltList);
        Console.WriteLine(datName + " 已生成");
        Console.WriteLine(new string('-', 100) + datName + " 结束");
    }

    private static void TransCook()
    {
        var datName = "t_cook2._dt";
        var xseedDatFile = Path.Combine(xseedDT22Dir, datName);
        var yltDatFile = Path.Combine(yltDT22Dir, datName);
        var xseedList = CookDat.Parse(xseedDatFile, SjisEncoding);
        var yltList = CookDat.Parse(yltDatFile, GbkEncoding);
        Console.WriteLine(new string('-', 100) + datName + " 开始");
        Console.WriteLine($"XSEED \t{datName} item Count: {xseedList.Count}");
        Console.WriteLine($"娱乐通 \t{datName} item Count: {yltList.Count}");
        if (yltList.Count != xseedList.Count) Console.WriteLine("数量不匹配");
        var text = "";
        var content = "";
        xseedList.ForEach(x =>
        {
            var yltItem = yltList.Find(z => z.Id == x.Id);
            if (yltItem != null)
            {
                text += yltItem.Name;
                text += yltItem.Desc;
                content += $"{x.Entry}|{x.Id}|{x.Name}|{yltItem.Name}|{x.Desc}|{yltItem.Desc}\n";
            }
            else
            {
                Console.WriteLine($"空Id: {x.Id}");
                content += $"{x.Entry}|{x.Id}|{x.Name}||{x.Desc}\n";
            }
        });
        var charsTxt = Path.GetFileNameWithoutExtension(datName) + "_chars.txt";
        File.WriteAllText(charsTxt, text);
        Console.WriteLine(charsTxt + " 已生成");
        var contentTxt = Path.GetFileNameWithoutExtension(datName) + ".txt";
        File.WriteAllText(contentTxt, content);
        Console.WriteLine(contentTxt + " 已生成");
        CookDat.Generate(datName, xseedList, yltList);
        Console.WriteLine(datName + " 已生成");
        Console.WriteLine(new string('-', 100) + datName + " 结束");
    }

    private static void TransName()
    {
        var datName = "t_name._dt";
        var xseedDatFile = Path.Combine(xseedDT22Dir, datName);
        var yltDatFile = Path.Combine(yltDT22Dir, datName);
        var xseedList = NameDat.Parse(xseedDatFile, SjisEncoding);
        var yltList = NameDat.Parse(yltDatFile, GbkEncoding);
        Console.WriteLine(new string('-', 100) + datName + " 开始");
        Console.WriteLine($"XSEED \t{datName} item Count: {xseedList.Count}");
        Console.WriteLine($"娱乐通 \t{datName} item Count: {yltList.Count}");
        if (yltList.Count != xseedList.Count) Console.WriteLine("数量不匹配");
        var text = "";
        var content = "";

        foreach (var item in xseedList)
        {
            var yltChar = yltList.Find(x => x.Id == item.Id);
            if (yltChar != null)
            {
                text += yltChar.Name;
                content += $"{item.Id}|{item.Name}|{yltChar.Name}\n";
            }
            else
            {
                Console.WriteLine($"空Id: {item.Id}");
                content += $"{item.Id}|{item.Name}\n";
            }
        }

        var charsTxt = Path.GetFileNameWithoutExtension(datName) + "_chars.txt";
        File.WriteAllText(charsTxt, text);
        Console.WriteLine(charsTxt + " 已生成");
        var contentTxt = Path.GetFileNameWithoutExtension(datName) + ".txt";
        File.WriteAllText(contentTxt, content);
        Console.WriteLine(contentTxt + " 已生成");
        NameDat.Generate(datName, xseedList, yltList);
        Console.WriteLine(datName + " 已生成");
        Console.WriteLine(new string('-', 100) + datName + " 结束");
    }

    private static void TransBook(string? datName = null)
    {
        if (datName == null)
        {
            string[] files =
            [
                "t_book01._dt", "t_book02._dt", "t_book03._dt", "t_book04._dt",
                "t_book05._dt", "t_book06._dt", "t_book07._dt", "t_book08._dt",
                "t_book09._dt", "t_book10._dt", "t_book11._dt", "t_book12._dt",
                "t_book13._dt", "t_book14._dt", "t_book15._dt", "t_book16._dt"
            ];
            foreach (var file in files) TransBook(file);
            return;
        }

        var xseedDatFile = Path.Combine(xseedDT22Dir, datName);
        var yltDatFile = Path.Combine(yltDT22Dir, datName);
        var xseedList = BookDat.Parse(xseedDatFile, SjisEncoding);
        var yltList = BookDat.Parse(yltDatFile, GbkEncoding);
        Console.WriteLine(new string('-', 100) + datName + " 开始");
        Console.WriteLine($"XSEED \t{datName} item Count: {xseedList.Count}");
        Console.WriteLine($"娱乐通 \t{datName} item Count: {yltList.Count}");
        if (yltList.Count != xseedList.Count) Console.WriteLine("数量不匹配");
        var text = "";
        var content = "";

        for (var i = 0; i < xseedList.Count; i++)
        {
            var xseedItem = xseedList[i];
            var yltItem = yltList[i];
            text += yltItem.Content;
            content += $"{xseedItem.Entry}|{xseedItem.Content}|{yltItem.Content}\n";
        }

        var charsTxt = Path.GetFileNameWithoutExtension(datName) + "_chars.txt";
        File.WriteAllText(charsTxt, text);
        Console.WriteLine(charsTxt + " 已生成");
        var contentTxt = Path.GetFileNameWithoutExtension(datName) + ".txt";
        File.WriteAllText(contentTxt, content);
        Console.WriteLine(contentTxt + " 已生成");
        BookDat.Generate(datName, xseedList, yltList);
        Console.WriteLine(datName + " 已生成");
        Console.WriteLine(new string('-', 100) + datName + " 结束");
    }

    private static void TransItTxt(string? datName = null)
    {
        if (datName == null)
        {
            string[] files = ["t_ittxt._dt", "t_ittxt2._dt"];
            foreach (var file in files) TransItTxt(file);
            return;
        }

        var xseedDatFile = Path.Combine(xseedDT22Dir, datName);
        var yltDatFile = Path.Combine(yltDT22Dir, datName);
        var xseedList = ItTxtDat.Parse(xseedDatFile, SjisEncoding);
        var yltList = ItTxtDat.Parse(yltDatFile, GbkEncoding);
        Console.WriteLine(new string('-', 100) + datName + " 开始");
        Console.WriteLine($"XSEED \t{datName} item Count: {xseedList.Count}");
        Console.WriteLine($"娱乐通 \t{datName} item Count: {yltList.Count}");
        if (yltList.Count != xseedList.Count) Console.WriteLine("数量不匹配");
        // xseedList.ForEach(x =>
        // {
        //     var exists=yltList.Exists(z => z.Id == x.Id);
        //     if (!exists)
        //     {
        //         Console.WriteLine($"entry: {x.Entry}  \tid: {x.Id}  \ttext: {x.Text}  \ttext2: {x.Text2}");
        //     }
        // });
        // xseed 多一个这个.  #**I 是物品图标;
        //         //entry: 37634    id: 750         text: Time Gem          text2: STR-20%, DEF-20%, ATS-15% (#93ICast 3)
        var text = "";
        var content = "";
        xseedList.ForEach(x =>
        {
            var yltItem = yltList.Find(z => z.Id == x.Id);
            if (yltItem != null)
            {
                text += yltItem.Name;
                text += yltItem.Desc;
                content += $"{x.Entry}|{x.Id}|{x.Name}|{yltItem.Name}|{x.Desc}|{yltItem.Desc}\n";
            }
            else
            {
                Console.WriteLine($"空Id: {x.Id}");
                content += $"{x.Entry}|{x.Id}|{x.Name}||{x.Desc}\n";
            }
        });
        var charsTxt = Path.GetFileNameWithoutExtension(datName) + "_chars.txt";
        File.WriteAllText(charsTxt, text);
        Console.WriteLine(charsTxt + " 已生成");
        var contentTxt = Path.GetFileNameWithoutExtension(datName) + ".txt";
        File.WriteAllText(contentTxt, content);
        Console.WriteLine(contentTxt + " 已生成");
        ItTxtDat.Generate(datName, xseedList, yltList);
        Console.WriteLine(datName + " 已生成");
        Console.WriteLine(new string('-', 100) + datName + " 结束");
    }

    private static void TransMagic()
    {
        var datName = "t_magic._dt";
        var xseedDatFile = Path.Combine(xseedDT22Dir, datName);
        var yltDatFile = Path.Combine(yltDT22Dir, datName);
        var xseedList = MagicDat.Parse(xseedDatFile, SjisEncoding);
        var yltList = MagicDat.Parse(yltDatFile, GbkEncoding);
        Console.WriteLine(new string('-', 100) + datName + " 开始");
        Console.WriteLine($"XSEED \t{datName} item Count: {xseedList.Count}");
        Console.WriteLine($"娱乐通 \t{datName} item Count: {yltList.Count}");
        if (yltList.Count != xseedList.Count) Console.WriteLine("数量不匹配");
        var text = "";
        var content = "";
        xseedList.ForEach(x =>
        {
            var yltItem = yltList.Find(z => z.Id == x.Id);
            if (yltItem != null)
            {
                text += yltItem.Name;
                text += yltItem.Desc;
                content += $"{x.Entry}|{x.Id}|{x.Name}|{yltItem.Name}|{x.Desc}|{yltItem.Desc}\n";
            }
            else
            {
                Console.WriteLine($"空Id: {x.Id}");
                content += $"{x.Entry}|{x.Id}|{x.Name}||{x.Desc}\n";
            }
        });
        var charsTxt = Path.GetFileNameWithoutExtension(datName) + "_chars.txt";
        File.WriteAllText(charsTxt, text);
        Console.WriteLine(charsTxt + " 已生成");
        var contentTxt = Path.GetFileNameWithoutExtension(datName) + ".txt";
        File.WriteAllText(contentTxt, content);
        Console.WriteLine(contentTxt + " 已生成");
        MagicDat.Generate(datName, xseedList, yltList);
        Console.WriteLine(datName + " 已生成");
        Console.WriteLine(new string('-', 100) + datName + " 结束");
    }
}