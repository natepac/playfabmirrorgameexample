using System.Runtime.InteropServices;

namespace Apathy
{
    // TODO: Fix this internally incase there are other platforms that also does
    // it differently so it may result in similar issues
# if (UNITY_EDITOR_OSX || ((UNITY_STANDALONE_OSX || UNITY_IOS) && !UNITY_EDITOR))
    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct sa_family_t
    {
        public const int size = sizeof(byte) * 2;
        [FieldOffset(0)] public byte sa_len;
        [FieldOffset(1)] public byte sa_family;
    }
# else
    internal unsafe struct sa_family_t
    {
        public const int size = sizeof(ushort);
        public ushort sa_family;
    }
# endif

    internal unsafe struct in_addr
    {
        public uint s_addr;
    }

    // IPv4 ////////////////////////////////////////////////////////////////////
    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct sockaddr
    {
        [FieldOffset(0)] public fixed byte data[16];

        [FieldOffset(0)] public sa_family_t sin_family;
        [FieldOffset(2)] public fixed byte sin_zero[14];
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct sockaddr_in
    {
        [FieldOffset(0)] public fixed byte data[16];

        [FieldOffset(0)] public sa_family_t sin_family;
        [FieldOffset(2)] public ushort sin_port;
        [FieldOffset(4)] public in_addr sin_addr;
        [FieldOffset(8)] public fixed byte sin_zero[8];
    }

    // IPv6 ////////////////////////////////////////////////////////////////////
    internal unsafe struct in_addr6
    {
        public fixed byte s6_addr[16];
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct sockaddr_in6
    {
        [FieldOffset(0)] public fixed byte data[28];

        [FieldOffset(0)] public sa_family_t sin6_family;
        [FieldOffset(2)] public ushort sin6_port;
        [FieldOffset(4)] public uint sin6_flowinfo;
        [FieldOffset(8)] public in_addr6 sin6_addr;
        [FieldOffset(24)] public uint sin6_scope_id;
    }

    // bindings ////////////////////////////////////////////////////////////////
    public static unsafe class NativeBindings
    {
#if UNITY_IOS && !UNITY_EDITOR
        const string dllName = "__Internal";
#elif UNITY_EDITOR_WIN && UNITY_2019_2_OR_NEWER
        const string dllName = "network.bindings.dll";
#elif UNITY_EDITOR_OSX && UNITY_2019_2_OR_NEWER
        const string dllName = "network.bindings.bundle";
#else
        const string dllName = "network.bindings";
#endif
        // initialize & terminate //////////////////////////////////////////////
        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int network_initialize();
        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int network_terminate();

        // create & close //////////////////////////////////////////////////////
        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int network_create_socket(ref long sock, ref NetworkEndPoint address, ref int error);
        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int network_close(long sock, ref int error);

        // configuration ///////////////////////////////////////////////////////
        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int network_set_dualmode(long sock, int enabled, ref int error);
        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int network_set_nonblocking(long sock, ref int error);
        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int network_set_send_buffer_size(long sock, int size, ref int error);
        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int network_set_receive_buffer_size(long sock, int size, ref int error);
        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int network_set_nodelay(long sock, int enabled, ref int error);
        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int network_set_keepalive(long sock, int enabled, ref int error);
        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int network_get_send_buffer_size(long sock, ref int size, ref int error);
        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int network_get_receive_buffer_size(long sock, ref int size, ref int error);
        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int network_get_error(long sock);

        // client //////////////////////////////////////////////////////////////
        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int network_connect(long sock, ref NetworkEndPoint address, ref int error);

        // server //////////////////////////////////////////////////////////////
        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int network_bind(long sock, ref NetworkEndPoint address, ref int error);
        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int network_listen(long sock, ref int error);
        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int network_accept(long sock, ref long client_sock, ref NetworkEndPoint client_address, ref int error);
        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int network_get_peer_address(long sock, ref NetworkEndPoint address, ref int error);

        // state ///////////////////////////////////////////////////////////////
        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int network_disconnected(long sock);

        // recv & send /////////////////////////////////////////////////////////
        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int network_available(long sock, ref int error);
        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int network_recv(long sock, void* buffer, int len, ref int error);
        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int network_send(long sock, void* buffer, int len, ref int error);
    }
}
