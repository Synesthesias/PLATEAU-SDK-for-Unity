using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class PLATEAUOrthographicViewCapture : EditorWindow
{
    private List<GameObject> targetObjects = new List<GameObject>();
    private GameObject triplanarTarget;
    private int imageWidth = 1024;
    private int imageHeight = 1024;
    private string savePath = "Assets/Screenshots/";
    private float textureScale = 1.0f;
    private float blendSharpness = 3.0f;
    private float brightness = 1.0f;

    // キャプチャしたテクスチャを保存
    private string lastFrontTexPath;
    private string lastSideTexPath;
    private string lastTopTexPath;

    [MenuItem("Tools/Orthographic View Capture")]
    public static void ShowWindow()
    {
        GetWindow<PLATEAUOrthographicViewCapture>("3面図キャプチャ & Triplanar適用");
    }

    private void OnGUI()
    {
        GUILayout.Label("3面図キャプチャ & Triplanar適用ツール", EditorStyles.boldLabel);

        // 対象オブジェクトの設定
        GUILayout.Label("キャプチャ対象オブジェクト:", EditorStyles.boldLabel);

        if (GUILayout.Button("選択中のオブジェクトを追加"))
        {
            foreach (GameObject obj in Selection.gameObjects)
            {
                if (!targetObjects.Contains(obj))
                {
                    targetObjects.Add(obj);
                }
            }
        }

        // オブジェクトリストの表示
        for (int i = 0; i < targetObjects.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            targetObjects[i] = (GameObject)EditorGUILayout.ObjectField(targetObjects[i], typeof(GameObject), true);
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

        GUILayout.Space(10);

        // キャプチャ設定
        GUILayout.Label("キャプチャ設定:", EditorStyles.boldLabel);
        imageWidth = EditorGUILayout.IntField("画像幅", imageWidth);
        imageHeight = EditorGUILayout.IntField("画像高さ", imageHeight);
        savePath = EditorGUILayout.TextField("保存パス", savePath);

        GUILayout.Space(10);

        // キャプチャボタン
        if (GUILayout.Button("3面図をキャプチャ"))
        {
            CaptureOrthographicViews();
        }

        GUILayout.Space(20);

        // Triplanar適用設定
        GUILayout.Label("Triplanar適用設定:", EditorStyles.boldLabel);
        triplanarTarget = (GameObject)EditorGUILayout.ObjectField("適用対象オブジェクト", triplanarTarget, typeof(GameObject), true);

        textureScale = EditorGUILayout.FloatField("テクスチャスケール", textureScale);
        blendSharpness = EditorGUILayout.Slider("ブレンドシャープネス", blendSharpness, 1f, 10f);
        brightness = EditorGUILayout.Slider("明度", brightness, 0f, 2f);

        GUI.enabled = triplanarTarget != null && !string.IsNullOrEmpty(lastFrontTexPath);
        if (GUILayout.Button("Triplanar投影を適用"))
        {
            ApplyTriplanarProjection();
        }
        GUI.enabled = true;

        if (triplanarTarget != null && string.IsNullOrEmpty(lastFrontTexPath))
        {
            EditorGUILayout.HelpBox("先に3面図をキャプチャしてください", MessageType.Warning);
        }
    }

    private void CaptureOrthographicViews()
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

        // 全オブジェクトの統合バウンディングボックスを計算
        Bounds combinedBounds = CalculateCombinedBounds();

        // 一時的なカメラを作成
        GameObject cameraObj = new GameObject("TempOrthographicCamera");
        Camera camera = cameraObj.AddComponent<Camera>();
        camera.orthographic = true;
        camera.backgroundColor = Color.white;
        camera.clearFlags = CameraClearFlags.SolidColor;

        try
        {
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // 正面図 (Front View - Z軸から)
            lastFrontTexPath = CaptureView(camera, combinedBounds, Vector3.forward, Vector3.up, "Front", timestamp);

            // 側面図 (Side View - X軸から)
            lastSideTexPath = CaptureView(camera, combinedBounds, Vector3.right, Vector3.up, "Side", timestamp);

            // 上面図 (Top View - Y軸から)
            lastTopTexPath = CaptureView(camera, combinedBounds, Vector3.up, Vector3.forward, "Top", timestamp);

            // アセットデータベースを更新
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("完了", $"3面図を {savePath} に保存しました", "OK");
        }
        finally
        {
            // 一時的なカメラを削除
            DestroyImmediate(cameraObj);
        }
    }

    private Bounds CalculateCombinedBounds()
    {
        Bounds bounds = new Bounds();
        bool boundsInitialized = false;

        foreach (GameObject obj in targetObjects)
        {
            if (obj == null) continue;

            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (!boundsInitialized)
                {
                    bounds = renderer.bounds;
                    boundsInitialized = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }
        }

        return bounds;
    }

    private string CaptureView(Camera camera, Bounds bounds, Vector3 direction, Vector3 up, string viewName, string timestamp)
    {
        // カメラの位置と向きを設定
        Vector3 center = bounds.center;
        float distance = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) * 2f;

        camera.transform.position = center + direction * distance;
        camera.transform.LookAt(center, up);

        // 直交投影のサイズを設定
        float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        camera.orthographicSize = maxSize * 0.6f;

        // ニアクリップとファークリップを設定
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = distance * 2f;

        // レンダーテクスチャを作成
        RenderTexture renderTexture = new RenderTexture(imageWidth, imageHeight, 24);
        camera.targetTexture = renderTexture;

        // レンダリング実行
        camera.Render();

        // テクスチャを読み取り
        RenderTexture.active = renderTexture;
        Texture2D screenshot = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
        screenshot.Apply();

        // ファイルに保存
        byte[] data = screenshot.EncodeToPNG();
        string fileName = $"OrthographicView_{viewName}_{timestamp}.png";
        string filePath = Path.Combine(savePath, fileName);
        File.WriteAllBytes(filePath, data);

        // クリーンアップ
        RenderTexture.active = null;
        camera.targetTexture = null;
        DestroyImmediate(renderTexture);
        DestroyImmediate(screenshot);

        Debug.Log($"{viewName}面図を保存しました: {filePath}");

        return filePath;
    }

    private void ApplyTriplanarProjection()
    {
        if (triplanarTarget == null)
        {
            EditorUtility.DisplayDialog("エラー", "適用対象オブジェクトが設定されていません", "OK");
            return;
        }

        // Triplanarシェーダーを読み込み
        Shader triplanarShader = Shader.Find("Custom/TriplanarProjection");
        if (triplanarShader == null)
        {
            EditorUtility.DisplayDialog("エラー", "TriplanarProjectionシェーダーが見つかりません", "OK");
            return;
        }

        // マテリアルを作成
        Material triplanarMaterial = new Material(triplanarShader);

        // テクスチャを読み込み
        Texture2D frontTex = AssetDatabase.LoadAssetAtPath<Texture2D>(lastFrontTexPath);
        Texture2D sideTex = AssetDatabase.LoadAssetAtPath<Texture2D>(lastSideTexPath);
        Texture2D topTex = AssetDatabase.LoadAssetAtPath<Texture2D>(lastTopTexPath);

        if (frontTex == null || sideTex == null || topTex == null)
        {
            EditorUtility.DisplayDialog("エラー", "テクスチャの読み込みに失敗しました", "OK");
            return;
        }

        // マテリアルにテクスチャを設定
        triplanarMaterial.SetTexture("_FrontTex", frontTex);
        triplanarMaterial.SetTexture("_SideTex", sideTex);
        triplanarMaterial.SetTexture("_TopTex", topTex);
        triplanarMaterial.SetFloat("_Scale", textureScale);
        triplanarMaterial.SetFloat("_BlendSharpness", blendSharpness);
        triplanarMaterial.SetFloat("_Brightness", brightness);

        // マテリアルを保存
        string materialPath = Path.Combine(savePath, $"TriplanarMaterial_{System.DateTime.Now:yyyyMMdd_HHmmss}.mat");
        AssetDatabase.CreateAsset(triplanarMaterial, materialPath);

        // 対象オブジェクトのRendererにマテリアルを適用
        Renderer[] renderers = triplanarTarget.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material = triplanarMaterial;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("完了", $"Triplanar投影を適用しました\nマテリアル: {materialPath}", "OK");
    }
}
