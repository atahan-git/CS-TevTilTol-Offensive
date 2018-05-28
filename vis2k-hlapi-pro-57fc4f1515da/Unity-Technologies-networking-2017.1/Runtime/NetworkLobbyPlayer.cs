#if ENABLE_UNET

using System;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;

namespace UnityEngine.Networking
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkLobbyPlayer")]
    public class NetworkLobbyPlayer : NetworkBehaviour
    {
        [SerializeField] public bool ShowLobbyGUI = true;

        public byte slot; // vis2k: public instead of get/set wrapper
        public bool readyToBegin; // vis2k: public instead of get/set wrapper

        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public override void OnStartClient()
        {
            var lobby = NetworkManager.singleton as NetworkLobbyManager;
            if (lobby)
            {
                lobby.lobbySlots[slot] = this;
                readyToBegin = false;
                OnClientEnterLobby();
            }
            else
            {
                Debug.LogError("LobbyPlayer could not find a NetworkLobbyManager. The LobbyPlayer requires a NetworkLobbyManager object to function. Make sure that there is one in the scene.");
            }
        }

        public void SendReadyToBeginMessage()
        {
            if (LogFilter.logDebug) { Debug.Log("NetworkLobbyPlayer SendReadyToBeginMessage"); }

            var lobby = NetworkManager.singleton as NetworkLobbyManager;
            if (lobby)
            {
                var msg = new LobbyReadyToBeginMessage();
                msg.slotId = (byte)playerControllerId;
                msg.readyState = true;
                lobby.client.Send(MsgType.LobbyReadyToBegin, msg);
            }
        }

        public void SendNotReadyToBeginMessage()
        {
            if (LogFilter.logDebug) { Debug.Log("NetworkLobbyPlayer SendReadyToBeginMessage"); }

            var lobby = NetworkManager.singleton as NetworkLobbyManager;
            if (lobby)
            {
                var msg = new LobbyReadyToBeginMessage();
                msg.slotId = (byte)playerControllerId;
                msg.readyState = false;
                lobby.client.Send(MsgType.LobbyReadyToBegin, msg);
            }
        }

        public void SendSceneLoadedMessage()
        {
            if (LogFilter.logDebug) { Debug.Log("NetworkLobbyPlayer SendSceneLoadedMessage"); }

            var lobby = NetworkManager.singleton as NetworkLobbyManager;
            if (lobby)
            {
                var msg = new IntegerMessage(playerControllerId);
                lobby.client.Send(MsgType.LobbySceneLoaded, msg);
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var lobby = NetworkManager.singleton as NetworkLobbyManager;
            if (lobby)
            {
                // dont even try this in the startup scene
                // Should we check if the LoadSceneMode is Single or Additive??
                // Can the lobby scene be loaded Additively??
                string loadedSceneName = scene.name;
                if (loadedSceneName == lobby.lobbyScene)
                {
                    return;
                }
            }

            if (isLocalPlayer)
            {
                SendSceneLoadedMessage();
            }
        }

        public void RemovePlayer()
        {
            if (isLocalPlayer && !readyToBegin)
            {
                if (LogFilter.logDebug) { Debug.Log("NetworkLobbyPlayer RemovePlayer"); }

                ClientScene.RemovePlayer(GetComponent<NetworkIdentity>().playerControllerId);
            }
        }

        // ------------------------ callbacks ------------------------

        public virtual void OnClientEnterLobby()
        {
        }

        public virtual void OnClientExitLobby()
        {
        }

        public virtual void OnClientReady(bool readyState)
        {
        }

        // ------------------------ Custom Serialization ------------------------

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            //writer.WritePackedUInt32(1); // vis2k: no point in always writing dirty=1
            writer.Write(slot);
            writer.Write(readyToBegin);
            return true;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            //var dirty = reader.ReadPackedUInt32(); // vis2k: see OnSerialize
            slot = reader.ReadByte();
            readyToBegin = reader.ReadBoolean();
        }

        // ------------------------ optional UI ------------------------

        void OnGUI()
        {
            if (!ShowLobbyGUI)
                return;

            var lobby = NetworkManager.singleton as NetworkLobbyManager;
            if (lobby)
            {
                if (!lobby.showLobbyGUI)
                    return;

                string loadedSceneName = SceneManager.GetSceneAt(0).name;
                if (loadedSceneName != lobby.lobbyScene)
                    return;
            }

            Rect rec = new Rect(100 + slot * 100, 200, 90, 20);

            if (isLocalPlayer)
            {
                string youStr;
                if (readyToBegin)
                {
                    youStr = "(Ready)";
                }
                else
                {
                    youStr = "(Not Ready)";
                }
                GUI.Label(rec, youStr);

                if (readyToBegin)
                {
                    rec.y += 25;
                    if (GUI.Button(rec, "STOP"))
                    {
                        SendNotReadyToBeginMessage();
                    }
                }
                else
                {
                    rec.y += 25;
                    if (GUI.Button(rec, "START"))
                    {
                        SendReadyToBeginMessage();
                    }

                    rec.y += 25;
                    if (GUI.Button(rec, "Remove"))
                    {
                        ClientScene.RemovePlayer(GetComponent<NetworkIdentity>().playerControllerId);
                    }
                }
            }
            else
            {
                GUI.Label(rec, "Player [" + netId + "]");
                rec.y += 25;
                GUI.Label(rec, "Ready [" + readyToBegin + "]");
            }
        }
    }
}

#endif // ENABLE_UNET
