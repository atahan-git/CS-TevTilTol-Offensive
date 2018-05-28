#if ENABLE_UNET
using System;
using System.Collections.Generic;

namespace UnityEngine.Networking
{
    // This is an internal class to allow the client and server to share scene-related functionality.
    // This code (mostly) used to be in ClientScene.
    internal class NetworkScene
    {
        // localObjects is NOT static. For the Host, even though there is one scene and gameObjects are
        // shared with the localClient, the set of active objects for each must be separate to prevent
        // out-of-order object initialization problems.
        internal Dictionary<NetworkInstanceId, NetworkIdentity> localObjects = new Dictionary<NetworkInstanceId, NetworkIdentity>(); // vis2k: internal instead of get/set wrapper

        static internal Dictionary<NetworkHash128, GameObject> guidToPrefab = new Dictionary<NetworkHash128, GameObject>(); // vis2k: internal instead of get/set wrapper
        static internal Dictionary<NetworkHash128, SpawnDelegate> spawnHandlers = new Dictionary<NetworkHash128, SpawnDelegate>(); // vis2k: internal instead of get/set wrapper
        static internal Dictionary<NetworkHash128, UnSpawnDelegate> unspawnHandlers = new Dictionary<NetworkHash128, UnSpawnDelegate>(); // vis2k: internal instead of get/set wrapper

        internal void Shutdown()
        {
            ClearLocalObjects();
            ClearSpawners();
        }

        internal void SetLocalObject(NetworkInstanceId netId, GameObject obj, bool isClient, bool isServer)
        {
            if (LogFilter.logDev) { Debug.Log("SetLocalObject " + netId + " " + obj); }

            if (obj == null)
            {
                localObjects[netId] = null;
                return;
            }

            NetworkIdentity foundNetworkIdentity = null;
            if (localObjects.ContainsKey(netId))
            {
                foundNetworkIdentity = localObjects[netId];
            }

            if (foundNetworkIdentity == null)
            {
                foundNetworkIdentity = obj.GetComponent<NetworkIdentity>();
                localObjects[netId] = foundNetworkIdentity;
            }

            foundNetworkIdentity.UpdateClientServer(isClient, isServer);
        }

        // this lets the client take an instance ID from the server and find
        // the local object that it corresponds too. This is temporary until
        // object references can be serialized transparently.
        internal GameObject FindLocalObject(NetworkInstanceId netId)
        {
            if (localObjects.ContainsKey(netId))
            {
                var uv = localObjects[netId];
                if (uv != null)
                {
                    return uv.gameObject;
                }
            }
            return null;
        }

        internal bool GetNetworkIdentity(NetworkInstanceId netId, out NetworkIdentity uv)
        {
            if (localObjects.ContainsKey(netId) && localObjects[netId] != null)
            {
                uv = localObjects[netId];
                return true;
            }
            uv = null;
            return false;
        }

        internal bool RemoveLocalObject(NetworkInstanceId netId)
        {
            return localObjects.Remove(netId);
        }

        internal bool RemoveLocalObjectAndDestroy(NetworkInstanceId netId)
        {
            if (localObjects.ContainsKey(netId))
            {
                NetworkIdentity localObject = localObjects[netId];
                Object.Destroy(localObject.gameObject);
                return localObjects.Remove(netId);
            }
            return false;
        }

        internal void ClearLocalObjects()
        {
            localObjects.Clear();
        }

        static internal void RegisterPrefab(GameObject prefab, NetworkHash128 newAssetId)
        {
            NetworkIdentity view = prefab.GetComponent<NetworkIdentity>();
            if (view)
            {
                view.SetDynamicAssetId(newAssetId);

                if (LogFilter.logDebug) { Debug.Log("Registering prefab '" + prefab.name + "' as asset:" + view.assetId); }
                guidToPrefab[view.assetId] = prefab;
            }
            else
            {
                if (LogFilter.logError) { Debug.LogError("Could not register '" + prefab.name + "' since it contains no NetworkIdentity component"); }
            }
        }

        static internal void RegisterPrefab(GameObject prefab)
        {
            NetworkIdentity view = prefab.GetComponent<NetworkIdentity>();
            if (view)
            {
                if (LogFilter.logDebug) { Debug.Log("Registering prefab '" + prefab.name + "' as asset:" + view.assetId); }
                guidToPrefab[view.assetId] = prefab;
                // vis2k: NetworkIdentity child amount check moved to NetworkIdentity.OnValidate
            }
            else
            {
                if (LogFilter.logError) { Debug.LogError("Could not register '" + prefab.name + "' since it contains no NetworkIdentity component"); }
            }
        }

        static internal bool GetPrefab(NetworkHash128 assetId, out GameObject prefab)
        {
            prefab = null; // vis2k: set null initially so the following code is shorter
            if (assetId.IsValid()) // vis2k: valid instead of !valid and early return
            {
                if (guidToPrefab.ContainsKey(assetId) && guidToPrefab[assetId] != null)
                {
                    prefab = guidToPrefab[assetId];
                    return true;
                }
            }
            return false;
        }

        static internal void ClearSpawners()
        {
            guidToPrefab.Clear();
            spawnHandlers.Clear();
            unspawnHandlers.Clear();
        }

        static public void UnregisterSpawnHandler(NetworkHash128 assetId)
        {
            spawnHandlers.Remove(assetId);
            unspawnHandlers.Remove(assetId);
        }

        static internal void RegisterSpawnHandler(NetworkHash128 assetId, SpawnDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
        {
            if (spawnHandler == null || unspawnHandler == null)
            {
                if (LogFilter.logError) { Debug.LogError("RegisterSpawnHandler custom spawn function null for " + assetId); }
                return;
            }

            if (LogFilter.logDebug) { Debug.Log("RegisterSpawnHandler asset '" + assetId + "' " + spawnHandler.GetMethodName() + "/" + unspawnHandler.GetMethodName()); }

            spawnHandlers[assetId] = spawnHandler;
            unspawnHandlers[assetId] = unspawnHandler;
        }

        static internal void UnregisterPrefab(GameObject prefab)
        {
            NetworkIdentity identity = prefab.GetComponent<NetworkIdentity>();
            if (identity == null)
            {
                if (LogFilter.logError) { Debug.LogError("Could not unregister '" + prefab.name + "' since it contains no NetworkIdentity component"); }
                return;
            }
            spawnHandlers.Remove(identity.assetId);
            unspawnHandlers.Remove(identity.assetId);
        }

        static internal void RegisterPrefab(GameObject prefab, SpawnDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
        {
            NetworkIdentity identity = prefab.GetComponent<NetworkIdentity>();
            if (identity == null)
            {
                if (LogFilter.logError) { Debug.LogError("Could not register '" + prefab.name + "' since it contains no NetworkIdentity component"); }
                return;
            }

            if (spawnHandler == null || unspawnHandler == null)
            {
                if (LogFilter.logError) { Debug.LogError("RegisterPrefab custom spawn function null for " + identity.assetId); }
                return;
            }

            if (!identity.assetId.IsValid())
            {
                if (LogFilter.logError) { Debug.LogError("RegisterPrefab game object " + prefab.name + " has no prefab. Use RegisterSpawnHandler() instead?"); }
                return;
            }

            if (LogFilter.logDebug) { Debug.Log("Registering custom prefab '" + prefab.name + "' as asset:" + identity.assetId + " " + spawnHandler.GetMethodName() + "/" + unspawnHandler.GetMethodName()); }

            spawnHandlers[identity.assetId] = spawnHandler;
            unspawnHandlers[identity.assetId] = unspawnHandler;
        }

        static internal bool GetSpawnHandler(NetworkHash128 assetId, out SpawnDelegate handler)
        {
            if (spawnHandlers.ContainsKey(assetId))
            {
                handler = spawnHandlers[assetId];
                return true;
            }
            handler = null;
            return false;
        }

        static internal bool InvokeUnSpawnHandler(NetworkHash128 assetId, GameObject obj)
        {
            if (unspawnHandlers.ContainsKey(assetId) && unspawnHandlers[assetId] != null)
            {
                UnSpawnDelegate handler = unspawnHandlers[assetId];
                handler(obj);
                return true;
            }
            return false;
        }

        internal void DestroyAllClientObjects()
        {
            foreach (var netId in localObjects.Keys)
            {
                NetworkIdentity uv = localObjects[netId];

                if (uv != null && uv.gameObject != null)
                {
                    if (!InvokeUnSpawnHandler(uv.assetId, uv.gameObject))
                    {
                        if (uv.sceneId.IsEmpty())
                        {
                            Object.Destroy(uv.gameObject);
                        }
                        else
                        {
                            uv.MarkForReset();
                            uv.gameObject.SetActive(false);
                        }
                    }
                }
            }
            ClearLocalObjects();
        }

        internal void DumpAllClientObjects()
        {
            foreach (var netId in localObjects.Keys)
            {
                NetworkIdentity uv = localObjects[netId];
                if (uv != null)
                    Debug.Log("ID:" + netId + " OBJ:" + uv.gameObject + " AS:" + uv.assetId);
                else
                    Debug.Log("ID:" + netId + " OBJ: null");
            }
        }
    }
}
#endif //ENABLE_UNET
