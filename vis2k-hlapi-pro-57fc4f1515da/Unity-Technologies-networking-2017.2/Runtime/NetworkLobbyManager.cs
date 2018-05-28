#if ENABLE_UNET

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;

namespace UnityEngine.Networking
{
    [AddComponentMenu("Network/NetworkLobbyManager")]
    public class NetworkLobbyManager : NetworkManager
    {
        struct PendingPlayer
        {
            public NetworkConnection conn;
            public GameObject lobbyPlayer;
        }

        // configuration
        [SerializeField] bool m_ShowLobbyGUI = true;
        [SerializeField] int m_MaxPlayers = 4;
        [SerializeField] int m_MaxPlayersPerConnection = 1;
        [SerializeField] int m_MinPlayers;
        [SerializeField] NetworkLobbyPlayer m_LobbyPlayerPrefab;
        [SerializeField] GameObject m_GamePlayerPrefab;
        [SerializeField] string m_LobbyScene = "";
        [SerializeField] string m_PlayScene = "";

        // runtime data
        List<PendingPlayer> m_PendingPlayers = new List<PendingPlayer>();
        public NetworkLobbyPlayer[] lobbySlots;

        // properties
        public bool showLobbyGUI             { get { return m_ShowLobbyGUI; } set { m_ShowLobbyGUI = value; } }
        public int maxPlayers                { get { return m_MaxPlayers; } set { m_MaxPlayers = value; } }
        public int maxPlayersPerConnection   { get { return m_MaxPlayersPerConnection; } set { m_MaxPlayersPerConnection = value; } }
        public int minPlayers                { get { return m_MinPlayers; } set { m_MinPlayers = value; } }
        public NetworkLobbyPlayer lobbyPlayerPrefab { get { return m_LobbyPlayerPrefab; } set { m_LobbyPlayerPrefab = value; } }
        public GameObject gamePlayerPrefab   { get { return m_GamePlayerPrefab; } set { m_GamePlayerPrefab = value; } }
        public string lobbyScene             { get { return m_LobbyScene; } set { m_LobbyScene = value; offlineScene = value; } }
        public string playScene              { get { return m_PlayScene; } set { m_PlayScene = value; } }

        void OnValidate()
        {
            // vis2k: clamp/min/max instead of if checks
            m_MaxPlayers = Mathf.Max(m_MaxPlayers, 1); // value or 1 if that's bigger
            m_MaxPlayersPerConnection = Mathf.Max(m_MaxPlayersPerConnection, 1); // value or 1 if that's bigger
            m_MaxPlayersPerConnection = Mathf.Min(m_MaxPlayersPerConnection, maxPlayers); // value or max if that's smaller
            m_MinPlayers = Mathf.Clamp(m_MinPlayers, 0, m_MaxPlayers); // between 0 and maxplayers

            if (m_LobbyPlayerPrefab != null && m_LobbyPlayerPrefab.GetComponent<NetworkIdentity>() == null) // vis2k: shorter
            {
                m_LobbyPlayerPrefab = null;
                Debug.LogWarning("LobbyPlayer prefab must have a NetworkIdentity component.");
            }

            if (m_GamePlayerPrefab != null && m_GamePlayerPrefab.GetComponent<NetworkIdentity>() == null) // vis2k: shorter
            {
                m_GamePlayerPrefab = null;
                Debug.LogWarning("GamePlayer prefab must have a NetworkIdentity component.");
            }
        }

        Byte FindSlot()
        {
            // vis2k: .Find instead of for loop. note that Byte.MaxValue needs to be returned in case of not found
            int index = Array.FindIndex(lobbySlots, sl => sl == null);
            return index != -1 ? (byte)index : Byte.MaxValue;
        }

        void SceneLoadedForPlayer(NetworkConnection conn, GameObject lobbyPlayerGameObject)
        {
            var lobbyPlayer = lobbyPlayerGameObject.GetComponent<NetworkLobbyPlayer>();
            if (lobbyPlayer == null)
            {
                // not a lobby player.. dont replace it
                return;
            }

            string loadedSceneName = SceneManager.GetSceneAt(0).name;
            if (LogFilter.logDebug) { Debug.Log("NetworkLobby SceneLoadedForPlayer scene:" + loadedSceneName + " " + conn); }

            if (loadedSceneName == m_LobbyScene)
            {
                // cant be ready in lobby, add to ready list
                PendingPlayer pending;
                pending.conn = conn;
                pending.lobbyPlayer = lobbyPlayerGameObject;
                m_PendingPlayers.Add(pending);
                return;
            }

            var controllerId = lobbyPlayerGameObject.GetComponent<NetworkIdentity>().playerControllerId;
            var gamePlayer = OnLobbyServerCreateGamePlayer(conn, controllerId);
            if (gamePlayer == null)
            {
                // get start position from base class
                Transform startPos = GetStartPosition();
                if (startPos != null)
                {
                    gamePlayer = (GameObject)Instantiate(gamePlayerPrefab, startPos.position, startPos.rotation);
                }
                else
                {
                    gamePlayer = (GameObject)Instantiate(gamePlayerPrefab, Vector3.zero, Quaternion.identity);
                }
            }

            if (!OnLobbyServerSceneLoadedForPlayer(lobbyPlayerGameObject, gamePlayer))
            {
                return;
            }

            // replace lobby player with game player
            NetworkServer.ReplacePlayerForConnection(conn, gamePlayer, controllerId);
        }

        static int CheckConnectionIsReadyToBegin(NetworkConnection conn)
        {
            // vis2k: .Count instead of for loop
            return conn.playerControllers.Count(
                pc => pc.IsValid &&
                      pc.gameObject.GetComponent<NetworkLobbyPlayer>() != null &&
                      pc.gameObject.GetComponent<NetworkLobbyPlayer>().readyToBegin
            );
        }

        public void CheckReadyToBegin()
        {
            string loadedSceneName = SceneManager.GetSceneAt(0).name;
            if (loadedSceneName != m_LobbyScene)
            {
                return;
            }

            int readyCount = 0;
            int playerCount = 0;
            foreach (NetworkConnection conn in NetworkServer.connections) // vis2k: foreach
            {
                if (conn != null) // vis2k: != instead of == and continue
                {
                    playerCount += 1;
                    readyCount += CheckConnectionIsReadyToBegin(conn);
                }
            }

            if (m_MinPlayers > 0 && readyCount < m_MinPlayers)
            {
                // not enough players ready yet.
                return;
            }

            if (readyCount < playerCount)
            {
                // not all players are ready yet
                return;
            }

            m_PendingPlayers.Clear();
            OnLobbyServerPlayersReady();
        }

        public void ServerReturnToLobby()
        {
            if (!NetworkServer.active)
            {
                Debug.Log("ServerReturnToLobby called on client");
                return;
            }
            ServerChangeScene(m_LobbyScene);
        }

        void CallOnClientEnterLobby()
        {
            OnLobbyClientEnter();
            foreach (NetworkLobbyPlayer player in lobbySlots) // vis2k: foreach
            {
                if (player != null) // vis2k: != instead of == and continue
                {
                    player.readyToBegin = false;
                    player.OnClientEnterLobby();
                }
            }
        }

        void CallOnClientExitLobby()
        {
            OnLobbyClientExit();
            foreach (NetworkLobbyPlayer player in lobbySlots) // vis2k: foreach
            {
                if (player != null) // vis2k: != instead of == and continue
                {
                    player.OnClientExitLobby();
                }
            }
        }

        public bool SendReturnToLobby()
        {
            if (client != null && client.isConnected) // vis2k: encapuslate in if instead of early return
            {
                client.Send(MsgType.LobbyReturnToLobby, new EmptyMessage());
                return true;
            }
            return false;
        }

        // ------------------------ server handlers ------------------------

        public override void OnServerConnect(NetworkConnection conn)
        {
            // numPlayers returns the player count including this one, so ok to be equal
            if (numPlayers > maxPlayers)
            {
                if (LogFilter.logWarn) { Debug.LogWarning("NetworkLobbyManager can't accept new connection [" + conn + "], too many players connected."); }
                conn.Disconnect();
                return;
            }

            // cannot join game in progress
            string loadedSceneName = SceneManager.GetSceneAt(0).name;
            if (loadedSceneName != m_LobbyScene)
            {
                if (LogFilter.logWarn) { Debug.LogWarning("NetworkLobbyManager can't accept new connection [" + conn + "], not in lobby and game already in progress."); }
                conn.Disconnect();
                return;
            }

            base.OnServerConnect(conn);

            // when a new client connects, set all old players as dirty so their current ready state is sent out
            foreach (NetworkLobbyPlayer player in lobbySlots) // vis2k: foreach
            {
                if (player != null)
                {
                    player.SetDirtyBit(1);
                }
            }

            OnLobbyServerConnect(conn);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            base.OnServerDisconnect(conn);

            // if lobbyplayer for this connection has not been destroyed by now, then destroy it here
            for (int i = 0; i < lobbySlots.Length; i++)
            {
                var player = lobbySlots[i];
                if (player != null) // vis2k: != instead of == and continue
                {
                    if (player.connectionToClient == conn)
                    {
                        lobbySlots[i] = null;
                        NetworkServer.Destroy(player.gameObject);
                    }
                }
            }

            OnLobbyServerDisconnect(conn);
        }

        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            string loadedSceneName = SceneManager.GetSceneAt(0).name;
            if (loadedSceneName != m_LobbyScene)
            {
                return;
            }

            // check MaxPlayersPerConnection
            int numPlayersForConnection = conn.playerControllers.Count(pc => pc.IsValid); // vis2k: .Count instead of for loop
            if (numPlayersForConnection >= maxPlayersPerConnection)
            {
                if (LogFilter.logWarn) { Debug.LogWarning("NetworkLobbyManager no more players for this connection."); }

                conn.Send(MsgType.LobbyAddPlayerFailed, new EmptyMessage());
                return;
            }

            byte slot = FindSlot();
            if (slot == Byte.MaxValue)
            {
                if (LogFilter.logWarn) { Debug.LogWarning("NetworkLobbyManager no space for more players"); }

                conn.Send(MsgType.LobbyAddPlayerFailed, new EmptyMessage());
                return;
            }

            var newLobbyGameObject = OnLobbyServerCreateLobbyPlayer(conn, playerControllerId);
            if (newLobbyGameObject == null)
            {
                newLobbyGameObject = (GameObject)Instantiate(lobbyPlayerPrefab.gameObject, Vector3.zero, Quaternion.identity);
            }

            var newLobbyPlayer = newLobbyGameObject.GetComponent<NetworkLobbyPlayer>();
            newLobbyPlayer.slot = slot;
            lobbySlots[slot] = newLobbyPlayer;

            NetworkServer.AddPlayerForConnection(conn, newLobbyGameObject, playerControllerId);
        }

        public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
        {
            var playerControllerId = player.playerControllerId;
            byte slot = player.gameObject.GetComponent<NetworkLobbyPlayer>().slot;
            lobbySlots[slot] = null;
            base.OnServerRemovePlayer(conn, player);

            foreach (NetworkLobbyPlayer lobbyPlayer in lobbySlots)
            {
                if (lobbyPlayer != null)
                {
                    lobbyPlayer.readyToBegin = false; // vis2k: no need for GetComponent

                    // vis2k: no more static message caching. code is already too complicated as it is.
                    LobbyReadyToBeginMessage msg = new LobbyReadyToBeginMessage();
                    msg.slotId = lobbyPlayer.slot;
                    msg.readyState = false;
                    NetworkServer.SendToReady(null, MsgType.LobbyReadyToBegin, msg);
                }
            }

            OnLobbyServerPlayerRemoved(conn, playerControllerId);
        }

        public override void ServerChangeScene(string sceneName)
        {
            if (sceneName == m_LobbyScene)
            {
                foreach (NetworkLobbyPlayer player in lobbySlots) // vis2k: foreach
                {
                    if (player != null) // vis2k: != instead of == and early return
                    {
                        // find the game-player object for this connection, and destroy it
                        var uv = player.GetComponent<NetworkIdentity>();

                        PlayerController playerController;
                        if (uv.connectionToClient.GetPlayerController(uv.playerControllerId, out playerController))
                        {
                            NetworkServer.Destroy(playerController.gameObject);
                        }

                        if (NetworkServer.active)
                        {
                            // re-add the lobby object
                            player.GetComponent<NetworkLobbyPlayer>().readyToBegin = false;
                            NetworkServer.ReplacePlayerForConnection(uv.connectionToClient, player.gameObject, uv.playerControllerId);
                        }
                    }
                }
            }
            base.ServerChangeScene(sceneName);
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            if (sceneName != m_LobbyScene)
            {
                // call SceneLoadedForPlayer on any players that become ready while we were loading the scene.
                foreach (PendingPlayer pending in m_PendingPlayers) // vis2k: foreach
                {
                    SceneLoadedForPlayer(pending.conn, pending.lobbyPlayer);
                }
                m_PendingPlayers.Clear();
            }

            OnLobbyServerSceneChanged(sceneName);
        }

        void OnServerReadyToBeginMessage(NetworkMessage netMsg)
        {
            // vis2k: no more static message caching. code is already too complicated as it is.
            LobbyReadyToBeginMessage msg = new LobbyReadyToBeginMessage();
            netMsg.ReadMessage(msg);
            if (LogFilter.logDebug) { Debug.Log("NetworkLobbyManager OnServerReadyToBeginMessage"); }

            PlayerController lobbyController;
            if (!netMsg.conn.GetPlayerController(msg.slotId, out lobbyController))
            {
                if (LogFilter.logError) { Debug.LogError("NetworkLobbyManager OnServerReadyToBeginMessage invalid playerControllerId " + msg.slotId); }
                return;
            }

            // set this player ready
            var lobbyPlayer = lobbyController.gameObject.GetComponent<NetworkLobbyPlayer>();
            lobbyPlayer.readyToBegin = msg.readyState;

            // tell every player that this player is ready
            var outMsg = new LobbyReadyToBeginMessage();
            outMsg.slotId = lobbyPlayer.slot;
            outMsg.readyState = msg.readyState;
            NetworkServer.SendToReady(null, MsgType.LobbyReadyToBegin, outMsg);

            // maybe start the game
            CheckReadyToBegin();
        }

        void OnServerSceneLoadedMessage(NetworkMessage netMsg)
        {

            // vis2k: no more static message caching. code is already too complicated as it is.
            IntegerMessage msg = new IntegerMessage();
            netMsg.ReadMessage(msg);
            if (LogFilter.logDebug) { Debug.Log("NetworkLobbyManager OnSceneLoadedMessage"); }

            PlayerController lobbyController;
            if (!netMsg.conn.GetPlayerController((short)msg.value, out lobbyController))
            {
                if (LogFilter.logError) { Debug.LogError("NetworkLobbyManager OnServerSceneLoadedMessage invalid playerControllerId " + msg.value); }
                return;
            }

            SceneLoadedForPlayer(netMsg.conn, lobbyController.gameObject);
        }

        void OnServerReturnToLobbyMessage(NetworkMessage netMsg)
        {
            if (LogFilter.logDebug) { Debug.Log("NetworkLobbyManager OnServerReturnToLobbyMessage"); }

            ServerReturnToLobby();
        }

        public override void OnStartServer()
        {
            if (string.IsNullOrEmpty(m_LobbyScene))
            {
                if (LogFilter.logError) { Debug.LogError("NetworkLobbyManager LobbyScene is empty. Set the LobbyScene in the inspector for the NetworkLobbyMangaer"); }
                return;
            }

            if (string.IsNullOrEmpty(m_PlayScene))
            {
                if (LogFilter.logError) { Debug.LogError("NetworkLobbyManager PlayScene is empty. Set the PlayScene in the inspector for the NetworkLobbyMangaer"); }
                return;
            }

            if (lobbySlots.Length == 0)
            {
                lobbySlots = new NetworkLobbyPlayer[maxPlayers];
            }

            NetworkServer.RegisterHandler(MsgType.LobbyReadyToBegin, OnServerReadyToBeginMessage);
            NetworkServer.RegisterHandler(MsgType.LobbySceneLoaded, OnServerSceneLoadedMessage);
            NetworkServer.RegisterHandler(MsgType.LobbyReturnToLobby, OnServerReturnToLobbyMessage);

            OnLobbyStartServer();
        }

        public override void OnStartHost()
        {
            OnLobbyStartHost();
        }

        public override void OnStopHost()
        {
            OnLobbyStopHost();
        }

        // ------------------------ client handlers ------------------------

        public override void OnStartClient(NetworkClient lobbyClient)
        {
            if (lobbySlots.Length == 0)
            {
                lobbySlots = new NetworkLobbyPlayer[maxPlayers];
            }

            if (m_LobbyPlayerPrefab != null && m_LobbyPlayerPrefab.gameObject != null) // vis2k: != instead of == for more obvious if/else cases
            {
                ClientScene.RegisterPrefab(m_LobbyPlayerPrefab.gameObject);
            }
            else
            {
                if (LogFilter.logError) { Debug.LogError("NetworkLobbyManager no LobbyPlayer prefab is registered. Please add a LobbyPlayer prefab."); }
            }

            if (m_GamePlayerPrefab != null) // vis2k: != instead of == for more obvious if/else cases
            {
                ClientScene.RegisterPrefab(m_GamePlayerPrefab);
            }
            else
            {
                if (LogFilter.logError) { Debug.LogError("NetworkLobbyManager no GamePlayer prefab is registered. Please add a GamePlayer prefab."); }
            }

            lobbyClient.RegisterHandler(MsgType.LobbyReadyToBegin, OnClientReadyToBegin);
            lobbyClient.RegisterHandler(MsgType.LobbyAddPlayerFailed, OnClientAddPlayerFailedMessage);

            OnLobbyStartClient(lobbyClient);
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            OnLobbyClientConnect(conn);
            CallOnClientEnterLobby();
            base.OnClientConnect(conn);
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            OnLobbyClientDisconnect(conn);
            base.OnClientDisconnect(conn);
        }

        public override void OnStopClient()
        {
            OnLobbyStopClient();
            CallOnClientExitLobby();
        }

        public override void OnClientSceneChanged(NetworkConnection conn)
        {
            string loadedSceneName = SceneManager.GetSceneAt(0).name;
            if (loadedSceneName == m_LobbyScene)
            {
                if (client.isConnected)
                {
                    CallOnClientEnterLobby();
                }
            }
            else
            {
                CallOnClientExitLobby();
            }

            base.OnClientSceneChanged(conn);
            OnLobbyClientSceneChanged(conn);
        }

        void OnClientReadyToBegin(NetworkMessage netMsg)
        {
            // vis2k: no more static message caching. code is already too complicated as it is.
            LobbyReadyToBeginMessage msg = new LobbyReadyToBeginMessage();
            netMsg.ReadMessage(msg);

            if (msg.slotId >= lobbySlots.Count())
            {
                if (LogFilter.logError) { Debug.LogError("NetworkLobbyManager OnClientReadyToBegin invalid lobby slot " + msg.slotId); }
                return;
            }

            var lobbyPlayer = lobbySlots[msg.slotId];
            if (lobbyPlayer == null || lobbyPlayer.gameObject == null)
            {
                if (LogFilter.logError) { Debug.LogError("NetworkLobbyManager OnClientReadyToBegin no player at lobby slot " + msg.slotId); }
                return;
            }

            lobbyPlayer.readyToBegin = msg.readyState;
            lobbyPlayer.OnClientReady(msg.readyState);
        }

        void OnClientAddPlayerFailedMessage(NetworkMessage netMsg)
        {
            if (LogFilter.logDebug) { Debug.Log("NetworkLobbyManager Add Player failed."); }
            OnLobbyClientAddPlayerFailed();
        }

        // ------------------------ lobby server virtuals ------------------------

        public virtual void OnLobbyStartHost()
        {
        }

        public virtual void OnLobbyStopHost()
        {
        }

        public virtual void OnLobbyStartServer()
        {
        }

        public virtual void OnLobbyServerConnect(NetworkConnection conn)
        {
        }

        public virtual void OnLobbyServerDisconnect(NetworkConnection conn)
        {
        }

        public virtual void OnLobbyServerSceneChanged(string sceneName)
        {
        }

        public virtual GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
        {
            return null;
        }

        public virtual GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
        {
            return null;
        }

        public virtual void OnLobbyServerPlayerRemoved(NetworkConnection conn, short playerControllerId)
        {
        }

        // for users to apply settings from their lobby player object to their in-game player object
        public virtual bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
        {
            return true;
        }

        public virtual void OnLobbyServerPlayersReady()
        {
            // all players are readyToBegin, start the game
            ServerChangeScene(m_PlayScene);
        }

        // ------------------------ lobby client virtuals ------------------------

        public virtual void OnLobbyClientEnter()
        {
        }

        public virtual void OnLobbyClientExit()
        {
        }

        public virtual void OnLobbyClientConnect(NetworkConnection conn)
        {
        }

        public virtual void OnLobbyClientDisconnect(NetworkConnection conn)
        {
        }

        public virtual void OnLobbyStartClient(NetworkClient lobbyClient)
        {
        }

        public virtual void OnLobbyStopClient()
        {
        }

        public virtual void OnLobbyClientSceneChanged(NetworkConnection conn)
        {
        }

        // for users to handle adding a player failed on the server
        public virtual void OnLobbyClientAddPlayerFailed()
        {
        }

        // ------------------------ optional UI ------------------------

        void OnGUI()
        {
            if (!showLobbyGUI)
                return;

            string loadedSceneName = SceneManager.GetSceneAt(0).name;
            if (loadedSceneName != m_LobbyScene)
                return;

            Rect backgroundRec = new Rect(90 , 180, 500, 150);
            GUI.Box(backgroundRec, "Players:");

            if (NetworkClient.active)
            {
                Rect addRec = new Rect(100, 300, 120, 20);
                if (GUI.Button(addRec, "Add Player"))
                {
                    TryToAddPlayer();
                }
            }
        }

        public void TryToAddPlayer()
        {
            if (NetworkClient.active)
            {
                short controllerId = -1;
                var controllers = NetworkClient.allClients[0].connection.playerControllers;

                if (controllers.Count < maxPlayers)
                {
                    controllerId = (short)controllers.Count;
                }
                else
                {
                    for (short i = 0; i < maxPlayers; i++)
                    {
                        if (!controllers[i].IsValid)
                        {
                            controllerId = i;
                            break;
                        }
                    }
                }
                if (LogFilter.logDebug) { Debug.Log("NetworkLobbyManager TryToAddPlayer controllerId " + controllerId + " ready:" + ClientScene.ready); }

                if (controllerId != -1) // vis2k: != instead of == and return
                {
                    if (ClientScene.ready)
                    {
                        ClientScene.AddPlayer(controllerId);
                    }
                    else
                    {
                        ClientScene.AddPlayer(NetworkClient.allClients[0].connection, controllerId);
                    }
                }
                else
                {
                    if (LogFilter.logDebug) { Debug.Log("NetworkLobbyManager No Space!"); }
                }
            }
            else
            {
                if (LogFilter.logDebug) { Debug.Log("NetworkLobbyManager NetworkClient not active!"); }
            }
        }
    }
}

#endif //ENABLE_UNET
