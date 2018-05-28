#if ENABLE_UNET
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace UnityEngine.Networking
{
    [RequireComponent(typeof(NetworkIdentity))]
    [AddComponentMenu("")]
    public class NetworkBehaviour : MonoBehaviour
    {
        //uint m_SyncVarDirtyBits; // vis2k: changed to long to support 64 syncvars
        protected ulong syncVarDirtyBits { get; private set; } // vis2k: private set instead of get/set wrapper
        float m_LastSendTime;

        // this prevents recursion when SyncVar hook functions are called.
        protected bool syncVarHookGuard { get; set; } // vis2k: get; set; instead of variable wrapper. a property is still needed for weaver.

        public bool localPlayerAuthority { get { return netIdentity.localPlayerAuthority; } }
        public bool isServer { get { return netIdentity.isServer; } }
        public bool isClient { get { return netIdentity.isClient; } }
        public bool isLocalPlayer { get { return netIdentity.isLocalPlayer; } }
        public bool hasAuthority { get { return netIdentity.hasAuthority; } }
        public NetworkInstanceId netId { get { return netIdentity.netId; } }
        public NetworkConnection connectionToServer { get { return netIdentity.connectionToServer; } }
        public NetworkConnection connectionToClient { get { return netIdentity.connectionToClient; } }
        public short playerControllerId { get { return netIdentity.playerControllerId; } }

        // vis2k: netIdentity + m_netIdentity instead of myView + m_MyView. no null check needed either, since
        // RequireComponent makes sure that we can never remove the NetworkIdentity anyway.
        NetworkIdentity m_netIdentity;
        internal NetworkIdentity netIdentity
        {
            get
            {
                if (m_netIdentity == null)
                {
                    m_netIdentity = GetComponent<NetworkIdentity>();
                }
                return m_netIdentity;
            }
        }

        const float k_DefaultSendInterval = 0.1f;

        // ----------------------------- Commands --------------------------------

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void SendCommandInternal(NetworkWriter writer, int channelId, string cmdName)
        {
            // local players can always send commands, regardless of authority, other objects must have authority.
            if (!(isLocalPlayer || hasAuthority))
            {
                if (LogFilter.logWarn) { Debug.LogWarning("Trying to send command for object without authority."); }
                return;
            }

            if (ClientScene.readyConnection == null)
            {
                if (LogFilter.logError) { Debug.LogError("Send command attempted with no client running [client=" + connectionToServer + "]."); }
                return;
            }

            writer.FinishMessage();
            ClientScene.readyConnection.SendWriter(writer, channelId);

#if UNITY_EDITOR
            UnityEditor.NetworkDetailStats.IncrementStat(
                UnityEditor.NetworkDetailStats.NetworkDirection.Outgoing,
                MsgType.Command, cmdName, 1);
#endif
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual bool InvokeCommand(int cmdHash, NetworkReader reader)
        {
            return InvokeCommandDelegate(cmdHash, reader); // vis2k: return result directly
        }

        // ----------------------------- Client RPCs --------------------------------

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void SendRPCInternal(NetworkWriter writer, int channelId, string rpcName)
        {
            // This cannot use NetworkServer.active, as that is not specific to this object.
            if (!isServer)
            {
                if (LogFilter.logWarn) { Debug.LogWarning("ClientRpc call on un-spawned object"); }
                return;
            }

            writer.FinishMessage();
            NetworkServer.SendWriterToReady(gameObject, writer, channelId);

#if UNITY_EDITOR
            UnityEditor.NetworkDetailStats.IncrementStat(
                UnityEditor.NetworkDetailStats.NetworkDirection.Outgoing,
                MsgType.Rpc, rpcName, 1);
#endif
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void SendTargetRPCInternal(NetworkConnection conn, NetworkWriter writer, int channelId, string rpcName)
        {
            // This cannot use NetworkServer.active, as that is not specific to this object.
            if (!isServer)
            {
                if (LogFilter.logWarn) { Debug.LogWarning("TargetRpc call on un-spawned object"); }
                return;
            }

            writer.FinishMessage();

            conn.SendWriter(writer, channelId);

#if UNITY_EDITOR
            UnityEditor.NetworkDetailStats.IncrementStat(
                UnityEditor.NetworkDetailStats.NetworkDirection.Outgoing,
                MsgType.Rpc, rpcName, 1);
#endif
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual bool InvokeRPC(int cmdHash, NetworkReader reader)
        {
            return InvokeRpcDelegate(cmdHash, reader); // vis2k: return result directly
        }

        // ----------------------------- Sync Events --------------------------------

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void SendEventInternal(NetworkWriter writer, int channelId, string eventName)
        {
            if (!NetworkServer.active)
            {
                if (LogFilter.logWarn) { Debug.LogWarning("SendEvent no server?"); }
                return;
            }

            writer.FinishMessage();
            NetworkServer.SendWriterToReady(gameObject, writer, channelId);

#if UNITY_EDITOR
            UnityEditor.NetworkDetailStats.IncrementStat(
                UnityEditor.NetworkDetailStats.NetworkDirection.Outgoing,
                MsgType.SyncEvent, eventName, 1);
#endif
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual bool InvokeSyncEvent(int cmdHash, NetworkReader reader)
        {
            return InvokeSyncEventDelegate(cmdHash, reader); // vis2k: return result directly
        }

        // ----------------------------- Sync Lists --------------------------------

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual bool InvokeSyncList(int cmdHash, NetworkReader reader)
        {
            return InvokeSyncListDelegate(cmdHash, reader); // vis2k: return result directly
        }

        // ----------------------------- Code Gen Path Helpers  --------------------------------

        public delegate void CmdDelegate(NetworkBehaviour obj, NetworkReader reader);
        protected delegate void EventDelegate(List<Delegate> targets, NetworkReader reader);

        protected enum UNetInvokeType
        {
            Command,
            ClientRpc,
            SyncEvent,
            SyncList
        };

        protected class Invoker
        {
            public UNetInvokeType invokeType;
            public Type invokeClass;
            public CmdDelegate invokeFunction;

            public string DebugString()
            {
                return invokeType + ":" + invokeClass + ":" + invokeFunction.GetMethodName(); // vis2k: one liner
            }
        };

        static Dictionary<int, Invoker> s_CmdHandlerDelegates = new Dictionary<int, Invoker>();

        [EditorBrowsable(EditorBrowsableState.Never)]
        static protected void RegisterCommandDelegate(Type invokeClass, int cmdHash, CmdDelegate func)
        {
            if (s_CmdHandlerDelegates.ContainsKey(cmdHash))
            {
                return;
            }
            Invoker inv = new Invoker();
            inv.invokeType = UNetInvokeType.Command;
            inv.invokeClass = invokeClass;
            inv.invokeFunction = func;
            s_CmdHandlerDelegates[cmdHash] = inv;
            if (LogFilter.logDev) { Debug.Log("RegisterCommandDelegate hash:" + cmdHash + " " + func.GetMethodName()); }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        static protected void RegisterRpcDelegate(Type invokeClass, int cmdHash, CmdDelegate func)
        {
            if (s_CmdHandlerDelegates.ContainsKey(cmdHash))
            {
                return;
            }
            Invoker inv = new Invoker();
            inv.invokeType = UNetInvokeType.ClientRpc;
            inv.invokeClass = invokeClass;
            inv.invokeFunction = func;
            s_CmdHandlerDelegates[cmdHash] = inv;
            if (LogFilter.logDev) { Debug.Log("RegisterRpcDelegate hash:" + cmdHash + " " + func.GetMethodName()); }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        static protected void RegisterEventDelegate(Type invokeClass, int cmdHash, CmdDelegate func)
        {
            if (s_CmdHandlerDelegates.ContainsKey(cmdHash))
            {
                return;
            }
            Invoker inv = new Invoker();
            inv.invokeType = UNetInvokeType.SyncEvent;
            inv.invokeClass = invokeClass;
            inv.invokeFunction = func;
            s_CmdHandlerDelegates[cmdHash] = inv;
            if (LogFilter.logDev) { Debug.Log("RegisterEventDelegate hash:" + cmdHash + " " + func.GetMethodName()); }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        static protected void RegisterSyncListDelegate(Type invokeClass, int cmdHash, CmdDelegate func)
        {
            if (s_CmdHandlerDelegates.ContainsKey(cmdHash))
            {
                return;
            }
            Invoker inv = new Invoker();
            inv.invokeType = UNetInvokeType.SyncList;
            inv.invokeClass = invokeClass;
            inv.invokeFunction = func;
            s_CmdHandlerDelegates[cmdHash] = inv;
            if (LogFilter.logDev) { Debug.Log("RegisterSyncListDelegate hash:" + cmdHash + " " + func.GetMethodName()); }
        }

        internal static string GetInvoker(int cmdHash)
        {
            if (!s_CmdHandlerDelegates.ContainsKey(cmdHash))
            {
                return null;
            }

            Invoker inv = s_CmdHandlerDelegates[cmdHash];
            return inv.DebugString();
        }

        // wrapper fucntions for each type of network operation
        internal static bool GetInvokerForHashCommand(int cmdHash, out Type invokeClass, out CmdDelegate invokeFunction)
        {
            return GetInvokerForHash(cmdHash, UNetInvokeType.Command, out invokeClass, out invokeFunction);
        }

        internal static bool GetInvokerForHashClientRpc(int cmdHash, out Type invokeClass, out CmdDelegate invokeFunction)
        {
            return GetInvokerForHash(cmdHash, UNetInvokeType.ClientRpc, out invokeClass, out invokeFunction);
        }

        internal static bool GetInvokerForHashSyncList(int cmdHash, out Type invokeClass, out CmdDelegate invokeFunction)
        {
            return GetInvokerForHash(cmdHash, UNetInvokeType.SyncList, out invokeClass, out invokeFunction);
        }

        internal static bool GetInvokerForHashSyncEvent(int cmdHash, out Type invokeClass, out CmdDelegate invokeFunction)
        {
            return GetInvokerForHash(cmdHash, UNetInvokeType.SyncEvent, out invokeClass, out invokeFunction);
        }

        static bool GetInvokerForHash(int cmdHash, NetworkBehaviour.UNetInvokeType invokeType, out Type invokeClass, out CmdDelegate invokeFunction)
        {
            Invoker invoker = null;
            if (!s_CmdHandlerDelegates.TryGetValue(cmdHash, out invoker))
            {
                if (LogFilter.logDev) { Debug.Log("GetInvokerForHash hash:" + cmdHash + " not found"); }
                invokeClass = null;
                invokeFunction = null;
                return false;
            }

            if (invoker == null)
            {
                if (LogFilter.logDev) { Debug.Log("GetInvokerForHash hash:" + cmdHash + " invoker null"); }
                invokeClass = null;
                invokeFunction = null;
                return false;
            }

            if (invoker.invokeType != invokeType)
            {
                if (LogFilter.logError) { Debug.LogError("GetInvokerForHash hash:" + cmdHash + " mismatched invokeType"); }
                invokeClass = null;
                invokeFunction = null;
                return false;
            }

            invokeClass = invoker.invokeClass;
            invokeFunction = invoker.invokeFunction;
            return true;
        }

        internal static void DumpInvokers()
        {
            Debug.Log("DumpInvokers size:" + s_CmdHandlerDelegates.Count);
            foreach (var inv in s_CmdHandlerDelegates)
            {
                Debug.Log("  Invoker:" + inv.Value.invokeClass + ":" + inv.Value.invokeFunction.GetMethodName() + " " + inv.Value.invokeType + " " + inv.Key);
            }
        }

        internal bool ContainsCommandDelegate(int cmdHash)
        {
            return s_CmdHandlerDelegates.ContainsKey(cmdHash);
        }

        internal bool InvokeCommandDelegate(int cmdHash, NetworkReader reader)
        {
            if (!s_CmdHandlerDelegates.ContainsKey(cmdHash))
            {
                return false;
            }

            Invoker inv = s_CmdHandlerDelegates[cmdHash];
            if (inv.invokeType != UNetInvokeType.Command)
            {
                return false;
            }

            if (GetType() != inv.invokeClass)
            {
                if (GetType().IsSubclassOf(inv.invokeClass))
                {
                    // allowed, commands function is on a base class.
                }
                else
                {
                    return false;
                }
            }

            inv.invokeFunction(this, reader);
            return true;
        }

        internal bool InvokeRpcDelegate(int cmdHash, NetworkReader reader)
        {
            if (!s_CmdHandlerDelegates.ContainsKey(cmdHash))
            {
                return false;
            }

            Invoker inv = s_CmdHandlerDelegates[cmdHash];
            if (inv.invokeType != UNetInvokeType.ClientRpc)
            {
                return false;
            }

            if (GetType() != inv.invokeClass)
            {
                if (GetType().IsSubclassOf(inv.invokeClass))
                {
                    // allowed, rpc function is on a base class.
                }
                else
                {
                    return false;
                }
            }

            inv.invokeFunction(this, reader);
            return true;
        }

        internal bool InvokeSyncEventDelegate(int cmdHash, NetworkReader reader)
        {
            if (!s_CmdHandlerDelegates.ContainsKey(cmdHash))
            {
                return false;
            }

            Invoker inv = s_CmdHandlerDelegates[cmdHash];
            if (inv.invokeType != UNetInvokeType.SyncEvent)
            {
                return false;
            }

            inv.invokeFunction(this, reader);
            return true;
        }

        internal bool InvokeSyncListDelegate(int cmdHash, NetworkReader reader)
        {
            if (!s_CmdHandlerDelegates.ContainsKey(cmdHash))
            {
                return false;
            }

            Invoker inv = s_CmdHandlerDelegates[cmdHash];
            if (inv.invokeType != UNetInvokeType.SyncList)
            {
                return false;
            }

            if (GetType() != inv.invokeClass)
            {
                return false;
            }

            inv.invokeFunction(this, reader);
            return true;
        }

        static internal string GetCmdHashHandlerName(int cmdHash)
        {
            if (!s_CmdHandlerDelegates.ContainsKey(cmdHash))
            {
                return cmdHash.ToString();
            }
            Invoker inv = s_CmdHandlerDelegates[cmdHash];
            return inv.invokeType + ":" + inv.invokeFunction.GetMethodName();
        }

        static string GetCmdHashPrefixName(int cmdHash, string prefix)
        {
            if (!s_CmdHandlerDelegates.ContainsKey(cmdHash))
            {
                return cmdHash.ToString();
            }
            Invoker inv = s_CmdHandlerDelegates[cmdHash];
            string name = inv.invokeFunction.GetMethodName(); // vis2k: string instead of var

            int index = name.IndexOf(prefix);
            if (index > -1)
            {
                name = name.Substring(prefix.Length);
            }
            return name;
        }

        internal static string GetCmdHashCmdName(int cmdHash)
        {
            return GetCmdHashPrefixName(cmdHash, "InvokeCmd");
        }

        internal static string GetCmdHashRpcName(int cmdHash)
        {
            return GetCmdHashPrefixName(cmdHash, "InvokeRpc");
        }

        internal static string GetCmdHashEventName(int cmdHash)
        {
            return GetCmdHashPrefixName(cmdHash, "InvokeSyncEvent");
        }

        internal static string GetCmdHashListName(int cmdHash)
        {
            return GetCmdHashPrefixName(cmdHash, "InvokeSyncList");
        }

        // ----------------------------- Helpers  --------------------------------

        [EditorBrowsable(EditorBrowsableState.Never)]
        //protected void SetSyncVarGameObject(GameObject newGameObject, ref GameObject gameObjectField, uint dirtyBit, ref NetworkInstanceId netIdField)
        protected void SetSyncVarGameObject(GameObject newGameObject, ref GameObject gameObjectField, ulong dirtyBit, ref NetworkInstanceId netIdField) // vis2k
        {
            if (syncVarHookGuard)
                return;

            NetworkInstanceId newGameObjectNetId = new NetworkInstanceId();
            if (newGameObject != null)
            {
                var uv = newGameObject.GetComponent<NetworkIdentity>();
                if (uv != null)
                {
                    newGameObjectNetId = uv.netId;
                    if (newGameObjectNetId.IsEmpty())
                    {
                        if (LogFilter.logWarn) { Debug.LogWarning("SetSyncVarGameObject GameObject " + newGameObject + " has a zero netId. Maybe it is not spawned yet?"); }
                    }
                }
            }

            NetworkInstanceId oldGameObjectNetId = new NetworkInstanceId();
            if (gameObjectField != null)
            {
                oldGameObjectNetId = gameObjectField.GetComponent<NetworkIdentity>().netId;
            }

            if (newGameObjectNetId != oldGameObjectNetId)
            {
                if (LogFilter.logDev) { Debug.Log("SetSyncVar GameObject " + GetType().Name + " bit [" + dirtyBit + "] netfieldId:" + oldGameObjectNetId + "->" + newGameObjectNetId); }
                SetDirtyBit(dirtyBit);
                gameObjectField = newGameObject;
                netIdField = newGameObjectNetId;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        //protected void SetSyncVar<T>(T value, ref T fieldValue, uint dirtyBit)
        protected void SetSyncVar<T>(T value, ref T fieldValue, ulong dirtyBit) // vis2k
        {
            bool changed = false;
            if (value == null)
            {
                changed = fieldValue != null; // vis2k: shorter
            }
            else
            {
                changed = !value.Equals(fieldValue);
            }
            if (changed)
            {
                if (LogFilter.logDev) { Debug.Log("SetSyncVar " + GetType().Name + " bit [" + dirtyBit + "] " + fieldValue + "->" + value); }
                SetDirtyBit(dirtyBit);
                fieldValue = value;
            }
        }

        // these are masks, not bit numbers, ie. 0x004 not 2
        //public void SetDirtyBit(uint dirtyBit)
        public void SetDirtyBit(ulong dirtyBit) // vis2k
        {
            syncVarDirtyBits |= dirtyBit;
        }

        public void ClearAllDirtyBits()
        {
            m_LastSendTime = Time.time;
            //m_SyncVarDirtyBits = 0;
            syncVarDirtyBits = 0L; // vis2k
        }

        internal int GetDirtyChannel()
        {
            if (Time.time - m_LastSendTime > GetNetworkSendInterval())
            {
                //if (m_SyncVarDirtyBits != 0)
                if (syncVarDirtyBits != 0L) // vis2k
                {
                    return GetNetworkChannel();
                }
            }
            return -1;
        }

        public virtual bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            if (!initialState)
            {
                //writer.WritePackedUInt32(0);
                writer.WritePackedUInt64(0); // vis2k
            }
            return false;
        }

        public virtual void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (!initialState)
            {
                //reader.ReadPackedUInt32();
                reader.ReadPackedUInt64(); // vis2k
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void PreStartClient()
        {
        }

        public virtual void OnNetworkDestroy()
        {
        }

        public virtual void OnStartServer()
        {
        }

        public virtual void OnStartClient()
        {
        }

        public virtual void OnStartLocalPlayer()
        {
        }

        public virtual void OnStartAuthority()
        {
        }

        public virtual void OnStopAuthority()
        {
        }

        public virtual bool OnRebuildObservers(HashSet<NetworkConnection> observers, bool initialize)
        {
            return false;
        }

        public virtual void OnSetLocalVisibility(bool vis)
        {
        }

        public virtual bool OnCheckObserver(NetworkConnection conn)
        {
            return true;
        }

        public virtual int GetNetworkChannel()
        {
            return Channels.DefaultReliable;
        }

        public virtual float GetNetworkSendInterval()
        {
            return k_DefaultSendInterval;
        }
    }
}
#endif //ENABLE_UNET
