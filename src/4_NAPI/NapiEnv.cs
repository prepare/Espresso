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
    }


}