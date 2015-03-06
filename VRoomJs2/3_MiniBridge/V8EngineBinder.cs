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
    public delegate void ManagedMethodCallDel(int mIndex, int hint, IntPtr metArgs);



    public class JsTypeDefinition : JsTypeMemberDefinition
    {
        //store definition for js
        List<JsFieldDefinition> fields = new List<JsFieldDefinition>();
        List<JsMethodDefinition> methods = new List<JsMethodDefinition>();
        List<JsPropertyDefinition> props = new List<JsPropertyDefinition>();

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
        public void AddMember(JsPropertyDefinition propDef)
        {
            propDef.SetOwner(this);
            props.Add(propDef);

        }
        /// <summary>
        /// serialization this typedefinition to binary format and 
        /// send to native side
        /// </summary>
        /// <param name="writer"></param>
        internal void WriteDefinitionToStream(BinaryWriter writer)
        {
            //----------------------
            //this is our custom protocol/convention with the MiniJsBridge            
            //we may change this in the future
            //eg. use json serialization/deserialization 
            //----------------------

            //1. kind/flags
            writer.Write((short)this.MemberId);
            //2. member id
            writer.Write((short)0);
            //3. typename                         
            WriteUtf16String(this.MemberName, writer);

            //4. num of field
            int j = fields.Count;
            writer.Write((short)j);
            for (int i = 0; i < j; ++i)
            {
                JsFieldDefinition fielddef = fields[i];
                //field flags
                writer.Write((short)0);

                //*** field id -- unique field id within one type
                writer.Write((short)fielddef.MemberId);

                //field name
                WriteUtf16String(fielddef.MemberName, writer);
            }
            //------------------
            j = methods.Count;
            writer.Write((short)j);
            for (int i = 0; i < j; ++i)
            {
                JsMethodDefinition methoddef = methods[i];
                //method flags
                writer.Write((short)0);
                //id
                writer.Write((short)methoddef.MemberId);
                //method name
                WriteUtf16String(methoddef.MemberName, writer);
            }

            //property
            j = props.Count;
            writer.Write((short)j);
            for (int i = 0; i < j; ++i)
            {
                JsPropertyDefinition property = this.props[i];
                //flags
                writer.Write((short)0);
                //id
                writer.Write((short)property.MemberId);
                //name
                WriteUtf16String(property.MemberName, writer);
            }

        }

        internal List<JsFieldDefinition> GetFields()
        {
            return this.fields;
        }
        internal List<JsMethodDefinition> GetMethods()
        {
            return this.methods;
        }
        internal List<JsPropertyDefinition> GetProperties()
        {
            return this.props;
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
        Property,
        Indexer,
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

    public class JsPropertyDefinition : JsTypeMemberDefinition
    {
        JsPropertyGetDefinition getter;
        JsPropertyGetDefinition setter;

        public JsPropertyDefinition(string name)
            : base(name, JsMemberKind.Property)
        {

        }
        public JsPropertyDefinition(string name, JsMethodCallDel getter, JsMethodCallDel setter)
            : base(name, JsMemberKind.Property)
        {

            if (getter != null)
            {
                this.GetterMethod = new JsPropertyGetDefinition(name, getter);
            }
            if (setter != null)
            {
                this.SetterMethod = new JsPropertySetDefinition(name, setter);
            }
        }
        public JsPropertyGetDefinition GetterMethod
        {
            get;
            set;
        }
        public JsPropertySetDefinition SetterMethod
        {
            get;
            set;
        }
        public bool IsIndexer { get; set; }
    }

    public class JsPropertyGetDefinition : JsMethodDefinition
    {
        public JsPropertyGetDefinition(string name)
            : base(name)
        {
        }
        public JsPropertyGetDefinition(string name, JsMethodCallDel getter)
            : base(name, getter)
        {
        }
    }
    public class JsPropertySetDefinition : JsMethodDefinition
    {
        public JsPropertySetDefinition(string name)
            : base(name)
        {
        }
        public JsPropertySetDefinition(string name, JsMethodCallDel setter)
            : base(name, setter)
        {
        }
    }



    public delegate void JsMethodCallDel(ManagedMethodArgs args);

    public struct ManagedMethodArgs
    {
        IntPtr metArgsPtr;
        public ManagedMethodArgs(IntPtr metArgsPtr)
        {
            this.metArgsPtr = metArgsPtr;
        }
        public string GetArgAsString(int index)
        {
            return NativeV8JsInterOp.ArgGetString(this.metArgsPtr, index);
        }
        public int GetArgAsInt32(int index)
        {
            return NativeV8JsInterOp.ArgGetInt32(this.metArgsPtr, index);
        }
        //------------------------
        public void SetResult(bool value)
        {
            NativeV8JsInterOp.ResultSetBool(metArgsPtr, value);
        }
        public void SetResult(int value)
        {
            NativeV8JsInterOp.ResultSetInt32(metArgsPtr, value);
        }
        public void SetResult(string value)
        {
            NativeV8JsInterOp.ResultSetString(metArgsPtr, value);
        }
        public void SetResult(double value)
        {
            NativeV8JsInterOp.ResultSetDouble(metArgsPtr, value);
        }
        public void SetResult(float value)
        {
            NativeV8JsInterOp.ResultSetFloat(metArgsPtr, value);
        }
        public void SetNativeObjResult(int value)
        {
            NativeV8JsInterOp.ResultSetNativeObject(metArgsPtr, value);
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
        //basic 
        static IntPtr hModuleV8;
        static ManagedListenerDel engineListenerDel;
       



        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TestCallBack();


        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr CreateWrapperForManagedObject(IntPtr unmanagedEnginePtr, int mIndex, IntPtr rtTypeDefinition);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern int GetManagedIndex(IntPtr unmanagedPtr);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern int RelaseWrapper(IntPtr unmanagedPtr);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.StdCall)]
        static extern void RegisterManagedCallback(IntPtr funcPointer, int callBackKind);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.StdCall)]
        static extern void ContextRegisterManagedCallback(IntPtr contextPtr, IntPtr funcPointer, int callBackKind);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern unsafe IntPtr ContextRegisterTypeDefinition(
            IntPtr unmanagedEnginePtr, int mIndex,
            void* stream, int length);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ArgGetInt32(IntPtr unmanaedArgPtr, int argIndex);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPWStr)]
        public static extern string ArgGetString(IntPtr unmanaedArgPtr, int argIndex);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ResultSetString(IntPtr unmageReturnResult, [MarshalAs(UnmanagedType.LPWStr)] string value);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ResultSetBool(IntPtr unmageReturnResult, bool value);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ResultSetInt32(IntPtr unmageReturnResult, int value);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ResultSetDouble(IntPtr unmageReturnResult, double value);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ResultSetFloat(IntPtr unmageReturnResult, float value);
        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ResultSetNativeObject(IntPtr unmageReturnResult, int proxyIndex);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ReleaseWrapper(IntPtr externalManagedHandler);

         
        static NativeV8JsInterOp()
        {
            //prepare 
            engineListenerDel = new ManagedListenerDel(EngineListener_Listen);  
        }

        static void RegisterManagedListener(ManagedListenerDel mListenerDel)
        {
            RegisterManagedCallback(
                 System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(mListenerDel),
                (int)ManagedCallbackKind.Listener);
        }

        internal static void CtxRegisterManagedMethodCall(JsContext jsContext, ManagedMethodCallDel mMethodCall)
        {
            ContextRegisterManagedCallback(
                jsContext.Handle.Handle,
                System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(mMethodCall),
                (int)ManagedCallbackKind.MethodCall);
        }
        public static void LoadV8(string v8bridgeDll)
        {

            IntPtr v8mod = UnsafeMethods.LoadLibrary(v8bridgeDll);
            hModuleV8 = v8mod;
            if (v8mod == IntPtr.Zero)
            {
                return;
            }
        }
        public static void RegisterCallBacks()
        {
            //------------------
            //built in listener
            //------------------
            NativeV8JsInterOp.RegisterManagedListener(engineListenerDel);
            //NativeV8JsInterOp.RegisterManagedMethodCall(engineMethodCallbackDel);
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
        //static void EngineListener_MethodCall(int mIndex, int methodKind, IntPtr metArgs)
        //{  
        //    switch (methodKind)
        //    {
        //        case 1:
        //            {
        //                //property get        
        //                if (mIndex == 0) return;
        //                //------------------------------------------
        //                JsMethodDefinition getterMethod = registerProperties[mIndex].GetterMethod;

        //                if (getterMethod != null)
        //                {
        //                    getterMethod.InvokeMethod(new ManagedMethodArgs(metArgs));
        //                }

        //            } break;
        //        case 2:
        //            {
        //                //property set
        //                if (mIndex == 0) return;
        //                //------------------------------------------
        //                JsMethodDefinition setterMethod = registerProperties[mIndex].SetterMethod;
        //                if (setterMethod != null)
        //                {
        //                    setterMethod.InvokeMethod(new ManagedMethodArgs(metArgs));
        //                }
        //            } break;
        //        default:
        //            {
        //                if (mIndex == 0) return;
        //                JsMethodDefinition foundMet = registerMethods[mIndex];
        //                if (foundMet != null)
        //                {
        //                    foundMet.InvokeMethod(new ManagedMethodArgs(metArgs));
        //                }
        //            } break;
        //    }


        //}
        //static void CollectMembers(JsTypeDefinition jsTypeDefinition)
        //{
        //    List<JsMethodDefinition> methods = jsTypeDefinition.GetMethods();
        //    int j = methods.Count;
        //    for (int i = 0; i < j; ++i)
        //    {
        //        JsMethodDefinition met = methods[i];
        //        met.SetMemberId(registerMethods.Count);
        //        registerMethods.Add(met);
        //    }

        //    List<JsPropertyDefinition> properties = jsTypeDefinition.GetProperties();
        //    j = properties.Count;
        //    for (int i = 0; i < j; ++i)
        //    {
        //        var p = properties[i];
        //        p.SetMemberId(registerProperties.Count);
        //        registerProperties.Add(p);
        //    }
        //}

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
                context.myjsContext.CollectionTypeMembers(jsTypeDefinition);
                //------------------------------------------------
                 
                jsTypeDefinition.WriteDefinitionToStream(binWriter);
                //------------------------------------------------
                finalBuffer = ms.ToArray();

                fixed (byte* tt = &finalBuffer[0])
                {

                    proxObject.SetUnmanagedObjectPointer(
                        ContextRegisterTypeDefinition(
                        context.nativeEngineContextProxy.UnmanagedPtr,
                        0, tt, finalBuffer.Length));
                }
                ms.Close();
            }
        }
        public static void CreateNativePart(JsContext context, NativeJsInstanceProxy proxyObj)
        {
            if (!proxyObj.HasNativeWrapperPart)
            {
                proxyObj.SetUnmanagedObjectPointer(
                    CreateWrapperForManagedObject(
                        context.Handle.Handle,
                        proxyObj.ManagedIndex,
                        proxyObj.JsTypeDefinition.nativeProxy.UnmanagedPtr));
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
    }


    public enum ManagedCallbackKind : int
    {
        Listener,
        MethodCall,
    }

    static class UnsafeMethods
    {

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