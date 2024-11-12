using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using static PLATEAU.Editor.RoadNetwork.RoadNetworkEditingSystem;

namespace PLATEAU.Editor.RoadNetwork.UIDocBind
{
    /// <summary>
    /// Node間を繋ぐLaneの繋がり
    /// １つ以上のLaneで構成する
    /// </summary>
    public interface INodeConnectionLane
    {
        public List<RnLane> lanes { get; }
    }

    /// <summary>
    /// 道路を編集する際に利用するデータモデルのインターフェイス
    /// クラス内でのLink、LaneはNode間を繋ぐLinkリスト、Laneリストを指す
    /// </summary>
    public interface IScriptableRoadMdl
    {
        public void Apply();
        public bool IsEditingDetailMode { get; set; }
        public int NumLeftLane { get; set; }
        public int NumRightLane { get; set; }
        public float MedianWidth { get; set; }

        public bool EnableLeftSideWalk { get; set; }
        public bool EnableRightSideWalk { get; set; }

    }

    public struct ScriptableRoadMdlData
    {
        public bool isEditingDetailMode;
        public int numLeftLane;
        public int numRightLane;
        public float medianWidth;
        public float leftSideWalkWidth;
        public float rightSideWalkWidth;
        public float roadWidth;

        public bool enableLeftSideWalk;
        public bool enableRightSideWalk;

        public void Reset(IScriptableRoadMdl mdl)
        {
            isEditingDetailMode = mdl.IsEditingDetailMode;
            numLeftLane = mdl.NumLeftLane;
            numRightLane = mdl.NumRightLane;
            medianWidth = mdl.MedianWidth;

            enableLeftSideWalk = mdl.EnableLeftSideWalk;
            enableRightSideWalk = mdl.EnableRightSideWalk;

        }

    }

    public class ScriptableRoadMdl : ScriptableObject, IScriptableRoadMdl
    {
        public ScriptableRoadMdl()
        {
        }

        public void Construct(RnRoadGroup road)
        {
            //Assert.IsNotNull(road);
            this.road = road;

            numLeftLane = this.road.GetLeftLaneCount();
            numRightLane = this.road.GetRightLaneCount();
            

            ResetCache();
        }

        public RnRoadGroup road;

        public bool isEditingDetailMode;
        public int numLeftLane = 3;
        public int numRightLane = 3;
        public float medianWidth = 0.0f;    // 0.0fは中央分離帯がないことを示す

        public bool enableLeftSideWalk = true;
        public bool enableRightSideWalk = true;

        ScriptableRoadMdlData cache;


        public void ResetCache()
        {
            cache.Reset(this);
        }

        // cacheとの比較を行い、変更があれば変更を通知する
        public void Apply()
        {
            throw new NotImplementedException();
        }

        public bool IsEditingDetailMode 
        {
            get => isEditingDetailMode;
            set => SetPropety(value, ref isEditingDetailMode, nameof(isEditingDetailMode)); 
        }
        public int NumLeftLane
        {
            get => numLeftLane;
            set => SetPropety(value, ref numLeftLane, nameof(NumLeftLane));
        }
        public int NumRightLane
        {
            get => numRightLane;
            set => SetPropety(value, ref numRightLane, nameof(NumRightLane));
        }

        public float MedianWidth 
        { 
            get => medianWidth; 
            set => SetPropety(value, ref medianWidth, nameof(medianWidth)); 
        }
        public bool EnableLeftSideWalk 
        { 
            get => enableLeftSideWalk; 
            set => SetPropety(value, ref enableLeftSideWalk, nameof(enableLeftSideWalk)); 
        }
        public bool EnableRightSideWalk 
        {
            get => enableRightSideWalk;
            set => SetPropety(value, ref enableRightSideWalk, nameof(enableRightSideWalk));
        }

        private static void SetPropety<_T>(in _T v, ref _T current, in string name)
            where _T : IEquatable<_T>
        {
            if (v.Equals(current))
            {
                return;
            }
            var pre = current;
            current = v;
            Notify(v, pre, name);
        }

        private static void Notify<_T>(in _T post, in _T pre, in string name)
    where _T : IEquatable<_T>
        {
            var s = string.Format("Changed property : {0}, {1} to {2}.", name, pre, post);
            Debug.Log(s);
        }

    }

    public class SerializedScriptableRoadMdl : SerializedObject, IScriptableRoadMdl
    {
        public SerializedScriptableRoadMdl(ScriptableRoadMdl mdl, RoadNetworkSimpleEditSysModule mod)
            : base(mdl)
        {
            Assert.IsNotNull(mdl);
            Assert.IsNotNull(mod);
            this.mod = mod;

            road = FindProperty("road");
            isEditingDetailMode = FindProperty("isEditingDetailMode");
            numLeftLane = FindProperty("numLeftLane");
            numRightLane = FindProperty("numRightLane");
            medianWidth = FindProperty("medianWidth");

            isApply = FindProperty("_isApply");

            enableLeftSideWalk = FindProperty("enableLeftSideWalk");
            enableRightSideWalk = FindProperty("enableRightSideWalk");

            ResetCache();
        }

        public RoadNetworkSimpleEditSysModule mod;

        public SerializedProperty road;

        public SerializedProperty isEditingDetailMode;
        public SerializedProperty numLeftLane;
        public SerializedProperty numRightLane;
        public SerializedProperty medianWidth;

        public SerializedProperty enableLeftSideWalk;
        public SerializedProperty enableRightSideWalk;

        public SerializedProperty isApply;

        public RnSideWalk lPre;
        public RnSideWalk rPre;

        ScriptableRoadMdlData cache;
        public int NumLeftLane { get => numLeftLane.intValue; set => numLeftLane.intValue = value; }
        public int NumRightLane { get => numRightLane.intValue; set => numRightLane.intValue = value; }
        public float MedianWidth { get => medianWidth.floatValue; set => medianWidth.floatValue = value; }
        public bool IsEditingDetailMode { get => isEditingDetailMode.boolValue; set => isEditingDetailMode.boolValue = value; }
        public bool EnableLeftSideWalk { get => enableLeftSideWalk.boolValue; set => enableLeftSideWalk.boolValue = value; }
        public bool EnableRightSideWalk { get => enableRightSideWalk.boolValue; set => enableRightSideWalk.boolValue = value; }

        public bool ChangeLaneWidth(float width)
        {
            throw new NotImplementedException();
        }

        public void ResetCache()
        {
            cache.Reset(this);
        }


        public void Apply()
        {
            if (this.road == null)
            {
                Debug.Log("編集対象のLinkGroupが設定されていない");
                return;
            }

            bool isChanged = false;
            var roadObj = targetObject as ScriptableRoadMdl;
            var roadGroup = roadObj.road;

            if (cache.isEditingDetailMode != IsEditingDetailMode)
            {
                //if (mod.CanSetDtailMode())
                //{
                //    Notify(IsEditingDetailMode, cache.isEditingDetailMode, nameof(IsEditingDetailMode));
                //    cache.isEditingDetailMode = IsEditingDetailMode;
                //    mod.SetDetailMode(IsEditingDetailMode);
                //    isChanged = true;
                //}
            }

            if (cache.numLeftLane != NumLeftLane)
            {
                Notify(NumLeftLane, cache.numLeftLane, nameof(NumLeftLane));
                cache.numLeftLane = NumLeftLane;
                isChanged = true;
                roadGroup.SetLeftLaneCount(NumLeftLane);

            }
            if (cache.numRightLane != NumRightLane)
            {
                Notify(NumRightLane, cache.numRightLane, nameof(NumRightLane));
                cache.numRightLane = NumRightLane;
                isChanged = true;
                roadGroup.SetRightLaneCount(NumRightLane);

            }
            if (cache.medianWidth != MedianWidth) 
            {
                Notify(MedianWidth, cache.medianWidth, nameof(MedianWidth));
                cache.medianWidth = MedianWidth;
                isChanged = true;

                if (MedianWidth == 0.0f)
                {
                    roadGroup.RemoveMedian();
                }
                else
                {
                    roadGroup.SetMedianWidth(MedianWidth, LaneWayMoveOption.MoveBothWay);
                }
            }

            if (cache.enableLeftSideWalk != EnableLeftSideWalk)
            {
                Notify(EnableLeftSideWalk, cache.enableLeftSideWalk, nameof(EnableLeftSideWalk));
                cache.enableLeftSideWalk = EnableLeftSideWalk;
                foreach (var road in roadGroup.Roads)
                {
                    //road.SideWalks
                    ////road.SideWalks.Count() == ; // レーンの向きを見て　左右の判断
                    //foreach (var lane in road.GetLeftLanes())
                    //{
                    //    lane
                    //}
                    //road.SideWalks[0].SetWidth(LeftSideWalkWidth);
                    //road.RemoveSideWalk(SideWalkType.Left);
                }
                isChanged = true;
            }

            if (cache.enableRightSideWalk != EnableRightSideWalk)
            {
                Notify(EnableRightSideWalk, cache.enableRightSideWalk, nameof(EnableRightSideWalk));
                cache.enableRightSideWalk = EnableRightSideWalk;
                isChanged = true;
            }


            if (isChanged)
            {

            }
        }

        private static void Notify<_T>(in _T post, in _T pre, in string name)
            where _T : IEquatable<_T>
        {
            var s = string.Format("Changed property : {0}, {1} to {2}.", name, pre, post);
            //Debug.Log(s); // デバッグ用
        }

    }
}
