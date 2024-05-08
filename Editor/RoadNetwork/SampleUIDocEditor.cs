using Codice.CM.Common.Serialization.Replication;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine.UIElements;

namespace PLATEAU
{
    public class SampleUIDocEditor : EditorWindow
    {

        [MenuItem("Sample/Sample1")]
        public static void ShowWindow()
        {
            GetWindow<SampleUIDocEditor>("SampleWindow");
        }


        private void OnEnable()
        {
            //string assetPath = "Packages/PlateauUnitySDK/Resources/RoadNetworkEditor.uxml";
            string assetPath = "Packages/com.synesthesias.plateau-unity-sdk/Resources/TestRoadNetworkEditor.uxml";
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
            var root = rootVisualElement;
            visualTree.CloneTree(root);

            // これ以下をMono/EditorWindow類を継承しないクラスに実装
            // ただし　インスペクタに表示する際にはシリアライズ可能なオブジェクトに値を設定する処理が入る
            // コールバックを用意する？ or Inspector拡張版を用意する？
            // UI ToolkitをInspector拡張で利用する　https://gamedev65535.com/entry/uitoolkit_base/ 

            var uiIntInput = root.Q<IntegerField>("intInput");
            var uiBtnAdd = root.Q<Button>("btnAdd");
            var uiBtnClear = root.Q<Button>("btnClear");
            var uiLblAnswer = root.Q<Label>("lblAnswer");

            //足し算ボタン押下
            uiBtnAdd.clickable.clicked += () =>
            {
                int answer = uiIntInput.value + int.Parse(uiLblAnswer.text);
                uiLblAnswer.text = answer.ToString();
            };

            //クリアボタン押下
            uiBtnClear.clickable.clicked += () =>
            {
                uiLblAnswer.text = "0";
            };
        }
    } 
}