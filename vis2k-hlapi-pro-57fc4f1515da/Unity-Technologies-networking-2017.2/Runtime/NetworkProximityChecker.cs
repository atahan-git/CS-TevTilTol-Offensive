#if ENABLE_UNET
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Networking
{
    [AddComponentMenu("Network/NetworkProximityChecker")]
    [RequireComponent(typeof(NetworkIdentity))]
    public class NetworkProximityChecker : NetworkBehaviour
    {
        public enum CheckMethod
        {
            Physics3D,
            Physics2D
        };

        public int visRange = 10;
        public float visUpdateInterval = 1.0f; // in seconds
        public CheckMethod checkMethod = CheckMethod.Physics3D;

        public bool forceHidden = false;

        float m_VisUpdateTime;

        void Update()
        {
            if (!NetworkServer.active)
                return;

            if (Time.time - m_VisUpdateTime > visUpdateInterval)
            {
                GetComponent<NetworkIdentity>().RebuildObservers(false);
                m_VisUpdateTime = Time.time;
            }
        }

        // called when a new player enters
        public override bool OnCheckObserver(NetworkConnection newObserver)
        {
            if (forceHidden)
                return false;

            // this cant use newObserver.playerControllers[0]. must iterate to find a valid player.
            // vis2k: .Find instead of for loop and Vector3.Distance instead of magnitude
            PlayerController controller = newObserver.playerControllers.Find(
                pc => pc != null && pc.gameObject != null
            );
            if (controller != null)
            {
                GameObject player = controller.gameObject;
                return Vector3.Distance(player.transform.position, transform.position) < visRange;
            }
            return false;
        }

        public override bool OnRebuildObservers(HashSet<NetworkConnection> observers, bool initial)
        {
            if (forceHidden)
            {
                // ensure player can still see themself
                var uv = GetComponent<NetworkIdentity>();
                if (uv.connectionToClient != null)
                {
                    observers.Add(uv.connectionToClient);
                }
                return true;
            }

            // find players within range
            switch (checkMethod)
            {
                case CheckMethod.Physics3D:
                {
                    foreach (Collider hit in Physics.OverlapSphere(transform.position, visRange)) // vis2k: foreach
                    {
                        // (if an object has a connectionToClient, it is a player)
                        var uv = hit.GetComponent<NetworkIdentity>();
                        if (uv != null && uv.connectionToClient != null)
                        {
                            observers.Add(uv.connectionToClient);
                        }
                    }
                    return true;
                }

                case CheckMethod.Physics2D:
                {
                    foreach (Collider2D hit in Physics2D.OverlapCircleAll(transform.position, visRange)) // vis2k: foreach
                    {
                        // (if an object has a connectionToClient, it is a player)
                        var uv = hit.GetComponent<NetworkIdentity>();
                        if (uv != null && uv.connectionToClient != null)
                        {
                            observers.Add(uv.connectionToClient);
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        // called hiding and showing objects on the host
        public override void OnSetLocalVisibility(bool vis)
        {
            // vis2k: replaced custom 'SetVis' function that emulated GetComponentsInChildren
            foreach (var rend in GetComponentsInChildren<Renderer>())
                rend.enabled = vis;
        }
    }
}
#endif
