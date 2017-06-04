//MIT, 2015-2017, WinterDev, EngineKit, brezza92

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
    class JsConvert2
    {
        public static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static readonly DateTime EPOCH_LocalTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        static JsConvert2()
        {

        }
        public JsConvert2(JsContext context)
        {
            _context = context;
        }

        readonly JsContext _context;


        public object FromJsValue(ref JsInterOpValue v)
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
                    return EPOCH_LocalTime.AddMilliseconds(v.I64);// + new TimeSpan(7, 0, 0);
                //return EPOCH_LocalTime.AddMilliseconds(v.I64);// + new TimeSpan(7, 0, 0);
                //return EPOCH.AddMilliseconds(v.I64);

                //return EPOCH.AddMilliseconds(v.Num);
                //return new DateTime((long)(v.Num * 10000) + 621355968000000000L - 26748000000000L);
                case JsValueType.Array:
                    {
                        throw new NotSupportedException();
                        //int len = v.Length;
                        //var r = new object[len];
                        //for (int i = 0; i < len; i++)
                        //{
                        //    var vi = (JsValue)Marshal.PtrToStructure(new IntPtr(v.Ptr.ToInt64() + (16 * i)), typeof(JsValue));
                        //    r[i] = FromJsValue(vi);
                        //}
                        //return r;
                    }

                case JsValueType.UnknownError:
                    if (v.Ptr != IntPtr.Zero)
                        return new JsException(Marshal.PtrToStringUni(v.Ptr));
                    return new JsInteropException("unknown error without reason");

                case JsValueType.StringError:
                    return new JsException(Marshal.PtrToStringUni(v.Ptr));

                case JsValueType.Managed:
                    return _context.KeepAliveGet(v.Index);
                case JsValueType.JsTypeWrap:
                    //auto unwrap
                    return this._context.GetObjectProxy(v.Index).WrapObject;
                case JsValueType.ManagedError:
                    Exception inner = _context.KeepAliveGet(v.Index) as Exception;
                    string msg = null;
                    if (v.Ptr != IntPtr.Zero)
                    {
                        msg = Marshal.PtrToStringUni(v.Ptr);
                    }
                    else
                    {
                        if (inner != null)
                        {
                            msg = inner.Message;
                        }
                    }
                    return new JsException(msg, inner);
#if NET40
                case JsValueType.Wrapped:
                    return new JsObject(_context, v.Ptr);
#else
                case JsValueType.Dictionary:
                    return JsDictionaryObject(ref v);
#endif
                case JsValueType.Wrapped:
                    return new JsObject(_context, v.Ptr);

                case JsValueType.Error:

                    throw new NotSupportedException();
                //return JsException.Create(this, (JsError)Marshal.PtrToStructure(v.Ptr, typeof(JsError)));

                case JsValueType.Function:
                    throw new NotSupportedException();
                //var fa = new JsValue[2];                    
                //for (int i = 0; i < 2; i++)
                //{
                //    fa[i] = (JsValue)Marshal.PtrToStructure(new IntPtr(v.Ptr.ToInt64() + (16 * i)), typeof(JsValue));
                //}
                //return new JsFunction(_context, fa[0].Ptr, fa[1].Ptr);
                default:
                    throw new InvalidOperationException("unknown type code: " + v.Type);
            }
        }
#if !NET40
        private JsObject JsDictionaryObject(ref JsInterOpValue v)
        {
            throw new NotSupportedException();
            //JsObject obj = new JsObject(this._context, v.Ptr);
            //int len = v.Length * 2;
            //for (int i = 0; i < len; i += 2)
            //{
            //    var key = (JsValue)Marshal.PtrToStructure(new IntPtr(v.Ptr.ToInt64() + (16 * i)), typeof(JsValue));
            //    var value = (JsValue)Marshal.PtrToStructure(new IntPtr(v.Ptr.ToInt64() + (16 * (i + 1))), typeof(JsValue));
            //    obj[(string)FromJsValue(key)] = FromJsValue(value);
            //}
            //return obj;
        }
#endif


        public void ToJsValue(int value, ref JsInterOpValue output)
        {
            output.Type = JsValueType.Integer;
            output.I32 = value;
        }
        public void ToJsValue(long value, ref JsInterOpValue output)
        {
            //TODO review here
            output.Type = JsValueType.Number;
            output.I64 = value;
        }
        public void ToJsValue(string value, ref JsInterOpValue output)
        {
            // We need to allocate some memory on the other side; will be free'd by unmanaged code.            
            JsContext.jsvalue_alloc_string(value, ref output);
            output.Type = JsValueType.String;
        }
        public void ToJsValue(char c, ref JsInterOpValue output)
        {
            //TODO: review here
            // We need to allocate some memory on the other side; will be free'd by unmanaged code.            
            JsContext.jsvalue_alloc_string(c.ToString(), ref output);
            output.Type = JsValueType.String;
        }
        public void ToJsValue(double value, ref JsInterOpValue output)
        {
            output.Type = JsValueType.Number;
            output.Num = value;
        }
        public void ToJsValue(float value, ref JsInterOpValue output)
        {
            output.Type = JsValueType.Number;
            output.Num = value;
        }
        public void ToJsValue(decimal value, ref JsInterOpValue output)
        {
            //data loss***
            //TODO: review here, convert to string?
            output.Type = JsValueType.Number;
            output.Num = (double)value;
        }
        public void ToJsValue(bool value, ref JsInterOpValue output)
        {
            output.Type = JsValueType.Boolean;
            output.I32 = value ? 1 : 0;
        }
        public void ToJsValue(DateTime dtm, ref JsInterOpValue output)
        {
            output.Type = JsValueType.Date;
            output.Num = Convert.ToInt64(dtm.Subtract(EPOCH).TotalMilliseconds);

            //return new JsValue
            //{
            //    Type = JsValueType.Date,
            //    Num = Convert.ToInt64(dtm.Subtract(EPOCH).TotalMilliseconds)
            //    /*(((DateTime)obj).Ticks - 621355968000000000.0 + 26748000000000.0)/10000.0*/
            //};
        }

        public void ToJsValue(INativeScriptable jsInstance, ref JsInterOpValue output)
        {
            //extension 
            //int keepAliveId = _context.KeepAliveAdd(jsInstance);

            output.Type = JsValueType.JsTypeWrap;
            output.Ptr = jsInstance.UnmanagedPtr;
            output.Index = jsInstance.ManagedIndex;

            //return new JsValue
            //{
            //    Type = JsValueType.JsTypeWrap,
            //    Ptr = jsInstance.UnmanagedPtr,
            //    Index = jsInstance.ManagedIndex
            //    //Index = keepAliveId jsInstance.ManagedIndex
            //};

        }
        public void ToJsValue(object[] arr, ref JsInterOpValue output)
        {
            int len = arr.Length;
            JsContext.jsvalue_alloc_array(len, ref output);

            if (output.Length != len)
            {
                throw new JsInteropException("can't allocate memory on the unmanaged side");
            }
            throw new NotSupportedException();
            for (int i = 0; i < len; i++)
            {
                //Marshal.StructureToPtr(AnyToJsValue(arr[i]), new IntPtr(v.Ptr.ToInt64() + (16 * i)), false);

            }
        }

        public void ToJsValueNull(ref JsInterOpValue output)
        {
            output.Type = JsValueType.Null;
        }

        public void AnyToJsValue(object obj, ref JsInterOpValue output)
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
                int keepAliveId = _context.KeepAliveAdd(obj);
                output.Type = JsValueType.JsTypeWrap;
                output.Ptr = prox.UnmanagedPtr;
                output.Index = keepAliveId;
                return;
            }
            //-----
            Type type = obj.GetType();

            // Check for nullable types (we will cast the value out of the box later).

          
            type = type.ExtGetInnerTypeIfNullableValue();


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
                JsContext.jsvalue_alloc_string(obj.ToString(), ref output);
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
                JsContext.jsvalue_alloc_array(arrLen, ref output);
                if (output.Length != arrLen)
                    throw new JsInteropException("can't allocate memory on the unmanaged side");


                for (int i = 0; i < arrLen; i++)
                {
                    JsInterOpValue eachElem = new JsInterOpValue();
                    AnyToJsValue(array[i], ref eachElem);
                    //store each elem to native array
                    //convert each element to array element-> recursive
                    //Marshal.StructureToPtr(
                    //    AnyToJsValue(array[i]), new IntPtr(v.Ptr.ToInt64() + (16 * i)), false);
                }
                return;
            }

            // Every object explicitly converted to a value becomes an entry of the
            // _keepalives list, to make sure the GC won't collect it while still in
            // use by the unmanaged Javascript engine. We don't try to track duplicates
            // because adding the same object more than one time acts more or less as
            // reference counting.  
            //check 

            var jsTypeDefinition = _context.GetJsTypeDefinition(type);
            INativeRef prox2 = _context.CreateWrapper(obj, jsTypeDefinition);
            //
            output.Type = JsValueType.JsTypeWrap;
            output.Ptr = prox2.UnmanagedPtr;
            output.Index = prox2.ManagedIndex;

        }

        //public JsValue AnyToJsValue(object obj)
        //{
        //    if (obj == null)
        //        return new JsValue { Type = JsValueType.Null };

        //    if (obj is INativeRef)
        //    {
        //        //extension
        //        INativeRef prox = (INativeRef)obj;
        //        int keepAliveId = _context.KeepAliveAdd(obj);
        //        return new JsValue { Type = JsValueType.JsTypeWrap, Ptr = prox.UnmanagedPtr, Index = keepAliveId };
        //    }

        //    Type type = obj.GetType();

        //    // Check for nullable types (we will cast the value out of the box later).
        //    //if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        //    if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        //        type = type.GenericTypeArguments[0]; //type = type.GetGenericArguments()[0];

        //    if (type == typeof(Boolean))
        //        return new JsValue { Type = JsValueType.Boolean, I32 = (bool)obj ? 1 : 0 };

        //    if (type == typeof(String) || type == typeof(Char))
        //    {
        //        // We need to allocate some memory on the other side; will be free'd by unmanaged code.
        //        return JsContext.jsvalue_alloc_string(obj.ToString());
        //    }

        //    if (type == typeof(Byte))
        //        return new JsValue { Type = JsValueType.Integer, I32 = (int)(Byte)obj };
        //    if (type == typeof(Int16))
        //        return new JsValue { Type = JsValueType.Integer, I32 = (int)(Int16)obj };
        //    if (type == typeof(UInt16))
        //        return new JsValue { Type = JsValueType.Integer, I32 = (int)(UInt16)obj };
        //    if (type == typeof(Int32))
        //        return new JsValue { Type = JsValueType.Integer, I32 = (int)obj };
        //    if (type == typeof(UInt32))
        //        return new JsValue { Type = JsValueType.Integer, I32 = (int)(UInt32)obj };

        //    if (type == typeof(Int64))
        //        return new JsValue { Type = JsValueType.Number, Num = (double)(Int64)obj };
        //    if (type == typeof(UInt64))
        //        return new JsValue { Type = JsValueType.Number, Num = (double)(UInt64)obj };
        //    if (type == typeof(Single))
        //        return new JsValue { Type = JsValueType.Number, Num = (double)(Single)obj };
        //    if (type == typeof(Double))
        //        return new JsValue { Type = JsValueType.Number, Num = (double)obj };
        //    if (type == typeof(Decimal))
        //        return new JsValue { Type = JsValueType.Number, Num = (double)(Decimal)obj };

        //    if (type == typeof(DateTime))
        //        return new JsValue
        //        {
        //            Type = JsValueType.Date,
        //            Num = Convert.ToInt64(((DateTime)obj).Subtract(EPOCH).TotalMilliseconds) /*(((DateTime)obj).Ticks - 621355968000000000.0 + 26748000000000.0)/10000.0*/
        //        };

        //    // Arrays of anything that can be cast to object[] are recursively convertef after
        //    // allocating an appropriate jsvalue on the unmanaged side.

        //    var array = obj as object[];
        //    if (array != null)
        //    {
        //        JsValue v = JsContext.jsvalue_alloc_array(array.Length);
        //        if (v.Length != array.Length)
        //            throw new JsInteropException("can't allocate memory on the unmanaged side");
        //        for (int i = 0; i < array.Length; i++)
        //            Marshal.StructureToPtr(AnyToJsValue(array[i]), new IntPtr(v.Ptr.ToInt64() + (16 * i)), false);
        //        return v;
        //    }

        //    // Every object explicitly converted to a value becomes an entry of the
        //    // _keepalives list, to make sure the GC won't collect it while still in
        //    // use by the unmanaged Javascript engine. We don't try to track duplicates
        //    // because adding the same object more than one time acts more or less as
        //    // reference counting.  
        //    //check 

        //    var jsTypeDefinition = _context.GetJsTypeDefinition(type);
        //    INativeRef prox2 = _context.CreateWrapper(obj, jsTypeDefinition);
        //    //int keepAliveId2 = _context.KeepAliveAdd(prox2);
        //    return new JsValue { Type = JsValueType.JsTypeWrap, Ptr = prox2.UnmanagedPtr, Index = prox2.ManagedIndex };
        //    //return new JsValue { Type = JsValueType.JsTypeWrap, Ptr = prox2.UnmanagedPtr, Index = keepAliveId2 };

        //    //return new JsValue { Type = JsValueType.Managed, Index = _context.KeepAliveAdd(obj) };
        //}

    }
}
