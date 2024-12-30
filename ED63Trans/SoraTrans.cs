#region

using ED63Trans.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

#endregion

namespace ED63Trans;

public class SoraTrans
{
    public class SoraMatch
    {
        public int Index;
        public int Length;
        public string Text = "";
    }

    public SoraTrans(string clm, string edd)
    {
        LoadClm(clm);
        LoadEdd(edd);
    }

    public void LoadClm(string clm)
    {
        //-------------clm-------------
        ClmText = File.ReadAllText(clm);
        ClmFile = clm;
        if (!Regex.IsMatch(ClmFile, @"ED6_DT(\d+)")) throw new Exception("脚本目录错误, 放在ED6_DT*格式目录中再试");

        MatchCollection? matches = null;
        //npc name match

        matches = Regex.Matches(ClmText, """
                                         npc char\S+\n.*?\".{1,}\"
                                         """);
        foreach (Match match in matches)
        {
            var soraMatch = new SoraMatch
            {
                Index = match.Index,
                Length = match.Length,
                Text = match.Value
            };
            ClmMatches.Add(soraMatch);
        }

        //text match
        matches = Regex.Matches(ClmText, @"\t*?(Text.*?)\{([\s\S]*?)\t\}\n|\t*?Menu ([\s\S]*?)(break|MenuWait|var\[)");
        foreach (Match match in matches)
        {
            var soraMatch = new SoraMatch
            {
                Index = match.Index,
                Length = match.Length,
                Text = match.Value
            };
            ClmMatches.Add(soraMatch);
        }

        //other text match
        matches = Regex.Matches(ClmText, @"TextSetName ""(.{1,})""");
        foreach (Match match in matches)
        {
            var soraMatch = new SoraMatch
            {
                Index = match.Index,
                Length = match.Length,
                Text = match.Value
            };
            ClmMatches.Add(soraMatch);
        }

        ClmMatches.Sort((x, y) => x.Index.CompareTo(y.Index));
        TransTexts = new string[ClmMatches.Count];
    }

    public void LoadEdd(string edd)
    {
        //-------------edd-------------
        if (!File.Exists(edd)) return;
        EddText = File.ReadAllText(edd);
        EddText = EddText.Replace("\r\n", "\n");
        EddFile = edd;
        //npc match
        var npcNames = Regex.Match(EddText, @"BuildStringList\(([\s\S]*?)\)\n").Value;
        var matches = Regex.Matches(npcNames, @"'.*?',\s*# \S+");
        foreach (Match match in matches)
        {
            if (match.Value.Contains('@') || match.Value.StartsWith("'',")) continue;
            var soraMatch = new SoraMatch
            {
                Index = match.Index,
                Length = match.Length,
                Text = match.Value
            };
            EddMatches.Add(soraMatch);
        }

        //text match
        matches = Regex.Matches(EddText, @"    \S+Talk([\s\S]*?)\n    \)|    Menu\(([\s\S]*?)\n    \)");
        foreach (Match match in matches)
        {
            var soraMatch = new SoraMatch
            {
                Index = match.Index,
                Length = match.Length,
                Text = match.Value
            };
            EddMatches.Add(soraMatch);
        }

        //other text match
        matches = Regex.Matches(EddText, @"SetChrName\(""(.{1,})""\)");
        foreach (Match match in matches)
        {
            var soraMatch = new SoraMatch
            {
                Index = match.Index,
                Length = match.Length,
                Text = match.Value
            };
            EddMatches.Add(soraMatch);
        }
        EddMatches.Sort((x, y) => x.Index.CompareTo(y.Index));
    }

    public static int autoTransIndex = -1;

    public void AutoTrans()
    {
        if (ClmMatches.Count != EddMatches.Count)
        {
            for (var i = 0; i < TransTexts.Length; i++)
                TransTexts[i] = ClmMatches[i].Text;
            return;
        }

        for (var i = 0; i < TransTexts.Length; i++)
        {
            try
            {
                autoTransIndex = i + 1;
                TransTexts[i] = EddText2ClmText(EddMatches[i].Text, ClmMatches[i].Text);
            }
            catch (Exception e)
            {
                TransTexts[i] = ClmMatches[i].Text;
                MainWindowViewModel.Instance!.AddLog($"{e.Message}: {autoTransIndex}");
            }
        }

        autoTransIndex = -1;
    }

    public string DATName { get; set; }
    public string ClmText { get; set; }
    public string ClmFile { get; set; }
    public List<SoraMatch> ClmMatches { get; set; } = [];

    public string EddText { get; set; }
    public string EddFile { get; set; }
    public List<SoraMatch> EddMatches { get; set; } = [];

    public string[] TransTexts { get; set; }

    public static string EddText2ClmText(string eddText, string clmText)
    {
        if (clmText.StartsWith("npc char")) return ReplaceNpcName(eddText, clmText);
        if (clmText.Contains("TextSetName")) return ReplaceSetTextName(eddText, clmText);

        if (clmText.Contains(ClmTextTalkNamed.FuncName))
        {
            var clmTextTalkNamed = new ClmTextTalkNamed(clmText);
            if (eddText.Contains(EddNpcTalk.FuncName))
            {
                var eddNpcTalk = new EddNpcTalk(eddText);
                return clmTextTalkNamed.Trans(eddNpcTalk);
            }

            throw new Exception($"{ClmTextTalkNamed.FuncName}匹配错误");
        }

        if (clmText.Contains(ClmTextMessage.FuncName))
        {
            var clmTextMessage = new ClmTextMessage(clmText);
            if (eddText.Contains(EddAnonymousTalk.FuncName))
            {
                var eddAnonymousTalk = new EddAnonymousTalk(eddText);
                return clmTextMessage.Trans(eddAnonymousTalk);
            }

            throw new Exception($"{ClmTextMessage.FuncName}匹配错误");
        }

        if (clmText.Contains(ClmTextTalk.FuncName))
        {
            var clmTextTalk = new ClmTextTalk(clmText);
            if (eddText.Contains(EddChrTalk.FuncName))
            {
                var eddCharTalk = new EddChrTalk(eddText);
                return clmTextTalk.Trans(eddCharTalk);
            }

            throw new Exception($"{ClmTextTalk.FuncName}匹配错误");
        }

        if (clmText.Contains(ClmMenu.FuncName))
        {
            var clmMenu = new ClmMenu(clmText);
            if (eddText.Contains(EddMenu.FuncName))
            {
                var eddMenu = new EddMenu(eddText);
                return clmMenu.Trans(eddMenu);
            }

            throw new Exception($"{ClmMenu.FuncName}匹配错误");
        }

        throw new Exception($"无法匹配翻译脚本 : {clmText}");

    }

    public string ApplyTrans()
    {
        var sb = new StringBuilder();
        var pos = 0;
        try
        {
            for (var i = 0; i < ClmMatches.Count; i++)
            {
                var clmMatch = ClmMatches[i];
                var transText = TransTexts[i];
                sb.Append(ClmText.Substring(pos, clmMatch.Index - pos));
                sb.Append(transText);
                pos = clmMatch.Index + clmMatch.Length;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        sb.Append(ClmText.Substring(pos, ClmText.Length - pos));
        return sb.ToString();
    }

    public static string CheckText(string text, string rawClmText)
    {
        var trans = text;
        if (text.Contains("\\u3000"))
        {
            MainWindowViewModel.Instance!.AddLog($"内含\\u3000: {autoTransIndex}");
        }
        if (!CheckIndent(text, rawClmText))
        {
            MainWindowViewModel.Instance!.AddLog($"文本缩进错误: {autoTransIndex}");
        }

        return trans;
    }

    public static bool CheckIndent(string text, string rawClmText)
    {
        var lines = text.Split("\n");
        var indentCount = 0;
        for (int i = 0; i < rawClmText.Length; i++)
        {
            if (rawClmText[i] == '\t')
                indentCount++;
            else
                break;
        }
        for (var i = 0; i < lines.Length - 1; i++)
        {
            if (lines[i].EndsWith("\t}") || lines[i].EndsWith("\t} {") || lines[i].Contains("MenuWait"))
            {
                indentCount--;
            }
            if (!lines[i].StartsWith(new string('\t', indentCount)) || lines[i][indentCount] == '\t')
            {
                return false;
            }
            if (lines[i].EndsWith('{') || lines[i].EndsWith(':') || lines[i].Contains("Menu menu["))
            {
                indentCount++;
            }
            if (lines[i].EndsWith("\t}"))
            {
                indentCount--;
            }
        }
        return true;
    }

    public static string GetVoiceScript(string clmText)
    {
        if (!Regex.IsMatch(clmText, "#\\d+v")) return "";
        return Regex.Match(clmText, "#\\d+v").Value;
    }

    public static string EddTexts2ClmText(string[] eddTexts, string indent, string[] clmTexts, string rawClmText)
    {
        //var talkIndex = 0;
        var trans = "";

        //if (clmTexts.Length > talkIndex)
        //{
        //    trans += GetVoiceScript(clmTexts[talkIndex]);
        //    talkIndex++;
        //}

        foreach (var t in eddTexts)
        {
            var text = t.Trim();
            if (text.StartsWith('\"') && text.EndsWith('\"'))
            {
                text = text.Substring(1, text.Length - 2);
                if (text.Contains("\\x07\\x"))
                    text = Regex.Replace(text, @"\\x07\\x(\S{2,2})", m =>
                    {
                        var color = m.Groups[1].Value;
                        return $"{{color {Convert.ToInt32(color, 16)}}}";
                    });
                text = text.Replace("\\x02\\x01", $"{{wait}}\n{indent}\t");
                text = text.Replace("\\x01", $"\n{indent}\t");
                text = text.Replace("\\x02\\x02", "{wait}\n");
                text = text.Replace("\\x02", "{wait}\n");
                trans += $"{text}";
                if (text.Contains("\\x03"))
                {
                    //trans += $"{indent}}} {{\n{indent}\t";
                    trans = trans.Replace("\\x03", $"{indent}}} {{\n{indent}\t");
                    //if (text.Contains("\\x05")) trans = trans.Replace("\\x05", "{0x05}");
                    //if (clmTexts.Length > talkIndex)
                    //{
                    //    trans += GetVoiceScript(clmTexts[talkIndex]);
                    //    talkIndex++;
                    //}
                    //else
                    //{
                    //    MainWindowViewModel.Instance!.AddLog($"语音脚本匹配出错: {rawClmText}");
                    //}
                }

                if (text.Contains("\\x05")) trans = trans.Replace("\\x05", "{0x05}");
            }
            else if (text.StartsWith("scpstr(SCPSTR_CODE_COLOR"))
            {
                var color = Regex.Match(text, "scpstr\\(SCPSTR_CODE_COLOR, 0x(.*?)\\)").Groups[1].Value;
                trans += $"{{color {Convert.ToInt32(color, 16)}}}";
            }
            else if (text.StartsWith("scpstr(SCPSTR_CODE_ITEM"))
            {
                var item = Regex.Match(text, "scpstr\\(SCPSTR_CODE_ITEM, 0x(.*?)\\)").Groups[1].Value;
                trans += $"{{item item[{Convert.ToInt32(item, 16)}]}}";
            }
            else if (text.StartsWith("scpstr(0x"))
            {
                var code = Regex.Match(text, "scpstr\\(0x(.*?)\\)").Groups[1].Value;
                trans += $"{{0x{code}}}";
            }
            else if (text.StartsWith("scpstr(SCPSTR_CODE_ENTER)"))
            {
                trans += "{wait}\n";
            }
        }
        //Check code
        if (!CheckCode(trans, rawClmText))
        {
            MainWindowViewModel.Instance!.AddLog($"自动翻译 Check code 错误: {autoTransIndex}");
        }

        return trans;
    }

    public static bool CheckCode(string trans, string rawClmText)
    {
        var pattern = $"#\\d+([bdefghijklmnopqrstuvxyzBDEFGHIJKLMNOPQRSTUVXYZ])";
        var matches = Regex.Matches(rawClmText, pattern);

        if (Regex.Count(trans, pattern) != matches.Count)
            return false;

        foreach (Match match in matches)
        {
            if (!trans.Contains(match.Value))
            {
                return false;
            }
        }
        return true;
    }

    public static string ReplaceNpcName(string eddText, string clmText)
    {
        var m = Regex.Match(clmText, @"npc char\S+\n\s+name \""");
        var text = m.Value;
        m = Regex.Match(eddText, @"'(.*?)',");
        var value = m.Groups[1].Value;
        if (value == "尤莉亚\\u3000\\u3000\\u3000\\u3000\\u3000\\u3000\\u3000")
        {
            value = "尤莉亚";
        }

        text += value + "\"";
        return CheckText(text.Trim(), clmText);
    }

    public static string ReplaceSetTextName(string eddText, string clmText)
    {
        //TextSetName "Masked Man"
        //SetChrName("戴面具的绅士")

        //TextSetName "戴面具的绅士"
        var text = "TextSetName \"";
        var m = Regex.Match(eddText, @"SetChrName\(""(.*?)""\)");
        text += m.Groups[1].Value.Trim('\u3000') + "\"";
        return CheckText(text.Trim('\u3000'), clmText);
    }

    public class ClmMenu
    {
        /*
            Menu menu[0] 10 10 1
                "[Read Chapter 1]" // 0
                "[Read Chapter 2]" // 1
                "[Read Chapter 3]" // 2
                "[Read Chapter 4]" // 3
                "[Read Chapter 5]" // 4
                "[Read Chapter 6]" // 5
                "[Cancel]" // 6
            MenuWait
         */
        public const string FuncName = "Menu menu";
        public string Indent;
        public string Head;
        public string[] Texts;
        public string[] Count;
        public string RawText;

        public ClmMenu(string clmText)
        {
            RawText = clmText;
            var matches = Regex.Matches(clmText, """
                                                 (\s+)(Menu menu.*?)\n|\s+\"(.*?)" \/\/ (.*?)\n
                                                 """);
            if (matches.Count == 0) throw new Exception($"{clmText} regex error");
            var m = matches.First();
            Indent = m.Groups[1].Value;
            Head = m.Groups[2].Value;
            Texts = new string[matches.Count - 1];
            Count = new string[matches.Count - 1];
            for (var i = 1; i < matches.Count; i++)
            {
                m = matches[i];
                if (!m.Success) continue;
                Texts[i - 1] = m.Groups[3].Value;
                Count[i - 1] = m.Groups[4].Value;
            }
        }

        public string Trans(EddMenu eddMenu)
        {
            var trans = $"{Indent}{Head}\n{Indent}";
            for (var i = 0; i < Texts.Length; i++)
                trans += $"\t\"{eddMenu.Texts[i].Replace("\\x01", "")}\" // {Count[i]}\n{Indent}";
            if (RawText.EndsWith("MenuWait"))
            {
                trans += "MenuWait";
            }
            else if (RawText.EndsWith("break"))
            {
                trans += "break";
            }
            else if (RawText.EndsWith("var["))
            {
                trans += "var[";
            }
            return CheckText(trans, RawText);
        }
    }

    public class ClmTextTalkNamed
    {
        /*
            TextTalkNamed name[8] "Kevin Graham" {
                #1075F#4PUh-uh! Wrong answer. This belongs to the Goddess,
                and it needs to be rightfully returned to Her.{wait}
            } {
                It's not something mere mortals like you should be
                keeping for themselves.{wait}
            }
        */
        public const string FuncName = "TextTalkNamed";
        public string Char; //name[8]
        public string Indent;
        public string Name; //Kevin Graham
        public string[] Texts;
        public string RawText;

        public ClmTextTalkNamed(string clmText)
        {
            RawText = clmText;
            var matches = Regex.Matches(clmText, """
                                                 (\s+)TextTalkNamed (.*?) \"(.*?)\"|\{\n([\S\s]*?)\n\s+}
                                                 """);
            if (matches.Count == 0) throw new Exception($"{clmText} regex error");
            var m = matches.First();
            Indent = m.Groups[1].Value;
            Char = m.Groups[2].Value;
            Name = m.Groups[3].Value;
            Texts = new string[matches.Count - 1];
            for (var i = 1; i < matches.Count; i++)
            {
                m = matches[i];
                if (!m.Success) continue;
                Texts[i - 1] = m.Groups[4].Value;
            }
        }

        public string Trans(EddNpcTalk eddNpcTalk)
        {
            var trans = $"{Indent}{FuncName} {Char} \"{eddNpcTalk.Name}\" {{\n{Indent}\t";
            trans += EddTexts2ClmText(eddNpcTalk.Texts, Indent, Texts, RawText);
            trans += $"{Indent}}}\n";
            return CheckText(trans, RawText);
        }
    }

    public class ClmTextTalk
    {
        public const string FuncName = "TextTalk";

        /*
            TextTalk char[85] {
                #6PSilence!{wait}
            } {
                You might've had your way before, but that's not how
                it's gonna go this time!{wait}
            }
        */
        public string Indent;
        public string[] Texts;
        public string Char; //char[12] / self
        public string RawText;

        public ClmTextTalk(string clmText)
        {
            RawText = clmText;
            var matches = Regex.Matches(clmText, """
                                                 (\s+)TextTalk (.*?) |\{\n([\S\s]*?)\n\s+}
                                                 """);
            if (matches.Count == 0) throw new Exception($"{clmText} regex error");
            var m = matches.First();
            Indent = m.Groups[1].Value;
            Char = m.Groups[2].Value;
            Texts = new string[matches.Count - 1];
            for (var i = 1; i < matches.Count; i++)
            {
                m = matches[i];
                Texts[i - 1] = m.Groups[3].Value;
            }
        }

        public string Trans(EddChrTalk eddCharTalk)
        {
            var trans = $"{Indent}{FuncName} {Char} {{\n{Indent}\t";
            trans += EddTexts2ClmText(eddCharTalk.Texts, Indent, Texts, RawText);
            trans += $"{Indent}}}\n";
            return CheckText(trans, RawText);
        }
    }

    public class ClmTextMessage
    {
        public const string FuncName = "TextMessage";
        public string Char; //null

        /*
            TextMessage null {
                {color 0}Yes, indeed!{wait}
            } {
                #4POn this historic day ten years ago, this fine
                company...{wait}
            }
         */
        public string Indent;
        public string[] Texts;
        public string RawText;

        public ClmTextMessage(string clmText)
        {
            RawText = clmText;
            var matches = Regex.Matches(clmText, """
                                                 (\s+)TextMessage (.*?) |\{\n([\S\s]*?)\n\s+\}
                                                 """);
            //head
            if (matches.Count == 0) throw new Exception($"{clmText} regex error");
            var m = matches.First();
            Indent = m.Groups[1].Value;
            Char = m.Groups[2].Value;
            Texts = new string[matches.Count - 1];
            for (var i = 1; i < matches.Count; i++)
            {
                m = matches[i];
                Texts[i - 1] = m.Groups[3].Value;
            }
        }

        public string Trans(EddAnonymousTalk eddAnonymousTalk)
        {
            var trans = $"{Indent}{FuncName} {Char} {{\n{Indent}\t";
            trans += EddTexts2ClmText(eddAnonymousTalk.Texts, Indent, Texts, RawText);
            trans += $"{Indent}}}\n";
            return CheckText(trans, RawText);
        }
    }

    public class EddMenu
    {
        /*
    Menu(
        0,
        10,
        10,
        1,
        (
            "【读第１卷】\x01",      # 0
            "【读第２卷】\x01",      # 1
            "【读第３卷】\x01",      # 2
            "【读第４卷】\x01",      # 3
            "【读第５卷】\x01",      # 4
            "【读第６卷】\x01",      # 5
            "【放弃】\x01",          # 6
        )
    )
         */

        public const string FuncName = "Menu(";
        public string[] Texts;

        public EddMenu(string eddText)
        {
            eddText = eddText.Replace("\r\n", "\n");
            var matches = Regex.Matches(eddText, """
                                                 \"(.*?)\",
                                                 """);
            Texts = new string[matches.Count];
            for (var i = 0; i < matches.Count; i++)
            {
                var m = matches[i];
                Texts[i] = m.Groups[1].Value;
            }
        }
    }

    public class EddNpcTalk
    {
        /*
            NpcTalk(
                0x11,
                "主办人",
                (
                    "#5P今日承蒙各位光临\x01",
                    "我们康莱特公司举办的宴会，\x01",
                    "在此表示衷心感谢。\x02",
                )
            )
         */
        public const string FuncName = "NpcTalk";
        public string Char; //0x109
        public string Name; //戴面具的绅士
        public string[] Texts;

        public EddNpcTalk(string eddText)
        {
            eddText = eddText.Replace("\r\n", "\n");
            var matches = Regex.Matches(eddText, """
                                                 \s+NpcTalk\(\n([\s\S]*?),\n\s+\"([\s\S]*?)\",|\n(.*),
                                                 """);
            //head
            if (matches.Count == 0) throw new Exception($"{eddText} regex error");
            var m = matches.First();
            Char = m.Groups[1].Value;
            Name = m.Groups[2].Value;
            Texts = new string[matches.Count - 1];
            for (var i = 1; i < matches.Count; i++)
            {
                m = matches[i];
                Texts[i - 1] = m.Groups[3].Value;
            }
        }
    }

    public class EddChrTalk
    {
        /*
            ChrTalk(
                0x10,
                (
                    scpstr(SCPSTR_CODE_COLOR, 0x2),
                    "#5P呵呵……\x01",
                    "还挺能给人找乐子的。\x02\x03",
                    "不过，事情现在才刚开始呢，\x01",
                    "凯文·格拉汉姆──\x07\x00\x02",
                )
            )
         */
        public const string FuncName = "ChrTalk";
        public string Char;
        public string[] Texts;

        public EddChrTalk(string eddText)
        {
            eddText = eddText.Replace("\r\n", "\n");
            var matches = Regex.Matches(eddText, """
                                                 \s+ChrTalk\(\n([\s\S]*?),\n\s+\(|\n(.*),
                                                 """);
            if (matches.Count == 0) throw new Exception($"{eddText} regex error");
            var m = matches.First();
            Char = m.Groups[1].Value;
            Texts = new string[matches.Count - 1];
            for (var i = 1; i < matches.Count; i++)
            {
                m = matches[i];
                Texts[i - 1] = m.Groups[2].Value;
            }
        }
    }

    public class EddAnonymousTalk
    {
        /*
            AnonymousTalk(
                (
                    "宝箱里装有",
                    scpstr(SCPSTR_CODE_ITEM, 0x1F6),
                    scpstr(SCPSTR_CODE_COLOR, 0x0),
                    "。\x01",
                    "不过现有的数量太多，",
                    scpstr(SCPSTR_CODE_ITEM, 0x1F6),
                    scpstr(SCPSTR_CODE_COLOR, 0x0),
                    "不能再拿更多了。\x02",
                )
            )
         */
        public const string FuncName = "AnonymousTalk";
        public string[] Texts;

        public EddAnonymousTalk(string eddText)
        {
            eddText = eddText.Replace("\r\n", "\n");
            var matches = Regex.Matches(eddText, """
                                                 \n(.*),
                                                 """);
            if (matches.Count == 0) throw new Exception($"{eddText} regex error");
            Texts = new string[matches.Count];
            for (var i = 0; i < matches.Count; i++)
            {
                var m = matches[i];
                Texts[i] = m.Groups[1].Value;
            }
        }
    }
}