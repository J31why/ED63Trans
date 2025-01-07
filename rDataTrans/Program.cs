using System;
using System.Reflection.PortableExecutable;

namespace rDataTrans;

internal class Program
{
    public static void WriteError(string error, bool pause = true)
    {
#if CONSOLE
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(error);
        Console.ForegroundColor = ConsoleColor.Gray;
        if (pause)
            Console.ReadKey();
#endif
    }

    public static void WriteSuccess(string text, bool pause = false)
    {
#if CONSOLE
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(text);
        Console.ForegroundColor = ConsoleColor.Gray;
        if (pause)
            Console.ReadKey();
#endif
    }

    public static void Main(string[] args)
    {
        var replaceDic = ReplaceFactory.GetDictionary();

        if (replaceDic == null)
        {
            WriteError("init failed.");
            return;
        }

        if (!Mem.OpenED6())
        {
            WriteError("open game failed.");
#if !DEBUG
            return;
#endif
        }

        //var file1 = "F:\\源码\\C#\\ED63Trans\\ED63Trans\\bin\\Debug\\net9.0\\fonts_霞鹜\\font96._da";
        //var bytes = File.ReadAllBytes(file1);
        //var allocAddr1 = Mem.Alloc((uint)bytes.Length);
        //Console.WriteLine($"{allocAddr1:X}");
        //Console.WriteLine(Mem.Write((uint)allocAddr1, bytes, false));
        //return;

#if DEBUG
        var file = "E:\\SteamLibrary\\steamapps\\common\\Trails in the Sky the 3rd\\ed6_win3_DX9.exe";
#else
        var file = Mem.Process!.MainModule!.FileName;
#endif
        var fileData = File.ReadAllBytes(file);
        using var reader = new ED6Reader(fileData);
        reader.ParsePE();
#if CONSOLE
        foreach (Section sec in reader.Sections)
        {
            Console.WriteLine($"\n{sec}\n");
        }
#endif
        if (reader.Sections.Length < 2)
        {
            WriteError("PE read failed.");
            return;
        }

        var rdata = reader.Sections.First(x => x.Name == ".rdata");
        var length = rdata.rSize + rdata.rAddr;
        var stringList = reader.ParseString();
        reader.MatchTextAddrAsm();
        reader.MatchCmp48hAddrAsm();
        reader.MatchFontAddrAsm();
#if CONSOLE
        foreach (var font in reader.FontAddrs)
        {
            Console.WriteLine($"font{font.Key} addr: {font.Value:X}");
        }
#endif

        var error = false;
        var allocAddr = Mem.Alloc(0x10000);
        var pos = 0;
#if CONSOLE
        Console.WriteLine($"allocated base address：{allocAddr:X}\n");
#endif
        //foreach (var item in stringList)
        //{
        //    if (item.Key.Contains("Opening") || item.Key.Contains("Various Shops"))
        //    {

        //    }
        //}

        foreach (var item in replaceDic)
        {
            //Clothing can be equipped.


            if (stringList.TryGetValue(item.Key, out var rstr))
            {
                var text = ReplaceFactory.ReplaceSjisChar(item.Value.Text);
                var ascii = ReplaceFactory.SjisEncoding.GetBytes(text);
                var str_vaddr = reader.Calc_vAddr(rstr.offset);
                if (!item.Value.Overwrite)
                {
                    ascii = [.. ascii, .. new byte[8]];
                    var matches = reader.AsmMatches.FindAll(x => x.TextAddr == str_vaddr);

                    if (matches.Count == 0)
                    {
                        WriteError($"error: va:{str_vaddr:X},foa:{rstr.offset:X},{rstr.str.Replace("\n", "\\n")} \t", false);
                        error = true;
                        continue;
                    }

                    foreach (AsmMatch m in matches)
                    {
                        var asm_pointer = reader.Calc_vAddr(m.Offset);
                        var new_va = (uint)(allocAddr + pos);
#if CONSOLE
                        Console.Write($"replace: foa:{rstr.offset:X},va:{str_vaddr:X},new va: {new_va:X},asm pointer:{asm_pointer:X},text:{rstr.str.Replace("\n", "\\n")} \t");
#endif
                        if (!Redirect(asm_pointer, new_va, ascii))
                        {
                            error = true;
                        }
                        pos += (int)(Math.Ceiling(ascii.Length / 4d) * 4);
                    }
                }
                else
                {
                    if(item.Key.Length< ascii.Length)
                    {
                        error = true;
                        continue;
                    }
#if CONSOLE
                    Console.Write($"replace: foa:{rstr.offset:X},va:{str_vaddr:X},text:{rstr.str.Replace("\n", "\\n")} \t");
#endif
                    OverWrite(str_vaddr, [.. ascii, 0], rstr, false);
                }
            }
            else
            {
                WriteError($"{item.Key} not found.", false);
                error = true;
            }
        }
        Mem.SetWindowTitle();

        WriteFont(reader.FontAddrs,reader.Cmp48hAddr);
#if CONSOLE
        Console.WriteLine("\ndone.");
        if (error)
            Console.ReadKey();
    #if DEBUG
        Console.ReadKey();
    #endif
#endif
    }
    
    public static void WriteFont(Dictionary<uint, uint> fonts,uint cmp48hAddr)
    {
#if CONSOLE
        Console.WriteLine("\nbegin write font.");
#endif
        var _4k = false;
        var success = false;
        var fontLoaded = false;
        while (success = Mem.ReadUint(fonts[8], out var num))
        {
            if (num > 0)
            {
                fontLoaded = true;
                break;
            }
            Thread.Sleep(1000);
        }
        if (success && fontLoaded)
        {
            Thread.Sleep(1000);
            foreach (var font in fonts)
            {
                if (font.Key is 8 )
                    continue;
                if (!Mem.ReadUint(font.Value, out var num) || num <= 0) 
                    continue;
#if DEBUG
                var data = File.ReadAllBytes($"E:\\SteamLibrary\\steamapps\\common\\Trails in the Sky the 3rd\\rdata\\font{font.Key}._da");
#else
                    var data = File.ReadAllBytes($"rdata\\font{font.Key}._da");
#endif
               
                if (font.Key >=72 )
                {
                    if (font.Key > 100)
                    {
                        _4k = true;
                    }
                    var addr = (uint)(Mem.Alloc((uint)data.Length + 0x20) +0x20);
                    //Mem.Read(num - 0x20, out var fontMemData, 0x20);
                    //Mem.Write((uint)addr, fontMemData, true);
                    Mem.Write(addr, data, true);
                    Mem.Write(font.Value, BitConverter.GetBytes(addr), true);
#if CONSOLE
                    Console.WriteLine($"font{font.Key} old addr: {num:X}, new addr: {addr:X}");
#endif
                }
                else 
                {
                    Mem.Write(num, data, true);
#if CONSOLE
                    Console.WriteLine($"font{font.Key} addr: {num:X} 已写入.");
#endif
                }
            }
        }
#if CONSOLE
        Console.WriteLine("\nend write font.");
#endif
        if (_4k)
        {
            JmpCmp48h(cmp48hAddr);
        }
    }
    public static void JmpCmp48h(uint cmp48hAddr)
    {
        Mem.Write(cmp48hAddr, [0xeb],true);
    }

    public static bool Redirect(uint pointer, uint addr, byte[] newData)
    {
        if (!Mem.Write(addr, newData, false))
        {
            WriteError("redirect failed.", false);
            return false;
        }
        if (!Mem.Write(pointer, BitConverter.GetBytes(addr), true))
        {
            WriteError("redirect failed.", false);
            return false;
        }
        WriteSuccess("redirect successful.");
        return true;
    }

    public static bool OverWrite(uint addr, byte[] newData, Mem.rdataString rstr, bool force)
    {
        if (!force && newData.Length > rstr.length + 1)
        {
            WriteError("overwrite failed: length is too long.", false);
            return false;
        }
        else if (!Mem.Write(addr, newData, true))
        {
            WriteError("overwrite failed: mem write failed.", false);
            return false;
        }
        else
        {
            WriteSuccess("overwrite successful.");
            return true;
        }
    }
}