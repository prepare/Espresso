//MIT, 2015-2017, WinterDev, EngineKit, brezza92

// This file is part of the VroomJs library.
//
// Author:
//     Federico Di Gregorio <fog@initd.org>
//
// Copyright © 2013 Federico Di Gregorio <fog@initd.org>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Espresso.Extension;

namespace Espresso
{
    /// <summary>
    /// js execution context
    /// </summary>
    public partial class JsContext : IDisposable
    {

        readonly int _id;
        readonly JsEngine _engine;
        readonly ManagedMethodCallDel engineMethodCallbackDel;
        readonly HandleRef _context; //native js context
        readonly Action<int> _notifyDispose;

        List<JsMethodDefinition> registerMethods = new List<JsMethodDefinition>();
        List<JsPropertyDefinition> registerProperties = new List<JsPropertyDefinition>();

        Dictionary<Type, JsTypeDefinition> mappingJsTypeDefinition = new Dictionary<Type, JsTypeDefinition>();
        Dictionary<Type, DelegateTemplate> cachedDelSamples = new Dictionary<Type, DelegateTemplate>();

        NativeObjectProxyStore proxyStore;
        JsTypeDefinitionBuilder jsTypeDefBuilder;

        /// <summary>
        /// converter object for this context
        /// </summary>
        readonly JsConvert _convert;
        // Keep objects passed to V8 alive even if no other references exist.
        readonly IKeepAliveStore _keepalives;
        // 
        internal JsContext(int id,
            JsEngine engine,
            Action<int> notifyDispose,
            JsTypeDefinitionBuilder jsTypeDefBuilder)
            : this(id, engine, notifyDispose, jscontext_new(id, engine.UnmanagedEngineHandler), jsTypeDefBuilder) { }
        internal JsContext(int id,
            JsEngine engine,
            Action<int> notifyDispose,
            IntPtr nativeJsContext,
            JsTypeDefinitionBuilder jsTypeDefBuilder)
        {

            //constructor setup
            _id = id;
            _notifyDispose = notifyDispose;
            _engine = engine;
            _keepalives = new KeepAliveDictionaryStore();
            //create native js context
            _context = new HandleRef(this, nativeJsContext);
            _convert = new JsConvert(this);

            this.jsTypeDefBuilder = jsTypeDefBuilder;

            engineMethodCallbackDel = new ManagedMethodCallDel(EngineListener_MethodCall);
            NativeV8JsInterOp.CtxRegisterManagedMethodCall(this, engineMethodCallbackDel);
            registerMethods.Add(null);//first is null
            registerProperties.Add(null); //first is null 
            proxyStore = new NativeObjectProxyStore(this);
        }

        internal INativeRef GetObjectProxy(int index)
        {
            return this.proxyStore.GetProxyObject(index);
        }


        internal JsConvert Converter
        {
            get { return this._convert; }
        }
        internal void CollectionTypeMembers(JsTypeDefinition jsTypeDefinition)
        {

            List<JsMethodDefinition> methods = jsTypeDefinition.GetMethods();
            int j = methods.Count;
            for (int i = 0; i < j; ++i)
            {
                JsMethodDefinition met = methods[i];
                met.SetMemberId(registerMethods.Count);
                registerMethods.Add(met);
            }

            List<JsPropertyDefinition> properties = jsTypeDefinition.GetProperties();
            j = properties.Count;
            for (int i = 0; i < j; ++i)
            {
                JsPropertyDefinition p = properties[i];
                p.SetMemberId(registerProperties.Count);
                registerProperties.Add(p);
            }

        }

        void EngineListener_MethodCall(int mIndex, int methodKind, IntPtr metArgs)
        {
            switch (methodKind)
            {
                case 1:
                    {
                        //property get        
                        if (mIndex == 0) return;
                        //------------------------------------------
                        JsMethodDefinition getterMethod = registerProperties[mIndex].GetterMethod;

                        if (getterMethod != null)
                        {
                            getterMethod.InvokeMethod(new ManagedMethodArgs(this, metArgs));
                        }

                    }
                    break;
                case 2:
                    {
                        //property set
                        if (mIndex == 0) return;
                        //------------------------------------------
                        JsMethodDefinition setterMethod = registerProperties[mIndex].SetterMethod;
                        if (setterMethod != null)
                        {
                            setterMethod.InvokeMethod(new ManagedMethodArgs(this, metArgs));
                        }
                    }
                    break;
                default:
                    {
                        if (mIndex == 0) return;
                        JsMethodDefinition foundMet = registerMethods[mIndex];
                        if (foundMet != null)
                        {
                            foundMet.InvokeMethod(new ManagedMethodArgs(this, metArgs));
                        }
                    }
                    break;
            }
        }
        public JsEngine Engine
        {
            get { return _engine; }
        }
        public HandleRef NativeContextHandle
        {
            get { return _context; }
        }
        public JsEngineStats GetStats()
        {
            return new JsEngineStats
            {
                KeepAliveMaxSlots = _keepalives.MaxSlots,
                KeepAliveAllocatedSlots = _keepalives.AllocatedSlots,
                KeepAliveUsedSlots = _keepalives.UsedSlots
            };
        }

        public object Execute(JsScript script, TimeSpan? executionTimeout = null)
        {
            if (script == null)
                throw new ArgumentNullException("script");

            CheckDisposed();

            bool executionTimedOut = false;
            Timer timer = null;
            if (executionTimeout.HasValue)
            {
                timer = new Timer(executionTimeout.Value.TotalMilliseconds);
                timer.Elapsed += (sender, args) =>
                {
                    timer.Stop();
                    executionTimedOut = true;
                    _engine.TerminateExecution();
                };
                timer.Start();
            }
            object res;
            try
            {
                JsValue v = new JsValue();
                jscontext_execute_script(_context, script.Handle, ref v);
                res = _convert.FromJsValue(ref v);
#if DEBUG_TRACE_API
        	Console.WriteLine("Cleaning up return value from execution");
#endif
                v.Dispose();
            }
            finally
            {
                if (executionTimeout.HasValue)
                {
                    timer.Dispose();
                }
            }

            if (executionTimedOut)
            {
                throw new JsExecutionTimedOutException();
            }

            Exception e = res as JsException;
            if (e != null)
                throw e;
            return res;
        }

        public object Execute(string code, string name = null, TimeSpan? executionTimeout = null)
        {
            //Stopwatch watch1 = new Stopwatch();
            //Stopwatch watch2 = new Stopwatch(); 
            //watch1.Start();
            if (code == null)
                throw new ArgumentNullException("code");

            CheckDisposed();

            bool executionTimedOut = false;
            Timer timer = null;
            if (executionTimeout.HasValue)
            {
                timer = new Timer(executionTimeout.Value.TotalMilliseconds);
                timer.Elapsed += (sender, args) =>
                {
                    timer.Stop();
                    executionTimedOut = true;
                    _engine.TerminateExecution();
                };
                timer.Start();
            }
            object res = null;
            try
            {
                //watch2.Start(); 
                JsValue output = new JsValue();
                jscontext_execute(_context, code, name ?? "<Unnamed Script>", ref output);

                //watch2.Stop();                 
                res = _convert.FromJsValue(ref output);
#if DEBUG_TRACE_API
        	Console.WriteLine("Cleaning up return value from execution");
#endif
                output.Dispose();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                if (executionTimeout.HasValue)
                {
                    timer.Dispose();
                }
            }

            if (executionTimedOut)
            {
                throw new JsExecutionTimedOutException();
            }

            Exception e = res as JsException;
            if (e != null)
                throw e;
            //watch1.Stop(); 
            // Console.WriteLine("Execution time " + watch2.ElapsedTicks + " total time " + watch1.ElapsedTicks);
            return res;
        }

        public object GetGlobal()
        {
            CheckDisposed();
            JsValue v = new JsValue();
            jscontext_get_global(_context, ref v);
            object res = _convert.FromJsValue(ref v);

            v.Dispose();
            Exception e = res as JsException;
            if (e != null)
                throw e;
            return res;
        }

        public object GetVariable(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();

            JsValue v = new JsValue();
            jscontext_get_variable(_context, name, ref v);
            object res = _convert.FromJsValue(ref v);
#if DEBUG_TRACE_API
			Console.WriteLine("Cleaning up return value get variable.");
#endif

            v.Dispose();
            Exception e = res as JsException;
            if (e != null)
                throw e;
            return res;
        }



        public void SetFunction(string name, Delegate func)
        {

            WeakDelegate del;
#if NET20

            if (func.Target != null)
            {
                del = new BoundWeakDelegate(func.Target, func.Method.Name);
            }
            else
            {
                del = new BoundWeakDelegate(func.Method.DeclaringType, func.Method.Name);
            }

#else
            MethodInfo mInfo = func.GetMethodInfo();
            if (func.Target != null)
            {
                del = new BoundWeakDelegate(func.Target, mInfo.Name);//.Method.Name);
            }
            else
            {
                //del = new BoundWeakDelegate(func.Method.DeclaringType, func.Method.Name);
                del = new BoundWeakDelegate(mInfo.DeclaringType, mInfo.Name);
            }
#endif
            this.SetVariableFromAny(name, del);
        }

        public void Flush()
        {
            jscontext_force_gc();
        }

        internal int KeepAliveAdd(object obj)
        {
            return _keepalives.Register(obj);
        }

        internal object KeepAliveGet(int slot)
        {
            return _keepalives.Get(slot);
        }

        internal void KeepAliveRemove(int slot)
        {
            _keepalives.Remove(slot);
        }



        bool _disposed;
        public bool IsDisposed
        {
            get { return _disposed; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            CheckDisposed();

            _disposed = true;

            jscontext_dispose(_context);

            if (disposing)
            {
                _keepalives.Clear();
            }

            _notifyDispose(_id);
        }

        void CheckDisposed()
        {
            if (_engine.IsDisposed)
            {
                throw new ObjectDisposedException("JsContext: engine has been disposed");
            }
            if (_disposed)
                throw new ObjectDisposedException("JsContext:" + _context.Handle);
        }

        ~JsContext()
        {
            if (!_engine.IsDisposed && !_disposed)
                Dispose(false);
        }


        internal bool TrySetMemberValue(Type type, object obj, string name, ref JsValue value)
        {
            // dictionaries.
            if (typeof(IDictionary).ExtIsAssignableFrom(type))
            {
                //this object has IDictionary interface ***
                IDictionary dictionary = (IDictionary)obj;
                _convert.FromJsValue(ref value);
                dictionary[name] = _convert.FromJsValue(ref value);
                return true;//success
            }
            //if not => then check property set ***
            PropertyInfo prop_withSetter = type.ExtGetPropertySetter(obj, name);
            if (prop_withSetter != null)
            {
                //set value by call this setter 
                prop_withSetter.SetValue(obj, _convert.FromJsValue(ref value), null);
                return true; //success
            }
            //TODO:check public field for this? 
            return false; //not found this proper field or 
        }

        internal void KeepAliveSetPropertyValue(int slot, string name, ref JsValue v, ref JsValue output)
        {
#if DEBUG_TRACE_API
			Console.WriteLine("setting prop " + name);
#endif
            // TODO: This is pretty slow: use a cache of generated code to make it faster.

            var obj = KeepAliveGet(slot);
            if (obj != null)
            {
                Type type;
                if (obj is Type)
                {
                    type = (Type)obj;
                }
                else
                {
                    type = obj.GetType();
                }
#if DEBUG_TRACE_API
				Console.WriteLine("setting prop " + name + " type " + type);
#endif
                try
                {

                    if (!string.IsNullOrEmpty(name))
                    {
                        //this version we don't  implicit auto convert to upper camel case...  
                        if (TrySetMemberValue(type, obj, name, ref v))
                        {
                            //no error 
                            output.Type = JsValueType.Empty;
                            return;
                        }
                    }
                    //TODO: review how to handle error again
                    output.Type = JsValueType.Error; //error on set property
                    output.I64 = (int)JsManagedError.SetPropertyNotFound;
                    //TODO: review if we need to store the exception information on keepalive-store or not
                    return;
                    //return JsValue.Error(KeepAliveAdd(
                    //    new InvalidOperationException(String.Format("property not found on {0}: {1} ", type, name))));
                }
                catch (Exception e)
                {
                    output.Type = JsValueType.Error; //error on set property
                    output.I64 = (int)JsManagedError.SetPropertyError;
                    return;
                    //TODO: review how to handle error again
                    //return JsValue.Error(KeepAliveAdd(e));
                }
            }
            output.Type = JsValueType.Error; //error on set property
            output.I64 = (int)JsManagedError.NotFoundManagedObjectId;
            return;
            //return JsValue.Error(KeepAliveAdd(new IndexOutOfRangeException("invalid keepalive slot: " + slot)));
        }

        internal bool TryGetMemberValue(Type type, object obj, string name, ref JsValue value)
        {
            object result;
            // dictionaries.
            if (typeof(IDictionary).ExtIsAssignableFrom(type))
            {
                //this implement IDic
                IDictionary dictionary = (IDictionary)obj;
                if (dictionary.Contains(name))
                {
                    result = dictionary[name];
                    _convert.AnyToJsValue(result, ref value);
                }
                else
                {
                    value.Type = JsValueType.Null;
                }
                return true;
            }


            //try public property
            PropertyInfo pi = type.ExtGetProperty(obj, name);
            if (pi != null)
            {
                result = pi.GetValue(obj, null);
                _convert.AnyToJsValue(result, ref value);
                return true;
            }
            // try field.
            FieldInfo fi = type.ExtGetField(obj, name);
            if (fi != null)
            {
                result = fi.GetValue(obj);
                _convert.AnyToJsValue(result, ref value);
                return true;
            }

            // Then with an instance method: the problem is that we don't have a list of
            // parameter types so we just check if any method with the given name exists
            // and then keep alive a "weak delegate", i.e., just a name and the target.
            // The real method will be resolved during the invokation itself.

            //TODO: check if we should use 'method-group' instead of first found method 
            throw new NotSupportedException();

            //BindingFlags mFlags = flags | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy;

            //// TODO: This is probably slooow.
            //foreach (var met in type.GetMembers(flags))
            //{
            //    //TODO: review here
            //    //only 1 ?
            //    //or list of all method with the same name
            //    if (met.Name == name)
            //    {
            //        if (type == obj)
            //        {
            //            result = new WeakDelegate(type, name);
            //        }
            //        else
            //        {
            //            result = new WeakDelegate(obj, name);
            //        }
            //        _convert2.AnyToJsValue(result, ref value);
            //        return true;
            //    }
            //}
            ////if (type.GetMethods(mFlags).Any(x => x.Name == name))
            ////{
            ////    if (type == obj)
            ////    {
            ////        result = new WeakDelegate(type, name);
            ////    }
            ////    else
            ////    {
            ////        result = new WeakDelegate(obj, name);
            ////    }
            ////    value = _convert.ToJsValue(result);
            ////    return true;
            ////}

            value.Type = JsValueType.Null;
            return false;
        }

        internal void KeepAliveGetPropertyValue(int slot, string name, ref JsValue output)
        {

            //TODO: review exception again
#if DEBUG_TRACE_API
			Console.WriteLine("getting prop " + name);
#endif
            // we need to fall back to the prototype verison we set up because v8 won't call an object as a function, it needs
            // to be from a proper FunctionTemplate.
            //TODO: review here again
            if (!string.IsNullOrEmpty(name) && name.Equals("valueOf", StringComparison.OrdinalIgnoreCase))
            {
                output.Type = JsValueType.Empty;
                return;
            }

            // TODO: This is pretty slow: use a cache of generated code to make it faster.
            var obj = KeepAliveGet(slot);
            if (obj != null)
            {
                Type type;
                if (obj is Type)
                {
                    type = (Type)obj;
                }
                else
                {
                    type = obj.GetType();
                }
#if DEBUG_TRACE_API
				Console.WriteLine("getting prop " + name + " type " + type);
#endif
                try
                {
                    if (!string.IsNullOrEmpty(name))
                    {

                        //this version we don't implicit convert to CamelCase 
                        //var upperCamelCase = Char.ToUpper(name[0]) + name.Substring(1); 
                        //if (TryGetMemberValue(type, obj, upperCamelCase, ref output))
                        //{
                        //    return;
                        //}
                        if (TryGetMemberValue(type, obj, name, ref output))
                        {
                            return;
                        }
                    }
                    output.Type = JsValueType.Error; //error on set property
                    output.I64 = (int)JsManagedError.GetPropertyNotFound;
                    return;
                }
                catch (TargetInvocationException e)
                {
                    // Client code probably isn't interested in the exception part related to
                    // reflection, so we unwrap it and pass to V8 only the real exception thrown.
                    //if (e.InnerException != null)
                    //    return JsValue.Error(KeepAliveAdd(e.InnerException));
                    //throw;
                    output.Type = JsValueType.Error; //error on set property
                    output.I64 = (int)JsManagedError.GetPropertyNotFound;
                    throw;
                }
                catch (Exception e)
                {
                    output.Type = JsValueType.Error; //error on set property
                    output.I64 = (int)JsManagedError.SetKeepAliveError;
                    return;
                }
            }
            output.Type = JsValueType.Error; //error on set property
            output.I64 = (int)JsManagedError.SetKeepAliveError;
            return;
        }


        static readonly Type[] s_emptyTypeArr = new Type[0];
        internal void KeepAliveGetValueOf(int slot, ref JsValue output)
        {
            object obj = KeepAliveGet(slot);
            if (obj == null)
            {
                output.Type = JsValueType.Error;
                output.I64 = (int)JsManagedError.NotFoundManagedObjectId;
                return;
            }
            // 
            Type type = obj.GetType();
            MethodInfo mi;
#if NET20
            mi = type.GetMethod("valueOf") ?? type.GetMethod("ValueOf");
#else
            mi = type.GetRuntimeMethod("ValueOf", s_emptyTypeArr);
#endif
            if (mi != null)
            {
                //shoul be static value
                object result = mi.Invoke(obj, null);//no parameter of this value then set to null
                _convert.AnyToJsValue(result, ref output);
                return;
            }
            _convert.AnyToJsValue(obj, ref output);
        }
        internal void KeepAliveInvoke(int slot, ref JsValue args, ref JsValue output)
        {
            // TODO: This is pretty slow: use a cache of generated code to make it faster.
#if DEBUG_TRACE_API
			Console.WriteLine("invoking");
#endif
            //Console.WriteLine(args);

            object obj = KeepAliveGet(slot);
            if (obj == null)
            {
                output.Type = JsValueType.Error;
                output.I64 = (int)JsManagedError.NotFoundManagedObjectId;
                return;
            }

            Type constructorType = obj as Type;
            if (constructorType != null)
            {
#if DEBUG_TRACE_API
					Console.WriteLine("constructing " + constructorType.Name);
#endif
                object[] constructorArgs = (object[])_convert.FromJsValue(ref args);
                //TODO: review here
                _convert.AnyToJsValue(
                    Activator.CreateInstance(constructorType, constructorArgs), ref output);
                return;
            }
            //expect slot is del
            WeakDelegate func = obj as WeakDelegate;
            if (func == null)
            {
                throw new Exception("not a function.");
            }

            //owner type of the delegate
            Type type = func.Target != null ? func.Target.GetType() : func.Type;
#if DEBUG_TRACE_API
				Console.WriteLine("invoking " + obj.Target + " method " + obj.MethodName);
#endif

            //review delegate invocation again  
            object[] argObjects = (object[])_convert.FromJsValue(ref args);

            int j = argObjects.Length;
            for (int i = 0; i < j; ++i)
            {
                object a_elem = argObjects[i];
                if (a_elem.GetType() == typeof(JsFunction))
                {
                    CheckAndResolveJsFunctions(func, (JsFunction)a_elem, obj, type, func.MethodName, argObjects);
                    break;
                }
            }

            throw new NotSupportedException();
            //try
            //{
            //    object result = type.InvokeMember(func.MethodName, flags, null, func.Target, a);
            //    _convert.AnyToJsValue(result, ref output);
            //    return;
            //}
            //catch (TargetInvocationException e)
            //{
            //    output.Type = JsValueType.Error;
            //    output.I64 = (int)JsManagedError.TargetInvocationError;
            //    return;
            //}
            //catch (Exception e)
            //{
            //    //review set error
            //    output.Type = JsValueType.Error;
            //    output.I64 = (int)JsManagedError.SetKeepAliveError;
            //    return;
            //} 
        }
        static void CheckAndResolveJsFunctions(WeakDelegate weakDel,
          JsFunction func,
          object obj,
          Type type,
          string methodName,
          object[] args)
        {
#if NET20
            //find proper method
            BindingFlags flags = BindingFlags.Public
                    | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy;

            if (weakDel.Target != null)
            {
                flags |= BindingFlags.Instance;
            }
            else
            {
                flags |= BindingFlags.Static;
            }

            if (obj is BoundWeakDelegate)
            {
                flags |= BindingFlags.NonPublic;
            }
#else

#endif
            MethodInfo foundMet = type.ExtGetMethod(obj, methodName);
            if (foundMet != null)
            {

            }
            // need to convert methods from JsFunction's into delegates?
            throw new NotSupportedException();
            //MethodInfo mi = type.GetMethod(methodName, flags);
            //ParameterInfo[] paramTypes = mi.GetParameters();

            //for (int i = Math.Min(paramTypes.Length, args.Length) - 1; i >= 0; --i)
            //{
            //    if (args[i] != null && args[i].GetType() == typeof(JsFunction))
            //    {
            //        JsFunction function = (JsFunction)args[i];
            //        args[i] = function.MakeDelegate(paramTypes[i].ParameterType);
            //    }
            //}
        }
        internal void KeepAliveDeleteProperty(int slot, string name, ref JsValue output)
        {
#if DEBUG_TRACE_API
			Console.WriteLine("deleting prop " + name);
#endif
            // TODO: This is pretty slow: use a cache of generated code to make it faster.
            var obj = KeepAliveGet(slot);
            if (obj == null)
            {
                output.Type = JsValueType.Error;
                output.I64 = (int)JsManagedError.NotFoundManagedObjectId;
                return;
            }

#if DEBUG_TRACE_API
				Console.WriteLine("deleting prop " + name + " type " + type);
#endif

            if (typeof(IDictionary).ExtIsAssignableFrom(obj.GetType()))
            {
                IDictionary dictionary = (IDictionary)obj;
                if (dictionary.Contains(name))
                {
                    dictionary.Remove(name);
                    _convert.ToJsValue(true, ref output);
                    return;
                }
            }
            _convert.ToJsValue(false, ref output);
        }

        internal void KeepAliveEnumerateProperties(int slot, ref JsValue output)
        {
#if DEBUG_TRACE_API
			Console.WriteLine("deleting prop " + name);
#endif
            // TODO: This is pretty slow: use a cache of generated code to make it faster.
            var obj = KeepAliveGet(slot);
            if (obj == null)
            {
                output.Type = JsValueType.Error;
                output.I64 = (int)JsManagedError.NotFoundManagedObjectId;
                return;
            }

#if DEBUG_TRACE_API
				Console.WriteLine("deleting prop " + name + " type " + type);
#endif


            Type obj_type = obj.GetType();
            if (typeof(IDictionary).ExtIsAssignableFrom(obj_type))
            {
                IDictionary dictionary = (IDictionary)obj;
                var keys01 = new System.Collections.Generic.List<string>();
                foreach (var k in dictionary.Keys)
                {
                    keys01.Add(k.ToString());
                }
                _convert.AnyToJsValue(keys01.ToArray(), ref output);
                return;
            }

            var mbNameList = new List<string>();
            obj_type.AddPublicMembers(mbNameList);
            _convert.AnyToJsValue(mbNameList.ToArray(), ref output);

        }

        public object Invoke(IntPtr funcPtr, IntPtr thisPtr, object[] args)
        {
            CheckDisposed();

            if (funcPtr == IntPtr.Zero)
                throw new JsInteropException("wrapped V8 function is empty (IntPtr is Zero)");

            JsValue a = new JsValue();
            if (args != null)
            {
                _convert.AnyToJsValue(args, ref a);
            }

            JsValue v = new JsValue();
            jscontext_invoke(_context, funcPtr, thisPtr, ref a, ref v);
            object res = _convert.FromJsValue(ref v);
            //
            a.Dispose();
            v.Dispose();

            //
            Exception e = res as JsException;
            if (e != null)
                throw e;
            return res;
        }
        public INativeScriptable CreateWrapper(object o, JsTypeDefinition jsTypeDefinition)
        {
            return proxyStore.CreateProxyForObject(o, jsTypeDefinition);
        }
        public void RegisterTypeDefinition(JsTypeDefinition jsTypeDefinition)
        {
            proxyStore.CreateProxyForTypeDefinition(jsTypeDefinition);
        }
        //---------------------------------------------------------------------------------------- 
        public JsTypeDefinition GetJsTypeDefinition(Type actualType)
        {

            JsTypeDefinition found;
            if (this.mappingJsTypeDefinition.TryGetValue(actualType, out found))
                return found;

            //if not found
            //just create it
            found = this.jsTypeDefBuilder.BuildTypeDefinition(actualType);
            this.mappingJsTypeDefinition.Add(actualType, found);
            this.RegisterTypeDefinition(found);

            return found;
        }

        internal bool GetCacheDelegateForType(Type anotherDelegateType, out DelegateTemplate delSample)
        {
            return this.cachedDelSamples.TryGetValue(anotherDelegateType, out delSample);
        }
        internal void CacheDelegateForType(Type anotherDelegateType, DelegateTemplate delegateType)
        {
            this.cachedDelSamples[anotherDelegateType] = delegateType;
        }
        //---------------------------------------------------------------------------------------- 

        /// <summary>
        /// set variable with any value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetVariableFromAny(string name, object value)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();

            JsValue a = new JsValue();
            JsValue b = new JsValue();
            _convert.AnyToJsValue(value, ref a);
            jscontext_set_variable(_context, name, ref a, ref b);
#if DEBUG_TRACE_API
			Console.WriteLine("Cleaning up return value from set variable");
#endif
            a.Dispose();
            b.Dispose();
            // TODO: Check the result of the operation for errors.
        }

        /// <summary>
        /// set variable with string value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetVariable(string name, string value)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();

            JsValue a = new JsValue();
            JsValue b = new JsValue();
            _convert.ToJsValue(value, ref a);

            jscontext_set_variable(_context, name, ref a, ref b);
#if DEBUG_TRACE_API
			Console.WriteLine("Cleaning up return value from set variable");
#endif
            a.Dispose();
            b.Dispose();
        }
        /// <summary>
        /// set variable  with int32 value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetVariable(string name, int value)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();

            JsValue a = new JsValue();
            JsValue b = new JsValue();

            _convert.ToJsValue(value, ref a);

            jscontext_set_variable(_context, name, ref a, ref b);
#if DEBUG_TRACE_API
			Console.WriteLine("Cleaning up return value from set variable");
#endif
            a.Dispose();
            b.Dispose();
        }
        /// <summary>
        /// set variable with double value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetVariable(string name, double value)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();

            JsValue a = new JsValue();
            JsValue b = new JsValue();

            _convert.ToJsValue(value, ref a);

            jscontext_set_variable(_context, name, ref a, ref b);
#if DEBUG_TRACE_API
			Console.WriteLine("Cleaning up return value from set variable");
#endif
            a.Dispose();
            b.Dispose();
        }
        /// <summary>
        ///  set variable with long value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetVariable(string name, long value)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();

            JsValue a = new JsValue();
            JsValue b = new JsValue();

            _convert.ToJsValue(value, ref a);
            jscontext_set_variable(_context, name, ref a, ref b);
#if DEBUG_TRACE_API
			Console.WriteLine("Cleaning up return value from set variable");
#endif
            a.Dispose();
            b.Dispose();
        }
        public void SetVariable(string name, DateTime value)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();
            JsValue a = new JsValue();
            JsValue b = new JsValue();

            _convert.ToJsValue(value, ref a);
            jscontext_set_variable(_context, name, ref a, ref b);
#if DEBUG_TRACE_API
			Console.WriteLine("Cleaning up return value from set variable");
#endif
            a.Dispose();
            b.Dispose();
        }
        public void SetVariable(string name, INativeScriptable proxy)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();
            JsValue a = new JsValue();
            JsValue b = new JsValue();
            _convert.ToJsValue(proxy, ref a);
            jscontext_set_variable(_context, name, ref a, ref b);
#if DEBUG_TRACE_API
			Console.WriteLine("Cleaning up return value from set variable");
#endif
            a.Dispose();
            b.Dispose();
        }
        public void SetVariableNull(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();

            JsValue a = new JsValue();
            JsValue b = new JsValue();

            _convert.ToJsValueNull(ref a);
            jscontext_set_variable(_context, name, ref a, ref b);
#if DEBUG_TRACE_API
			Console.WriteLine("Cleaning up return value from set variable");
#endif
            a.Dispose();
            b.Dispose();
        }
        public void SetVariableAutoWrap<T>(string name, T result)
             where T : class
        {
            Type actualType = result.GetType();
            JsTypeDefinition jsTypeDef = this.GetJsTypeDefinition(actualType);
            INativeScriptable proxy = this.CreateWrapper(result, jsTypeDef);
            this.SetVariable(name, proxy);
        }

    }

    class Timer
    {
        //dummy timer
        public event EventHandler Elapsed;
        public Timer(double millisec)
        {

        }

        public void Start()
        {

        }

        public void Stop()
        {

        }

        public void Dispose()
        {

        }
    }
    class ContextNotFoundException : Exception
    {
        public ContextNotFoundException(int contextId)
        {
            this.ContextId = contextId;
        }
        public int ContextId { get; private set; }
    }
}
