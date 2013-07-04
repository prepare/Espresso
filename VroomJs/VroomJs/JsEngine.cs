using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace VroomJs {
	public class JsEngine : IDisposable {
		[DllImport("VroomJsNative")]
		static extern void js_set_object_marshal_type(JsObjectMarshalType objectMarshalType);

		[DllImport("VroomJsNative")]
		static extern IntPtr jsengine_new();

		[DllImport("VroomJsNative")]
		static extern void jsengine_dispose(HandleRef engine);

		HashSet<HandleRef> _aliveContexts = new HashSet<HandleRef>();

		static JsEngine() {
			JsObjectMarshalType objectMarshalType = JsObjectMarshalType.Dictionary;
#if NET40
        	objectMarshalType = JsObjectMarshalType.Dynamic;
#endif
			js_set_object_marshal_type(objectMarshalType);
		}

		readonly HandleRef _engine;

		public JsEngine() {
			_engine = new HandleRef(this, jsengine_new());
		}

		public JsContext CreateContext() {
			CheckDisposed();
			JsContext ctx = new JsContext(_engine, ContextDisposed);
			lock(_aliveContexts) {
				_aliveContexts.Add(ctx.Handle);
			}
			return ctx;
		}

		private void ContextDisposed(HandleRef handle) {
			lock (_aliveContexts) {
				_aliveContexts.Remove(handle);
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
            	foreach (HandleRef aliveContext in _aliveContexts) {
					JsContext.jscontext_dispose(aliveContext);
            	}
				_aliveContexts.Clear();
            }

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
