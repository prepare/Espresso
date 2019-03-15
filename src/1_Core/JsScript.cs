//MIT, 2015-present, WinterDev, EngineKit, brezza92
//MIT, 2013, Federico Di Gregorio <fog@initd.org>
using System;
using System.Runtime.InteropServices;

namespace Espresso
{
    public class JsScript : IDisposable
    {

        readonly int _id;
        readonly JsEngine _engine;
        readonly HandleRef _script;
        readonly Action<int> _notifyDispose;
        bool _disposed;
        internal JsScript(int id, JsEngine engine, HandleRef engineHandle, JsConvert convert, string code, string name, Action<int> notifyDispose)
        {
            _id = id;
            _engine = engine;
            _notifyDispose = notifyDispose;

            _script = new HandleRef(this, jsscript_new(engineHandle));
            JsValue output = new JsValue();
            jsscript_compile(_script, code, name, ref output);

            object res = convert.FromJsValue(ref output);
            Exception e = res as JsException;
            if (e != null)
            {
                throw e;
            }
        }

        internal JsEngine Engine => _engine;

        internal HandleRef Handle => _script;

        public bool IsDisposed => _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            CheckDisposed();

            _disposed = true;

            jsscript_dispose(_script);

            _notifyDispose(_id);
        }

        void CheckDisposed()
        {
            if (_engine.IsDisposed)
            {
                throw new ObjectDisposedException("JsScript: engine has been disposed");
            }
            if (_disposed)
                throw new ObjectDisposedException("JsScript:" + _script.Handle);
        }

        ~JsScript()
        {
            if (!_engine.IsDisposed && !_disposed)
                Dispose(false);
        }


        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.StdCall)]
        static extern IntPtr jsscript_new(HandleRef engine);


        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern void jsscript_compile(HandleRef script,
            string str,
            string name,
            ref JsValue output);

        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.StdCall)]
        static extern IntPtr jsscript_dispose(HandleRef script);


    }
}
