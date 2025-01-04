using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod
{
    /// <summary>
    /// 道路ネットワークを地形にスナップさせます。
    /// 現在は使われていません。
    /// </summary>
    public class RoadNetworkEditLandSnapper
    {
        private const float SnapHeightOffset = 0.1f; // ポイントスナップ時の高低差のオフセット（0だとポイント間を繋ぐ線がめり込むことがあるため）
        
        
        public void SnapPointsToDemAndTran(IEnumerable<RnPoint> items)
        {
            foreach (var item in items)
            {
                SnapPointToDemAndTran(item);
            }
        }

        public void SnapPointToDemAndTran(RnPoint item)
        {
            Ray ray;
            const float rayDis = 1000.0f;
            const float maxRayDistance = rayDis * 2.0f;
            ray = new Ray(item.Vertex + Vector3.up * rayDis, Vector3.down);
            SnapPointToObj(item, ray, maxRayDistance, "dem_", "tran_");
        }

        public void SnapPointToObj(RnPoint item, in Ray ray, float maxDistance, params string[] filter)
        {
            var hits = Physics.RaycastAll(ray, maxDistance);    // 地形メッシュが埋まっていてもスナップ出来るように

            var isTarget = false;
            var closestDist = float.MaxValue;
            Vector3 targetPos = Vector3.zero;
            foreach (RaycastHit hit in hits)
            {
                foreach (var f in filter)
                {
                    if (hit.collider.name.Contains(f))
                    {
                        var dis = Vector3.Distance(hit.point, ray.origin);
                        if (dis < closestDist)
                        {
                            closestDist = dis;
                            targetPos = hit.point;
                        }
                        isTarget = true;
                        continue;
                    }
                }
            }

            if (isTarget)
            {
                item.Vertex = targetPos + Vector3.up * SnapHeightOffset;
                return;
            }

        }
    }
}