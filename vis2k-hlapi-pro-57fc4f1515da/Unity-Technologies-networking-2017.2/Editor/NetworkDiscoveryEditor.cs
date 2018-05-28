#if ENABLE_UNET
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityEditor
{
    [CustomEditor(typeof(NetworkDiscovery), true)]
    [CanEditMultipleObjects]
    public class NetworkDiscoveryEditor : Editor
    {
        bool m_Initialized;
        NetworkDiscovery m_Discovery;

        SerializedProperty broadcastPortProperty;
        SerializedProperty broadcastKeyProperty;
        SerializedProperty broadcastVersionProperty;
        SerializedProperty broadcastSubVersionProperty;
        SerializedProperty broadcastIntervalProperty;
        SerializedProperty useNetworkManagerProperty;
        SerializedProperty m_BroadcastDataProperty;
        SerializedProperty showGUIProperty;
        SerializedProperty offsetXProperty;
        SerializedProperty offsetYProperty;

        GUIContent broadcastPortLabel;
        GUIContent broadcastKeyLabel;
        GUIContent broadcastVersionLabel;
        GUIContent broadcastSubVersionLabel;
        GUIContent broadcastIntervalLabel;
        GUIContent useNetworkManagerLabel;
        GUIContent m_BroadcastDataLabel;

        void Init()
        {
            if (m_Initialized)
            {
                if (broadcastPortProperty == null)
                {
                    // need to re-init
                }
                else
                {
                    return;
                }
            }

            m_Initialized = true;
            m_Discovery = target as NetworkDiscovery;

            broadcastPortProperty = serializedObject.FindProperty("broadcastPort");
            broadcastKeyProperty = serializedObject.FindProperty("broadcastKey");
            broadcastVersionProperty = serializedObject.FindProperty("broadcastVersion");
            broadcastSubVersionProperty = serializedObject.FindProperty("broadcastSubVersion");
            broadcastIntervalProperty = serializedObject.FindProperty("broadcastInterval");
            useNetworkManagerProperty = serializedObject.FindProperty("useNetworkManager");
            m_BroadcastDataProperty = serializedObject.FindProperty("m_BroadcastData");
            showGUIProperty = serializedObject.FindProperty("showGUI");
            offsetXProperty = serializedObject.FindProperty("offsetX");
            offsetYProperty = serializedObject.FindProperty("offsetY");

            broadcastPortLabel = new GUIContent("Broadcast Port", "The network port to broadcast to, and listen on.");
            broadcastKeyLabel = new GUIContent("Broadcast Key", "The key to broadcast. This key typically identifies the application.");
            broadcastVersionLabel = new GUIContent("Broadcast Version", "The version of the application to broadcast. This is used to match versions of the same application.");
            broadcastSubVersionLabel = new GUIContent("Broadcast SubVersion", "The sub-version of the application to broadcast.");
            broadcastIntervalLabel = new GUIContent("Broadcast Interval", "How often in milliseconds to broadcast when running as a server.");
            useNetworkManagerLabel = new GUIContent("Use NetworkManager", "Broadcast information from the NetworkManager, and auto-join matching games using the NetworkManager.");
            m_BroadcastDataLabel = new GUIContent("Broadcast Data", "The data to broadcast when not using the NetworkManager");
        }

        public override void OnInspectorGUI()
        {
            Init();
            serializedObject.Update();
            DrawControls();
            serializedObject.ApplyModifiedProperties();
        }

        void DrawControls()
        {
            if (m_Discovery == null)
                return;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(broadcastPortProperty, broadcastPortLabel);

            EditorGUILayout.PropertyField(broadcastKeyProperty, broadcastKeyLabel);
            EditorGUILayout.PropertyField(broadcastVersionProperty, broadcastVersionLabel);
            EditorGUILayout.PropertyField(broadcastSubVersionProperty, broadcastSubVersionLabel);
            EditorGUILayout.PropertyField(broadcastIntervalProperty, broadcastIntervalLabel);
            EditorGUILayout.PropertyField(useNetworkManagerProperty, useNetworkManagerLabel);
            if (m_Discovery.useNetworkManager)
            {
                EditorGUILayout.LabelField(m_BroadcastDataLabel, new GUIContent(m_BroadcastDataProperty.stringValue));
            }
            else
            {
                EditorGUILayout.PropertyField(m_BroadcastDataProperty, m_BroadcastDataLabel);
            }

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(showGUIProperty);
            if (m_Discovery.showGUI)
            {
                EditorGUILayout.PropertyField(offsetXProperty);
                EditorGUILayout.PropertyField(offsetYProperty);
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            if (Application.isPlaying)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("hostId", m_Discovery.hostId.ToString());
                EditorGUILayout.LabelField("running", m_Discovery.running.ToString());
                EditorGUILayout.LabelField("isServer", m_Discovery.isServer.ToString());
                EditorGUILayout.LabelField("isClient", m_Discovery.isClient.ToString());
            }
        }
    }
}
#endif
