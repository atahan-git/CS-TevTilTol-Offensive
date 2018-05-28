using System;

#if ENABLE_UNET

namespace UnityEngine.Networking
{
    // a server's connection TO a LocalClient.
    // sending messages on this connection causes the client's
    // handler function to be invoked directly
    class ULocalConnectionToClient : NetworkConnection
    {
        public LocalClient localClient { get; private set; } // vis2k: private set instead of variable + get/set wrapper

        public ULocalConnectionToClient(LocalClient localClient)
        {
            address = "localClient";
            this.localClient = localClient;
        }

        public override bool Send(short msgType, MessageBase msg)
        {
            localClient.InvokeHandlerOnClient(msgType, msg, Channels.DefaultReliable);
            return true;
        }

        public override bool SendUnreliable(short msgType, MessageBase msg)
        {
            localClient.InvokeHandlerOnClient(msgType, msg, Channels.DefaultUnreliable);
            return true;
        }

        public override bool SendByChannel(short msgType, MessageBase msg, int channelId)
        {
            localClient.InvokeHandlerOnClient(msgType, msg, channelId);
            return true;
        }

        public override bool SendBytes(byte[] bytes, int numBytes, int channelId)
        {
            localClient.InvokeBytesOnClient(bytes, channelId);
            return true;
        }

        public override bool SendWriter(NetworkWriter writer, int channelId)
        {
            localClient.InvokeBytesOnClient(writer.ToArray(), channelId);
            return true;
        }

        public override void GetStatsOut(out int numMsgs, out int numBufferedMsgs, out int numBytes, out int lastBufferedPerSecond)
        {
            numMsgs = 0;
            numBufferedMsgs = 0;
            numBytes = 0;
            lastBufferedPerSecond = 0;
        }

        public override void GetStatsIn(out int numMsgs, out int numBytes)
        {
            numMsgs = 0;
            numBytes = 0;
        }
    }

    // a localClient's connection TO a server.
    // send messages on this connection causes the server's
    // handler function to be invoked directly.

    internal class ULocalConnectionToServer : NetworkConnection
    {
        NetworkServer localServer;

        public ULocalConnectionToServer(NetworkServer localServer)
        {
            address = "localServer";
            this.localServer = localServer;
        }

        public override bool Send(short msgType, MessageBase msg)
        {
            return localServer.InvokeHandlerOnServer(this, msgType, msg, Channels.DefaultReliable);
        }

        public override bool SendUnreliable(short msgType, MessageBase msg)
        {
            return localServer.InvokeHandlerOnServer(this, msgType, msg, Channels.DefaultUnreliable);
        }

        public override bool SendByChannel(short msgType, MessageBase msg, int channelId)
        {
            return localServer.InvokeHandlerOnServer(this, msgType, msg, channelId);
        }

        public override bool SendBytes(byte[] bytes, int numBytes, int channelId)
        {
            if (numBytes <= 0)
            {
                if (LogFilter.logError) { Debug.LogError("LocalConnection:SendBytes cannot send zero bytes"); }
                return false;
            }
            return localServer.InvokeBytes(this, bytes, numBytes, channelId);
        }

        public override bool SendWriter(NetworkWriter writer, int channelId)
        {
            return localServer.InvokeBytes(this, writer.ToArray(), writer.Position, channelId);
        }

        public override void GetStatsOut(out int numMsgs, out int numBufferedMsgs, out int numBytes, out int lastBufferedPerSecond)
        {
            numMsgs = 0;
            numBufferedMsgs = 0;
            numBytes = 0;
            lastBufferedPerSecond = 0;
        }

        public override void GetStatsIn(out int numMsgs, out int numBytes)
        {
            numMsgs = 0;
            numBytes = 0;
        }
    }
}
#endif //ENABLE_UNET
