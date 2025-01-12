#region

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Threading;

#endregion

namespace ED63Trans.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public static MainWindowViewModel? Instance { get; private set; }
    [ObservableProperty] private float _halfCharXOffset;
    [ObservableProperty] private float _halfCharYOffset = 6;
    [ObservableProperty] private float _charXOffset;
    [ObservableProperty] private float _charYOffset = 6;
    private SoraTrans? _clm;
    [ObservableProperty] private string _clmCurrentParagraph = "";
    [ObservableProperty] private int _clmCurrentParagraphIndex;
    [ObservableProperty] private int _clmMatchCount;
    [ObservableProperty] private string _eddCurrentParagraph = "";
    [ObservableProperty] private int _eddCurrentParagraphIndex;
    [ObservableProperty] private int _eddMatchCount;
    [ObservableProperty] private string _fontName = "霞鹜文楷 GB";
    [ObservableProperty] private int _fontSize = 38;
    [ObservableProperty] private float _halfCharHeightScale = 1f;
    [ObservableProperty] private int _halfCharSize = 38;
    [ObservableProperty] private float _halfCharWidthScale = 0.7f;
    [ObservableProperty] private float _wideHalfCharWidthScale = 0.57f; //针对字符 W、M 的缩放, 这玩意儿太宽了, 其他字符又太窄了
    [ObservableProperty] private bool _isBold = false;

    [ObservableProperty] private string _log = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsTranslating))]
    private string _openedClm = "";

    [ObservableProperty] private int _pixelSize = 40;

    [ObservableProperty] private byte[]? _previewData;
    [ObservableProperty] private bool _previewDataIsHalfChar;

    [ObservableProperty] private string _replaceChars = "";
    [ObservableProperty] private int _replaceCharsLineCount;
    private string _translatedParagraph = "";

    public string TranslatedParagraph
    {
        get => _translatedParagraph;
        set
        {
            var v = value.Replace("\r", "");
            if (string.Equals(_translatedParagraph, v, StringComparison.Ordinal)) return;
            _translatedParagraph = v;
            OnTranslatedParagraphChanged(v);
            OnPropertyChanged();
        }
    }
    [ObservableProperty] private string? _yltScriptPath = "";
    [ObservableProperty] private string? _xseedGamePath = "";
    [ObservableProperty] private string? _transScriptSavePath = "";
    [ObservableProperty] private string? _aureoleToolPath = "";
    
    [ObservableProperty] private string? _transParaCount = "";
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MatchCountsValidString), nameof(MatchCountsValidColor))]
    private bool _isMatchCountsValid;

    public MsTransViewModel MsTransViewModel { get; } = new();
    public string MatchCountsValidString => IsMatchCountsValid ? "成功" : "错误";
    public IBrush MatchCountsValidColor => IsMatchCountsValid ? Brushes.Green : Brushes.Red;
    public bool IsTranslating => !string.IsNullOrEmpty(OpenedClm);

    public const string replaceFileName = "F:\\源码\\C#\\ED63Trans\\ED63Trans\\replace.txt";

    public const string XseedED21Dir = "E:\\SteamLibrary\\steamapps\\common\\Trails in the Sky the 3rd\\ED6_DT21";
    public List<string> XseedED21DirClms = [];

    public MainWindowViewModel()
    {
        
        Instance = this;

        try
        {
            if (File.Exists("settings.json"))
            {
                var dic = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText("settings.json"));
                _xseedGamePath = dic?["XSEED"];
                _yltScriptPath = dic?["YLT"];
                _transScriptSavePath = dic?["Trans"];
                _aureoleToolPath = dic?["Aureole"];
            }

            if (!File.Exists(replaceFileName))
                File.WriteAllText(replaceFileName,
                    """
                    889F=·
                    88A0=…
                    88A1=⊙
                    """);
            ReplaceChars = File.ReadAllText(replaceFileName);
            XseedED21DirClms = Directory.EnumerateFiles(XseedED21Dir, "*.clm").ToList();
        }
        catch
        {
            //
        }

        if (!File.Exists("save.txt")) return;
        var file = File.ReadAllText("save.txt");
        if (File.Exists(file))
        {
            OpenClmFile(file);
        }

        // 更改字体尺寸
        // var files = Directory.EnumerateFiles("C:\\Users\\Jelly\\Desktop\\ED6SCRIPT\\ED6_DT21_初版1", "*.clm");
        //
        // foreach (var f in files)
        // {
        //     if (f.Contains("raw_"))
        //         continue;
        //     
        //     var text = File.ReadAllText(f);
        //     if (!Regex.IsMatch(text, "#[4-5]S"))
        //         continue;
        //     var new_text= Regex.Replace(text, "#[4-5]S", "#3S");
        //     var outFile = "C:\\Users\\Jelly\\Desktop\\ED6SCRIPT\\ED6_DT21_尺寸修正2\\"+Path.GetFileName(f);
        //     File.WriteAllText(outFile,new_text);
        //     var tool = Path.Combine(AureoleToolPath ?? "", "calmare.exe");
        //     ExecuteTool(tool, $"\"{outFile}\"");
        // }




        // ConvertSn_2(
        //     "C:\\Users\\Jelly\\Desktop\\ED6SCRIPT\\ED6_DT21_初版1\\raw_u7002.clm",
        //     "C:\\Users\\Jelly\\Desktop\\ED6SCRIPT\\ED6_DT21_初版1\\u7002.clm");

        //foreach (var sn in Directory.EnumerateFiles("E:\\SteamLibrary\\steamapps\\common\\Trails in the Sky the 3rd\\ED6_DT21\\raw"))
        //{
        //    Process.Start(Path.Combine(AureoleToolPath, "calmare.exe"), $"\"{sn}\"");
        //   // File.Move(sn,
        //   //Path.Combine("F:\\源码\\C#\\ED63Trans\\ed63rd_cn\\ED6_DT21\\原版en",
        //   //Path.GetFileName(sn).Replace(" ", "").ToLower()));
        //}

        //OpenClmFile("E:\\SteamLibrary\\steamapps\\common\\Trails in the Sky the 3rd\\ED6_DT21\\e1110.clm");

        //check chars txt file

        // var files = Directory.EnumerateFiles("F:\\源码\\C#\\ED63Trans\\DatTrans\\bin\\Debug\\net9.0", "*_chars.txt");

        //foreach (var f in files)
        //{
        //    var chars2 = File.ReadAllText(f);
        //    CheckChineseAndAddReplaceChars(chars2);
        //}

        //var chars = File.ReadAllText("E:\\SteamLibrary\\steamapps\\common\\Trails in the Sky the 3rd\\exerdata.json");
        //CheckChineseAndAddReplaceChars(chars);
        //SaveReplaceChars();
    }

    partial void OnReplaceCharsChanged(string value)
    {
        ReplaceCharsLineCount = value.Split("\n").Length;
    }

    [RelayCommand]
    private async Task BeginTransPara()
    {
        if (!int.TryParse(TransParaCount, out var count))
            return;
        TransCurrentParagraph();
        for (int i = 0; i < count; i++)
        {
            NextTranslationMatch();
            await Task.Delay(100);
            TransCurrentParagraph();
            await Task.Delay(150);
        }

        TransParaCount = "";
    }
    [RelayCommand]
    private void SaveSettings()
    {
        var dic = new Dictionary<string, string>
        {
            ["XSEED"] = XseedGamePath ?? "",
            ["YLT"] = YltScriptPath ?? "",
            ["Trans"] = TransScriptSavePath ?? "",
            ["Aureole"] = AureoleToolPath ?? ""
        };
        using var file = File.OpenWrite("settings.json");
        JsonSerializer.Serialize(file, dic);
        file.Flush();
    }

    [RelayCommand]
    private void DropClmFile(DragEventArgs e)
    {
        var dropFile = e.Data.GetFiles()?.First();
        if (dropFile != null && dropFile.Path.LocalPath.ToLower().EndsWith(".clm"))
        {
            var file = dropFile.Path.LocalPath;
            OpenClmFile(file);
        }
    }

    private void OpenClmFile(string file)
    {
        try
        {
            if (_clm != null)
                CloseClmFile();

            if (YltScriptPath == null)
            {
                AddLog("未设置目录.");
                return;
            }

            OpenedClm = file;
            File.WriteAllText("save.txt", file);
            var name = Path.GetFileNameWithoutExtension(file);
            var edMatch = Regex.Match(file, @"ED6_DT(\d+)");

            var dat = $"ED6_DT{edMatch.Groups[1].Value}";
            _clm = new SoraTrans(file, Path.Combine(YltScriptPath, dat, name.Trim() + ".py"))
            {
                DATName = dat
            };
            ClmMatchCount = _clm.ClmMatches.Count > 0 ? _clm.ClmMatches.Count : 0;
            EddMatchCount = _clm.EddMatches.Count > 0 ? _clm.EddMatches.Count : 0;
            _clm.AutoTrans();
            ClmCurrentParagraphIndex = ClmMatchCount > 0 ? 1 : 0;
            EddCurrentParagraphIndex = EddMatchCount > 0 ? 1 : 0;
            IsMatchCountsValid = ClmMatchCount == EddMatchCount;
        }
        catch (Exception e)
        {
            AddLog(e.Message);
        }
    }

    [RelayCommand]
    private void SaveTransClm()
    {
        if (_clm == null) return;
        var transClm = _clm.ApplyTrans();
        var file = Path.Combine(TransScriptSavePath, _clm.DATName);
        if (!Directory.Exists(file)) Directory.CreateDirectory(file);
        file = Path.Combine(file, "raw_" + Path.GetFileName(_clm.ClmFile));
        File.WriteAllText(file, transClm);
    }

    [RelayCommand]
    private void TransCurrentParagraph()
    {
        try
        {
            if (_clm is not { ClmMatches.Count: > 0 }) return;
            var index = ClmCurrentParagraphIndex - 1;
            if (index < 0 || index > _clm.ClmMatches.Count) return;
            SoraTrans.autoTransIndex = ClmCurrentParagraphIndex;
            TranslatedParagraph = SoraTrans.EddText2ClmText(EddCurrentParagraph, ClmCurrentParagraph);
            SoraTrans.autoTransIndex = -1;
        }
        catch (Exception e)
        {
            AddLog(e.Message);
        }
    }

    [RelayCommand]
    private void CheckCurrentTransParagraph()
    {
        try
        {
            if (!SoraTrans.CheckCode(TranslatedParagraph, ClmCurrentParagraph))
            {
                AddLog("check code 检测错误.");
            }
            if (!SoraTrans.CheckIndent(TranslatedParagraph, ClmCurrentParagraph))
            {
                AddLog("check indent 检测错误.");
            }
        }
        catch (Exception e)
        {
            AddLog(e.Message);
        }
    }
    
    void OnTranslatedParagraphChanged(string value)
    {
        if (_clm is not { ClmMatches.Count: > 0 }) return;
        _clm.TransTexts[ClmCurrentParagraphIndex - 1] = value;
  
    }

    [RelayCommand]
    public void SaveReplaceChars()
    {
        File.WriteAllText(replaceFileName, ReplaceChars);
    }

    private void ConvertSn_2(string file,string outFile)
    {
        try
        {
            var text = File.ReadAllText(file);
            CheckChineseAndAddReplaceChars(text);
            SaveReplaceChars();
            var replaced = ReplaceClmChars(text);
            File.WriteAllText(outFile, replaced);
            var tool = Path.Combine(AureoleToolPath ?? "", "calmare.exe");
            if (!File.Exists(tool)) throw new Exception("calmare.exe文件找不到.");
            ExecuteTool(tool, $"\"{outFile}\"");
        }
        catch (Exception e)
        {
            AddLog(e.Message);
        }
    }

    [RelayCommand]
    private void ConvertSn()
    {
        if (_clm == null || TransScriptSavePath == null) return;
        try
        {
            AddLog($"开始转化{Path.GetFileName(_clm.ClmFile)}");
            var file = Path.Combine(TransScriptSavePath, _clm.DATName, "raw_" + Path.GetFileName(_clm.ClmFile));
            var text = File.ReadAllText(file);
            CheckChineseAndAddReplaceChars(text);
            SaveReplaceChars();
            var replaced = ReplaceClmChars(text);
            var replacedFile = Path.Combine(TransScriptSavePath, _clm.DATName, Path.GetFileName(_clm.ClmFile));
            File.WriteAllText(replacedFile, replaced);
            var tool = Path.Combine(AureoleToolPath ?? "", "calmare.exe");
            if (!File.Exists(tool)) throw new Exception("calmare.exe文件找不到.");
            ExecuteTool(tool, $"\"{replacedFile}\"");
        }
        catch (Exception e)
        {
            AddLog(e.Message);
        }
    }

    [RelayCommand]
    private async Task LoadOtherEdd()
    {
        if (_clm == null) return;
        var dialog = new ContentDialog()
        {
            Title = "输入路径",
            PrimaryButtonText = "确定",
            CloseButtonText = "关闭",
        };
        var tb = new TextBox()
        {
            Text = YltScriptPath + "\\ED6_DT21",
            MinWidth = 450
        };
        dialog.Content = tb;
        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary) return;
        var file = tb.Text;
        if (!File.Exists(file)) return;
        _clm.EddMatches.Clear();
        _clm.LoadEdd(file);
        EddMatchCount = _clm.EddMatches.Count;
        if (EddMatchCount > 0)
            EddCurrentParagraphIndex = 1;
    }
    [RelayCommand]
    private async Task  SearchEddText()
    {
        if (_clm == null) return;
        var dialog = new ContentDialog
        {
            Title = "搜索娱乐通文本",
            PrimaryButtonText = "确定",
            CloseButtonText = "关闭",
        };
        var tb = new TextBox()
        {
            MinWidth = 450
        };
        dialog.Content = tb;
        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary) return;
        var text = tb.Text;
        if (string.IsNullOrWhiteSpace(text)) return;
        var index= _clm.EddMatches.FindIndex(x => x.Text.Contains(text));
        if (index == -1)
        {
            AddLog("未搜索到文本");
            return;
        }

        index++;
        EddCurrentParagraphIndex=index;
        AddLog($"已搜索到: {index}");
    }
    [RelayCommand]
    private void AddSnFileToGame()
    {
        if (_clm == null || TransScriptSavePath == null) return;
        try
        {
            var file = Path.Combine(TransScriptSavePath, _clm.DATName,
                Path.GetFileNameWithoutExtension(_clm.ClmFile) + "._sn");
            if (!File.Exists(file))
            {
                AddLog($"文件不存在: {file}");
                return;
            }

            var factoria = Path.Combine(AureoleToolPath ?? "", "factoria.exe");
            if (!File.Exists(factoria))
            {
                AddLog($"factoria不存在: {factoria}");
                return;
            }

            var dat = Path.Combine(XseedGamePath ?? "", _clm.DATName + ".dir");
            if (!File.Exists(dat))
            {
                AddLog($"{_clm.DATName + ".dir"}不存在: {dat}");
                return;
            }

            ExecuteTool(factoria, $"add \"{dat}\" \"{file}\"");
        }
        catch (Exception e)
        {
            AddLog(e.Message);
        }
    }

    public void ExecuteTool(string tool, string argu)
    {
        var command = $"{tool} {argu}";
        var startInfo = new ProcessStartInfo("cmd", "/c " + command)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = false // 隐藏CMD窗口
        };
        try
        {
            using var process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (tool.Contains("factoria"))
            {
                if (Regex.IsMatch(output, @"added (.*?)$"))
                    AddLog($"{Path.GetFileNameWithoutExtension(tool)}: {Regex.Match(output, @"added (.*?)$").Value}");
                else
                    AddLog($"{Path.GetFileNameWithoutExtension(tool)}: {output}");
            }
            else
            {
                AddLog($"{Path.GetFileNameWithoutExtension(tool)}: {output}");
            }

            if (!string.IsNullOrEmpty(error)) AddLog($"{Path.GetFileNameWithoutExtension(tool)}错误: {error}");
        }
        catch (Exception ex)
        {
            AddLog(ex.Message);
        }
    }

    public string ReplaceClmChars(string text)
    {
        var maches = Regex.Matches(ReplaceChars, @"(\S+)=(\S)");
        Dictionary<string, string> dic = [];
        foreach (Match m in maches)
        {
            var code = Convert.ToUInt16(m.Groups[1].Value, 16);
            var ch = m.Groups[2].Value;
            var sjisChar = SoraFont.GetSJISChar(code);
            dic[ch] = sjisChar;
        }

        var pattern = string.Join("|", dic.Keys.Select(Regex.Escape));
        text = Regex.Replace(text, pattern, m => dic[Regex.Unescape(m.Value)]);
        return text;
    }

    public void CheckChineseAndAddReplaceChars(string text)
    {
        var pattern = @"[^\x00-\xff]";

        (int min, int max) sjis1 = (0x889F, 0x9FFC);
        (int min, int max) sjis2 = (0xE040, 0xEAA4);
        var code = sjis1.min;
        bool isNew;
        var matches = Regex.Matches(text, pattern);

        foreach (Match m in matches)
        {
            var ch = m.Value;
            if (ch == "㈱") //游戏内部处理
                continue;
            //是否在sjis内
            var sjisCode = SoraFont.GetSJISCode(ch.ToCharArray().First());

            if (sjisCode > 0 && sjisCode < sjis1.min) continue;
            //先看replace有没有这个字
            var has = ReplaceChars.Contains($"={ch}");
            if (has) continue;
            //再获取没有使用过的code
            isNew = false;
            if (code <= sjis1.max)
                for (var i = code; i <= sjis1.max; i++)
                {
                    //游戏没有使用的编码
                    if (i is 0x985D)
                        continue;

                    if (i is >= 0x9873 and <= 0x989e)
                        continue;

                    var b = BitConverter.GetBytes(i)[0];
                    if (b is < 0x40 or > 0xFC or 0X7F)
                        continue;
                    if (ReplaceChars.Contains($"{i:X}="))
                        continue;
                    code = i;
                    isNew = true;
                    break;
                }

            if (code == 0)
            {
                code = sjis2.min;
                isNew = false;
            }

            if (!isNew)
                for (var i = code; i <= sjis2.max; i++)
                {
                    var b = BitConverter.GetBytes(i)[0];
                    if (b is < 0x40 or > 0xFC) continue;
                    if (ReplaceChars.Contains($"{i:X}="))
                        continue;
                    code = i;
                    isNew = true;
                    break;
                }

            if (!isNew)
            {
                AddLog("SJIS汉字编码用完哩");
                return;
            }

            if (ReplaceChars == "")
                ReplaceChars += $"{code:X}={ch}";
            else
                ReplaceChars += $"\n{code:X}={ch}";

            code++;
        }
    }

    [RelayCommand]
    private void LocateClmFile()
    {
        if (_clm == null) return;
        Process.Start("explorer.exe", $"/select, \"{_clm.ClmFile}\"");
    }

 
    [RelayCommand]
    private void CloseClmFile()
    {
        OpenedClm = "";
        _clm = null;
        ClmMatchCount = 0;
        ClmCurrentParagraphIndex = 0;
        EddCurrentParagraphIndex = 0;
        TranslatedParagraph = "";
        ClmCurrentParagraph = "";
        EddCurrentParagraph = "";
    }
    
    [RelayCommand]
    private void NextClmFile()
    {
        var index = 0;
        if (_clm != null)
        {
            index = XseedED21DirClms.IndexOf(_clm.ClmFile);
            if (index < XseedED21DirClms.Count - 1)
                index++;
        }

        OpenClmFile(XseedED21DirClms[index]);
    }

    [RelayCommand]
    private void PrevClmFile()
    {
        var index = 0;
        if (_clm != null)
        {
            index = XseedED21DirClms.IndexOf(_clm.ClmFile);
            index--;
            if (index < 0)
                return;
        }

        OpenClmFile(XseedED21DirClms[index]);
    }

    [RelayCommand]
    private void OneKey2Trans()
    {
        if (ClmMatchCount == 0 || EddMatchCount == 0)
        {
            AddLog("不能直接汉化.");
            return;
        }

        SaveTransClm();
        ConvertSn();
        //AddSnFileToGame();
    }

    partial void OnClmCurrentParagraphIndexChanged(int value)
    {
        if (_clm == null || _clm.ClmMatches.Count == 0) return;
        var index = value - 1;
        if (index < 0)
            index = 0;
        if (index >= _clm.ClmMatches.Count) index = _clm.ClmMatches.Count - 1;
        ClmCurrentParagraph = _clm.ClmMatches[index].Text;
        TranslatedParagraph = _clm.TransTexts[index];
    }

    partial void OnEddCurrentParagraphIndexChanged(int value)
    {
        if (_clm == null || _clm.EddMatches.Count == 0) return;
        var index = value - 1;
        if (index < 0)
            index = 0;
        if (index >= _clm.EddMatches.Count) index = _clm.EddMatches.Count - 1;
        EddCurrentParagraph = _clm.EddMatches[index].Text;
    }

    [RelayCommand]
    private void JmpTranslationMatch(string text)
    {
        try
        {
            var index = Convert.ToInt32(text);
            if (index <= 0 || index > ClmMatchCount) return;
            ClmCurrentParagraphIndex = index;
            EddCurrentParagraphIndex = index;
        }
        catch
        {
            //
        }
    }

    [RelayCommand]
    private void PrevTranslationMatch()
    {
        PrevClmParagraph();
        PrevEddParagraph();
    }

    [RelayCommand]
    private void NextTranslationMatch()
    {
        NextClmParagraph();
        NextEddParagraph();
    }

    [RelayCommand]
    private void PrevClmParagraph()
    {
        if (ClmCurrentParagraphIndex > 1)
        {
            ClmCurrentParagraphIndex--;
        }
        else
        {
            ClmCurrentParagraphIndex = ClmMatchCount;
        }
    }

    [RelayCommand]
    private void NextClmParagraph()
    {
        if (ClmCurrentParagraphIndex < ClmMatchCount)
        {
            ClmCurrentParagraphIndex++;
        }
        else
        {
            ClmCurrentParagraphIndex = 1;
        }
    }

    [RelayCommand]
    private void PrevEddParagraph()
    {
        if (EddCurrentParagraphIndex > 1)
        {
            EddCurrentParagraphIndex--;
        }
        else
        {
            EddCurrentParagraphIndex = EddMatchCount;
        }
    }

    [RelayCommand]
    private void NextEddParagraph()
    {
        if (EddCurrentParagraphIndex < EddMatchCount)
        {
            EddCurrentParagraphIndex++;
        }
        else
        {
            EddCurrentParagraphIndex = 1;
        }
    }

    [RelayCommand]
    private void ClearLog()
    {
        Log = "";
    }

    [RelayCommand]
    private void OpenFontDir()
    {
        if (!Directory.Exists("fonts")) Directory.CreateDirectory("fonts");
        Process.Start("explorer.exe", "fonts");
    }

    [RelayCommand]
    private void AddCurrentPixelSizeFontFileToGame()
    {
        var fontName = $"fonts\\font{PixelSize}._da";
        if (!File.Exists(fontName))
        {
            AddLog($"文件不存在: {fontName}");
            return;
        }

        var factoria = Path.Combine(AureoleToolPath ?? "", "factoria.exe");
        if (!File.Exists(factoria))
        {
            AddLog($"factoria不存在: {factoria}");
            return;
        }

        var dat = Path.Combine(XseedGamePath ?? "", "ED6_DT20.dir");
        if (!File.Exists(dat))
        {
            AddLog($"ED6_DT20.dir不存在: {dat}");
            return;
        }

        ExecuteTool(factoria, $"add \"{dat}\" \"{fontName}\"");
    }

    [RelayCommand]
    private void Preview(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            PreviewData = null;
            return;
        }

        var font = new SoraFont
        {
            FontSize = FontSize,
            FontName = FontName,
            IsBold = IsBold,
            PixelSize = PixelSize,
            HalfCharWidthScale = HalfCharWidthScale,
            HalfCharHeightScale = HalfCharHeightScale,
            HalfCharSize = HalfCharSize,
            HalfCharXOffset = HalfCharXOffset,
            HalfCharYOffset = HalfCharYOffset,
            CharXOffset = CharXOffset,
            CharYOffset = CharYOffset,
            WideHalfCharWidthScale = WideHalfCharWidthScale
        };
        var c = text.ToCharArray().First();
        PreviewDataIsHalfChar = font.IsHalfChar(c);
        var data = font.CreateChar(c, ReplaceChars, AddLog);
        PreviewData = data;
    }

    public void AddLog(string text)
    {
        Log += $"[{DateTime.Now:HH:mm:ss}]: {text}\n";
    }

    [RelayCommand]
    private void CreateSoraFont()
    {
        var font = new SoraFont
        {
            FontSize = FontSize,
            FontName = FontName,
            IsBold = IsBold,
            PixelSize = PixelSize,
            HalfCharWidthScale = HalfCharWidthScale,
            HalfCharHeightScale = HalfCharHeightScale,
            HalfCharSize = HalfCharSize,
            HalfCharXOffset = HalfCharXOffset,
            HalfCharYOffset = HalfCharYOffset,
            CharXOffset = CharXOffset,
            CharYOffset = CharYOffset,
            WideHalfCharWidthScale = WideHalfCharWidthScale
        };
        var data = font.Create(ReplaceChars, AddLog);
        if (!Directory.Exists("fonts")) Directory.CreateDirectory("fonts");
        var fontName = $"fonts\\font{PixelSize}._da";
        File.WriteAllBytes(fontName, data);
    }
}