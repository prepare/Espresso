//MIT, 2015-present, WinterDev, EngineKit, brezza92

using System;
using System.Collections.Generic;
using System.IO;

namespace Espresso
{

    public abstract class JsTypeMemberDefinition
    {
        readonly string _mbname;
        readonly JsMemberKind _memberKind;
        JsTypeDefinition _owner;
        int _memberId;
        internal INativeRef _nativeProxy;
        public JsTypeMemberDefinition(string mbname, JsMemberKind memberKind)
        {
            _mbname = mbname;
            _memberKind = memberKind;
        }
        public bool IsRegisterd => _nativeProxy != null;
        public string MemberName => _mbname;
        public JsMemberKind MemberKind => _memberKind;
        public void SetOwner(JsTypeDefinition owner) => _owner = owner;

        protected static void WriteUtf16String(string str, BinaryWriter writer)
        {
            char[] charBuff = str.ToCharArray();
            writer.Write((short)charBuff.Length);
            writer.Write(charBuff);
        }
        public int MemberId => _memberId;
        public void SetMemberId(int memberId) => _memberId = memberId;


    }
    public class JsTypeDefinition : JsTypeMemberDefinition
    {
        //store definition for js
        List<JsFieldDefinition> _fields = new List<JsFieldDefinition>();
        List<JsMethodDefinition> _methods = new List<JsMethodDefinition>();
        List<JsPropertyDefinition> _props = new List<JsPropertyDefinition>();

        public JsTypeDefinition(string typename)
            : base(typename, JsMemberKind.Type)
        {
        }
        public void AddMember(JsMethodDefinition methodDef)
        {
            methodDef.SetOwner(this);
            _methods.Add(methodDef);
        }
        public void AddMember(JsPropertyDefinition propDef)
        {
            propDef.SetOwner(this);
            _props.Add(propDef);

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
            int j = _fields.Count;
            writer.Write((short)j);
            for (int i = 0; i < j; ++i)
            {
                JsFieldDefinition fielddef = _fields[i];
                //field flags
                writer.Write((short)0);

                //*** field id -- unique field id within one type
                writer.Write((short)fielddef.MemberId);

                //field name
                WriteUtf16String(fielddef.MemberName, writer);
            }
            //------------------
            j = _methods.Count;
            writer.Write((short)j);
            for (int i = 0; i < j; ++i)
            {
                JsMethodDefinition methoddef = _methods[i];
                //method flags
                writer.Write((short)0);
                //id
                writer.Write((short)methoddef.MemberId);
                //method name
                WriteUtf16String(methoddef.MemberName, writer);
            }

            //property
            j = _props.Count;
            writer.Write((short)j);
            for (int i = 0; i < j; ++i)
            {
                JsPropertyDefinition property = _props[i];
                //flags
                writer.Write((short)0);
                //id
                writer.Write((short)property.MemberId);
                //name
                WriteUtf16String(property.MemberName, writer);
            }

        }

        internal List<JsFieldDefinition> GetFields() => _fields;

        internal List<JsMethodDefinition> GetMethods() => _methods;

        internal List<JsPropertyDefinition> GetProperties() => _props;

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
        public JsPropertyDefinition(string name)
            : base(name, JsMemberKind.Property)
        {
            //create blank property and we can add getter/setter later

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
        public JsPropertyDefinition(string name, System.Reflection.PropertyInfo propInfo)
            : base(name, JsMemberKind.Property)
        {

#if NET20

            var getter = propInfo.GetGetMethod(true);
            if (getter != null)
            {
                this.GetterMethod = new JsPropertyGetDefinition(name, getter);
            }
            var setter = propInfo.GetSetMethod(true);
            if (setter != null)
            {
                this.SetterMethod = new JsPropertySetDefinition(name, setter);
            }
#else
            var getter = propInfo.GetMethod;
            if (getter != null)
            {
                this.GetterMethod = new JsPropertyGetDefinition(name, getter);
            }
            var setter = propInfo.SetMethod;
            if (setter != null)
            {
                this.SetterMethod = new JsPropertySetDefinition(name, setter);
            }
#endif

        }
        public JsPropertyGetDefinition GetterMethod { get; set; }
        public JsPropertySetDefinition SetterMethod { get; set; }
        public bool IsIndexer { get; set; }
    }

    public class JsPropertyGetDefinition : JsMethodDefinition
    {

        public JsPropertyGetDefinition(string name, JsMethodCallDel getter)
            : base(name, getter)
        {
        }
        public JsPropertyGetDefinition(string name, System.Reflection.MethodInfo getterMethod)
            : base(name, getterMethod)
        {
        }
    }

    public class JsPropertySetDefinition : JsMethodDefinition
    {

        public JsPropertySetDefinition(string name, JsMethodCallDel setter)
            : base(name, setter)
        {
        }
        public JsPropertySetDefinition(string name, System.Reflection.MethodInfo setterMethod)
            : base(name, setterMethod)
        {
        }
    }

    public class JsMethodDefinition : JsTypeMemberDefinition
    {

        JsMethodCallDel _methodCallDel;
        System.Reflection.MethodInfo _method;
        System.Reflection.ParameterInfo[] _parameterInfoList;
        System.Type _methodReturnType;
        bool _isReturnTypeVoid;

        public JsMethodDefinition(string methodName, JsMethodCallDel methodCallDel)
            : base(methodName, JsMemberKind.Method)
        {
            _methodCallDel = methodCallDel;
        }

        public JsMethodDefinition(string methodName, System.Reflection.MethodInfo method)
            : base(methodName, JsMemberKind.Method)
        {
            _method = method;
            //analyze expected arg type
            //and conversion plan
            _parameterInfoList = method.GetParameters();
            _methodReturnType = method.ReturnType;
            _isReturnTypeVoid = _methodReturnType == typeof(void);
        }

        internal System.Reflection.ParameterInfo[] Parameters => _parameterInfoList;
        internal System.Reflection.MethodInfo MethodInfo => _method;
        internal JsMethodCallDel JsMetDelegate => _methodCallDel;

        public void InvokeMethod(ManagedMethodArgs args)
        {
            if (_method != null)
            {
                //invoke method

                object thisArg = args.GetThisArg();

                //actual input arg count
                int actualArgCount = args.ArgCount;
                //prepare parameters
                int expectedParameterCount = _parameterInfoList.Length;
                object[] parameters = new object[expectedParameterCount];

                //TODO: review here
                //check exact number
                int lim = Math.Min(actualArgCount, expectedParameterCount);
                //fill from the begin 
                for (int i = 0; i < lim; ++i)
                {
                    object arg = args.GetArgAsObject(i);
                    //if type not match then covert it
                    if (arg is JsFunction)
                    {
                        //convert to deledate
                        //check if the target need delegate
                        var func = (JsFunction)arg;
                        //create delegate for a specific target type***
                        parameters[i] = func.MakeDelegate(_parameterInfoList[i].ParameterType);
                    }
                    else
                    {
                        parameters[i] = arg;
                    }
                }

                //send to .net 
                object result = _method.Invoke(thisArg, parameters);

                if (_isReturnTypeVoid)
                {
                    //set to undefine because of void
                    args.SetResultUndefined();
                }
                else
                {
                    args.SetResultObj(result);
                }
            }
            else
            {
                _methodCallDel(args);
            }
        }




#if DEBUG
        public override string ToString()
        {
            return this.MemberName;
        }
#endif
    }

    public delegate void JsMethodCallDel(ManagedMethodArgs args);

    public struct JsArgValue
    {
        JsValue jsvalue;
    }


    public struct ManagedMethodArgs
    {
        IntPtr _metArgsPtr;
        JsContext _context;
        public ManagedMethodArgs(JsContext context, IntPtr metArgsPtr)
        {
            _context = context;
            _metArgsPtr = metArgsPtr;
        }
        public int ArgCount => NativeV8JsInterOp.ArgCount(_metArgsPtr);

        public object GetThisArg()
        {
            JsValue output = new JsValue();
            NativeV8JsInterOp.ArgGetThis(_metArgsPtr, ref output);
            return _context.Converter.FromJsValue(ref output);
        }
        public object GetArgAsObject(int index)
        {
            JsValue output = new JsValue();
            NativeV8JsInterOp.ArgGetObject(_metArgsPtr, index, ref output);
            return _context.Converter.FromJsValue(ref output);
        }
        //--------------------------------------------------------------------
        public void SetResult(bool value)
        {
            NativeV8JsInterOp.ResultSetBool(_metArgsPtr, value);
        }
        public void SetResult(int value)
        {
            NativeV8JsInterOp.ResultSetInt32(_metArgsPtr, value);
        }
        public void SetResult(string value)
        {
            NativeV8JsInterOp.ResultSetString(_metArgsPtr, value);
        }
        public void SetResult(double value)
        {
            NativeV8JsInterOp.ResultSetDouble(_metArgsPtr, value);
        }
        public void SetResult(float value)
        {
            NativeV8JsInterOp.ResultSetFloat(_metArgsPtr, value);
        }
        public void SetResultNull()
        {
            NativeV8JsInterOp.ResultSetJsNull(_metArgsPtr);
        }
        public void SetResultUndefined()
        {
            //TODO: review here again
            NativeV8JsInterOp.ResultSetJsVoid(_metArgsPtr);
        }
        public void SetResultObj(object result)
        {
            JsValue output = new JsValue();
            _context.Converter.AnyToJsValue(result, ref output);
            NativeV8JsInterOp.ResultSetValue(_metArgsPtr, ref output);
        }

        public void SetResultObj(object result, JsTypeDefinition jsTypeDef)
        {
            if (!jsTypeDef.IsRegisterd)
            {
                _context.RegisterTypeDefinition(jsTypeDef);
            }

            INativeScriptable proxy = _context.CreateWrapper(result, jsTypeDef);
            JsValue output = new JsValue();
            _context.Converter.ToJsValue(proxy, ref output);
            NativeV8JsInterOp.ResultSetValue(_metArgsPtr, ref output);
        }
        public void SetResultAutoWrap<T>(T result)
            where T : class, new()
        {

            Type actualType = result.GetType();
            JsTypeDefinition jsTypeDef = _context.GetJsTypeDefinition(actualType);
            INativeScriptable proxy = _context.CreateWrapper(result, jsTypeDef);
            JsValue output = new JsValue();
            _context.Converter.ToJsValue(proxy, ref output);
            NativeV8JsInterOp.ResultSetValue(_metArgsPtr, ref output);

        }

    }
}