//MIT, 2015-present, WinterDev, EngineKit, brezza92
//MIT, 2013, Federico Di Gregorio <fog@initd.org>

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Espresso
{


    partial class JsEngine : IDisposable
    {

        delegate void KeepaliveRemoveDelegate(int context, int slot);
        delegate void KeepAliveGetPropertyValueDelegate(int context, int slot,
            [MarshalAs(UnmanagedType.LPWStr)] string name,
            ref JsValue output);
        delegate void KeepAliveSetPropertyValueDelegate(int context,
            int slot,
            [MarshalAs(UnmanagedType.LPWStr)] string name,
            ref JsValue value,
            ref JsValue output);
        delegate void KeepAliveValueOfDelegate(int context, int slot, ref JsValue output);
        delegate void KeepAliveInvokeDelegate(int context, int slot, ref JsValue args, ref JsValue output);
        delegate void KeepAliveDeletePropertyDelegate(int context, int slot, [MarshalAs(UnmanagedType.LPWStr)] string name, ref JsValue output);
        delegate void KeepAliveEnumeratePropertiesDelegate(int context, int slot, ref JsValue output);


        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern void js_dump_allocated_items();

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr jsengine_new(
            KeepaliveRemoveDelegate keepaliveRemove,
            KeepAliveGetPropertyValueDelegate keepaliveGetPropertyValue,
            KeepAliveSetPropertyValueDelegate keepaliveSetPropertyValue,
            KeepAliveValueOfDelegate keepaliveValueOf,
            KeepAliveInvokeDelegate keepaliveInvoke,
            KeepAliveDeletePropertyDelegate keepaliveDeleteProperty,
            KeepAliveEnumeratePropertiesDelegate keepaliveEnumerateProperties,
            int maxYoungSpace,
            int maxOldSpace
        );
        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr jsengine_registerManagedDels(
           IntPtr jsEngineNativePtr,
           KeepaliveRemoveDelegate keepaliveRemove,
           KeepAliveGetPropertyValueDelegate keepaliveGetPropertyValue,
           KeepAliveSetPropertyValueDelegate keepaliveSetPropertyValue,
           KeepAliveValueOfDelegate keepaliveValueOf,
           KeepAliveInvokeDelegate keepaliveInvoke,
           KeepAliveDeletePropertyDelegate keepaliveDeleteProperty,
           KeepAliveEnumeratePropertiesDelegate keepaliveEnumerateProperties
       );

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern void jsengine_terminate_execution(HandleRef engine);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern void jsengine_dump_heap_stats(HandleRef engine);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern void jsengine_dispose(HandleRef engine);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern void jsengine_dispose_object(HandleRef engine, IntPtr obj);

    }


    public delegate void EngineSetupCallback(JsEngine jsEngine, JsContext ctx);



    public static class NodeJsEngine
    {
        //-------------------------------------------------------------------------------------------------------
        //this is Espresso-ND
        //the Espresso + NodeJS
        //-------------------------------------------------------------------------------------------------------

        static JsTypeDefinitionBuilder s_jsTypeDefBuilder = new DefaultJsTypeDefinitionBuilder();
        public static void SetDefaultJsTypeDefinitionBuilder(JsTypeDefinitionBuilder jstypedefBuilder)
        {
            s_jsTypeDefBuilder = jstypedefBuilder;
        }
        public static int Run(EngineSetupCallback engineSetup)
        {
            JsEngine eng = null;
            return RunJsEngine((IntPtr nativeEngine, IntPtr nativeContext) =>
            {
                eng = new JsEngine(nativeEngine);
                JsContext context = eng.CreateContext(nativeContext, s_jsTypeDefBuilder);
                engineSetup(eng, context);
            },
            (IntPtr nativeEngine, IntPtr nativeContext, int exitcode) =>
            {
                JsEngine.DisposeEngineFromNativeSide(eng);
            });
        }
        public static int Run(string[] parameters, EngineSetupCallback engineSetup)
        {
            JsEngine eng = null;
            return RunJsEngine(parameters, (IntPtr nativeEngine, IntPtr nativeContext) =>
            {
               eng = new JsEngine(nativeEngine);
               //
               JsContext context = eng.CreateContext(nativeContext, s_jsTypeDefBuilder);
               engineSetup(eng, context);
            },
            (IntPtr nativeEngine, IntPtr nativeContext, int exitcode) =>
            {
                JsEngine.DisposeEngineFromNativeSide(eng);
            });
        }

        //---------------------
        static int RunJsEngine(
          NativeEngineSetupCallback engineSetupCb,
          NativeEngineClosingCallback engineClosingCb)
        {
            return RunJsEngine(

                // "hello.espr" not on disk,
                //this make espresso-ND callback to LoadMainSrcFile() in our .net code
                //and we can handle how to load the rest 
                new string[] { "hello.espr" },
                engineSetupCb,
                engineClosingCb);
        }
        static int RunJsEngine(string[] parameters, NativeEngineSetupCallback engineSetupCb,
               NativeEngineClosingCallback engineClosingCb)
        {
            List<string> nodeStartPars = new List<string>();
            nodeStartPars.Add("node"); //essential first parameter
            if (parameters != null) nodeStartPars.AddRange(parameters);

            return RunJsEngine(nodeStartPars.Count, nodeStartPars.ToArray(), engineSetupCb, engineClosingCb);
        }
        //-------------------------------------------------------------------------------------------------------
        [DllImport(JsBridge.LIB_NAME, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        static extern int RunJsEngine(int argc, string[] args, NativeEngineSetupCallback engineSetupCb, NativeEngineClosingCallback engineClosingCb);
    }
}