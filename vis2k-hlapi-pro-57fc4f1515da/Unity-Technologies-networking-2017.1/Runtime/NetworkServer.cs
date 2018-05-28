#if ENABLE_UNET
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking.Types;
using UnityEngineInternal;

namespace UnityEngine.Networking
{
    public sealed class NetworkServer
    {
        public static bool active { get; private set; } // vis2k: private set instead of variable + get/set wrapper
        static volatile NetworkServer s_Instance;
        static object s_Sync = new Object();
        public static bool dontListen; // vis2k: public instead of get/set wrapper
        bool m_LocalClientActive;

        // only used for localConnection accessor
        List<NetworkConnection> m_LocalConnectionsFakeList = new List<NetworkConnection>();
        ULocalConnectionToClient m_LocalConnection = null;

        NetworkScene m_NetworkScene;
        HashSet<int> m_ExternalConnections;
        ServerSimpleWrapper m_SimpleServerSimple;

        float m_MaxDelay = 0.1f;

        // this is cached here for easy access when checking the size of state update packets in NetworkIdentity
        static internal ushort maxPacketSize;

        static public List<NetworkConnection> localConnections { get { return instance.m_LocalConnectionsFakeList; } }

        static public int listenPort { get { return instance.m_SimpleServerSimple.listenPort; } }
        static public int serverHostId { get { return instance.m_SimpleServerSimple.serverHostId; } }

        static public ReadOnlyCollection<NetworkConnection> connections  { get { return instance.m_SimpleServerSimple.connections; } }
        static public Dictionary<short, NetworkMessageDelegate> handlers { get { return instance.m_SimpleServerSimple.handlers; } }
        static public HostTopology hostTopology { get { return instance.m_SimpleServerSimple.hostTopology; }}
        public static Dictionary<NetworkInstanceId, NetworkIdentity> objects { get { return instance.m_NetworkScene.localObjects; } }

#if ENABLE_UNET_HOST_MIGRATION
        [Obsolete("Moved to NetworkMigrationManager")]
        public static bool sendPeerInfo { get { return false; } set {} }
#else
        [Obsolete("Removed")]
        public static bool sendPeerInfo { get { return false; } set {} }
#endif

        public static bool useWebSockets { get { return instance.m_SimpleServerSimple.useWebSockets; } set { instance.m_SimpleServerSimple.useWebSockets = value; } }

        internal static NetworkServer instance
        {
            get
            {
                if (s_Instance == null)
                {
                    lock (s_Sync)
                    {
                        if (s_Instance == null)
                        {
                            s_Instance = new NetworkServer();
                        }
                    }
                }
                return s_Instance;
            }
        }

        public static bool localClientActive { get { return instance.m_LocalClientActive; } }
        public static int numChannels { get { return instance.m_SimpleServerSimple.hostTopology.DefaultConfig.ChannelCount; } }

        public static float maxDelay { get { return instance.m_MaxDelay; } set { instance.InternalSetMaxDelay(value); } }


        static public Type networkConnectionClass
        {
            get { return instance.m_SimpleServerSimple.networkConnectionClass; }
        }

        static public void SetNetworkConnectionClass<T>() where T : NetworkConnection
        {
            instance.m_SimpleServerSimple.SetNetworkConnectionClass<T>();
        }

        NetworkServer()
        {
            NetworkTransport.Init();
            if (LogFilter.logDev) { Debug.Log("NetworkServer Created version " + Version.Current); }
            m_ExternalConnections = new HashSet<int>();
            m_NetworkScene = new NetworkScene();
            m_SimpleServerSimple = new ServerSimpleWrapper(this);
        }

        static public bool Configure(ConnectionConfig config, int maxConnections)
        {
            return instance.m_SimpleServerSimple.Configure(config, maxConnections);
        }

        static public bool Configure(HostTopology topology)
        {
            return instance.m_SimpleServerSimple.Configure(topology);
        }

        public static void Reset()
        {
#if UNITY_EDITOR
            UnityEditor.NetworkDetailStats.ResetAll();
#endif
            NetworkTransport.Shutdown();
            NetworkTransport.Init();
            s_Instance = null;
            active = false;
        }

        public static void Shutdown()
        {
            if (s_Instance != null)
            {
                s_Instance.InternalDisconnectAll();

                if (dontListen)
                {
                    // was never started, so dont stop
                }
                else
                {
                    s_Instance.m_SimpleServerSimple.Stop();
                }

                s_Instance = null;
            }
            dontListen = false;
            active = false;
        }

        static public bool Listen(MatchInfo matchInfo, int listenPort)
        {
            if (!matchInfo.usingRelay)
            {
                return instance.InternalListen(null, listenPort);
            }

            instance.InternalListenRelay(matchInfo.address, matchInfo.port, matchInfo.networkId, Utility.GetSourceID(), matchInfo.nodeId);
            return true;
        }

        internal void RegisterMessageHandlers()
        {
            m_SimpleServerSimple.RegisterHandlerSafe(MsgType.Ready, OnClientReadyMessage);
            m_SimpleServerSimple.RegisterHandlerSafe(MsgType.Command, OnCommandMessage);
            m_SimpleServerSimple.RegisterHandlerSafe(MsgType.LocalPlayerTransform, NetworkTransform.OnClientToServerSync);
            //m_SimpleServerSimple.RegisterHandlerSafe(MsgType.LocalChildTransform, NetworkTransformChild.HandleChildTransform);
            m_SimpleServerSimple.RegisterHandlerSafe(MsgType.RemovePlayer, OnRemovePlayerMessage);
            m_SimpleServerSimple.RegisterHandlerSafe(MsgType.Animation, NetworkAnimator.OnAnimationServerMessage);
            m_SimpleServerSimple.RegisterHandlerSafe(MsgType.AnimationParameters, NetworkAnimator.OnAnimationParametersServerMessage);
            m_SimpleServerSimple.RegisterHandlerSafe(MsgType.AnimationTrigger, NetworkAnimator.OnAnimationTriggerServerMessage);
            m_SimpleServerSimple.RegisterHandlerSafe(MsgType.Fragment, NetworkConnection.OnFragment);

            // also setup max packet size.
            maxPacketSize = hostTopology.DefaultConfig.PacketSize;
        }

        static public void ListenRelay(string relayIp, int relayPort, NetworkID netGuid, SourceID sourceId, NodeID nodeId)
        {
            instance.InternalListenRelay(relayIp, relayPort, netGuid, sourceId, nodeId);
        }

        void InternalListenRelay(string relayIp, int relayPort, NetworkID netGuid, SourceID sourceId, NodeID nodeId)
        {
            m_SimpleServerSimple.ListenRelay(relayIp, relayPort, netGuid, sourceId, nodeId);
            active = true;
            RegisterMessageHandlers();
        }

        static public bool Listen(int serverPort)
        {
            return instance.InternalListen(null, serverPort);
        }

        static public bool Listen(string ipAddress, int serverPort)
        {
            return instance.InternalListen(ipAddress, serverPort);
        }

        internal bool InternalListen(string ipAddress, int serverPort)
        {
            if (dontListen)
            {
                // dont start simpleServer - this mode uses external connections instead
                m_SimpleServerSimple.Initialize();
            }
            else if (!m_SimpleServerSimple.Listen(ipAddress, serverPort)) // vis2k: else if instead of else+if
            {
                return false;
            }

            maxPacketSize = hostTopology.DefaultConfig.PacketSize;
            active = true;
            RegisterMessageHandlers();
            return true;
        }

#if ENABLE_UNET_HOST_MIGRATION
        static public NetworkClient BecomeHost(NetworkClient oldClient, int port, MatchInfo matchInfo, int oldConnectionId, PeerInfoMessage[] peers)
        {
            return instance.BecomeHostInternal(oldClient, port, matchInfo, oldConnectionId, peers);
        }

        internal NetworkClient BecomeHostInternal(NetworkClient oldClient, int port, MatchInfo matchInfo, int oldConnectionId, PeerInfoMessage[] peers)
        {
            if (active)
            {
                if (LogFilter.logError) { Debug.LogError("BecomeHost already a server."); }
                return null;
            }

            if (!NetworkClient.active)
            {
                if (LogFilter.logError) { Debug.LogError("BecomeHost NetworkClient not active."); }
                return null;
            }

            // setup a server

            NetworkServer.Configure(hostTopology);

            if (matchInfo == null)
            {
                if (LogFilter.logDev) { Debug.Log("BecomeHost Listen on " + port); }

                if (!NetworkServer.Listen(port))
                {
                    if (LogFilter.logError) { Debug.LogError("BecomeHost bind failed."); }
                    return null;
                }
            }
            else
            {
                if (LogFilter.logDev) { Debug.Log("BecomeHost match:" + matchInfo.networkId); }
                NetworkServer.ListenRelay(matchInfo.address, matchInfo.port, matchInfo.networkId, Utility.GetSourceID(), matchInfo.nodeId);
            }

            // setup server objects
            foreach (var uv in ClientScene.objects.Values)
            {
                if (uv != null && uv.gameObject != null) // vis2k: != null checks instead of continue
                {
                    NetworkIdentity.AddNetworkId(uv.netId.Value);

                    //NOTE: have to pass false to isServer here so that onStartServer sets object up properly.
                    m_NetworkScene.SetLocalObject(uv.netId, uv.gameObject, false, false);
                    uv.OnStartServer(true);
                }
            }

            // reset the client peer info(?)

            if (LogFilter.logDev) { Debug.Log("NetworkServer BecomeHost done. oldConnectionId:" + oldConnectionId); }
            RegisterMessageHandlers();

            if (!NetworkClient.RemoveClient(oldClient))
            {
                if (LogFilter.logError) { Debug.LogError("BecomeHost failed to remove client"); }
            }

            if (LogFilter.logDev) { Debug.Log("BecomeHost localClient ready"); }

            // make a localclient for me
            var newLocalClient = ClientScene.ReconnectLocalServer();
            ClientScene.Ready(newLocalClient.connection);

            // cause local players and objects to be reconnected
            ClientScene.SetReconnectId(oldConnectionId, peers);
            ClientScene.AddPlayer(ClientScene.readyConnection, 0);

            return newLocalClient;
        }
#endif

        void InternalSetMaxDelay(float seconds)
        {
            // set on existing connections
            foreach (NetworkConnection conn in connections) // vis2k: foreach
            {
                if (conn != null)
                    conn.SetMaxDelay(seconds);
            }

            // save for future connections
            m_MaxDelay = seconds;
        }

        // called by LocalClient to add itself. dont call directly.
        internal int AddLocalClient(LocalClient localClient)
        {
            if (m_LocalConnectionsFakeList.Count != 0)
            {
                Debug.LogError("Local Connection already exists");
                return -1;
            }

            m_LocalConnection = new ULocalConnectionToClient(localClient);
            m_LocalConnection.connectionId = 0;
            m_SimpleServerSimple.SetConnectionAtIndex(m_LocalConnection);

            // this is for backwards compatibility with localConnections property
            m_LocalConnectionsFakeList.Add(m_LocalConnection);

            m_LocalConnection.InvokeHandlerNoData(MsgType.Connect);

            return 0;
        }

        internal void RemoveLocalClient(NetworkConnection localClientConnection)
        {
            // vis2k: .Remove instead of for loop
            m_LocalConnectionsFakeList.RemoveAll(conn => conn.connectionId == localClientConnection.connectionId);

            if (m_LocalConnection != null)
            {
                m_LocalConnection.Disconnect();
                m_LocalConnection.Dispose();
                m_LocalConnection = null;
            }
            m_LocalClientActive = false;
            m_SimpleServerSimple.RemoveConnectionAtIndex(0);
        }

        internal void SetLocalObjectOnServer(NetworkInstanceId netId, GameObject obj)
        {
            if (LogFilter.logDev) { Debug.Log("SetLocalObjectOnServer " + netId + " " + obj); }

            m_NetworkScene.SetLocalObject(netId, obj, false, true);
        }

        internal void ActivateLocalClientScene()
        {
            if (m_LocalClientActive)
                return;

            // ClientScene for a local connection is becoming active. any spawned objects need to be started as client objects
            m_LocalClientActive = true;
            foreach (var uv in objects.Values)
            {
                if (!uv.isClient)
                {
                    if (LogFilter.logDev) { Debug.Log("ActivateClientScene " + uv.netId + " " + uv.gameObject); }

                    ClientScene.SetLocalObject(uv.netId, uv.gameObject);
                    uv.OnStartClient();
                }
            }
        }

        static public bool SendToAll(short msgType, MessageBase msg)
        {
            if (LogFilter.logDev) { Debug.Log("Server.SendToAll msgType:" + msgType); }

            bool result = true;

            // remote connections
            foreach (NetworkConnection conn in connections) // vis2k: foreach
            {
                if (conn != null)
                    result &= conn.Send(msgType, msg);
            }

            return result;
        }

        // this is like SendToReady - but it doesn't check the ready flag on the connection.
        // this is used for ObjectDestroy messages.
        static bool SendToObservers(GameObject contextObj, short msgType, MessageBase msg)
        {
            if (LogFilter.logDev) { Debug.Log("Server.SendToObservers id:" + msgType); }

            // vis2k: refactored control flow
            var uv = contextObj.GetComponent<NetworkIdentity>();
            if (uv != null && uv.observers != null)
            {
                bool result = true;
                foreach (NetworkConnection conn in uv.observers)
                {
                    result &= conn.Send(msgType, msg);
                }
                return result;
            }
            return false;
        }

        static public bool SendToReady(GameObject contextObj, short msgType, MessageBase msg)
        {
            if (LogFilter.logDev) { Debug.Log("Server.SendToReady id:" + msgType); }

            if (contextObj == null)
            {
                foreach (NetworkConnection conn in connections) // vis2k: foreach
                {
                    if (conn != null && conn.isReady)
                    {
                        conn.Send(msgType, msg);
                    }
                }
                return true;
            }


            // vis2k: refactored control flow
            var uv = contextObj.GetComponent<NetworkIdentity>();
            if (uv != null && uv.observers != null)
            {
                bool result = true;
                foreach (NetworkConnection conn in uv.observers)
                {
                    if (conn.isReady)
                    {
                        result &= conn.Send(msgType, msg);
                    }
                }
                return result;
            }
            return false;
        }

        static public void SendWriterToReady(GameObject contextObj, NetworkWriter writer, int channelId)
        {
            if (writer.Position > ushort.MaxValue) // vis2k: ushort.MaxValue instead of short.MaxValue to support 64k
            {
                throw new UnityException("NetworkWriter used buffer is too big!");
            }
            SendBytesToReady(contextObj, writer.ToArray(), writer.Position, channelId);
        }

        static public void SendBytesToReady(GameObject contextObj, byte[] buffer, int numBytes, int channelId)
        {
            if (contextObj == null)
            {
                // no context.. send to all ready connections
                bool success = true;
                foreach (NetworkConnection conn in connections) // vis2k: foreach
                {
                    if (conn != null && conn.isReady)
                    {
                        if (!conn.SendBytes(buffer, numBytes, channelId))
                        {
                            success = false;
                        }
                    }
                }
                if (!success)
                {
                    if (LogFilter.logWarn) { Debug.LogWarning("SendBytesToReady failed"); }
                }
                return;
            }

            var uv = contextObj.GetComponent<NetworkIdentity>();
            try
            {
                bool success = true;
                foreach (NetworkConnection conn in uv.observers) // vis2k: foreach
                {
                    if (conn.isReady) // vis2k: if ready instead of if !ready and continue
                    {
                        if (!conn.SendBytes(buffer, numBytes, channelId))
                        {
                            success = false;
                        }
                    }
                }
                if (!success)
                {
                    if (LogFilter.logWarn) { Debug.LogWarning("SendBytesToReady failed for " + contextObj); }
                }
            }
            catch (NullReferenceException)
            {
                // observers may be null if object has not been spawned
                if (LogFilter.logWarn) { Debug.LogWarning("SendBytesToReady object " + contextObj + " has not been spawned"); }
            }
        }

        public static void SendBytesToPlayer(GameObject player, byte[] buffer, int numBytes, int channelId)
        {
            foreach (NetworkConnection conn in connections) // vis2k: foreach
            {
                if (conn != null) // vis2k: != instead of == and continue
                {
                    foreach (PlayerController pc in conn.playerControllers) // vis2k: foreach
                    {
                        if (pc.IsValid && pc.gameObject == player)
                        {
                            conn.SendBytes(buffer, numBytes, channelId);
                            break;
                        }
                    }
                }
            }
        }

        static public bool SendUnreliableToAll(short msgType, MessageBase msg)
        {
            if (LogFilter.logDev) { Debug.Log("Server.SendUnreliableToAll msgType:" + msgType); }

            bool result = true;
            foreach (NetworkConnection conn in connections) // vis2k: foreach
            {
                if (conn != null)
                    result &= conn.SendUnreliable(msgType, msg);
            }
            return result;
        }

        static public bool SendUnreliableToReady(GameObject contextObj, short msgType, MessageBase msg)
        {
            if (LogFilter.logDev) { Debug.Log("Server.SendUnreliableToReady id:" + msgType); }

            if (contextObj == null)
            {
                // no context.. send to all ready connections
                foreach (NetworkConnection conn in connections) // vis2k: foreach
                {
                    if (conn != null && conn.isReady)
                    {
                        conn.SendUnreliable(msgType, msg);
                    }
                }
                return true;
            }

            bool result = true;
            var uv = contextObj.GetComponent<NetworkIdentity>();
            foreach (NetworkConnection conn in uv.observers) // vis2k: foreach
            {
                if (conn.isReady) // vis2k: ready instead of !ready and continue
                {
                    result &= conn.SendUnreliable(msgType, msg);
                }
            }
            return result;
        }

        static public bool SendByChannelToAll(short msgType, MessageBase msg, int channelId)
        {
            if (LogFilter.logDev) { Debug.Log("Server.SendByChannelToAll id:" + msgType); }

            bool result = true;
            foreach (NetworkConnection conn in connections) // vis2k: foreach
            {
                if (conn != null)
                    result &= conn.SendByChannel(msgType, msg, channelId);
            }
            return result;
        }

        static public bool SendByChannelToReady(GameObject contextObj, short msgType, MessageBase msg, int channelId)
        {
            if (LogFilter.logDev) { Debug.Log("Server.SendByChannelToReady msgType:" + msgType); }

            if (contextObj == null)
            {
                // no context.. send to all ready connections
                foreach (NetworkConnection conn in connections) // vis2k: foreach
                {
                    if (conn != null && conn.isReady)
                    {
                        conn.SendByChannel(msgType, msg, channelId);
                    }
                }
                return true;
            }

            bool result = true;
            var uv = contextObj.GetComponent<NetworkIdentity>();
            foreach (NetworkConnection conn in uv.observers) // vis2k: foreach
            {
                if (conn.isReady) // vis2k: ready instead of !ready and continue
                {
                    result &= conn.SendByChannel(msgType, msg, channelId);
                }
            }
            return result;
        }

        static public void DisconnectAll()
        {
            instance.InternalDisconnectAll();
        }

        internal void InternalDisconnectAll()
        {
            m_SimpleServerSimple.DisconnectAllConnections();

            if (m_LocalConnection != null)
            {
                m_LocalConnection.Disconnect();
                m_LocalConnection.Dispose();
                m_LocalConnection = null;
            }

            m_LocalClientActive = false;
        }

        // The user should never need to pump the update loop manually
        internal static void Update()
        {
            if (s_Instance != null)
                s_Instance.InternalUpdate();
        }

        void UpdateServerObjects()
        {
            // vis2k: original code cought null and missing ref exceptions and only removed null entries
            // every 100 frames. this is ultra complicated, uses extra state and is probably even slower
            // than removing null entries after updating each time:
            List<NetworkInstanceId> remove = new List<NetworkInstanceId>();
            foreach (var kvp in objects)
            {
                if (kvp.Value != null && kvp.Value.gameObject != null)
                {
                    kvp.Value.UNetUpdate();
                }
                else
                {
                    remove.Add(kvp.Key); 
                }
            }

            // now remove
            foreach (NetworkInstanceId key in remove)
            {
                objects.Remove(key);
            }
        }

        internal void InternalUpdate()
        {
            m_SimpleServerSimple.Update();

            if (dontListen)
            {
                m_SimpleServerSimple.UpdateConnections();
            }

            UpdateServerObjects();
        }

        void OnConnected(NetworkConnection conn)
        {
            if (LogFilter.logDebug) { Debug.Log("Server accepted client:" + conn.connectionId); }

            // add player info
            conn.SetMaxDelay(m_MaxDelay);

            conn.InvokeHandlerNoData(MsgType.Connect);

            SendCrc(conn);
        }

        void OnDisconnected(NetworkConnection conn)
        {
            conn.InvokeHandlerNoData(MsgType.Disconnect);

            // vis2k: shorter
            if (conn.playerControllers.Any(pc => pc.gameObject != null))
            {
                //NOTE: should there be default behaviour here to destroy the associated player?
                if (LogFilter.logWarn) { Debug.LogWarning("Player not destroyed when connection disconnected."); }
            }

            if (LogFilter.logDebug) { Debug.Log("Server lost client:" + conn.connectionId); }
            conn.RemoveObservers();
            conn.Dispose();
        }

        void OnData(NetworkConnection conn, int receivedSize, int channelId)
        {
#if UNITY_EDITOR
            UnityEditor.NetworkDetailStats.IncrementStat(
                UnityEditor.NetworkDetailStats.NetworkDirection.Incoming,
                MsgType.LLAPIMsg, "msg", 1);
#endif
            conn.TransportReceive(m_SimpleServerSimple.messageBuffer, receivedSize, channelId);
        }

        void GenerateConnectError(byte error)
        {
            if (LogFilter.logError) { Debug.LogError("UNet Server Connect Error: " + error); }
            GenerateError(null, error);
        }

        void GenerateDataError(NetworkConnection conn, byte error)
        {
            NetworkError dataError = (NetworkError)error;
            if (LogFilter.logError) { Debug.LogError("UNet Server Data Error: " + dataError); }
            GenerateError(conn, error);
        }

        void GenerateDisconnectError(NetworkConnection conn, byte error)
        {
            NetworkError disconnectError = (NetworkError)error;
            if (LogFilter.logError) { Debug.LogError("UNet Server Disconnect Error: " + disconnectError + " conn:[" + conn + "]:" + conn.connectionId); }
            GenerateError(conn, error);
        }

        void GenerateError(NetworkConnection conn, byte error)
        {
            if (handlers.ContainsKey(MsgType.Error))
            {
                ErrorMessage msg = new ErrorMessage();
                msg.errorCode = error;

                // write the message to a local buffer
                NetworkWriter writer = new NetworkWriter();
                msg.Serialize(writer);

                // pass a reader (attached to local buffer) to handler
                NetworkReader reader = new NetworkReader(writer);
                conn.InvokeHandler(MsgType.Error, reader, 0);
            }
        }

        static public void RegisterHandler(short msgType, NetworkMessageDelegate handler)
        {
            instance.m_SimpleServerSimple.RegisterHandler(msgType, handler);
        }

        static public void UnregisterHandler(short msgType)
        {
            instance.m_SimpleServerSimple.UnregisterHandler(msgType);
        }

        static public void ClearHandlers()
        {
            instance.m_SimpleServerSimple.ClearHandlers();
        }

        static public void ClearSpawners()
        {
            NetworkScene.ClearSpawners();
        }

        static public void GetStatsOut(out int numMsgs, out int numBufferedMsgs, out int numBytes, out int lastBufferedPerSecond)
        {
            numMsgs = 0;
            numBufferedMsgs = 0;
            numBytes = 0;
            lastBufferedPerSecond = 0;

            foreach (NetworkConnection conn in connections) // vis2k: foreach
            {
                if (conn != null)
                {
                    int snumMsgs;
                    int snumBufferedMsgs;
                    int snumBytes;
                    int slastBufferedPerSecond;

                    conn.GetStatsOut(out snumMsgs, out snumBufferedMsgs, out snumBytes, out slastBufferedPerSecond);

                    numMsgs += snumMsgs;
                    numBufferedMsgs += snumBufferedMsgs;
                    numBytes += snumBytes;
                    lastBufferedPerSecond += slastBufferedPerSecond;
                }
            }
        }

        static public void GetStatsIn(out int numMsgs, out int numBytes)
        {
            numMsgs = 0;
            numBytes = 0;
            foreach (NetworkConnection conn in connections) // vis2k: foreach
            {
                if (conn != null)
                {
                    int cnumMsgs;
                    int cnumBytes;

                    conn.GetStatsIn(out cnumMsgs, out cnumBytes);

                    numMsgs += cnumMsgs;
                    numBytes += cnumBytes;
                }
            }
        }

        // send this message to the player only
        static public void SendToClientOfPlayer(GameObject player, short msgType, MessageBase msg)
        {
            foreach (NetworkConnection conn in connections) // vis2k: foreach
            {
                if (conn != null)
                {
                    foreach (PlayerController pc in conn.playerControllers) // vis2k: foreach
                    {
                        if (pc.IsValid && pc.gameObject == player)
                        {
                            conn.Send(msgType, msg);
                            return;
                        }
                    }
                }
            }

            if (LogFilter.logError) { Debug.LogError("Failed to send message to player object '" + player.name + ", not found in connection list"); }
        }

        static public void SendToClient(int connectionId, short msgType, MessageBase msg)
        {
            if (connectionId < connections.Count)
            {
                var conn = connections[connectionId];
                if (conn != null)
                {
                    conn.Send(msgType, msg);
                    return;
                }
            }
            if (LogFilter.logError) { Debug.LogError("Failed to send message to connection ID '" + connectionId + ", not found in connection list"); }
        }

        static public bool ReplacePlayerForConnection(NetworkConnection conn, GameObject player, short playerControllerId, NetworkHash128 assetId)
        {
            NetworkIdentity id;
            if (GetNetworkIdentity(player, out id))
            {
                id.SetDynamicAssetId(assetId);
            }
            return instance.InternalReplacePlayerForConnection(conn, player, playerControllerId);
        }

        static public bool ReplacePlayerForConnection(NetworkConnection conn, GameObject player, short playerControllerId)
        {
            return instance.InternalReplacePlayerForConnection(conn, player, playerControllerId);
        }

        static public bool AddPlayerForConnection(NetworkConnection conn, GameObject player, short playerControllerId, NetworkHash128 assetId)
        {
            NetworkIdentity id;
            if (GetNetworkIdentity(player, out id))
            {
                id.SetDynamicAssetId(assetId);
            }
            return instance.InternalAddPlayerForConnection(conn, player, playerControllerId);
        }

        static public bool AddPlayerForConnection(NetworkConnection conn, GameObject player, short playerControllerId)
        {
            return instance.InternalAddPlayerForConnection(conn, player, playerControllerId);
        }

        internal bool InternalAddPlayerForConnection(NetworkConnection conn, GameObject playerGameObject, short playerControllerId)
        {
            NetworkIdentity playerNetworkIdentity;
            if (!GetNetworkIdentity(playerGameObject, out playerNetworkIdentity))
            {
                if (LogFilter.logError) { Debug.Log("AddPlayer: playerGameObject has no NetworkIdentity. Please add a NetworkIdentity to " + playerGameObject); }
                return false;
            }
            playerNetworkIdentity.Reset();

            if (!CheckPlayerControllerIdForConnection(conn, playerControllerId))
                return false;

            // cannot have a player object in "Add" version
            PlayerController oldController = null;
            GameObject oldPlayer = null;
            if (conn.GetPlayerController(playerControllerId, out oldController))
            {
                oldPlayer = oldController.gameObject;
            }
            if (oldPlayer != null)
            {
                if (LogFilter.logError) { Debug.Log("AddPlayer: player object already exists for playerControllerId of " + playerControllerId); }
                return false;
            }

            PlayerController newPlayerController = new PlayerController(playerGameObject, playerControllerId);
            conn.SetPlayerController(newPlayerController);

            // Set the playerControllerId on the NetworkIdentity on the server, NetworkIdentity.SetLocalPlayer is not called on the server (it is on clients and that sets the playerControllerId there)
            playerNetworkIdentity.SetConnectionToClient(conn, newPlayerController.playerControllerId);

            SetClientReady(conn);

            if (SetupLocalPlayerForConnection(conn, playerNetworkIdentity, newPlayerController))
            {
                return true;
            }

            if (LogFilter.logDebug) { Debug.Log("Adding new playerGameObject object netId: " + playerGameObject.GetComponent<NetworkIdentity>().netId + " asset ID " + playerGameObject.GetComponent<NetworkIdentity>().assetId); }

            FinishPlayerForConnection(conn, playerNetworkIdentity, playerGameObject);
            if (playerNetworkIdentity.localPlayerAuthority)
            {
                playerNetworkIdentity.SetClientOwner(conn);
            }
            return true;
        }

        static bool CheckPlayerControllerIdForConnection(NetworkConnection conn, short playerControllerId)
        {
            if (playerControllerId < 0)
            {
                if (LogFilter.logError) { Debug.LogError("AddPlayer: playerControllerId of " + playerControllerId + " is negative"); }
                return false;
            }
            if (playerControllerId > PlayerController.MaxPlayersPerClient)
            {
                if (LogFilter.logError) { Debug.Log("AddPlayer: playerControllerId of " + playerControllerId + " is too high. max is " + PlayerController.MaxPlayersPerClient); }
                return false;
            }
            if (playerControllerId > PlayerController.MaxPlayersPerClient / 2)
            {
                if (LogFilter.logWarn) { Debug.LogWarning("AddPlayer: playerControllerId of " + playerControllerId + " is unusually high"); }
            }
            return true;
        }

        bool SetupLocalPlayerForConnection(NetworkConnection conn, NetworkIdentity uv, PlayerController newPlayerController)
        {
            if (LogFilter.logDev) { Debug.Log("NetworkServer SetupLocalPlayerForConnection netID:" + uv.netId); }

            var localConnection = conn as ULocalConnectionToClient;
            if (localConnection != null)
            {
                if (LogFilter.logDev) { Debug.Log("NetworkServer AddPlayer handling ULocalConnectionToClient"); }

                // Spawn this player for other players, instead of SpawnObject:
                if (uv.netId.IsEmpty())
                {
                    // it is allowed to provide an already spawned object as the new player object.
                    // so dont spawn it again.
                    uv.OnStartServer(true);
                }
                uv.RebuildObservers(true);
                SendSpawnMessage(uv, null);

                // Set up local player instance on the client instance and update local object map
                localConnection.localClient.AddLocalPlayer(newPlayerController);
                uv.SetClientOwner(conn);

                // Trigger OnAuthority
                uv.ForceAuthority(true);

                // Trigger OnStartLocalPlayer
                uv.SetLocalPlayer(newPlayerController.playerControllerId);
                return true;
            }
            return false;
        }

        static void FinishPlayerForConnection(NetworkConnection conn, NetworkIdentity uv, GameObject playerGameObject)
        {
            if (uv.netId.IsEmpty())
            {
                // it is allowed to provide an already spawned object as the new player object.
                // so dont spawn it again.
                Spawn(playerGameObject);
            }

            OwnerMessage owner = new OwnerMessage();
            owner.netId = uv.netId;
            owner.playerControllerId = uv.playerControllerId;
            conn.Send(MsgType.Owner, owner);
        }

        internal bool InternalReplacePlayerForConnection(NetworkConnection conn, GameObject playerGameObject, short playerControllerId)
        {
            NetworkIdentity playerNetworkIdentity;
            if (!GetNetworkIdentity(playerGameObject, out playerNetworkIdentity))
            {
                if (LogFilter.logError) { Debug.LogError("ReplacePlayer: playerGameObject has no NetworkIdentity. Please add a NetworkIdentity to " + playerGameObject); }
                return false;
            }

            if (!CheckPlayerControllerIdForConnection(conn, playerControllerId))
                return false;

            //NOTE: there can be an existing player
            if (LogFilter.logDev) { Debug.Log("NetworkServer ReplacePlayer"); }

            // is there already an owner that is a different object??
            PlayerController oldOwner;
            if (conn.GetPlayerController(playerControllerId, out oldOwner))
            {
                oldOwner.unetView.SetNotLocalPlayer();
                oldOwner.unetView.ClearClientOwner();
            }

            PlayerController newPlayerController = new PlayerController(playerGameObject, playerControllerId);
            conn.SetPlayerController(newPlayerController);

            // Set the playerControllerId on the NetworkIdentity on the server, NetworkIdentity.SetLocalPlayer is not called on the server (it is on clients and that sets the playerControllerId there)
            playerNetworkIdentity.SetConnectionToClient(conn, newPlayerController.playerControllerId);

            //NOTE: DONT set connection ready.

            if (LogFilter.logDev) { Debug.Log("NetworkServer ReplacePlayer setup local"); }

            if (SetupLocalPlayerForConnection(conn, playerNetworkIdentity, newPlayerController))
            {
                return true;
            }

            if (LogFilter.logDebug) { Debug.Log("Replacing playerGameObject object netId: " + playerGameObject.GetComponent<NetworkIdentity>().netId + " asset ID " + playerGameObject.GetComponent<NetworkIdentity>().assetId); }

            FinishPlayerForConnection(conn, playerNetworkIdentity, playerGameObject);
            if (playerNetworkIdentity.localPlayerAuthority)
            {
                playerNetworkIdentity.SetClientOwner(conn);
            }
            return true;
        }

        static bool GetNetworkIdentity(GameObject go, out NetworkIdentity view)
        {
            view = go.GetComponent<NetworkIdentity>();
            if (view == null)
            {
                if (LogFilter.logError) { Debug.LogError("UNET failure. GameObject doesn't have NetworkIdentity."); }
                return false;
            }
            return true;
        }

        static public void SetClientReady(NetworkConnection conn)
        {
            instance.SetClientReadyInternal(conn);
        }

        internal void SetClientReadyInternal(NetworkConnection conn)
        {
            if (LogFilter.logDebug) { Debug.Log("SetClientReadyInternal for conn:" + conn.connectionId); }

            if (conn.isReady)
            {
                if (LogFilter.logDebug) { Debug.Log("SetClientReady conn " + conn.connectionId + " already ready"); }
                return;
            }

            if (conn.playerControllers.Count == 0)
            {
                // this is now allowed
                if (LogFilter.logDebug) { Debug.LogWarning("Ready with no player object"); }
            }

            conn.isReady = true;

            var localConnection = conn as ULocalConnectionToClient;
            if (localConnection != null)
            {
                if (LogFilter.logDev) { Debug.Log("NetworkServer Ready handling ULocalConnectionToClient"); }

                // Setup spawned objects for local player
                // Only handle the local objects for the first player (no need to redo it when doing more local players)
                // and don't handle player objects here, they were done above
                foreach (NetworkIdentity uv in objects.Values)
                {
                    // Need to call OnStartClient directly here, as it's already been added to the local object dictionary
                    // in the above SetLocalPlayer call
                    if (uv != null && uv.gameObject != null)
                    {
                        var vis = uv.OnCheckObserver(conn);
                        if (vis)
                        {
                            uv.AddObserver(conn);
                        }
                        if (!uv.isClient)
                        {
                            if (LogFilter.logDev) { Debug.Log("LocalClient.SetSpawnObject calling OnStartClient"); }
                            uv.OnStartClient();
                        }
                    }
                }
                return;
            }

            // Spawn/update all current server objects
            if (LogFilter.logDebug) { Debug.Log("Spawning " + objects.Count + " objects for conn " + conn.connectionId); }

            ObjectSpawnFinishedMessage msg = new ObjectSpawnFinishedMessage();
            msg.state = 0;
            conn.Send(MsgType.SpawnFinished, msg);

            foreach (NetworkIdentity uv in objects.Values)
            {
                if (uv == null)
                {
                    if (LogFilter.logWarn) { Debug.LogWarning("Invalid object found in server local object list (null NetworkIdentity)."); }
                    continue;
                }
                if (!uv.gameObject.activeSelf)
                {
                    continue;
                }

                if (LogFilter.logDebug) { Debug.Log("Sending spawn message for current server objects name='" + uv.gameObject.name + "' netId=" + uv.netId); }

                var vis = uv.OnCheckObserver(conn);
                if (vis)
                {
                    uv.AddObserver(conn);
                }
            }

            msg.state = 1;
            conn.Send(MsgType.SpawnFinished, msg);
        }

        static internal void ShowForConnection(NetworkIdentity uv, NetworkConnection conn)
        {
            if (conn.isReady)
                instance.SendSpawnMessage(uv, conn);
        }

        static internal void HideForConnection(NetworkIdentity uv, NetworkConnection conn)
        {
            ObjectDestroyMessage msg = new ObjectDestroyMessage();
            msg.netId = uv.netId;
            conn.Send(MsgType.ObjectHide, msg);
        }

        // call this to make all the clients not ready, such as when changing levels.
        static public void SetAllClientsNotReady()
        {
            foreach (NetworkConnection conn in connections) // vis2k: foreach
            {
                if (conn != null)
                {
                    SetClientNotReady(conn);
                }
            }
        }

        static public void SetClientNotReady(NetworkConnection conn)
        {
            instance.InternalSetClientNotReady(conn);
        }

        internal void InternalSetClientNotReady(NetworkConnection conn)
        {
            if (conn.isReady)
            {
                if (LogFilter.logDebug) { Debug.Log("PlayerNotReady " + conn); }
                conn.isReady = false;
                conn.RemoveObservers();

                NotReadyMessage msg = new NotReadyMessage();
                conn.Send(MsgType.NotReady, msg);
            }
        }

        // default ready handler.
        static void OnClientReadyMessage(NetworkMessage netMsg)
        {
            if (LogFilter.logDebug) { Debug.Log("Default handler for ready message from " + netMsg.conn); }
            SetClientReady(netMsg.conn);
        }

        // default remove player handler
        static void OnRemovePlayerMessage(NetworkMessage netMsg)
        {
            // vis2k: no more static message caching. code is already too complicated as it is.
            RemovePlayerMessage msg = new RemovePlayerMessage();
            netMsg.ReadMessage(msg);

            PlayerController player = null;
            netMsg.conn.GetPlayerController(msg.playerControllerId, out player);
            if (player != null)
            {
                netMsg.conn.RemovePlayerController(msg.playerControllerId);
                Destroy(player.gameObject);
            }
            else
            {
                if (LogFilter.logError) { Debug.LogError("Received remove player message but could not find the player ID: " + msg.playerControllerId); }
            }
        }

        // Handle command from specific player, this could be one of multiple players on a single client
        static  void OnCommandMessage(NetworkMessage netMsg)
        {
            int cmdHash = (int)netMsg.reader.ReadPackedUInt32();
            var netId = netMsg.reader.ReadNetworkId();

            var cmdObject = FindLocalObject(netId);
            if (cmdObject == null)
            {
                if (LogFilter.logWarn) { Debug.LogWarning("Instance not found when handling Command message [netId=" + netId + "]"); }
                return;
            }

            var uv = cmdObject.GetComponent<NetworkIdentity>();
            if (uv == null)
            {
                if (LogFilter.logWarn) { Debug.LogWarning("NetworkIdentity deleted when handling Command message [netId=" + netId + "]"); }
                return;
            }

            // Commands can be for player objects, OR other objects with client-authority
            // vis2k: any instead of for loop and separate !foundOwner check
            if (!netMsg.conn.playerControllers.Any(
                    pc => pc.gameObject != null &&
                    pc.gameObject.GetComponent<NetworkIdentity>().netId == uv.netId))
            {
                if (uv.clientAuthorityOwner != netMsg.conn)
                {
                    if (LogFilter.logWarn) { Debug.LogWarning("Command for object without authority [netId=" + netId + "]"); }
                    return;
                }
            }

            if (LogFilter.logDev) { Debug.Log("OnCommandMessage for netId=" + netId + " conn=" + netMsg.conn); }
            uv.HandleCommand(cmdHash, netMsg.reader);
        }

        internal void SpawnObject(GameObject obj)
        {
            if (!NetworkServer.active)
            {
                if (LogFilter.logError) { Debug.LogError("SpawnObject for " + obj + ", NetworkServer is not active. Cannot spawn objects without an active server."); }
                return;
            }

            NetworkIdentity objNetworkIdentity;
            if (!GetNetworkIdentity(obj, out objNetworkIdentity))
            {
                if (LogFilter.logError) { Debug.LogError("SpawnObject " + obj + " has no NetworkIdentity. Please add a NetworkIdentity to " + obj); }
                return;
            }
            objNetworkIdentity.Reset();

            objNetworkIdentity.OnStartServer(false);

            if (LogFilter.logDebug) { Debug.Log("SpawnObject instance ID " + objNetworkIdentity.netId + " asset ID " + objNetworkIdentity.assetId); }

            objNetworkIdentity.RebuildObservers(true);
            //SendSpawnMessage(objNetworkIdentity, null);
        }

        internal void SendSpawnMessage(NetworkIdentity uv, NetworkConnection conn)
        {
            if (uv.serverOnly)
                return;

            if (LogFilter.logDebug) { Debug.Log("Server SendSpawnMessage: name=" + uv.name + " sceneId=" + uv.sceneId + " netid=" + uv.netId); }
            if (uv.sceneId.IsEmpty())
            {
                ObjectSpawnMessage msg = new ObjectSpawnMessage();
                msg.netId = uv.netId;
                msg.assetId = uv.assetId;
                msg.position = uv.transform.position;
                msg.rotation = uv.transform.rotation;

                // include synch data
                NetworkWriter writer = new NetworkWriter();
                uv.UNetSerializeAllVars(writer);
                if (writer.Position > 0)
                {
                    msg.payload = writer.ToArray();
                }

                if (conn != null)
                {
                    conn.Send(MsgType.ObjectSpawn, msg);
                }
                else
                {
                    SendToReady(uv.gameObject, MsgType.ObjectSpawn, msg);
                }

#if UNITY_EDITOR
                UnityEditor.NetworkDetailStats.IncrementStat(
                    UnityEditor.NetworkDetailStats.NetworkDirection.Outgoing,
                    MsgType.ObjectSpawn, uv.assetId.ToString(), 1);
#endif
            }
            else
            {
                ObjectSpawnSceneMessage msg = new ObjectSpawnSceneMessage();
                msg.netId = uv.netId;
                msg.sceneId = uv.sceneId;
                msg.position = uv.transform.position;

                // include synch data
                NetworkWriter writer = new NetworkWriter();
                uv.UNetSerializeAllVars(writer);
                if (writer.Position > 0)
                {
                    msg.payload = writer.ToArray();
                }

                if (conn != null)
                {
                    conn.Send(MsgType.ObjectSpawnScene, msg);
                }
                else
                {
                    SendToReady(uv.gameObject, MsgType.ObjectSpawn, msg);
                }

#if UNITY_EDITOR
                UnityEditor.NetworkDetailStats.IncrementStat(
                    UnityEditor.NetworkDetailStats.NetworkDirection.Outgoing,
                    MsgType.ObjectSpawnScene, "sceneId", 1);
#endif
            }
        }

        static public void DestroyPlayersForConnection(NetworkConnection conn)
        {
            if (conn.playerControllers.Count == 0)
            {
                if (LogFilter.logWarn) { Debug.LogWarning("Empty player list given to NetworkServer.Destroy(), nothing to do."); }
                return;
            }

            if (conn.clientOwnedObjects != null)
            {
                var tmp = new HashSet<NetworkInstanceId>(conn.clientOwnedObjects);
                foreach (var netId in tmp)
                {
                    var obj = FindLocalObject(netId);
                    if (obj != null)
                    {
                        DestroyObject(obj);
                    }
                }
            }

            foreach (PlayerController player in conn.playerControllers)
            {
                if (player.IsValid)
                {
                    if (player.unetView == null)
                    {
                        // the playerController's object has been destroyed, but RemovePlayerForConnection was never called.
                        // this is ok, just dont double destroy it.
                    }
                    else
                    {
                        DestroyObject(player.unetView, true);
                    }
                    player.gameObject = null;
                }
            }
            conn.playerControllers.Clear();
        }

        static void UnSpawnObject(GameObject obj)
        {
            if (obj == null)
            {
                if (LogFilter.logDev) { Debug.Log("NetworkServer UnspawnObject is null"); }
                return;
            }

            NetworkIdentity objNetworkIdentity;
            if (!GetNetworkIdentity(obj, out objNetworkIdentity)) return;

            UnSpawnObject(objNetworkIdentity);
        }

        static void UnSpawnObject(NetworkIdentity uv)
        {
            DestroyObject(uv, false);
        }

        static void DestroyObject(GameObject obj)
        {
            if (obj == null)
            {
                if (LogFilter.logDev) { Debug.Log("NetworkServer DestroyObject is null"); }
                return;
            }

            NetworkIdentity objNetworkIdentity;
            if (!GetNetworkIdentity(obj, out objNetworkIdentity)) return;

            DestroyObject(objNetworkIdentity, true);
        }

        static void DestroyObject(NetworkIdentity uv, bool destroyServerObject)
        {
            if (LogFilter.logDebug) { Debug.Log("DestroyObject instance:" + uv.netId); }
            if (objects.ContainsKey(uv.netId))
            {
                objects.Remove(uv.netId);
            }

            if (uv.clientAuthorityOwner != null)
            {
                uv.clientAuthorityOwner.RemoveOwnedObject(uv);
            }

#if UNITY_EDITOR
            UnityEditor.NetworkDetailStats.IncrementStat(
                UnityEditor.NetworkDetailStats.NetworkDirection.Outgoing,
                MsgType.ObjectDestroy, uv.assetId.ToString(), 1);
#endif

            ObjectDestroyMessage msg = new ObjectDestroyMessage();
            msg.netId = uv.netId;
            SendToObservers(uv.gameObject, MsgType.ObjectDestroy, msg);

            uv.ClearObservers();
            if (NetworkClient.active && instance.m_LocalClientActive)
            {
                uv.OnNetworkDestroy();
                ClientScene.SetLocalObject(msg.netId, null);
            }

            // when unspawning, dont destroy the server's object
            if (destroyServerObject)
            {
                Object.Destroy(uv.gameObject);
            }
            uv.MarkForReset();
        }

        static public void ClearLocalObjects()
        {
            objects.Clear();
        }

        static public void Spawn(GameObject obj)
        {
            if (VerifyCanSpawn(obj))  // vis2k: very instead of !verify and early return
            {
                instance.SpawnObject(obj);
            }
        }

        static bool CheckForPrefab(GameObject obj)
        {
#if UNITY_EDITOR
            return (UnityEditor.PrefabUtility.GetPrefabParent(obj) == null) && (UnityEditor.PrefabUtility.GetPrefabObject(obj) != null);
#else
            return false;
#endif
        }

        static bool VerifyCanSpawn(GameObject obj)
        {
            if (CheckForPrefab(obj))
            {
                Debug.LogErrorFormat("GameObject {0} is a prefab, it can't be spawned. This will cause errors in builds.", obj.name);
                return false;
            }

            return true;
        }

        static public Boolean SpawnWithClientAuthority(GameObject obj, GameObject player)
        {
            var uv = player.GetComponent<NetworkIdentity>();
            if (uv == null)
            {
                Debug.LogError("SpawnWithClientAuthority player object has no NetworkIdentity");
                return false;
            }

            if (uv.connectionToClient == null)
            {
                Debug.LogError("SpawnWithClientAuthority player object is not a player.");
                return false;
            }

            return SpawnWithClientAuthority(obj, uv.connectionToClient);
        }

        static public bool SpawnWithClientAuthority(GameObject obj, NetworkConnection conn)
        {
            if (!conn.isReady)
            {
                Debug.LogError("SpawnWithClientAuthority NetworkConnection is not ready!");
                return false;
            }

            Spawn(obj);

            var uv = obj.GetComponent<NetworkIdentity>();
            if (uv == null || !uv.isServer)
            {
                // spawning the object failed.
                return false;
            }

            return uv.AssignClientAuthority(conn);
        }

        static public bool SpawnWithClientAuthority(GameObject obj, NetworkHash128 assetId, NetworkConnection conn)
        {
            Spawn(obj, assetId);

            var uv = obj.GetComponent<NetworkIdentity>();
            if (uv == null || !uv.isServer)
            {
                // spawning the object failed.
                return false;
            }

            return uv.AssignClientAuthority(conn);
        }

        static public void Spawn(GameObject obj, NetworkHash128 assetId)
        {
            if (VerifyCanSpawn(obj)) // vis2k: if verify instead of !verify and early return
            {
                NetworkIdentity id;
                if (GetNetworkIdentity(obj, out id))
                {
                    id.SetDynamicAssetId(assetId);
                }
                instance.SpawnObject(obj);
            }
        }

        static public void Destroy(GameObject obj)
        {
            DestroyObject(obj);
        }

        static public void UnSpawn(GameObject obj)
        {
            UnSpawnObject(obj);
        }

        internal bool InvokeBytes(ULocalConnectionToServer conn, byte[] buffer, int numBytes, int channelId)
        {
            NetworkReader reader = new NetworkReader(buffer);

            reader.ReadInt16(); // size
            short msgType = reader.ReadInt16();

            if (handlers.ContainsKey(msgType) && m_LocalConnection != null)
            {
                // this must be invoked with the connection to the client, not the client's connection to the server
                m_LocalConnection.InvokeHandler(msgType, reader, channelId);
                return true;
            }
            return false;
        }

        // invoked for local clients
        internal bool InvokeHandlerOnServer(ULocalConnectionToServer conn, short msgType, MessageBase msg, int channelId)
        {
            if (handlers.ContainsKey(msgType) && m_LocalConnection != null)
            {
                // write the message to a local buffer
                NetworkWriter writer = new NetworkWriter();
                msg.Serialize(writer);

                // pass a reader (attached to local buffer) to handler
                NetworkReader reader = new NetworkReader(writer);

                // this must be invoked with the connection to the client, not the client's connection to the server
                m_LocalConnection.InvokeHandler(msgType, reader, channelId);
                return true;
            }
            if (LogFilter.logError) { Debug.LogError("Local invoke: Failed to find local connection to invoke handler on [connectionId=" + conn.connectionId + "] for MsgId:" + msgType); }
            return false;
        }

        static public GameObject FindLocalObject(NetworkInstanceId netId)
        {
            return instance.m_NetworkScene.FindLocalObject(netId);
        }

        static public Dictionary<short, NetworkConnection.PacketStat> GetConnectionStats()
        {
            Dictionary<short, NetworkConnection.PacketStat> stats = new Dictionary<short, NetworkConnection.PacketStat>();

            foreach (NetworkConnection conn in connections) // vis2k: foreach
            {
                if (conn != null)
                {
                    foreach (short k in conn.packetStats.Keys)
                    {
                        if (stats.ContainsKey(k))
                        {
                            NetworkConnection.PacketStat s = stats[k];
                            s.count += conn.packetStats[k].count;
                            s.bytes += conn.packetStats[k].bytes;
                            stats[k] = s;
                        }
                        else
                        {
                            stats[k] = new NetworkConnection.PacketStat(conn.packetStats[k]);
                        }
                    }
                }
            }
            return stats;
        }

        static public void ResetConnectionStats()
        {
            foreach (NetworkConnection conn in connections) // vis2k: foreach
            {
                if (conn != null)
                {
                    conn.ResetStats();
                }
            }
        }

        static public bool AddExternalConnection(NetworkConnection conn)
        {
            return instance.AddExternalConnectionInternal(conn);
        }

        bool AddExternalConnectionInternal(NetworkConnection conn)
        {
            if (conn.connectionId < 0)
                return false;

            if (conn.connectionId < connections.Count && connections[conn.connectionId] != null)
            {
                if (LogFilter.logError) { Debug.LogError("AddExternalConnection failed, already connection for id:" + conn.connectionId); }
                return false;
            }

            if (LogFilter.logDebug) { Debug.Log("AddExternalConnection external connection " + conn.connectionId); }
            m_SimpleServerSimple.SetConnectionAtIndex(conn);
            m_ExternalConnections.Add(conn.connectionId);
            conn.InvokeHandlerNoData(MsgType.Connect);

            return true;
        }

        static public void RemoveExternalConnection(int connectionId)
        {
            instance.RemoveExternalConnectionInternal(connectionId);
        }

        bool RemoveExternalConnectionInternal(int connectionId)
        {
            if (!m_ExternalConnections.Contains(connectionId))
            {
                if (LogFilter.logError) { Debug.LogError("RemoveExternalConnection failed, no connection for id:" + connectionId); }
                return false;
            }
            if (LogFilter.logDebug) { Debug.Log("RemoveExternalConnection external connection " + connectionId); }

            var conn = m_SimpleServerSimple.FindConnection(connectionId);
            if (conn != null)
            {
                conn.RemoveObservers();
            }
            m_SimpleServerSimple.RemoveConnectionAtIndex(connectionId);

            return true;
        }

        static public bool SpawnObjects()
        {
            if (active)
            {
                NetworkIdentity[] uvs = Resources.FindObjectsOfTypeAll<NetworkIdentity>();
                foreach (NetworkIdentity uv in uvs) // vis2k: foreach
                {
                    if (uv.gameObject.hideFlags == HideFlags.NotEditable || uv.gameObject.hideFlags == HideFlags.HideAndDontSave)
                        continue;

                    if (uv.sceneId.IsEmpty())
                        continue;

                    if (LogFilter.logDebug) { Debug.Log("SpawnObjects sceneId:" + uv.sceneId + " name:" + uv.gameObject.name); }
                    uv.Reset();
                    uv.gameObject.SetActive(true);
                }
                foreach (NetworkIdentity uv in uvs) // vis2k: foreach
                {
                    if (uv.gameObject.hideFlags == HideFlags.NotEditable || uv.gameObject.hideFlags == HideFlags.HideAndDontSave)
                        continue;

                    // If not a scene object
                    if (uv.sceneId.IsEmpty())
                        continue;

                    // What does this mean???
                    if (uv.isServer)
                        continue;

                    // Log this? Something is wrong if this happens? Dangling UnetView?
                    if (uv.gameObject == null)
                        continue;

                    Spawn(uv.gameObject);

                    // these objects are server authority - even if "localPlayerAuthority" is set on them
                    uv.ForceAuthority(true);
                }
            }
            return true;
        }

        static void SendCrc(NetworkConnection targetConnection)
        {
            if (NetworkCRC.singleton == null)
                return;

            if (NetworkCRC.scriptCRCCheck == false)
                return;

            CRCMessage crcMsg = new CRCMessage();

            // build entries
            List<CRCMessageEntry> entries = new List<CRCMessageEntry>();
            foreach (var name in NetworkCRC.singleton.scripts.Keys)
            {
                CRCMessageEntry entry = new CRCMessageEntry();
                entry.name = name;
                entry.channel = (byte)NetworkCRC.singleton.scripts[name];
                entries.Add(entry);
            }
            crcMsg.scripts = entries.ToArray();

            targetConnection.Send(MsgType.CRC, crcMsg);
        }

#if ENABLE_UNET_HOST_MIGRATION
        [Obsolete("moved to NetworkMigrationManager")]
#else
        [Obsolete("Removed")]
#endif
        public void SendNetworkInfo(NetworkConnection targetConnection)
        {
        }

        class ServerSimpleWrapper : NetworkServerSimple
        {
            NetworkServer m_Server;
            public ServerSimpleWrapper(NetworkServer server)
            {
                m_Server = server;
            }

            public override void OnConnectError(int connectionId, byte error)
            {
                m_Server.GenerateConnectError(error);
            }

            public override void OnDataError(NetworkConnection conn, byte error)
            {
                m_Server.GenerateDataError(conn, error);
            }

            public override void OnDisconnectError(NetworkConnection conn, byte error)
            {
                m_Server.GenerateDisconnectError(conn, error);
            }

            public override void OnConnected(NetworkConnection conn)
            {
                m_Server.OnConnected(conn);
            }

            public override void OnDisconnected(NetworkConnection conn)
            {
                m_Server.OnDisconnected(conn);
            }

            public override void OnData(NetworkConnection conn, int receivedSize, int channelId)
            {
                m_Server.OnData(conn, receivedSize, channelId);
            }
        }
    };
}
#endif //ENABLE_UNET
