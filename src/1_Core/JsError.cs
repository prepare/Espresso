//MIT, 2015-present, WinterDev, EngineKit, brezza92
//MIT, 2013, Federico Di Gregorio <fog@initd.org>

using System;
namespace Espresso
{

    struct JsError
    {
        public int line;
        public int column;
        public IntPtr type;
        public IntPtr resource;
        public IntPtr message;
        public IntPtr exception;
    }
}
