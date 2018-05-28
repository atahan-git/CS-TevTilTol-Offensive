using System;
using UnityEngine;

namespace UnityStandardAssets.Utility
{
    public class TimedObjectDestructor : MonoBehaviour
    {
        [SerializeField] private float m_TimeOut = 1.0f;
        [SerializeField] private bool m_DetachChildren = false;
		[SerializeField] private bool m_isHead = false;

        private void Awake()
        {
            Invoke("DestroyNow", m_TimeOut);
        }


        private void DestroyNow()
        {
            if (m_DetachChildren)
            {
                transform.DetachChildren();
            }
			if (m_isHead) {
				Instantiate (STORAGE_Explosions.s.head, transform.position, Quaternion.identity);
			}
            DestroyObject(gameObject);
        }
    }
}
