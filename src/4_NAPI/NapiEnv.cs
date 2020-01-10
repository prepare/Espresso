//MIT, 2019, WinterDev

using System;
using System.Collections;
using System.Collections.Generic;
using Espresso.Extension;
using System.Runtime.InteropServices;

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

    public class NodeJsValue : INapiValue
    {
        IntPtr _nativePtr;
        public NodeJsValue(IntPtr ptr)
        {
            _nativePtr = ptr;
        }
        public IntPtr UnmanagedPtr => _nativePtr;

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
    public class NodeJsExternalBuffer : INapiValue
    {
        IntPtr _nativePtr;
        internal NodeJsExternalBuffer(IntPtr ptr)
        {
            _nativePtr = ptr;
        }
        public IntPtr UnmanagedPtr => _nativePtr;
    }


    public class MyNativeMemBuffer : System.IDisposable
    {

        static Dictionary<int, MyNativeMemBuffer> s_registerMems = new Dictionary<int, MyNativeMemBuffer>();
        static int s_memId = 0;

        int _buffer_id;
        private MyNativeMemBuffer() { }
        public int Length { get; private set; }
        public IntPtr Ptr { get; private set; }


        static MyNativeMemBuffer()
        {
            //temp fix

        }

        public void Dispose()
        {
            if (Ptr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(Ptr);
                Ptr = IntPtr.Zero;
            }
        }

        public static MyNativeMemBuffer AllocNativeMem(int len)
        {
            IntPtr nativeMemBuffer = Marshal.AllocHGlobal(len);
            //clear native mem
            //temp fix
            unsafe
            {
                byte* ptr = (byte*)nativeMemBuffer;
                for (int i = 0; i < len; ++i)
                {
                    ptr[i] = 0;
                }

            }

            if (nativeMemBuffer != IntPtr.Zero)
            {
                MyNativeMemBuffer buffer = new MyNativeMemBuffer();
                buffer.Ptr = nativeMemBuffer;
                buffer.Length = len;
                buffer._buffer_id = System.Threading.Interlocked.Increment(ref s_memId);
                //register this buffer
                lock (s_registerMems)
                {
                    s_registerMems.Add(buffer._buffer_id, buffer);
                }
                return buffer;
            }
            else
            {
                return null;
            }
        }
    }

    public static class MyNativeMemBufferNapiEnvExtensions
    {
        public static NodeJsExternalBuffer CreateExternalBuffer(this NapiEnv env, MyNativeMemBuffer memBuffer)
        {
            //we must handle the native buffer properly!!!
            return env.CreateExternalBuffer(memBuffer.Ptr, memBuffer.Length);
        }
    }

    public enum napi_valuetype
    {
        // ES6 types (corresponds to typeof)
        napi_undefined,
        napi_null,
        napi_boolean,
        napi_number,
        napi_string,
        napi_symbol,
        napi_object,
        napi_function,
        napi_external,
        napi_bigint,
    }
    public class NapiEnv
    {
        readonly IntPtr _napi_env;
        static napi_finalize s_napiFinalize;
        static IntPtr s_finalizerPtr;
        static NapiEnv()
        {
            s_napiFinalize = NapiMemFinalizer;
            s_finalizerPtr = Marshal.GetFunctionPointerForDelegate(s_napiFinalize);
        }
        static void NapiMemFinalizer(IntPtr env, IntPtr finalize_data, IntPtr finalize_hint)
        {

        }
        internal NapiEnv(IntPtr napi_env)
        {
            _napi_env = napi_env;
        }

        public NodeJsArray CreateArray()
        {

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

        public NodeJsExternalBuffer CreateExternalBuffer(IntPtr nativeBuffer, int byteLen)
        {
            //we must handle the native buffer properly!!!
            napi_status status = napi_create_external_buffer(_napi_env,
                byteLen,
                nativeBuffer,
                //s_finalizerPtr,//TODO: review here, use custom finalizer ???
                IntPtr.Zero,
                IntPtr.Zero,
                out IntPtr result);
            if (status == napi_status.napi_ok)
            {
                return new NodeJsExternalBuffer(result);
            }
            else
            {
                return null;
            }
        }


        public NodeJsString CreateString(string input)
        {

            byte[] str_buffer = System.Text.Encoding.UTF8.GetBytes(input);
            unsafe
            {
                fixed (byte* str_buffer_ptr = &str_buffer[0])
                {
                    napi_status status = napi_create_string_utf8(_napi_env, (IntPtr)str_buffer_ptr,
                        str_buffer.Length,
                        out IntPtr result);

                    if (status == napi_status.napi_ok)
                    {
                        return new NodeJsString(result);
                    }
                    else
                    {
                        //
                        return null;
                    }
                }
            }
        }

        public NodeJsValue RunScript(string script)
        {
            unsafe
            {

                NodeJsString script_nodejs_string = CreateString(script);
                if (script_nodejs_string != null)
                {
                    napi_status status = napi_run_script(_napi_env,
                       script_nodejs_string.UnmanagedPtr,
                        out IntPtr result
                    );

                    if (status == napi_status.napi_ok)
                    {
                        return new NodeJsValue(result);
                    }
                }
                return null;
            }
        }
        public napi_valuetype Typeof(IntPtr value)
        {
            napi_typeof(_napi_env, value, out napi_valuetype value_type);
            return value_type;
        }
    }
}