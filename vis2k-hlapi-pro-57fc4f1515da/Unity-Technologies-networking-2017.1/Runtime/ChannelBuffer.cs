// vis2k:
// * removed free packet caching because UNET is complex enough as it is.
//   note: this was used to avoid 'new ChannelPacket' allocations
// * replaced ChannelPacket with a simple MemoryStream
// * ChannelPacket isn't recreated after changing max size anymore. check is used when writing.
#if ENABLE_UNET
using System;
using System.IO;
using System.Collections.Generic;

namespace UnityEngine.Networking
{
    class ChannelBuffer
    {
        NetworkConnection m_Connection;

        MemoryStream m_CurrentPacket = new MemoryStream(); // vis2k: MemoryStream instead of ChannelPacket. and create directly.

        float m_LastFlushTime;

        byte m_ChannelId;
        int m_MaxPacketSize;
        bool m_IsReliable;
        bool m_AllowFragmentation;
        bool m_IsBroken;
        int m_MaxPendingPacketCount;

        public const int MaxPendingPacketCount = 16;  // this is per connection. each is around 1400 bytes (MTU)
        public const int MaxBufferedPackets = 512;

        Queue<MemoryStream> m_PendingPackets; // vis2k: queue of MemoryStream instead of ChannelPacket
        static internal int pendingPacketCount; // this is across all connections. only used for profiler metrics.

        // config
        public float maxDelay = 0.01f;

        // stats
        float m_LastBufferedMessageCountTimer = Time.realtimeSinceStartup;

        public int numMsgsOut { get; private set; }
        public int numBufferedMsgsOut { get; private set; }
        public int numBytesOut { get; private set; }

        public int numMsgsIn { get; private set; }
        public int numBytesIn { get; private set; }

        public int numBufferedPerSecond { get; private set; }
        public int lastBufferedPerSecond { get; private set; }

        static NetworkWriter s_SendWriter = new NetworkWriter();
        static NetworkWriter s_FragmentWriter = new NetworkWriter();

        // We need to reserve some space for header information, this will be taken off the total channel buffer size
        const int k_PacketHeaderReserveSize = 100;

        public ChannelBuffer(NetworkConnection conn, int bufferSize, byte cid, bool isReliable, bool isSequenced)
        {
            m_Connection = conn;
            m_MaxPacketSize = bufferSize - k_PacketHeaderReserveSize;

            m_ChannelId = cid;
            m_MaxPendingPacketCount = MaxPendingPacketCount;
            m_IsReliable = isReliable;
            m_AllowFragmentation = (isReliable && isSequenced);
            if (isReliable)
            {
                m_PendingPackets = new Queue<MemoryStream>();
            }
        }

        public bool SetOption(ChannelOption option, int value)
        {
            switch (option)
            {
                case ChannelOption.MaxPendingBuffers:
                {
                    if (!m_IsReliable)
                    {
                        // not an error
                        //if (LogFilter.logError) { Debug.LogError("Cannot set MaxPendingBuffers on unreliable channel " + m_ChannelId); }
                        return false;
                    }
                    if (value < 0 || value >= MaxBufferedPackets)
                    {
                        if (LogFilter.logError) { Debug.LogError("Invalid MaxPendingBuffers for channel " + m_ChannelId + ". Must be greater than zero and less than " + MaxBufferedPackets); }
                        return false;
                    }
                    m_MaxPendingPacketCount = value;
                    return true;
                }

                case ChannelOption.AllowFragmentation:
                {
                    m_AllowFragmentation = (value != 0);
                    return true;
                }

                case ChannelOption.MaxPacketSize:
                {
                    if (m_CurrentPacket.Position > 0 || m_PendingPackets.Count > 0)
                    {
                        if (LogFilter.logError) { Debug.LogError("Cannot set MaxPacketSize after sending data."); }
                        return false;
                    }

                    if (value <= 0)
                    {
                        if (LogFilter.logError) { Debug.LogError("Cannot set MaxPacketSize less than one."); }
                        return false;
                    }

                    if (value > m_MaxPacketSize)
                    {
                        if (LogFilter.logError) { Debug.LogError("Cannot set MaxPacketSize to greater than the existing maximum (" + m_MaxPacketSize + ")."); }
                        return false;
                    }
                    m_MaxPacketSize = value;
                    return true;
                }
            }
            return false;
        }

        public void CheckInternalBuffer()
        {
            if (Time.realtimeSinceStartup - m_LastFlushTime > maxDelay && m_CurrentPacket.Position > 0)
            {
                SendInternalBuffer();
                m_LastFlushTime = Time.realtimeSinceStartup;
            }

            if (Time.realtimeSinceStartup - m_LastBufferedMessageCountTimer > 1.0f)
            {
                lastBufferedPerSecond = numBufferedPerSecond;
                numBufferedPerSecond = 0;
                m_LastBufferedMessageCountTimer = Time.realtimeSinceStartup;
            }
        }

        public bool SendWriter(NetworkWriter writer)
        {
            return SendBytes(writer.ToArray(), writer.Position);
        }

        public bool Send(short msgType, MessageBase msg)
        {
            // build the stream
            s_SendWriter.StartMessage(msgType);
            msg.Serialize(s_SendWriter);
            s_SendWriter.FinishMessage();

            numMsgsOut += 1;
            return SendWriter(s_SendWriter);
        }

        // vis2k: replaced NetBuffer with MemoryStream, see NetworkReader/Writer.
        internal MemoryStream fragmentBuffer = new MemoryStream();
        bool readingFragment = false;

        internal bool HandleFragment(NetworkReader reader)
        {
            int state = reader.ReadByte();
            if (state == 0)
            {
                if (readingFragment == false)
                {
                    fragmentBuffer.Seek(0, SeekOrigin.Begin);
                    readingFragment = true;
                }

                byte[] data = reader.ReadBytesAndSize();
                fragmentBuffer.Write(data, 0, data.Length);
                return false;
            }
            else
            {
                readingFragment = false;
                return true;
            }
        }

        internal bool SendFragmentBytes(byte[] bytes, int bytesToSend)
        {
            const int fragmentHeaderSize = 32;
            int pos = 0;
            while (bytesToSend > 0)
            {
                int diff = Math.Min(bytesToSend, m_MaxPacketSize - fragmentHeaderSize);
                byte[] buffer = new byte[diff];
                Array.Copy(bytes, pos, buffer, 0, diff);

                s_FragmentWriter.StartMessage(MsgType.Fragment);
                s_FragmentWriter.Write((byte)0);
                s_FragmentWriter.WriteBytesFull(buffer);
                s_FragmentWriter.FinishMessage();
                SendWriter(s_FragmentWriter);

                pos += diff;
                bytesToSend -= diff;
            }

            // send finish
            s_FragmentWriter.StartMessage(MsgType.Fragment);
            s_FragmentWriter.Write((byte)1);
            s_FragmentWriter.FinishMessage();
            SendWriter(s_FragmentWriter);

            return true;
        }

        internal bool SendBytes(byte[] bytes, int bytesToSend)
        {
#if UNITY_EDITOR
            UnityEditor.NetworkDetailStats.IncrementStat(
                UnityEditor.NetworkDetailStats.NetworkDirection.Outgoing,
                MsgType.HLAPIMsg, "msg", 1);
#endif
            if (bytesToSend >= UInt16.MaxValue)
            {
                if (LogFilter.logError) { Debug.LogError("ChannelBuffer:SendBytes cannot send packet larger than " + UInt16.MaxValue + " bytes"); }
                return false;
            }

            if (bytesToSend <= 0)
            {
                // zero length packets getting into the packet queues are bad.
                if (LogFilter.logError) { Debug.LogError("ChannelBuffer:SendBytes cannot send zero bytes"); }
                return false;
            }

            if (bytesToSend > m_MaxPacketSize)
            {
                if (m_AllowFragmentation)
                {
                    return SendFragmentBytes(bytes, bytesToSend);
                }
                else
                {
                    // cannot do HLAPI fragmentation on this channel
                    if (LogFilter.logError) { Debug.LogError("Failed to send big message of " + bytesToSend + " bytes. The maximum is " + m_MaxPacketSize + " bytes on channel:" + m_ChannelId); }
                    return false;
                }
            }

            // not enough space for bytes to send?
            if (m_CurrentPacket.Position + bytesToSend > m_MaxPacketSize)
            {
                if (m_IsReliable)
                {
                    if (m_PendingPackets.Count == 0)
                    {
                        // nothing in the pending queue yet, just flush and write
                        if (!SendToTransport(m_CurrentPacket, m_IsReliable, m_Connection, m_ChannelId))
                        {
                            QueuePacket();
                        }
                        m_CurrentPacket.Write(bytes, 0, bytesToSend);
                        return true;
                    }

                    if (m_PendingPackets.Count >= m_MaxPendingPacketCount)
                    {
                        if (!m_IsBroken)
                        {
                            // only log this once, or it will spam the log constantly
                            if (LogFilter.logError) { Debug.LogError("ChannelBuffer buffer limit of " + m_PendingPackets.Count + " packets reached."); }
                        }
                        m_IsBroken = true;
                        return false;
                    }

                    // calling SendToTransport here would write out-of-order data to the stream. just queue
                    QueuePacket();
                    m_CurrentPacket.Write(bytes, 0, bytesToSend);
                    return true;
                }

                if (!SendToTransport(m_CurrentPacket, m_IsReliable, m_Connection, m_ChannelId))
                {
                    if (LogFilter.logError) { Debug.Log("ChannelBuffer SendBytes no space on unreliable channel " + m_ChannelId); }
                    return false;
                }

                m_CurrentPacket.Write(bytes, 0, bytesToSend);
                return true;
            }

            m_CurrentPacket.Write(bytes, 0, bytesToSend);
            if (maxDelay == 0.0f)
            {
                return SendInternalBuffer();
            }
            return true;
        }

        void QueuePacket()
        {
            pendingPacketCount += 1;
            m_PendingPackets.Enqueue(m_CurrentPacket);
            m_CurrentPacket = new MemoryStream();
        }

        public bool SendInternalBuffer()
        {
#if UNITY_EDITOR
            UnityEditor.NetworkDetailStats.IncrementStat(
                UnityEditor.NetworkDetailStats.NetworkDirection.Outgoing,
                MsgType.LLAPIMsg, "msg", 1);
#endif
            if (m_IsReliable && m_PendingPackets.Count > 0)
            {
                // send until transport can take no more
                while (m_PendingPackets.Count > 0)
                {
                    MemoryStream packet = m_PendingPackets.Dequeue();
                    if (!SendToTransport(packet, m_IsReliable, m_Connection, m_ChannelId))
                    {
                        m_PendingPackets.Enqueue(packet);
                        break;
                    }
                    pendingPacketCount -= 1;

                    if (m_IsBroken && m_PendingPackets.Count < (m_MaxPendingPacketCount / 2))
                    {
                        if (LogFilter.logWarn) { Debug.LogWarning("ChannelBuffer recovered from overflow but data was lost."); }
                        m_IsBroken = false;
                    }
                }
                return true;
            }
            return SendToTransport(m_CurrentPacket, m_IsReliable, m_Connection, m_ChannelId);
        }

        // vis2k: send packet to transport (moved here from old ChannelPacket.cs)
        static bool SendToTransport(MemoryStream packet, bool reliable, NetworkConnection conn, int channelId)
        {
            // vis2k: control flow improved to something more logical and shorter
            byte error;
            if (conn.TransportSend(packet.ToArray(), (ushort)packet.Position, channelId, out error))
            {
                packet.Position = 0;
                return true;
            }
            else
            {
                // NoResources and reliable? Then it will be resent, so don't reset position, just return false.
                if (error == (int)NetworkError.NoResources && reliable)
                {
#if UNITY_EDITOR
                    UnityEditor.NetworkDetailStats.IncrementStat(
                        UnityEditor.NetworkDetailStats.NetworkDirection.Outgoing,
                        MsgType.HLAPIResend, "msg", 1);
#endif
                    return false;
                }

                // otherwise something unexpected happened. log the error, reset position and return.
                if (LogFilter.logError) { Debug.LogError("SendToTransport failed. error:" + (NetworkError)error + " channel:" + channelId + " bytesToSend:" + packet.Position); }
                packet.Position = 0;
                return false;
            }
        }
    }
}
#endif //ENABLE_UNET
