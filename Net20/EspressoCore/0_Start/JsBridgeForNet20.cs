using System;
using System.Runtime.InteropServices;
namespace VroomJs
{

    static partial class JsBridge
    {
        static IntPtr hModuleV8;
        public static void LoadV8(string dllfile)
        {
            hModuleV8 = LoadLibrary(dllfile);
            if (hModuleV8 == IntPtr.Zero)
            {
                return;
            }
            NativeV8JsInterOp.V8Init();
        }

        public static void UnloadV8()
        {
            if (hModuleV8 != IntPtr.Zero)
            {
                FreeLibrary(hModuleV8);
                hModuleV8 = IntPtr.Zero;
            }
        }
        [DllImport("Kernel32.dll")]
        public static extern IntPtr LoadLibrary(string libraryName);
        [DllImport("Kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);
        [DllImport("Kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
        [DllImport("Kernel32.dll")]
        public static extern uint SetErrorMode(int uMode);
        [DllImport("Kernel32.dll")]
        public static extern uint GetLastError();
    }

}