using PLATEAU.CityInfo;
using PLATEAU.Util;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.AdjustModel
{
    public class PLATEAUOrthographicViewCaptureService
    {
        public enum Face
        {
            Front,
            Back,
            Left,
            Right,
            Top
        }

        public class CaptureRequest
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
            /// シェーダパラメータ) Z方向の正規化係数
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

        private class BuildingInfo
        {
            /// <summary>
            /// 建物名
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// LOD情報
            /// </summary>
            public Dictionary<int, GameObject> Lods { get; } = new Dictionary<int, GameObject>();

            /// <summary>
            /// 最大Lod
            /// </summary>
            public float MaxLod => Lods.Any() ? Lods.Keys.Max() : 0;

            /// <summary>
            /// Triplanar変換対象かどうか. Lod1より大きい
            /// </summary>
            public bool IsTriplanarTarget
            {
                get
                {
                    var lod1 = Lods.GetValueOrDefault(1);
                    return lod1 && MaxLod > 1;
                }
            }

            public BuildingInfo(string name)
            {
                Name = name;
            }
        }

        private readonly string savePath;
        private readonly float pixelsPerMeter;
        private readonly CameraClearFlags cameraClearFlag;
        private readonly Color cameraClearColor;
        private readonly bool isDynamicTileMode;

        public PLATEAUOrthographicViewCaptureService(
            string savePath,
            float pixelsPerMeter,
            bool isDynamicTileMode,
            CameraClearFlags cameraClearFlag = CameraClearFlags.Nothing,
            Color cameraClearColor = default)
        {
            this.savePath = savePath;
            this.pixelsPerMeter = pixelsPerMeter;
            this.cameraClearFlag = cameraClearFlag;
            this.cameraClearColor = cameraClearColor == default ? Color.black : cameraClearColor;
            this.isDynamicTileMode = isDynamicTileMode;
        }

        /// <summary>
        /// メッシュコードごとのワークフォルダ
        /// </summary>
        private string GetFolderPath(string meshCode)
        {
            return $"{savePath}/{meshCode}";
        }

        /// <summary>
        /// メッシュコード/面によるテクスチャパス
        /// </summary>
        private string GetTexturePath(string meshCode, Face face)
        {
            return $"{GetFolderPath(meshCode)}/{face.ToString()}.png";
        }

        /// <summary>
        /// メッシュコード/面によるTriplanarマテリアルパス
        /// </summary>
        private string GetMaterialPath(string meshCode)
        {
            return $"{GetFolderPath(meshCode)}/{meshCode}Triplanar.mat";
        }

        /// <summary>
        /// PLATEAUInstancedCityModelの直下の子の名前から, キャプチャ対象のオブジェクトかどうかを取得する
        /// </summary>
        private bool IsTargetBuilding(GameObject obj, out string meshCode)
        {
            meshCode = "";

            var type = string.Empty;
            if (isDynamicTileMode)
            {
                //tile_zoom_X_grid_メッシュコード_タイプ_XXXXX_YY_opという形式なのでタイプを抽出
                var names = obj.name.Split("_");
                if (names.Length < 6)
                    return false;
                meshCode = names[4];
                type = names[5];
            }
            else
            {
                var names = obj.name.Split("_");
                if (names.Length < 2)
                    return false;
                // meshCode_タイプ_XXXX_YY.gmlという形式
                meshCode = names[0];
                type = names[1];
            }
            
            return type == "bldg";
        }

        /// <summary>
        /// key   : MeshCode
        /// value : そのメッシュコードの建物情報
        /// </summary>
        private Dictionary<string, List<BuildingInfo>> GetBuildings(GameObject model)
        {
            var ret = new Dictionary<string, List<BuildingInfo>>();
            
            var targets = isDynamicTileMode
                ? new List<Transform> { model.transform }
                : model.transform.GetChildren().ToList();

            foreach (var tr in targets)
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
                Dictionary<string, Dictionary<int, GameObject>> objectLodTable = new();
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
        /// modelのLod1建物にmaterialを設定する
        /// </summary>
        public void SetLod1Material(GameObject model, Material material)
        {
            var buildings = GetBuildings(model);
            foreach (var b in buildings)
            {
                SetLod1Material(b.Value, material);
            }
        }

        /// <summary>
        /// LOD1オブジェクト/LOD2以上のオブジェクトのビジブル切り替えを行う
        /// </summary>
        public void SwitchLod1Visible(GameObject model, bool isLod1Visible)
        {
            var buildings = GetBuildings(model);
            var sb = new StringBuilder();
            foreach (var kvp in buildings)
            {
                sb.AppendLine($"MeshCode: {kvp.Key}, BuildingCount: {kvp.Value.Count}");
                foreach (var vBuildingInfo in kvp.Value)
                {
                    sb.AppendLine("\tBuilding Name: " + vBuildingInfo.Name);
                    foreach (var lod in vBuildingInfo.Lods)
                    {
                        sb.AppendLine($"\t\tLOD{lod.Key}: {lod.Value.name}");
                    }
                }
            }

            sb.AppendLine("作業開始");
            
            
            foreach (var m in buildings.Values.SelectMany(x => x))
            {
                // LOD1を表示する場合, Lod1以下で最大のものを表示
                if (isLod1Visible)
                {
                    var maxLod = m.Lods.Keys.Where(l => l <= 1).Max();
                    foreach (var x in m.Lods)
                    {
                        x.Value.SetActive(x.Key == maxLod);
                        sb.AppendLine($"\tLOD{x.Key} Visible: {x.Value.activeSelf}");
                    }
                }
                // そうじゃない場合は最大のLODを表示する
                else
                {
                    var maxLod = m.Lods.Keys.Max();
                    foreach (var x in m.Lods)
                    {
                        var active = isDynamicTileMode ? x.Key != 1 : x.Key == maxLod;
                        x.Value.SetActive(active);
                        sb.AppendLine($"\tLOD{x.Key} Visible: {x.Value.activeSelf}");
                    }
                }
            }
            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// 5面図キャプチャ処理からマテリアルの生成まで行う
        /// </summary>
        public void Execute(Camera camera, GameObject model)
        {

            camera.clearFlags = cameraClearFlag;
            camera.backgroundColor = cameraClearColor;
            camera.orthographic = true;
            
            // 現在表示されていて, 非表示にするゲームオブジェクト
            var invisibleObjects = new HashSet<GameObject>();

            // 変換対象のゲームオブジェクト
            var targetBuildingObjects = new Dictionary<string, GameObject>();

            SwitchLod1Visible(model, false);

            if (isDynamicTileMode)
            {
                if(IsTargetBuilding(model, out var meshCode))
                {
                    targetBuildingObjects[meshCode] = model;
                }
            }
            else
            {
                foreach (var tr in model.transform.GetChildren())
                {
                    // もともと非表示のものは無視する
                    if (tr.gameObject.activeInHierarchy == false)
                        continue;

                    if (IsTargetBuilding(tr.gameObject, out var meshCode) == false)
                    {
                        // 対象のゲームオブジェクト以外は非表示に
                        invisibleObjects.Add(tr.gameObject);
                        continue;
                    }

                    targetBuildingObjects[meshCode] = tr.gameObject;
                }
            }

            
            // 一旦全部非表示にする
            foreach (var obj in invisibleObjects)
                obj.SetActive(false);

            foreach (var obj in targetBuildingObjects.Values)
                obj.SetActive(false);

            foreach (var item in targetBuildingObjects)
            {
                // 表示に戻す
                item.Value.SetActive(true);

                // 全オブジェクトの統合バウンディングボックスを計算
                Bounds combinedBounds = CalculateCombinedBounds(item.Value);

                var request = new CaptureRequest(item.Key, combinedBounds, pixelsPerMeter);
                
                camera.Render();
                
                // テクスチャのキャプチャ
                CaptureAllFaces(request, camera);

                // マテリアル作る前にアセットデータベースを更新
                // Loadで読み込めるように
                AssetDatabase.Refresh();

                // マテリアルの作成
                CreateTriplanarMaterial(request);

                // 非表示に戻す
                item.Value.SetActive(false);
            }

            foreach (var obj in invisibleObjects)
                obj.SetActive(true);

            foreach (var obj in targetBuildingObjects.Values)
                obj.SetActive(true);

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

        /// <summary>
        /// 複数のモデルに対して実行
        /// </summary>
        public void Execute(Camera camera, IEnumerable<GameObject> models)
        {
            foreach (var model in models)
            {
                if (!model)
                    continue;
                Execute(camera, model);
            }
        }

        private static void SetLod1Material(List<BuildingInfo> buildings, Material mat)
        {
            foreach (var b in buildings)
            {
                if (b.IsTriplanarTarget == false)
                    continue;

                var lod1 = b.Lods.GetValueOrDefault(1);
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
        private void CaptureAllFaces(CaptureRequest request, Camera camera)
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
        private string CaptureFace(Camera camera, CaptureRequest req, Face face)
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
            Object.DestroyImmediate(renderTexture);
            Object.DestroyImmediate(screenshot);

            Debug.Log($"{face}面図を保存しました: {filePath}");
            return filePath;
        }

        /// <summary>
        /// 対象を包括するバウンディングボックスを取得
        /// </summary>
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
        private void CreateTriplanarMaterial(CaptureRequest req)
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
}