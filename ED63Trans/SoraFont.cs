#region

using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia.Media;

#endregion

namespace ED63Trans;

public class SoraFont
{
    public delegate void ErrorCallback(string message);

    private readonly (int min, int max)[] _sjisDoubleBytes =
    [
        //special signal
        (0x8140, 0x81F7), (0x8240, 0x82F1), (0x8340, 0x83D6), (0x8440, 0x84BE), (0x8740, 0x879C),
        //hanzi
        (0x889F, 0x9FFC), (0xE040, 0xEAA4)
    ];

    private readonly (int min, int max) _sjisSingleBytes = (0x20, 0xDF);
    public float CharXOffset;
    public float CharYOffset;
    public float HalfCharXOffset;
    public float HalfCharYOffset;
    public string FontName = "";
    public int FontSize;
    public float HalfCharHeightScale;
    public int HalfCharSize;
    public float HalfCharWidthScale;
    public bool IsBold;
    public int PixelSize;
    public float WideHalfCharWidthScale; //针对字符 W、M 的缩放, 这玩意儿太宽了, 其他字符又太窄了
    public static readonly Encoding JisEncoding = Encoding.GetEncoding("shift_jis");
    private readonly Dictionary<int, string> _replaceChars = [];


    private bool IsWideHalfChar(char c)
    {
        return c is 'W' or 'M' or '@' or '%';
    }

    private bool IsWideHalfChar(string c)
    {
        return IsWideHalfChar(c.ToCharArray().FirstOrDefault());
    }

    private void UpdateReplacedChars(string replaceList)
    {
        _replaceChars.Clear();
        replaceList = replaceList.Replace(" ", "");
        var matches = Regex.Matches(replaceList, @"(\S{2,4})=(\S)");
        foreach (Match match in matches)
        {
            var code = Convert.ToUInt16(match.Groups[1].Value, 16);
            var c = match.Groups[2].Value;
            _replaceChars[code] = c;
        }
    }

    public static string GetSJISChar(int code)
    {
        if (code <= 255) return JisEncoding.GetString([(byte)code]);
        var charBytes = BitConverter.GetBytes((ushort)code).Reverse().ToArray();
        return JisEncoding.GetString(charBytes);
    }

    public static int GetSJISCode(char c)
    {
        var code = 0;

        var bytes = JisEncoding.GetBytes(c.ToString());

        if (bytes.Length == 1)
            code = bytes.First();
        else
            code = BitConverter.ToUInt16(
                JisEncoding.GetBytes(c.ToString())
                    .Reverse()
                    .ToArray()
            );

        if (code == 0x3f && c != 0x3f) code = 0;

        return code;
    }

    public bool IsHalfChar(char c)
    {
        var isHalf = JisEncoding.GetByteCount(c.ToString()) == 1;
        var code = GetSJISCode(c);
        return code > 0 && isHalf;
    }

    public byte[] CreateChar(char c, string replaceList, ErrorCallback callback)
    {
        UpdateReplacedChars(replaceList);
        using var bitmap = new SKBitmap(PixelSize, PixelSize);
        using var canvas = new SKCanvas(bitmap);
        using var typeface = SKTypeface.FromFamilyName(FontName,
            IsBold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal, SKFontStyleWidth.Normal,
            SKFontStyleSlant.Upright);
        using var font = new SKFont(typeface, FontSize);
        font.Hinting = SKFontHinting.Full;
        using var paint = CreatePaint();
        var ms = new MemoryStream();
        try
        {
            var isHalf = IsHalfChar(c);
            var code = GetSJISCode(c);
            if (code == 0)
                code = -1;
            if (isHalf)
            {
                font.Size = HalfCharSize;
                canvas.Save();
                canvas.Scale(HalfCharWidthScale, HalfCharHeightScale);

                if (IsWideHalfChar(c))
                {
                    canvas.Restore();
                    canvas.Scale(WideHalfCharWidthScale, HalfCharHeightScale);
                }
            }

            font.GetFontMetrics(out var metrics);

            GenerateSoraChar(code, c.ToString(), PixelSize, isHalf, metrics, bitmap, canvas, font, paint, ref ms,
                callback);
            var bytes = ms.ToArray();
            return bytes;
        }
        finally
        {
            ms.Dispose();
        }
    }

    public byte[] Create(string replaceList, ErrorCallback callback)
    {
        UpdateReplacedChars(replaceList);
        using var bitmap = new SKBitmap(PixelSize, PixelSize);
        using var canvas = new SKCanvas(bitmap);
        canvas.Save();
        using var typeface = SKTypeface.FromFamilyName(FontName,
            IsBold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal, SKFontStyleWidth.Normal,
            SKFontStyleSlant.Upright);
        using var font = new SKFont(typeface, FontSize);
        font.Hinting = SKFontHinting.Full;
        using var paint = CreatePaint();
       
        var ms = new MemoryStream();
        //single byte
        font.Size = HalfCharSize;
        font.GetFontMetrics(out var metrics);
        canvas.Save();

        canvas.Scale(HalfCharWidthScale, HalfCharHeightScale);

        for (var i = _sjisSingleBytes.min; i <= _sjisSingleBytes.max; i++)
        {
            var ch = JisEncoding.GetString([(byte)i]);
            if (IsWideHalfChar(ch) )
            {
                canvas.Restore();
                canvas.Save();
                canvas.Scale(WideHalfCharWidthScale, HalfCharHeightScale);
                GenerateSoraChar(i, ch, PixelSize, true, metrics, bitmap, canvas, font, paint, ref ms, callback);
                canvas.Restore();
                canvas.Save();
                canvas.Scale(HalfCharWidthScale, HalfCharHeightScale);
                continue;
            }

            GenerateSoraChar(i, ch, PixelSize, true, metrics, bitmap, canvas, font, paint, ref ms, callback);
        }

        canvas.Restore();
        canvas.Save();
        //double byte
        font.Size = FontSize;
        font.GetFontMetrics(out metrics);
        foreach (var doubleBytes in _sjisDoubleBytes)
            for (var i = doubleBytes.min; i <= doubleBytes.max; i++)
            {
                var code = (ushort)i;
                var charBytes = BitConverter.GetBytes(code).Reverse().ToArray();
                var b = charBytes.Last();
                if (b is < 0x40 or > 0xFC) continue;

                var ch = _replaceChars.TryGetValue(code, out var replaceChar)
                    ? replaceChar
                    : JisEncoding.GetString(charBytes);

                GenerateSoraChar(i, ch, PixelSize, false, metrics, bitmap, canvas, font, paint, ref ms, callback);
            }

        var bytes = ms.ToArray();
        ms.Dispose();
        return bytes;
    }

    private SKPaint CreatePaint()
    {
        var kernel = new float[9] {
            0, -.1f,    0,
            -.1f, 1.4f, -.1f,
            0, -.1f,    0,
        };

        using var imageFilter = SKImageFilter.CreateMatrixConvolution(new SKSizeI(3, 3), kernel, 1.0F, 0.0F,
            new SKPointI(1, 1), SKShaderTileMode.Repeat, true);
        // 创建色彩矩阵来增加亮度

        var paint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true,
            //ImageFilter = imageFilter,
        };
        return paint;
    }
    private void GenerateSoraChar(int code, string ch, int pixelSize, bool isHalfChar, SKFontMetrics metrics,
        SKBitmap bitmap, SKCanvas canvas,
        SKFont font, SKPaint paint, ref MemoryStream data, ErrorCallback callback)
    {
        if (code is 0x7F or 0x80 or 0xA0) ch = "\u303f";
        font.MeasureText(ch, out var bounds);
        float x, y;

        if (isHalfChar)
        {
            if (code is 0x2f)   //  0x2f = /
            {
                var margin = new SKRect();
                margin.Left += ((float)pixelSize / 2 - bounds.Width * HalfCharWidthScale) / 2;
                if (margin.Left < 0)
                    margin.Left = 0;
                x = margin.Left;
            }
            else
            {
                x = -bounds.Left + HalfCharXOffset;
            }

            y = -metrics.Ascent - HalfCharYOffset;
            //检测
            if (IsWideHalfChar(ch))
            {
                if (bounds.Width * WideHalfCharWidthScale + HalfCharXOffset > (float)pixelSize / 2 ||
                    bounds.Height * HalfCharHeightScale  > pixelSize)
                    callback($"out of pixel bounds : {code}, {ch}, HalfChar.");
            }
            else if (bounds.Width * HalfCharWidthScale + HalfCharXOffset > (float)pixelSize / 2 ||
                     bounds.Height * HalfCharHeightScale  > pixelSize)
            {
                callback($"out of pixel bounds : {code}, {ch}, HalfChar.");
            }
        }
        else
        {
            ch = code switch
            {
                0x889F=>"・",
                0x88a1 => "\u266a",//⊙ = ♪
                0x81ef => "♥",//㈱ = ♥
                _ => ch
            };
            ch = ch switch
            {
                "\u247e" => "\u246a",//⑾ = ⑪
                "\u247f" => "\u246b",//⑿ = ⑫
                "\u2480" => "\u246c",//⒀ = ⑬
                "\u2481" => "\u246d",//⒁ = ⑭
                "\u2482" => "\u246e",//⒂ = ⑮
                _ => ch
            };
            
            
            x = 0 + CharXOffset;
            y = -metrics.Ascent - CharYOffset;
            // if (code is > 0x889f or -1)
            // {
            //     var margin = new SKRect();
            //     margin.Left += (pixelSize - bounds.Width) / 2;
            //     margin.Top += (pixelSize - bounds.Height) / 2;
            // x = -bounds.Left + margin.Left;
            // y = -bounds.Top + margin.Top;
            // }
            // else
            // {
            //     x = 0 + CharXOffset;
            //     y = -metrics.Ascent - CharYOffset;
            // }

            if (bounds.Left+bounds.Width + CharXOffset > pixelSize || bounds.Height > pixelSize)
                callback($"out of pixel bounds : {code}, {ch}");
        }
        canvas.Clear();
        
        canvas.DrawText(ch, x, y, font, paint);
  
        var width = isHalfChar ? pixelSize / 2 : pixelSize;
        for (var i = 0; i < pixelSize; i++)
            for (var j = 0; j < width; j += 2)
            {
                var a1 = (double)bitmap.GetPixel(j, i).Alpha;
                a1 = Brighten(a1);
                a1 = Math.Ceiling(a1/ 0x11);
                var a2 = (double)bitmap.GetPixel(j + 1, i).Alpha;
                a2 = Brighten(a2);
                a2 = Math.Ceiling(a2/ 0x11);
             
                if (a1 > 15)
                    a1 = 15;
                if (a2 > 15)
                    a2 = 15;
                var b = ((int)a1 << 4) + (int)a2;
                data.WriteByte((byte)b);
            }
    }

    private double Brighten(double value)
    {
        value *= 1.1;
        return value switch
        {
            0 => 0,
            _ => value + 8,
        };
    }
}