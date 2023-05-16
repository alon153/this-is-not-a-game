// using System;
// using System.Collections.Generic;
// using UnityEditor;
// using UnityEngine;
//
// namespace Audio
// {
//     [CustomEditor(typeof(AudioBank))]
//     public class AudioBankEditor : UnityEditor.Editor
//     {
//         private SerializedProperty _type;
//         private SerializedProperty _playerSounds;
//         private SerializedProperty _yoyoSounds;
//         private SerializedProperty _reference;
//
//         private void OnEnable()
//         {
//             _type = serializedObject.FindProperty("_type");
//             _playerSounds = serializedObject.FindProperty("_playerSounds");
//             _yoyoSounds = serializedObject.FindProperty("_yoyoSounds");
//             _reference = serializedObject.FindProperty("_reference");
//         }
//
//         public override void OnInspectorGUI()
//         {
//             base.OnInspectorGUI();
//             var bank = (AudioBank) target;
//             
//             serializedObject.Update();
//             
//             GUILayout.Label("Player Sounds:");
//             EditorGUI.indentLevel++;
//             foreach (var pair in bank.PlayerRefs)
//             {
//                 GUILayout.Label($"{pair._type}: {pair._reference.Path}");
//             }
//             
//             GUILayout.Label("Yoyo Sounds:");
//             EditorGUI.indentLevel--;
//             
//             Enum enumVal = null;
//             PlayerSounds player = 0;
//             YoyoSounds yoyo = 0;
//
//             SoundType _soundType = (SoundType) _type.enumValueIndex; 
//             enumVal = _soundType switch
//             {
//                 SoundType.PLAYER => EditorGUILayout.EnumPopup("Sound Tag: ", player),
//                 SoundType.YOYO => EditorGUILayout.EnumPopup("Sound Tag: ", yoyo),
//                 _ => enumVal
//             };
//
//             if (GUILayout.Button("Add Sound"))
//                 bank.AddSound(_soundType, enumVal, _reference.serializedObject.targetObject.ToString());
//
//             serializedObject.ApplyModifiedProperties();
//         }
//     }
//     
// }