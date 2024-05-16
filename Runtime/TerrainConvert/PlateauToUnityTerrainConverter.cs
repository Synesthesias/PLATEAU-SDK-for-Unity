using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.TerrainConvert
{
    /// <summary>
    /// 都市の3Dモデルを PLATEAU から Unity に変換してシーンに配置します。
    /// </summary>
    public static class PlateauToUnityTerrainConverter
    {
        // TODO 処理をキャンセルする機能が未実装
        /// <summary>
        /// 引数の cityModel を Unity向けに変換し、シーンに配置します。
        /// 非同期処理です。必ずメインスレッドで呼ぶ必要があります。
        /// 成否を bool で返します。
        /// </summary>
        public static async Task<TerrainConvertResult> PlateauTerrainToScene(GameObject[] srcGameObjs, IProgressDisplay progressDisplay,
        string progressName, TerrainConvertOption convertOption, Model plateauModel, bool skipRoot)
        {
            ConvertedTerrainData terrainData;
            try
            {
                terrainData = await Task.Run(() =>
                {
                    var convertedTerrainData = new ConvertedTerrainData(plateauModel, convertOption);
                    return convertedTerrainData;
                });
            }
            catch (Exception e)
            {
                Debug.LogError("Terrainデータの取得に失敗しました。\n" + e);
                return TerrainConvertResult.Fail();
            }
           return await terrainData.PlaceToScene(srcGameObjs, convertOption, skipRoot);
        }
    }

    public class TerrainConvertOption
    {
        public enum ImageOutput
        {
            None,
            PNG,
            RAW,
            PNG_RAW
        }

        /// <summary> キャンセル用トークンです。 </summary>
        public CancellationToken? CancellationToken { get; }

        public int TextureWidth {get;}

        public int TextureHeight { get; }

        public GameObject[] SrcGameObjs { get; }

        public bool DoDestroySrcObjs { get; }

        //Debug Image Output
        public ImageOutput HeightmapImageOutput { get; }

        public TerrainConvertOption(GameObject[] srcGameObjs, int heightmapSize,  bool doDestroySrcObjs, ImageOutput heightmapImageOutput = ImageOutput.None,  CancellationToken? cancellationToken = null)
        {
            SrcGameObjs = srcGameObjs;
            TextureWidth = TextureHeight = heightmapSize;
            DoDestroySrcObjs = doDestroySrcObjs;
            HeightmapImageOutput = heightmapImageOutput;
            CancellationToken = cancellationToken;
        }
    }

    public class TerrainConvertResult
    {
        public bool IsSucceed { get; set; } = true;

        /// <summary> 変換によってシーンに配置したゲームオブジェクトのリスト </summary>
        public List<GameObject> GeneratedObjs { get; } = new();

        /// <summary> 結果のゲームオブジェクトの一覧に追加します。</summary>
        public void Add(GameObject obj)
        {
            GeneratedObjs.Add(obj);
        }

        /// <summary> 複数の<see cref="TerrainConvertResult"/>を統合します。 </summary>
        public void Merge(TerrainConvertResult other)
        {
            IsSucceed &= other.IsSucceed;
            GeneratedObjs.AddRange(other.GeneratedObjs);
        }

        public static TerrainConvertResult Fail()
        {
            return new TerrainConvertResult() { IsSucceed = false };
        }
    }
}

