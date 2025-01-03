﻿#region

using System.Diagnostics;
using System.Runtime.InteropServices;

#endregion

namespace rDataTrans;

public static class Mem
{
    public class rdataString
    {
        public long offset;
        public int length;
        public string str = "";
    }

    [DllImport("kernel32.dll", EntryPoint = "OpenProcess")]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", EntryPoint = "WriteProcessMemory")]
    public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize,
        IntPtr lpNumberOfBytesWritten);

    [DllImport("kernel32.dll")]
    public static extern void CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    [DllImport("KERNEL32.dll", ExactSpelling = true, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern bool SetWindowText(IntPtr hWnd, string lpString);

    private const int MEM_COMMIT = 0x00001000;
    private const int PAGE_READWRITE = 0x04;
    private const int PROCESS_ALL_ACCESS = 0x1F0FFF;

    private static IntPtr hProcess;
    public static Process? Process;

    public static bool OpenED6()
    {
        var processes = Process.GetProcessesByName("ed6_win3_DX9");
        Process = null;
        if (processes.Length > 0)
        {
            Process = processes.First();
            return Open(Process);
        }
        processes = Process.GetProcessesByName("ed6_win3");
        if (processes.Length > 0)
        {
            Process = processes.First();
            return Open(Process);
        }
        return false;
    }

    public static void SetWindowTitle()
    {
        if (Process == null) return;
        /*The Legend of Heroes: Trails in the Sky the 3rd (DX9) - SoraVoice (Lite) 20230823*/
        var name = Process.MainWindowTitle.Replace("The Legend of Heroes: Trails in the Sky the 3rd", "英雄传说：空之轨迹the 3rd");
        IntPtr hWnd = Process.MainWindowHandle;
        SetWindowText(hWnd, name);
    }

    private static bool Open(Process p)
    {
        hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, p.Id);
        return hProcess > 0;
    }

    public static IntPtr Alloc(uint size)
    {
        return VirtualAllocEx(hProcess, IntPtr.Zero, size, MEM_COMMIT, PAGE_READWRITE);
    }

    public static bool Write(uint addr, byte[] newData, bool changeProtect)
    {
        var p = new IntPtr(addr);
        bool result;
        uint old = 0;
        if (changeProtect)
        {
            result = VirtualProtectEx(hProcess, p, newData.Length, PAGE_READWRITE, out old);
            if (!result)
                return false;
        }
        result = WriteProcessMemory(hProcess, p, newData, newData.Length, IntPtr.Zero);

        if (changeProtect)
        {
            VirtualProtectEx(hProcess, p, newData.Length, old, out old);
        }
        return result;
    }
}