// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
namespace System.Runtime.InteropServices
{

    using System;

    [System.Runtime.InteropServices.ComVisible(true)]
    public struct HandleRef
    {

        // ! Do not add or rearrange fields as the EE depends on this layout.
        //------------------------------------------------------------------
        internal Object _wrapper;
        internal IntPtr _handle;
        //------------------------------------------------------------------

        public HandleRef(Object wrapper, IntPtr handle)
        {
            _wrapper = wrapper;
            _handle = handle;
        }

        public Object Wrapper
        {
            get
            {
                return _wrapper;
            }
        }

        public IntPtr Handle
        {
            get
            {
                return _handle;
            }
        }


        public static explicit operator IntPtr(HandleRef value)
        {
            return value._handle;
        }

        public static IntPtr ToIntPtr(HandleRef value)
        {
            return value._handle;
        }
    }
}