using PLATEAU.Dataset;
using PLATEAU.Geometries;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// DynamicTileのBoundsまわりの処理をまとめたクラス。
    /// zoomLevel 9 ～ zoomLevel 11まで対応　(zoomLevel追加時は改修が必要）
    /// </summary>
    internal class DynamicTileTool
    {
        //zoomLevelごとにタイルの上位zoomLevelとなるParentを設定する
        public static void AssignParentTiles(List<PLATEAUDynamicTile> tiles)
        {
            List<PLATEAUDynamicTile> z11 = tiles.Where(t => t.ZoomLevel == 11).ToList();
            List<PLATEAUDynamicTile> z10 = tiles.Where(t => t.ZoomLevel == 10).ToList();
            List<PLATEAUDynamicTile> z9 = tiles.Where(t => t.ZoomLevel == 9).ToList();

            SetParent(z11, z10);
            SetParent(z10, z9);
        }
        public static void SetParent(List<PLATEAUDynamicTile> inner, List<PLATEAUDynamicTile> outer)
        {
            var innerType = string.Empty;
            var outerType = string.Empty;
            foreach (var innerTile in inner)
            {
                var splitsAddress = innerTile.Address.Split("_");
                innerType = splitsAddress.Length > 5 ? splitsAddress[5] : string.Empty;
                foreach (var outerTile in outer)
                {
                    splitsAddress = outerTile.Address.Split("_");
                    outerType = splitsAddress.Length > 5 ? splitsAddress[5] : string.Empty;
                    //センターを含む場合
                    if (outerTile.Extent.Contains(innerTile.Extent.center))
                    {
                        if (innerType == outerType)
                        {
                            innerTile.ParentTile = outerTile; //上位のタイルを親として設定(同じタイプの場合のみ）
                        }

                        outerTile.ChildrenTiles ??= new List<PLATEAUDynamicTile>();
                        if (!outerTile.ChildrenTiles.Contains(innerTile))
                        {
                            outerTile.ChildrenTiles.Add(innerTile); //親の子リストに追加
                        }
                    }
                }
            }
        }

        //zoomLevelごとにタイルのBoundsを更新するメソッド
        //上位zoomLevelのBoundsが下位zoomLevelのBoundsを包含する場合は、上位のものを利用
        public static void UpdateBounds(List<PLATEAUDynamicTile> tiles)
        {
            List<PLATEAUDynamicTile> z11 = tiles.Where(t => t.ZoomLevel == 11).ToList();
            List<PLATEAUDynamicTile> z10 = tiles.Where(t => t.ZoomLevel == 10).ToList();
            List<PLATEAUDynamicTile> z9 = tiles.Where(t => t.ZoomLevel == 9).ToList();

            ReplaceBounds(z11, z10);
            ReplaceBounds(z10, z9);
        }

        public static void ReplaceBounds(List<PLATEAUDynamicTile> inner, List<PLATEAUDynamicTile> outer)
        {
            foreach (var innerTile in inner)
            {
                foreach (var outerTile in outer)
                {
                    //センターを含む場合
                    if (outerTile.Extent.Contains(innerTile.Extent.center))
                    {
                        innerTile.Extent = outerTile.Extent; //上位のBoundsを利用
                    }
                }
            }
        }

        /// <summary>
        /// Addressからパッケージを取得する。
        /// </summary>
        public static PredefinedCityModelPackage GetPackage(string address)
        {
            if (string.IsNullOrEmpty(address))
                return PredefinedCityModelPackage.None;

            Match match = Regex.Match(address, @"^tile_zoom_\d+_grid_[^_]+_(.+)$");
            if (match.Success)
            {
                string originalName = match.Groups[1].Value;
                string type = originalName.Split('_').FirstOrDefault();
                if (string.IsNullOrEmpty(type))
                    return PredefinedCityModelPackage.None;
                return DatasetAccessor.FeatureTypeToPackage(type);
            }
            else
            {
                Debug.LogError("アドレスからパッケージ名が取得できませんでした。Address: " + address);
            }
            return PredefinedCityModelPackage.None;
        }

        /// <summary>
        /// メッシュコードからタイルの範囲を初期化する。
        /// </summary>
        /// <param name="meshcode"></param>
        /// <returns></returns>
        public static Bounds GetExtentFromMeshCode(string meshcode, GeoReference geo)
        {
            GridCode gridCode = GridCode.Create(meshcode);

            var min = geo.Project(gridCode.Extent.Min);
            var max = geo.Project(gridCode.Extent.Max);

            var bounds = new Bounds();
            bounds.SetMinMax(new Vector3((float)min.X, (float)min.Y, (float)min.Z), new Vector3((float)max.X, (float)max.Y, (float)max.Z));

            return bounds;
        }

        /// <summary>
        /// メッシュコードを取得する。
        /// GameObject名：tile_zoom_(タイルのズームレベル)_grid_(タイルの位置を示すメッシュコード)_(従来のゲームオブジェクト名)_(同名の場合のID)
        /// 例:tile_zoom_0_grid_meshcode_gameobjectname_0
        /// </summary>
        /// <returns></returns>
        public static string GetMeshCodeFromAddress(string Address)
        {
            Match match = Regex.Match(Address, @"_grid_([^_]+)_");
            if (match.Success)
            {
                string meshcode = match.Groups[1].Value;
                return meshcode;
            }
            Debug.LogError($"メッシュコードが見つかりません : {Address}");
            return null;
        }

        /// <summary>
        /// ズームレベルからデフォルトの解像度の分母を取得します。(固定値)
        /// [9: 1/4, 10: 1/2, 11: 1/1]
        /// </summary>
        /// <param name="zoomLevel"></param>
        /// <returns></returns>
        public static int GetDefaultDenominatorFromZoomLevel(int zoomLevel)
        {
            return zoomLevel switch
            {
                9 => 4,
                10 => 2,
                11 => 1,
                _ => 0 // 未対応
            };
        }
    }
}
