#if ENABLE_UNET
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Networking
{
    // This has a list of real connections
    // The local or "fake" connections are kept separate because sometimes you
    // only want to iterate through those, and not all connections.
    class ConnectionArray
    {
        internal List<NetworkConnection> localConnections; // vis2k: internal instead of get/set wrapper
        internal List<NetworkConnection> connections; // vis2k: internal instead of get/set wrapper

        public int Count { get { return connections.Count; } }
        public int LocalIndex { get { return -localConnections.Count; } }

        public ConnectionArray()
        {
            connections = new List<NetworkConnection>();
            localConnections = new List<NetworkConnection>();
        }

        public int Add(int connId, NetworkConnection conn)
        {
            if (connId < 0)
            {
                if (LogFilter.logWarn) {Debug.LogWarning("ConnectionArray Add bad id " + connId); }
                return -1;
            }

            if (connId < connections.Count && connections[connId] != null)
            {
                if (LogFilter.logWarn) { Debug.LogWarning("ConnectionArray Add dupe at " + connId); }
                return -1;
            }

            while (connId > (connections.Count - 1))
            {
                connections.Add(null);
            }

            connections[connId] = conn;
            return connId;
        }

        // call this if you know the connnection exists
        public NetworkConnection Get(int connId)
        {
            if (connId < 0)
            {
                return localConnections[Mathf.Abs(connId) - 1];
            }

            // vis2k: fixed: connId == connections.Count was allowed before, but would throw an error, e.g. empty list at [0]
            if (connId < connections.Count)
            {
                return connections[connId];
            }
            else
            {
                if (LogFilter.logWarn) { Debug.LogWarning("ConnectionArray Get invalid index " + connId); }
                return null;
            }
        }

        // call this if the connection may not exist (in disconnect handler)
        public NetworkConnection GetUnsafe(int connId)
        {
            // vis2k: fixed: connId == connections.Count was allowed before, but would throw an error, e.g. empty list at [0]
            return (0 <= connId && connId < connections.Count) ? connections[connId] : null;
        }

        public void Remove(int connId)
        {
            if (connId < 0)
            {
                localConnections[Mathf.Abs(connId) - 1] = null;
                return;
            }

            // vis2k: fixed: connId == connections.Count was allowed before, but would throw an error, e.g. empty list at [0]
            if (connId < connections.Count)
            {
                connections[connId] = null;
            }
            else
            {
                if (LogFilter.logWarn) { Debug.LogWarning("ConnectionArray Remove invalid index " + connId); }
            }
        }

        public int AddLocal(NetworkConnection conn)
        {
            localConnections.Add(conn);
            int index = -localConnections.Count;
            conn.connectionId = index;
            return index;
        }

        public bool ContainsPlayer(GameObject player, out NetworkConnection conn)
        {
            conn = null;
            if (player == null)
                return false;

            for (int i = LocalIndex; i < connections.Count; ++i)
            {
                // vis2k: linq for shorter code
                conn = Get(i);
                if (conn != null && conn.playerControllers.Any(pc => pc.IsValid && pc.gameObject == player))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
#endif //ENABLE_UNET
