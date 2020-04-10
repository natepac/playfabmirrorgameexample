// incoming message queue of <connectionId, message>
// (not a HashSet because one connection can have multiple new messages)
// -> a struct to minimize GC
using System;

namespace Apathy
{
    public struct Message
    {
        public readonly int connectionId;
        public readonly EventType eventType;

        // the data ArraySegment is only valid until the next Update call
        public readonly ArraySegment<byte> data;

        public Message(int connectionId, EventType eventType, ArraySegment<byte> data)
        {
            this.connectionId = connectionId;
            this.eventType = eventType;
            this.data = data;
        }
    }
}