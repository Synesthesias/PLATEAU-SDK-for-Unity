using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
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

    //public class ObservableData<_Type> : IObservable<_Type>
    //{

    //    public ObservableData(in _Type data)
    //    {
    //        this.data = data;
    //    }

    //    private _Type data;
    //    private Action<IObserver<_Type>> subScribe;

    //    public IDisposable Subscribe(IObserver<_Type> observer)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    /// <summary>
    /// 道路を編集する際に利用するデータモデルのインターフェイス
    /// クラス内でのLink、LaneはNode間を繋ぐLinkリスト、Laneリストを指す
    /// </summary>
    public interface IScriptableRoadMdl
    {
        //public bool SetEditingTarget(INodeConnectionLink target);

        //public INodeConnectionLink EditingTarget{ get; set; }
        public void Apply();

        // 処理の成否を返す
        // 値の設定後に確認するようにする
        public bool IsSuccess { get; }
        public bool IsEditingDetailMode { get; set; }
        public int NumLeftLane { get; set; }
        public int NumRightLane { get; set; }
        public float MedianWidth { get; set; }
        public float LeftSideWalkWidth { get; set; }
        public float RightSideWalkWidth { get; set; }
        public float RoadWidth { get; set; }

        //public bool ChangeLeftLaneCount(int count);
        //public bool ChangeRightLaneCount(int count);
        //public bool SetEnableSideWalk(bool isEnable);

        //public bool ChangeRoadWidth(float width);
        //public bool ChangeLeftSideWalkWidth(float width);
        //public bool ChangeRightSideWalkWidth(float width);

        //public bool ChangeLaneWidth(float width);
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

        public void Reset(IScriptableRoadMdl mdl)
        {
            isEditingDetailMode = mdl.IsEditingDetailMode;
            numLeftLane = mdl.NumLeftLane;
            numRightLane = mdl.NumRightLane;
            medianWidth = mdl.MedianWidth;
            leftSideWalkWidth = mdl.LeftSideWalkWidth;
            rightSideWalkWidth = mdl.RightSideWalkWidth;
            roadWidth = mdl.RoadWidth;

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
        public float leftSideWalkWidth = 0.0f;
        public float rightSideWalkWidth = 0.0f;
        public float roadWidth = 0.0f;

        ScriptableRoadMdlData cache;


        public void ResetCache()
        {
            cache.Reset(this);
        }

        // cacheとの比較を行い、変更があれば変更を通知する
        public void Apply()
        {
            throw new NotImplementedException();
            //if (cache.numLeftLane != NumLeftLane)
            //{
            //    Notify(NumLeftLane, cache.numLeftLane, nameof(NumLeftLane));
            //    cache.numLeftLane = NumLeftLane;
            //}
            //if (cache.numRightLane != NumRightLane)
            //{
            //    Notify(NumRightLane, cache.numRightLane, nameof(NumRightLane));
            //    cache.numRightLane = NumRightLane;
            //}
            //if (cache.medianWidth != MedianWidth)
            //{
            //    Notify(MedianWidth, cache.medianWidth, nameof(MedianWidth));
            //    cache.medianWidth = MedianWidth;
            //}
            //if (cache.leftSideWalkWidth != LeftSideWalkWidth)
            //{
            //    Notify(LeftSideWalkWidth, cache.leftSideWalkWidth, nameof(LeftSideWalkWidth));
            //    cache.leftSideWalkWidth = LeftSideWalkWidth;
            //}
            //if (cache.rightSideWalkWidth != RightSideWalkWidth)
            //{
            //    Notify(RightSideWalkWidth, cache.rightSideWalkWidth, nameof(RightSideWalkWidth));
            //    cache.rightSideWalkWidth = RightSideWalkWidth;
            //}
            //if (cache.roadWidth != RoadWidth)
            //{
            //    Notify(RoadWidth, cache.roadWidth, nameof(RoadWidth));
            //    cache.roadWidth = RoadWidth;
            //}
        }
        public bool IsSuccess { get; set; }

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

        public float LeftSideWalkWidth
        {
            get => leftSideWalkWidth;
            set => SetPropety(value, ref leftSideWalkWidth, nameof(leftSideWalkWidth));
        }
        public float RightSideWalkWidth
        {
            get => rightSideWalkWidth;
            set => SetPropety(value, ref rightSideWalkWidth, nameof(rightSideWalkWidth));
        }

        public float RoadWidth
        {
            get => roadWidth;
            set => SetPropety(value, ref roadWidth, nameof(roadWidth));
        }
        public float MedianWidth 
        { 
            get => medianWidth; 
            set => SetPropety(value, ref medianWidth, nameof(medianWidth)); 
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
            //serializedObject = mdl;
            this.mod = mod;

            //            public float _leftSideWalkWidth = 0.0f;
            //public float _rightSideWalkWidth = 0.0f;
            //public float _laneWidth = 0.0f;
            //public bool _isApply = false;
            road = FindProperty("road");
            isEditingDetailMode = FindProperty("isEditingDetailMode");
            numLeftLane = FindProperty("numLeftLane");
            numRightLane = FindProperty("numRightLane");
            medianWidth = FindProperty("medianWidth");
            leftSideWalkWidth = FindProperty("leftSideWalkWidth");
            rightSideWalkWidth = FindProperty("rightSideWalkWidth");
            roadWidth = FindProperty("roadWidth");

            isApply = FindProperty("_isApply");
            //_numLeftLane = serializedObject.FindProperty("_numLeftLane");
            //_numRightLane = serializedObject.FindProperty("_numRightLane");
            //_enableSideWalk = serializedObject.FindProperty("_enableSideWalk");
            //_leftSideWalkWidth = serializedObject.FindProperty("_leftSideWalkWidth");
            //_rightSideWalkWidth = serializedObject.FindProperty("_rightSideWalkWidth");
            //_isApply = serializedObject.FindProperty("_isApply");

            ResetCache();
        }

        public RoadNetworkSimpleEditSysModule mod;

        public SerializedProperty road;

        public SerializedProperty isEditingDetailMode;
        public SerializedProperty numLeftLane;
        public SerializedProperty numRightLane;
        public SerializedProperty medianWidth;
        public SerializedProperty leftSideWalkWidth;
        public SerializedProperty rightSideWalkWidth;
        public SerializedProperty roadWidth;

        public SerializedProperty isApply;
        //public UIDocBindHelper.IAccessor<int> _numLeftLane;
        //public UIDocBindHelper.IAccessor<int> _numRightLane;
        //public UIDocBindHelper.IAccessor<bool> _enableSideWalk ;
        //public UIDocBindHelper.IAccessor<float> _leftSideWalkWidth;
        //public UIDocBindHelper.IAccessor<float> _rightSideWalkWidth;
        //public UIDocBindHelper.IAccessor<bool> _isApply;

        ScriptableRoadMdlData cache;
        public bool IsSuccess => throw new NotImplementedException();

        public int NumLeftLane { get => numLeftLane.intValue; set => numLeftLane.intValue = value; }
        public int NumRightLane { get => numRightLane.intValue; set => numRightLane.intValue = value; }
        public float MedianWidth { get => medianWidth.floatValue; set => medianWidth.floatValue = value; }
        public float LeftSideWalkWidth { get => leftSideWalkWidth.floatValue; set => leftSideWalkWidth.floatValue = value; }
        public float RightSideWalkWidth { get => rightSideWalkWidth.floatValue; set => rightSideWalkWidth.floatValue = value; }
        public float RoadWidth { get => roadWidth.floatValue; set => roadWidth.floatValue = value; }
        public bool IsEditingDetailMode { get => isEditingDetailMode.boolValue; set => isEditingDetailMode.boolValue = value; }

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
            var road = roadObj.road;

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
                road.SetLeftLaneCount(NumLeftLane);

            }
            if (cache.numRightLane != NumRightLane)
            {
                Notify(NumRightLane, cache.numRightLane, nameof(NumRightLane));
                cache.numRightLane = NumRightLane;
                isChanged = true;
                road.SetRightLaneCount(NumRightLane);

            }
            if (cache.medianWidth != MedianWidth) 
            {
                Notify(MedianWidth, cache.medianWidth, nameof(MedianWidth));
                cache.medianWidth = MedianWidth;
                isChanged = true;

                if (MedianWidth == 0.0f)
                {
                    road.RemoveMedian();
                }
                else
                {
                    road.SetMedianWidth(MedianWidth, LaneWayMoveOption.MoveBothWay);
                }
            }

            if (cache.roadWidth != RoadWidth)
            {
                Notify(RoadWidth, cache.roadWidth, nameof(RoadWidth));
                cache.roadWidth = RoadWidth;
            }
            if (cache.leftSideWalkWidth != LeftSideWalkWidth)
            {
                Notify(LeftSideWalkWidth, cache.leftSideWalkWidth, nameof(LeftSideWalkWidth));
                cache.leftSideWalkWidth = LeftSideWalkWidth;
            }
            if (cache.rightSideWalkWidth != RightSideWalkWidth)
            {
                Notify(RightSideWalkWidth, cache.rightSideWalkWidth, nameof(RightSideWalkWidth));
                cache.rightSideWalkWidth = RightSideWalkWidth;
            }


            if (isChanged)
            {

            }
        }

        private static void Notify<_T>(in _T post, in _T pre, in string name)
            where _T : IEquatable<_T>
        {
            var s = string.Format("Changed property : {0}, {1} to {2}.", name, pre, post);
            Debug.Log(s);
        }

    }
}
