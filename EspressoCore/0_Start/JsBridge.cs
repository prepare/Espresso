//2015 MIT, WinterDev
using System;
namespace VroomJs
{
    public static partial class JsBridge
    {
        public const string LIB_NAME = "libespr";
        
        public static void V8Init()
        {
            NativeV8JsInterOp.V8Init();
        } 
        public static int LibVersion
        {
            get { return JsContext.getVersion(); }
        }
        //---------------------------------------------
#if DEBUG
        public static void dbugTestCallbacks()
        {
            NativeV8JsInterOp.RegisterCallBacks();
            NativeV8JsInterOp.TestCallBack();
        }
#endif
    }

}