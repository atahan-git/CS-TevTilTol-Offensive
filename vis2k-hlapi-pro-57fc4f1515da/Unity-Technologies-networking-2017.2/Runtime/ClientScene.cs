#if ENABLE_UNET
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking.NetworkSystem;

namespace UnityEngine.Networking
{
    public class ClientScene
    {
        public static List<PlayerController> localPlayers = new List<PlayerController>(); // vis2k: public instead of get/set wrapper
        public static NetworkConnection readyConnection { get; private set; } // vis2k: private set instead of private variable + get wrapper
        static Dictionary<NetworkSceneId, NetworkIdentity> s_SpawnableObjects;

        public static bool ready { get; private set; } // vis2k: private set instead of private variable + get wrapper
        static bool s_IsSpawnFinished;
        static NetworkScene s_NetworkScene = new NetworkScene();

#if ENABLE_UNET_HOST_MIGRATION
        public const int ReconnectIdInvalid = -1;
        public const int ReconnectIdHost = 0;
        static int s_ReconnectId = ReconnectIdInvalid;
        static PeerInfoMessage[] s_Peers;
        static bool hasMigrationPending() { return s_ReconnectId != ReconnectIdInvalid; }

        static public void SetReconnectId(int newReconnectId, PeerInfoMessage[] peers)
        {
            s_ReconnectId = newReconnectId;
            s_Peers = peers;

            if (LogFilter.logDebug) { Debug.Log("ClientScene::SetReconnectId: " + newReconnectId); }
        }
#endif

        static internal void SetNotReady()
        {
            ready = false;
        }

        struct PendingOwner
        {
            public NetworkInstanceId netId;
            public short playerControllerId;
        }
        static List<PendingOwner> s_PendingOwnerIds = new List<PendingOwner>();


#if ENABLE_UNET_HOST_MIGRATION
        public static int reconnectId { get { return s_ReconnectId; } }
#endif

        //NOTE: spawn handlers, prefabs and local objects now live in NetworkScene
        public static Dictionary<NetworkInstanceId, NetworkIdentity> objects { get { return s_NetworkScene.localObjects; } }
        public static Dictionary<NetworkHash128, GameObject> prefabs { get { return NetworkScene.guidToPrefab; } }
        public static Dictionary<NetworkSceneId, NetworkIdentity> spawnableObjects { get { return s_SpawnableObjects; } }

        internal static void Shutdown()
        {
            s_NetworkScene.Shutdown();
            localPlayers = new List<PlayerController>();
            s_PendingOwnerIds = new List<PendingOwner>();
            s_SpawnableObjects = null;
            readyConnection = null;
            ready = false;
            s_IsSpawnFinished = false;
#if ENABLE_UNET_HOST_MIGRATION
            s_ReconnectId = ReconnectIdInvalid;
#endif
            NetworkTransport.Shutdown();
            NetworkTransport.Init();
        }

        internal static bool GetPlayerController(short playerControllerId, out PlayerController player)
        {
            player = null;
            if (playerControllerId >= localPlayers.Count)
            {
                if (LogFilter.logWarn) { Debug.Log("ClientScene::GetPlayer: no local player found for: " + playerControllerId); }
                return false;
            }

            if (localPlayers[playerControllerId] == null)
            {
                if (LogFilter.logWarn) { Debug.LogWarning("ClientScene::GetPlayer: local player is null for: " + playerControllerId); }
                return false;
            }
            player = localPlayers[playerControllerId];
            return player.gameObject != null;
        }

        // this is called from message handler for Owner message
        internal static void InternalAddPlayer(NetworkIdentity view, short playerControllerId)
        {
            if (LogFilter.logDebug) { Debug.LogWarning("ClientScene::InternalAddPlayer: playerControllerId : " + playerControllerId); }

            if (playerControllerId >= localPlayers.Count)
            {
                if (LogFilter.logWarn) { Debug.LogWarning("ClientScene::InternalAddPlayer: playerControllerId higher than expected: " + playerControllerId); }
                while (playerControllerId >= localPlayers.Count)
                {
                    localPlayers.Add(new PlayerController());
                }
            }

            // NOTE: It can be "normal" when changing scenes for the player to be destroyed and recreated.
            // But, the player structures are not cleaned up, we'll just replace the old player
            var newPlayer = new PlayerController {gameObject = view.gameObject, playerControllerId = playerControllerId, unetView = view};
            localPlayers[playerControllerId] = newPlayer;
            readyConnection.SetPlayerController(newPlayer);
        }

        // use this if already ready
        public static bool AddPlayer(short playerControllerId)
        {
            return AddPlayer(null, playerControllerId);
        }

        // use this to implicitly become ready
        public static bool AddPlayer(NetworkConnection readyConn, short playerControllerId)
        {
            return AddPlayer(readyConn, playerControllerId, null);
        }

        // use this to implicitly become ready
        public static bool AddPlayer(NetworkConnection readyConn, short playerControllerId, MessageBase extraMessage)
        {
            if (playerControllerId < 0)
            {
                if (LogFilter.logError) { Debug.LogError("ClientScene::AddPlayer: playerControllerId of " + playerControllerId + " is negative"); }
                return false;
            }
            if (playerControllerId > PlayerController.MaxPlayersPerClient)
            {
                if (LogFilter.logError) { Debug.LogError("ClientScene::AddPlayer: playerControllerId of " + playerControllerId + " is too high, max is " + PlayerController.MaxPlayersPerClient); }
                return false;
            }
            if (playerControllerId > PlayerController.MaxPlayersPerClient / 2)
            {
                if (LogFilter.logWarn) { Debug.LogWarning("ClientScene::AddPlayer: playerControllerId of " + playerControllerId + " is unusually high"); }
            }

            // fill out local players array
            while (playerControllerId >= localPlayers.Count)
            {
                localPlayers.Add(new PlayerController());
            }

            // ensure valid ready connection
            if (readyConn == null)
            {
                if (!ready)
                {
                    if (LogFilter.logError) { Debug.LogError("Must call AddPlayer() with a connection the first time to become ready."); }
                    return false;
                }
            }
            else
            {
                ready = true;
                readyConnection = readyConn;
            }

            PlayerController existingPlayerController;
            if (readyConnection.GetPlayerController(playerControllerId, out existingPlayerController))
            {
                if (existingPlayerController.IsValid && existingPlayerController.gameObject != null)
                {
                    if (LogFilter.logError) { Debug.LogError("ClientScene::AddPlayer: playerControllerId of " + playerControllerId + " already in use."); }
                    return false;
                }
            }

            if (LogFilter.logDebug) { Debug.Log("ClientScene::AddPlayer() for ID " + playerControllerId + " called with connection [" + readyConnection + "]"); }

#if ENABLE_UNET_HOST_MIGRATION
            if (!hasMigrationPending())
            {
#endif
                var msg = new AddPlayerMessage();
                msg.playerControllerId = playerControllerId;
                if (extraMessage != null)
                {
                    var writer = new NetworkWriter();
                    extraMessage.Serialize(writer);
                    msg.msgData = writer.ToArray();
                    msg.msgSize = writer.Position;
                }
                readyConnection.Send(MsgType.AddPlayer, msg);
#if ENABLE_UNET_HOST_MIGRATION
            }
            else
            {
                return SendReconnectMessage(extraMessage);
            }
#endif
            return true;
        }

#if ENABLE_UNET_HOST_MIGRATION
        public static bool SendReconnectMessage(MessageBase extraMessage)
        {
            if (!hasMigrationPending())
                return false;

            if (LogFilter.logDebug) { Debug.Log("ClientScene::AddPlayer reconnect " + s_ReconnectId);           }

            if (s_Peers == null)
            {
                SetReconnectId(ReconnectIdInvalid, null);
                if (LogFilter.logError)
                {
                    Debug.LogError("ClientScene::AddPlayer: reconnecting, but no peers.");
                }
                return false;
            }

            // reconnect all the players
            // vis2k: used foreach and improved syntax
            foreach (PeerInfoMessage peer in s_Peers)
            {
                if (peer.playerIds != null && peer.connectionId == s_ReconnectId)
                {
                    foreach (PeerInfoPlayer playerId in peer.playerIds)
                    {
                        var msg = new ReconnectMessage();
                        msg.oldConnectionId = s_ReconnectId;
                        msg.netId = playerId.netId;
                        msg.playerControllerId = playerId.playerControllerId;
                        if (extraMessage != null)
                        {
                            var writer = new NetworkWriter();
                            extraMessage.Serialize(writer);
                            msg.msgData = writer.ToArray();
                            msg.msgSize = writer.Position;
                        }

                        readyConnection.Send(MsgType.ReconnectPlayer, msg);
                    }
                }
            }
            // this should only be done once.
            SetReconnectId(ReconnectIdInvalid, null);
            return true;
        }
#endif

        public static bool RemovePlayer(short playerControllerId)
        {
            if (LogFilter.logDebug) { Debug.Log("ClientScene::RemovePlayer() for ID " + playerControllerId + " called with connection [" + readyConnection + "]"); }

            PlayerController playerController;
            if (readyConnection.GetPlayerController(playerControllerId, out playerController))
            {
                var msg = new RemovePlayerMessage();
                msg.playerControllerId = playerControllerId;
                readyConnection.Send(MsgType.RemovePlayer, msg);

                readyConnection.RemovePlayerController(playerControllerId);
                localPlayers[playerControllerId] = new PlayerController();

                Object.Destroy(playerController.gameObject);
                return true;
            }
            if (LogFilter.logError) { Debug.LogError("Failed to find player ID " + playerControllerId); }
            return false;
        }

        public static bool Ready(NetworkConnection conn)
        {
            if (ready)
            {
                if (LogFilter.logError) { Debug.LogError("A connection has already been set as ready. There can only be one."); }
                return false;
            }
            if (LogFilter.logDebug) { Debug.Log("ClientScene::Ready() called with connection [" + conn + "]"); }

            if (conn != null)
            {
                var msg = new ReadyMessage();
                conn.Send(MsgType.Ready, msg);
                ready = true;
                readyConnection = conn;
                readyConnection.isReady = true;
                return true;
            }
            if (LogFilter.logError) { Debug.LogError("Ready() called with invalid connection object: conn=null"); }
            return false;
        }

        static public NetworkClient ConnectLocalServer()
        {
            var newClient = new LocalClient();
            NetworkServer.instance.ActivateLocalClientScene();
            newClient.InternalConnectLocalServer(true);
            return newClient;
        }

#if ENABLE_UNET_HOST_MIGRATION
        static internal NetworkClient ReconnectLocalServer()
        {
            LocalClient newClient = new LocalClient();
            NetworkServer.instance.ActivateLocalClientScene();
            newClient.InternalConnectLocalServer(false);
            return newClient;
        }
#endif
        static internal void HandleClientDisconnect(NetworkConnection conn)
        {
            if (readyConnection == conn && ready)
            {
                ready = false;
                readyConnection = null;
            }
        }

        internal static void PrepareToSpawnSceneObjects()
        {
            //NOTE: what if there are already objects in this dict?! should we merge with them?
            s_SpawnableObjects = new Dictionary<NetworkSceneId, NetworkIdentity>();

            // vis2k: loop simplified
            foreach (NetworkIdentity uv in Resources.FindObjectsOfTypeAll<NetworkIdentity>())
            {
                // not active yet, not hidden, sceneid not empty?
                if (!uv.gameObject.activeSelf && 
                    uv.gameObject.hideFlags != HideFlags.NotEditable &&
                    uv.gameObject.hideFlags != HideFlags.HideAndDontSave &&
                    !uv.sceneId.IsEmpty())
                {
                    s_SpawnableObjects[uv.sceneId] = uv;
                    if (LogFilter.logDebug) { Debug.Log("ClientScene::PrepareSpawnObjects sceneId:" + uv.sceneId); }
                }
            }
        }

        internal static NetworkIdentity SpawnSceneObject(NetworkSceneId sceneId)
        {
            if (s_SpawnableObjects.ContainsKey(sceneId))
            {
                NetworkIdentity foundId = s_SpawnableObjects[sceneId];
                s_SpawnableObjects.Remove(sceneId);
                return foundId;
            }
            return null;
        }

        static internal void RegisterSystemHandlers(NetworkClient client, bool localClient)
        {
            if (localClient)
            {
                client.RegisterHandlerSafe(MsgType.ObjectDestroy, OnLocalClientObjectDestroy);
                client.RegisterHandlerSafe(MsgType.ObjectHide, OnLocalClientObjectHide);
                client.RegisterHandlerSafe(MsgType.ObjectSpawn, OnLocalClientObjectSpawn);
                client.RegisterHandlerSafe(MsgType.ObjectSpawnScene, OnLocalClientObjectSpawnScene);
                client.RegisterHandlerSafe(MsgType.LocalClientAuthority, OnClientAuthority);
            }
            else
            {
                // LocalClient shares the sim/scene with the server, no need for these events
                client.RegisterHandlerSafe(MsgType.ObjectSpawn, OnObjectSpawn);
                client.RegisterHandlerSafe(MsgType.ObjectSpawnScene, OnObjectSpawnScene);
                client.RegisterHandlerSafe(MsgType.SpawnFinished, OnObjectSpawnFinished);
                client.RegisterHandlerSafe(MsgType.ObjectDestroy, OnObjectDestroy);
                client.RegisterHandlerSafe(MsgType.ObjectHide, OnObjectDestroy);
                client.RegisterHandlerSafe(MsgType.UpdateVars, OnUpdateVarsMessage);
                client.RegisterHandlerSafe(MsgType.Owner, OnOwnerMessage);
                client.RegisterHandlerSafe(MsgType.SyncList, OnSyncListMessage);
                client.RegisterHandlerSafe(MsgType.Animation, NetworkAnimator.OnAnimationClientMessage);
                client.RegisterHandlerSafe(MsgType.AnimationParameters, NetworkAnimator.OnAnimationParametersClientMessage);
                client.RegisterHandlerSafe(MsgType.LocalClientAuthority, OnClientAuthority);
            }

            client.RegisterHandlerSafe(MsgType.Rpc, OnRPCMessage);
            client.RegisterHandlerSafe(MsgType.SyncEvent, OnSyncEventMessage);
            client.RegisterHandlerSafe(MsgType.AnimationTrigger, NetworkAnimator.OnAnimationTriggerClientMessage);
        }

        // ------------------------ NetworkScene pass-throughs ---------------------

        static internal string GetStringForAssetId(NetworkHash128 assetId)
        {
            GameObject prefab;
            if (NetworkScene.GetPrefab(assetId, out prefab))
            {
                return prefab.name;
            }

            SpawnDelegate handler;
            if (NetworkScene.GetSpawnHandler(assetId, out handler))
            {
                return handler.GetMethodName();
            }

            return "unknown";
        }

        // this assigns the newAssetId to the prefab. This is for registering dynamically created game objects for already know assetIds.
        static public void RegisterPrefab(GameObject prefab, NetworkHash128 newAssetId)
        {
            NetworkScene.RegisterPrefab(prefab, newAssetId);
        }

        static public void RegisterPrefab(GameObject prefab)
        {
            NetworkScene.RegisterPrefab(prefab);
        }

        static public void RegisterPrefab(GameObject prefab, SpawnDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
        {
            NetworkScene.RegisterPrefab(prefab, spawnHandler, unspawnHandler);
        }

        static public void UnregisterPrefab(GameObject prefab)
        {
            NetworkScene.UnregisterPrefab(prefab);
        }

        static public void RegisterSpawnHandler(NetworkHash128 assetId, SpawnDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
        {
            NetworkScene.RegisterSpawnHandler(assetId, spawnHandler, unspawnHandler);
        }

        static public void UnregisterSpawnHandler(NetworkHash128 assetId)
        {
            NetworkScene.UnregisterSpawnHandler(assetId);
        }

        static public void ClearSpawners()
        {
            NetworkScene.ClearSpawners();
        }

        static public void DestroyAllClientObjects()
        {
            s_NetworkScene.DestroyAllClientObjects();
        }

        static public void SetLocalObject(NetworkInstanceId netId, GameObject obj)
        {
            // if still receiving initial state, dont set isClient
            s_NetworkScene.SetLocalObject(netId, obj, s_IsSpawnFinished, false);
        }

        static public GameObject FindLocalObject(NetworkInstanceId netId)
        {
            return s_NetworkScene.FindLocalObject(netId);
        }

        static void ApplySpawnPayload(NetworkIdentity uv, Vector3 position, byte[] payload, NetworkInstanceId netId, GameObject newGameObject)
        {
            if (!uv.gameObject.activeSelf)
            {
                uv.gameObject.SetActive(true);
            }
            uv.transform.position = position;
            if (payload != null && payload.Length > 0)
            {
                var payloadReader = new NetworkReader(payload);
                uv.OnUpdateVars(payloadReader, true);
            }
            if (newGameObject == null)
            {
                return;
            }

            newGameObject.SetActive(true);
            uv.SetNetworkInstanceId(netId);
            SetLocalObject(netId, newGameObject);

            // objects spawned as part of initial state are started on a second pass
            if (s_IsSpawnFinished)
            {
                uv.OnStartClient();
                CheckForOwner(uv);
            }
        }

        static void OnObjectSpawn(NetworkMessage netMsg)
        {
            // vis2k: no more static message caching. code is already too complicated as it is.
            ObjectSpawnMessage msg = new ObjectSpawnMessage();
            netMsg.ReadMessage(msg);

            if (!msg.assetId.IsValid())
            {
                if (LogFilter.logError) { Debug.LogError("OnObjSpawn netId: " + msg.netId + " has invalid asset Id"); }
                return;
            }
            if (LogFilter.logDebug) { Debug.Log("Client spawn handler instantiating [netId:" + msg.netId + " asset ID:" + msg.assetId + " pos:" + msg.position + "]"); }

#if UNITY_EDITOR
            UnityEditor.NetworkDetailStats.IncrementStat(
                UnityEditor.NetworkDetailStats.NetworkDirection.Incoming,
                MsgType.ObjectSpawn, GetStringForAssetId(msg.assetId), 1);
#endif

            NetworkIdentity localNetworkIdentity;
            if (s_NetworkScene.GetNetworkIdentity(msg.netId, out localNetworkIdentity))
            {
                // this object already exists (was in the scene), just apply the update to existing object
                ApplySpawnPayload(localNetworkIdentity, msg.position, msg.payload, msg.netId, null);
                return;
            }

            GameObject prefab;
            SpawnDelegate handler;
            if (NetworkScene.GetPrefab(msg.assetId, out prefab))
            {
                var obj = (GameObject)Object.Instantiate(prefab, msg.position, msg.rotation);
                if (LogFilter.logDebug)
                {
                    Debug.Log("Client spawn handler instantiating [netId:" + msg.netId + " asset ID:" + msg.assetId + " pos:" + msg.position + " rotation: " + msg.rotation + "]");
                }

                localNetworkIdentity = obj.GetComponent<NetworkIdentity>();
                if (localNetworkIdentity == null)
                {
                    if (LogFilter.logError) { Debug.LogError("Client object spawned for " + msg.assetId + " does not have a NetworkIdentity"); }
                    return;
                }
                localNetworkIdentity.Reset();
                ApplySpawnPayload(localNetworkIdentity, msg.position, msg.payload, msg.netId, obj);
            }
            // lookup registered factory for type:
            else if (NetworkScene.GetSpawnHandler(msg.assetId, out handler))
            {
                GameObject obj = handler(msg.position, msg.assetId);
                if (obj == null)
                {
                    if (LogFilter.logWarn) { Debug.LogWarning("Client spawn handler for " + msg.assetId + " returned null"); }
                    return;
                }
                localNetworkIdentity = obj.GetComponent<NetworkIdentity>();
                if (localNetworkIdentity == null)
                {
                    if (LogFilter.logError) { Debug.LogError("Client object spawned for " + msg.assetId + " does not have a network identity"); }
                    return;
                }
                localNetworkIdentity.Reset();
                localNetworkIdentity.SetDynamicAssetId(msg.assetId);
                ApplySpawnPayload(localNetworkIdentity, msg.position, msg.payload, msg.netId, obj);
            }
            else
            {
                if (LogFilter.logError) { Debug.LogError("Failed to spawn server object, did you forget to add it to the NetworkManager? assetId=" + msg.assetId + " netId=" + msg.netId); }
            }
        }

        static void OnObjectSpawnScene(NetworkMessage netMsg)
        {
            // vis2k: no more static message caching. code is already too complicated as it is.
            ObjectSpawnSceneMessage msg = new ObjectSpawnSceneMessage();
            netMsg.ReadMessage(msg);

            if (LogFilter.logDebug) { Debug.Log("Client spawn scene handler instantiating [netId:" + msg.netId + " sceneId:" + msg.sceneId + " pos:" + msg.position); }


#if UNITY_EDITOR
            UnityEditor.NetworkDetailStats.IncrementStat(
                UnityEditor.NetworkDetailStats.NetworkDirection.Incoming,
                MsgType.ObjectSpawnScene, "sceneId", 1);
#endif

            NetworkIdentity localNetworkIdentity;
            if (s_NetworkScene.GetNetworkIdentity(msg.netId, out localNetworkIdentity))
            {
                // this object already exists (was in the scene)
                ApplySpawnPayload(localNetworkIdentity, msg.position, msg.payload, msg.netId, localNetworkIdentity.gameObject);
                return;
            }

            NetworkIdentity spawnedId = SpawnSceneObject(msg.sceneId);
            if (spawnedId == null)
            {
                if (LogFilter.logError)
                {
                    Debug.LogError("Spawn scene object not found for " + msg.sceneId + " SpawnableObjects.Count=" + s_SpawnableObjects.Count);
                    // vis2k: dump the whole spawnable objects dict for easier debugging
                    foreach (var kvp in s_SpawnableObjects)
                        Debug.Log("Spawnable: SceneId=" + kvp.Key + " name=" + kvp.Value.name);
                }
                return;
            }

            if (LogFilter.logDebug) { Debug.Log("Client spawn for [netId:" + msg.netId + "] [sceneId:" + msg.sceneId + "] obj:" + spawnedId.gameObject.name); }
            ApplySpawnPayload(spawnedId, msg.position, msg.payload, msg.netId, spawnedId.gameObject);
        }

        static void OnObjectSpawnFinished(NetworkMessage netMsg)
        {
            // vis2k: no more static message caching. code is already too complicated as it is.
            ObjectSpawnFinishedMessage msg = new ObjectSpawnFinishedMessage();
            netMsg.ReadMessage(msg);
            if (LogFilter.logDebug) { Debug.Log("SpawnFinished:" + msg.state); }

            if (msg.state == 0)
            {
                PrepareToSpawnSceneObjects();
                s_IsSpawnFinished = false;
                return;
            }

            foreach (var uv in objects.Values)
            {
                if (!uv.isClient)
                {
                    uv.OnStartClient();
                    CheckForOwner(uv);
                }
            }
            s_IsSpawnFinished = true;
        }

        static void OnObjectDestroy(NetworkMessage netMsg)
        {
            // vis2k: no more static message caching. code is already too complicated as it is.
            ObjectDestroyMessage msg = new ObjectDestroyMessage();
            netMsg.ReadMessage(msg);
            if (LogFilter.logDebug) { Debug.Log("ClientScene::OnObjDestroy netId:" + msg.netId); }

            NetworkIdentity localObject;
            if (s_NetworkScene.GetNetworkIdentity(msg.netId, out localObject))
            {
#if UNITY_EDITOR
                UnityEditor.NetworkDetailStats.IncrementStat(
                    UnityEditor.NetworkDetailStats.NetworkDirection.Incoming,
                    MsgType.ObjectDestroy, GetStringForAssetId(localObject.assetId), 1);
#endif
                localObject.OnNetworkDestroy();

                if (!NetworkScene.InvokeUnSpawnHandler(localObject.assetId, localObject.gameObject))
                {
                    // default handling
                    if (localObject.sceneId.IsEmpty())
                    {
                        Object.Destroy(localObject.gameObject);
                    }
                    else
                    {
                        // scene object.. disable it in scene instead of destroying
                        localObject.gameObject.SetActive(false);
                        s_SpawnableObjects[localObject.sceneId] = localObject;
                    }
                }
                s_NetworkScene.RemoveLocalObject(msg.netId);
                localObject.MarkForReset();
            }
            else
            {
                if (LogFilter.logDebug) { Debug.LogWarning("Did not find target for destroy message for " + msg.netId); }
            }
        }

        static void OnLocalClientObjectDestroy(NetworkMessage netMsg)
        {
            // vis2k: no more static message caching. code is already too complicated as it is.
            ObjectDestroyMessage msg = new ObjectDestroyMessage();
            netMsg.ReadMessage(msg);
            if (LogFilter.logDebug) { Debug.Log("ClientScene::OnLocalObjectObjDestroy netId:" + msg.netId); }

            s_NetworkScene.RemoveLocalObject(msg.netId);
        }

        static void OnLocalClientObjectHide(NetworkMessage netMsg)
        {
            // vis2k: no more static message caching. code is already too complicated as it is.
            ObjectDestroyMessage msg = new ObjectDestroyMessage();
            netMsg.ReadMessage(msg);
            if (LogFilter.logDebug) { Debug.Log("ClientScene::OnLocalObjectObjHide netId:" + msg.netId); }

            NetworkIdentity localObject;
            if (s_NetworkScene.GetNetworkIdentity(msg.netId, out localObject))
            {
                localObject.OnSetLocalVisibility(false);
            }
        }

        static void OnLocalClientObjectSpawn(NetworkMessage netMsg)
        {
            // vis2k: no more static message caching. code is already too complicated as it is.
            ObjectSpawnMessage msg = new ObjectSpawnMessage();
            netMsg.ReadMessage(msg);

            NetworkIdentity localObject;
            if (s_NetworkScene.GetNetworkIdentity(msg.netId, out localObject))
            {
                localObject.OnSetLocalVisibility(true);
            }
        }

        static void OnLocalClientObjectSpawnScene(NetworkMessage netMsg)
        {
            // vis2k: no more static message caching. code is already too complicated as it is.
            ObjectSpawnSceneMessage msg = new ObjectSpawnSceneMessage();
            netMsg.ReadMessage(msg);

            NetworkIdentity localObject;
            if (s_NetworkScene.GetNetworkIdentity(msg.netId, out localObject))
            {
                localObject.OnSetLocalVisibility(true);
            }
        }

        static void OnUpdateVarsMessage(NetworkMessage netMsg)
        {
            NetworkInstanceId netId = netMsg.reader.ReadNetworkId();
            if (LogFilter.logDev) { Debug.Log("ClientScene::OnUpdateVarsMessage " + netId + " channel:" + netMsg.channelId); }

            NetworkIdentity localObject;
            if (s_NetworkScene.GetNetworkIdentity(netId, out localObject))
            {
                localObject.OnUpdateVars(netMsg.reader, false);
            }
            else
            {
                // vis2k: note that this happens sometimes
                if (LogFilter.logWarn) { Debug.LogWarning("Did not find target for sync message for " + netId + " . Note: this can be completely normal because UDP messages may arrive out of order, so this message might have arrived after a Destroy message."); }
            }
        }

        static void OnRPCMessage(NetworkMessage netMsg)
        {
            var cmdHash = (int)netMsg.reader.ReadPackedUInt32();
            var netId = netMsg.reader.ReadNetworkId();

            if (LogFilter.logDebug) { Debug.Log("ClientScene::OnRPCMessage hash:" + cmdHash + " netId:" + netId); }

            NetworkIdentity uv;
            if (s_NetworkScene.GetNetworkIdentity(netId, out uv))
            {
                uv.HandleRPC(cmdHash, netMsg.reader);
            }
            else
            {
                if (LogFilter.logWarn)
                {
                    string errorCmdName = NetworkBehaviour.GetCmdHashHandlerName(cmdHash);
                    Debug.LogWarningFormat("Could not find target object with netId:{0} for RPC call {1}", netId, errorCmdName);
                }
            }
        }

        static void OnSyncEventMessage(NetworkMessage netMsg)
        {
            var cmdHash = (int)netMsg.reader.ReadPackedUInt32();
            var netId = netMsg.reader.ReadNetworkId();

            if (LogFilter.logDebug) { Debug.Log("ClientScene::OnSyncEventMessage " + netId); }

            NetworkIdentity uv;
            if (s_NetworkScene.GetNetworkIdentity(netId, out uv))
            {
                uv.HandleSyncEvent(cmdHash, netMsg.reader);
            }
            else
            {
                if (LogFilter.logWarn) { Debug.LogWarning("Did not find target for SyncEvent message for " + netId); }
            }

#if UNITY_EDITOR
            UnityEditor.NetworkDetailStats.IncrementStat(
                UnityEditor.NetworkDetailStats.NetworkDirection.Outgoing,
                MsgType.SyncEvent, NetworkBehaviour.GetCmdHashHandlerName(cmdHash), 1);
#endif
        }

        static void OnSyncListMessage(NetworkMessage netMsg)
        {
            var netId = netMsg.reader.ReadNetworkId();
            var cmdHash = (int)netMsg.reader.ReadPackedUInt32();

            if (LogFilter.logDebug) { Debug.Log("ClientScene::OnSyncListMessage " + netId); }

            NetworkIdentity uv;
            if (s_NetworkScene.GetNetworkIdentity(netId, out uv))
            {
                uv.HandleSyncList(cmdHash, netMsg.reader);
            }
            else
            {
                if (LogFilter.logWarn) { Debug.LogWarning("Did not find target for SyncList message for " + netId); }
            }

#if UNITY_EDITOR
            UnityEditor.NetworkDetailStats.IncrementStat(
                UnityEditor.NetworkDetailStats.NetworkDirection.Outgoing,
                MsgType.SyncList, NetworkBehaviour.GetCmdHashHandlerName(cmdHash), 1);
#endif
        }

        static void OnClientAuthority(NetworkMessage netMsg)
        {
            // vis2k: no more static message caching. code is already too complicated as it is.
            ClientAuthorityMessage msg = new ClientAuthorityMessage();
            netMsg.ReadMessage(msg);

            if (LogFilter.logDebug) { Debug.Log("ClientScene::OnClientAuthority for  connectionId=" + netMsg.conn.connectionId + " netId: " + msg.netId); }

            NetworkIdentity uv;
            if (s_NetworkScene.GetNetworkIdentity(msg.netId, out uv))
            {
                uv.HandleClientAuthority(msg.authority);
            }
        }

        // OnClientAddedPlayer?
        static void OnOwnerMessage(NetworkMessage netMsg)
        {
            // vis2k: no more static message caching. code is already too complicated as it is.
            OwnerMessage msg = new OwnerMessage();
            netMsg.ReadMessage(msg);

            if (LogFilter.logDebug) { Debug.Log("ClientScene::OnOwnerMessage - connectionId=" + netMsg.conn.connectionId + " netId: " + msg.netId); }


            // is there already an owner that is a different object??
            PlayerController oldOwner;
            if (netMsg.conn.GetPlayerController(msg.playerControllerId, out oldOwner))
            {
                oldOwner.unetView.SetNotLocalPlayer();
            }

            NetworkIdentity localNetworkIdentity;
            if (s_NetworkScene.GetNetworkIdentity(msg.netId, out localNetworkIdentity))
            {
                // this object already exists
                localNetworkIdentity.SetConnectionToServer(netMsg.conn);
                localNetworkIdentity.SetLocalPlayer(msg.playerControllerId);
                InternalAddPlayer(localNetworkIdentity, msg.playerControllerId);
            }
            else
            {
                var pendingOwner = new PendingOwner { netId = msg.netId, playerControllerId = msg.playerControllerId };
                s_PendingOwnerIds.Add(pendingOwner);
            }
        }

        static void CheckForOwner(NetworkIdentity uv)
        {
            // vis2k: linq instead of for loop
            int index = s_PendingOwnerIds.FindIndex(p => p.netId == uv.netId);
            if (index != -1)
            {
                // found owner, turn into a local player
                PendingOwner pendingOwner = s_PendingOwnerIds[index];

                // Set isLocalPlayer to true on this NetworkIdentity and trigger OnStartLocalPlayer in all scripts on the same GO
                uv.SetConnectionToServer(readyConnection);
                uv.SetLocalPlayer(pendingOwner.playerControllerId);

                if (LogFilter.logDev) { Debug.Log("ClientScene::OnOwnerMessage - player=" + uv.gameObject.name); }
                if (readyConnection.connectionId < 0)
                {
                    if (LogFilter.logError) { Debug.LogError("Owner message received on a local client."); }
                    return;
                }
                InternalAddPlayer(uv, pendingOwner.playerControllerId);

                s_PendingOwnerIds.RemoveAt(index);
            }
        }
    }
}
#endif //ENABLE_UNET
