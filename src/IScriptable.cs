//MIT, 2015-present, WinterDev, EngineKit, brezza92
using System;
namespace Espresso
{
    public interface INativeRef
    {
        int ManagedIndex { get; }
        object WrapObject { get; }
        bool HasNativeSide { get; }
        void SetUnmanagedPtr(IntPtr unmanagedObjectPtr);
        IntPtr UnmanagedPtr { get; }
    }
    public interface INativeScriptable : INativeRef
    {
        IntPtr UnmanagedTypeDefinitionPtr { get; }
    }

}
