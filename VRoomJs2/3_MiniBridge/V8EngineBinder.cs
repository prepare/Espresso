//2015, MIT ,WinterDev

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;

using VroomJs;

namespace NativeV8
{


    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void ManagedListenerDel(int mIndex,
        [MarshalAs(UnmanagedType.LPWStr)]string methodName,
        IntPtr args);


    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void ManageMethodCallDel(int mIndex, IntPtr args, IntPtr result);



    public class JsTypeDefinition : JsTypeMemberDefinition
    {
        //store definition for js
        List<JsFieldDefinition> fields = new List<JsFieldDefinition>();
        List<JsMethodDefinition> methods = new List<JsMethodDefinition>();
        public JsTypeDefinition(string typename)
            : base(typename, JsMemberKind.Type)
        {

        }
        public void AddMember(JsFieldDefinition fieldDef)
        {
            fieldDef.SetOwner(this);
            fields.Add(fieldDef);
        }
        public void AddMember(JsMethodDefinition methodDef)
        {
            methodDef.SetOwner(this);
            methods.Add(methodDef);
        }

        /// <summary>
        /// serialization this typedefinition to binary format and 
        /// send to native side
        /// </summary>
        /// <param name="writer"></param>
        public void WriteDefinitionToStream(BinaryWriter writer)
        {
            //----------------------
            //this is our custom protocol/convention with the MiniJsBridge            
            //we may change this in the future
            //eg. use json serialization/deserialization 
            //----------------------
            //1.
            writer.Write((short)this.MemberId);
            //2. type kind
            writer.Write((short)0);
            //3. typename                         
            WriteUtf16String(this.MemberName, writer);
            //4. num of field
            int j = fields.Count;
            writer.Write((short)j);
            for (int i = 0; i < j; ++i)
            {
                JsFieldDefinition fielddef = fields[i];
                //*** field id -- unique field id within one type
                writer.Write((short)fielddef.MemberId);
                //field flags
                writer.Write((short)0);
                //field name
                WriteUtf16String(fielddef.MemberName, writer);
            }
            //------------------
            j = methods.Count;
            writer.Write((short)j);
            for (int i = 0; i < j; ++i)
            {
                JsMethodDefinition methoddef = methods[i];
                writer.Write((short)methoddef.MemberId);
                //method flags
                writer.Write((short)0);
                //method name
                WriteUtf16String(methoddef.MemberName, writer);
            }
        }

        internal List<JsFieldDefinition> GetFields()
        {
            return fields;
        }
        internal List<JsMethodDefinition> GetMethods()
        {
            return methods;
        }

    }
    public abstract class JsTypeMemberDefinition
    {
        string mbname;
        JsMemberKind memberKind;
        JsTypeDefinition owner;
        int memberId;
        internal NativeObjectProxy nativeProxy;
        public JsTypeMemberDefinition(string mbname, JsMemberKind memberKind)
        {
            this.mbname = mbname;
            this.memberKind = memberKind;
        }
        public bool IsRegisterd
        {
            get
            {
                return this.nativeProxy != null;
            }
        }

        public string MemberName
        {
            get
            {
                return this.mbname;
            }
        }
        public JsMemberKind MemberKind
        {
            get
            {
                return this.memberKind;
            }
        }
        public void SetOwner(JsTypeDefinition owner)
        {
            this.owner = owner;
        }
        protected static void WriteUtf16String(string str, BinaryWriter writer)
        {
            char[] charBuff = str.ToCharArray();
            writer.Write((short)charBuff.Length);
            writer.Write(charBuff);
        }

        public int MemberId
        {
            get
            {
                return this.memberId;
            }
        }
        public void SetMemberId(int memberId)
        {
            this.memberId = memberId;
        }

    }
    public enum JsMemberKind
    {
        Field,
        Method,
        Event,
        PropertyGet,
        PropertySet,
        IndexerGet,
        IndexerSet,
        Type
    }

    public class JsFieldDefinition : JsTypeMemberDefinition
    {
        public JsFieldDefinition(string fieldname)
            : base(fieldname, JsMemberKind.Field)
        {

        }
    }

    public delegate void JsMethodCallDel(ManagedMethodArgs args);

    public struct ManagedMethodArgs
    {
        IntPtr nativeArgPtr;
        IntPtr nativeResultPtr;
        public ManagedMethodArgs(IntPtr nativeArgPtr, IntPtr nativeResultPtr)
        {
            this.nativeArgPtr = nativeArgPtr;
            this.nativeResultPtr = nativeResultPtr;
        }
        public string GetArgAsString(int index)
        {
            return NativeV8JsInterOp.ArgGetString(this.nativeArgPtr, index);
        }
        public int GetArgAsInt32(int index)
        {
            return NativeV8JsInterOp.ArgGetInt32(this.nativeArgPtr, index);
        }
        //------------------------
        public void SetResult(bool value)
        {
            NativeV8JsInterOp.ArgSetBool(nativeResultPtr, value);
        }
        public void SetResult(int value)
        {
            NativeV8JsInterOp.ArgSetInt32(nativeResultPtr, value);
        }
        public void SetResult(string value)
        {
            NativeV8JsInterOp.ArgSetString(nativeResultPtr, value);
        }
        public void SetResult(double value)
        {
            NativeV8JsInterOp.ArgSetDouble(nativeResultPtr, value);
        }
        public void SetResult(float value)
        {
            NativeV8JsInterOp.ArgSetFloat(nativeResultPtr, value);
        }
        public void SetNativeObjResult(int value)
        {
            NativeV8JsInterOp.ArgSetNativeObject(nativeResultPtr, value);
        }
        //------------------------
    }


    public class JsMethodDefinition : JsTypeMemberDefinition
    {
        JsMethodCallDel methodCallDel;
        public JsMethodDefinition(string methodName)
            : base(methodName, JsMemberKind.Method)
        {
        }
        public JsMethodDefinition(string methodName, JsMethodCallDel methodCallDel)
            : base(methodName, JsMemberKind.Method)
        {

            SetCall(methodCallDel);
        }
        public void SetCall(JsMethodCallDel methodCallDel)
        {
            this.methodCallDel = methodCallDel;
        }
        public void InvokeMethod(ManagedMethodArgs arg)
        {
            if (methodCallDel != null)
            {
                methodCallDel(arg);
            }
        }
    }
    public abstract class NativeObjectProxy
    {
        object wrapObject;
        int mIndex;
        IntPtr unmanagedObjectPtr;
        public NativeObjectProxy(int mIndex, object wrapObject)
        {
            this.mIndex = mIndex;
            this.wrapObject = wrapObject;
        }
        public int ManagedIndex
        {
            get
            {
                return this.mIndex;
            }
        }
        public object WrapObject
        {
            get
            {
                return this.wrapObject;
            }
        }

        public bool HasNativeWrapperPart
        {
            get
            {
                return this.unmanagedObjectPtr != IntPtr.Zero;
            }
        }
        public void SetUnmanagedObjectPointer(IntPtr unmanagedObjectPtr)
        {
            this.unmanagedObjectPtr = unmanagedObjectPtr;
        }
        public IntPtr UnmanagedPtr
        {
            get
            {
                return this.unmanagedObjectPtr;
            }
        }
    }
    class NativeObjectProxy<T> : NativeObjectProxy
        where T : class
    {
        public NativeObjectProxy(int mIndex, T wrapObject)
            : base(mIndex, wrapObject)
        {

        }
    }

    /// <summary>
    /// instance of object with type definition
    /// </summary>
    public class NativeJsInstanceProxy : NativeObjectProxy
    {
        JsTypeDefinition jsTypeDef;
        public NativeJsInstanceProxy(int mIndex, object wrapObject, JsTypeDefinition jsTypeDef)
            : base(mIndex, wrapObject)
        {
            this.jsTypeDef = jsTypeDef;

        }
        public JsTypeDefinition JsTypeDefinition
        {
            get
            {
                return this.jsTypeDef;
            }
        }
    }

    class NativeObjectProxyStore
    {
        List<NativeObjectProxy> exportList = new List<NativeObjectProxy>();
        JsContext2 ownerContext;
        public NativeObjectProxyStore(JsContext2 ownerContext)
        {
            this.ownerContext = ownerContext;
        }
        public NativeJsInstanceProxy CreateProxyForObject(object o, JsTypeDefinition jsTypeDefinition)
        {
            NativeJsInstanceProxy proxyObject = new NativeJsInstanceProxy(
                exportList.Count,
                o,
                jsTypeDefinition);

            exportList.Add(proxyObject);

            //register
            NativeV8JsInterOp.CreateNativePart(ownerContext.myjsContext, proxyObject);
            return proxyObject;
        }
        public NativeObjectProxy CreateProxyForTypeDefinition(JsTypeDefinition jsTypeDefinition)
        {

            NativeObjectProxy<JsTypeDefinition> proxyObject = new NativeObjectProxy<JsTypeDefinition>(exportList.Count, jsTypeDefinition);
            //store data this side too
            jsTypeDefinition.nativeProxy = proxyObject;
            //store in exported list
            exportList.Add(proxyObject);
            //register type definition
            NativeV8JsInterOp.RegisterTypeDef(ownerContext, jsTypeDefinition);
            return proxyObject;
        }
        public void Dispose()
        {

            int j = exportList.Count;
            for (int i = exportList.Count - 1; i > -1; --i)
            {
                NativeV8JsInterOp.UnRegisterNativePart(exportList[i]);
            }
            exportList.Clear();
        }

    }



    public class JsContext2
    {
        internal NativeObjectProxy nativeEngineContextProxy;
        NativeObjectProxyStore proxyStore;
        Stack<IntPtr> v8ContextLock = new Stack<IntPtr>();

        internal JsContext myjsContext;
        public JsContext2(JsContext myjsContext)
        {
            this.myjsContext = myjsContext;
            this.proxyStore = new NativeObjectProxyStore(this);
            NativeObjectProxy<JsContext> wrapJsContext = new NativeObjectProxy<JsContext>(1, myjsContext);
            wrapJsContext.SetUnmanagedObjectPointer(myjsContext.Handle.Handle);
            this.nativeEngineContextProxy = wrapJsContext;
        }
        public void SetParameter(string parname, object value)
        {
            this.myjsContext.SetVariable(parname, value);
        }
        public NativeJsInstanceProxy CreateWrapper(object o, JsTypeDefinition jsTypeDefinition)
        {
            return proxyStore.CreateProxyForObject(o, jsTypeDefinition);
        }

        //public void SetParameter(string parname, int arg)
        //{
        //    NativeV8JsInterOp.RegParamInt32(nativeEngineContextProxy.UnmanagedPtr, parname, arg);
        //}
        //public void SetParameter(string parname, float arg)
        //{
        //    NativeV8JsInterOp.RegParamFloat(nativeEngineContextProxy.UnmanagedPtr, parname, arg);
        //}
        //public void SetParameter(string parname, double arg)
        //{
        //    NativeV8JsInterOp.RegParamDouble(nativeEngineContextProxy.UnmanagedPtr, parname, arg);
        //}
        //public void SetParameter(string parname, string arg)
        //{
        //    NativeV8JsInterOp.RegParamString(nativeEngineContextProxy.UnmanagedPtr, parname, arg);
        //}
        //public void SetParameter(string parname, NativeObjectProxy mobject)
        //{

        //    //assign parameters and native object proxy
        //    //this.EnterContext();
        //    NativeV8JsInterOp.RegParamExternalManaged(
        //        nativeEngineContextProxy.UnmanagedPtr,
        //        parname,
        //        mobject.UnmanagedPtr);
        //    // this.ExitContext();
        //}

        public void RegisterTypeDefinition(JsTypeDefinition jsTypeDefinition)
        {
            proxyStore.CreateProxyForTypeDefinition(jsTypeDefinition);
        }
        //public void EnterContext()
        //{
        //    this.v8ContextLock.Push(NativeV8JsInterOp.EngineContextEnter(this.nativeEngineContextProxy.UnmanagedPtr));
        //}
        //public void ExitContext()
        //{
        //    NativeV8JsInterOp.EngineContextExit(this.nativeEngineContextProxy.UnmanagedPtr, this.v8ContextLock.Pop());
        //}

        public void Close()
        {
            this.proxyStore.Dispose();
            // NativeV8JsInterOp.ReleaseJsContext(this);

        }
        //public void Run(string striptSource)
        //{
        //    int runResult = NativeV8JsInterOp.EngineContextRun(nativeEngineContextProxy.UnmanagedPtr, striptSource);

        //}
    }



    public static class NativeV8JsInterOp
    {
        //-------------------------------------------------------------------------------
        //basic 
        static IntPtr hModuleV8;
        //static Dictionary<string, NativeMethodMap> importFuncs;

        //-------------------------------------------------------------------------------
        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetMiniBridgeVersion();

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TestCallBack();

        //static CreateWrapperForManagedObject CreateWrapperForManagedObject;
        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr CreateWrapperForManagedObject(IntPtr unmanagedEnginePtr, int mIndex, IntPtr rtTypeDefinition);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern int GetManagedIndex(IntPtr unmanagedPtr);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern int RelaseWrapper(IntPtr unmanagedPtr);
        //static RelaseWrapper ReleaseWrapper;
        //-------------------------------------------------------------------------------
        static ManagedListenerDel engineListenerDel;
        static ManageMethodCallDel engineMethodCallbackDel;
        //public static EngineContextRunDel EngineContextRun;
        //-------------------------------------------------------------------------------
        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.StdCall)]
        static extern void RegisterManagedCallback(IntPtr funcPointer, int callBackKind);
        //static RegisterManagedCallBack RegisterManagedCallback;

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern unsafe IntPtr EngineRegisterTypeDefinition(
            IntPtr unmanagedEnginePtr, int mIndex,
            void* stream, int length);


        //static EngineRegisterTypeDefinitionDel RegisterTypeDefinition;
        //static CreateEngineContextDel CreateEngineContext;
        //static ReleaseEngineContextDel ReleaseEngineContext;

        //public static RegisterExternalParameter_int32Del RegParamInt32;
        //public static RegisterExternalParameter_floatDel RegParamFloat;
        //public static RegisterExternalParameter_doubleDel RegParamDouble;
        //public static RegisterExternalParameter_stringDel RegParamString;
        //public static RegisterExternalParameter_ExternalManagedDel RegParamExternalManaged;

        //public static EngineContextEnterDel EngineContextEnter;
        //public static EngineContextExitDel EngineContextExit;

        //----------------------------------------------------------------------------
        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ArgGetAttachDataAsInt32(IntPtr unmanaedArgPtr);
        //public static ArgGetAttachDataAsInt32Del ArgGetAttachDataAsInt32;
        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ArgGetInt32(IntPtr unmanaedArgPtr, int argIndex);
        // public static ArgGetInt32Del ArgGetInt32;

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPWStr)]
        public static extern string ArgGetString(IntPtr unmanaedArgPtr, int argIndex);


        //----------------------------------------------------------------------------

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ArgSetString(IntPtr unmageReturnResult, [MarshalAs(UnmanagedType.LPWStr)] string value);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ArgSetBool(IntPtr unmageReturnResult, bool value);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ArgSetInt32(IntPtr unmageReturnResult, int value);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ArgSetDouble(IntPtr unmageReturnResult, double value);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ArgSetFloat(IntPtr unmageReturnResult, float value);
        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ArgSetNativeObject(IntPtr unmageReturnResult, int proxyIndex);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ReleaseWrapper(IntPtr externalManagedHandler);



        //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        //public delegate void ArgSetStringDel(IntPtr unmageReturnResult, [MarshalAs(UnmanagedType.LPWStr)] string value);
        //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        //public delegate void ArgSetInt32Del(IntPtr unmageReturnResult, int value);
        //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        //public delegate void ArgSetBoolDel(IntPtr unmageReturnResult, bool value);
        //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        //public delegate void ArgSetDoubleDel(IntPtr unmageReturnResult, double value);
        //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        //public delegate void ArgSetFloatDel(IntPtr unmageReturnResult, float value);

        //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        //public delegate void ArgSetNativeObjectDel(IntPtr unmageReturnResult, int proxyIndex);
        ////==================================================


        //public static LibGetVersionDel LibGetVersion;
        //----------------------------------------------------------------------------
        //MY_DLL_EXPORT int ArgGetInt32(const v8::Arguments* args,int index);
        //MY_DLL_EXPORT wchar_t* ArgGetString(const v8::Arguments* args,int index);
        //MY_DLL_EXPORT int ArgGetAttachDataAsInt32(const v8::Arguments* args,int index);

        static List<JsMethodDefinition> registerMethods = new List<JsMethodDefinition>();
        static NativeV8JsInterOp()
        {
            //prepare 
            engineListenerDel = new ManagedListenerDel(EngineListener_Listen);
            engineMethodCallbackDel = new ManageMethodCallDel(EngineListener_MethodCall);
            registerMethods.Add(null);  //fisrt is null***
        }

        static void RegisterManagedListener(ManagedListenerDel mListenerDel)
        {
            RegisterManagedCallback(
                 System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(mListenerDel),
                (int)ManagedCallbackKind.Listener);
        }
        static void RegisterManagedMethodCall(ManageMethodCallDel mMethodCall)
        {

            RegisterManagedCallback(
                System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(mMethodCall),
                (int)ManagedCallbackKind.MethodCall);
        }
        //public static JsContext CreateNewJsContext()
        //{

        //    JsContext nativeJsContext = new JsContext();
        //    NativeObjectProxy<JsContext> wrapJsContext = new NativeObjectProxy<JsContext>(1, nativeJsContext);
        //    IntPtr unmangedPtr = CreateEngineContext(wrapJsContext.ManagedIndex);
        //    wrapJsContext.SetUnmanagedObjectPointer(unmangedPtr);
        //    nativeJsContext.nativeEngineContextProxy = wrapJsContext;
        //    return nativeJsContext;
        //}
        //public static void ReleaseJsContext(JsContext jsContext)
        //{

        //    ReleaseEngineContext(jsContext.nativeEngineContextProxy.UnmanagedPtr);
        //    jsContext.nativeEngineContextProxy.SetUnmanagedObjectPointer(IntPtr.Zero);
        //}

        //public static void InvokeAction(string methodName, object[] parameters)
        //{
        //    NativeMethodMap foundMap;
        //    if (importFuncs.TryGetValue(methodName, out foundMap))
        //    {

        //        foundMap.GetDelegate().DynamicInvoke(foundMap);

        //    }
        //}
        //public static Delegate GetDelegate(string methodName)
        //{
        //    NativeMethodMap foundMap;
        //    if (importFuncs.TryGetValue(methodName, out foundMap))
        //    {
        //        return foundMap.GetDelegate();
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //public static object InvokeFunction(string methodName, object[] parameters)
        //{
        //    NativeMethodMap foundMap;
        //    if (importFuncs.TryGetValue(methodName, out foundMap))
        //    {

        //        return foundMap.GetDelegate().DynamicInvoke(parameters);
        //    }
        //    else
        //    {

        //        return null;
        //    }
        //}

        public static void LoadV8(string v8bridgeDll)
        {

            IntPtr v8mod = UnsafeMethods.LoadLibrary(v8bridgeDll);
            hModuleV8 = v8mod;

            if (v8mod == IntPtr.Zero)
            {
                return;
            }
            //--------------------------------------------------------
            //store native address of specific funcs
            //old technique***

            //List<NativeMethodMap> requestFuncs = new List<NativeMethodMap>()
            //{
            //    //new NativeFunctionMap<TestMe>("TestMe"),
            //    //new NativeFunctionMap<TestMe_WithScript>("TestMe_WithScript"),
            //    new NativeActionMap<LibGetVersionDel>("LibGetVersion"), 
            //    new NativeActionMap<CreateWrapperForManagedObject>("CreateWrapperForManagedObject"), 
            //    new NativeFunctionMap<GetManagedIndex>("GetManagedIndex"),
            //    new NativeFunctionMap<RelaseWrapper>("ReleaseWrapper"),
            //    //----------------------------------------------------------------------
            //    new NativeActionMap<RegisterManagedCallBack>("RegisterManagedCallback"),          
            //    new NativeFunctionMap<EngineRegisterTypeDefinitionDel>("EngineRegisterTypeDefinition"),
            //    //new NativeFunctionMap<CreateEngineContextDel>("CreateEngineContext"),
            //    //new NativeActionMap<ReleaseEngineContextDel>("ReleaseEngineContext"),
            //    ////----------------------------------------------------------------------
            //    //new NativeActionMap<RegisterExternalParameter_int32Del>("RegisterExternalParameter_int32"),
            //    //new NativeActionMap<RegisterExternalParameter_floatDel>("RegisterExternalParameter_float"),
            //    //new NativeActionMap<RegisterExternalParameter_doubleDel>("RegisterExternalParameter_double"),
            //    //new NativeActionMap<RegisterExternalParameter_stringDel>("RegisterExternalParameter_string"),
            //    //new NativeActionMap<RegisterExternalParameter_ExternalManagedDel>("RegisterExternalParameter_External"),
            //    ////----------------------------------------------------------------------
            //    //new NativeFunctionMap<EngineContextRunDel>("EngineContextRun"),
            //    //new NativeActionMap<EngineContextEnterDel>("EngineContextEnter"),
            //    //new NativeActionMap<EngineContextExitDel>("EngineContextExit"),
            //    //---------------------------------------------------------------------- 
            //    new NativeFunctionMap<ArgGetAttachDataAsInt32Del>("ArgGetAttachDataAsInt32"),
            //    new NativeFunctionMap<ArgGetInt32Del>("ArgGetInt32"),
            //    new NativeFunctionMap<ArgGetStringDel>("ArgGetString"),
            //    //---------------------------------------------------------------------- 
            //    new NativeActionMap<ArgSetInt32Del>("ArgSetInt32"),
            //    new NativeActionMap<ArgSetFloatDel>("ArgSetFloat"),
            //    new NativeActionMap<ArgSetDoubleDel>("ArgSetDouble"),
            //    new NativeActionMap<ArgSetStringDel>("ArgSetString"),
            //    new NativeActionMap<ArgSetBoolDel>("ArgSetBool"),
            //    new NativeActionMap<ArgSetNativeObjectDel>("ArgSetNativeObject"),
            //};
            //----------------------------------------------------------
            //importFuncs = new Dictionary<string, NativeMethodMap>();

            //int j = requestFuncs.Count;
            //for (int i = 0; i < j; ++i)
            //{
            //    NativeMethodMap met = requestFuncs[i];
            //    met.Resolve(v8mod);
            //    importFuncs.Add(met.NativeMethodName, met);
            //}
            //CreateWrapperForManagedObject = (CreateWrapperForManagedObject)importFuncs["CreateWrapperForManagedObject"].GetDelegate();
            //GetManagedIndex = (GetManagedIndex)importFuncs["GetManagedIndex"].GetDelegate();
            //ReleaseWrapper = (RelaseWrapper)importFuncs["ReleaseWrapper"].GetDelegate();
            //RegisterManagedCallback = (RegisterManagedCallBack)importFuncs["RegisterManagedCallback"].GetDelegate();

            ////------------------------------------
            ////
            //RegisterTypeDefinition = (EngineRegisterTypeDefinitionDel)importFuncs["EngineRegisterTypeDefinition"].GetDelegate();
            ////GCHandle.Alloc(RegisterTypeDefinition, GCHandleType.Pinned);
            ////---------
            ////engine
            //CreateEngineContext = (CreateEngineContextDel)importFuncs["CreateEngineContext"].GetDelegate();
            //ReleaseEngineContext = (ReleaseEngineContextDel)importFuncs["ReleaseEngineContext"].GetDelegate();
            ////parameter
            //RegParamInt32 = (RegisterExternalParameter_int32Del)importFuncs["RegisterExternalParameter_int32"].GetDelegate();
            //RegParamFloat = (RegisterExternalParameter_floatDel)importFuncs["RegisterExternalParameter_float"].GetDelegate();
            //RegParamDouble = (RegisterExternalParameter_doubleDel)importFuncs["RegisterExternalParameter_double"].GetDelegate();
            //RegParamString = (RegisterExternalParameter_stringDel)importFuncs["RegisterExternalParameter_string"].GetDelegate();
            //RegParamExternalManaged = (RegisterExternalParameter_ExternalManagedDel)importFuncs["RegisterExternalParameter_External"].GetDelegate();
            ////---------
            //EngineContextRun = (EngineContextRunDel)importFuncs["EngineContextRun"].GetDelegate();
            //EngineContextEnter = (EngineContextEnterDel)importFuncs["EngineContextEnter"].GetDelegate();
            //EngineContextExit = (EngineContextExitDel)importFuncs["EngineContextExit"].GetDelegate();
            ////---------
            //ArgGetInt32 = (ArgGetInt32Del)importFuncs["ArgGetInt32"].GetDelegate();
            //ArgGetString = (ArgGetStringDel)importFuncs["ArgGetString"].GetDelegate();
            //ArgGetAttachDataAsInt32 = (ArgGetAttachDataAsInt32Del)importFuncs["ArgGetAttachDataAsInt32"].GetDelegate();

            ////---------
            //ArgSetDouble = (ArgSetDoubleDel)importFuncs["ArgSetDouble"].GetDelegate();
            //ArgSetFloat = (ArgSetFloatDel)importFuncs["ArgSetFloat"].GetDelegate();
            //ArgSetString = (ArgSetStringDel)importFuncs["ArgSetString"].GetDelegate();
            //ArgSetInt32 = (ArgSetInt32Del)importFuncs["ArgSetInt32"].GetDelegate();
            //ArgSetBool = (ArgSetBoolDel)importFuncs["ArgSetBool"].GetDelegate();
            //ArgSetNativeObject = (ArgSetNativeObjectDel)importFuncs["ArgSetNativeObject"].GetDelegate();

            //LibGetVersion = (LibGetVersionDel)importFuncs["LibGetVersion"].GetDelegate();
            //------------------
            //built in listener
            //------------------
            //NativeV8JsInterOp.RegisterManagedListener(engineListenerDel);
            //NativeV8JsInterOp.RegisterManagedMethodCall(engineMethodCallbackDel);
            //-------------------
        }
        public static void RegisterCallBacks()
        {
            //------------------
            //built in listener
            //------------------
            NativeV8JsInterOp.RegisterManagedListener(engineListenerDel);
            NativeV8JsInterOp.RegisterManagedMethodCall(engineMethodCallbackDel);
        }
        public static int GetLibVersion()
        {
            return VroomJs.JsContext.getVersion();
        }
        public static void UnloadV8()
        {
            if (hModuleV8 != IntPtr.Zero)
            {
                UnsafeMethods.FreeLibrary(hModuleV8);
                hModuleV8 = IntPtr.Zero;
            }
        }

        static void EngineListener_Listen(int mIndex, string methodName, IntPtr args)
        {

        }


        static void EngineListener_MethodCall(int mIndex, IntPtr args, IntPtr result)
        {

            int data = NativeV8JsInterOp.ArgGetAttachDataAsInt32(args);
            if (data == 0)
            {
                return;
            }
            JsMethodDefinition foundMet = registerMethods[data];
            if (foundMet != null)
            {

                foundMet.InvokeMethod(new ManagedMethodArgs(args, result));
            }

        }
        static void CollectMethods(JsTypeDefinition jsTypeDefinition)
        {
            List<JsMethodDefinition> methods = jsTypeDefinition.GetMethods();
            int j = methods.Count;
            for (int i = 0; i < j; ++i)
            {
                JsMethodDefinition met = methods[i];
                met.SetMemberId(registerMethods.Count);
                registerMethods.Add(met);
            }
        }
        public static unsafe void RegisterTypeDef(JsContext2 context, JsTypeDefinition jsTypeDefinition)
        {

            NativeObjectProxy proxObject = jsTypeDefinition.nativeProxy;
            byte[] finalBuffer = null;
            using (MemoryStream ms = new MemoryStream())
            {
                //serialize with our custom protocol
                //plan change to json ?

                //utf16
                BinaryWriter binWriter = new BinaryWriter(ms, System.Text.Encoding.Unicode);
                //binay format 
                //1. typename
                //2. fields
                //3. method
                //4. indexer get/set   
                binWriter.Write((short)1);//start marker
                //data...

                //------------------------------------------------
                CollectMethods(jsTypeDefinition);
                jsTypeDefinition.WriteDefinitionToStream(binWriter);
                //------------------------------------------------
                finalBuffer = ms.ToArray();

                fixed (byte* tt = &finalBuffer[0])
                {
                    //context.EnterContext();                     
                    proxObject.SetUnmanagedObjectPointer(
                        EngineRegisterTypeDefinition(
                        context.nativeEngineContextProxy.UnmanagedPtr,
                        0, tt, finalBuffer.Length));
                    //context.ExitContext(); 
                }
                ms.Close();
            }
        }
        public static void CreateNativePart(JsContext context, NativeJsInstanceProxy proxyObj)
        {
            if (!proxyObj.HasNativeWrapperPart)
            {

                //context.EnterContext();                 
                proxyObj.SetUnmanagedObjectPointer(
                    CreateWrapperForManagedObject(
                        context.Handle.Handle,
                        proxyObj.ManagedIndex,
                        proxyObj.JsTypeDefinition.nativeProxy.UnmanagedPtr));
                //context.ExitContext();
            }
        }
        public static void UnRegisterNativePart(NativeObjectProxy proxyObj)
        {
            if (proxyObj.HasNativeWrapperPart)
            {
                ReleaseWrapper(proxyObj.UnmanagedPtr);
                proxyObj.SetUnmanagedObjectPointer(IntPtr.Zero);
            }
        }
        public static int GetManagedIndexFromNativePart(NativeObjectProxy proxyObj)
        {
            return GetManagedIndex(proxyObj.UnmanagedPtr);
        }

        //--------------------------------------------------------------------------
    }

}