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

    public partial class JsContext : IDisposable
    {

        readonly int _id;
        readonly JsEngine _engine;
        readonly ManagedMethodCallDel engineMethodCallbackDel;

        List<JsMethodDefinition> registerMethods = new List<JsMethodDefinition>();
        List<JsPropertyDefinition> registerProperties = new List<JsPropertyDefinition>();

        Dictionary<Type, JsTypeDefinition> mappingJsTypeDefinition = new Dictionary<Type, JsTypeDefinition>();
        Dictionary<Type, DelegateTemplate> cachedDelSamples = new Dictionary<Type, DelegateTemplate>();

        NativeObjectProxyStore proxyStore;
        JsTypeDefinitionBuilder jsTypeDefBuilder;

        internal JsContext(int id,
            JsEngine engine,
            Action<int> notifyDispose,
            JsTypeDefinitionBuilder jsTypeDefBuilder)
        {

            _id = id;
            _notifyDispose = notifyDispose;
            _engine = engine;
            _keepalives = new KeepAliveDictionaryStore();
            //create native js context
            _context = new HandleRef(this, jscontext_new(id, engine.UnmanagedEngineHandler));
            _convert2 = new JsConvert2(this);

            this.jsTypeDefBuilder = jsTypeDefBuilder;

            engineMethodCallbackDel = new ManagedMethodCallDel(EngineListener_MethodCall);
            NativeV8JsInterOp.CtxRegisterManagedMethodCall(this, engineMethodCallbackDel);
            registerMethods.Add(null);//first is null
            registerProperties.Add(null); //first is null


            proxyStore = new NativeObjectProxyStore(this);

        }
        internal JsContext(int id,
            JsEngine engine,
            Action<int> notifyDispose,
            IntPtr nativeJsContext,
            JsTypeDefinitionBuilder jsTypeDefBuilder)
        {

            _id = id;
            _notifyDispose = notifyDispose;
            _engine = engine;
            _keepalives = new KeepAliveDictionaryStore();
            //create native js context
            _context = new HandleRef(this, nativeJsContext);
            _convert2 = new JsConvert2(this);

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


        internal JsConvert2 Converter2
        {
            get { return this._convert2; }
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
                var p = properties[i];
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
        readonly HandleRef _context;

        public HandleRef Handle
        {
            get { return _context; }
        }


        readonly JsConvert2 _convert2;
        // Keep objects passed to V8 alive even if no other references exist.
        readonly IKeepAliveStore _keepalives;

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
                JsInterOpValue v = new JsInterOpValue();
                jscontext_execute_script(_context, script.Handle, ref v);
                res = _convert2.FromJsValue(ref v);
#if DEBUG_TRACE_API
        	Console.WriteLine("Cleaning up return value from execution");
#endif
                jsvalue_dispose(ref v);
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
#if DEBUG
                int ver = getVersion();//just check version
#endif
                JsInterOpValue output = new JsInterOpValue();
                jscontext_execute(_context, code, name ?? "<Unnamed Script>", ref output);

                //watch2.Stop();                 
                res = _convert2.FromJsValue(ref output);
#if DEBUG_TRACE_API
        	Console.WriteLine("Cleaning up return value from execution");
#endif
                jsvalue_dispose(ref output);
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
            JsInterOpValue v = new JsInterOpValue();
            jscontext_get_global(_context, ref v);
            object res = _convert2.FromJsValue(ref v);
            jsvalue_dispose(ref v);

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

            JsInterOpValue v = new JsInterOpValue();
            jscontext_get_variable(_context, name, ref v);
            object res = _convert2.FromJsValue(ref v);
#if DEBUG_TRACE_API
			Console.WriteLine("Cleaning up return value get variable.");
#endif
            jsvalue_dispose(ref v);

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

        #region Keep-alive management and callbacks.

        internal int KeepAliveAdd(object obj)
        {
            return _keepalives.Add(obj);
        }

        internal object KeepAliveGet(int slot)
        {
            return _keepalives.Get(slot);
        }

        internal void KeepAliveRemove(int slot)
        {
            _keepalives.Remove(slot);
        }

        #endregion

        #region IDisposable implementation

        private readonly Action<int> _notifyDispose;
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

        #endregion


        internal bool TrySetMemberValue(Type type, object obj, string name, ref JsInterOpValue value)
        {
            // dictionaries.
            if (typeof(IDictionary).ExtIsAssignableFrom(type))
            {

                IDictionary dictionary = (IDictionary)obj;
                _convert2.FromJsValue(ref value);
                //we get member from this type 
                throw new NotSupportedException();
                //dictionary[name] = _convert2.FromJsValue(value);
                return true;
            }
            throw new NotSupportedException();
            //BindingFlags flags;
            //if (type == obj)
            //{
            //    flags = BindingFlags.Public | BindingFlags.Static;
            //}
            //else
            //{
            //    flags = BindingFlags.Public | BindingFlags.Instance;
            //}

            //PropertyInfo pi = type.GetProperty(name, flags | BindingFlags.SetProperty);
            //if (pi != null)
            //{
            //    pi.SetValue(obj, _convert2.FromJsValue(ref value), null);
            //    return true;
            //}

            return false;
        }

        internal void KeepAliveSetPropertyValue(int slot, string name, ref JsInterOpValue v)
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
                        string upperCamelCase = Char.ToUpper(name[0]) + name.Substring(1);
                        if (TrySetMemberValue(type, obj, upperCamelCase, ref v))
                        {
                            //no error
                            v.Type = JsValueType.Null;
                            return;
                        }
                        if (TrySetMemberValue(type, obj, name, ref v))
                        {
                            //no error
                            v.Type = JsValueType.Null;
                            return;
                        }
                    }
                    //TODO: review how to handle error again
                    throw new NotSupportedException();
                    //return JsValue.Error(KeepAliveAdd(
                    //    new InvalidOperationException(String.Format("property not found on {0}: {1} ", type, name))));
                }
                catch (Exception e)
                {
                    //TODO: review how to handle error again
                    //return JsValue.Error(KeepAliveAdd(e));
                }
            }
            throw new NotSupportedException();
            //return JsValue.Error(KeepAliveAdd(new IndexOutOfRangeException("invalid keepalive slot: " + slot)));
        }

        internal bool TryGetMemberValue(Type type, object obj, string name, ref JsInterOpValue value)
        {
            object result;
            // dictionaries.
            if (typeof(IDictionary).ExtIsAssignableFrom(type))
            {
                IDictionary dictionary = (IDictionary)obj;
                if (dictionary.Contains(name))
                {
                    result = dictionary[name];
                    _convert2.AnyToJsValue(result, ref value);
                }
                else
                {
                    value.Type = JsValueType.Null;
                }
                return true;
            }


            // First of all try with a public property (the most common case).
            PropertyInfo pi = type.ExtGetProperty(obj, name);// type.GetProperty(name, flags | BindingFlags.GetProperty);
            if (pi != null)
            {
                result = pi.GetValue(obj, null);
                _convert2.AnyToJsValue(result, ref value);
                return true;
            }

            // try field.
            FieldInfo fi = type.ExtGetField(obj, name);
            if (fi != null)
            {
                result = fi.GetValue(obj);
                _convert2.AnyToJsValue(result, ref value);
                return true;
            }

            // Then with an instance method: the problem is that we don't have a list of
            // parameter types so we just check if any method with the given name exists
            // and then keep alive a "weak delegate", i.e., just a name and the target.
            // The real method will be resolved during the invokation itself.

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

        internal void KeepAliveGetPropertyValue(int slot, string name, ref JsInterOpValue output)
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
                        var upperCamelCase = Char.ToUpper(name[0]) + name.Substring(1);

                        if (TryGetMemberValue(type, obj, upperCamelCase, ref output))
                        {
                            return;
                        }
                        if (TryGetMemberValue(type, obj, name, ref output))
                        {
                            return;
                        }
                    }
                    throw new NotSupportedException();
                    // Else an error.
                    //return JsValue.Error(KeepAliveAdd(
                    //    new InvalidOperationException(String.Format("property not found on {0}: {1} ", type, name))));
                }
                catch (TargetInvocationException e)
                {
                    // Client code probably isn't interested in the exception part related to
                    // reflection, so we unwrap it and pass to V8 only the real exception thrown.
                    //if (e.InnerException != null)
                    //    return JsValue.Error(KeepAliveAdd(e.InnerException));
                    //throw;
                }
                catch (Exception e)
                {
                    throw new NotSupportedException();
                    //return JsValue.Error(KeepAliveAdd(e));
                }
            }
            //TODO: review exception again
            throw new NotSupportedException();
            //return JsValue.Error(KeepAliveAdd(new IndexOutOfRangeException("invalid keepalive slot: " + slot)));
        }

        internal void KeepAliveValueOf(int slot, ref JsInterOpValue output)
        {
            object obj = KeepAliveGet(slot);
            if (obj != null)
            {

                Type type = obj.GetType();
                MethodInfo mi;
#if NET20
                mi = type.GetMethod("valueOf") ?? type.GetMethod("ValueOf");
#else
                mi = type.GetRuntimeMethod("ValueOf", new Type[0]);
#endif
                if (mi != null)
                {
                    object result = mi.Invoke(obj, new object[0]);
                    _convert2.AnyToJsValue(result, ref output);
                    return;
                }
                _convert2.AnyToJsValue(obj, ref output);
                return;
            }
            //error
            //JsValue.Error(KeepAliveAdd(new IndexOutOfRangeException("invalid keepalive slot: " + slot)));

        }


        internal void KeepAliveInvoke(int slot, ref JsInterOpValue args, ref JsInterOpValue output)
        {
            // TODO: This is pretty slow: use a cache of generated code to make it faster.
#if DEBUG_TRACE_API
			Console.WriteLine("invoking");
#endif
            //   Console.WriteLine(args);

            var obj = KeepAliveGet(slot);
            if (obj != null)
            {
                Type constructorType = obj as Type;
                if (constructorType != null)
                {
#if DEBUG_TRACE_API
					Console.WriteLine("constructing " + constructorType.Name);
#endif
                    object[] constructorArgs = (object[])_convert2.FromJsValue(ref args);
                    _convert2.AnyToJsValue(Activator.CreateInstance(constructorType, constructorArgs), ref output);
                    return;
                }

                WeakDelegate func = obj as WeakDelegate;
                if (func == null)
                {
                    throw new Exception("not a function.");
                }

                Type type = func.Target != null ? func.Target.GetType() : func.Type;
#if DEBUG_TRACE_API
				Console.WriteLine("invoking " + obj.Target + " method " + obj.MethodName);
#endif

                //review delegate invocation again  
                object[] a = (object[])_convert2.FromJsValue(ref args);

                foreach (var a_elem in a)
                {
                    if (a_elem.GetType() == typeof(JsFunction))
                    {
                        CheckAndResolveJsFunctions(func, (JsFunction)a_elem, obj, type, func.MethodName, a);
                        break;
                    }
                }

                throw new NotSupportedException();
                //if (a.Any(z => z != null && z.GetType() == typeof(JsFunction)))
                //{
                //    CheckAndResolveJsFunctions(type, func.MethodName, flags, a);
                //}

                //try
                //{
                //    object result = type.InvokeMember(func.MethodName, flags, null, func.Target, a);
                //    _convert2.AnyToJsValue(result, ref output);
                //    return;
                //}
                //catch (TargetInvocationException e)
                //{
                //    throw new NotSupportedException();
                //    //return JsValue.Error(KeepAliveAdd(e.InnerException));
                //    return;
                //}
                //catch (Exception e)
                //{
                //    //review set error
                //    throw new NotSupportedException();
                //    //return JsValue.Error(KeepAliveAdd(e));
                //    return;
                //}
            }
            throw new NotSupportedException();
            //return JsValue.Error(KeepAliveAdd(new IndexOutOfRangeException("invalid keepalive slot: " + slot)));
            return;
        }




        //        internal JsValue KeepAliveInvoke(int slot, JsValue args)
        //        {
        //            // TODO: This is pretty slow: use a cache of generated code to make it faster.
        //#if DEBUG_TRACE_API
        //			Console.WriteLine("invoking");
        //#endif
        //            //   Console.WriteLine(args);

        //            var obj = KeepAliveGet(slot);
        //            if (obj != null)
        //            {
        //                Type constructorType = obj as Type;
        //                if (constructorType != null)
        //                {
        //#if DEBUG_TRACE_API
        //					Console.WriteLine("constructing " + constructorType.Name);
        //#endif
        //                    object[] constructorArgs = (object[])_convert.FromJsValue(args);
        //                    return _convert.AnyToJsValue(Activator.CreateInstance(constructorType, constructorArgs));
        //                }

        //                WeakDelegate func = obj as WeakDelegate;
        //                if (func == null)
        //                {
        //                    throw new Exception("not a function.");
        //                }

        //                Type type = func.Target != null ? func.Target.GetType() : func.Type;
        //#if DEBUG_TRACE_API
        //				Console.WriteLine("invoking " + obj.Target + " method " + obj.MethodName);
        //#endif
        //                object[] a = (object[])_convert.FromJsValue(args);


        //                // need to convert methods from JsFunction's into delegates?
        //                foreach (var a_elem in a)
        //                {
        //                    if (a.GetType() == typeof(JsFunction))
        //                    {
        //                        CheckAndResolveJsFunctions(type, func.MethodName, a);
        //                        break;
        //                    }
        //                }
        //                //if (a.Any(z => z != null && z.GetType() == typeof(JsFunction)))
        //                //{
        //                //    CheckAndResolveJsFunctions(type, func.MethodName, flags, a);
        //                //}

        //                try
        //                {
        //                    var method = type.GetRuntimeMethod(func.MethodName, null);
        //                    object result = method.Invoke(func.Target, a);
        //                    //object result = type.InvokeMember(func.MethodName, flags, null, func.Target, a);
        //                    return _convert.AnyToJsValue(result);
        //                }
        //                catch (TargetInvocationException e)
        //                {
        //                    return JsValue.Error(KeepAliveAdd(e.InnerException));
        //                }
        //                catch (Exception e)
        //                {
        //                    return JsValue.Error(KeepAliveAdd(e));
        //                }
        //            }

        //            return JsValue.Error(KeepAliveAdd(new IndexOutOfRangeException("invalid keepalive slot: " + slot)));
        //        }


        private static void CheckAndResolveJsFunctions(WeakDelegate weakDel,
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

        //private static void CheckAndResolveJsFunctions(Type type, string methodName, object[] args)
        //{

        //    //MethodInfo mi = type.GetMethod(methodName, flags);
        //    MethodInfo mi = type.GetRuntimeMethod(methodName, null);
        //    //TODO: type.GetRuntimeMethods();
        //    ParameterInfo[] paramTypes = mi.GetParameters();

        //    for (int i = Math.Min(paramTypes.Length, args.Length) - 1; i >= 0; --i)
        //    {
        //        if (args[i] != null && args[i].GetType() == typeof(JsFunction))
        //        {
        //            JsFunction function = (JsFunction)args[i];
        //            args[i] = function.MakeDelegate(paramTypes[i].ParameterType);
        //        }
        //    }
        //}



        internal void KeepAliveDeleteProperty(int slot, string name, ref JsInterOpValue output)
        {
#if DEBUG_TRACE_API
			Console.WriteLine("deleting prop " + name);
#endif
            // TODO: This is pretty slow: use a cache of generated code to make it faster.
            var obj = KeepAliveGet(slot);
            if (obj != null)
            {
#if DEBUG_TRACE_API
				Console.WriteLine("deleting prop " + name + " type " + type);
#endif

                if (typeof(IDictionary).ExtIsAssignableFrom(obj.GetType()))
                {
                    IDictionary dictionary = (IDictionary)obj;
                    if (dictionary.Contains(name))
                    {
                        dictionary.Remove(name);
                        _convert2.ToJsValue(true, ref output);
                        return;
                    }
                }
                _convert2.ToJsValue(false, ref output);
                return;
            }
            throw new NotSupportedException();
            //TODO review err again ***
            //JsValue.Error(KeepAliveAdd(new IndexOutOfRangeException("invalid keepalive slot: " + slot)));
            return;
        }

        internal void KeepAliveEnumerateProperties(int slot, ref JsInterOpValue output)
        {
#if DEBUG_TRACE_API
			Console.WriteLine("deleting prop " + name);
#endif
            // TODO: This is pretty slow: use a cache of generated code to make it faster.
            var obj = KeepAliveGet(slot);
            if (obj != null)
            {
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

                    _convert2.AnyToJsValue(keys01.ToArray(), ref output);
                    return;
                }

                var mbNameList = new System.Collections.Generic.List<string>();
                obj_type.AddPublicMembers(mbNameList);

                _convert2.AnyToJsValue(mbNameList.ToArray(), ref output);
                return;
            }

            //TODO: review here again
            throw new NotSupportedException();
            //JsValue.Error(KeepAliveAdd(new IndexOutOfRangeException("invalid keepalive slot: " + slot)));

        }

        public object Invoke(IntPtr funcPtr, IntPtr thisPtr, object[] args)
        {
            CheckDisposed();

            if (funcPtr == IntPtr.Zero)
                throw new JsInteropException("wrapped V8 function is empty (IntPtr is Zero)");

            JsInterOpValue a = new JsInterOpValue();
            if (args != null)
            {
                _convert2.AnyToJsValue(args, ref a);
            }

            JsInterOpValue v = new JsInterOpValue();
            jscontext_invoke(_context, funcPtr, thisPtr, ref a, ref v);
            object res = _convert2.FromJsValue(ref v);
            jsvalue_dispose(ref v);
            jsvalue_dispose(ref a);
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

        public void SetVariableFromAny(string name, object value)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();

            JsInterOpValue a = new JsInterOpValue();
            JsInterOpValue b = new JsInterOpValue();
            _convert2.AnyToJsValue(value, ref a);
            jscontext_set_variable(_context, name, ref a, ref b);
#if DEBUG_TRACE_API
			Console.WriteLine("Cleaning up return value from set variable");
#endif
            jsvalue_dispose(ref a);
            jsvalue_dispose(ref b);
            // TODO: Check the result of the operation for errors.
        }

        public void SetVariable(string name, string value)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();

            JsInterOpValue a = new JsInterOpValue();
            JsInterOpValue b = new JsInterOpValue();
            _convert2.AnyToJsValue(value, ref a);
            jscontext_set_variable(_context, name, ref a, ref b);
#if DEBUG_TRACE_API
			Console.WriteLine("Cleaning up return value from set variable");
#endif
            jsvalue_dispose(ref a);
            jsvalue_dispose(ref b);
        }
        public void SetVariable(string name, int value)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();

            JsInterOpValue a = new JsInterOpValue();
            JsInterOpValue b = new JsInterOpValue();
            _convert2.AnyToJsValue(value, ref a);
            jscontext_set_variable(_context, name, ref a, ref b);
#if DEBUG_TRACE_API
			Console.WriteLine("Cleaning up return value from set variable");
#endif
            jsvalue_dispose(ref a);
            jsvalue_dispose(ref b);
        }
        public void SetVariable(string name, double value)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();
            JsInterOpValue a = new JsInterOpValue();
            JsInterOpValue b = new JsInterOpValue();
            _convert2.AnyToJsValue(value, ref a);
            jscontext_set_variable(_context, name, ref a, ref b);
#if DEBUG_TRACE_API
			Console.WriteLine("Cleaning up return value from set variable");
#endif
            jsvalue_dispose(ref a);
            jsvalue_dispose(ref b);
        }
        public void SetVariable(string name, long value)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();
            JsInterOpValue a = new JsInterOpValue();
            JsInterOpValue b = new JsInterOpValue();

            _convert2.AnyToJsValue(value, ref a);
            jscontext_set_variable(_context, name, ref a, ref b);
#if DEBUG_TRACE_API
			Console.WriteLine("Cleaning up return value from set variable");
#endif
            jsvalue_dispose(ref a);
            jsvalue_dispose(ref b);
        }
        public void SetVariable(string name, DateTime value)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();
            JsInterOpValue a = new JsInterOpValue();
            JsInterOpValue b = new JsInterOpValue();

            _convert2.AnyToJsValue(value, ref a);
            jscontext_set_variable(_context, name, ref a, ref b);
#if DEBUG_TRACE_API
			Console.WriteLine("Cleaning up return value from set variable");
#endif
            jsvalue_dispose(ref a);
            jsvalue_dispose(ref b);
        }
        public void SetVariable(string name, INativeScriptable proxy)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();
            JsInterOpValue a = new JsInterOpValue();
            JsInterOpValue b = new JsInterOpValue();
            _convert2.AnyToJsValue(proxy, ref a);
            jscontext_set_variable(_context, name, ref a, ref b);
#if DEBUG_TRACE_API
			Console.WriteLine("Cleaning up return value from set variable");
#endif
            jsvalue_dispose(ref a);
            jsvalue_dispose(ref b);
            // TODO: Check the result of the operation for errors.
        }
        public void SetVariableNull(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();

            JsInterOpValue a = new JsInterOpValue();
            JsInterOpValue b = new JsInterOpValue();

            _convert2.ToJsValueNull(ref a);
            jscontext_set_variable(_context, name, ref a, ref b);
#if DEBUG_TRACE_API
			Console.WriteLine("Cleaning up return value from set variable");
#endif
            jsvalue_dispose(ref a);
            jsvalue_dispose(ref b);
        }

        public void SetVariableAutoWrap<T>(string name, T result)
             where T : class
        {
            Type actualType = result.GetType();
            var jsTypeDef = this.GetJsTypeDefinition(actualType);
            var proxy = this.CreateWrapper(result, jsTypeDef);
            this.SetVariable(name, proxy);
        }
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
        //----------------------------------------------------------------------------------------


        internal bool GetCacheDelegateForType(Type anotherDelegateType, out DelegateTemplate delSample)
        {
            return this.cachedDelSamples.TryGetValue(anotherDelegateType, out delSample);
        }
        internal void CacheDelegateForType(Type anotherDelegateType, DelegateTemplate delegateType)
        {
            this.cachedDelSamples[anotherDelegateType] = delegateType;
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
}
