//2013 MIT, Federico Di Gregorio<fog@initd.org>

using System;
using System.Runtime.InteropServices;

namespace Espresso
{
    //---------------------------------------
    //2017-06-04
    //1. for internal inter-op only -> always be private
    //for inter-op with native lib, .net core on macOS x64 dose not support explicit layout
    //so we need sequential layout
    //2. this is a quite large object, and is designed to be used on stack,
    //pass by reference to native side
    //---------------------------------------
    [StructLayout(LayoutKind.Sequential)]
    struct JsInterOpValue
    {
        public int I32;//4
        public long I64;//8
        public double Num;//8
        /// <summary>
        /// native ptr
        /// </summary>
        public IntPtr Ptr;//8 on 64 bits
        //type
        public JsValueType Type; //4
        //len of string and array
        public int Length;
        /// <summary>
        /// index to managed slot
        /// </summary>
        public int Index;
    }

    partial class JsContext
    {
        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int getVersion();

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr jscontext_new(int id, HandleRef engine);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jscontext_dispose(HandleRef engine);

        //TODO: review remove this?
        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern void jscontext_force_gc();


        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        static extern void jscontext_execute(HandleRef context,
            [MarshalAs(UnmanagedType.LPWStr)] string str,
            [MarshalAs(UnmanagedType.LPWStr)] string name,
            ref JsInterOpValue output);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        static extern void jscontext_execute_script(HandleRef context, HandleRef script, ref JsInterOpValue output);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern void jscontext_get_global(HandleRef engine, ref JsInterOpValue output);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern void jscontext_get_variable(HandleRef engine,
            [MarshalAs(UnmanagedType.LPWStr)] string name,
            ref JsInterOpValue output);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern void jscontext_set_variable(HandleRef engine,
            [MarshalAs(UnmanagedType.LPWStr)] string name,
            ref JsInterOpValue value,
            ref JsInterOpValue output);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static internal extern void jsvalue_alloc_string(
            [MarshalAs(UnmanagedType.LPWStr)] string str,
            ref JsInterOpValue output);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static internal extern void jsvalue_alloc_array(int length,
            ref JsInterOpValue output);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static internal extern void jsvalue_dispose(ref JsInterOpValue value);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static internal extern void jscontext_invoke(HandleRef engine,
            IntPtr funcPtr,
            IntPtr thisPtr,
            ref JsInterOpValue value,
            ref JsInterOpValue output);


        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern void jscontext_get_property_names(
            HandleRef engine,
            IntPtr ptr,
            ref JsInterOpValue output);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern void jscontext_get_property_value(HandleRef engine, IntPtr ptr,
            [MarshalAs(UnmanagedType.LPWStr)] string name,
            ref JsInterOpValue output);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern void jscontext_set_property_value(HandleRef engine,
            IntPtr ptr, [MarshalAs(UnmanagedType.LPWStr)] string name,
            ref JsInterOpValue value,
            ref JsInterOpValue output);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern void jscontext_invoke_property(HandleRef engine, IntPtr ptr,
            [MarshalAs(UnmanagedType.LPWStr)] string name,
            ref JsInterOpValue args,
            ref JsInterOpValue output);

    }
}
