// vis2k:
// * OnGUI uses GUILayout now
// * unnecessary getters/setters replaced with just a public variable
#if ENABLE_UNET
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UnityEngine.Networking
{
    public struct NetworkBroadcastResult
    {
        public string serverAddress;
        public byte[] broadcastData;
    }

    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkDiscovery")]
    public class NetworkDiscovery : MonoBehaviour
    {
        const int k_MaxBroadcastMsgSize = 1024;

        // config data
        public int broadcastPort = 47777;
        public int broadcastKey = 2222;
        public int broadcastVersion = 1;
        public int broadcastSubVersion = 1;
        public int broadcastInterval = 1000;
        public bool useNetworkManager = false;
        [SerializeField] string m_BroadcastData = "HELLO";
        public bool showGUI = true;
        public int offsetX;
        public int offsetY;

        // runtime data
        public int hostId = -1;
        public bool running;
        public bool isServer;
        public bool isClient;

        byte[] m_MsgInBuffer = new byte[k_MaxBroadcastMsgSize]; // vis2k: initialize directly
        HostTopology m_DefaultTopology;
        public Dictionary<string, NetworkBroadcastResult> broadcastsReceived;

        public string broadcastData
        {
            get { return m_BroadcastData; }
            set
            {
                m_BroadcastData = value;
                if (useNetworkManager)
                {
                    if (LogFilter.logWarn) { Debug.LogWarning("NetworkDiscovery broadcast data changed while using NetworkManager. This can prevent clients from finding the server. The format of the broadcast data must be 'NetworkManager:IPAddress:Port'."); }
                }
            }
        }

        public bool Initialize()
        {
            if (m_BroadcastData.Length >= k_MaxBroadcastMsgSize)
            {
                if (LogFilter.logError) { Debug.LogError("NetworkDiscovery Initialize - data too large. max is " + k_MaxBroadcastMsgSize); }
                return false;
            }

            if (!NetworkTransport.IsStarted)
            {
                NetworkTransport.Init();
            }

            if (useNetworkManager && NetworkManager.singleton != null)
            {
                m_BroadcastData = "NetworkManager:" + NetworkManager.singleton.networkAddress + ":" + NetworkManager.singleton.networkPort;
                if (LogFilter.logInfo) { Debug.Log("NetworkDiscovery set broadcast data to:" + m_BroadcastData); }
            }

            broadcastsReceived = new Dictionary<string, NetworkBroadcastResult>();

            ConnectionConfig cc = new ConnectionConfig();
            cc.AddChannel(QosType.Unreliable);
            m_DefaultTopology = new HostTopology(cc, 1);

            if (isServer)
                StartAsServer();

            if (isClient)
                StartAsClient();

            return true;
        }

        // listen for broadcasts
        public bool StartAsClient()
        {
            if (hostId != -1 || running)
            {
                if (LogFilter.logWarn) { Debug.LogWarning("NetworkDiscovery StartAsClient already started"); }
                return false;
            }

            if (m_MsgInBuffer == null)
            {
                if (LogFilter.logError) { Debug.LogError("NetworkDiscovery StartAsClient, NetworkDiscovery is not initialized"); }
                return false;
            }

            hostId = NetworkTransport.AddHost(m_DefaultTopology, broadcastPort);
            if (hostId == -1)
            {
                if (LogFilter.logError) { Debug.LogError("NetworkDiscovery StartAsClient - addHost failed"); }
                return false;
            }

            byte error;
            NetworkTransport.SetBroadcastCredentials(hostId, broadcastKey, broadcastVersion, broadcastSubVersion, out error);

            running = true;
            isClient = true;
            if (LogFilter.logDebug) { Debug.Log("StartAsClient Discovery listening"); }
            return true;
        }

        // perform actual broadcasts
        public bool StartAsServer()
        {
            if (hostId != -1 || running)
            {
                if (LogFilter.logWarn) { Debug.LogWarning("NetworkDiscovery StartAsServer already started"); }
                return false;
            }

            hostId = NetworkTransport.AddHost(m_DefaultTopology, 0);
            if (hostId == -1)
            {
                if (LogFilter.logError) { Debug.LogError("NetworkDiscovery StartAsServer - addHost failed"); }
                return false;
            }

            byte err;
            byte[] msgOutBuffer = Encoding.UTF8.GetBytes(m_BroadcastData);
            if (!NetworkTransport.StartBroadcastDiscovery(hostId, broadcastPort, broadcastKey, broadcastVersion, broadcastSubVersion, msgOutBuffer, msgOutBuffer.Length, broadcastInterval, out err))
            {
                if (LogFilter.logError) { Debug.LogError("NetworkDiscovery StartBroadcast failed err: " + err); }
                return false;
            }

            running = true;
            isServer = true;
            if (LogFilter.logDebug) { Debug.Log("StartAsServer Discovery broadcasting"); }
            DontDestroyOnLoad(gameObject);
            return true;
        }

        public void StopBroadcast()
        {
            if (hostId == -1)
            {
                if (LogFilter.logError) { Debug.LogError("NetworkDiscovery StopBroadcast not initialized"); }
                return;
            }

            if (!running)
            {
                Debug.LogWarning("NetworkDiscovery StopBroadcast not started");
                return;
            }
            if (isServer)
            {
                NetworkTransport.StopBroadcastDiscovery();
            }

            NetworkTransport.RemoveHost(hostId);
            hostId = -1;
            running = false;
            isServer = false;
            isClient = false;
            m_MsgInBuffer = null;
            broadcastsReceived = null;
            if (LogFilter.logDebug) { Debug.Log("Stopped Discovery broadcasting"); }
        }

        void Update()
        {
            if (hostId == -1)
                return;

            if (isServer)
                return;

            NetworkEventType networkEvent;
            do
            {
                int connectionId;
                int channelId;
                int receivedSize;
                byte error;
                networkEvent = NetworkTransport.ReceiveFromHost(hostId, out connectionId, out channelId, m_MsgInBuffer, k_MaxBroadcastMsgSize, out receivedSize, out error);

                if (networkEvent == NetworkEventType.BroadcastEvent)
                {
                    NetworkTransport.GetBroadcastConnectionMessage(hostId, m_MsgInBuffer, k_MaxBroadcastMsgSize, out receivedSize, out error);

                    string senderAddr;
                    int senderPort;
                    NetworkTransport.GetBroadcastConnectionInfo(hostId, out senderAddr, out senderPort, out error);

                    var recv = new NetworkBroadcastResult();
                    recv.serverAddress = senderAddr;
                    recv.broadcastData = new byte[receivedSize];
                    Buffer.BlockCopy(m_MsgInBuffer, 0, recv.broadcastData, 0, receivedSize);
                    broadcastsReceived[senderAddr] = recv;

                    OnReceivedBroadcast(senderAddr, Encoding.UTF8.GetString(m_MsgInBuffer));
                }
            }
            while (networkEvent != NetworkEventType.Nothing);
        }

        void OnDestroy()
        {
            if (isServer && running && hostId != -1)
            {
                NetworkTransport.StopBroadcastDiscovery();
                NetworkTransport.RemoveHost(hostId);
            }

            if (isClient && running && hostId != -1)
            {
                NetworkTransport.RemoveHost(hostId);
            }
        }

        public virtual void OnReceivedBroadcast(string fromAddress, string data)
        {
            //Debug.Log("Got broadcast from [" + fromAddress + "] " + data);
        }

        void OnGUI()
        {
            if (!showGUI) return;

            // vis2k: GUILayout instead of GUI + manual spacing
            GUILayout.BeginArea(new Rect(5 + offsetX, 5 + offsetY, 215, 9999));
            GUILayout.BeginVertical("Box");

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                GUILayout.Box("( WebGL cannot broadcast )");
            }
            else
            {
                if (m_MsgInBuffer == null)
                {
                    if (GUILayout.Button("Initialize Broadcast"))
                    {
                        Initialize();
                    }
                }
                else
                {
                    string suffix = "";
                    if (isServer)
                        suffix = "(server)";
                    if (isClient)
                        suffix = "(client)";

                    GUILayout.Label("initialized " + suffix);

                    if (running)
                    {
                        if (GUILayout.Button("Stop"))
                        {
                            StopBroadcast();
                        }

                        if (broadcastsReceived != null)
                        {
                            foreach (var addr in broadcastsReceived.Keys)
                            {
                                var value = broadcastsReceived[addr];
                                if (GUILayout.Button("Game at " + addr) && useNetworkManager)
                                {
                                    string dataString = Encoding.UTF8.GetString(value.broadcastData);
                                    var items = dataString.Split(':');
                                    if (items.Length == 3 && items[0] == "NetworkManager")
                                    {
                                        if (NetworkManager.singleton != null && NetworkManager.singleton.client == null)
                                        {
                                            NetworkManager.singleton.networkAddress = items[1];
                                            NetworkManager.singleton.networkPort = Convert.ToInt32(items[2]);
                                            NetworkManager.singleton.StartClient();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Start Broadcasting"))
                        {
                            StartAsServer();
                        }

                        if (GUILayout.Button("Listen for Broadcast"))
                        {
                            StartAsClient();
                        }
                    }
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
#endif
