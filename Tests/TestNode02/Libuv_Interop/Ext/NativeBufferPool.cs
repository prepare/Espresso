//MIT, 2016-2017, EngineKit
//modified from https://github.com/dotnet/corefxlab

using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Net.Libuv.Internal
{
    class NativeBufferPool : IDisposable
    {
        int eachSlotSizeInByte;//native byte buffer size 
        int slotNum = 1024;
        IntPtr largeUnmanagedMem;
        Stack<Span<byte>> memslots;

        //this version we need lock obj
        object lockObj = new object();

        public NativeBufferPool(int eachSlotSizeInByte)
        {

            this.eachSlotSizeInByte = eachSlotSizeInByte;
            this.largeUnmanagedMem = System.Runtime.InteropServices.Marshal.AllocHGlobal(eachSlotSizeInByte * (slotNum + 1));
            memslots = new Stack<System.Span<byte>>(slotNum);
            int j = slotNum;
            unsafe
            {
                byte* nativeMem = (byte*)largeUnmanagedMem.ToPointer();
                for (int i = 0; i < slotNum; ++i)
                {
                    byte* p = nativeMem + (i * eachSlotSizeInByte);
                    memslots.Push(new Span<byte>(p, eachSlotSizeInByte));
                }
            }
        }
        public Span<byte> Rent(int size)
        {
            //request from pool
            unsafe
            {
                Span<byte> buffer;
                if (size > eachSlotSizeInByte)
                {
#if DEBUG
                    System.Diagnostics.Debugger.Break();
#endif
                    //alloc new one ***
                    buffer = new Span<byte>(Marshal.AllocHGlobal(size * Marshal.SizeOf(typeof(byte))).ToPointer(), size, true);
                    return buffer;
                }
                else
                {
                    //use exit one
                    //beware multithread problem
                    //find free mem slot
                    if (memslots.Count > 0)
                    {
                        //TODO: review here, about lock, no lock
                        lock (lockObj)
                        {
                            return memslots.Pop();
                        }
                    }
                    else
                    {
#if DEBUG
                        System.Diagnostics.Debugger.Break();
#endif
                        buffer = new Span<byte>(Marshal.AllocHGlobal(size * Marshal.SizeOf(typeof(byte))).ToPointer(), size, true);
                        return buffer;

                    }
                }
            }
        }
        public void Return(Span<byte> span)
        {
            //free byte buffer to pool 
            if (span.FromFreeSpace)
            {
                Marshal.FreeHGlobal(span.UnsafePointer);
            }
            else
            {
                //TODO: review here, about lock, no lock
                //push back to memslots
                lock (lockObj)
                {
                    memslots.Push(span);
                }
            }
        }
        public void Dispose()
        {
            if (largeUnmanagedMem != IntPtr.Zero)
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(largeUnmanagedMem);
                largeUnmanagedMem = IntPtr.Zero;
            }
        }
    }
}
