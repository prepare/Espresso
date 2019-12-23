//MIT, 2019, WinterDev

using System;
using System.Collections;
using System.Collections.Generic;
using Espresso.Extension;

namespace Espresso
{
    partial class JsContext
    {
        NodeJsApi.NapiEnv _nodeJsNapiEnv;
        public NodeJsApi.NapiEnv GetJsNodeNapiEnv()
        {
            if (_nodeJsNapiEnv != null) return _nodeJsNapiEnv;

            JsValue nativeContext = new JsValue();
            js_new_napi_env(this.NativeContextHandle, ref nativeContext);
            if (nativeContext.Type == JsValueType.Wrapped)
            {
                //OK
                return _nodeJsNapiEnv = new NodeJsApi.NapiEnv(nativeContext.Ptr);
            }
            else
            {
                return null;
            }
        }
    }
}


namespace Espresso.NodeJsApi
{
    using static NodeJsApiNativeMethods; //provide napi binding points

    public interface INapiValue
    {
        IntPtr UnmanagedPtr { get; }
    }

    public class NodeJsArray : INapiValue
    {
        IntPtr _nativePtr;
        internal NodeJsArray(IntPtr ptr)
        {
            _nativePtr = ptr;
        }
        public IntPtr UnmanagedPtr => _nativePtr;
    }
    public class NodeJsString : INapiValue
    {
        IntPtr _nativePtr;
        internal NodeJsString(IntPtr ptr)
        {
            _nativePtr = ptr;
        }
        public IntPtr UnmanagedPtr => _nativePtr;
    }

    public class NapiEnv
    {
        readonly IntPtr _napi_env;
        internal NapiEnv(IntPtr napi_env)
        {
            _napi_env = napi_env;
        }

        public NodeJsArray CreateArray()
        {
            //TODO: check return status
            napi_status status = napi_create_array(_napi_env, out IntPtr nativeNodeJsArr);
            if (status == napi_status.napi_ok)
            {
                return new NodeJsArray(nativeNodeJsArr);
            }
            else
            {
                //
                return null;
            }
        }
        public NodeJsArray CreateArray(int len)
        {
            //TODO: check return status
            napi_status status = napi_create_array_with_length(_napi_env, len, out IntPtr nativeNodeJsArr);
            if (status == napi_status.napi_ok)
            {
                return new NodeJsArray(nativeNodeJsArr);
            }
            else
            {
                //
                return null;
            }
        }

        public NodeJsString CreateString(string input)
        {
            //TODO: check return status
            byte[] str_buffer = System.Text.Encoding.UTF8.GetBytes(input);

            unsafe
            {
                fixed (byte* str_buffer_ptr = &str_buffer[0])
                {
                    napi_status status = napi_create_string_utf8(_napi_env, (IntPtr)str_buffer_ptr,
                        str_buffer.Length, out IntPtr nativeNodeJsArr);

                    if (status == napi_status.napi_ok)
                    {
                        return new NodeJsString(nativeNodeJsArr);
                    }
                    else
                    {
                        //
                        return null;
                    }
                }

            }
        }
    }


}