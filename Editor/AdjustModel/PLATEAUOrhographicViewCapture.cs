using PlasticGui.WorkspaceWindow.Locks;
using PLATEAU.CityInfo;
using PLATEAU.PolygonMesh;
using PLATEAU.RoadNetwork.Graph;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;

public class PLATEAUOrthographicViewCapture : EditorWindow
{
    private static List<PLATEAUInstancedCityModel> targetObjects = new();
    private GameObject triplanarTarget;
    private static float pixelsPerMeter = 1f;
    private static string savePath = "Assets/Screenshots/";
    private static float textureScale = 1.0f;
    private static float blendSharpness = 3.0f;
    private static float brightness = 1.0f;
    // キャプチャしたテクスチャを保存
    private static string lastFrontTexPath;
    private static string lastSideTexPath;
    private static string lastTopTexPath;
    private static CameraClearFlags cameraClearFlag = CameraClearFlags.Nothing;
    private static Color cameraClearColor = Color.black;
    //private static int layerIndex = 6;

    //private static Shader triplanarShader;
    enum Face
    {
        Front,
        Back,
        Left,
        Right,
        Top
    }


    class CaptureRequest
    {
        /// <summary>
        /// 一つの面に関するリクエスト情報
        /// </summary>
        public record FaceRequest
        {
            public string TexturePath { get; }

            public Vector3 Direction { get; }

            public Vector3 Up { get; }

            public Vector2Int ImageSize { get; }

            public Vector3 Coef { get; }

            public FaceRequest(Vector3 direction, Vector3 up, Vector2 imageSize)
            {
                Direction = direction;
                Up = up;
                ImageSize = imageSize.FloorToInt();
            }
        }

        /// <summary>
        /// メッシュコード
        /// </summary>
        public string MeshCode { get; }

        /// <summary>
        /// バウンディングボックス
        /// </summary>
        public Bounds Bounds { get; }

        public Vector3 XCoef { get; }

        public Vector3 YCoef { get; }
        public Vector3 ZCoef { get; }

        public float PixelsPerMeter { get; }
        /// <summary>
        /// 各面ごとの情報
        /// </summary>
        public IReadOnlyDictionary<Face, FaceRequest> Faces { get; }

        public CaptureRequest(string meshCode, Bounds bounds, float pixelsPerMeter)
        {
            MeshCode = meshCode;
            Bounds = bounds;
            PixelsPerMeter = pixelsPerMeter;

            var size = Bounds.size;
#if false
            XCoef = size.Zy().RevScaled().Axy(0f) * pixelsPerMeter;
            YCoef = size.Xz().RevScaled().Xay(0f) * pixelsPerMeter;
            ZCoef = size.Xy().RevScaled().Xya(0f) * pixelsPerMeter;
#endif
            XCoef = size.RevScaled();
            YCoef = size.RevScaled();
            ZCoef = size.RevScaled();
            Faces = new Dictionary<Face, FaceRequest>
            {
                [Face.Front] = new FaceRequest(Vector3.forward, Vector3.up, size.Xy()),
                [Face.Back] = new FaceRequest(Vector3.back, Vector3.up, size.Xy()),
                [Face.Left] = new FaceRequest(Vector3.left, Vector3.up, size.Zy()),
                [Face.Right] = new FaceRequest(Vector3.right, Vector3.up, size.Zy()),
                [Face.Top] = new FaceRequest(Vector3.up, Vector3.forward, size.Xz())
            };
        }
    }


    /// <summary>
    /// メッシュコードごとのワークフォルダ
    /// </summary>
    /// <param name="meshCode"></param>
    /// <returns></returns>
    static string GetFolderPath(string meshCode)
    {
        return $"{savePath}/{meshCode}";
    }

    /// <summary>
    /// メッシュコード/面によるテクスチャパス
    /// </summary>
    /// <param name="meshCode"></param>
    /// <param name="face"></param>
    /// <returns></returns>
    static string GetTexturePath(string meshCode, Face face)
    {
        return $"{GetFolderPath(meshCode)}/{face.ToString()}.png";
    }


    /// <summary>
    /// メッシュコード/面によるTriplanarマテリアルパス
    /// </summary>
    /// <param name="meshCode"></param>
    /// <param name="face"></param>
    /// <returns></returns>
    static string GetMaterialPath(string meshCode)
    {
        return $"{GetFolderPath(meshCode)}/{meshCode}Triplanar.mat";
    }

    [MenuItem("Tools/Orthographic View Capture")]
    public static void ShowWindow()
    {
        GetWindow<PLATEAUOrthographicViewCapture>("3面図キャプチャ & Triplanar適用");
    }

    /// <summary>
    /// PLATEAUInstancedCityModelの直下の子の名前から, キャプチャ対象のオブジェクトかどうかを取得する
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="meshCode"></param>
    /// <returns></returns>
    private static bool IsTargetBuilding(GameObject obj, out string meshCode)
    {
        meshCode = "";

        var names = obj.name.Split("_");
        if (names.Length < 2)
            return false;
        // meshCode_タイプ_XXXX_YY.gmlという形式
        meshCode = names[0];
        var type = names[1];
        return type == "bldg";
    }

    class BuildingInfo
    {
        public string Name { get; }
        public Dictionary<float, GameObject> Lods { get; } = new Dictionary<float, GameObject>();

        public BuildingInfo(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// key   : MeshCode
    /// value : そのメッシュコードの建物情報
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    private Dictionary<string, List<BuildingInfo>> GetBuildings(PLATEAUInstancedCityModel model)
    {
        var ret = new Dictionary<string, List<BuildingInfo>>();
        foreach (var tr in model.transform.GetChildren())
        {
            if (tr.gameObject.activeInHierarchy == false)
                continue;

            if (IsTargetBuilding(tr.gameObject, out var meshCode) == false)
            {
                continue;
            }

            ret[meshCode] = new List<BuildingInfo>();

            // key : gameObject名
            // value : { key : LOD番号, value : そのGameObject}
            Dictionary<string, Dictionary<float, GameObject>> objectLodTable = new();
            foreach (var child in tr.GetChildren())
            {
                var m = Regex.Match(child.name, @"LOD(\d+)");
                if (m.Success == false)
                {
                    continue;
                }

                var lod = int.Parse(m.Groups[1].Value);


                foreach (var c in child.GetChildren())
                {
                    var t = objectLodTable.GetValueOrCreate(c.name);
                    t[lod] = c.gameObject;
                }
            }


            foreach (var item in objectLodTable)
            {
                var b = new BuildingInfo(item.Key);
                foreach (var x in item.Value)
                {
                    b.Lods[x.Key] = x.Value;
                }
                ret[meshCode].Add(b);
            }
        }
        return ret;
    }

    private void SwitchLod1Visible(bool isLod1Visible)
    {
        foreach (var model in targetObjects)
        {
            var buildings = GetBuildings(model);
            foreach (var m in buildings.Values.SelectMany(x => x))
            {

                // LOD1を表示する場合
                if (isLod1Visible)
                {
                    var maxLod = m.Lods.Keys.Where(l => l <= 1).Max();
                    foreach (var x in m.Lods)
                    {
                        x.Value.SetActive(x.Key == maxLod);
                    }
                }
                // そうじゃない場合は最大のLODを表示する
                else
                {
                    var maxLod = m.Lods.Keys.Max();
                    foreach (var x in m.Lods)
                    {
                        x.Value.SetActive(x.Key == maxLod);
                    }
                }
            }
        }
    }

    private void Capture(Camera camera, PLATEAUInstancedCityModel model)
    {
        // 現在表示されていて, 非表示にするゲームオブジェクト
        var invisibleObjects = new HashSet<GameObject>();

        // キャプチャが終わった後に非表示に戻すゲームオブジェクト
        var afterInvisibleObjects = new HashSet<GameObject>();

        Vector3 boundMax = Vector3.one * float.MinValue;
        Vector3 boundMin = Vector3.one * float.MaxValue;


        Dictionary<string, GameObject> targetObjects = new Dictionary<string, GameObject>();

        SwitchLod1Visible(false);

        foreach (var tr in model.transform.GetChildren())
        {
            if (tr.gameObject.activeInHierarchy == false)
                continue;

            if (IsTargetBuilding(tr.gameObject, out var meshCode) == false)
            {
                // 対象のゲームオブジェクト以外は非表示に
                invisibleObjects.Add(tr.gameObject);
                continue;
            }

            // レンダー対象だったら削除する
            targetObjects[meshCode] = tr.gameObject;

            //foreach (var child in tr.GetChildren())
            //{
            //    var m = Regex.Match(child.name, @"LOD(\d+)");
            //    if (m.Success == false)
            //    {
            //        continue;
            //    }

            //    var lod = int.Parse(m.Groups[1].Value);

            //    if (lod <= 1)
            //    {
            //        if (child.gameObject.activeInHierarchy)
            //        {
            //            invisibleObjects.Add(child.gameObject);
            //        }
            //    }
            //    else
            //    {
            //        if (child.gameObject.activeInHierarchy == false)
            //        {
            //            child.gameObject.SetActive(true);
            //            afterInvisibleObjects.Add(child.gameObject);
            //        }
            //    }
            //}
        }

        // 一旦全部非表示にする
        foreach (var obj in invisibleObjects)
            obj.SetActive(false);

        foreach (var obj in targetObjects.Values)
            obj.SetActive(false);

        foreach (var item in targetObjects)
        {
            // 表示に戻す
            item.Value.SetActive(true);

            var bound = CalculateCombinedBounds(item.Value);
            CaptureOrthographicViews(item.Key, item.Value, camera);
        }

        foreach (var obj in invisibleObjects)
            obj.SetActive(true);

        foreach (var obj in afterInvisibleObjects)
            obj.SetActive(false);

        var buildings = GetBuildings(model);

        //foreach (var build in buildings)
        //{
        //    var matPath = GetMaterialPath(build.Key);
        //    var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        //    foreach (var b in build.Value)
        //    {
        //        var lod1 = b.Lods.GetValueOrDefault(1);
        //        if (lod1 == null)
        //            continue;
        //        var mesh = lod1.GetComponent<MeshRenderer>();
        //        if (mesh == null)
        //            continue;
        //        mesh.materials[0] = mat;
        //    }
        //}
    }

    private static void CaptureOrthographicViews(string meshCode, GameObject obj, Camera camera)
    {
        // 保存ディレクトリの作成

        if (!Directory.Exists(GetFolderPath(meshCode)))
        {
            Directory.CreateDirectory(GetFolderPath(meshCode));
        }

        // 全オブジェクトの統合バウンディングボックスを計算
        Bounds combinedBounds = CalculateCombinedBounds(obj);

        var request = new CaptureRequest(meshCode, combinedBounds, pixelsPerMeter);

        // テクスチャの作成
        foreach (var f in request.Faces)
        {
            CaptureView(camera, request, f.Key);
        }

        // マテリアルの作成
        CreateTriplanarMaterial(request);


        // アセットデータベースを更新
        AssetDatabase.Refresh();
    }

    private static string CaptureView(
        Camera camera
        , CaptureRequest req
        , Face face)
    {
        // カメラの位置と向きを設定
        var bounds = req.Bounds;
        var faceReq = req.Faces[face];

        var direction = faceReq.Direction;
        var up = faceReq.Up;
        var imageSize = faceReq.ImageSize;

        Vector3 center = bounds.center;
        float distance = Mathf.Abs(Vector3.Dot(bounds.size, direction));

        camera.transform.position = center + direction * (distance + 1f);
        camera.transform.LookAt(center, up);

        // 直交投影のサイズを設定
        camera.orthographicSize = imageSize.Min() * 0.5f;
        //camera.rect = cameraRect;
        // ニアクリップとファークリップを設定
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = distance * 2f;

        // レンダーテクスチャを作成
        imageSize = (imageSize.ToVector2() * pixelsPerMeter).ToVector2Int();
        RenderTexture renderTexture = new RenderTexture(imageSize.x, imageSize.y, 24);
        camera.targetTexture = renderTexture;
        // レンダリング実行
        camera.Render();

        // テクスチャを読み取り
        RenderTexture.active = renderTexture;
        Texture2D screenshot = new Texture2D(imageSize.x, imageSize.y, TextureFormat.RGBA32, false);
        screenshot.ReadPixels(new Rect(0, 0, imageSize.x, imageSize.y), 0, 0);
        screenshot.Apply();

        // ファイルに保存
        byte[] data = screenshot.EncodeToPNG();
        string filePath = GetTexturePath(req.MeshCode, face);
        File.WriteAllBytes(filePath, data);

        // クリーンアップ
        RenderTexture.active = null;
        camera.targetTexture = null;
        DestroyImmediate(renderTexture);
        DestroyImmediate(screenshot);

        Debug.Log($"{face}面図を保存しました: {filePath}");

        return filePath;
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
                var cityModel = obj.GetComponent<PLATEAUInstancedCityModel>();
                if (!cityModel)
                    continue;
                if (!targetObjects.Contains(cityModel))
                {
                    targetObjects.Add(cityModel);
                }
            }
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

        GUILayout.Space(10);

        // キャプチャ設定
        GUILayout.Label("キャプチャ設定:", EditorStyles.boldLabel);
        pixelsPerMeter = EditorGUILayout.FloatField("Pixels per Meter", pixelsPerMeter);
        savePath = EditorGUILayout.TextField("保存パス", savePath);
        cameraClearFlag = (CameraClearFlags)EditorGUILayout.EnumPopup("Camera Clear Flag", cameraClearFlag);
        cameraClearColor = EditorGUILayout.ColorField("Camera Clear Color", cameraClearColor);
        //layerIndex = EditorGUILayout.IntField("Layer Index", layerIndex);
        //triplanarShader = (Shader)EditorGUILayout.ObjectField("TriplanarShader", triplanarShader, typeof(Shader), true);
        GUILayout.Space(10);

        // キャプチャボタン
        if (GUILayout.Button("キャプチャ"))
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

        var boundingBox = CalculateCombinedBounds();
        EditorGUILayout.Vector3Field("バウンディングボックス[min]", boundingBox.min);
        EditorGUILayout.Vector3Field("バウンディングボックス[max]", boundingBox.max);
        EditorGUILayout.Vector3Field("バウンディングボックス[size]", boundingBox.size);
        EditorGUILayout.Vector3Field("バウンディングボックス[center]", boundingBox.center);

        var xy = (boundingBox.size.Xy() * pixelsPerMeter).FloorToInt();
        var zy = (boundingBox.size.Zy() * pixelsPerMeter).FloorToInt();
        var xz = (boundingBox.size.Xz() * pixelsPerMeter).FloorToInt();

        EditorGUILayout.Vector2Field("サイズ[xy]", xy);
        EditorGUILayout.Vector2Field("サイズ逆数[xy]", Vector2.one.RevScaled(xy.ToVector2()));
        EditorGUILayout.Vector2Field("サイズ[zy]", zy);
        EditorGUILayout.Vector2Field("サイズ逆数[zy]", Vector2.one.RevScaled(zy.ToVector2()));
        EditorGUILayout.Vector2Field("サイズ[xz]", xz);
        EditorGUILayout.Vector2Field("サイズ逆数[xz]", Vector2.one.RevScaled(xz.ToVector2()));

        GUI.enabled = triplanarTarget != null && !string.IsNullOrEmpty(lastFrontTexPath);
        if (GUILayout.Button("Triplanar投影を適用"))
        {
            // ApplyTriplanarProjection();
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
        camera.backgroundColor = cameraClearColor;
        camera.clearFlags = cameraClearFlag;
        //camera.cullingMask = 1 << layerIndex;

        try
        {
            foreach (var obj in targetObjects)
            {
                Capture(camera, obj);
            }
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

        foreach (GameObject obj in targetObjects.Select(x => x.gameObject))
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

    private static Bounds CalculateCombinedBounds(GameObject obj)
    {
        Bounds bounds = new Bounds();
        bool boundsInitialized = false;
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
        return bounds;
    }

    private static void CreateTriplanarMaterial(CaptureRequest req)
    {
        //if (triplanarTarget == null)
        //{
        //    EditorUtility.DisplayDialog("エラー", "適用対象オブジェクトが設定されていません", "OK");
        //    return;
        //}

        //// Triplanarシェーダーを読み込み
        //if (triplanarShader == null)
        //{
        //    EditorUtility.DisplayDialog("エラー", "TriplanarProjectionシェーダーが見つかりません", "OK");
        //    return;
        //}

        // マテリアルを作成

        var triplanarShader = Shader.Find("Shader Graphs/PLATEAULod1TriplanarShader");
        Material triplanarMaterial = new Material(triplanarShader);

        // テクスチャを読み込み

        triplanarMaterial.SetFloat("_Tile", 1f);
        triplanarMaterial.SetVector("_Offset", req.Bounds.min);
        triplanarMaterial.SetFloat("_PixelsPerMeter", req.PixelsPerMeter);
        foreach (var face in EnumEx.GetValues<Face>())
        {
            string filePath = GetTexturePath(req.MeshCode, face);
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
            triplanarMaterial.SetTexture($"_{face}Texture", tex);
        }

        triplanarMaterial.SetVector("_XCoef", req.XCoef);
        triplanarMaterial.SetVector("_YCoef", req.YCoef);
        triplanarMaterial.SetVector("_ZCoef", req.ZCoef);

        // マテリアルを保存
        string materialPath = GetMaterialPath(req.MeshCode);

        AssetDatabase.CreateAsset(triplanarMaterial, materialPath);

        //// 対象オブジェクトのRendererにマテリアルを適用
        //Renderer[] renderers = triplanarTarget.GetComponentsInChildren<Renderer>();
        //foreach (Renderer renderer in renderers)
        //{
        //    renderer.material = triplanarMaterial;
        //}

        //AssetDatabase.SaveAssets();
        //AssetDatabase.Refresh();

        //EditorUtility.DisplayDialog("完了", $"Triplanar投影を適用しました\nマテリアル: {materialPath}", "OK");
    }
}
