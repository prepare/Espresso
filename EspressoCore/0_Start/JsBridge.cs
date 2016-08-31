//2015 MIT, WinterDev
using System;
namespace VroomJs
{
    public delegate void EngineSetupCallback(JsEngine jsEngine, JsContext currentContext);

    public static partial class JsBridge
    {
        public const string LIB_NAME = "libespr";
        static JsEngineSetupCallbackDel engineSetupDel;
        static EngineSetupCallback enginSetupCallback2;

        static JsBridge()
        {
            engineSetupDel = new JsEngineSetupCallbackDel(EngineListener_EngineSetupCallback);
        }
        public static void V8Init()
        {
            NativeV8JsInterOp.V8Init();
        }
        public static int LibVersion
        {
            get { return JsContext.getVersion(); }
        }
        //---------------------------------------------

        public static void RegisterEngineSetupCallback(EngineSetupCallback enginSetupCallback)
        {
            JsBridge.enginSetupCallback2 = enginSetupCallback;
            NativeV8JsInterOp.RegisterManagedCallback(
                System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(engineSetupDel),
               1);

        }

        static void EngineListener_EngineSetupCallback(IntPtr nativeJsEnginePtr, IntPtr currentNativeJsContext)
        {
            //create from native engine
            JsEngine jsEngine = new JsEngine(nativeJsEnginePtr, new DefaultJsTypeDefinitionBuilder());
            JsContext ctx = jsEngine.CreateContext(currentNativeJsContext);

            if (enginSetupCallback2 != null)
            {
                enginSetupCallback2(jsEngine, ctx);
            }
        }
#if DEBUG
        public static void dbugTestCallbacks()
        {
            NativeV8JsInterOp.RegisterCallBacks();
            NativeV8JsInterOp.TestCallBack();
        }
#endif
    }

}