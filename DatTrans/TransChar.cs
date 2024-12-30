#region

using System.Text;
using System.Text.RegularExpressions;

#endregion

namespace DatTrans;

public class TransChar
{
    public static readonly Encoding SjisEncoding = Encoding.GetEncoding("shift_jis");
    public static readonly Encoding GbkEncoding = Encoding.GetEncoding("GBK");
    public static string ReplaceListFileName = "";

    public static string GetSJISChar(int code)
    {
        if (code <= 255) return SjisEncoding.GetString([(byte)code]);
        var charBytes = BitConverter.GetBytes((ushort)code).Reverse().ToArray();
        return SjisEncoding.GetString(charBytes);
    }

    public static string ReplaceClmChars(string text)
    {
        var txt = File.ReadAllText(ReplaceListFileName);
        var maches = Regex.Matches(txt, @"(\S+)=(\S)");
        Dictionary<string, string> dic = [];
        foreach (Match m in maches)
        {
            var code = Convert.ToUInt16(m.Groups[1].Value, 16);
            var ch = m.Groups[2].Value;
            var sjisChar = GetSJISChar(code);
            dic[ch] = sjisChar;
        }

        var pattern = string.Join("|", dic.Keys.Select(Regex.Escape));
        text = Regex.Replace(text, pattern, m => dic[Regex.Unescape(m.Value)]);
        return text;
    }
}