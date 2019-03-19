//MIT, 2015-present, WinterDev, EngineKit, brezza92
//MIT, 2013, Federico Di Gregorio <fog@initd.org>

using System;
using System.Collections.Generic;

namespace Espresso
{
    partial class JsContext
    {


        public IEnumerable<string> GetMemberNames(JsObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            CheckDisposed();

            if (obj.Handle == IntPtr.Zero)
                throw new JsInteropException("wrapped V8 object is empty (IntPtr is Zero)");


            JsValue v = new JsValue();
            jscontext_get_property_names(_context, obj.Handle, ref v);
            object res = _convert.FromJsValue(ref v);

            v.Dispose();
            Exception e = res as JsException;
            if (e != null)
                throw e;

            object[] arr = (object[])res;
            string[] strArr = new string[arr.Length];
            for (int i = arr.Length - 1; i >= 0; --i)
            {
                strArr[i] = arr[i].ToString();
            }
            return strArr;
        }


        public object GetPropertyValue(JsObject obj, string name)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();

            if (obj.Handle == IntPtr.Zero)
                throw new JsInteropException("wrapped V8 object is empty (IntPtr is Zero)");

            JsValue output = new JsValue();
            jscontext_get_property_value(_context, obj.Handle, name, ref output);
            //
            object res = _convert.FromJsValue(ref output);
            //TODO: review here
            //we should dispose only type that contains native data***

            output.Dispose();
            Exception e = res as JsException;
            if (e != null)
                throw e;
            return res;
        }

        public void SetPropertyValue(JsObject obj, string name, object value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();

            if (obj.Handle == IntPtr.Zero)
                throw new JsInteropException("wrapped V8 object is empty (IntPtr is Zero)");

            JsValue a = new JsValue();
            JsValue output = new JsValue();

            _convert.AnyToJsValue(value, ref a);
            jscontext_set_property_value(_context, obj.Handle, name, ref a, ref output);

            //TODO: review exceptio here
            //not need to convert all the time if we not have error
            object res = _convert.FromJsValue(ref output);

            output.Dispose();
            a.Dispose();
            Exception e = res as JsException;
            if (e != null)
                throw e;
        }

        public object InvokeProperty(JsObject obj, string name, object[] args)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();

            if (obj.Handle == IntPtr.Zero)
                throw new JsInteropException("wrapped V8 object is empty (IntPtr is Zero)");


            JsValue a = new JsValue(); // Null value unless we're given args.
            a.Type = JsValueType.Null;
            if (args != null)
            {
                _convert.AnyToJsValue(args, ref a);
            }

            JsValue v = new JsValue();
            jscontext_invoke_property(_context, obj.Handle, name, ref a, ref v);
            object res = _convert.FromJsValue(ref v);
            v.Dispose();
            a.Dispose();

            Exception e = res as JsException;
            if (e != null)
                throw e;
            return res;
        }
    }
}
