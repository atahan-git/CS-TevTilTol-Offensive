// vis2k: GUILayout instead of spacey += ...; removed Update hotkeys to avoid
// confusion if someone accidentally presses one.
using System;
using System.ComponentModel;

#if ENABLE_UNET

namespace UnityEngine.Networking
{
    [AddComponentMenu("Network/NetworkManagerHUD")]
    [RequireComponent(typeof(NetworkManager))]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class NetworkManagerHUD : MonoBehaviour
    {
        public NetworkManager manager;
        public bool showGUI = true;
        public int offsetX;
        public int offsetY;

        // Runtime variable
        bool m_ShowServer;

        void Awake()
        {
            manager = GetComponent<NetworkManager>();
        }

        void OnGUI()
        {
            if (!showGUI) return;

            bool noConnection = (manager.client == null || manager.client.connection == null ||
                                 manager.client.connection.connectionId == -1);

            // guilayout with proper background so it's always readable
            GUILayout.BeginArea(new Rect(5 + offsetX, 5 + offsetY, 215, 9999));
            GUILayout.BeginVertical("Box");
            if (!manager.IsClientConnected() && !NetworkServer.active && manager.matchMaker == null)
            {
                if (noConnection)
                {
                    // LAN Host
                    if (UnityEngine.Application.platform != RuntimePlatform.WebGLPlayer)
                    {
                        if (GUILayout.Button("LAN Host"))
                        {
                            manager.StartHost();
                        }
                    }

                    // LAN Client + IP
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("LAN Client"))
                    {
                        manager.StartClient();
                    }
                    manager.networkAddress = GUILayout.TextField(manager.networkAddress);
                    GUILayout.EndHorizontal();

                    // LAN Server Only
                    if (UnityEngine.Application.platform == RuntimePlatform.WebGLPlayer)
                    {
                        // cant be a server in webgl build
                        GUILayout.Box("(  WebGL cannot be server  )");
                    }
                    else
                    {
                        if (GUILayout.Button("LAN Server Only")) manager.StartServer();
                    }
                }
                else
                {
                    // Connecting
                    GUILayout.Label("Connecting to " + manager.networkAddress + ":" + manager.networkPort + "..");
                    if (GUILayout.Button("Cancel Connection Attempt"))
                    {
                        manager.StopClient();
                    }
                }
            }
            else
            {
                // server / client status message
                if (NetworkServer.active)
                {
                    string serverMsg = "Server: port=" + manager.networkPort;
                    if (manager.useWebSockets)
                    {
                        serverMsg += " (Using WebSockets)";
                    }

                    GUILayout.Label(serverMsg);
                }
                if (manager.IsClientConnected())
                {
                    GUILayout.Label("Client: address=" + manager.networkAddress + " port=" + manager.networkPort);
                }
            }

            // client ready
            if (manager.IsClientConnected() && !ClientScene.ready)
            {
                if (GUILayout.Button("Client Ready"))
                {
                    ClientScene.Ready(manager.client.connection);

                    if (ClientScene.localPlayers.Count == 0)
                    {
                        ClientScene.AddPlayer(0);
                    }
                }
            }

            // stop
            if (NetworkServer.active || manager.IsClientConnected())
            {
                if (GUILayout.Button("Stop"))
                {
                    manager.StopHost();
                }
            }

            // matchmaker
            GUILayout.Space(10);
            if (!NetworkServer.active && !manager.IsClientConnected() && noConnection)
            {
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    GUILayout.Box("(WebGL cannot use Match Maker)");
                }
                else
                {
                    if (manager.matchMaker == null)
                    {
                        if (GUILayout.Button("Enable Match Maker"))
                        {
                            manager.StartMatchMaker();
                        }
                    }
                    else
                    {
                        if (manager.matchInfo == null)
                        {
                            if (manager.matches == null)
                            {
                                // create match
                                if (GUILayout.Button("Create Internet Match"))
                                {
                                    manager.matchMaker.CreateMatch(manager.matchName, manager.matchSize, true, "", "", "", 0, 0, manager.OnMatchCreate);
                                }

                                // room name
                                GUILayout.BeginHorizontal();
                                GUILayout.Label("Room Name:");
                                manager.matchName = GUILayout.TextField(manager.matchName);
                                GUILayout.EndHorizontal();

                                // find match
                                if (GUILayout.Button("Find Internet Match"))
                                {
                                    manager.matchMaker.ListMatches(0, 20, "", false, 0, 0, manager.OnMatchList);
                                }
                            }
                            else
                            {
                                // list of matches
                                for (int i = 0; i < manager.matches.Count; i++)
                                {
                                    var match = manager.matches[i];
                                    if (GUILayout.Button("Join Match:" + match.name))
                                    {
                                        manager.matchName = match.name;
                                        manager.matchMaker.JoinMatch(match.networkId, "", "", "", 0, 0, manager.OnMatchJoined);
                                    }
                                }

                                // back
                                if (GUILayout.Button("Back to Match Menu"))
                                {
                                    manager.matches = null;
                                }
                            }
                        }

                        // change matchmake server
                        if (GUILayout.Button("Change MM server"))
                        {
                            m_ShowServer = !m_ShowServer;
                        }
                        if (m_ShowServer)
                        {
                            if (GUILayout.Button("Local"))
                            {
                                manager.SetMatchHost("localhost", 1337, false);
                                m_ShowServer = false;
                            }
                            if (GUILayout.Button("Internet"))
                            {
                                manager.SetMatchHost("mm.unet.unity3d.com", 443, true);
                                m_ShowServer = false;
                            }
                            if (GUILayout.Button("Staging"))
                            {
                                manager.SetMatchHost("staging-mm.unet.unity3d.com", 443, true);
                                m_ShowServer = false;
                            }
                        }

                        GUILayout.Label("MM Uri: " + manager.matchMaker.baseUri);

                        if (GUILayout.Button("Disable Match Maker"))
                        {
                            manager.StopMatchMaker();
                        }
                    }
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
#endif //ENABLE_UNET
