//2013 MIT, Federico Di Gregorio<fog@initd.org>

using System;
using System.Runtime.InteropServices;

namespace Espresso
{
    partial class JsContext
    {
      

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
            ref JsValue output);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        static extern void jscontext_execute_script(HandleRef context, HandleRef script, ref JsValue output);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern void jscontext_get_global(HandleRef engine, ref JsValue output);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern void jscontext_get_variable(HandleRef engine,
            [MarshalAs(UnmanagedType.LPWStr)] string name,
            ref JsValue output);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern void jscontext_set_variable(HandleRef engine,
            [MarshalAs(UnmanagedType.LPWStr)] string name,
            ref JsValue value,
            ref JsValue output);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static internal extern void jsvalue_alloc_string(
            [MarshalAs(UnmanagedType.LPWStr)] string str,
            ref JsValue output);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static internal extern void jsvalue_alloc_array(int length,
            ref JsValue output);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static internal extern void jsvalue_dispose(ref JsValue value);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static internal extern void jscontext_invoke(HandleRef engine,
            IntPtr funcPtr,
            IntPtr thisPtr,
            ref JsValue value,
            ref JsValue output);


        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern void jscontext_get_property_names(
            HandleRef engine,
            IntPtr ptr,
            ref JsValue output);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern void jscontext_get_property_value(HandleRef engine, IntPtr ptr,
            [MarshalAs(UnmanagedType.LPWStr)] string name,
            ref JsValue output);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern void jscontext_set_property_value(HandleRef engine,
            IntPtr ptr, [MarshalAs(UnmanagedType.LPWStr)] string name,
            ref JsValue value,
            ref JsValue output);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern void jscontext_invoke_property(HandleRef engine, IntPtr ptr,
            [MarshalAs(UnmanagedType.LPWStr)] string name,
            ref JsValue args,
            ref JsValue output);

    }
}
