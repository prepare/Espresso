//MIT, 2015-2017, WinterDev, EngineKit, brezza92
//MIT, 2013, Federico Di Gregorio <fog@initd.org>

using System;
namespace Espresso
{

    struct JsError
    {   
        readonly IntPtr nativeError;
        public JsError(IntPtr nativeError)
        {
            this.nativeError = nativeError;
        }
    }
}
