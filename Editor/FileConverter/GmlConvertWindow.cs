﻿using PlasticPipe.PlasticProtocol.Messages;
using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Editor.FileConverter.GUIContents;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor.FileConverter
{

    public class GmlConvertWindow : EditorWindow
    {
        private GmlSelectorGUI gmlSelectorGUI;
        private bool isInitialized;
        private Vector2 scrollPosition;
        
        [MenuItem("Plateau/都市モデルインポート")]
        public static void Open()
        {
            var window = GetWindow<GmlConvertWindow>("都市モデルインポート");
            window.Show();
            window.Init();
        }

        private void Init()
        {
            this.gmlSelectorGUI = new GmlSelectorGUI();
            this.isInitialized = true;
        }

        private void OnGUI()
        {
            if (!this.isInitialized) Init();
            HeaderDrawer.Reset();
            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
            this.gmlSelectorGUI.DrawGUI();
            EditorGUILayout.EndScrollView();
        }
    }
}