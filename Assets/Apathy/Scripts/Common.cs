using System;
using UnityEngine;

namespace Apathy
{
    public abstract class Common
    {
        // we use large buffers instead of threads and queues (= TCP for games)
        // 7MB per client should be okay. if the send buffer gets full and we
        // cached 7MB of packets then it's time to drop the connection.
        // (7MB is the maximum on OSX before ENOBUFS errors appear)
        public int ReceiveBufferSize = 1024 * 1024 * 7;
        public int SendBufferSize = 1024 * 1024 * 7;

        // NoDelay disables nagle algorithm. lowers CPU% and latency but
        // increases bandwidth
        public bool NoDelay = false;

        // cache buffers to avoid allocations
        // -> header: 4 bytes for 1 integer
        protected byte[] headerBuffer = new byte[4];
        // -> payload: MaxMessageSize + header size
        protected byte[] payloadBuffer;

        // Prevent allocation attacks. Each packet is prefixed with a length
        // header, so an attacker could send a fake packet with length=2GB,
        // causing the server to allocate 2GB and run out of memory quickly.
        // -> simply increase max packet size if you want to send around bigger
        //    files!
        // -> 16KB per message should be more than enough.
        public int MaxMessageSize = 16 * 1024;

        // Client tick rate is often higher than server tick rate, especially if
        // server is under heavy load or limited to 20Hz or similar. Server
        // needs to process 'a few' messages per tick per connection. Processing
        // only one per tick can cause an ever growing backlog, hence ever
        // growing latency on the client. Set this to a reasonable amount, but
        // not too big so that the server never deadlocks reading too many
        // messages per tick (which would be way worse than one of the clients
        // having high latency.
        // -> see GetNextMessages for a more in-depth explanation!
        //
        // note: changes at runtime don't have any effect!
        public int MaxReceivesPerTickPerConnection = 10;

        // disconnect detection
        protected static bool WasDisconnected(long socket)
        {
            // check if any recent errors
            if (NativeBindings.network_get_error(socket) != 0)
                return true;

            // try our disconnected method
            if (NativeBindings.network_disconnected(socket) == 1)
                return true;

            // doesn't look like a disconnect
            return false;
        }

        protected void ConfigureSocket(long socket)
        {
            int error = 0;

            if (NativeBindings.network_set_nonblocking(socket, ref error) != 0)
                Debug.LogError("network_set_nonblocking failed: " + (NativeError)error);

            if (NativeBindings.network_set_send_buffer_size(socket, SendBufferSize, ref error) != 0)
                Debug.LogError("network_set_send_buffer_size failed: " + (NativeError)error);

            if (NativeBindings.network_set_receive_buffer_size(socket, ReceiveBufferSize, ref error) != 0)
                Debug.LogError("network_set_receive_buffer_size failed: " + (NativeError)error);

            if (NativeBindings.network_set_nodelay(socket, NoDelay ? 1 : 0, ref error) != 0)
                Debug.LogError("network_set_nodelay failed: " + (NativeError)error);

            // enable TCP_KEEPALIVE to detect closed connections / wires
            if (NativeBindings.network_set_keepalive(socket, 1, ref error) != 0)
                Debug.LogError("network_set_keepalive failed: " + (NativeError)error);
        }

        // read exactly 'size' bytes if (and only if) available
        protected static unsafe bool ReadIfAvailable(long socket, int size, byte[] buffer)
        {
            // check how much is available
            int error = 0;
            int available = NativeBindings.network_available(socket, ref error);
            if (available >= size)
            {
                if (size <= buffer.Length)
                {
                    // need to pin memory before passing to C
                    // (https://stackoverflow.com/questions/46527470/pass-byte-array-from-unity-c-sharp-to-c-plugin)
                    fixed (void* buf = buffer)
                    {
                        int bytesRead = NativeBindings.network_recv(socket, buf, size, ref error);
                        if (bytesRead > 0)
                        {
                            //Debug.LogWarning("network_recv: avail=" + available + " read=" + bytesRead);
                            return true;
                        }
                        else Debug.LogError("network_recv failed: " + bytesRead + " error=" + (NativeError)error);
                    }
                }
                else Debug.LogError("ReadIfAvailable: buffer(" + buffer.Length + ") too small for " + size + " bytes");
            }
            else if (available == -1)
            {
                Debug.LogError("network_available failed for socket:" + socket + " error: " + (NativeError)error);
            }
            return false;
        }

        // send bytes if send buffer not full (in which case we should
        // disconnect because the receiving end pretty much timed out)
        protected unsafe bool SendIfNotFull(long socket, ArraySegment<byte> data)
        {
            // construct payload if not constructed yet or MaxSize changed
            // (we do allow changing MaxMessageSize at runtime)
            int payloadSize = MaxMessageSize + headerBuffer.Length;
            if (payloadBuffer == null || payloadBuffer.Length != payloadSize)
            {
                payloadBuffer = new byte[payloadSize];
            }

            // construct header (size)
            Utils.IntToBytesBigEndianNonAlloc(data.Count, headerBuffer);

            // calculate packet size (header + data)
            int packetSize = headerBuffer.Length + data.Count;

            // copy into payload buffer
            // NOTE: we write the full payload at once instead of writing first
            //       header and then data, because this way NODELAY mode is more
            //       efficient by sending the whole message as one packet.
            Array.Copy(headerBuffer, 0, payloadBuffer, 0, headerBuffer.Length);
            Array.Copy(data.Array, data.Offset, payloadBuffer, headerBuffer.Length, data.Count);

            fixed (void* buffer = payloadBuffer)
            {
                //Debug.Log("network_send: " + socketHandle + " payload=" + BitConverter.ToString(payloadBuffer, 0, packetSize));
                int error = 0;
                int sent = NativeBindings.network_send(socket, buffer, packetSize, ref error);
                if (sent < 0)
                {
                    Debug.LogError("network_send failed: socket=" + socket + " error=" + (NativeError)error);
                    return false;
                }
                return true;
            }
        }
    }
}
