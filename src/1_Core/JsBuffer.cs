//MIT, 2015-present, WinterDev, EngineKit, brezza92

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
using System.Runtime.InteropServices;
using Espresso.Extension;

namespace Espresso
{
    public class JsBuffer
    {
        JsObject _jsobj;
        public JsBuffer(JsObject jsobj)
        {
            _jsobj = jsobj;
        }
        public int GetBufferLen()
        {
            JsValue value = new JsValue();
            JsContext.jsvalue_buffer_get_len(_jsobj.Context.NativeContextHandle, _jsobj.Handle, ref value);
            return value.I32;
        }
        /// <summary>
        /// copy data from nodejs side to .net side
        /// </summary>
        /// <param name="dstMem"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public int CopyBuffer(IntPtr dstMem, int len)
        {
            JsValue value = new JsValue();
            JsContext.jsvalue_buffer_copy_buffer_data(
                _jsobj.Context.NativeContextHandle,
                _jsobj.Handle,
                dstMem,
                len,
                ref value);

            return value.I32;
        }
        public void SetBuffer(IntPtr srcMem, int copyLen)
        {
            JsValue value = new JsValue();
            JsContext.jsvalue_buffer_write_buffer_data(
                _jsobj.Context.NativeContextHandle,
                _jsobj.Handle,
                 0,
                srcMem,
                copyLen,
                ref value);
        }
        public void SetBuffer(IntPtr srcMem, int srcOffset, int copyLen)
        {
            JsValue value = new JsValue();
            JsContext.jsvalue_buffer_write_buffer_data(
                _jsobj.Context.NativeContextHandle,
                _jsobj.Handle,
                srcOffset,
                srcMem,
                copyLen,
                ref value);
        }
    }

}
