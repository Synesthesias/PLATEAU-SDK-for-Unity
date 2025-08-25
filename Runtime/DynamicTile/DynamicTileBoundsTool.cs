using System.Collections.Generic;
using System.Linq;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// DynamicTileのBoundsまわりの処理をまとめたクラス。
    /// zoomLevel 9 ～ zoomLevel 11まで対応　(zoomLevel追加時は改修が必要）
    /// </summary>
    internal class DynamicTileBoundsTool
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
            foreach (var innerTile in inner)
            {
                foreach (var outerTile in outer)
                {
                    //センターを含む場合
                    if (outerTile.Extent.Contains(innerTile.Extent.center))
                    {
                        innerTile.ParentTile = outerTile; //上位のタイルを親として設定
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
    }
}
