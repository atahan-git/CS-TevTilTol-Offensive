using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking.Types;
using System.Collections.ObjectModel;

#if ENABLE_UNET

namespace UnityEngine.Networking
{
    public class NetworkServerSimple
    {
        bool m_Initialized = false;
        public int listenPort; // vis2k: public variable instead of get/set wrapper
        public int serverHostId = -1; // vis2k: public variable instead of get/set wrapper
        int m_RelaySlotId = -1;
        public bool useWebSockets; // vis2k: public variable instead of get/set wrapper

        public byte[] messageBuffer { get; private set; } // vis2k: private set instead of get/set wrapper
        public NetworkReader messageReader { get; private set; } // vis2k: private set instead of get/set wrapper

        Type m_NetworkConnectionClass = typeof(NetworkConnection);
        public HostTopology hostTopology { get; private set; } // vis2k: private set instead of get/set wrapper
        List<NetworkConnection> m_Connections = new List<NetworkConnection>();
        public ReadOnlyCollection<NetworkConnection> connections; // vis2k: public instead of get/set wrapper

        NetworkMessageHandlers m_MessageHandlers = new NetworkMessageHandlers();

        public Dictionary<short, NetworkMessageDelegate> handlers { get { return m_MessageHandlers.GetHandlers(); } }

        public Type networkConnectionClass
        {
            get { return m_NetworkConnectionClass; }
        }

        public void SetNetworkConnectionClass<T>() where T : NetworkConnection
        {
            m_NetworkConnectionClass = typeof(T);
        }

        public NetworkServerSimple()
        {
            connections = new ReadOnlyCollection<NetworkConnection>(m_Connections);
        }

        public virtual void Initialize()
        {
            if (m_Initialized)
                return;

            m_Initialized = true;
            NetworkTransport.Init();

            messageBuffer = new byte[NetworkMessage.MaxMessageSize];
            messageReader = new NetworkReader(messageBuffer);

            if (hostTopology == null)
            {
                var config = new ConnectionConfig();
                config.AddChannel(QosType.ReliableSequenced);
                config.AddChannel(QosType.Unreliable);
                hostTopology = new HostTopology(config, 8);
            }

            if (LogFilter.logDebug) { Debug.Log("NetworkServerSimple initialize."); }
        }

        public bool Configure(ConnectionConfig config, int maxConnections)
        {
            HostTopology top = new HostTopology(config, maxConnections);
            return Configure(top);
        }

        public bool Configure(HostTopology topology)
        {
            hostTopology = topology;
            return true;
        }

        public bool Listen(string ipAddress, int serverListenPort)
        {
            Initialize();
            listenPort = serverListenPort;

            if (useWebSockets)
            {
                serverHostId = NetworkTransport.AddWebsocketHost(hostTopology, serverListenPort, ipAddress);
            }
            else
            {
                serverHostId = NetworkTransport.AddHost(hostTopology, serverListenPort, ipAddress);
            }

            if (serverHostId == -1)
            {
                return false;
            }

            if (LogFilter.logDebug) { Debug.Log("NetworkServerSimple listen: " + ipAddress + ":" + listenPort); }
            return true;
        }

        public bool Listen(int serverListenPort)
        {
            return Listen(serverListenPort, hostTopology);
        }

        public bool Listen(int serverListenPort, HostTopology topology)
        {
            hostTopology = topology;
            Initialize();
            listenPort = serverListenPort;

            if (useWebSockets)
            {
                serverHostId = NetworkTransport.AddWebsocketHost(hostTopology, serverListenPort);
            }
            else
            {
                serverHostId = NetworkTransport.AddHost(hostTopology, serverListenPort);
            }

            if (serverHostId == -1)
            {
                return false;
            }

            if (LogFilter.logDebug) { Debug.Log("NetworkServerSimple listen " + listenPort); }
            return true;
        }

        public void ListenRelay(string relayIp, int relayPort, NetworkID netGuid, SourceID sourceId, NodeID nodeId)
        {
            Initialize();

            serverHostId = NetworkTransport.AddHost(hostTopology, listenPort);
            if (LogFilter.logDebug) { Debug.Log("Server Host Slot Id: " + serverHostId); }

            Update();

            byte error;
            NetworkTransport.ConnectAsNetworkHost(
                serverHostId,
                relayIp,
                relayPort,
                netGuid,
                sourceId,
                nodeId,
                out error);

            m_RelaySlotId = 0;
            if (LogFilter.logDebug) { Debug.Log("Relay Slot Id: " + m_RelaySlotId); }
        }

        public void Stop()
        {
            if (LogFilter.logDebug) { Debug.Log("NetworkServerSimple stop "); }
            NetworkTransport.RemoveHost(serverHostId);
            serverHostId = -1;
        }

        internal void RegisterHandlerSafe(short msgType, NetworkMessageDelegate handler)
        {
            m_MessageHandlers.RegisterHandlerSafe(msgType, handler);
        }

        public void RegisterHandler(short msgType, NetworkMessageDelegate handler)
        {
            m_MessageHandlers.RegisterHandler(msgType, handler);
        }

        public void UnregisterHandler(short msgType)
        {
            m_MessageHandlers.UnregisterHandler(msgType);
        }

        public void ClearHandlers()
        {
            m_MessageHandlers.ClearMessageHandlers();
        }

        // this can be used independantly of Update() - such as when using external connections and not listening.
        public void UpdateConnections()
        {
            foreach (NetworkConnection conn in m_Connections) // vis2k: foreach
            {
                if (conn != null)
                    conn.FlushChannels();
            }
        }

        public void Update()
        {
            if (serverHostId == -1)
                return;

            int connectionId;
            int channelId;
            int receivedSize;
            byte error;

            var networkEvent = NetworkEventType.DataEvent;
            if (m_RelaySlotId != -1)
            {
                networkEvent = NetworkTransport.ReceiveRelayEventFromHost(serverHostId, out error);
                if (NetworkEventType.Nothing != networkEvent)
                {
                    if (LogFilter.logDebug) { Debug.Log("NetGroup event:" + networkEvent); }
                }
                if (networkEvent == NetworkEventType.ConnectEvent)
                {
                    if (LogFilter.logDebug) { Debug.Log("NetGroup server connected"); }
                }
                if (networkEvent == NetworkEventType.DisconnectEvent)
                {
                    if (LogFilter.logDebug) { Debug.Log("NetGroup server disconnected"); }
                }
            }

            do
            {
                networkEvent = NetworkTransport.ReceiveFromHost(serverHostId, out connectionId, out channelId, messageBuffer, (int)messageBuffer.Length, out receivedSize, out error);
                if (networkEvent != NetworkEventType.Nothing)
                {
                    if (LogFilter.logDev) { Debug.Log("Server event: host=" + serverHostId + " event=" + networkEvent + " error=" + error); }
                }

                switch (networkEvent)
                {
                    case NetworkEventType.ConnectEvent:
                    {
                        HandleConnect(connectionId, error);
                        break;
                    }

                    case NetworkEventType.DataEvent:
                    {
                        HandleData(connectionId, channelId, receivedSize, error);
                        break;
                    }

                    case NetworkEventType.DisconnectEvent:
                    {
                        HandleDisconnect(connectionId, error);
                        break;
                    }

                    case NetworkEventType.Nothing:
                        break;

                    default:
                        if (LogFilter.logError) { Debug.LogError("Unknown network message type received: " + networkEvent); }
                        break;
                }
            }
            while (networkEvent != NetworkEventType.Nothing);

            UpdateConnections();
        }

        public NetworkConnection FindConnection(int connectionId)
        {
            if (connectionId < 0 || connectionId >= m_Connections.Count)
                return null;

            return m_Connections[connectionId];
        }

        public bool SetConnectionAtIndex(NetworkConnection conn)
        {
            while (m_Connections.Count <= conn.connectionId)
            {
                m_Connections.Add(null);
            }

            if (m_Connections[conn.connectionId] != null)
            {
                // already a connection at this index
                return false;
            }

            m_Connections[conn.connectionId] = conn;
            conn.SetHandlers(m_MessageHandlers);
            return true;
        }

        public bool RemoveConnectionAtIndex(int connectionId)
        {
            if (connectionId < 0 || connectionId >= m_Connections.Count)
                return false;

            m_Connections[connectionId] = null;
            return true;
        }

        void HandleConnect(int connectionId, byte error)
        {
            if (LogFilter.logDebug) { Debug.Log("NetworkServerSimple accepted client:" + connectionId); }

            if (error != 0)
            {
                OnConnectError(connectionId, error);
                return;
            }

            string address;
            int port;
            NetworkID networkId;
            NodeID node;
            byte error2;
            NetworkTransport.GetConnectionInfo(serverHostId, connectionId, out address, out port, out networkId, out node, out error2);

            NetworkConnection conn = (NetworkConnection)Activator.CreateInstance(m_NetworkConnectionClass);
            conn.SetHandlers(m_MessageHandlers);
            conn.Initialize(address, serverHostId, connectionId, hostTopology);
            conn.lastError = (NetworkError)error2;

            // add connection at correct index
            while (m_Connections.Count <= connectionId)
            {
                m_Connections.Add(null);
            }
            m_Connections[connectionId] = conn;

            OnConnected(conn);
        }

        void HandleDisconnect(int connectionId, byte error)
        {
            if (LogFilter.logDebug) { Debug.Log("NetworkServerSimple disconnect client:" + connectionId); }

            var conn = FindConnection(connectionId);
            if (conn == null)
            {
                return;
            }
            conn.lastError = (NetworkError)error;

            if (error != 0)
            {
                if ((NetworkError)error != NetworkError.Timeout)
                {
                    m_Connections[connectionId] = null;
                    if (LogFilter.logError) { Debug.LogError("Server client disconnect error, connectionId: " + connectionId + " error: " + (NetworkError)error); }

                    OnDisconnectError(conn, error);
                    return;
                }
            }

            conn.Disconnect();
            m_Connections[connectionId] = null;
            if (LogFilter.logDebug) { Debug.Log("Server lost client:" + connectionId); }

            OnDisconnected(conn);
        }

        void HandleData(int connectionId, int channelId, int receivedSize, byte error)
        {
            var conn = FindConnection(connectionId);
            if (conn == null)
            {
                if (LogFilter.logError) { Debug.LogError("HandleData Unknown connectionId:" + connectionId); }
                return;
            }
            conn.lastError = (NetworkError)error;

            if (error != 0)
            {
                OnDataError(conn, error);
                return;
            }

            messageReader.SeekZero();
            OnData(conn, receivedSize, channelId);
        }

        public void SendBytesTo(int connectionId, byte[] bytes, int numBytes, int channelId)
        {
            var outConn = FindConnection(connectionId);
            if (outConn == null)
            {
                return;
            }
            outConn.SendBytes(bytes, numBytes, channelId);
        }

        public void SendWriterTo(int connectionId, NetworkWriter writer, int channelId)
        {
            var outConn = FindConnection(connectionId);
            if (outConn == null)
            {
                return;
            }
            outConn.SendWriter(writer, channelId);
        }

        public void Disconnect(int connectionId)
        {
            var outConn = FindConnection(connectionId);
            if (outConn == null)
            {
                return;
            }
            outConn.Disconnect();
            m_Connections[connectionId] = null;
        }

        public void DisconnectAllConnections()
        {
            for (int i = 0; i < m_Connections.Count; i++)
            {
                NetworkConnection conn = m_Connections[i];
                if (conn != null)
                {
                    conn.Disconnect();
                    conn.Dispose();
                }
            }
        }

        // --------------------------- virtuals ---------------------------------------

        public virtual void OnConnectError(int connectionId, byte error)
        {
            Debug.LogError("OnConnectError error:" + error);
        }

        public virtual void OnDataError(NetworkConnection conn, byte error)
        {
            Debug.LogError("OnDataError error:" + error);
        }

        public virtual void OnDisconnectError(NetworkConnection conn, byte error)
        {
            Debug.LogError("OnDisconnectError error:" + error);
        }

        public virtual void OnConnected(NetworkConnection conn)
        {
            conn.InvokeHandlerNoData(MsgType.Connect);
        }

        public virtual void OnDisconnected(NetworkConnection conn)
        {
            conn.InvokeHandlerNoData(MsgType.Disconnect);
        }

        public virtual void OnData(NetworkConnection conn, int receivedSize, int channelId)
        {
            conn.TransportReceive(messageBuffer, receivedSize, channelId);
        }
    }
}
#endif
