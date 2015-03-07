//2015, MIT ,WinterDev

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;

using VroomJs;

namespace NativeV8
{
    

    public abstract class JsTypeDefinitionBuilderBase
    {
        internal JsTypeDefinition BuildTypeDefinition(Type t)
        {
            return this.OnBuildRequest(t);
        }
        protected abstract JsTypeDefinition OnBuildRequest(Type t); 
    }

}