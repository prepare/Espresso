//MIT, 2019, WinterDev

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Espresso.Extension;

namespace Espresso.NodeJsApi
{
    enum napi_status
    {
        //from NodeJs
        //see => js_native_api.h 
        napi_ok,
        napi_invalid_arg,
        napi_object_expected,
        napi_string_expected,
        napi_name_expected,
        napi_function_expected,
        napi_number_expected,
        napi_boolean_expected,
        napi_array_expected,
        napi_generic_failure,
        napi_pending_exception,
        napi_cancelled,
        napi_escape_called_twice,
        napi_handle_scope_mismatch,
        napi_callback_scope_mismatch,
        napi_queue_full,
        napi_closing,
        napi_bigint_expected,
        napi_date_expected,
        napi_arraybuffer_expected,
        napi_detachable_arraybuffer_expected,
    }

    static class NodeJsApiNativeMethods
    {

        //https://nodejs.org/api/n-api.html#n_api_napi_create_array
        /// <summary>
        /// This API returns an N-API value corresponding to a JavaScript Array type. JavaScript arrays are described in Section 22.1 of the ECMAScript Language  
        /// </summary>         
        /// <param name="env"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        [DllImport(JsBridge.LIB_NAME)]
        internal static extern napi_status napi_create_array(IntPtr env, out IntPtr result);


        //https://nodejs.org/api/n-api.html#n_api_napi_create_array_with_length
        /// <summary>
        /// This API returns an N-API value corresponding to a JavaScript Array type. The Array's length property is set to the passed-in length parameter. However, the underlying buffer is not guaranteed to be pre-allocated by the VM when the array is created. That behavior is left to the underlying VM implementation. If the buffer must be a contiguous block of memory that can be directly read and/or written via C, consider using napi_create_external_arraybuffer.
        /// </summary>
        /// <param name="env"></param>
        /// <param name="length"> The initial length of the Array</param>
        /// <param name="result"></param>
        /// <returns></returns>
        [DllImport(JsBridge.LIB_NAME)]
        internal static extern napi_status napi_create_array_with_length(IntPtr env, int length, out IntPtr result);


        //https://nodejs.org/api/n-api.html#n_api_napi_create_arraybuffer
        /// <summary>
        /// This API returns an N-API value corresponding to a JavaScript ArrayBuffer. ArrayBuffers are used to represent fixed-length binary data buffers. They are normally used as a backing-buffer for TypedArray objects. The ArrayBuffer allocated will have an underlying byte buffer whose size is determined by the length parameter that's passed in. The underlying buffer is optionally returned back to the caller in case the caller wants to directly manipulate the buffer. This buffer can only be written to directly from native code. To write to this buffer from JavaScript, a typed array or DataView object would need to be created.        
        /// </summary>
        /// <param name="env"></param>
        /// <param name="byte_length">The length in bytes of the array buffer to create</param>
        /// <param name="result_memPtr"> Pointer to the underlying byte buffer of the ArrayBuffer.</param>
        /// <param name="result"></param>
        /// <returns></returns>
        [DllImport(JsBridge.LIB_NAME)]
        internal static extern napi_status napi_create_arraybuffer(IntPtr env,
            int byte_length,
            out IntPtr result_memPtr,
            out IntPtr result);


        //https://nodejs.org/api/n-api.html#n_api_napi_create_buffer
        /// <summary>
        /// This API allocates a node::Buffer object. While this is still a fully-supported data structure, in most cases using a TypedArray will suffice.
        /// </summary>
        /// <param name="env"></param>
        /// <param name="size">Size in bytes of the underlying buffer.</param>
        /// <param name="nativeMemPtr">Raw pointer to the underlying buffer</param>
        /// <param name="result"></param>
        /// <returns></returns>
        [DllImport(JsBridge.LIB_NAME)]
        internal static extern napi_status napi_create_buffer(IntPtr env,
           int size,
           out IntPtr nativeMemPtr,
           out IntPtr result);


        //https://nodejs.org/api/n-api.html#n_api_napi_create_buffer_copy
        /// <summary>
        /// This API allocates a node::Buffer object and initializes it with data copied from the passed-in buffer. While this is still a fully-supported data structure, in most cases using a TypedArray will suffice.
        /// </summary>
        /// <param name="env"></param>
        /// <param name="length">Size in bytes of the input buffer (should be the same as the size of the new buffer).</param>
        /// <param name="raw_src_data"> Raw pointer to the underlying buffer to copy from.</param>
        /// <param name="result_data"> Pointer to the new Buffer's underlying data buffer.</param>
        /// <param name="result"> A napi_value representing a node::Buffer.</param>
        /// <returns></returns>
        [DllImport(JsBridge.LIB_NAME)]
        internal static extern napi_status napi_create_buffer_copy(IntPtr env,
            int length,
            IntPtr raw_src_data,
            out IntPtr result_data,
            out IntPtr result);


        //--------------------
        //https://nodejs.org/api/n-api.html#n_api_napi_create_date
        /// <summary>
        /// This API allocates a JavaScript Date object.
        /// This API does not observe leap seconds; 
        /// they are ignored, as ECMAScript aligns with POSIX time specification. 
        /// </summary>
        /// <param name="env"></param>
        /// <param name="time"> ECMAScript time value in milliseconds since 01 January, 1970 UTC.</param>
        /// <param name="result">A napi_value representing a JavaScript Date.</param>
        /// <returns></returns>
        [DllImport(JsBridge.LIB_NAME)]
        internal static extern napi_status napi_create_date(IntPtr env,
           double time,
           out IntPtr result);
         

    }
}
