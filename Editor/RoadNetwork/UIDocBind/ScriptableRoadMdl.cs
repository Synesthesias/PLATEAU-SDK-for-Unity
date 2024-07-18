using PLATEAU.RoadNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace PLATEAU.Editor.RoadNetwork.UIDocBind
{
    /// <summary>
    /// Node間をつなぐLinkの繋がり
    /// 1つ以上のLinkで構成する
    /// </summary>
    public interface INodeConnectionLink
    {
        /// <summary>
        /// このNodeConnectionが持つLinkのリスト
        /// </summary>
        public List<RnLink> Links { get; }

        /// <summary>
        /// このNodeConnectionが持つINodeConnectionLaneのリスト
        /// 車線に対して処理を行う際にこちらを参照する
        /// </summary>
        public List<INodeConnectionLane> Lanes { get; }
    }

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
        public int NumLeftLane { get; set; }
        public int NumRightLane { get; set; }
        public bool EnableSideWalk { get; set; }

        public ObservableCollection<float> RoadWidth { get; }
        public float LeftSideWalkWidth { get; set; }
        public float RightSideWalkWidth { get; set; }
        public float LaneWidth { get; set; }

        //public bool ChangeLeftLaneCount(int count);
        //public bool ChangeRightLaneCount(int count);
        //public bool SetEnableSideWalk(bool isEnable);

        //public bool ChangeRoadWidth(float width);
        //public bool ChangeLeftSideWalkWidth(float width);
        //public bool ChangeRightSideWalkWidth(float width);

        //public bool ChangeLaneWidth(float width);
    }

    public class ScriptableRoadMdl : ScriptableObject, IScriptableRoadMdl
    {
        public ScriptableRoadMdl(INodeConnectionLink road)
        {
            //Assert.IsNotNull(road);
            //this.road = road;

            _roadWidth.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Replace)
                {
                    var idx = e.NewStartingIndex;
                    Assert.IsTrue(e.NewItems.Count == 1);
                    var propName = string.Format("{0}[{1}]", nameof(RoadWidth), idx);
                    var newV = (float)e.NewItems[0];
                    var oldV = (float)e.OldItems[0];
                    Notify(newV, oldV, propName);
                }
            };

            ResetCache();
        }

        private INodeConnectionLink road;

        // テスト用にフィールドを作成　不要になったものは削除する
        public int _numLeftLane = 3;
        public int _numRightLane = 3;
        public bool _enableSideWalk = false;
        public ObservableCollection<float> _roadWidth = new ObservableCollection<float>();
        public float _leftSideWalkWidth = 0.0f;
        public float _rightSideWalkWidth = 0.0f;
        public float _laneWidth = 0.0f;

        private struct Cache
        {
            public int numLeftLane;
            public int numRightLane;
            public bool enableSideWalk;
            public ObservableCollection<float> roadWidth;
            public float leftSideWalkWidth;
            public float rightSideWalkWidth;
            public float laneWidth;
        }
        Cache cache;


        public void ResetCache()
        {
            cache.numLeftLane = NumLeftLane;
            cache.numRightLane = NumRightLane;
            cache.enableSideWalk = EnableSideWalk;
            cache.roadWidth = RoadWidth;
            cache.leftSideWalkWidth = LeftSideWalkWidth;
            cache.rightSideWalkWidth = RightSideWalkWidth;
            cache.laneWidth = LaneWidth;
        }

        // cacheとの比較を行い、変更があれば変更を通知する
        public void Apply()
        {
            if (cache.numLeftLane != NumLeftLane)
            {
                Notify(NumLeftLane, cache.numLeftLane, nameof(NumLeftLane));
                cache.numLeftLane = NumLeftLane;
            }
            if (cache.numRightLane != NumRightLane)
            {
                Notify(NumRightLane, cache.numRightLane, nameof(NumRightLane));
                cache.numRightLane = NumRightLane;
            }
            if (cache.enableSideWalk != EnableSideWalk)
            {
                Notify(EnableSideWalk, cache.enableSideWalk, nameof(EnableSideWalk));
                cache.enableSideWalk = EnableSideWalk;
            }
            if (cache.roadWidth != RoadWidth)
            {
                //Notify(RoadWidth, cache.roadWidth, nameof(RoadWidth));
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
            if (cache.laneWidth != LaneWidth)
            {
                Notify(LaneWidth, cache.laneWidth, nameof(LaneWidth));
                cache.laneWidth = LaneWidth;
            }
        }
        public bool IsSuccess { get; set; }

        public int NumLeftLane
        {
            get => _numLeftLane;
            set => SetPropety(value, ref _numLeftLane, nameof(NumLeftLane));
        }
        public int NumRightLane
        {
            get => _numRightLane;
            set => SetPropety(value, ref _numRightLane, nameof(NumRightLane));
        }
        public bool EnableSideWalk
        {
            get => _enableSideWalk;
            set => SetPropety(value, ref _enableSideWalk, nameof(EnableSideWalk));
        }

        public ObservableCollection<float> RoadWidth => _roadWidth;

        public float LeftSideWalkWidth
        {
            get => _leftSideWalkWidth;
            set => SetPropety(value, ref _leftSideWalkWidth, nameof(_leftSideWalkWidth));
        }
        public float RightSideWalkWidth
        {
            get => _rightSideWalkWidth;
            set => SetPropety(value, ref _rightSideWalkWidth, nameof(_rightSideWalkWidth));
        }

        public float LaneWidth
        {
            get => _laneWidth;
            set => SetPropety(value, ref _laneWidth, nameof(_laneWidth));
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
        public SerializedScriptableRoadMdl(ScriptableRoadMdl mdl)
            : base(mdl)
        {
            Assert.IsNotNull(mdl);
            //serializedObject = mdl;

            //            public float _leftSideWalkWidth = 0.0f;
            //public float _rightSideWalkWidth = 0.0f;
            //public float _laneWidth = 0.0f;
            //public bool _isApply = false;
            _numLeftLane = FindProperty("_numLeftLane");
            _numRightLane = FindProperty("_numRightLane");
            _enableSideWalk = FindProperty("_enableSideWalk");
            _leftSideWalkWidth = FindProperty("_leftSideWalkWidth");
            _rightSideWalkWidth = FindProperty("_rightSideWalkWidth");
            _isApply = FindProperty("_isApply");
            //_numLeftLane = serializedObject.FindProperty("_numLeftLane");
            //_numRightLane = serializedObject.FindProperty("_numRightLane");
            //_enableSideWalk = serializedObject.FindProperty("_enableSideWalk");
            //_leftSideWalkWidth = serializedObject.FindProperty("_leftSideWalkWidth");
            //_rightSideWalkWidth = serializedObject.FindProperty("_rightSideWalkWidth");
            //_isApply = serializedObject.FindProperty("_isApply");
        }

        //SerializedObject serializedObject;
        public SerializedProperty _numLeftLane;
        public SerializedProperty _numRightLane;
        public SerializedProperty _enableSideWalk;
        public SerializedProperty _leftSideWalkWidth;
        public SerializedProperty _rightSideWalkWidth;
        public SerializedProperty _isApply;
        //public UIDocBindHelper.IAccessor<int> _numLeftLane;
        //public UIDocBindHelper.IAccessor<int> _numRightLane;
        //public UIDocBindHelper.IAccessor<bool> _enableSideWalk ;
        //public UIDocBindHelper.IAccessor<float> _leftSideWalkWidth;
        //public UIDocBindHelper.IAccessor<float> _rightSideWalkWidth;
        //public UIDocBindHelper.IAccessor<bool> _isApply;

        private struct Cache
        {
            public int numLeftLane;
            public int numRightLane;
            public bool enableSideWalk;
            public ObservableCollection<float> roadWidth;
            public float leftSideWalkWidth;
            public float rightSideWalkWidth;
            public float laneWidth;
        }
        Cache cache;

        public INodeConnectionLink EditingTarget
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public bool IsSuccess => throw new NotImplementedException();

        public int NumLeftLane { get => _numLeftLane.intValue; set => _numLeftLane.intValue = value; }
        public int NumRightLane { get => _numRightLane.intValue; set => _numRightLane.intValue = value; }
        public bool EnableSideWalk { get => _enableSideWalk.boolValue; set => _enableSideWalk.boolValue = value; }

        public ObservableCollection<float> RoadWidth => throw new NotImplementedException();

        public float LeftSideWalkWidth { get => _leftSideWalkWidth.floatValue; set => _leftSideWalkWidth.floatValue = value; }
        public float RightSideWalkWidth { get => _rightSideWalkWidth.floatValue; set => _rightSideWalkWidth.floatValue = value; }
        public float LaneWidth { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool ChangeLaneWidth(float width)
        {
            throw new NotImplementedException();
        }

        public void Apply()
        {
            if (cache.numLeftLane != NumLeftLane)
            {
                Notify(NumLeftLane, cache.numLeftLane, nameof(NumLeftLane));
                cache.numLeftLane = NumLeftLane;
            }
            if (cache.numRightLane != NumRightLane)
            {
                Notify(NumRightLane, cache.numRightLane, nameof(NumRightLane));
                cache.numRightLane = NumRightLane;
            }
            if (cache.enableSideWalk != EnableSideWalk)
            {
                Notify(EnableSideWalk, cache.enableSideWalk, nameof(EnableSideWalk));
                cache.enableSideWalk = EnableSideWalk;
            }
            //if (cache.roadWidth != RoadWidth)
            //{
            //    //Notify(RoadWidth, cache.roadWidth, nameof(RoadWidth));
            //    cache.roadWidth = RoadWidth;
            //}
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
            //if (cache.laneWidth != LaneWidth)
            //{
            //    Notify(LaneWidth, cache.laneWidth, nameof(LaneWidth));
            //    cache.laneWidth = LaneWidth;
            //}
        }

        private static void Notify<_T>(in _T post, in _T pre, in string name)
            where _T : IEquatable<_T>
        {
            var s = string.Format("Changed property : {0}, {1} to {2}.", name, pre, post);
            Debug.Log(s);
        }

    }
}
