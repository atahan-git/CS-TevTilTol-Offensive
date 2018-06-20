using System;
using UnityEngine;
using UnityEngine.UI;

namespace UnityStandardAssets.Utility
{
    public class TimedObjectDestructor : MonoBehaviour
    {
        [SerializeField] private float m_TimeOut = 1.0f;
        [SerializeField] private bool m_DetachChildren = false;
		[SerializeField] private bool m_isHead = false;
		[SerializeField] private Slider m_slider;

		float timer = 0;

        private void Awake()
        {
            Invoke("DestroyNow", m_TimeOut);
			timer = m_TimeOut;
        }

		void Update (){
			timer -= m_TimeOut;
			if (m_slider != null) {
				m_slider.value = timer / m_TimeOut;
			}
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
