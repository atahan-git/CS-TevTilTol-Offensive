#if ENABLE_UNET
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.Networking.NetworkSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Networking
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkIdentity")]
    public sealed class NetworkIdentity : MonoBehaviour
    {
        // configuration
        [SerializeField] NetworkSceneId m_SceneId;
        [SerializeField] NetworkHash128 m_AssetId;
        [SerializeField] bool           m_ServerOnly;
        [SerializeField] bool           m_LocalPlayerAuthority;

        // runtime data
        public bool                 isClient { get; private set; } // vis2k: private set instead of get/set wrapper
        bool                        m_IsServer;
        public bool hasAuthority    { get; private set; } // vis2k: private set instead of get/set wrapper

        public NetworkInstanceId    netId { get; private set; } // vis2k: private set instead of get/set wrapper
        bool                        m_IsLocalPlayer;
        NetworkConnection           m_ConnectionToServer;
        NetworkConnection           m_ConnectionToClient;
        short                       m_PlayerId = -1;
        NetworkBehaviour[]          m_NetworkBehaviours;

        // there is a list AND a hashSet of connections, for fast verification of dupes, but the main operation is iteration over the list.
        HashSet<int>                m_ObserverConnections;
        List<NetworkConnection>     m_Observers;
        public NetworkConnection    clientAuthorityOwner { get; private set; } // vis2k: private set instead of get/set wrapper

        // member used to mark a identity for future reset
        // check MarkForReset for more information.
        bool                        m_Reset = false;
        // properties
        public bool isServer { get { return m_IsServer && NetworkServer.active; } } // vis2k: simplfied into one &&. note: dont return true if server stopped.
        public NetworkSceneId sceneId { get { return m_SceneId; } }
        public bool serverOnly { get { return m_ServerOnly; } set { m_ServerOnly = value; } }
        public bool localPlayerAuthority { get { return m_LocalPlayerAuthority; } set { m_LocalPlayerAuthority = value; } }

        public NetworkHash128 assetId
        {
            get
            {
#if UNITY_EDITOR
                // This is important because sometimes OnValidate does not run (like when adding view to prefab with no child links)
                if (!m_AssetId.IsValid())
                    SetupIDs();
#endif
                return m_AssetId;
            }
        }
        internal void SetDynamicAssetId(NetworkHash128 newAssetId)
        {
            if (!m_AssetId.IsValid() || m_AssetId.Equals(newAssetId))
            {
                m_AssetId = newAssetId;
            }
            else
            {
                if (LogFilter.logWarn) { Debug.LogWarning("SetDynamicAssetId object already has an assetId <" + m_AssetId + ">"); }
            }
        }

        // used when adding players
        internal void SetClientOwner(NetworkConnection conn)
        {
            if (clientAuthorityOwner != null)
            {
                if (LogFilter.logError) { Debug.LogError("SetClientOwner m_ClientAuthorityOwner already set!"); }
            }
            clientAuthorityOwner = conn;
            clientAuthorityOwner.AddOwnedObject(this);
        }

        // used during dispose after disconnect
        internal void ClearClientOwner()
        {
            clientAuthorityOwner = null;
        }

        internal void ForceAuthority(bool authority)
        {
            if (hasAuthority != authority) // vis2k: != instead of == and early return
            {
                hasAuthority = authority;
                if (authority)
                {
                    OnStartAuthority();
                }
                else
                {
                    OnStopAuthority();
                }
            }
        }

        public bool isLocalPlayer { get { return m_IsLocalPlayer; } }
        public short playerControllerId { get { return m_PlayerId; } }
        public NetworkConnection connectionToServer { get { return m_ConnectionToServer; } }
        public NetworkConnection connectionToClient { get { return m_ConnectionToClient; } }

        public ReadOnlyCollection<NetworkConnection> observers
        {
            get
            {
                // vis2k: shorter
                return m_Observers != null ? new ReadOnlyCollection<NetworkConnection>(m_Observers) : null;
            }
        }

        static uint s_NextNetworkId = 1;
        static internal NetworkInstanceId GetNextNetworkId()
        {
            return new NetworkInstanceId(s_NextNetworkId++); // vis2k: shorter and saves 4 bytes
        }

        void CacheBehaviours()
        {
            if (m_NetworkBehaviours == null)
            {
                m_NetworkBehaviours = GetComponents<NetworkBehaviour>();
            }
        }

        public delegate void ClientAuthorityCallback(NetworkConnection conn, NetworkIdentity uv, bool authorityState);
        public static ClientAuthorityCallback clientAuthorityCallback;

        static internal void AddNetworkId(uint id)
        {
            if (id >= s_NextNetworkId)
            {
                s_NextNetworkId = id + 1;
            }
        }

        // only used during spawning on clients to set the identity.
        internal void SetNetworkInstanceId(NetworkInstanceId newNetId)
        {
            netId = newNetId;
            if (newNetId.Value == 0)
            {
                m_IsServer = false;
            }
        }

        // only used during post-processing
        internal void ForceSceneId(int newSceneId) // vis2k: made internal. no one should ever call that otherwise.
        {
            m_SceneId = new NetworkSceneId((uint)newSceneId);
        }

        // only used in SetLocalObject
        internal void UpdateClientServer(bool isClientFlag, bool isServerFlag)
        {
            isClient |= isClientFlag;
            m_IsServer |= isServerFlag;
        }

        // used when the player object for a connection changes
        internal void SetNotLocalPlayer()
        {
            m_IsLocalPlayer = false;

            // dont change authority for objects on the host
            if (NetworkServer.active && NetworkServer.localClientActive)
            {
                return;
            }

            hasAuthority = false;
        }

        // this is used when a connection is destroyed, since the "observers" property is read-only
        internal void RemoveObserverInternal(NetworkConnection conn)
        {
            if (m_Observers != null)
            {
                m_Observers.Remove(conn);
                m_ObserverConnections.Remove(conn.connectionId);
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_ServerOnly && m_LocalPlayerAuthority)
            {
                if (LogFilter.logWarn) { Debug.LogWarning("Disabling Local Player Authority for " + gameObject + " because it is server-only."); }
                m_LocalPlayerAuthority = false;
            }

            SetupIDs();

            // vis2k:
            // make sure that there are no other NetworkIdentity components in
            // children. this way we don't have to check it in all the other
            // classes separately.
            // note: OnValidate might not be called immediately when modifying a
            //       child object, but it is always called when pressing Play or
            //       building the project.
            if (GetComponentsInChildren<NetworkIdentity>().Length > 1)
            {
                if (LogFilter.logWarn) { Debug.LogWarning((ThisIsAPrefab() ? "The prefab '" : " The Object '") + name + "' has multiple NetworkIdentity components. There can only be one NetworkIdentity on a prefab, and it must be on the root object."); }
            }
        }

        void AssignAssetID(GameObject prefab)
        {
            string path = AssetDatabase.GetAssetPath(prefab);
            m_AssetId = NetworkHash128.Parse(AssetDatabase.AssetPathToGUID(path));
        }

        bool ThisIsAPrefab()
        {
            PrefabType prefabType = PrefabUtility.GetPrefabType(gameObject);
            if (prefabType == PrefabType.Prefab)
                return true;
            return false;
        }

        bool ThisIsASceneObjectWithPrefabParent(out GameObject prefab)
        {
            prefab = null;
            PrefabType prefabType = PrefabUtility.GetPrefabType(gameObject);
            if (prefabType == PrefabType.None)
                return false;
            prefab = (GameObject)PrefabUtility.GetPrefabParent(gameObject);
            if (prefab == null)
            {
                if (LogFilter.logError) { Debug.LogError("Failed to find prefab parent for scene object [name:" + gameObject.name + "]"); }
                return false;
            }
            return true;
        }

        void SetupIDs()
        {
            GameObject prefab;
            if (ThisIsAPrefab())
            {
                if (LogFilter.logDev) { Debug.Log("This is a prefab: " + gameObject.name); }
                AssignAssetID(gameObject);
            }
            else if (ThisIsASceneObjectWithPrefabParent(out prefab))
            {
                if (LogFilter.logDev) { Debug.Log("This is a scene object with prefab link: " + gameObject.name); }
                AssignAssetID(prefab);
            }
            else
            {
                if (LogFilter.logDev) { Debug.Log("This is a pure scene object: " + gameObject.name); }
                m_AssetId.Reset();
            }
        }

#endif
        void OnDestroy()
        {
            if (m_IsServer && NetworkServer.active)
            {
                NetworkServer.Destroy(gameObject);
            }
        }

        internal void OnStartServer(bool allowNonZeroNetId)
        {
            if (m_IsServer)
            {
                return;
            }
            m_IsServer = true;

            if (m_LocalPlayerAuthority)
            {
                // local player on server has NO authority
                hasAuthority = false;
            }
            else
            {
                // enemy on server has authority
                hasAuthority = true;
            }

            m_Observers = new List<NetworkConnection>();
            m_ObserverConnections = new HashSet<int>();
            CacheBehaviours();

            // If the instance/net ID is invalid here then this is an object instantiated from a prefab and the server should assign a valid ID
            if (netId.IsEmpty())
            {
                netId = GetNextNetworkId();
            }
            else if (!allowNonZeroNetId) // vis2k: shorter
            {
                if (LogFilter.logError) { Debug.LogError("Object has non-zero netId " + netId + " for " + gameObject); }
                return;
            }

            if (LogFilter.logDev) { Debug.Log("OnStartServer " + gameObject + " GUID:" + netId); }
            NetworkServer.instance.SetLocalObjectOnServer(netId, gameObject);

            foreach (NetworkBehaviour comp in m_NetworkBehaviours) // vis2k: foreach
            {
                try
                {
                    comp.OnStartServer();
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception in OnStartServer:" + e.Message + " " + e.StackTrace);
                }
            }

            if (NetworkClient.active && NetworkServer.localClientActive)
            {
                // there will be no spawn message, so start the client here too
                ClientScene.SetLocalObject(netId, gameObject);
                OnStartClient();
            }

            if (hasAuthority)
            {
                OnStartAuthority();
            }
        }

        internal void OnStartClient()
        {
            // vis2k: set to true without checking !isClient first
            isClient = true;
            CacheBehaviours();

            if (LogFilter.logDev) { Debug.Log("OnStartClient " + gameObject + " GUID:" + netId + " localPlayerAuthority:" + localPlayerAuthority); }
            foreach (NetworkBehaviour comp in m_NetworkBehaviours) // vis2k: foreach
            {
                try
                {
                    comp.PreStartClient(); // generated startup to resolve object references
                    comp.OnStartClient(); // user implemented startup
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception in OnStartClient:" + e.Message + " " + e.StackTrace);
                }
            }
        }

        internal void OnStartAuthority()
        {
            foreach (NetworkBehaviour comp in m_NetworkBehaviours) // vis2k: foreach
            {
                try
                {
                    comp.OnStartAuthority();
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception in OnStartAuthority:" + e.Message + " " + e.StackTrace);
                }
            }
        }

        internal void OnStopAuthority()
        {
            foreach (NetworkBehaviour comp in m_NetworkBehaviours) // vis2k: foreach
            {
                try
                {
                    comp.OnStopAuthority();
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception in OnStopAuthority:" + e.Message + " " + e.StackTrace);
                }
            }
        }

        internal void OnSetLocalVisibility(bool vis)
        {
            foreach (NetworkBehaviour comp in m_NetworkBehaviours) // vis2k: foreach
            {
                try
                {
                    comp.OnSetLocalVisibility(vis);
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception in OnSetLocalVisibility:" + e.Message + " " + e.StackTrace);
                }
            }
        }

        internal bool OnCheckObserver(NetworkConnection conn)
        {
            foreach (NetworkBehaviour comp in m_NetworkBehaviours) // vis2k: foreach
            {
                try
                {
                    if (!comp.OnCheckObserver(conn))
                        return false;
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception in OnCheckObserver:" + e.Message + " " + e.StackTrace);
                }
            }
            return true;
        }

        // vis2k: readstring bug prevention: https://issuetracker.unity3d.com/issues/unet-networkwriter-dot-write-causing-readstring-slash-readbytes-out-of-range-errors-in-clients
        // -> OnSerialize writes length,componentData,length,componentData,...
        // -> OnDeserialize carefully extracts each data, then deserializes each component with separate readers
        //    -> it will be impossible to read too many or too few bytes in OnDeserialize
        //    -> we can properly track down errors
        internal bool OnSerializeSafely(NetworkBehaviour comp, NetworkWriter writer, bool initialState)
        {
            // serialize into a temporary writer
            NetworkWriter temp = new NetworkWriter();
            bool result = false;
            try
            {
                result = comp.OnSerialize(temp, initialState);
            }
            catch (Exception e)
            {
                // show a detailed error and let the user know what went wrong
                Debug.LogError("OnSerialize failed for: object=" + name + " component=" + comp.GetType() + " sceneId=" + m_SceneId + "\n\n" + e.ToString());
            }
            byte[] bytes = temp.ToArray();
            if (LogFilter.logDebug) { Debug.Log("OnSerializeSafely written for object=" + comp.name + " component=" + comp.GetType() + " sceneId=" + m_SceneId + " length=" + bytes.Length); }

            // serialize length,data into the real writer, untouched by user code
            writer.WriteBytesAndSize(bytes, bytes.Length); // length,data
            return result;
        }

        internal void OnDeserializeAllSafely(NetworkBehaviour[] components, NetworkReader reader, bool initialState)
        {
            foreach (var comp in components)
            {
                // extract data length and data safely, untouched by user code
                // -> returns empty array if length is 0, so .Length is always the proper length
                byte[] bytes = reader.ReadBytesAndSize();
                if (LogFilter.logDebug) { Debug.Log("OnDeserializeSafely extracted: " + comp.name + " component=" + comp.GetType() + " sceneId=" + m_SceneId + " length=" + bytes.Length); }

                // call OnDeserialize with a temporary reader, so that the
                // original one can't be messed with. we also wrap it in a
                // try-catch block so there's no way to mess up another
                // component's deserialization
                try
                {
                    comp.OnDeserialize(new NetworkReader(bytes), initialState);
                }
                catch (Exception e)
                {
                    // show a detailed error and let the user know what went wrong
                    Debug.LogError("OnDeserialize failed for: object=" + name + " component=" + comp.GetType() + " sceneId=" + m_SceneId + " length=" + bytes.Length + ". Possible Reasons:\n  * Do " + comp.GetType() + "'s OnSerialize and OnDeserialize calls write the same amount of data(" + bytes.Length +" bytes)? \n  * Was there an exception in " + comp.GetType() + "'s OnSerialize/OnDeserialize code?\n  * Are the server and client the exact same project?\n  * Maybe this OnDeserialize call was meant for another GameObject? The sceneIds can easily get out of sync if the Hierarchy was modified only in the client OR the server. Try rebuilding both.\n\n" + e.ToString());
                }
            }
        }
        ////////////////////////////////////////////////////////////////////////

        // happens on server
        internal void UNetSerializeAllVars(NetworkWriter writer)
        {
            foreach (NetworkBehaviour comp in m_NetworkBehaviours) // vis2k: foreach
            {
                OnSerializeSafely(comp, writer, true); // vis2k: use OnSerializeSafely instead of OnSerialize
            }
        }

        // happens on client
        internal void HandleClientAuthority(bool authority)
        {
            if (!localPlayerAuthority)
            {
                if (LogFilter.logError) { Debug.LogError("HandleClientAuthority " + gameObject + " does not have localPlayerAuthority"); }
                return;
            }

            ForceAuthority(authority);
        }

        // helper function for Handle** functions
        bool GetInvokeComponent(int cmdHash, Type invokeClass, out NetworkBehaviour invokeComponent)
        {
            // dont use GetComponent(), already have a list - avoids an allocation
            // vis2k: Find instead of for loop to find it and no extra foundComp variables
            invokeComponent = Array.Find(m_NetworkBehaviours,
                comp => comp.GetType() == invokeClass || comp.GetType().IsSubclassOf(invokeClass)
            );
            if (invokeComponent == null)
            {
                string errorCmdName = NetworkBehaviour.GetCmdHashHandlerName(cmdHash);
                if (LogFilter.logError) { Debug.LogError("Found no behaviour for incoming [" + errorCmdName + "] on " + gameObject + ",  the server and client should have the same NetworkBehaviour instances [netId=" + netId + "]."); }
                return false;
            }
            return true;
        }

        // happens on client
        internal void HandleSyncEvent(int cmdHash, NetworkReader reader)
        {
            // this doesn't use NetworkBehaviour.InvokeSyncEvent function (anymore). this method of calling is faster.
            // The hash is only looked up once, insted of twice(!) per NetworkBehaviour on the object.

            if (gameObject == null)
            {
                string errorCmdName = NetworkBehaviour.GetCmdHashHandlerName(cmdHash);
                if (LogFilter.logWarn) { Debug.LogWarning("SyncEvent [" + errorCmdName + "] received for deleted object [netId=" + netId + "]"); }
                return;
            }

            // find the matching SyncEvent function and networkBehaviour class
            NetworkBehaviour.CmdDelegate invokeFunction;
            Type invokeClass;
            bool invokeFound = NetworkBehaviour.GetInvokerForHashSyncEvent(cmdHash, out invokeClass, out invokeFunction);
            if (!invokeFound)
            {
                // We don't get a valid lookup of the command name when it doesn't exist...
                string errorCmdName = NetworkBehaviour.GetCmdHashHandlerName(cmdHash);
                if (LogFilter.logError) { Debug.LogError("Found no receiver for incoming [" + errorCmdName + "] on " + gameObject + ",  the server and client should have the same NetworkBehaviour instances [netId=" + netId + "]."); }
                return;
            }

            // find the right component to invoke the function on
            NetworkBehaviour invokeComponent;
            if (!GetInvokeComponent(cmdHash, invokeClass, out invokeComponent))
            {
                string errorCmdName = NetworkBehaviour.GetCmdHashHandlerName(cmdHash);
                if (LogFilter.logWarn) { Debug.LogWarning("SyncEvent [" + errorCmdName + "] handler not found [netId=" + netId + "]"); }
                return;
            }

            invokeFunction(invokeComponent, reader);

#if UNITY_EDITOR
            UnityEditor.NetworkDetailStats.IncrementStat(
                UnityEditor.NetworkDetailStats.NetworkDirection.Incoming,
                MsgType.SyncEvent, NetworkBehaviour.GetCmdHashEventName(cmdHash), 1);
#endif
        }

        // happens on client
        internal void HandleSyncList(int cmdHash, NetworkReader reader)
        {
            // this doesn't use NetworkBehaviour.InvokSyncList function (anymore). this method of calling is faster.
            // The hash is only looked up once, insted of twice(!) per NetworkBehaviour on the object.

            if (gameObject == null)
            {
                string errorCmdName = NetworkBehaviour.GetCmdHashHandlerName(cmdHash);
                if (LogFilter.logWarn) { Debug.LogWarning("SyncList [" + errorCmdName + "] received for deleted object [netId=" + netId + "]"); }
                return;
            }

            // find the matching SyncList function and networkBehaviour class
            NetworkBehaviour.CmdDelegate invokeFunction;
            Type invokeClass;
            bool invokeFound = NetworkBehaviour.GetInvokerForHashSyncList(cmdHash, out invokeClass, out invokeFunction);
            if (!invokeFound)
            {
                // We don't get a valid lookup of the command name when it doesn't exist...
                string errorCmdName = NetworkBehaviour.GetCmdHashHandlerName(cmdHash);
                if (LogFilter.logError) { Debug.LogError("Found no receiver for incoming [" + errorCmdName + "] on " + gameObject + ",  the server and client should have the same NetworkBehaviour instances [netId=" + netId + "]."); }
                return;
            }

            // find the right component to invoke the function on
            NetworkBehaviour invokeComponent;
            if (!GetInvokeComponent(cmdHash, invokeClass, out invokeComponent))
            {
                string errorCmdName = NetworkBehaviour.GetCmdHashHandlerName(cmdHash);
                if (LogFilter.logWarn) { Debug.LogWarning("SyncList [" + errorCmdName + "] handler not found [netId=" + netId + "]"); }
                return;
            }

            invokeFunction(invokeComponent, reader);

#if UNITY_EDITOR
            UnityEditor.NetworkDetailStats.IncrementStat(
                UnityEditor.NetworkDetailStats.NetworkDirection.Incoming,
                MsgType.SyncList, NetworkBehaviour.GetCmdHashListName(cmdHash), 1);
#endif
        }

        // happens on server
        internal void HandleCommand(int cmdHash, NetworkReader reader)
        {
            // this doesn't use NetworkBehaviour.InvokeCommand function (anymore). this method of calling is faster.
            // The hash is only looked up once, insted of twice(!) per NetworkBehaviour on the object.

            if (gameObject == null)
            {
                string errorCmdName = NetworkBehaviour.GetCmdHashHandlerName(cmdHash);
                if (LogFilter.logWarn) { Debug.LogWarning("Command [" + errorCmdName + "] received for deleted object [netId=" + netId + "]"); }
                return;
            }

            // find the matching Command function and networkBehaviour class
            NetworkBehaviour.CmdDelegate invokeFunction;
            Type invokeClass;
            bool invokeFound = NetworkBehaviour.GetInvokerForHashCommand(cmdHash, out invokeClass, out invokeFunction);
            if (!invokeFound)
            {
                // We don't get a valid lookup of the command name when it doesn't exist...
                string errorCmdName = NetworkBehaviour.GetCmdHashHandlerName(cmdHash);
                if (LogFilter.logError) { Debug.LogError("Found no receiver for incoming [" + errorCmdName + "] on " + gameObject + ",  the server and client should have the same NetworkBehaviour instances [netId=" + netId + "]."); }
                return;
            }

            // find the right component to invoke the function on
            NetworkBehaviour invokeComponent;
            if (!GetInvokeComponent(cmdHash, invokeClass, out invokeComponent))
            {
                string errorCmdName = NetworkBehaviour.GetCmdHashHandlerName(cmdHash);
                if (LogFilter.logWarn) { Debug.LogWarning("Command [" + errorCmdName + "] handler not found [netId=" + netId + "]"); }
                return;
            }

            invokeFunction(invokeComponent, reader);

#if UNITY_EDITOR
            UnityEditor.NetworkDetailStats.IncrementStat(
                UnityEditor.NetworkDetailStats.NetworkDirection.Incoming,
                MsgType.Command, NetworkBehaviour.GetCmdHashCmdName(cmdHash), 1);
#endif
        }

        // happens on client
        internal void HandleRPC(int cmdHash, NetworkReader reader)
        {
            // this doesn't use NetworkBehaviour.InvokeClientRpc function (anymore). this method of calling is faster.
            // The hash is only looked up once, insted of twice(!) per NetworkBehaviour on the object.

            if (gameObject == null)
            {
                string errorCmdName = NetworkBehaviour.GetCmdHashHandlerName(cmdHash);
                if (LogFilter.logWarn) { Debug.LogWarning("ClientRpc [" + errorCmdName + "] received for deleted object [netId=" + netId + "]"); }
                return;
            }

            // find the matching ClientRpc function and networkBehaviour class
            NetworkBehaviour.CmdDelegate invokeFunction;
            Type invokeClass;
            bool invokeFound = NetworkBehaviour.GetInvokerForHashClientRpc(cmdHash, out invokeClass, out invokeFunction);
            if (!invokeFound)
            {
                // We don't get a valid lookup of the command name when it doesn't exist...
                string errorCmdName = NetworkBehaviour.GetCmdHashHandlerName(cmdHash);
                if (LogFilter.logError) { Debug.LogError("Found no receiver for incoming [" + errorCmdName + "] on " + gameObject + ",  the server and client should have the same NetworkBehaviour instances [netId=" + netId + "]."); }
                return;
            }

            // find the right component to invoke the function on
            NetworkBehaviour invokeComponent;
            if (!GetInvokeComponent(cmdHash, invokeClass, out invokeComponent))
            {
                string errorCmdName = NetworkBehaviour.GetCmdHashHandlerName(cmdHash);
                if (LogFilter.logWarn) { Debug.LogWarning("ClientRpc [" + errorCmdName + "] handler not found [netId=" + netId + "]"); }
                return;
            }

            invokeFunction(invokeComponent, reader);

#if UNITY_EDITOR
            UnityEditor.NetworkDetailStats.IncrementStat(
                UnityEditor.NetworkDetailStats.NetworkDirection.Incoming,
                MsgType.Rpc, NetworkBehaviour.GetCmdHashRpcName(cmdHash), 1);
#endif
        }

        // invoked by unity runtime immediately after the regular "Update()" function.
        internal void UNetUpdate()
        {
            // check if any behaviours are ready to send
            uint dirtyChannelBits = 0;
            foreach (NetworkBehaviour comp in m_NetworkBehaviours) // vis2k: foreach
            {
                int channelId = comp.GetDirtyChannel();
                if (channelId != -1)
                {
                    dirtyChannelBits |= (uint)(1 << channelId);
                }
            }
            if (dirtyChannelBits == 0)
                return;

            NetworkWriter writer = new NetworkWriter(); // vis2k: create here to avoid global state
            for (int channelId = 0; channelId < NetworkServer.numChannels; channelId++)
            {
                if ((dirtyChannelBits & (uint)(1 << channelId)) != 0)
                {
                    writer.StartMessage(MsgType.UpdateVars);
                    writer.Write(netId);

                    bool wroteData = false;
                    ushort oldPos;
                    foreach (NetworkBehaviour comp in m_NetworkBehaviours) // vis2k: foreach
                    {
                        oldPos = writer.Position;
                        if (comp.GetDirtyChannel() != channelId)
                        {
                            // component could write more than one dirty-bits, so call the serialize func
                            //comp.OnSerialize(s_UpdateWriter, false);
                            OnSerializeSafely(comp, writer, false);
                            continue;
                        }

                        //if (comp.OnSerialize(s_UpdateWriter, false))
                        if (OnSerializeSafely(comp, writer, false))
                        {
                            comp.ClearAllDirtyBits();

#if UNITY_EDITOR
                            UnityEditor.NetworkDetailStats.IncrementStat(
                                UnityEditor.NetworkDetailStats.NetworkDirection.Outgoing,
                                MsgType.UpdateVars, comp.GetType().Name, 1);
#endif

                            wroteData = true;
                        }
                        if (writer.Position - oldPos > NetworkServer.maxPacketSize)
                        {
                            if (LogFilter.logWarn) { Debug.LogWarning("Large state update of " + (writer.Position - oldPos) + " bytes for netId:" + netId + " from script:" + comp); }
                        }
                    }

                    if (!wroteData)
                    {
                        // nothing to send.. this could be a script with no OnSerialize function setting dirty bits
                        continue;
                    }

                    writer.FinishMessage();
                    NetworkServer.SendWriterToReady(gameObject, writer, channelId);
                }
            }
        }

        internal void OnUpdateVars(NetworkReader reader, bool initialState)
        {
            if (initialState && m_NetworkBehaviours == null)
            {
                m_NetworkBehaviours = GetComponents<NetworkBehaviour>();
            }

            // vis2k: deserialize safely
            OnDeserializeAllSafely(m_NetworkBehaviours, reader, initialState);

            /*old unsafe deserialize code
            for (int i = 0; i < m_NetworkBehaviours.Length; i++)
            {
                NetworkBehaviour comp = m_NetworkBehaviours[i];


#if UNITY_EDITOR
                var oldReadPos = reader.Position;
#endif
                comp.OnDeserialize(reader, initialState);
#if UNITY_EDITOR
                if (reader.Position - oldReadPos > 1)
                {
                    //MakeFloatGizmo("Received Vars " + comp.GetType().Name + " bytes:" + (reader.Position - oldReadPos), Color.white);
                    UnityEditor.NetworkDetailStats.IncrementStat(
                        UnityEditor.NetworkDetailStats.NetworkDirection.Incoming,
                        MsgType.UpdateVars, comp.GetType().Name, 1);
                }
#endif
            }
            */
        }

        internal void SetLocalPlayer(short localPlayerControllerId)
        {
            m_IsLocalPlayer = true;
            m_PlayerId = localPlayerControllerId;

            // there is an ordering issue here that originAuthority solves. OnStartAuthority should only be called if m_HasAuthority was false when this function began,
            // or it will be called twice for this object. But that state is lost by the time OnStartAuthority is called below, so the original value is cached
            // here to be checked below.
            bool originAuthority = hasAuthority;
            if (localPlayerAuthority)
            {
                hasAuthority = true;
            }

            foreach (NetworkBehaviour comp in m_NetworkBehaviours) // vis2k: foreach
            {
                comp.OnStartLocalPlayer();

                if (localPlayerAuthority && !originAuthority)
                {
                    comp.OnStartAuthority();
                }
            }
        }

        internal void SetConnectionToServer(NetworkConnection conn)
        {
            m_ConnectionToServer = conn;
        }

        internal void SetConnectionToClient(NetworkConnection conn, short newPlayerControllerId)
        {
            m_PlayerId = newPlayerControllerId;
            m_ConnectionToClient = conn;
        }

        internal void OnNetworkDestroy()
        {
            for (int i = 0; m_NetworkBehaviours != null && i < m_NetworkBehaviours.Length; ++i)
            {
                NetworkBehaviour comp = m_NetworkBehaviours[i];
                comp.OnNetworkDestroy();
            }
            m_IsServer = false;
        }

        internal void ClearObservers()
        {
            if (m_Observers != null)
            {
                foreach (NetworkConnection observer in m_Observers) // vis2k: foreach
                {
                    observer.RemoveFromVisList(this, true);
                }
                m_Observers.Clear();
                m_ObserverConnections.Clear();
            }
        }

        internal void AddObserver(NetworkConnection conn)
        {
            if (m_Observers == null)
            {
                if (LogFilter.logError) { Debug.LogError("AddObserver for " + gameObject + " observer list is null"); }
                return;
            }

            // uses hashset for better-than-list-iteration lookup performance.
            if (m_ObserverConnections.Contains(conn.connectionId))
            {
                if (LogFilter.logDebug) { Debug.Log("Duplicate observer " + conn.address + " added for " + gameObject); }
                return;
            }

            if (LogFilter.logDev) { Debug.Log("Added observer " + conn.address + " added for " + gameObject); }

            m_Observers.Add(conn);
            m_ObserverConnections.Add(conn.connectionId);
            conn.AddToVisList(this);
        }

        internal void RemoveObserver(NetworkConnection conn)
        {
            if (m_Observers == null)
                return;

            // NOTE this is linear performance now..
            m_Observers.Remove(conn);
            m_ObserverConnections.Remove(conn.connectionId);
            conn.RemoveFromVisList(this, false);
        }

        public void RebuildObservers(bool initialize)
        {
            if (m_Observers == null)
                return;

            bool changed = false;
            bool result = false;
            HashSet<NetworkConnection> newObservers = new HashSet<NetworkConnection>();
            HashSet<NetworkConnection> oldObservers = new HashSet<NetworkConnection>(m_Observers);

            foreach (NetworkBehaviour comp in m_NetworkBehaviours) // vis2k: foreach
            {
                result |= comp.OnRebuildObservers(newObservers, initialize);
            }
            if (!result)
            {
                // none of the behaviours rebuilt our observers, use built-in rebuild method
                if (initialize)
                {
                    foreach (NetworkConnection conn in NetworkServer.connections) // vis2k: foreach
                    {
                        if (conn != null && conn.isReady) // vis2k: one liner
                            AddObserver(conn);
                    }

                    foreach (NetworkConnection conn in NetworkServer.localConnections) // vis2k: foreach
                    {
                        if (conn != null && conn.isReady) // vis2k: one liner
                            AddObserver(conn);
                    }
                }
                return;
            }

            // apply changes from rebuild
            foreach (var conn in newObservers)
            {
                if (conn != null) // vis2k: != instead of == and continue
                {
                    if (conn.isReady) // vis2k: if+else instead of continue
                    {
                        if (initialize || !oldObservers.Contains(conn))
                        {
                            // new observer
                            conn.AddToVisList(this);
                            if (LogFilter.logDebug) { Debug.Log("New Observer for " + gameObject + " " + conn); }
                            changed = true;
                        }
                    }
                    else
                    {
                        if (LogFilter.logWarn) { Debug.LogWarning("Observer is not ready for " + gameObject + " " + conn); }
                    }
                }
            }

            foreach (var conn in oldObservers)
            {
                if (!newObservers.Contains(conn))
                {
                    // removed observer
                    conn.RemoveFromVisList(this, false);
                    if (LogFilter.logDebug) { Debug.Log("Removed Observer for " + gameObject + " " + conn); }
                    changed = true;
                }
            }

            // special case for local client.
            if (initialize)
            {
                foreach (NetworkConnection conn in NetworkServer.localConnections)
                {
                    if (!newObservers.Contains(conn))
                    {
                        OnSetLocalVisibility(false);
                    }
                }
            }

            if (changed) // vis2k: if changed instead of if !changed + return
            {
                m_Observers = new List<NetworkConnection>(newObservers);

                // rebuild hashset once we have the final set of new observers
                m_ObserverConnections.Clear();
                foreach (NetworkConnection conn in m_Observers) // vis2k: foreach
                {
                    m_ObserverConnections.Add(conn.connectionId);
                }
            }
        }

        public bool RemoveClientAuthority(NetworkConnection conn)
        {
            if (!isServer)
            {
                if (LogFilter.logError) { Debug.LogError("RemoveClientAuthority can only be call on the server for spawned objects."); }
                return false;
            }

            if (connectionToClient != null)
            {
                if (LogFilter.logError) { Debug.LogError("RemoveClientAuthority cannot remove authority for a player object"); }
                return false;
            }

            if (clientAuthorityOwner == null)
            {
                if (LogFilter.logError) { Debug.LogError("RemoveClientAuthority for " + gameObject + " has no clientAuthority owner."); }
                return false;
            }

            if (clientAuthorityOwner != conn)
            {
                if (LogFilter.logError) { Debug.LogError("RemoveClientAuthority for " + gameObject + " has different owner."); }
                return false;
            }

            clientAuthorityOwner.RemoveOwnedObject(this);
            clientAuthorityOwner = null;

            // server now has authority (this is only called on server)
            ForceAuthority(true);

            // send msg to that client
            var msg = new ClientAuthorityMessage();
            msg.netId = netId;
            msg.authority = false;
            conn.Send(MsgType.LocalClientAuthority, msg);

            if (clientAuthorityCallback != null)
            {
                clientAuthorityCallback(conn, this, false);
            }
            return true;
        }

        public bool AssignClientAuthority(NetworkConnection conn)
        {
            if (!isServer)
            {
                if (LogFilter.logError) { Debug.LogError("AssignClientAuthority can only be call on the server for spawned objects."); }
                return false;
            }
            if (!localPlayerAuthority)
            {
                if (LogFilter.logError) { Debug.LogError("AssignClientAuthority can only be used for NetworkIdentity component with LocalPlayerAuthority set."); }
                return false;
            }

            if (clientAuthorityOwner != null && conn != clientAuthorityOwner)
            {
                if (LogFilter.logError) { Debug.LogError("AssignClientAuthority for " + gameObject + " already has an owner. Use RemoveClientAuthority() first."); }
                return false;
            }

            if (conn == null)
            {
                if (LogFilter.logError) { Debug.LogError("AssignClientAuthority for " + gameObject + " owner cannot be null. Use RemoveClientAuthority() instead."); }
                return false;
            }

            clientAuthorityOwner = conn;
            clientAuthorityOwner.AddOwnedObject(this);

            // server no longer has authority (this is called on server). Note that local client could re-acquire authority below
            ForceAuthority(false);

            // send msg to that client
            var msg = new ClientAuthorityMessage();
            msg.netId = netId;
            msg.authority = true;
            conn.Send(MsgType.LocalClientAuthority, msg);

            if (clientAuthorityCallback != null)
            {
                clientAuthorityCallback(conn, this, true);
            }
            return true;
        }

        // marks the identity for future reset, this is because we cant reset the identity during destroy
        // as people might want to be able to read the members inside OnDestroy(), and we have no way
        // of invoking reset after OnDestroy is called.
        internal void MarkForReset()
        {
            m_Reset = true;
        }

        // if we have marked an identity for reset we do the actual reset.
        internal void Reset()
        {
            if (!m_Reset)
                return;

            m_Reset = false;
            m_IsServer = false;
            isClient = false;
            hasAuthority = false;

            netId = NetworkInstanceId.Zero;
            m_IsLocalPlayer = false;
            m_ConnectionToServer = null;
            m_ConnectionToClient = null;
            m_PlayerId = -1;
            m_NetworkBehaviours = null;

            ClearObservers();
            clientAuthorityOwner = null;
        }

#if UNITY_EDITOR
        // this is invoked by the UnityEngine when a Mono Domain reload happens in the editor.
        // the transport layer has state in C++, so when the C# state is lost (on domain reload), the C++ transport layer must be shutown as well.
        static internal void UNetDomainReload()
        {
            NetworkManager.OnDomainReload();
        }
#endif

        // this is invoked by the UnityEngine
        static internal void UNetStaticUpdate()
        {
            NetworkServer.Update();
            NetworkClient.UpdateClients();
            NetworkManager.UpdateScene();

#if UNITY_EDITOR
            NetworkDetailStats.NewProfilerTick(Time.time);
#endif
        }
    };
}
#endif //ENABLE_UNET
