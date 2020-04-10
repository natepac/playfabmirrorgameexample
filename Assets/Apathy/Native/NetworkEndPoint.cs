using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Apathy
{
    /// <summary>
    /// NetworkFamily indicates what type of underlying medium we are using.
    /// </summary>
    public enum NetworkFamily
    {
        AF_INET = 2,
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_WSA_10_0
        // windows:
        //   https://docs.microsoft.com/en-us/windows/win32/api/winsock2/nf-winsock2-socket
        AF_INET6 = 23
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS
        // mac:
        //   https://gist.github.com/cyberroadie/3490843
        AF_INET6 = 30
#else
        // linux:
        //   https://github.com/torvalds/linux/blob/master/include/linux/socket.h
        AF_INET6 = 10
#endif
    }

    /// <summary>
    /// The NetworkEndPoint is our representation of the <see cref="System.Net.IPEndPoint"/> type.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct NetworkEndPoint
    {
        internal const int Length = 28;
        [FieldOffset(0)] internal fixed byte data[Length];
        [FieldOffset(0)] internal sa_family_t family;
        [FieldOffset(2)] internal ushort nbo_port;
        [FieldOffset(4)] internal int ipc_handle;
        [FieldOffset(28)] public int length;

        public ushort Port
        {
            // convert port to network byte order (on systems where it's needed)
            get { return (ushort)IPAddress.NetworkToHostOrder((short)nbo_port); }
            set { nbo_port = (ushort)IPAddress.HostToNetworkOrder((short)value); }
        }
        public string Ip
        {
            get
            {
                // CreateIpv4 creates a sockaddr_in and copies data to
                // NetworkEndPoint. so let's get it back.
                fixed (byte* bytes = data)
                {
                    // IPv4
                    if (length == sizeof(sockaddr_in))
                    {
                        // copy data bytes into a sockaddr_in struct
                        sockaddr_in sai = new sockaddr_in();
                        UnsafeUtility.MemCpy(sai.data, bytes, sizeof(sockaddr_in));
                        uint ipInt = sai.sin_addr.s_addr;

                        // convert to byte[]
                        byte[] array = BitConverter.GetBytes(ipInt);

                        // convert to string
                        // note: IPAddress uses network byte order. no need to
                        //       manually convert anything.
                        return new IPAddress(array).ToString();
                    }
                    // IPv6
                    else if (length == sizeof(sockaddr_in6))
                    {
                        // copy data bytes into a sockaddr_in6 struct
                        sockaddr_in6 sai = new sockaddr_in6();
                        UnsafeUtility.MemCpy(sai.data, bytes, sizeof(sockaddr_in6));

                        // copy the ip bytes out of the struct to an array
                        byte[] array = new byte[16];
                        fixed (byte* buffer = array)
                            UnsafeUtility.MemCpy(buffer, sai.sin6_addr.s6_addr, 16);

                        // convert to string
                        // note: IPAddress uses network byte order. no need to
                        //       manually convert anything.
                        return new IPAddress(array).ToString();
                    }
                }
                return "";
            }
        }

        public NetworkFamily Family
        {
            get => (NetworkFamily) family.sa_family;
#if (UNITY_EDITOR_OSX || ((UNITY_STANDALONE_OSX || UNITY_IOS) && !UNITY_EDITOR))
            set => family.sa_family = (byte) value;
#else
            set => family.sa_family = (ushort)value;
#endif
        }

        public bool IsValid => Family != 0;

        // ip byte[] is in network byte order from IPAddress.GetAddressBytes!
        public static NetworkEndPoint CreateIPv4(byte[] ip, ushort port)
        {
            // convert port to network byte order (on systems where it's needed)
            port = (ushort)IPAddress.HostToNetworkOrder((short)port);

            // fill sockaddr_in struct
            sockaddr_in sai = new sockaddr_in();
#if (UNITY_EDITOR_OSX || ((UNITY_STANDALONE_OSX || UNITY_IOS) && !UNITY_EDITOR))
            sai.sin_family.sa_family = (byte)NetworkFamily.AF_INET;
            sai.sin_family.sa_len = (byte)sizeof(sockaddr_in);
#else
            sai.sin_family.sa_family = (ushort)NetworkFamily.AF_INET;
#endif
            sai.sin_port = port;
            sai.sin_addr.s_addr = BitConverter.ToUInt32(ip, 0);

            // copy sockaddr_in into NetworkEndpoint data bytes
            int len = sizeof(sockaddr_in);
            NetworkEndPoint address = new NetworkEndPoint
            {
                length = len
            };

            UnsafeUtility.MemCpy(address.data, sai.data, len);
            return address;
        }

        // ip byte[] is in network byte order from IPAddress.GetAddressBytes!
        public static NetworkEndPoint CreateIPv6(byte[] ip, ushort port)
        {
            // convert port to network byte order (on systems where it's needed)
            port = (ushort)IPAddress.HostToNetworkOrder((short)port);

            // fill sockaddr_in6 struct
            sockaddr_in6 sai = new sockaddr_in6();
#if (UNITY_EDITOR_OSX || ((UNITY_STANDALONE_OSX || UNITY_IOS) && !UNITY_EDITOR))
            sai.sin6_family.sa_family = (byte)NetworkFamily.AF_INET6;
            sai.sin6_family.sa_len = (byte)sizeof(sockaddr_in6);
#else
            sai.sin6_family.sa_family = (ushort)NetworkFamily.AF_INET6;
#endif
            sai.sin6_port = port;
            fixed (byte* buf = ip)
            {
                UnsafeUtility.MemCpy(sai.sin6_addr.s6_addr, buf, 16);
            }

            // copy sockaddr_in6 into NetworkEndpoint data bytes
            int len = sizeof(sockaddr_in6);
            NetworkEndPoint address = new NetworkEndPoint
            {
                length = len
            };

            UnsafeUtility.MemCpy(address.data, sai.data, len);
            return address;
        }

        // returns true if we can fully parse the input and return a valid endpoint
        public static bool TryParse(string ip, ushort port, out NetworkEndPoint endpoint)
        {
            endpoint = default(NetworkEndPoint);

            try
            {
                // note: IPAddress uses network byte order. no need to manually
                // convert to big/little endian.
                IPAddress address = IPAddress.Parse(ip);
                byte[] bytes = address.GetAddressBytes();

                // IPv4
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    endpoint = CreateIPv4(bytes, port);
                    return endpoint.IsValid;
                }
                // IPv6
                else if (address.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    endpoint = CreateIPv6(bytes, port);
                    return endpoint.IsValid;
                }
                // unknown format
                else
                {
                    Debug.LogError("Failed to parse IP: " + ip);
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool operator ==(NetworkEndPoint lhs, NetworkEndPoint rhs)
        {
            return lhs.Compare(rhs);
        }

        public static bool operator !=(NetworkEndPoint lhs, NetworkEndPoint rhs)
        {
            return !lhs.Compare(rhs);
        }

        public override bool Equals(object other)
        {
            return this == (NetworkEndPoint)other;
        }

        public override int GetHashCode()
        {
            fixed (byte* buffer = data)
            {
                unchecked
                {
                    int result = 0;
                    for (int i = 0; i < Length; i++)
                    {
                        result = (result * 31) ^ (int)(IntPtr) (buffer + 1);
                    }
                    return result;
                }
            }
        }

        bool Compare(NetworkEndPoint other)
        {
            if (length != other.length)
                return false;

            fixed (void* buffer = data)
            {
                if (UnsafeUtility.MemCmp(buffer, other.data, length) == 0)
                    return true;
            }

            return false;
        }
    }
}