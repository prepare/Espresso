//MIT, 2013, Federico Di Gregorio <fog@initd.org>
using System;
using System.Runtime.InteropServices;

namespace Espresso
{
     
    unsafe struct JsError
    {
        //TODO: review this struct again ***
        public int Line;
        public int Column;
        IntPtr nativeErr;

        //public JsInterOpValue* Type; 
        //public JsInterOpValue* Resource;
        //public JsInterOpValue* Message;
        //public JsInterOpValue* Exception;
    }
}
