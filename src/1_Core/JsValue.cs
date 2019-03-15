//MIT, 2015-present, WinterDev, EngineKit, brezza92
//MIT, 2013, Federico Di Gregorio <fog@initd.org>

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

namespace Espresso
{

    //---------------------------------------
    //2017-06-04
    //1. for internal inter-op only -> always be private
    //for inter-op with native lib, .net core on macOS x64 dose not support explicit layout
    //so we need sequential layout
    //2. this is a quite large object, and is designed to be used on stack,
    //pass by reference to native side
    //---------------------------------------
    /// <summary>
    /// for internal inter-op only -> always be private,used on stack,pass by reference
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct JsValue
    {
        /// <summary>
        /// type and flags
        /// </summary>
        public JsValueType Type;
        /// <summary>
        /// this for 32 bits values, also be used as string len, array len  and index to managed slot index
        /// </summary>
        public int I32;//
        /// <summary>
        /// native ptr (may point to native object, native array, native string)
        /// </summary>
        public IntPtr Ptr;
        /// <summary>
        /// store float or double
        /// </summary>
        public double Num;// 
        /// <summary>
        /// store 64 bits value
        /// </summary>
        public long I64;//




        //--------------------------------
        public void Dispose()
        {
            //TODO:
            //if v dose not contain unmanaged data 
            //then we don't send it back to delete on unmanaged side
            switch (this.Type)
            {
                case JsValueType.Number:
                case JsValueType.Boolean:
                case JsValueType.Date:
                case JsValueType.Empty:
                    break;
                default:
                    jsvalue_dispose(ref this);
                    break;
            }

        }
        [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        static extern void jsvalue_dispose(ref JsValue value);
    }



    enum JsValueType : int
    {
        UnknownError = -1,
        Empty = 0,
        Null = 1,
        Boolean = 2,
        Integer = 3,
        Number = 4,
        String = 5,
        Date = 6,
        Index = 7,
        Array = 10,
        StringError = 11,
        Managed = 12,
        ManagedError = 13,
        Wrapped = 14,
        Dictionary = 15,
        Error = 16,
        Function = 17,

        //---------------
        //my extension
        JsTypeWrap = 18
    }

    enum JsManagedError
    {
        Empty,
        SetPropertyError,
        SetPropertyNotFound,
        GetPropertyNotFound,
        SetKeepAliveError,
        NotFoundManagedObjectId,
        TargetInvocationError,
    }
}
