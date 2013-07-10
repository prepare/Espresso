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
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Timers;

namespace VroomJs
{
	public partial class JsContext : IDisposable
	{
    	[DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
	    static extern IntPtr jscontext_new(int id, HandleRef engine);

		[DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
		public static extern void jscontext_dispose(HandleRef engine);

		[DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
		static extern void jscontext_force_gc();

		[DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
		static extern JsValue jscontext_execute(HandleRef engine, [MarshalAs(UnmanagedType.LPWStr)] string str);

		[DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
		static extern JsValue jscontext_get_global(HandleRef engine);

		[DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
		static extern JsValue jscontext_get_variable(HandleRef engine, [MarshalAs(UnmanagedType.LPWStr)] string name);

		[DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
		static extern JsValue jscontext_set_variable(HandleRef engine, [MarshalAs(UnmanagedType.LPWStr)] string name, JsValue value);

		[DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
		static internal extern JsValue jsvalue_alloc_string([MarshalAs(UnmanagedType.LPWStr)] string str);

		[DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
		static internal extern JsValue jsvalue_alloc_array(int length);

		[DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
		static internal extern void jsvalue_dispose(JsValue value);

		private int _id;
		private JsEngine _engine;

		public JsContext(int id, JsEngine engine, HandleRef engineHandle, Action<int> notifyDispose) {
			_id = id;
			_engine = engine;
			_notifyDispose = notifyDispose;

            _keepalives = new KeepAliveDictionaryStore();
			_context = new HandleRef(this, jscontext_new(id, engineHandle));
			_convert = new JsConvert(this);
		}

        readonly HandleRef _context;

		public HandleRef Handle {
			get { return _context; }
		}

        readonly JsConvert _convert;

        // Keep objects passed to V8 alive even if no other references exist.
        readonly IKeepAliveStore _keepalives;
		
        public JsEngineStats GetStats()
        {
            return new JsEngineStats {
                KeepAliveMaxSlots = _keepalives.MaxSlots,
                KeepAliveAllocatedSlots = _keepalives.AllocatedSlots,
                KeepAliveUsedSlots = _keepalives.UsedSlots
            };
        }

        public object Execute(string code, TimeSpan? executionTimeout = null) {
        	if (code == null)
        		throw new ArgumentNullException("code");

        	CheckDisposed();

        	bool executionTimedOut = false;
        	Timer timer = null;
        	if (executionTimeout.HasValue) {
        		timer = new Timer(executionTimeout.Value.TotalMilliseconds);
        		timer.Elapsed += (sender, args) => {
        			timer.Stop();
        			executionTimedOut = true;
					_engine.TerminateExecution();
        		};
				timer.Start();
        	}
        	object res;
			try {
				JsValue v = jscontext_execute(_context, code);
				res = _convert.FromJsValue(v);

#if DEBUG_TRACE_API
        	Console.WriteLine("Cleaning up return value from execution");
#endif
				jsvalue_dispose(v);
			} finally {
				if (executionTimeout.HasValue) {
					timer.Dispose();
				}
			}
			
			if (executionTimedOut) {
				throw new JsExecutionTimedOutException();
			}

        	Exception e = res as JsException;
            if (e != null)
                throw e;
            return res;
        }

		public object GetGlobal() 
		{
			CheckDisposed();	
			JsValue v = jscontext_get_global(_context);
            object res = _convert.FromJsValue(v);
            jsvalue_dispose(v);

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

            JsValue v = jscontext_get_variable(_context, name);
            object res = _convert.FromJsValue(v);
#if DEBUG_TRACE_API
			Console.WriteLine("Cleaning up return value get variable.");
#endif
			jsvalue_dispose(v);

            Exception e = res as JsException;
            if (e != null)
                throw e;
            return res;
        }

        public void SetVariable(string name, object value)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();

            JsValue a = _convert.ToJsValue(value);
            jscontext_set_variable(_context, name, a);
#if DEBUG_TRACE_API
			Console.WriteLine("Cleaning up return value from set variable");
#endif
            jsvalue_dispose(a);

            // TODO: Check the result of the operation for errors.
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
		
		public bool IsDisposed {
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

			if (disposing) {
				_keepalives.Clear();
			}

			_notifyDispose(_id);
        }

        void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("JsContext:" + _context.Handle);
        }

        ~JsContext()
        {
            if (!_disposed)
                Dispose(false);
        }

        #endregion

		internal JsValue KeepAliveSetPropertyValue(int slot, string name, JsValue value) {
#if DEBUG_TRACE_API
			Console.WriteLine("setting prop " + name);
#endif
			// TODO: This is pretty slow: use a cache of generated code to make it faster.

			var obj = KeepAliveGet(slot);
			if (obj != null) {
				Type type = obj.GetType();
#if DEBUG_TRACE_API
				Console.WriteLine("setting prop " + name + " type " + type);
#endif

				// We can only set properties; everything else is an error.
				try {
					PropertyInfo pi = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
					if (pi != null) {
						pi.SetValue(obj, _convert.FromJsValue(value), null);
						return JsValue.Null;
					}

					// dictionaries.
					if (typeof(IDictionary).IsAssignableFrom(obj.GetType())) {
						IDictionary dictionary = (IDictionary)obj;
						dictionary[name] = _convert.FromJsValue(value);
						return JsValue.Null;
					}

					return JsValue.Error(KeepAliveAdd(
						new InvalidOperationException(String.Format("property not found on {0}: {1} ", type, name))));
				} catch (Exception e) {
					return JsValue.Error(KeepAliveAdd(e));
				}
			}

			return JsValue.Error(KeepAliveAdd(new IndexOutOfRangeException("invalid keepalive slot: " + slot)));
		}

		internal JsValue KeepAliveGetPropertyValue(int slot, string name) {
#if DEBUG_TRACE_API
			Console.WriteLine("getting prop " + name);
#endif
			// TODO: This is pretty slow: use a cache of generated code to make it faster.

			var obj = KeepAliveGet(slot);
			if (obj != null) {
				Type type = obj.GetType();
#if DEBUG_TRACE_API
				Console.WriteLine("getting prop " + name + " type " + type);
#endif
				if (name == "toString") {
					name = "ToString";
				}

				if (name == "valueOf") {
					return _convert.ToJsValue(obj);
				}

				try {
					// First of all try with a public property (the most common case).

					PropertyInfo pi = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
					if (pi != null)
						return _convert.ToJsValue(pi.GetValue(obj, null));

					// try field.
					FieldInfo fi = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
					if (fi != null)
						return _convert.ToJsValue(fi.GetValue(obj));
					
					// dictionaries.
					if (typeof(IDictionary).IsAssignableFrom(obj.GetType())) {
						IDictionary dictionary = (IDictionary)obj;
						if (dictionary.Contains(name)) {
							return _convert.ToJsValue(dictionary[name]);
						} else {
							return JsValue.Null;
						}
					}

					// Then with an instance method: the problem is that we don't have a list of
					// parameter types so we just check if any method with the given name exists
					// and then keep alive a "weak delegate", i.e., just a name and the target.
					// The real method will be resolved during the invokation itself.

					const BindingFlags mFlags = BindingFlags.Instance | BindingFlags.Public
											   | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy;
					// TODO: This is probably slooow.
					if (type.GetMethods(mFlags).Any(x => x.Name == name))
						return _convert.ToJsValue(new WeakDelegate(obj, name));

					// Else an error.

					return JsValue.Error(KeepAliveAdd(
						new InvalidOperationException(String.Format("property not found on {0}: {1} ", type, name))));
				} catch (TargetInvocationException e) {
					// Client code probably isn't interested in the exception part related to
					// reflection, so we unwrap it and pass to V8 only the real exception thrown.
					if (e.InnerException != null)
						return JsValue.Error(KeepAliveAdd(e.InnerException));
					throw;
				} catch (Exception e) {
					return JsValue.Error(KeepAliveAdd(e));
				}
			}

			return JsValue.Error(KeepAliveAdd(new IndexOutOfRangeException("invalid keepalive slot: " + slot)));
		}

		internal JsValue KeepAliveInvoke(int slot, JsValue args) {
			
			// TODO: This is pretty slow: use a cache of generated code to make it faster.
#if DEBUG_TRACE_API
			Console.WriteLine("invoking");
#endif
			//   Console.WriteLine(args);
			
			var obj = KeepAliveGet(slot) as WeakDelegate;
			if (obj != null) {
				Type type = obj.Target.GetType();
#if DEBUG_TRACE_API
				Console.WriteLine("invoking " + obj.Target + " method " + obj.MethodName);
#endif
				object[] a = (object[])_convert.FromJsValue(args);

				const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public
						| BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy;
		
				try {
					return _convert.ToJsValue(type.InvokeMember(obj.MethodName, flags, null, obj.Target, a));
				} catch (TargetInvocationException e) {
					return JsValue.Error(KeepAliveAdd(e.InnerException));
				} catch (Exception e) {
					return JsValue.Error(KeepAliveAdd(e));
				}
			}

			return JsValue.Error(KeepAliveAdd(new IndexOutOfRangeException("invalid keepalive slot: " + slot)));
		}


		
	}
}
