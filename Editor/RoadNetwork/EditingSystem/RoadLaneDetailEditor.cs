using PLATEAU.Editor.RoadNetwork.EditingSystemSubMod;
using PLATEAU.RoadAdjust;
using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.Editor.RoadNetwork.EditingSystem
{
    /// <summary>
    /// 道路編集で詳細モードがONになっている場合の処理です。
    /// 詳細モードは未完成です。
    /// </summary>
    internal class RoadLaneDetailEditor : LineSelectEdit.ICreatedLineReceiver
    {
        private LineSelectEdit lineSelectEdit;
        private RoadNetworkEditTarget editTarget;

        public RoadLaneDetailEditor()
        {
            lineSelectEdit = new LineSelectEdit(this);
        }

        public void Draw(RnRoadGroup roadGroup, RoadNetworkEditTarget editTargetArg)
        {
            editTarget = editTargetArg;
            var targetLines = new List<IEditTargetLine>();
            foreach (var road in roadGroup.Roads)
            {
                targetLines.AddRange(road.AllWays().Select(w => new EditTargetRoadWay(w, road)));
            }
            lineSelectEdit.Draw(targetLines.ToArray());
            // RoadEditSceneGUIState state = new RoadEditSceneGUIState();
            //
            // var currentCamera = SceneView.currentDrawingSceneView.camera;
            // state.currentCamera = currentCamera;
            //
            //
            // foreach (var road in roadGroupEditorData.Ref.Roads)
            // {
            //     if (state.isDirtyTarget)
            //     {
            //         break;
            //     }
            //
            //     foreach (var sideWalk in road.SideWalks)
            //     {
            //         foreach (var point in sideWalk.OutsideWay.Points)
            //         {
            //             if (state.isDirtyTarget)
            //             {
            //                 break;
            //             }
            //
            //             var parent = sideWalk.OutsideWay;
            //
            //             var size = HandleUtility.GetHandleSize(point) * RoadNetworkEditTargetSelectButton.PointHndScaleFactor;
            //
            //             DeployPointMoveHandle(point, state, size);
            //         }
            //     }
            //
            //     foreach (var lane in road.AllLanes)
            //     {
            //         if (state.isDirtyTarget)
            //         {
            //             break;
            //         }
            //
            //         HashSet<RnWay> ways = new HashSet<RnWay>(lane.BothWays);
            //
            //         foreach (var way in ways)
            //         {
            //             if (state.isDirtyTarget)
            //             {
            //                 break;
            //             }
            //
            //             foreach (var point in way.Points)
            //             {
            //                 if (state.isDirtyTarget)
            //                 {
            //                     break;
            //                 }
            //
            //                 var parent = way;
            //
            //                 var size = HandleUtility.GetHandleSize(point) * RoadNetworkEditTargetSelectButton.PointHndScaleFactor;
            //
            //
            //                 // ctrlを押しているか
            //                 if (Event.current.control == false)
            //                 {
            //                     DeployPointMoveHandle(point, state, size);
            //                     continue;
            //                 }
            //                 else
            //                 {
            //                     var currentEvent = Event.current;
            //                     {
            //                         // ポイントの追加
            //                         if (currentEvent.shift == false)
            //                         {
            //                             // ポイントの追加ボタンの表示
            //                             var isClicked = Handles.Button(point, Quaternion.identity, size, size,
            //                                 RoadNetworkAddPointButtonHandleCap);
            //                             if (isClicked)
            //                             {
            //                                 // parent.Pointsからpointを検索してインデックスを取得
            //                                 var idx = parent.Points.ToList().IndexOf(point);
            //                                 if (idx == -1)
            //                                     continue;
            //                                 state.delayCommand += () =>
            //                                 {
            //                                     parent.LineString.Points.Insert(idx, new RnPoint(point.Vertex + Vector3.up));
            //                                     Debug.Log("ポイント追加ボタンが押された");
            //                                 };
            //                                 state.isDirtyTarget = true;
            //                                 continue;
            //                             }
            //                         }
            //                         // ポイントの削除
            //                         else
            //                         {
            //                             // ポイントの削除ボタンの表示
            //                             var isClicked = Handles.Button(point, Quaternion.identity, size, size,
            //                                 RoadNetworkRemovePointButtonHandleCap);
            //                             if (isClicked)
            //                             {
            //                                 state.delayCommand += () =>
            //                                 {
            //
            //                                     var isSuc = parent.LineString.Points.Remove(point);
            //                                     if (isSuc)
            //                                     {
            //                                         Debug.Log("ポイントが削除された");
            //                                     }
            //                                     else
            //                                     {
            //                                         Debug.Log("ポイントが削除されなかった");
            //                                     }
            //                                     
            //                                 };
            //                                 state.isDirtyTarget = true;
            //                                 continue;
            //                             }
            //                         }
            //                     }
            //                 }
            //             }
            //         }
            //     }
            // }
            //
            // // 遅延実行 コレクションの要素数などを変化させる
            // if (state.delayCommand != null)
            //     state.delayCommand.Invoke();
            //
            // // 選択した道路オブジェクトに変更があったとき
            // if (state.isDirtyTarget)
            // {
            //     editTarget.SetDirty(); // 通知
            // }
        }
        
        public void OnLineCreated(Spline createdSpline, IEditTargetLine targetLineBase)
        {
            var targetLine = targetLineBase as EditTargetRoadWay ;
            targetLine.Apply(targetLine.Road, createdSpline);

            var reproduceTarget = new RrTargetRoadBases(editTarget.RoadNetwork, new[] { targetLine.Road });
            new RoadReproducer().Generate(reproduceTarget, CrosswalkFrequency.All);
        }


        private static void DeployPointMoveHandle(RnPoint point, RoadEditSceneGUIState state, float size)
        {
            EditorGUI.BeginChangeCheck();
            var vertPos = DeployFreeMoveHandle(point, size, snap: Vector3.zero);
            if (EditorGUI.EndChangeCheck())
            {
                var mousePos = Event.current.mousePosition;
                var ray = HandleUtility.GUIPointToWorldRay(mousePos);
                const float maxRayDistance = 1000.0f;
                new RoadNetworkEditLandSnapper().SnapPointToObj(point, ray, maxRayDistance, "dem_", "tran_");
                state.isDirtyTarget = true;
            }
        }

        private static Vector3 DeployFreeMoveHandle(in Vector3 pos, float size, in Vector3 snap)
        {
            return Handles.FreeMoveHandle(pos, size, snap, Handles.SphereHandleCap);
        }

        private static void RoadNetworkRemovePointButtonHandleCap(int controlID, Vector3 position, Quaternion rotation,
            float size, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.MouseMove:
                case EventType.Layout:
                    Handles.CubeHandleCap(controlID, position, rotation, size, eventType);

                    break;
                case EventType.Repaint:
                    Handles.DrawWireDisc(position, Vector3.up,
                        size * RoadNetworkEditTargetSelectButton.LinkHndScaleFactor);
                    Handles.DrawWireCube(position,
                        new Vector3(size, size * RoadNetworkEditTargetSelectButton.PointHndScaleFactor, size * 0.15f));
                    break;
            }
        }

        private static void RoadNetworkAddPointButtonHandleCap(int controlID, Vector3 position, Quaternion rotation,
            float size, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.MouseMove:
                case EventType.Layout:
                    Handles.CubeHandleCap(controlID, position, rotation, size, eventType);

                    break;
                case EventType.Repaint:
                    Handles.DrawWireDisc(position, Vector3.up,
                        size * RoadNetworkEditTargetSelectButton.LinkHndScaleFactor);
                    Handles.DrawWireCube(position,
                        new Vector3(size, size * RoadNetworkEditTargetSelectButton.PointHndScaleFactor, size * 0.15f));
                    Handles.DrawWireCube(position,
                        new Vector3(size * 0.15f, size * RoadNetworkEditTargetSelectButton.PointHndScaleFactor, size));
                    break;
            }
        }
        
        
        /// <summary>
        /// 編集対象の線のうち、道路のWayです。
        /// </summary>
        private class EditTargetRoadWay : IEditTargetLine
        {
            public RnWay Way { get; }
            public RnRoad Road { get; }

            public EditTargetRoadWay(RnWay way, RnRoad road)
            {
                Way = way;
                Road = road;
            }
            

            public void Apply(RnRoadBase roadBase, Spline createdSpline)
            {
                // 適用後のLineStringを生成します。
                var positions = createdSpline
                    .Knots
                    .Select(k => k.Position)
                    .Select(f => new Vector3(f.x, f.y, f.z))
                    .Select(v => new RnPoint(v));
                var lineString = new RnLineString(positions);
                var reverse = lineString.Points;
                reverse.Reverse();
                var lineStringReverse = new RnLineString(reverse);
                
                // 同じWayをすべて見つけます。
                var correspondWays = Road.AllWays().Where(w => Way.IsSameLineSequence(w)).ToArray();
                // 車道と歩道の間の線は、逆順もチェックしないと発見できません
                var correspondWaysReverse = Road.AllWays().Where(w => Way.IsSameLineSequenceReverse(w)).ToArray();
                
                // 適用します。
                foreach (var w in correspondWays)
                {
                    w.LineString = lineString;
                }

                foreach (var w in correspondWaysReverse)
                {
                    w.LineString = lineStringReverse;
                }
            }

            public Vector3[] Line
            {
                get => Way.Points.Select(p => p.Vertex).ToArray();
            }
        }

        
    }
}