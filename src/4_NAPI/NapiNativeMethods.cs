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


    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void napi_finalize(IntPtr env, IntPtr finalize_data, IntPtr finalize_hint);

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



        //-------------------
        //External ...
        //https://nodejs.org/api/n-api.html#n_api_napi_create_external
        /// <summary>
        /// This API allocates a JavaScript value with external data attached to it. This is used to pass external data through JavaScript code, so it can be retrieved later by native code using napi_get_value_external.
        /// </summary>
        /// <param name="env"></param>
        /// <param name="nativeMemPtr">Raw pointer to the external data</param>
        /// <param name="finalize_cb">Optional callback to call when the external value is being collected</param>
        /// <param name="finalize_hint"> Optional hint to pass to the finalize callback during collection.</param>
        /// <param name="result"> A napi_value representing an external value</param>
        /// <returns></returns>
        [DllImport(JsBridge.LIB_NAME)]
        internal static extern napi_status napi_create_external(IntPtr env,
           IntPtr nativeMemPtr,
           IntPtr finalize_cb,
           IntPtr finalize_hint,
           out IntPtr result);
        //        The API adds a napi_finalize callback which will be called when the JavaScript object just created is ready for garbage collection. It is similar to napi_wrap() except that:

        //    the native data cannot be retrieved later using napi_unwrap(),
        //    nor can it be removed later using napi_remove_wrap(), and
        //    the object created by the API can be used with napi_wrap().
        //The created value is not an object, and therefore does not support additional properties. It is considered a distinct value type: calling napi_typeof() with an external value yields napi_external.



        /// <summary>
        /// https://nodejs.org/api/n-api.html#n_api_napi_create_external_arraybuffer
        /// </summary>
        /// <param name="env"></param>
        /// <param name="nativeMemPtr_external_data"> Pointer to the underlying byte buffer of the ArrayBuffer</param>
        /// <param name="byte_length"> The length in bytes of the underlying buffer.</param>
        /// <param name="finalize_cb">Optional callback to call when the ArrayBuffer is being collected.</param>
        /// <param name="finalize_hint"> Optional hint to pass to the finalize callback during collection.</param>
        /// <param name="result"> A napi_value representing a JavaScript ArrayBuffer</param>
        /// <returns>Returns napi_ok if the API succeeded.</returns>
        [DllImport(JsBridge.LIB_NAME)]
        internal static extern napi_status napi_create_external_arraybuffer(IntPtr env,
            IntPtr nativeMemPtr_external_data,
            int byte_length,
            IntPtr finalize_cb,
            IntPtr finalize_hint,
            out IntPtr result);

        //This API returns an N-API value corresponding to a JavaScript ArrayBuffer. The underlying byte buffer of the ArrayBuffer is externally allocated and managed. The caller must ensure that the byte buffer remains valid until the finalize callback is called.
        //The API adds a napi_finalize callback which will be called when the JavaScript object just created is ready for garbage collection. It is similar to napi_wrap() except that:
        //    the native data cannot be retrieved later using napi_unwrap(),
        //    nor can it be removed later using napi_remove_wrap(), and
        //    the object created by the API can be used with napi_wrap().
        //JavaScript ArrayBuffers are described in Section 24.1 of the ECMAScript Language Specification.


        //https://nodejs.org/api/n-api.html#n_api_napi_create_external_buffer
        /// <summary>
        /// This API allocates a node::Buffer object and initializes it with data backed by the passed in buffer. While this is still a fully-supported data structure, in most cases using a TypedArray will suffice.
        /// </summary>
        /// <param name="env"></param>
        /// <param name="length">Size in bytes of the input buffer (should be the same as the size of the new buffer)</param>
        /// <param name="nativeMemPtr_external_data">Raw pointer to the underlying buffer to copy from</param>
        /// <param name="finalize_cb">Optional callback to call when the ArrayBuffer is being collected.</param>
        /// <param name="finalize_hint"> Optional hint to pass to the finalize callback during collection.</param>
        /// <param name="result">A napi_value representing a node::Buffer</param>
        /// <returns>Returns napi_ok if the API succeeded.</returns>
        [DllImport(JsBridge.LIB_NAME)]
        internal static extern napi_status napi_create_external_buffer(IntPtr env,
           int length,
           IntPtr nativeMemPtr_external_data,
           IntPtr finalize_cb,
           IntPtr finalize_hint,
           out IntPtr result);
        // This API allocates a node::Buffer object and initializes it with data backed by the passed in buffer. While this is still a fully-supported data structure, in most cases using a TypedArray will suffice.

        //The API adds a napi_finalize callback which will be called when the JavaScript object just created is ready for garbage collection. It is similar to napi_wrap() except that:

        //    the native data cannot be retrieved later using napi_unwrap(),
        //    nor can it be removed later using napi_remove_wrap(), and
        //    the object created by the API can be used with napi_wrap().

        //For Node.js >=4 Buffers are Uint8Arrays. //***

        //-------------------
        //https://nodejs.org/api/n-api.html#n_api_napi_create_string_utf16
        /// <summary>
        /// This API creates a JavaScript String object from a UTF16-LE-encoded C string. The native string is copied.
        /// </summary>
        /// <param name="env"></param>
        /// <param name="str">Character buffer representing a UTF16-LE-encoded string.</param>
        /// <param name="size">The length of the string in two-byte code units, or NAPI_AUTO_LENGTH if it is null-terminated.</param>
        /// <param name="result">A napi_value representing a JavaScript String</param>
        /// <returns>Returns napi_ok if the API succeeded.</returns>
        [DllImport(JsBridge.LIB_NAME)]
        internal static extern napi_status napi_create_string_utf16(IntPtr env,
           IntPtr str,
           int size,
           out IntPtr result);
        //The JavaScript String type is described in Section 6.1.4 of the ECMAScript Language Specification.


        //https://nodejs.org/api/n-api.html#n_api_napi_create_string_utf8
        /// <summary>
        /// This API creates a JavaScript String object from a UTF8-encoded C string. The native string is copied.
        /// </summary>
        /// <param name="env"></param>
        /// <param name="str">Character buffer representing a UTF8-encoded string</param>
        /// <param name="size">The length of the string in bytes, or NAPI_AUTO_LENGTH if it is null-terminated.</param>
        /// <param name="result">A napi_value representing a JavaScript String.</param>
        /// <returns>napi_ok if the API succeeded.</returns>
        [DllImport(JsBridge.LIB_NAME)]
        internal static extern napi_status napi_create_string_utf8(IntPtr env,
          IntPtr str,
          int size,
          out IntPtr result);
        //The JavaScript String type is described in Section 6.1.4 of the ECMAScript Language Specification.



        //.. TODO.... add more...


        //Script execution
        //https://nodejs.org/api/n-api.html#n_api_napi_run_script
        /// <summary> 
        ///  This function executes a string of JavaScript code and returns its result with the following caveats:
        /// Unlike eval, this function does not allow the script to access the current lexical scope, and therefore also does not allow to access the module scope, meaning that pseudo-globals such as require will not be available.
        /// The script can access the global scope.Function and var declarations in the script will be added to the global object. Variable declarations made using let and const will be visible globally, but will not be added to the global object.
        /// The value of this is global within the script.
        /// </summary>
        /// <param name="env"></param>
        /// <param name="script"> A JavaScript string containing the script to execute.</param>
        /// <param name="result">The value resulting from having executed the script.</param>
        /// <returns></returns>
        // 
        [DllImport(JsBridge.LIB_NAME)]
        internal static extern napi_status napi_run_script(IntPtr env,
            IntPtr script,
            out IntPtr result);


        //-----------
        /// <summary>
        /// This API represents behavior similar to invoking the typeof Operator on the object as defined in 
        /// Section 12.5.5 of the ECMAScript Language Specification.
        /// However, it has support for detecting an External value.
        /// If value has a type that is invalid, an error is returned.
        /// </summary>
        /// <param name="env"></param>
        /// <param name="value"> The JavaScript value whose type to query.</param>
        /// <param name="result">The type of the JavaScript value</param>
        /// <returns></returns>
        [DllImport(JsBridge.LIB_NAME)]
        internal static extern napi_status napi_typeof(IntPtr env,
            IntPtr value,
            out napi_valuetype result);



        /// <summary>
        /// This API represents invoking the IsArray operation on the object as defined in 
        /// Section 7.2.2 of the ECMAScript Language Specification.
        /// </summary>
        /// <param name="env"></param>
        /// <param name="value">The JavaScript value to check</param>
        /// <param name="result">Whether the given object is an array.</param>
        /// <returns></returns>
        [DllImport(JsBridge.LIB_NAME)]
        internal static extern napi_status napi_is_array(IntPtr env,
          IntPtr value,
          out bool result);


        /// <summary>
        /// This API checks if the Object passed in is an array buffer.
        /// </summary>
        /// <param name="env"></param>
        /// <param name="value">The JavaScript value to check</param>
        /// <param name="result">Whether the given object is an ArrayBuffer</param>
        /// <returns></returns>
        [DllImport(JsBridge.LIB_NAME)]
        internal static extern napi_status napi_is_arraybuffer(IntPtr env,
            IntPtr value,
            out bool result);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="env"></param>
        /// <param name="value">The JavaScript value to check</param>
        /// <param name="result"> Whether the given napi_value represents a node::Buffer object.</param>
        /// <returns></returns>
        [DllImport(JsBridge.LIB_NAME)]
        internal static extern napi_status napi_is_buffer(IntPtr env,
           IntPtr value,
           out bool result);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="env"></param>
        /// <param name="value"></param>
        /// <param name="result">Whether the given napi_value represents a JavaScript Date object.</param>
        /// <returns></returns>
        [DllImport(JsBridge.LIB_NAME)]
        internal static extern napi_status napi_is_date(IntPtr env,
          IntPtr value,
          out bool result);

        /// <summary>
        /// This API checks if the Object passed in is an Error.
        /// </summary>
        /// <param name="env"></param>
        /// <param name="value"></param>
        /// <param name="result">Whether the given napi_value represents an Error object</param>
        /// <returns></returns>
        [DllImport(JsBridge.LIB_NAME)]
        internal static extern napi_status napi_is_error(IntPtr env,
              IntPtr value,
              out bool result);

        /// <summary>
        /// This API checks if the Object passed in is a typed array.
        /// </summary>
        /// <param name="env"></param>
        /// <param name="value"></param>
        /// <param name="result">Whether the given napi_value represents a TypedArray</param>
        /// <returns></returns>
        [DllImport(JsBridge.LIB_NAME)]
        internal static extern napi_status napi_is_typedarray(IntPtr env,
           IntPtr value,
           out bool result);

        /// <summary>
        /// This API checks if the Object passed in is a DataView.
        /// </summary>
        /// <param name="env"></param>
        /// <param name="value"></param>
        /// <param name="result"> Whether the given napi_value represents a DataView.</param>
        /// <returns></returns>
        [DllImport(JsBridge.LIB_NAME)]
        internal static extern napi_status napi_is_dataview(IntPtr env,
          IntPtr value,
          out bool result);
        //-----------

    }
}
