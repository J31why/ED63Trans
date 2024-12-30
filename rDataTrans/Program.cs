namespace rDataTrans;

internal class Program
{
    public static void WriteError(string error, bool pause = true)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(error);
        Console.ForegroundColor = ConsoleColor.Gray;
        if (pause)
            Console.ReadKey();
    }

    public static void WriteSuccess(string text, bool pause = false)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(text);
        Console.ForegroundColor = ConsoleColor.Gray;
        if (pause)
            Console.ReadKey();
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

#if DEBUG
        var file = "E:\\SteamLibrary\\steamapps\\common\\Trails in the Sky the 3rd\\ed6_win3_DX9.exe";
#else
        var file = Mem.Process!.MainModule!.FileName;
#endif
        var fileData = File.ReadAllBytes(file);
        using var reader = new ED6Reader(fileData);
        reader.ParsePE();

        foreach (Section sec in reader.Sections)
        {
            Console.WriteLine($"\n{sec}\n");
        }

        if (reader.Sections.Length < 2)
        {
            WriteError("PE read failed.");
            return;
        }

        var rdata = reader.Sections.First(x => x.Name == ".rdata");
        var length = rdata.rSize + rdata.rAddr;
        var stringList = reader.ParseString();
        reader.MatchAsm();
        var error = false;
        var allocAddr = Mem.Alloc(0x10000);
        var pos = 0;
        Console.WriteLine($"allocated base address：{allocAddr:X}\n");

        foreach (var item in replaceDic)
        {
            //Clothing can be equipped.

            if (stringList.TryGetValue(item.Key, out var rstr))
            {
                var text = ReplaceFactory.ReplaceSjisChar(item.Value.Text);
                var ascii = ReplaceFactory.SjisEncoding.GetBytes(text);
                ascii = [.. ascii, .. new byte[8]];
                var str_vaddr = reader.Clac_vAddr(rstr.offset);
                var matches = reader.AsmMatches.FindAll(x => x.TextAddr == str_vaddr);

                if (matches.Count == 0)
                {
                    WriteError($"error: va:{str_vaddr:X},foa:{rstr.offset:X},{rstr.str.Replace("\n", "\\n")} \t", false);
                    error = true;
                    continue;
                }

                foreach (AsmMatch m in matches)
                {
                    var asm_pointer = reader.Clac_vAddr(m.Offset);
                    var new_va = (uint)(allocAddr + pos);
                    Console.Write($"replace: foa:{rstr.offset:X},va:{str_vaddr:X},new va: {new_va:X},asm pointer:{asm_pointer:X},text:{rstr.str.Replace("\n", "\\n")} \t");

                    if (!Redirect(asm_pointer, new_va, ascii))
                    {
                        error = true;
                    }
                    pos += (int)(Math.Ceiling(ascii.Length / 4d) * 4);
                }
            }
            else
            {
                WriteError($"{item.Key} not found.", false);
                error = true;
            }
        }
        Mem.SetWindowTitle();

        Console.WriteLine("\ndone.");
        if (error)
            Console.ReadKey();
#if DEBUG
        Console.ReadKey();
#endif
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