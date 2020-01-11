//MIT, https://github.com/dotnet/corefxlab
using System.Runtime.InteropServices;

namespace System.Net.Libuv.Internal
{
    // This is roughly based on LibuvSharp
    public static class UVInterop
    {
        const string LIBUV = "node";
        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr uv_default_loop();

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_loop_init(IntPtr handle);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_loop_close(IntPtr ptr);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr uv_loop_size();

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern void uv_run(IntPtr loop, uv_run_mode mode);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern void uv_stop(IntPtr loop);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_loop_alive(IntPtr loop);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_is_closing(IntPtr handle);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_handle_size(UVHandle.HandleType type);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern void uv_close(IntPtr handle, close_callback cb);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public extern static int uv_ip4_addr(string ip, int port, out sockaddr_in address);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public extern static int uv_ip6_addr(string ip, int port, out sockaddr_in6 address);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_listen(IntPtr stream, int backlog, handle_callback callback);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_accept(IntPtr server, IntPtr client);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern sbyte* uv_strerror(int systemErrorCode);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern sbyte* uv_err_name(int systemErrorCode);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_req_size(RequestType type);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_tcp_bind(IntPtr handle, ref sockaddr_in sockaddr, uint flags);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_tcp_bind(IntPtr handle, ref sockaddr_in6 sockaddr, uint flags);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_tcp_init(IntPtr loop, IntPtr handle);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_tcp_nodelay(IntPtr handle, int enable);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_tcp_keepalive(IntPtr handle, int enable, int delay);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_read_start(IntPtr stream, alloc_callback_unix alloc_callback, read_callback_unix read_callback);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_read_start(IntPtr stream, alloc_callback_win alloc_callback, read_callback_win read_callback);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static int uv_try_write(IntPtr handle, UVBuffer.Windows* buffersList, int bufferCount);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static int uv_try_write(IntPtr handle, UVBuffer.Unix* buffersList, int bufferCount);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_shutdown(IntPtr req, IntPtr handle, handle_callback callback);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_read_watcher_start(IntPtr stream, Action<IntPtr> read_watcher_callback);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_read_stop(IntPtr stream);

        [DllImport(LIBUV, EntryPoint = "uv_write", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int uv_write_unix(IntPtr req, IntPtr handle, UVBuffer.Unix* bufferList, int bufferCount, handle_callback callback);

        [DllImport(LIBUV, EntryPoint = "uv_write", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int uv_write_win(IntPtr req, IntPtr handle, UVBuffer.Windows* bufferList, int bufferCount, handle_callback callback);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_is_active(IntPtr handle);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern UVHandle.HandleType uv_guess_handle(int fd);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern void uv_ref(IntPtr handle);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern void uv_unref(IntPtr handle);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_has_ref(IntPtr handle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void read_callback_unix(IntPtr stream, IntPtr size, ref UVBuffer.Unix buffer);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void read_callback_win(IntPtr stream, IntPtr size, ref UVBuffer.Windows buffer);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void alloc_callback_unix(IntPtr data, uint size, out UVBuffer.Unix buffer);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void alloc_callback_win(IntPtr data, uint size, out UVBuffer.Windows buffer);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void close_callback(IntPtr handle);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void handle_callback(IntPtr req, int status);

        //========================================================================
        //timer
        //========================================================================
        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_timer_init(IntPtr loop, IntPtr timer);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_timer_start(IntPtr timer, handle_callback callback, ulong timeout, ulong repeat);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_timer_stop(IntPtr timer);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_timer_again(IntPtr timer);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern void uv_timer_set_repeat(IntPtr timer, ulong repeat);

        [DllImport(LIBUV, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong uv_timer_get_repeat(IntPtr timer);
        //---------------------------------------------------------------


    }

    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public struct sockaddr_in
    {
        public int a, b, c, d;
    }

    [StructLayout(LayoutKind.Sequential, Size = 28)]
    public struct sockaddr_in6
    {
        public int a, b, c, d, e, f, g;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct uv_handle_t
    {
        public IntPtr data;
        public IntPtr loop;
        public UVHandle.HandleType type;
        public IntPtr close_cb;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct uv_stream_t
    {
        public IntPtr write_queue_size;
        public IntPtr alloc_cb;
        public IntPtr read_cb;
        public IntPtr read2_cb;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct uv_req_t
    {
        public IntPtr data;
        public RequestType type;
    }

    public enum RequestType : int
    {
        UV_UNKNOWN_REQ = 0,
        UV_REQ,
        UV_CONNECT,
        UV_WRITE,
        UV_SHUTDOWN,
        UV_UDP_SEND,
        UV_FS,
        UV_WORK,
        UV_GETADDRINFO,
        UV_GETNAMEINFO,
        UV_REQ_TYPE_PRIVATE,
        UV_REQ_TYPE_MAX,
    }

    public enum uv_run_mode : int
    {
        UV_RUN_DEFAULT = 0,
        UV_RUN_ONCE,
        UV_RUN_NOWAIT
    };
}
