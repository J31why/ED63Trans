using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;

namespace ED63Trans.ViewModels;
public partial class MsText : ViewModelBase
{
    [ObservableProperty]  private ushort _offset = 0;
    [ObservableProperty]  private string _text = "";
    [ObservableProperty]  private byte[] _bytes = [];
}
public partial class MsTransViewModel: ViewModelBase
{
    private static Encoding gbk = Encoding.GetEncoding(936);

    [ObservableProperty] private string _xseedText="";
    [ObservableProperty] private string _yltText="";
    [ObservableProperty] private string _myText="";
    [ObservableProperty] private string _currentDat="";
    [ObservableProperty] private ObservableCollection<MsText> _xseedList=[];
    [ObservableProperty] private ObservableCollection<MsText> _yltList=[];
    private string DatFile = "";
    private List<string> _msDatFiles;
    public List<byte[]> BanBytes = [];
    public MsTransViewModel()
    {
        _msDatFiles =Directory.EnumerateFiles("E:\\SteamLibrary\\steamapps\\common\\Trails in the Sky the 3rd\\ED6_DT30 - 副本").ToList();
        
        _currentDat = "ms15060._dt";
        var index = _msDatFiles.FindIndex(x => x.Contains(_currentDat));
        if (index == -1) return;
        var f1 = _msDatFiles[index];
        var f2 = $"E:\\Games\\ED63RD\\ED_SORA3\\ED6_DT30\\{Path.GetFileName(f1)}";
        GetTexts(f1,f2);
    }
    public static IEnumerable<int> IndexOf(byte[] source, int start, byte[] pattern)
    {
        for (int i = start; i < source.Length; i++)
        {
            if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
            {
                yield return i;
            }
        }
    }
    [RelayCommand]
    private void PrevMs()
    {
        
        var index = 0;
        if (!string.IsNullOrEmpty(CurrentDat))
            index = _msDatFiles.FindIndex(x => x.EndsWith(CurrentDat));
        index--;
        if (index <0)
            index = 0;
        Reset();
        var f1 = _msDatFiles[index];
        var f2 = $"E:\\Games\\ED63RD\\ED_SORA3\\ED6_DT30\\{Path.GetFileName(f1)}";
        GetTexts(f1,f2);
    }

    private string str = "";
    [RelayCommand]
    private void NextMs()
    {
        var index = 0;
        if (!string.IsNullOrEmpty(CurrentDat))
            index = _msDatFiles.FindIndex(x => x.EndsWith(CurrentDat));
        index++;
        if (index >=_msDatFiles.Count)
            index = _msDatFiles.Count-1;
        Reset();
        var f1 = _msDatFiles[index];
        var f2 = $"E:\\Games\\ED63RD\\ED_SORA3\\ED6_DT30\\{Path.GetFileName(f1)}";
        GetTexts(f1,f2);
        
        // str = str+CurrentDat+"\n"+XseedText+YltText+"\n";
        // if (CurrentDat.Contains("32360"))
        // {
        //     File.WriteAllText("C:\\Users\\Jelly\\Desktop\\ED6SCRIPT\\ms.txt",str);
        // }
    }

    public void Reset()
    {
        XseedList.Clear();
        YltList.Clear();
        XseedText="";
        YltText="";
        MyText="";
        CurrentDat="";
    }
    [RelayCommand]
    public void ConvertMyText()
    {
        var lines = MyText.Replace("\r","").Split("\n");
        if (lines.Length != XseedList.Count || lines.Length == 0)
        {
            MainWindowViewModel.Instance!.AddLog("行数错误.");
            return;
        }
        using var xfs= new FileStream(DatFile, FileMode.Open);
        var path = Path.Combine(MainWindowViewModel.Instance!.TransScriptSavePath!, "Monster", CurrentDat);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        MainWindowViewModel.Instance.CheckChineseAndAddReplaceChars(MyText);
        MainWindowViewModel.Instance.SaveReplaceChars();
        using var myfs= new FileStream(path, FileMode.CreateNew);
        long start = 0;
        for (int i = 0; i < XseedList.Count; i++)
        {
            var item = XseedList[i];
            var myText = lines[i];
            xfs.Position = start;
            var buffer = new byte[item.Offset - start];
            xfs.ReadExactly(buffer);
            myfs.Write(buffer);
            var text = (string?)myText;
            if (text == null)
            {
                MainWindowViewModel.Instance.AddLog("转义解析错误.");
                return;
            }

            if (text.Contains("\r"))
            {
                MainWindowViewModel.Instance.AddLog("\\r错误.");
                return;
            }
            text = MainWindowViewModel.Instance.ReplaceClmChars(text);
            var bytes = SoraFont.JisEncoding.GetBytes(text);
            myfs.Write(bytes);
            myfs.WriteByte(0);
            start = xfs.Position + item.Bytes.Length + 1;
        }

        if (start < xfs.Length)
        {
            xfs.Position = start;
            var buffer = new byte[xfs.Length - start];
            xfs.ReadExactly(buffer);
            myfs.Write(buffer);
        }
        MainWindowViewModel.Instance!.AddLog($"{CurrentDat}已生成.");
    }
    
    public  bool ValidateBytes(byte[] bytes)
    {
        // if (bytes.Length==2&& BanBytes.Any(x => x.AsSpan().SequenceEqual(bytes)))
        // {
        //     return false;
        // }
        if (bytes.Contains((byte)0xff))
            return false;
        if (bytes[0] < 0x20 )
            return false;
        
        if (bytes is [0xF4, 1] or [0xA2, 1] or [0x90, 1] or [0xE8,3] or [104,66]or [0x8D,3]  or [0x4e,0x1])
            return false;
        return true;
    }
    public static bool Validate(string text)
    {
        if (text=="轧")
            return false;
        if (!Regex.IsMatch(text,"[a-zA-Z]{2,}|[\\u4e00-\\u9fa5]{1,}"))
            return false;
        return !Regex.IsMatch(text, 
            @"[\u0001-\u001f]"
            );
    }

    [RelayCommand]
    private void DeleteMs(MsText text)
    {
        var index = XseedList.IndexOf(text);
        if (index >= 0)
        {
            XseedList.RemoveAt(index);
        }
    }
    public  void GetTexts(string xseedFile, string yltFile)
    {
        DatFile = xseedFile;
        CurrentDat = Path.GetFileName(xseedFile);
        using var xfs= new FileStream(xseedFile, FileMode.Open);
        using var xreader = new BinaryReader(xfs);
        using var yfs= new FileStream(yltFile, FileMode.Open);
        using var yreader = new BinaryReader(yfs);
        xfs.Seek(0x8b, SeekOrigin.Begin);
        List<byte> bytes = [];
        List<MsText> xTexts = [], yTexts = [];
        while (xfs.Position < xfs.Length)
        {
            var pos = xfs.Position;
            byte b = 0;
            while ((b= xreader.ReadByte()) !=0 &&xfs.Position < xfs.Length)
            {
                bytes.Add(b);
                
                if (!ValidateBytes(bytes.ToArray()))
                {
                    bytes.Clear();
                    break;
                }
            }

          
            if (bytes.Count > 1)
            {
                var text = new MsText
                {
                    Offset = (ushort)pos,
                    Bytes = bytes.ToArray(),
                };
                text.Text = SoraFont.JisEncoding.GetString(text.Bytes);
                if (Validate(text.Text) && !Regex.IsMatch(text.Text, @"[\u4e00-\u9fa5]"))
                {
                    xTexts.Add(text);
                }
            }
            bytes.Clear();
        }
        yfs.Seek(0x8b, SeekOrigin.Begin);
        while (yfs.Position < yfs.Length-1)
        {
            var pos = yfs.Position;
            byte b = 0;
            while ((b= yreader.ReadByte()) !=0 &&yfs.Position < yfs.Length)
            {
                bytes.Add(b);
                
                if (!ValidateBytes(bytes.ToArray()))
                {
                    bytes.Clear();
                    break;
                }
            }
   
            if (bytes.Count > 1)
            {
                var text = new MsText
                {
                    Offset = (ushort)pos,
                    Bytes = bytes.ToArray(),
                };
                text.Text = gbk.GetString(text.Bytes);
                if (bytes.Count == 2 && text.Text is "?" or"\u3000")
                {
                    bytes.Clear();
                    continue;
                }
                
                if (Validate(text.Text))
                {
                    yTexts.Add(text);
                }
            }
            bytes.Clear();
        }

        if (xTexts.Count == yTexts.Count)
        {
            for (int i = 0; i < xTexts.Count; i++)
            {
                var xText = xTexts[i];
                var yText = yTexts[i];
                if (xText.Bytes.AsSpan().SequenceEqual(yText.Bytes))
                {
                    xTexts.RemoveAt(i);
                }
            }
        }

        XseedList.Clear();
        YltList.Clear();
        
        for (var index = 0; index < xTexts.Count; index++)
        {
            var text = xTexts[index];
            if (text.Text.Contains("R-Ty") || text.Text.ToLower().Contains("wind f") )
            {
                //Rod Angel ms14890
                
            }
            if (index == xTexts.Count - 1)
                XseedText += $"{Escape(text.Text)}";
            else        
                XseedText += $"{Escape(text.Text)}\n";
            XseedList.Add(text);
        }

        for (var index = 0; index < yTexts.Count; index++)
        {
            var text = yTexts[index];
            if (text.Text.Contains("钢鳞"))
            {
                
            }
            if (index == yTexts.Count - 1)
                YltText += $"{Escape(text.Text)}";
            else        
                YltText += $"{Escape(text.Text)}\n";
            YltList.Add(text);
        }

        if (XseedList.Count == YltList.Count)
        {
            MyText = YltText;
        }
            
        
    }

    public string Escape(string text)
    {
        //string literalText = JsonConvert.SerializeObject(text);
        return text;
    }
    
}