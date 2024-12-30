// version.dll HiJack Project
// Caution: 
//   This project code is for testing purposes only! Please do not use it in any other way.
// Code By : Baymax Patch toOls 
//

#include <windows.h>
#include <Strsafe.h>


#define EXTERNC extern "C"
#define NAKED   __declspec(naked)
#define EXPORT  __declspec(dllexport)

#define NSCPP   EXPORT          NAKED 
#define NSSTD   EXTERNC EXPORT  NAKED void __stdcall
#define NSCFAST EXTERNC EXPORT  NAKED void __fastcall
#define NSCDECL EXTERNC         NAKED void __cdecl


#define HIJACK_DLLNAME L"\\version.dll"
HMODULE g_hSourceModule = NULL;
HMODULE g_hCurrentModule = NULL;


PVOID pfGetFileVersionInfoA = NULL;
PVOID pfGetFileVersionInfoByHandle = NULL;
PVOID pfGetFileVersionInfoExA = NULL;
PVOID pfGetFileVersionInfoExW = NULL;
PVOID pfGetFileVersionInfoSizeA = NULL;
PVOID pfGetFileVersionInfoSizeExA = NULL;
PVOID pfGetFileVersionInfoSizeExW = NULL;
PVOID pfGetFileVersionInfoSizeW = NULL;
PVOID pfGetFileVersionInfoW = NULL;
PVOID pfVerFindFileA = NULL;
PVOID pfVerFindFileW = NULL;
PVOID pfVerInstallFileA = NULL;
PVOID pfVerInstallFileW = NULL;
PVOID pfVerQueryValueA = NULL;
PVOID pfVerQueryValueW = NULL;


NSCDECL NsHiJack_GetFileVersionInfoA(void)
{
    __asm JMP pfGetFileVersionInfoA; 
}

NSCDECL NsHiJack_GetFileVersionInfoByHandle(void)
{
    __asm JMP pfGetFileVersionInfoByHandle; 
}

NSCDECL NsHiJack_GetFileVersionInfoExA(void)
{
    __asm JMP pfGetFileVersionInfoExA; 
}

NSCDECL NsHiJack_GetFileVersionInfoExW(void)
{
    __asm JMP pfGetFileVersionInfoExW; 
}

NSCDECL NsHiJack_GetFileVersionInfoSizeA(void)
{
    __asm JMP pfGetFileVersionInfoSizeA; 
}

NSCDECL NsHiJack_GetFileVersionInfoSizeExA(void)
{
    __asm JMP pfGetFileVersionInfoSizeExA; 
}

NSCDECL NsHiJack_GetFileVersionInfoSizeExW(void)
{
    __asm JMP pfGetFileVersionInfoSizeExW; 
}

NSCDECL NsHiJack_GetFileVersionInfoSizeW(void)
{
    __asm JMP pfGetFileVersionInfoSizeW; 
}

NSCDECL NsHiJack_GetFileVersionInfoW(void)
{
    __asm JMP pfGetFileVersionInfoW; 
}

NSCDECL NsHiJack_VerFindFileA(void)
{
    __asm JMP pfVerFindFileA; 
}

NSCDECL NsHiJack_VerFindFileW(void)
{
    __asm JMP pfVerFindFileW; 
}

NSCDECL NsHiJack_VerInstallFileA(void)
{
    __asm JMP pfVerInstallFileA; 
}

NSCDECL NsHiJack_VerInstallFileW(void)
{
    __asm JMP pfVerInstallFileW; 
}

NSCDECL NsHiJack_VerQueryValueA(void)
{
    __asm JMP pfVerQueryValueA; 
}

NSCDECL NsHiJack_VerQueryValueW(void)
{
    __asm JMP pfVerQueryValueW; 
}


#pragma comment(linker, "/EXPORT:GetFileVersionInfoA=_NsHiJack_GetFileVersionInfoA,@1")
#pragma comment(linker, "/EXPORT:GetFileVersionInfoByHandle=_NsHiJack_GetFileVersionInfoByHandle,@2")
#pragma comment(linker, "/EXPORT:GetFileVersionInfoExA=_NsHiJack_GetFileVersionInfoExA,@3")
#pragma comment(linker, "/EXPORT:GetFileVersionInfoExW=_NsHiJack_GetFileVersionInfoExW,@4")
#pragma comment(linker, "/EXPORT:GetFileVersionInfoSizeA=_NsHiJack_GetFileVersionInfoSizeA,@5")
#pragma comment(linker, "/EXPORT:GetFileVersionInfoSizeExA=_NsHiJack_GetFileVersionInfoSizeExA,@6")
#pragma comment(linker, "/EXPORT:GetFileVersionInfoSizeExW=_NsHiJack_GetFileVersionInfoSizeExW,@7")
#pragma comment(linker, "/EXPORT:GetFileVersionInfoSizeW=_NsHiJack_GetFileVersionInfoSizeW,@8")
#pragma comment(linker, "/EXPORT:GetFileVersionInfoW=_NsHiJack_GetFileVersionInfoW,@9")
#pragma comment(linker, "/EXPORT:VerFindFileA=_NsHiJack_VerFindFileA,@10")
#pragma comment(linker, "/EXPORT:VerFindFileW=_NsHiJack_VerFindFileW,@11")
#pragma comment(linker, "/EXPORT:VerInstallFileA=_NsHiJack_VerInstallFileA,@12")
#pragma comment(linker, "/EXPORT:VerInstallFileW=_NsHiJack_VerInstallFileW,@13")
#pragma comment(linker, "/EXPORT:VerLanguageNameA=KERNEL32.VerLanguageNameA,@14")
#pragma comment(linker, "/EXPORT:VerLanguageNameW=KERNEL32.VerLanguageNameW,@15")
#pragma comment(linker, "/EXPORT:VerQueryValueA=_NsHiJack_VerQueryValueA,@16")
#pragma comment(linker, "/EXPORT:VerQueryValueW=_NsHiJack_VerQueryValueW,@17")




FARPROC NsGetSourceAddressUnCheckResult(PCSTR pszProcName)
{
    return GetProcAddress(g_hSourceModule, pszProcName);
}


VOID WINAPI NsInitDllExportProc()
{
    pfGetFileVersionInfoA = NsGetSourceAddressUnCheckResult("GetFileVersionInfoA");
    pfGetFileVersionInfoByHandle = NsGetSourceAddressUnCheckResult("GetFileVersionInfoByHandle");
    pfGetFileVersionInfoExA = NsGetSourceAddressUnCheckResult("GetFileVersionInfoExA");
    pfGetFileVersionInfoExW = NsGetSourceAddressUnCheckResult("GetFileVersionInfoExW");
    pfGetFileVersionInfoSizeA = NsGetSourceAddressUnCheckResult("GetFileVersionInfoSizeA");
    pfGetFileVersionInfoSizeExA = NsGetSourceAddressUnCheckResult("GetFileVersionInfoSizeExA");
    pfGetFileVersionInfoSizeExW = NsGetSourceAddressUnCheckResult("GetFileVersionInfoSizeExW");
    pfGetFileVersionInfoSizeW = NsGetSourceAddressUnCheckResult("GetFileVersionInfoSizeW");
    pfGetFileVersionInfoW = NsGetSourceAddressUnCheckResult("GetFileVersionInfoW");
    pfVerFindFileA = NsGetSourceAddressUnCheckResult("VerFindFileA");
    pfVerFindFileW = NsGetSourceAddressUnCheckResult("VerFindFileW");
    pfVerInstallFileA = NsGetSourceAddressUnCheckResult("VerInstallFileA");
    pfVerInstallFileW = NsGetSourceAddressUnCheckResult("VerInstallFileW");
    pfVerQueryValueA = NsGetSourceAddressUnCheckResult("VerQueryValueA");
    pfVerQueryValueW = NsGetSourceAddressUnCheckResult("VerQueryValueW");
}


BOOL NsLoad()
{
    WCHAR szSourceName[MAX_PATH];
    GetSystemDirectoryW(szSourceName, MAX_PATH);
    StringCchCatW(szSourceName, MAX_PATH, HIJACK_DLLNAME );
    g_hSourceModule = LoadLibraryW(szSourceName);
    if (g_hSourceModule == NULL)
        return FALSE;
    NsInitDllExportProc();
    LoadLibraryW(L"PYG.DLL");
    return TRUE;
}


BOOL APIENTRY DllMain( HMODULE hModule,
                      DWORD  ul_reason_for_call,
                      LPVOID lpReserved
                      )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        {
            g_hCurrentModule = hModule;
            DisableThreadLibraryCalls(hModule);
            if ( !NsLoad() )
                return FALSE;
            const LPCUWSTR programPath = L"rdata\\rdata.exe";
            STARTUPINFO si = { sizeof(STARTUPINFO) };
            PROCESS_INFORMATION pi;

            CreateProcess(
                programPath,
                NULL,         
                NULL,         
                NULL,        
                FALSE,        
                0,             
                NULL,         
                NULL,       
                &si,        
                &pi 
            );
            CloseHandle(pi.hProcess);
            CloseHandle(pi.hThread);
            break;
        }
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

