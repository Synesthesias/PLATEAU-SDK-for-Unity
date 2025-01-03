using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace PLATEAU.Editor.RoadNetwork.EditingSystem
{
    /// <summary>
    /// 交差点編集でトラックを編集します。
    /// </summary>
    public class IntersectionTrackEditor
    {
        
        private RnNeighbor selectEntablePoint = null;
        private RnNeighbor selectExitablePoint = null;
        
        public bool IsSelectdEntablePoint
        {
            get => selectEntablePoint != null;
        }
        
        public void Draw(RoadNetworkEditTarget editTarget, RnIntersection targetIntersection)
        {
            var buttonSize = 2.0f;

            bool isSelectdEntablePoint = IsSelectdEntablePoint;
            if (isSelectdEntablePoint == false)
            {
                foreach (var item in EnterablePoints(targetIntersection))
                {
                    // 流入点の位置にボタンを表示する
                    if (Handles.Button(item.CalcCenter(), Quaternion.identity, buttonSize, buttonSize,
                            RoadNetworkEntarablePointButtonHandleCap))
                    {
                        SetEntablePoint(item, targetIntersection);
                        // 流入点が選択された
                        break;
                    }
                }
            }
            else
            {
                foreach (var item in ExitablePoints(targetIntersection))
                {
                    // 流出点の位置にボタンを表示する
                    if (Handles.Button(item.CalcCenter(), Quaternion.identity, buttonSize, buttonSize,
                            RoadNetworkExitablePointButtonHandleCap))
                    {
                        // 流出点が選択された
                        SetExitablePoint(item, targetIntersection);
                        break;
                    }
                }
            }

            // Trackの生成、削除に必要な設定が済んで更新できるか？
            if (CanTryUpdateTrack)
            {
                UpdateTrack(targetIntersection);
                editTarget.SetDirty();
            }
        }
        
        private bool CanTryUpdateTrack
        {
            get => selectEntablePoint != null && selectExitablePoint != null;
        }

        private IReadOnlyCollection<RnNeighbor> EnterablePoints(RnIntersection target)
        {
            return CollectEnterablePoints(target);
        }


        private IReadOnlyCollection<RnNeighbor> ExitablePoints(RnIntersection target)
        {
            return CollectExitablePoints(target);
        }
        
        private static IReadOnlyCollection<RnNeighbor> CollectEnterablePoints(RnIntersection data)
        {
            var enterablePoints = new List<RnNeighbor>(data.Neighbors.Count());
            foreach (var neighbor in data.Neighbors)
            {
                if (CheckEnterablePoint(neighbor))
                    enterablePoints.Add(neighbor);
            }
            return enterablePoints;
        }

        private static bool CheckEnterablePoint(RnNeighbor neighbor)
        {
            var isInboud = (neighbor.GetFlowType() & RnFlowTypeMask.Inbound) > 0;
            return isInboud;
        }
        
        private static IReadOnlyCollection<RnNeighbor> CollectExitablePoints(RnIntersection data)
        {
            var exitablePoints = new List<RnNeighbor>(data.Neighbors.Count());
            foreach (var neighbor in data.Neighbors)
            {
                if (CheckExitablePoint(neighbor))
                    exitablePoints.Add(neighbor);
            }
            return exitablePoints;
        }

        private static bool CheckExitablePoint(RnNeighbor neighbor)
        {
            var isOutbound = (neighbor.GetFlowType() & RnFlowTypeMask.Outbound) > 0;
            return isOutbound;
        }
        
        /// <summary>
        /// 流入点と流出点を返す
        /// </summary>
        public (RnNeighbor, RnNeighbor) SelectedPoints
        {
            get => (selectEntablePoint, selectExitablePoint);
        }

        public void SetEntablePoint(RnNeighbor neighbor, RnIntersection targetIntersection)
        {
            Assert.IsNotNull(neighbor);

            // 選択中の交差点に含まれているか
            Assert.IsTrue(EnterablePoints(targetIntersection).Contains(neighbor));

            selectEntablePoint = neighbor;
        }

        public void SetExitablePoint(RnNeighbor neighbor, RnIntersection targetIntersection)
        {
            Assert.IsNotNull(neighbor);

            // 選択中の交差点に含まれているか
            Assert.IsTrue(ExitablePoints(targetIntersection).Contains(neighbor));

            selectExitablePoint = neighbor;
        }

        /// <summary>
        /// 選択状態やトラックの有無で処理を分岐する
        /// </summary>
        public void UpdateTrack(RnIntersection targetIntersection)
        {
            Assert.IsTrue(CanTryUpdateTrack);

            var track = targetIntersection.FindTrack(selectEntablePoint, selectExitablePoint);
            if (track != null)
                targetIntersection.RemoveTrack(track);
            else
                targetIntersection.TryAddOrUpdateTrack(selectEntablePoint, selectExitablePoint);

            selectEntablePoint = null;
            selectExitablePoint = null;
        }

        public void RemoveTarck()
        {
            selectEntablePoint = null;
            selectExitablePoint = null;
        }
        
        private static void RoadNetworkExitablePointButtonHandleCap(int controlID, Vector3 position,
            Quaternion rotation, float size, EventType eventType)
        {
            if (eventType == EventType.Repaint)
                Handles.color = Color.blue;
            Handles.SphereHandleCap(controlID, position, rotation, size, eventType);
        }
        
        private static void RoadNetworkEntarablePointButtonHandleCap(int controlID, Vector3 position,
            Quaternion rotation, float size, EventType eventType)
        {
            if (eventType == EventType.Repaint)
                Handles.color = Color.red;
            Handles.SphereHandleCap(controlID, position, rotation, size, eventType);
        }
    }
}