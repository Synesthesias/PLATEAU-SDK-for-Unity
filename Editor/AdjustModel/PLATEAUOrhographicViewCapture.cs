using PLATEAU.CityInfo;
using PLATEAU.Editor.AdjustModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.AdjustModel
{
    public class PLATEAUOrthographicViewCapture : EditorWindow
    {
        /// <summary>
        /// 変換対象のオブジェクト
        /// </summary>
        private static List<PLATEAUInstancedCityModel> targetObjects = new();
        /// <summary>
        /// テクスチャの解像度. 1m当たりのピクセル数
        /// </summary>
        private static float pixelsPerMeter = 1f;
        /// <summary>
        /// 保存先
        /// </summary>
        private static string savePath = "Assets/PLATEAU/Lod1";
        /// <summary>
        /// カメラの背景設定
        /// </summary>
        private static CameraClearFlags cameraClearFlag = CameraClearFlags.Nothing;
        /// <summary>
        /// カメラの背景色
        /// </summary>
        private static Color cameraClearColor = Color.black;
        /// <summary>
        /// LOD1のマテリアルを戻すための物
        /// </summary>
        private static Material defaultMaterial;

        [MenuItem("PLATEAU/Debug/Orthographic View Capture")]
        public static void ShowWindow()
        {
            GetWindow<PLATEAUOrthographicViewCapture>("LOD1 5面図キャプチャ");
        }

        /// <summary>
        /// LOD1オブジェクト/LOD2以上のオブジェクトのビジブル切り替えを行う
        /// </summary>
        /// <param name="isLod1Visible"></param>
        private void SwitchLod1Visible(bool isLod1Visible)
        {
            var service = new Lod1TextureCaptureService(savePath, pixelsPerMeter, false,cameraClearFlag, cameraClearColor);
            foreach (var model in targetObjects)
            {
                if (!model)
                    continue;
                service.SwitchLod1Visible(model.gameObject, isLod1Visible);
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("LOD1マテリアル生成", EditorStyles.boldLabel);

            // 対象オブジェクトの設定
            GUILayout.Label("キャプチャ対象オブジェクト:", EditorStyles.boldLabel);

            if (GUILayout.Button("選択中のオブジェクトを追加"))
            {
                foreach (GameObject obj in Selection.gameObjects)
                {
                    var cityModel = obj.GetComponent<PLATEAUInstancedCityModel>();
                    if (!cityModel)
                        continue;
                    if (!targetObjects.Contains(cityModel))
                    {
                        targetObjects.Add(cityModel);
                    }
                }
            }
            // オブジェクトリストの表示
            for (int i = 0; i < targetObjects.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                targetObjects[i] = (PLATEAUInstancedCityModel)EditorGUILayout.ObjectField(targetObjects[i], typeof(PLATEAUInstancedCityModel), true);
                if (GUILayout.Button("削除", GUILayout.Width(50)))
                {
                    targetObjects.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("リストをクリア"))
            {
                targetObjects.Clear();
            }

            using (var _ = new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Show LOD1"))
                {
                    SwitchLod1Visible(true);
                }
                if (GUILayout.Button("Show LOD2"))
                {
                    SwitchLod1Visible(false);
                }
            }

            GUILayout.Space(10);

            // キャプチャ設定
            GUILayout.Label("キャプチャ設定:", EditorStyles.boldLabel);
            pixelsPerMeter = EditorGUILayout.FloatField("Pixels per Meter", pixelsPerMeter);
            savePath = EditorGUILayout.TextField("保存パス", savePath);
            cameraClearFlag = (CameraClearFlags)EditorGUILayout.EnumPopup("Camera Clear Flag", cameraClearFlag);
            cameraClearColor = EditorGUILayout.ColorField("Camera Clear Color", cameraClearColor);
            defaultMaterial = (Material)EditorGUILayout.ObjectField("DefaultMaterial", defaultMaterial, typeof(Material), true);
            GUILayout.Space(10);

            // キャプチャボタン
            if (GUILayout.Button("実行"))
            {
                Execute();
            }
        }

        private void Execute()
        {
            if (targetObjects.Count == 0)
            {
                EditorUtility.DisplayDialog("エラー", "対象オブジェクトが設定されていません", "OK");
                return;
            }

            // 保存ディレクトリの作成
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            // 一時的なカメラを作成
            GameObject cameraObj = new GameObject("TempOrthographicCamera");
            Camera camera = cameraObj.AddComponent<Camera>();
            camera.orthographic = true;
            camera.backgroundColor = cameraClearColor;
            camera.clearFlags = cameraClearFlag;

            try
            {
                var service = new Lod1TextureCaptureService(savePath, pixelsPerMeter, false,cameraClearFlag, cameraClearColor);
                service.Execute(camera, targetObjects.Select(m=>m.gameObject));

                // defaultMaterialが設定されていればLOD1マテリアルを設定
                if (defaultMaterial != null)
                {
                    foreach (var model in targetObjects)
                    {
                        if (!model)
                            continue;
                        service.SetLod1Material(model.gameObject, defaultMaterial);
                    }
                }

                // アセットデータベースを更新
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("完了", $"マテリアルを {savePath} に保存しました", "OK");
            }
            finally
            {
                // 一時的なカメラを削除
                DestroyImmediate(cameraObj);
            }
        }
    }
}