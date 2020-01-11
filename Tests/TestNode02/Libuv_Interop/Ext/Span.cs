//MIT, 2016-2017, EngineKit 
//modified from https://github.com/dotnet/corefxlab

using System.IO;
namespace System
{  
    public unsafe struct Span<T>
    {
        int len;
        IntPtr addr;
        bool fromFreeSpace;
        internal Span(IntPtr addr, int len)
        {
            this.len = len;
            this.addr = addr;
            fromFreeSpace = false;
        }

        internal Span(byte* addr, int len)
        {
            this.len = len;
            this.addr = new IntPtr((void*)addr);
            fromFreeSpace = false;
        }
        internal Span(void* addr, int len, bool fromFreeSpace)
        {
            this.len = len;
            this.addr = new IntPtr(addr);
            this.fromFreeSpace = fromFreeSpace;
        }
        public int Length
        {
            get { return len; }
        }
        public IntPtr UnsafePointer
        {
            get { return addr; }
        }
        internal bool FromFreeSpace
        {
            get { return fromFreeSpace; }
        }

    }

}
