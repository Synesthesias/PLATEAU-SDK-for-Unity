using PlasticGui.WorkspaceWindow.Locks;
using PLATEAU.CityInfo;
using PLATEAU.Dataset;
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
            /// <summary>
            /// 面の法線方向
            /// </summary>
            public Vector3 Direction { get; }

            /// <summary>
            /// Upベクトル
            /// </summary>
            public Vector3 Up { get; }

            /// <summary>
            /// 画像サイズ
            /// </summary>
            public Vector2Int ImageSize { get; }

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

        /// <summary>
        /// シェーダパラメータ) X方向の正規化係数
        /// </summary>
        public Vector3 XCoef { get; }

        /// <summary>
        /// シェーダパラメータ) Y方向の正規化係数
        /// </summary>
        public Vector3 YCoef { get; }

        /// <summary>
        /// シェーダパラメータ) X方向の正規化係数
        /// </summary>
        public Vector3 ZCoef { get; }

        /// <summary>
        /// 解像度用
        /// </summary>
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

            var size = Vector3.Max(Bounds.size, Vector3.one * 1e-6f);
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

    [MenuItem("PLATEAU/Debug/Orthographic View Capture")]
    public static void ShowWindow()
    {
        GetWindow<PLATEAUOrthographicViewCapture>("LOD1 5面図キャプチャ");
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
        /// <summary>
        /// 建物名
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// LOD情報
        /// </summary>
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
    static private Dictionary<string, List<BuildingInfo>> GetBuildings(PLATEAUInstancedCityModel model)
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

            var list = ret.GetValueOrCreate(meshCode);
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
                list.Add(b);
            }
        }
        return ret;
    }

    /// <summary>
    /// modelのLod1建物にdefaultMaterialを設定する
    /// </summary>
    /// <param name="model"></param>
    private static void SetDefaultBuildingMaterial(PLATEAUInstancedCityModel model)
    {
        var buildings = GetBuildings(model);
        foreach (var b in buildings)
        {
            SetLod1Material(b.Value, defaultMaterial);
        }
    }

    /// <summary>
    /// LOD1オブジェクト/LOD2以上のオブジェクトのビジブル切り替えを行う
    /// </summary>
    /// <param name="isLod1Visible"></param>
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

    /// <summary>
    /// 5面図キャプチャ処理からマテリアルの生成まで行う
    /// </summary>
    /// <param name="camera"></param>
    /// <param name="model"></param>
    private void Execute(Camera camera, PLATEAUInstancedCityModel model)
    {
        // 現在表示されていて, 非表示にするゲームオブジェクト
        var invisibleObjects = new HashSet<GameObject>();

        // キャプチャが終わった後に非表示に戻すゲームオブジェクト
        var afterInvisibleObjects = new HashSet<GameObject>();

        var targetObjects = new Dictionary<string, GameObject>();

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

            targetObjects[meshCode] = tr.gameObject;
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

            // 全オブジェクトの統合バウンディングボックスを計算
            Bounds combinedBounds = CalculateCombinedBounds(item.Value);

            var request = new CaptureRequest(item.Key, combinedBounds, pixelsPerMeter);

            // テクスチャのキャプチャ
            CaptureAllFaces(request, camera);

            // マテリアルの作成
            CreateTriplanarMaterial(request);
        }

        foreach (var obj in invisibleObjects)
            obj.SetActive(true);

        foreach (var obj in afterInvisibleObjects)
            obj.SetActive(false);

        var buildings = GetBuildings(model);

        foreach (var build in buildings)
        {
            var matPath = GetMaterialPath(build.Key);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            SetLod1Material(build.Value, mat);
        }

        // アセットデータベースを更新
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static private void SetLod1Material(List<BuildingInfo> buildings, Material mat)
    {
        foreach (var b in buildings)
        {
            var lod1 = b.Lods.GetValueOrDefault(1);
            if (lod1 == null)
                continue;

            // 対象オブジェクトのRendererにマテリアルを適用
            Renderer[] renderers = lod1.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.material = mat;
            }
        }
    }

    /// <summary>
    /// 全ての面をキャプチャ
    /// </summary>
    /// <param name="request"></param>
    /// <param name="camera"></param>
    private static void CaptureAllFaces(CaptureRequest request, Camera camera)
    {
        // 保存ディレクトリの作成

        if (!Directory.Exists(GetFolderPath(request.MeshCode)))
        {
            Directory.CreateDirectory(GetFolderPath(request.MeshCode));
        }

        // テクスチャの作成
        foreach (var f in request.Faces)
        {
            CaptureFace(camera, request, f.Key);
        }
    }

    /// <summary>
    /// 1面のキャプチャ
    /// </summary>
    /// <param name="camera"></param>
    /// <param name="req"></param>
    /// <param name="face"></param>
    /// <returns></returns>
    private static string CaptureFace(
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

        // 法線方向にずらすして中心を見るようにする
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
        //layerIndex = EditorGUILayout.IntField("Layer Index", layerIndex);
        //triplanarShader = (Shader)EditorGUILayout.ObjectField("TriplanarShader", triplanarShader, typeof(Shader), true);
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
        //camera.cullingMask = 1 << layerIndex;

        try
        {
            foreach (var obj in targetObjects)
            {
                Execute(camera, obj);
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

    /// <summary>
    /// 対象を包括するバウンディングボックスを取得
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
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

    /// <summary>
    /// マテリアル生成
    /// </summary>
    /// <param name="req"></param>
    private static void CreateTriplanarMaterial(CaptureRequest req)
    {
        // マテリアルを作成

        var triplanarShader = Shader.Find("Shader Graphs/PLATEAULod1TriplanarShader");
        Material triplanarMaterial = new Material(triplanarShader);

        // テクスチャを読み込み
        triplanarMaterial.SetFloat("_Tile", 1f);
        triplanarMaterial.SetVector("_Min", req.Bounds.min);
        triplanarMaterial.SetVector("_Max", req.Bounds.max);
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
    }
}
