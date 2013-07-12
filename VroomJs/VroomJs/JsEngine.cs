using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace VroomJs {
	public class JsEngine : IDisposable {

		delegate void KeepaliveRemoveDelegate(int context, int slot);
		delegate JsValue KeepAliveGetPropertyValueDelegate(int context, int slot, [MarshalAs(UnmanagedType.LPWStr)] string name);
		delegate JsValue KeepAliveSetPropertyValueDelegate(int context, int slot, [MarshalAs(UnmanagedType.LPWStr)] string name, JsValue value);
		delegate JsValue KeepAliveInvokeDelegate(int context, int slot, JsValue args);

		[DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
		static extern void js_set_object_marshal_type(JsObjectMarshalType objectMarshalType);

		[DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
		static extern IntPtr jsengine_new(
			KeepaliveRemoveDelegate keepaliveRemove,
			KeepAliveGetPropertyValueDelegate keepaliveGetPropertyValue,
			KeepAliveSetPropertyValueDelegate keepaliveSetPropertyValue,
			KeepAliveInvokeDelegate keepaliveInvoke,
			int maxYoungSpace, int maxOldSpace
		);
		
		[DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
		static extern void jsengine_terminate_execution(HandleRef engine);
			
		[DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
		static extern void jsengine_dump_heap_stats(HandleRef engine);

		[DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
		static extern void jsengine_dispose(HandleRef engine);

		// Make sure the delegates we pass to the C++ engine won't fly away during a GC.
		readonly KeepaliveRemoveDelegate _keepalive_remove;
		readonly KeepAliveGetPropertyValueDelegate _keepalive_get_property_value;
		readonly KeepAliveSetPropertyValueDelegate _keepalive_set_property_value;
		readonly KeepAliveInvokeDelegate _keepalive_invoke;

		private readonly Dictionary<int, JsContext> _aliveContexts = new Dictionary<int, JsContext>();
		private int _currentContextId = 0;

		static JsEngine() {
			JsObjectMarshalType objectMarshalType = JsObjectMarshalType.Dictionary;
#if NET40
        	objectMarshalType = JsObjectMarshalType.Dynamic;
#endif
			js_set_object_marshal_type(objectMarshalType);
		}

		readonly HandleRef _engine;

		public JsEngine(int maxYoungSpace = -1, int maxOldSpace = -1) {
			_keepalive_remove = new KeepaliveRemoveDelegate(KeepAliveRemove);
			_keepalive_get_property_value = new KeepAliveGetPropertyValueDelegate(KeepAliveGetPropertyValue);
			_keepalive_set_property_value = new KeepAliveSetPropertyValueDelegate(KeepAliveSetPropertyValue);
			_keepalive_invoke = new KeepAliveInvokeDelegate(KeepAliveInvoke);

			_engine = new HandleRef(this, jsengine_new(
				_keepalive_remove,
				_keepalive_get_property_value,
				_keepalive_set_property_value, 
				_keepalive_invoke,
				maxYoungSpace, 
				maxOldSpace));
		}

		public void TerminateExecution() {
			jsengine_terminate_execution(_engine);
		}

		public void DumpHeapStats() {
			jsengine_dump_heap_stats(_engine);
		}
		
		private JsValue KeepAliveInvoke(int contextId, int slot, JsValue args) {
			JsContext context;
			if (!_aliveContexts.TryGetValue(contextId, out context)) {
				throw new Exception("fail");
			}
			return context.KeepAliveInvoke(slot, args);
		}

		private JsValue KeepAliveSetPropertyValue(int contextId, int slot, string name, JsValue value) {
#if DEBUG_TRACE_API
			Console.WriteLine("set prop " + contextId + " " + slot);
#endif
			JsContext context;
			if (!_aliveContexts.TryGetValue(contextId, out context)) {
				throw new Exception("fail");
			}
			return context.KeepAliveSetPropertyValue(slot, name, value);
		}

		private JsValue KeepAliveGetPropertyValue(int contextId, int slot, string name) {
#if DEBUG_TRACE_API
			Console.WriteLine("get prop " + contextId + " " + slot);
#endif
			JsContext context;
			if (!_aliveContexts.TryGetValue(contextId, out context)) {
				throw new Exception("fail");
			}
			return context.KeepAliveGetPropertyValue(slot, name);
		}

		private void KeepAliveRemove(int contextId, int slot) {
#if DEBUG_TRACE_API
			Console.WriteLine("Keep alive remove for " + contextId + " " + slot);
#endif
			JsContext context;
			if (!_aliveContexts.TryGetValue(contextId, out context)) {
				return;
			}
			context.KeepAliveRemove(slot);
		}

		public JsContext CreateContext() {
			CheckDisposed();
			int id = Interlocked.Increment(ref _currentContextId);
			JsContext ctx = new JsContext(id, this, _engine, ContextDisposed);
			lock(_aliveContexts) {
				_aliveContexts.Add(id, ctx);
			}
			return ctx;
		}

		private void ContextDisposed(int id) {
			lock (_aliveContexts) {
				_aliveContexts.Remove(id);
			}
		}
		
		#region IDisposable implementation

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

            if (disposing) {
            	foreach (var aliveContext in _aliveContexts) {
					JsContext.jscontext_dispose(aliveContext.Value.Handle);
            	}
				_aliveContexts.Clear();
            }
#if DEBUG_TRACE_API
				Console.WriteLine("Calling jsEngine dispose: " + _engine.Handle.ToInt64());
#endif
        
			jsengine_dispose(_engine);
        }

        void CheckDisposed()
        {
            if (_disposed)
               throw new ObjectDisposedException("JsEngine:" + _engine.Handle);
        }

        ~JsEngine()
        {
            if (!_disposed)
                Dispose(false);
        }

        #endregion
	}
}
