using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace PLATEAU.Editor.RoadNetwork.EditingSystem
{
    internal class EditingIntersection
    {
        
        public EditorData<RnIntersection> Intersection { get; private set; }
        private bool activate = false;

        private RnNeighbor selectEntablePoint = null;
        private RnNeighbor selectExitablePoint = null;
        
        
        public bool SetTarget(EditorData<RnIntersection> intersection)
        {
            if (this.Intersection == intersection)
                return false;

            if (intersection == null)
            {
                Activate(false);
                return true;
            }

            this.Intersection = intersection;
            return true;
        }

        public void Activate(bool activate)
        {
            this.activate = activate;
        }

        public IReadOnlyCollection<RnNeighbor> EnterablePoints
        {
            get
            {
                var d = Intersection.ReqSubData<EnterablePointEditorData>();
                return d.Points;
            }
        }

        public IReadOnlyCollection<RnNeighbor> ExitablePoints
        {
            get
            {
                var d = Intersection.ReqSubData<ExitablePointEditorData>();
                return d.Points;
            }
        }

        /// <summary>
        /// 流入点と流出点を返す
        /// </summary>
        public (RnNeighbor, RnNeighbor) SelectedPoints
        {
            get => (selectEntablePoint, selectExitablePoint);
        }

        public bool IsSelectdEntablePoint
        {
            get => selectEntablePoint != null;
        }

        public bool CanTryUpdateTrack
        {
            get => selectEntablePoint != null && selectExitablePoint != null;
        }

        public void SetEntablePoint(RnNeighbor neighbor)
        {
            Assert.IsNotNull(neighbor);

            // 選択中の交差点に含まれているか
            Assert.IsTrue(EnterablePoints.Contains(neighbor));

            selectEntablePoint = neighbor;
        }

        public void SetExitablePoint(RnNeighbor neighbor)
        {
            Assert.IsNotNull(neighbor);

            // 選択中の交差点に含まれているか
            Assert.IsTrue(ExitablePoints.Contains(neighbor));

            selectExitablePoint = neighbor;
        }

        /// <summary>
        /// 選択状態やトラックの有無で処理を分岐する
        /// </summary>
        public void UpdateTrack()
        {
            Assert.IsTrue(CanTryUpdateTrack);

            var track = Intersection.Ref.FindTrack(selectEntablePoint, selectExitablePoint);
            if (track != null)
                Intersection.Ref.RemoveTrack(track);
            else
                Intersection.Ref.TryAddOrUpdateTrack(selectEntablePoint, selectExitablePoint);

            selectEntablePoint = null;
            selectExitablePoint = null;
        }

        public void RemoveTarck()
        {
            selectEntablePoint = null;
            selectExitablePoint = null;
        }

        //public void CreateSubData()
        //{
        //    intersection.ClearSubData();

        //    var enterablePointEditorData = EnterablePointEditorData.Create(intersection); 
        //    intersection.TryAdd(enterablePointEditorData);
        //}


        

        // private bool isShapeEditingMode = false;
    }
}