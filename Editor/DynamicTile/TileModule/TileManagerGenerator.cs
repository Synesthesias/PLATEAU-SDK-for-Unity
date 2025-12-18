using PLATEAU.DynamicTile;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.DynamicTile.TileModule
{
    /// <summary>
    /// 動的タイルインポートについて、<see cref="PLATEAUTileManager"/>の生成方法を記述します。
    /// </summary>
    public class TileManagerGenerator : IOnTileGenerateStart, IAfterTileAssetBuild
    {
        private readonly DynamicTileProcessingContext context;

        public TileManagerGenerator(DynamicTileProcessingContext context)
        {
            this.context = context;
        }
        
        /// <summary> タイル生成前、古い<see cref="PLATEAUTileManager"/>を削除します。 </summary>
        public bool OnTileGenerateStart()
        {
            var manager = Object.FindObjectOfType<PLATEAUTileManager>();
            if (manager != null)
            {
                Object.DestroyImmediate(manager.gameObject);
            }
            
            return true;
        }

        public void OnTileGenerateStartFailed()
        {
            // noop
        }

        /// <summary>
        /// タイル生成後、<see cref="PLATEAUTileManager"/>を生成し、カタログパスのみ設定します。
        /// </summary>
        /// <returns></returns>
        public bool AfterTileAssetBuild()
        {
            // managerを生成
            var managerObj = new GameObject("DynamicTileManager");
            var manager = managerObj.AddComponent<PLATEAUTileManager>();
            manager.OutputPath = context.Config.OutputPath;
            
            // 最新のカタログファイルのパスを取得（Asset相対/フルの両方に対応）
            string catalogSearchDir;
            if (context.IsExcludeAssetFolder)
            {
                catalogSearchDir = Path.IsPathRooted(context.BuildFolderPath)
                    ? context.BuildFolderPath
                    : AssetPathUtil.GetFullPath(context.BuildFolderPath);
            }
            else
            {
                // Assets内出力の場合、StreamingAssets/PLATEAUBundles/{GroupName} に出力されているはず
                catalogSearchDir = $"Assets/StreamingAssets/{AddressableLoader.AddressableLocalBuildFolderName}/{context.AddressableGroupName}";
            }
            
            var catalogFiles = TileCatalogSearcher.FindCatalogFiles(catalogSearchDir, true);
            if (catalogFiles.Length == 0)
            {
                Debug.LogError($"カタログファイルが見つかりません。検索フォルダ={catalogSearchDir}");
                return false;
            }

            var catalogPath = catalogFiles[0];
            
            manager.SaveCatalogPath(catalogPath);
            
            return true;
        }
        
        /// <summary>
        /// <see cref="PLATEAUTileManager"/>が保持するタイル範囲がSceneViewカメラにぴったり収まるようなカメラ位置を計算して返します。
        /// シーンビューのカメラは真下を向きます。
        /// ただし、離れすぎて見えない場合は見える程度の距離にします。
        /// </summary>
        public static void FocusSceneViewCameraToTiles(PLATEAUTileManager manager) 
        {
            if (manager == null)
            {
                Debug.LogWarning("PLATEAUTileManagerがnullです。");
                return;
            }

            var bounds = manager.GetTileBounds();
            if (bounds.size == Vector3.zero)
            {
                Debug.LogWarning("有効なタイルBoundsが存在しません。");
                return;
            }

            // SceneViewカメラ情報を取得
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null || sceneView.camera == null)
            {
                Debug.LogWarning("シーンビューまたはシーンビューカメラが見つからないため、タイルへのカメラのフォーカスを中止します。");
                return;
            }

            Camera cam = sceneView.camera;
            float verticalFov = cam.fieldOfView;
            float aspect = cam.aspect;

            // XZ平面上での半サイズ
            float halfWidth = bounds.size.x * 0.5f;
            float halfDepth = bounds.size.z * 0.5f;

            // バウンディング球半径 (XZ 上で計算)
            float radius = Mathf.Sqrt(halfWidth * halfWidth + halfDepth * halfDepth);

            // 縦 FOV から必要距離を計算
            float distanceVertical = radius / Mathf.Tan(Mathf.Deg2Rad * verticalFov * 0.5f);

            // 横 FOV も考慮 (aspect から計算)
            float horizontalFov = 2f * Mathf.Atan(Mathf.Tan(Mathf.Deg2Rad * verticalFov * 0.5f) * aspect);
            float distanceHorizontal = radius / Mathf.Tan(horizontalFov * 0.5f);

            // 離れすぎてタイルが隠れないように
            float maxDistance = 10000f; // フォールバック
            if(manager.loadDistances != null && manager.loadDistances.Count > 0)
            {
                maxDistance = manager.loadDistances.Select(ld => ld.Value.Item2).Max() * 0.4f; // 0.4の根拠は勘
            }

            // 横か縦か大きい方を採用
            float distance = Mathf.Min(distanceVertical, distanceHorizontal);
            distance = Mathf.Min(distance, maxDistance);
            
            // シーンビューの視点をタイルにフォーカス
            var nextPivot = bounds.center;

            // カメラ移動を反映させます。これがないと、手動でシーンを動かすまでタイルが出てきません。
            EditorApplication.delayCall += () =>
            {
                // 1フレーム目
                var sv = SceneView.lastActiveSceneView;
                if (sv != null && sv.camera != null && manager != null)
                {
                    sv.pivot = nextPivot;
                    sv.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
                    sv.size = distance;
                    
                    var camPos = sv.camera.transform.position;
                    manager.UpdateCameraPosition(camPos);
                    manager.UpdateAssetsByCameraPosition(camPos).ContinueWithErrorCatch();
                    sv.Repaint();

					// 2フレーム目
					void NudgeOnce()
					{
						var sv2 = SceneView.lastActiveSceneView;
						if (sv2 == null || sv2.camera == null || manager == null)
						{
							EditorApplication.update -= NudgeOnce;
							return;
						}
						var delta = 0.6f;
						sv2.pivot = nextPivot + new Vector3(delta, 0f, 0f);
						sv2.Repaint();
						EditorApplication.QueuePlayerLoopUpdate();

						var camPos2 = sv2.camera.transform.position;
						manager.UpdateCameraPosition(camPos2);
						manager.UpdateAssetsByCameraPosition(camPos2).ContinueWithErrorCatch();

						EditorApplication.update -= NudgeOnce;
					}
					EditorApplication.update += NudgeOnce;
                }
            };
            
            EditorApplication.QueuePlayerLoopUpdate();
        }
    }
}