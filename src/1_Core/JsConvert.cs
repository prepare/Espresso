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
    class JsConvert
    {
        static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        static readonly DateTime EPOCH_LocalTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        readonly JsContext _context;
        public JsConvert(JsContext context)
        {
            _context = context;
        }
        /// <summary>
        /// convert from jsvalue to managed value
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public object FromJsValue(ref JsValue v)
        {
#if DEBUG_TRACE_API
			Console.WriteLine("Converting Js value to .net");
#endif
            switch (v.Type)
            {
                case JsValueType.Empty:
                case JsValueType.Null:
                    return null;
                case JsValueType.Boolean:
                    return v.I32 != 0;
                case JsValueType.Integer:
                    return v.I32;
                case JsValueType.Index:
                    return (UInt32)v.I64;
                case JsValueType.Number:
                    return v.Num;
                case JsValueType.String:
                    return Marshal.PtrToStringUni(v.Ptr);
                case JsValueType.Date:
                    /*
                    // The formula (v.num * 10000) + 621355968000000000L was taken from a StackOverflow
                    // question and should be OK. Then why do we need to compensate by -26748000000000L
                    // (a value determined from the failing tests)?!
                    return new DateTime((long)(v.Num * 10000) + 621355968000000000L - 26748000000000L);
                     */
                    //var msFromJsTime = v.I64 % 1000;
                    return EPOCH_LocalTime.AddMilliseconds(v.I64);
                case JsValueType.Array:
                    {
                        int len = v.I32;
                        object[] newarr = new object[len];
                        unsafe
                        {
                            JsValue* arr = (JsValue*)v.Ptr;
                            for (int i = 0; i < len; ++i)
                            {
                                newarr[i] = FromJsValuePtr((arr + i));
                            }
                        }
                        return newarr;
                    }

                case JsValueType.UnknownError:
                    if (v.Ptr != IntPtr.Zero)
                        return new JsException(Marshal.PtrToStringUni(v.Ptr));
                    return new JsInteropException("unknown error without reason");

                case JsValueType.StringError:
                    return new JsException(Marshal.PtrToStringUni(v.Ptr));

                case JsValueType.Managed:
                    return _context.KeepAliveGet(v.I32);
                case JsValueType.JsTypeWrap:
                    //auto unwrap
                    return _context.GetObjectProxy(v.I32).WrapObject;
                case JsValueType.ManagedError:
                    Exception inner = _context.KeepAliveGet(v.I32) as Exception;
                    string msg = null;
                    if (v.Ptr != IntPtr.Zero)
                    {
                        msg = Marshal.PtrToStringUni(v.Ptr);
                    }
                    else if (inner != null)
                    {
                        msg = inner.Message;
                    }
                    return new JsException(msg, inner);
                case JsValueType.Dictionary:
                    return CreateJsDictionaryObject(ref v);

                case JsValueType.Wrapped:
                    return new JsObject(_context, v.Ptr);
                case JsValueType.Error:
                    return JsException.Create(this, v.Ptr);
                case JsValueType.Function:
                    //convert from js function delegate to managed
                    //this compose of function ptr and delegate's target
                    unsafe
                    {
                        JsValue* arr = (JsValue*)v.Ptr;
                        return new JsFunction(_context, (arr)->Ptr, (arr + 1)->Ptr);
                    }
                default:
                    throw new InvalidOperationException("unknown type code: " + v.Type);
            }
        }
        public unsafe object FromJsValuePtr(JsValue* v)
        {
#if DEBUG_TRACE_API
			Console.WriteLine("Converting Js value to .net");
#endif
            switch (v->Type)
            {
                case JsValueType.Empty:
                case JsValueType.Null:
                    return null;
                case JsValueType.Boolean:
                    return v->I32 != 0;
                case JsValueType.Integer:
                    return v->I32;
                case JsValueType.Index:
                    return (UInt32)v->I64;
                case JsValueType.Number:
                    return v->Num;
                case JsValueType.String:
                    return Marshal.PtrToStringUni(v->Ptr);
                case JsValueType.Date:
                    /*
                    // The formula (v.num * 10000) + 621355968000000000L was taken from a StackOverflow
                    // question and should be OK. Then why do we need to compensate by -26748000000000L
                    // (a value determined from the failing tests)?!
                    return new DateTime((long)(v.Num * 10000) + 621355968000000000L - 26748000000000L);
                     */
                    return EPOCH_LocalTime.AddMilliseconds(v->I64);
                case JsValueType.Array:
                    {
                        int len = v->I32;
                        object[] newarr = new object[len];
                        JsValue* arr = (JsValue*)v->Ptr;
                        for (int i = 0; i < len; ++i)
                        {
                            newarr[i] = FromJsValuePtr((arr + i));
                        }
                        return newarr;
                    }
                case JsValueType.UnknownError:
                    if (v->Ptr != IntPtr.Zero)
                    {
                        //TODO: x64 macOS ,review string marshal again 
                        return new JsException(Marshal.PtrToStringUni(v->Ptr));
                    }
                    return new JsInteropException("unknown error without reason");
                case JsValueType.StringError:
                    //TODO: x64 macOS ,review string marshal again
                    return new JsException(Marshal.PtrToStringUni(v->Ptr));
                case JsValueType.Managed:
                    //get managed object from slot***
                    return _context.KeepAliveGet(v->I32);
                case JsValueType.JsTypeWrap:
                    //auto unwrap
                    return _context.GetObjectProxy(v->I32).WrapObject;
                case JsValueType.ManagedError:
                    Exception inner = _context.KeepAliveGet(v->I32) as Exception;
                    string msg = null;
                    if (v->Ptr != IntPtr.Zero)
                    {
                        msg = Marshal.PtrToStringUni(v->Ptr);
                    }
                    else if (inner != null)
                    {
                        msg = inner.Message;
                    }
                    return new JsException(msg, inner);
                case JsValueType.Dictionary:
                    return CreateJsDictionaryObjectFromPtr(v);
                case JsValueType.Wrapped:
                    return new JsObject(_context, v->Ptr);
                case JsValueType.Error:
                    return JsException.Create(this, v->Ptr);
                case JsValueType.Function:
                    //convert from js function delegate to managed
                    //this compose of function ptr and delegate's target
                    unsafe
                    {
                        JsValue* arr = (JsValue*)v->Ptr;
                        return new JsFunction(_context, arr->Ptr, (arr + 1)->Ptr);
                    }
                default:
                    throw new InvalidOperationException("unknown type code: " + v->Type);
            }
        }

        JsObject CreateJsDictionaryObject(ref JsValue v)
        {
            //js dic is key-pair object
            JsObject obj = new JsObject(_context, v.Ptr);
            int count = v.I32 * 2;//key and value
            unsafe
            {
                JsValue* arr = (JsValue*)v.Ptr;
                for (int i = 0; i < count;)
                {
                    JsValue* key = arr + i;
                    JsValue* value = arr + i + 1;

                    //TODO: review when key is not string 
                    obj[(string)FromJsValuePtr(key)] = FromJsValuePtr(value);
                    i += 2;
                }
            }
            return obj;
        }

        unsafe JsObject CreateJsDictionaryObjectFromPtr(JsValue* v)
        {
            //js dic is key-pair

            JsObject obj = new JsObject(_context, v->Ptr);
            int count = v->I32 * 2;//key and value
            unsafe
            {
                JsValue* arr = (JsValue*)v->Ptr;
                for (int i = 0; i < count;)
                {
                    JsValue* key = (arr + i);
                    JsValue* value = (arr + i + 1);

                    //TODO: review when key is not string 
                    obj[(string)FromJsValuePtr(key)] = FromJsValuePtr(value);
                    i += 2;
                }
            }
            return obj;
        }
        /// <summary>
        /// fill int to native jsvalue
        /// </summary>
        /// <param name="value"></param>
        /// <param name="output"></param>
        public void ToJsValue(int value, ref JsValue output)
        {
            output.Type = JsValueType.Integer;
            output.I32 = value;
        }
        public void ToJsValue(long value, ref JsValue output)
        {
            //TODO review here
            output.Type = JsValueType.Number;
            output.I64 = value;
        }

        public void ToJsValue(string value, ref JsValue output)
        {
            // We need to allocate some memory on the other side; will be free'd by unmanaged code.            
            char[] buff = value.ToCharArray();
            unsafe
            {
                fixed (char* c1 = value)
                {
                    JsContext.jsvalue_alloc_string(c1, value.Length, ref output);
                }
            }

            output.Type = JsValueType.String;
            output.I32 = value.Length;
        }
        public void ToJsValue(char c, ref JsValue output)
        {
            //TODO: review here
            // We need to allocate some memory on the other side; will be free'd by unmanaged code.            
            unsafe
            {
                JsContext.jsvalue_alloc_string(&c, 1, ref output);
            }
            output.Type = JsValueType.String;
            output.I32 = 1;
        }
        public void ToJsValue(double value, ref JsValue output)
        {
            output.Type = JsValueType.Number;
            output.Num = value;
        }
        public void ToJsValue(float value, ref JsValue output)
        {
            output.Type = JsValueType.Number;
            output.Num = value;
        }
        public void ToJsValue(decimal value, ref JsValue output)
        {
            //data loss***
            //TODO: review here, convert to string?
            output.Type = JsValueType.Number;
            output.Num = (double)value;
        }
        public void ToJsValue(bool value, ref JsValue output)
        {
            output.Type = JsValueType.Boolean;
            output.I32 = value ? 1 : 0;
        }
        public void ToJsValue(DateTime dtm, ref JsValue output)
        {
            output.Type = JsValueType.Date;
            output.Num = Convert.ToInt64(dtm.Subtract(EPOCH).TotalMilliseconds);
        }
        public void ToJsValue(INativeScriptable jsInstance, ref JsValue output)
        {
            output.Type = JsValueType.JsTypeWrap;
            output.Ptr = jsInstance.UnmanagedPtr;
            output.I32 = jsInstance.ManagedIndex;
        }
        /// <summary>
        /// fill array to native js value
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="output"></param>
        public void ToJsValue(object[] arr, ref JsValue output)
        {
            int len = arr.Length;
            //alloc array
            JsContext.jsvalue_alloc_array(len, ref output);
            if (output.I32 != len)
            {
                throw new JsInteropException("can't allocate memory on the unmanaged side");
            }
            unsafe
            {
                JsValue* nativeArr = (JsValue*)output.Ptr;
                for (int i = 0; i < len; i++)
                {
                    AnyToJsValuePtr(arr[i], nativeArr + i);
                }
            }
        }
        public void ToJsValueNull(ref JsValue output)
        {
            output.Type = JsValueType.Null;
        }
        /// <summary>
        /// convert any object to jsvalue
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="output"></param>
        public void AnyToJsValue(object obj, ref JsValue output)
        {
            if (obj == null)
            {
                output.Type = JsValueType.Null;
                return;
            }
            //-----

            if (obj is INativeRef)
            {
                //extension
                INativeRef prox = (INativeRef)obj;
                output.I32 = _context.KeepAliveAdd(obj);
                output.Type = JsValueType.JsTypeWrap;
                output.Ptr = prox.UnmanagedPtr;
                return;
            }
            //-----
            Type type = obj.GetType();

            // Check for nullable types (we will cast the value out of the box later).


            Type innerTypeOfNullable = type.ExtGetInnerTypeIfNullableValue();
            if (innerTypeOfNullable != null)
            {
                type = innerTypeOfNullable;
            }
            //
            if (type == typeof(Boolean))
            {
                output.Type = JsValueType.Boolean;
                output.I32 = (bool)obj ? 1 : 0;
                return;
            }

            if (type == typeof(String) || type == typeof(Char))
            {
                // We need to allocate some memory on the other side;
                // will be free'd by unmanaged code. 

                string strdata = obj.ToString();
                unsafe
                {
                    fixed (char* buffer = strdata)
                    {
                        JsContext.jsvalue_alloc_string(buffer, strdata.Length, ref output);
                    }
                }
                output.Type = JsValueType.String;
                return;
            }
            //-----------------------------------------------------------
            if (type == typeof(Byte))
            {
                output.Type = JsValueType.Integer;
                output.I32 = (int)(byte)obj;
                return;
            }
            if (type == typeof(Int16))
            {
                output.Type = JsValueType.Integer;
                output.I32 = (int)(Int16)obj;
                return;
            }
            if (type == typeof(UInt16))
            {
                output.Type = JsValueType.Integer;
                output.I32 = (int)(UInt16)obj;
                return;
            }
            if (type == typeof(Int32))
            {
                output.Type = JsValueType.Integer;
                output.I32 = (int)obj;
                return;
            }
            if (type == typeof(UInt32))
            {
                //TODO: review Type here when send to native side
                output.Type = JsValueType.Integer;
                output.I32 = (int)(uint)obj;
                return;
            }

            if (type == typeof(Int64))
            {
                output.Type = JsValueType.Number;
                output.Num = (double)(Int64)obj;
                return;
            }

            if (type == typeof(UInt64))
            {
                output.Type = JsValueType.Number;
                output.Num = (double)(UInt64)obj;
                return;
            }


            if (type == typeof(Single))
            {
                output.Type = JsValueType.Number;
                output.Num = (double)(Single)obj;
                return;
            }


            if (type == typeof(Double))
            {
                output.Type = JsValueType.Number;
                output.Num = (double)obj;
                return;
            }

            if (type == typeof(Decimal))
            {
                //TODO: review here
                //.net decimal is larger than double?
                output.Type = JsValueType.Number;
                output.Num = (double)(Decimal)obj;
                return;
            }

            if (type == typeof(DateTime))
            {
                output.Type = JsValueType.Date;
                output.Num = Convert.ToInt64(((DateTime)obj).Subtract(EPOCH).TotalMilliseconds); /*(((DateTime)obj).Ticks - 621355968000000000.0 + 26748000000000.0)/10000.0*/
                return;
            }
            // Arrays of anything that can be cast to object[] are recursively convertef after
            // allocating an appropriate jsvalue on the unmanaged side.

            var array = obj as object[];
            if (array != null)
            {
                //alloc space for array
                int arrLen = array.Length;
                output.Type = JsValueType.Array;
                JsContext.jsvalue_alloc_array(arrLen, ref output);
                if (output.I32 != arrLen)
                    throw new JsInteropException("can't allocate memory on the unmanaged side");

                unsafe
                {
                    JsValue* jsarr = (JsValue*)output.Ptr;
                    for (int i = 0; i < arrLen; i++)
                    {
                        AnyToJsValuePtr(array[i], (jsarr + i));
                    }
                }
                return;
            }

            // Every object explicitly converted to a value becomes an entry of the
            // _keepalives list, to make sure the GC won't collect it while still in
            // use by the unmanaged Javascript engine. We don't try to track duplicates
            // because adding the same object more than one time acts more or less as
            // reference counting.  
            //check 

            JsTypeDefinition jsTypeDefinition = _context.GetJsTypeDefinition(type);
            INativeRef prox2 = _context.CreateWrapper(obj, jsTypeDefinition);
            //
            output.Type = JsValueType.JsTypeWrap;
            output.Ptr = prox2.UnmanagedPtr;
            output.I32 = prox2.ManagedIndex;

        }
        public unsafe void AnyToJsValuePtr(object obj, JsValue* output)
        {
            if (obj == null)
            {
                output->Type = JsValueType.Null;
                return;
            }
            //-----

            if (obj is INativeRef)
            {
                //extension
                INativeRef prox = (INativeRef)obj;
                output->I32 = _context.KeepAliveAdd(obj);
                output->Type = JsValueType.JsTypeWrap;
                output->Ptr = prox.UnmanagedPtr;
                return;
            }
            //-----
            Type type = obj.GetType();
            // Check for nullable types (we will cast the value out of the box later). 
            Type innerTypeOfNullable = type.ExtGetInnerTypeIfNullableValue();
            if (innerTypeOfNullable != null)
            {
                type = innerTypeOfNullable;
            }
            
            
            if (type == typeof(Boolean))
            {
                output->Type = JsValueType.Boolean;
                output->I32 = (bool)obj ? 1 : 0;
                return;
            }

            if (type == typeof(String) || type == typeof(Char))
            {
                // We need to allocate some memory on the other side;
                // will be free'd by unmanaged code.
                string strdata = obj.ToString();
                unsafe
                {
                    fixed (char* b = strdata)
                    {
                        JsContext.jsvalue_alloc_string2(b, strdata.Length, output);
                    }
                }
                output->Type = JsValueType.String;
                return;
            }
            //-----------------------------------------------------------
            if (type == typeof(Byte))
            {
                output->Type = JsValueType.Integer;
                output->I32 = (int)(byte)obj;
                return;
            }
            if (type == typeof(Int16))
            {
                output->Type = JsValueType.Integer;
                output->I32 = (int)(Int16)obj;
                return;
            }
            if (type == typeof(UInt16))
            {
                output->Type = JsValueType.Integer;
                output->I32 = (int)(UInt16)obj;
                return;
            }
            if (type == typeof(Int32))
            {
                output->Type = JsValueType.Integer;
                output->I32 = (int)obj;
                return;
            }

            if (type == typeof(UInt32))
            {
                //TODO: review Type here when send to native side
                output->Type = JsValueType.Integer;
                output->I32 = (int)(uint)obj;
                return;
            }

            if (type == typeof(Int64))
            {
                output->Type = JsValueType.Number;
                output->Num = (double)(Int64)obj;
                return;
            }

            if (type == typeof(UInt64))
            {
                output->Type = JsValueType.Number;
                output->Num = (double)(UInt64)obj;
                return;
            }


            if (type == typeof(Single))
            {
                output->Type = JsValueType.Number;
                output->Num = (double)(Single)obj;
                return;
            }


            if (type == typeof(Double))
            {
                output->Type = JsValueType.Number;
                output->Num = (double)obj;
                return;
            }

            if (type == typeof(Decimal))
            {
                //TODO: review here
                //.net decimal is larger than double?
                output->Type = JsValueType.Number;
                output->Num = (double)(Decimal)obj;
                return;
            }

            if (type == typeof(DateTime))
            {
                output->Type = JsValueType.Date;
                output->Num = Convert.ToInt64(((DateTime)obj).Subtract(EPOCH).TotalMilliseconds); /*(((DateTime)obj).Ticks - 621355968000000000.0 + 26748000000000.0)/10000.0*/
                return;
            }
            // Arrays of anything that can be cast to object[] are recursively convertef after
            // allocating an appropriate jsvalue on the unmanaged side.

            var array = obj as object[];
            if (array != null)
            {
                //alloc space for array
                int arrLen = array.Length;
                JsContext.jsvalue_alloc_array(arrLen, output);

                if (output->I32 != arrLen)
                    throw new JsInteropException("can't allocate memory on the unmanaged side");
                //

                output->Type = JsValueType.Array;
                unsafe
                {
                    JsValue* arr = (JsValue*)output->Ptr;
                    for (int i = 0; i < arrLen; i++)
                    {
                        AnyToJsValuePtr(array[i], arr + i);
                    }
                }

                return;
            }

            // Every object explicitly converted to a value becomes an entry of the
            // _keepalives list, to make sure the GC won't collect it while still in
            // use by the unmanaged Javascript engine. We don't try to track duplicates
            // because adding the same object more than one time acts more or less as
            // reference counting.  
            //check 

            JsTypeDefinition jsTypeDefinition = _context.GetJsTypeDefinition(type);
            INativeRef prox2 = _context.CreateWrapper(obj, jsTypeDefinition);
            //
            output->Type = JsValueType.JsTypeWrap;
            output->Ptr = prox2.UnmanagedPtr;
            output->I32 = prox2.ManagedIndex;
        }
    }
}
